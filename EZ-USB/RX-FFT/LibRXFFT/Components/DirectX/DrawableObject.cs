using System.Drawing;

namespace LibRXFFT.Components.DirectX
{
    public class LabelledLine
    {
        public double Position;
        public string Label;
        public uint Color;

        public LabelledLine(string label, double position, Color color)
        {
            this.Position = position;
            this.Label = label;
            this.Color = (uint)color.ToArgb();
        }
    }

    public class FrequencyMarker
    {
        public string Label;
        public string Description;
        public long Frequency;

        public FrequencyMarker()
        {
            this.Label = "<Enter label here>";
            this.Description = "<Enter description here>";
            this.Frequency = 0;
        }

        public FrequencyMarker(long frequency)
        {
            this.Label = "<Enter label here>";
            this.Description = "<Enter description here>";
            this.Frequency = frequency;
        }

        public FrequencyMarker(string label, string description, long frequency)
        {
            this.Label = label;
            this.Description = description;
            this.Frequency = frequency;
        }
    }

    public class StringLabel
    {
        public string Label;
        public int X;
        public int Y;
        public uint Color;

        public StringLabel(string label, int x, int y, uint color)
        {
            this.Label = label;
            this.X = x;
            this.Y = y;
            this.Color = color;
        }
    }
}
