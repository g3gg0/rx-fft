

-- globals for PSK demodulation
BaudRate = 2400;   -- this is before biphase decoding
MinDbDistance = 5;
UseFastAtan2 = true;       -- will use a inaccurate but fast atan2() function

-- globals for decoding
DumpType = 3; -- 1 = dump raw bit stream from PSK, 2 = remove header/trailer and dump BMC decoded data, 3 = decode message
ShortHeaderSequence = "0000000000000001";
FullHeaderSequence = "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001";

-- internal state variables
BitsProcessed = 0;
BitStream = "";
BMCActive = false;

CommandTypes = 
{
	[9] = "Lock",
	[10] = "Unlock",
	[11] = "Rear lid",
	[15] = "Lock finish?",
}


-- called when script is loaded
function Init()
	print("-------------------------");
	print("  Audi ZV  Decoder v1.0  ");
	print("         by g3gg0.de     ");
	print("-------------------------");
	
	-- set up default values
	PSK = new("PSKDemodulator");
	PSK.BaudRate = BaudRate;	
	PSK.MinDbDistance = MinDbDistance;	
	PSK.UseFastAtan2 = UseFastAtan2;
	PSK.SamplingRate = SamplingRate;
	SetDemodulator(PSK);
	
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
			-- now skip first and last bit and clock that into BMC
			BMCActive = true;
			DecodedBitStream = "";
			
			-- synchronize BMC
			BMC.Synchronize(BMC, BitStream:sub(1, 1) == '0');
			
			for pos = 2, BitsProcessed- 1 do
				BMC.ClockBit(BMC, BitStream:sub(pos, pos) == '1');
			end
			
			BMCActive = false;
			
			if(DumpType == 2) then
				print("BMC Decoded: "..DecodedBitStream);
			else
				ProcessBitFrame(DecodedBitStream);
			end
		end
	end
end

-- called when preceding bitdecoder (like BMC decoder) thinks it is in sync again.
-- this tells us that it seems to have lost its synchronization and the data we received before might be crap.
function Resynchronized()
	if(not BMCActive) then	
		BitStream = "";
		BitsProcessed = 0;
	else
		DecodedBitStream = "";
	end
end

-- whenever there is a new data bit, its getting clocked using this function. the data bit is already decoded by the BMC decoder.
function ClockBit(state)

	if(not BMCActive) then	
		-- clock a new bit. simply append to the BitStream buffer
		if(state) then
			BitStream = BitStream.."1";
		else
			BitStream = BitStream.."0";
		end
		
		BitsProcessed = BitsProcessed + 1;	
	else
		if(state) then
			DecodedBitStream = DecodedBitStream.."0";
		else
			DecodedBitStream = DecodedBitStream.."1";
		end
	end
end



function ProcessBitFrame(frame)

	if(frame:len() == 234) then
		if(frame:sub(1,FullHeaderSequence:len()) == FullHeaderSequence) then
			-- split the frame into 8 bit words
			local payload = frame:sub(FullHeaderSequence:len() + 1) ;
			local frameData = SplitFrame(payload, 8);
			local command = "unknown";
			
			if(CommandTypes[BinToInt(frameData[1])] ~= nil) then
				command = CommandTypes[BinToInt(frameData[1])];
			end
			
			print("FullFrame");
			print("    Command:   "..string.format("0x%02X ", BinToInt(frameData[1])).." ("..command..")");
			print("    Key:       "..string.format("0x%02X%02X%02X%02X%02X%02X%02X", BinToInt(frameData[2]), BinToInt(frameData[3]), BinToInt(frameData[4]), BinToInt(frameData[5]), BinToInt(frameData[6]), BinToInt(frameData[7]), BinToInt(frameData[8])));
			print("    Separator: "..string.format("0x%02X ", BinToInt(frameData[9])));
			print("    Car ID:    "..string.format("0x%02X ", BinToInt(frameData[10])));
			print("    Chk:       "..string.format("0x%02X%02X", BinToInt(frameData[11]), BinToInt(frameData[12])));
		else
			print("FullFrame, but invalid Header");
		end
	elseif(frame:len() == 24) then
		if(frame:sub(1,ShortHeaderSequence:len()) == ShortHeaderSequence) then
			-- split the frame into 8 bit words
			local payload = frame:sub(ShortHeaderSequence:len() + 1) ;
			local frameData = SplitFrame(payload, 8);
			local command = "unknown";
			
			if(CommandTypes[BinToInt(frameData[1])] ~= nil) then
				command = CommandTypes[BinToInt(frameData[1])];
			end
			print("ShortFrame:    "..string.format("0x%02X ", BinToInt(frameData[1])).." ("..command..")");
		else
			print("ShortFrame, but invalid Header");
		end	
	else
		print("Invalid frame length ("..frame:len()..")");
		print("    Frame: "..frame);
	end
	
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


