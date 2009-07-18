using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Runtime.InteropServices;

namespace LibRXFFT.Components.DirectX
{
    public enum eUserAction
    {
        None,
        YPos,
        XPos,
        YZoom,
        XZoom,
        YOffset,
        XOffset,
        UserCallback
    }

    public enum eUserEvent
    {
        None,
        MousePosX,
        MousePosY,
        MouseDragX,
        MouseDragXShift,
        MouseDragXAlt,
        MouseDragXControl,
        MouseDragY,
        MouseDragYShift,
        MouseDragYAlt,
        MouseDragYControl,
        MouseWheel,
        MouseWheelShift,
        MouseWheelAlt,
        MouseWheelControl,
        RenderOverlay
    }

    public struct Point
    {
        public double X;
        public double Y;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector4 PositionRhw;
        public uint Color;
    }

    public delegate void UserEventCallbackDelegate(eUserEvent evt, double delta);
}
