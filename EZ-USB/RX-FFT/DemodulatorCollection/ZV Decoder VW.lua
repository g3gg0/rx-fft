

-- globals for PSK demodulation
BaudRate = 2000;   -- this is before biphase decoding
MinDbDistance = 8;

-- globals for decoding
DumpType = 1; -- 1 = dump raw bit stream from PSK, 2 = dump decoded data, 3 = decode message

-- internal state variables
BitsProcessed = 0;
BitStream = "";

-- called when script is loaded
function Init()
	print("-------------------------");
	print("    VW ZV  Decoder v1.0  ");
	print("         by g3gg0.de     ");
	print("-------------------------");
	
	-- set up default values
	ASK = new("ASKDemodulator");
	ASK.BaudRate = BaudRate;
	ASK.MinDbDistance = MinDbDistance;	
	ASK.SamplingRate = SamplingRate;
	ASK.BitTimeLocked = false;
	ASK.SignalStrengthLocked = true;
	SetDemodulator(ASK);
	
	-- create a new biphase decoder and set ourself as its bitsink
	BMC = new("BiphaseDecoder");
	BMC.BitSink = this();
end

-- called whenever sampling rate has changed
function SamplingRateChanged()
end

-- called when carrier was detected.
function TransmissionStart()
	BitStream = "";
	BitsProcessed = 0;
end

-- called when the demodulator lost its signal. 
function TransmissionEnd()

	if(DumpType == 1) then
		print("Raw bit data: "..BitStream);
	else
		if(BitsProcessed>2) then

			DecodedBitStream = "";

			-- [....]
			
			if(DumpType == 2) then
				print("Decoded: "..DecodedBitStream);
			else
				ProcessBitFrame(DecodedBitStream);
			end
		end
	end
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

end



function ProcessBitFrame(frame)


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


