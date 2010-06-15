using System;
using LibRXFFT.Libraries.USB_RX.Devices;

namespace LibRXFFT.Libraries.USB_RX.Misc
{

    public class AtmelFilter : FilterInformation, IComparable
    {
        private Atmel Atmel;

        public int Id;

        public long _Width;
        public long _OutputFrequency;

        public AtmelFilter(Atmel atmel, int id, long width, long rate)
        {
            this.Atmel = atmel;
            this.Id = id;
            this._Width = width;
            this._OutputFrequency = rate;
        }

        public long Width
        {
            get
            {
                return _Width;
            }
        }

        public long Rate
        {
            get
            {
                return _OutputFrequency;
            }
        }

        public string Location
        {
            get
            {
                return "Atmel Filter #" + Id.ToString();
            }
        }

        public object SourceDevice
        {
            get { return Atmel; }
        }


        #region IComparable Member

        public int CompareTo(object obj)
        {
            if (obj.GetType() != typeof(AD6636FilterFile))
            {
                return -1;
            }
            AD6636FilterFile other = (AD6636FilterFile)obj;

            return (Width > other.Width) ? -1 : 1;
        }

        #endregion
    }
}
