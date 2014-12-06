-- TETRA
-- 21759,9305 samples make up a frame which contains 4 bursts
-- thats 56,66 ms a

-- globals for GMSK demodulation
SymbolsPerSecond = 8000;   -- 
SymbolsToCheck = 16;        -- GMSK demod will try to synchronize using that many symbols. lower = faster, higher = more accurate
UseFastAtan2 = true;       -- will use a inaccurate but fast atan2() function

-- globals for frame decoding
FrameLength = 160;
BitsPerByte = 8;

HeaderSequence = "1010011"; -- header to look for. thats the difference encoded synch header 01100010


-- internal state variables
BitsProcessed = 0;
BitStream = "";
SyncedFrames = 0;
FailedFrames = 0;


-- called when script is loaded
function Init()
	print("-------------------------");
	print("  TETRAPOL Decoder v0.1  ");
	print("         by g3gg0.de     ");
	print("-------------------------");
	
	-- use a GMSK demodulator
	GMSK = new("GMSKDemodulator");
	GMSK.SymbolsPerSecond = SymbolsPerSecond;	
	GMSK.SymbolsToCheck = SymbolsToCheck;	
	GMSK.UseFastAtan2 = UseFastAtan2;
	GMSK.SamplingRate = SamplingRate;
	SetDemodulator(GMSK);

	-- set up a differential decoder which calls 'ClockDiffedBit' 
	Undiffer = new("DifferenceDecoder");
	Undiffer.BitSink = new_args("ScriptableSink", this().LuaVm);
	Undiffer.BitSink.FunctionPrefix = "Undiffer_";
	Undiffer.BitSink.Running = true;
end

-- called from differential decoder
function Undiffer_ClockBit(state)
	-- clock a new bit. simply append to the UndiffedBitStream buffer
	if(state) then
		UndiffedBitStream = UndiffedBitStream.."1";
	else
		UndiffedBitStream = UndiffedBitStream.."0";
	end	
end

-- those are empty
function Undiffer_Resynchronized()
end
function Undiffer_Desynchronized()
end
function Undiffer_TransmissionStart()
end
function Undiffer_TransmissionEnd()
end
function Undiffer_Init()
end

-- called whenever sampling rate has changed
function SamplingRateChanged()
end

-- called when carrier was detected. for now not really used
function TransmissionStart()
end

-- called when the demodulator lost its signal. similar as above.
function TransmissionEnd()
end

-- called when preceding bitdecoder thinks it is in sync again.
-- this tells us that it seems to have lost its synchronization and the data we received before might be crap.
function Resynchronized()
	BitStream = "";
	BitsProcessed = 0;
end



function ProcessUndiffedBitFrame(frame)
	-- split the bits frame into words
	local words = SplitFrame(frame, BitsPerByte);
	local frameBytesPos = 1;
	local frameBytes = {};
	local frameByteString = "";
	

	if(not(#words == 19)) then
		return;
	end

	for pos = 1, #words do
		-- convert the 10 bit word into a byte using the last 8 bit
		local word = words[pos];
		local plainVal = BinToInt(word);
				
		frameBytes[frameBytesPos] = plainVal;
		frameBytesPos = frameBytesPos + 1;
		
		frameByteString = frameByteString .. string.format("%02X ", plainVal);
		--print ( "Word ".. pos .. ":  "..word.." =("..plainVal..")");
	end
	
	ProcessByteFrame(frameBytes);
	print("Frame: "..frameByteString);
end

-- a new 160 bit frame was received. feed it into the differential decoder and process the result
function ProcessBitFrame(frame)
	
	-- first bit is always zero
	UndiffedBitStream = "0";
	Undiffer:Resynchronized()
	
	for pos = 1, frame:len() do
		
		if(frame:sub(1,1) == "0") then
			Undiffer:ClockBit(false);
		else
			Undiffer:ClockBit(true);
		end
		
		frame = frame:sub(2);
	end
	
	--print("Undiffed: "..UndiffedBitStream);
	
	-- will result in 161 bits since we define the first bit (0) but still
	-- let diff decoder process 160 bits. the last bit would be the first of
	-- the next frame and should be 0 again.

	-- now process the payload
	ProcessUndiffedBitFrame(UndiffedBitStream:sub(9,9+151));
	
	-- use last bit for parity check
	if(not (UndiffedBitStream:sub(161) == "0")) then
		print(" [E] frame faulty (had bit flip)");
	end
	
end


-- whenever there is a new data bit, its getting clocked using this function.
function ClockBit(state)
	failed = false;
	
	-- clock a new bit. simply append to the BitStream buffer
	if(state) then
		BitStream = BitStream.."1";
	else
		BitStream = BitStream.."0";
	end
	
	BitsProcessed = BitsProcessed + 1;
	
	local headerLength = HeaderSequence:len();
	local streamLength = BitStream:len();
	
	-- check if the clocked bits are enough to check the sync header
	if(streamLength > headerLength) then	
		-- check if this is the header sequence
		-- this is like a shift register of length 'headerLength' and we are always comparing to the header sequence
		if(not(BitStream:sub(1,headerLength) == HeaderSequence)) then
			
			-- the header does not match. increase faulty frame counter if this is a full frame
			failed = true;
			if(BitsProcessed >= FrameLength) then
				FailedFrames = FailedFrames + 1;
			end
			
			-- the first time or when there are too many failed frames, resynchronize
			if(SyncedFrames < 20 or FailedFrames > 10) then
				--print("Resync "..BitStream);
				
				SyncedFrames = 0;
			
				-- seems not to be the header. remove the first bit. (=shift left)
				BitStream = BitStream:sub(2);
				BitsProcessed = BitsProcessed - 1;
			end
		end
	end

	-- when we read enough bits, dump the frame
	if(BitsProcessed >= FrameLength) then
	
		-- process the raw difference encoded bits
		ProcessBitFrame(BitStream);
		Resynchronized();
		
		-- counter how many frames were decoded since last syn
		SyncedFrames = SyncedFrames + 1;
		
		-- if the frame had a correct header, reset faulty counter.
		if(failed == false) then
			FailedFrames = 0;
		else
			print(" [E] frame faulty (had synch error)");
		end
	end	
end




function ProcessByteFrame(frame)

end

function SplitFrame(frame, wordLength)
	local pos = 1;
	local words = {};
	
	while ( frame:len() > 0 ) do
		words[pos] = frame:sub(1,wordLength);
		pos = pos + 1;
		frame = frame:sub(wordLength + 1);
	end
	
	return words;
end

function BinToInt(binary)
	local value = 0;
	
	while ( binary:len() > 0 ) do
		value = value * 2;
		
		if ( binary:sub(1,1) == "1") then
			value = value + 1;
		end
		
		binary = binary:sub(2);
	end
	
	return value;
end


