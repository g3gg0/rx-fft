

function Init()
	print("Init")
	
	SetDemod(new("DemodulatorCollection.Demodulators.GMSKDemodulator"));
end


-- called when carrier was detected. for now not really useful
function TransmissionStart()
	print("Transmission has started")
end

-- called when the demodulator lost its signal. same as above.
function TransmissionEnd()
	print("Transmission has ended")
end

-- called when preceding bitdecoder (like BMC decoder) thinks it is in sync again.
-- this tells us that it seems to have lost its synchronization and the data we received before might be crap.
function Resynchronized()
	print("Bitstream resynchronized")

end

-- whenever there is a new data bit, its getting clocked using this function
function ClockBit(state)
	print("ClockBit")
end

