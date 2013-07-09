

-- globals for GMSK demodulation
SymbolsPerSecond = 16000;   -- this is before biphase decoding
SymbolsToCheck = 3;        -- GMSK demod will try to synchronize using that many symbols. lower = faster, higher = more accurate
UseFastAtan2 = true;       -- will use a inaccurate but fast atan2() function

-- globals for SGP decoding
FrameLength = 1024*8;
BitsPerByte = 8;
HeaderSequence = "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"; -- header bytes 2A 2A 2A 10, but crypted




BitsProcessed = 0;
BitStream = "";


-- called when script is loaded
function Init()
	print("---------------------");
	print("    FSK Decoder      ");
	print("    by g3gg0.de      ");
	print("---------------------");
	
	-- set up default values
	GetDemod().SymbolsPerSecond = SymbolsPerSecond;	
	GetDemod().SymbolsToCheck = SymbolsToCheck;	
	GetDemod().UseFastAtan2 = UseFastAtan2;	
	
	-- reinitialize demodulator again
	GetDemod().Init(GetDemod());
end

-- called when carrier was detected. for now not really used
function TransmissionStart()
	print("Transmission has started")
end

-- called when the demodulator lost its signal. similar as above.
function TransmissionEnd()
	print("Transmission has ended")
end

-- called when preceding bitdecoder (like BMC decoder) thinks it is in sync again.
-- this tells us that it seems to have lost its synchronization and the data we received before might be crap.
function Resynchronized()
	BitStream = "";
	BitsProcessed = 0;
end

-- whenever there is a new data bit, its getting clocked using this function. the data bit is already decoded by the BMC decoder.
function ClockBit(state)
	-- clock a new bit. simply append to the BitStream buffer
	if(state) then
		BitStream = BitStream.."1";
	else
		BitStream = BitStream.."0";
	end
	
	BitsProcessed = BitsProcessed + 1;
	
	local headerLength = HeaderSequence:len();
	local streamLength = BitStream:len();
	
	-- check if the clocked bits are enough to build the header
	if(streamLength > headerLength) then
	
		-- check if this is the header sequence
		-- this is like a shift register of length 'headerLength' and we are always comparing to the header sequence
		if(not(BitStream:sub(1,headerLength) == HeaderSequence)) then
		
			-- seems not to be the header. remove the first bit. (=shift left)
			BitStream = BitStream:sub(2);
			--BitsProcessed = BitsProcessed - 1;
		end
	end

	-- when we read enough bits, dump the frame
	if(BitsProcessed >= FrameLength) then
	print("Frame: "..BitStream);
		ProcessBitFrame(BitStream);
		Resynchronized();
	end
	
end


function ProcessByteFrame(frame)
	local framePos = 0;
end


function ProcessBitFrame(frame)
	-- split the 2400 bit frame into 10 bit words
	local words = SplitFrame(frame, BitsPerByte);
	local frameBytesPos = 1;
	local frameBytes = {};
	local frameByteString = "";
	
	for pos = 1, #words do
		-- convert the 10 bit word into a byte using the last 8 bit
		local word = words[pos];
		local cryptVal = BinToInt(words[pos]:sub(-8));
		-- decrypt this byte
		local plainVal = cryptVal;
		
		frameBytes[frameBytesPos] = plainVal;
		frameBytesPos = frameBytesPos + 1;
		
		frameByteString = frameByteString .. string.format("%02X ", plainVal);
		--print ( "Word ".. pos .. ":  "..word.." =("..cryptVal..") =("..plainVal..")");
	end
	
	ProcessByteFrame(frameBytes);
	--print("Frame: "..frameByteString);
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




