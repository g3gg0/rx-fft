

-- globals for GMSK demodulation
SymbolsPerSecond = 4800;   -- this is before biphase decoding
SymbolsToCheck = 3;        -- GMSK demod will try to synchronize using that many symbols. lower = faster, higher = more accurate
UseFastAtan2 = true;       -- will use a inaccurate but fast atan2() function

-- globals for SGP decoding
FrameLength = 2400;
BitsPerByte = 10;
HeaderSequence = "001000000100100000010011110011"; -- header bytes 2A 2A 2A 10, but crypted


-- internal state variables
BitsProcessed = 0;
BitStream = "";
ByteDecryptTable = {};


-- called when script is loaded
function Init()
	print("-------------------------");
	print("  RS-92 SGP Decoder v2.0 ");
	print("         by g3gg0.de     ");
	print("-------------------------");

	
	-- create decrypt table
	for srcByte = 0, 255 do
		local data = bit_reverse(srcByte, 8);
		data = bit_xor(data, 0xAA);
		data = bit_xor(data, bit_shr(data,1));
		
		ByteDecryptTable[data + 1] = srcByte;
	end

	
	-- set up default values
	GMSK = new("GMSKDemodulator");
	GMSK.SymbolsPerSecond = SymbolsPerSecond;	
	GMSK.SymbolsToCheck = SymbolsToCheck;	
	GMSK.UseFastAtan2 = UseFastAtan2;
	GMSK.SamplingRate = SamplingRate;
	
	Biphase = new("BiphaseDecoder");
	Biphase.BitSink = this();
	
	SetDemodulator(GMSK);
	SetBitSink(Biphase);
	
	-- now the setup is like that:
	--
	--   Source signal -> GMSK -> Biphase -> LUA (this script)
	--

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
			BitsProcessed = BitsProcessed - 1;
		end
	end

	-- when we read enough bits, dump the frame
	if(BitsProcessed >= FrameLength) then
		ProcessBitFrame(BitStream);
		Resynchronized();
	end	
end


function ProcessByteFrame(frame)
	local framePos = 0;
	local headerLength = HeaderSequence:len()/BitsPerByte;
	local subFrameType = 0;
	local subFrameLength = 0;
	local strBuf = "";

	--	frame must be 240 byte long
	if(not(#frame == 240)) then
		return;
	end
	
	print(" Frame data");
	print("------------------------------");
	strBuf = "";
	for pos = 1, headerLength do
		strBuf = strBuf..string.format("%02X ", frame[framePos+pos])
	end
	print("    Header:       "..strBuf);
	
	framePos = headerLength + 1;
	repeat
		subFrameType = frame[framePos];
		framePos = framePos + 1;
		
		if(not(subFrameType == 0xFF)) then
			subFrameLength = frame[framePos] * 2;
			framePos = framePos + 1;			
				
			if(framePos + subFrameLength + 3 > #frame) then
				print("Subframe too long!");
				return;
			end
			
			--print("    SubFrame:   "..string.format("%02X", subFrameType)..", "..subFrameLength.." Byte");
			DumpSubFrame(subFrameType, frame, framePos, subFrameLength);
			framePos = framePos + subFrameLength;
			print("    CRC:      "..string.format("%02X%02X", frame[framePos+1], frame[framePos]));
			framePos = framePos + 2;
		end
	until (subFrameType == 0xFF)
	
	strBuf = "";
	for pos = 1, #frame do
		strBuf = strBuf..string.format("%02X ", frame[pos])
	end	
	print("    Raw data:     "..strBuf);
	print("------------------------------");
	print("");
end

function DumpSubFrame(subFrameType, frameData, framePos, subFrameLength)
	local strBuf = "";

	-- status subframe
	if(subFrameType == 0x65) then
		
		print("  ==[Status]==");
		print("    Frame #:  "..string.format("%02X%02X", frameData[framePos+1], frameData[framePos+0]));
		
		strBuf = "";
		for pos = 0, 9 do
			strBuf = strBuf..string.format("%c", frameData[framePos+2+pos])
		end
		print("    ID:       "..strBuf:gsub(" ", "_"));
		print("    NUL:      "..string.format("%02X", frameData[framePos+12]));
		print("    State:    "..string.format("%02X%02X", frameData[framePos+14], frameData[framePos+13]));
		print("    CF #:     "..string.format("%02X", frameData[framePos+15]));
		
		strBuf = "";
		for pos = 0, 15 do
			strBuf = strBuf..string.format("%02X ", frameData[framePos+16+pos])
		end
		print("    CF:       "..strBuf);
		
		strBuf = "    Raw Data: ";
		for pos = 0, subFrameLength - 1 do
			strBuf = strBuf..string.format("%02X ", frameData[framePos+pos])
			if(pos%16 == 15) then
				print(strBuf);
				strBuf = "              ";
			end			
		end		
		if(strBuf:len() > 0) then
			print(strBuf);
		end
	end

	-- PTU subframe
	if(subFrameType == 0x69) then
		print("  ==[PTU]==");
		
		strBuf = "    Raw Data: ";
		for pos = 0, subFrameLength - 1 do
			strBuf = strBuf..string.format("%02X ", frameData[framePos+pos])
			if(pos%16 == 15) then
				print(strBuf);
				strBuf = "              ";
			end			
		end		
		if(strBuf:len() > 0) then
			print(strBuf);
		end
	end

	-- GPS subframe
	if(subFrameType == 0x67) then
		print("  ==[GPS]==");
		
		local milliseconds = frameData[framePos+3] * 0x1000000 + frameData[framePos+2] * 0x10000 + frameData[framePos+1] * 0x100 + frameData[framePos];
		local relDays = milliseconds / (1000*60*60*24);
		local relHours = (milliseconds / (1000*60*60)) % 24;
		local relMins = (milliseconds / (1000*60)) % 60;
		local relSecs = (milliseconds / 1000) % 60;
		local relMillisecs = (milliseconds % 1000);
		
		print("    Time:     +"..string.format("%02dd %02dh %02dm %02ds %03dms", relDays, relHours, relMins, relSecs, relMillisecs));

		strBuf = "    Channel:  |";
		for pos = 1, 12 do
			strBuf = strBuf..string.format(" %02d |", pos);
		end
		print(strBuf);
		
		strBuf = "    PRN:      |";
		for pos = 0, 3 do
			local word = frameData[framePos+7+2*pos] * 0x100 + frameData[framePos+6+2*pos];
			local prn1 = word % 31;
			local prn2 = (word/32) % 31;
			local prn3 = (word/1024) % 31;
			
			strBuf = strBuf..string.format(" %02d |", prn1);
			strBuf = strBuf..string.format(" %02d |", prn2);
			strBuf = strBuf..string.format(" %02d |", prn3);
		end
		print(strBuf);
		
		strBuf = "    Raw Data: ";
		for pos = 0, subFrameLength - 1 do
			strBuf = strBuf..string.format("%02X ", frameData[framePos+pos])
			if(pos%16 == 15) then
				print(strBuf);
				strBuf = "              ";
			end			
		end		
		if(strBuf:len() > 0) then
			print(strBuf);
		end
	end

	-- extra subframe
	if(subFrameType == 0x68) then
		print("  ==[Extra]==");
		strBuf = "    Raw Data: ";
		for pos = 0, subFrameLength - 1 do
			strBuf = strBuf..string.format("%02X ", frameData[framePos+pos])
			if(pos%16 == 15) then
				print(strBuf);
				strBuf = "              ";
			end			
		end		
		if(strBuf:len() > 0) then
			print(strBuf);
		end
	end
end


function ProcessBitFrame(frame)
	-- split the 2400 bit frame into 10 bit words
	local words = SplitFrame(frame, BitsPerByte);
	local frameBytesPos = 1;
	local frameBytes = {};
	local frameByteString = "";
	
	if(not(#frame == 2400)) then
		return;
	end

	for pos = 1, #words do
		-- convert the 10 bit word into a byte using the last 8 bit
		local word = words[pos];
		local cryptVal = BinToInt(words[pos]:sub(-8));
		local plainVal = ByteDecryptTable[cryptVal+1];
				
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


