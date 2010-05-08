using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using System.Drawing;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public enum eDockState
    {
        Hidden,
        Expanded,
        Collapsed,
        Expanding,
        Collapsing
    }

    public enum eInputEventType
    {
        Nothing,
        MouseMoved,
        MouseClick,
        MouseDoubleClick,
        MouseButtonDown,
        MouseButtonUp,
        MouseWheel,
        MouseEnter,
        MouseLeave,
        KeyDown,
        KeyUp
    }

    public class InputEvent
    {
        public eInputEventType Type;

        public Point MousePosition;
        public MouseButtons MouseButtons;
        public int MouseWheelDelta;

        public Keys KeyData;
    }

    public class DirectXDrawable : DirectXDrawBase
    {
        public virtual int AbsoluteHeight 
        {
            get
            {
                return (int)(MainPlot.DirectXHeight * Height);
            }
            set
            {
                Height = (double)value / MainPlot.DirectXHeight;
            }
        }
        public virtual int AbsoluteWidth 
        {
            get
            {
                return (int)(MainPlot.DirectXWidth * Width);
            }
            set
            {
                Width = (double)value / MainPlot.DirectXWidth;
            }
        }
        public virtual int AbsoluteXPosition
        {
            get
            {
                return (int)(MainPlot.DirectXWidth * XPosition);
            }
            set
            {
                XPosition = (double)value / MainPlot.DirectXWidth;
            }
        }
        public virtual int AbsoluteYPosition
        {
            get
            {
                return (int)(MainPlot.DirectXHeight * YPosition);
            }
            set
            {
                YPosition = (double)value / MainPlot.DirectXHeight;
            }
        }

        private double _Width = 0;
        public virtual double Width
        {
            get
            {
                return _Width;
            }

            set
            {
                _Width = value;
                PositionUpdated = true;
            }
        }
        private double _Height = 0;
        public virtual double Height
        {
            get
            {
                return _Height;
            }

            set
            {
                _Height = value;
                PositionUpdated = true;
            }
        }
        private double _XPosition = 0;
        public virtual double XPosition
        {
            get
            {
                return _XPosition;
            }

            set
            {
                _XPosition = value;
                PositionUpdated = true;
            }
        }
        private double _YPosition = 0;
        public virtual double YPosition
        {
            get
            {
                return _YPosition;
            }

            set
            {
                _YPosition = value;
                PositionUpdated = true;
            }
        }

        protected bool PositionUpdated = false;
        public DirectXPlot MainPlot;

        public DirectXDrawable(DirectXPlot mainPlot)
        {
            MainPlot = mainPlot;

            AllocateResources();
            MainPlot.AddDrawable(this);
        }

        public virtual void UpdatePositions()
        {
        }

        public virtual bool ProcessInputEvent(InputEvent evt)
        {
            return false;
        }

        public virtual void AllocateResources()
        {
            XPosition = XPosition;
            YPosition = YPosition;
            Width = Width;
            Height = Height;
        }

        public virtual void Render()
        {
        }

        public virtual void ReleaseResources()
        {
        }

    }
}
