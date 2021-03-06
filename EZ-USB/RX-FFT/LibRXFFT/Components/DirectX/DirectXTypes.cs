﻿using System.Runtime.InteropServices;
using SlimDX;

namespace LibRXFFT.Components.DirectX
{
    public enum eUserAction
    {
        None,
        YPos,
        XPos,
        YZoomIn,
        XZoomIn,
        YZoomOut,
        XZoomOut,
        YOffset,
        XOffset,
        XOffsetOverview,
        UserCallback
    }

    public enum eUserEvent
    {
        None,

        MouseEnter,
        MouseLeave,
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
        MouseWheelUp,
        MouseWheelUpShift,
        MouseWheelUpAlt,
        MouseWheelUpControl,
        MouseWheelDown,
        MouseWheelDownShift,
        MouseWheelDownAlt,
        MouseWheelDownControl,

        MouseDownLeft,
        MouseDownRight,
        MouseDownLeftAlt,
        MouseDownRightAlt,
        MouseDownLeftControl,
        MouseDownRightControl,
        MouseDownLeftShift,
        MouseDownRightShift,
        MouseUpLeft,
        MouseUpRight,
        MouseUpLeftAlt,
        MouseUpRightAlt,
        MouseUpLeftControl,
        MouseUpRightControl,
        MouseUpLeftShift,
        MouseUpRightShift,

        MouseClickLeft,
        MouseClickLeftShift,
        MouseClickLeftAlt,
        MouseClickLeftControl,
        MouseClickMiddle,
        MouseClickMiddleShift,
        MouseClickMiddleAlt,
        MouseClickMiddleControl,
        MouseClickRight,
        MouseClickRightShift,
        MouseClickRightAlt,
        MouseClickRightControl,

        MouseDoubleClickLeft,
        MouseDoubleClickLeftShift,
        MouseDoubleClickLeftAlt,
        MouseDoubleClickLeftControl,
        MouseDoubleClickMiddle,
        MouseDoubleClickMiddleShift,
        MouseDoubleClickMiddleAlt,
        MouseDoubleClickMiddleControl,
        MouseDoubleClickRight,
        MouseDoubleClickRightShift,
        MouseDoubleClickRightAlt,
        MouseDoubleClickRightControl,

        RenderOverlay,
        StatusUpdated
    }

    public struct Point
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector4 PositionRhw;
        public uint Color;
    }

    public delegate void UserEventCallbackDelegate(eUserEvent evt, double delta);
}
