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
        public virtual int AbsoluteHeight { get; set; }
        public virtual int AbsoluteWidth { get; set; }
        public virtual int AbsoluteXPosition { get; set; }
        public virtual int AbsoluteYPosition { get; set; }

        private double _Width = 0;
        public virtual double Width
        {
            get
            {
                return _Width;
            }

            set
            {
                AbsoluteWidth = (int)(MainPlot.DirectXWidth * value);
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
                AbsoluteHeight = (int)(MainPlot.DirectXHeight * value);
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
                AbsoluteXPosition = (int)(MainPlot.DirectXWidth * value);
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
                AbsoluteYPosition = (int)(MainPlot.DirectXHeight * value);
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
