using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace LibRXFFT.Libraries.USB_RX.Misc
{

    public class AtmelFilter : FilterInformation, IComparable
    {
        public int Id;

        public long _Width;
        public long _OutputFrequency;

        public AtmelFilter(int id, long width, long rate)
        {
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
