
namespace DemodulatorCollection.Interfaces
{
    public interface BitClockSink
    {
        void ClockBit(bool state);
        void TransmissionStart();
        void TransmissionEnd();

        void Resynchronized();
    }
}