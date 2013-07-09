

-- Close
-- BMC Decoded: 101000 1011111011111111000001000111000111001110 1 1111010 01000010 1
-- BMC Decoded: 101000 1111101011000010101111111110101000010011 1 1111010 10111000 1
-- BMC Decoded: 101000 1010000001011000010011111110111111010000 1 1111010 11101000 1
-- BMC Decoded: 101000 1100000100011101000000010110100110110101 0 1111010 01001001 0
-- BMC Decoded: 101000 1000010100101100111101110110001011000001 0 1111010 11000111 0
-- BMC Decoded: 101000 0100000111010001001011110001000100100011 1 1111010 10111101 1
-- BMC Decoded: 101000 1110001001111110111001001101110110010110 1 1111010 00110110 1
-- BMC Decoded: 101000 0010100010110101101011001101111011000100 1 1111010 00000111 0

-- Open
-- BMC Decoded: 101000 0101001000100000111100001001001100110111 0 1111010 11111010 1
-- BMC Decoded: 101000 0110000111100100010011110011001001101100 0 1111010 10101001 1
-- BMC Decoded: 101000 0001011010110011001110111000110110001110 0 1111010 01010000 1
-- BMC Decoded: 101000 0100010010110010110000000000001000101111 1 1111010 11000010 0
-- BMC Decoded: 101000 1011110000001100010101000001101011001110 1 1111010 10101000 0

-- Light
-- BMC Decoded: 101000 1110001001101101101000110010110010000110 0 1111010 10010100 0
-- BMC Decoded: 101000 1110001001101101101000110010110010000110 0 1111010 00010100 1
-- BMC Decoded: 101000 0100101001000000110110000011101111110110 0 1111010 10011011 0
-- BMC Decoded: 101000 0100101001000000110110000011101111110110 0 1111010 00011011 1

-- BMC Decoded: 101000 1001000110001111000000000100001110001111 1 1111010 01111110 1
-- BMC Decoded: 101000 1001000110001111000000000100001110001111 1 1111010 11111110 0
-- BMC Decoded: 101000 1010000110011000110000100111101110111000 0 1111010 00011011 1
-- BMC Decoded: 101000 1010000110011000110000100111101110111000 0 1111010 10011011 0
             -- ======                                            =======              Match:  Bit 0 over 6 bits.
             -- |----- ---------------------------------------| ^                      Parity (odd ) match:  Bit 46 over 46 bits before.
             -- |----- ---------------------------------------- - ------- -------| ^   Parity (odd ) match:  Bit 62 over 62 bits before.

-- globals for PSK demodulation
BaudRate = 2000;   -- this is before biphase decoding
MinDbDistance = 10;

-- globals for decoding
DumpType = 0; -- 1 = dump raw bit stream from PSK, 2 = dump decoded data, 3 = decode message

-- internal state variables
BitsProcessed = 0;
BitStream = "";

-- called when script is loaded
function Init()
	print("-------------------------");
	print("   BMW ZV  Decoder v1.0  ");
	print("         by g3gg0.de     ");
	print("-------------------------");
	
	-- set up default values
	ASK = new("ASKDemodulator");
	ASK.BaudRate = BaudRate;
	ASK.MinDbDistance = MinDbDistance;	
	ASK.SamplingRate = SamplingRate;
	ASK.BitTimeLocked = true;
	ASK.SignalStrengthLocked = true;
    ASK.EnableAGC = false;
	SetDemodulator(ASK);
	
	-- create a new biphase decoder and set ourself as its bitsink
	BMC = new("BiphaseDecoder");
	BMC.BitSink = this();
    BMC.Verbose= false;
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

function Desynchronized()
	if(not BMCActive) then	
		BitStream = "";
		BitsProcessed = 0;
	else
        if(DumpType == 2) then
            print("BMC Decoded: "..DecodedBitStream);
        else
            ProcessBitFrame(DecodedBitStream);
        end    
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

    --if(frame:sub(1,6) == "101000") then
        print("    Frame: "..frame);
    --end
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


