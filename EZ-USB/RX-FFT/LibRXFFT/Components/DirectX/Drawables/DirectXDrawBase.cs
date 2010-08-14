using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class DirectXDrawBase
    {
        protected int BuildFilledRectangle(Vertex[] vertexBuffer, int pos, float startX, float endX, float startY, float endY, uint color)
        {
            return BuildFilledRectangle(vertexBuffer, pos, startX, endX, startY, endY, color, color);
        }

        protected int BuildFilledRectangle(Vertex[] vertexBuffer, int pos, float startX, float endX, float startY, float endY, uint colorLeft, uint colorRight)
        {
            return BuildFilledRectangle(vertexBuffer, pos, startX, endX, startY, endY, colorLeft, colorRight, colorLeft, colorRight);
        }

        protected int BuildFilledRectangle(Vertex[] vertexBuffer, int pos, float startX, float endX, float startY, float endY, uint colorLeftUpper, uint colorRightUpper, uint colorLeftLower, uint colorRightLower)
        {
            vertexBuffer[pos].PositionRhw.X = startX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = colorLeftLower;

            vertexBuffer[pos].PositionRhw.X = startX;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = colorLeftUpper;

            vertexBuffer[pos].PositionRhw.X = endX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = colorRightLower;

            vertexBuffer[pos].PositionRhw.X = endX;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = colorRightUpper;

            return pos;
        }

        protected int BuildRectangle(Vertex[] vertexBuffer, int pos, float startX, float endX, float startY, float endY, uint color)
        {
            vertexBuffer[pos].PositionRhw.X = startX;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;

            vertexBuffer[pos].PositionRhw.X = startX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;


            vertexBuffer[pos].PositionRhw.X = startX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;

            vertexBuffer[pos].PositionRhw.X = endX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;


            vertexBuffer[pos].PositionRhw.X = endX;
            vertexBuffer[pos].PositionRhw.Y = endY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;

            vertexBuffer[pos].PositionRhw.X = endX;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;


            vertexBuffer[pos].PositionRhw.X = endX - 1;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;

            vertexBuffer[pos].PositionRhw.X = startX + 1;
            vertexBuffer[pos].PositionRhw.Y = startY;
            vertexBuffer[pos].PositionRhw.Z = 0.5f;
            vertexBuffer[pos].PositionRhw.W = 1;
            vertexBuffer[pos++].Color = color;

            return pos;
        }
    }
}
