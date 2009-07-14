using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX
{
    public class DirectXBurstDisplay : DirectXWaveformDisplay
    {
        internal Font SmallFont = null;

        Vertex[] guideLines = new Vertex[6];
        public bool[] BurstBits = new bool[0];

        internal override void AllocateResources()
        {
            SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));
        }

        internal override void ReleaseResources()
        {
            if (SmallFont != null)
                SmallFont.Dispose();
            SmallFont = null;
        }



        internal void DrawBit(int bitNum, int color1, int color2, int colorText)
        {
            float stubLength = (float)DirectXHeight / 3;
            float bitStartPos = (float)(XAxisGridOffset * XZoomFactor - DisplayXOffset + ((bitNum - 0.5f) * XAxisUnit * XZoomFactor));
            float bitEndPos = (float)(XAxisGridOffset * XZoomFactor - DisplayXOffset + ((bitNum + 0.5f) * XAxisUnit * XZoomFactor));
            float yPos = DirectXHeight / 2;

            if (bitNum >= 0 && bitNum < BurstBits.Length)
                yPos = BurstBits[bitNum] ? stubLength : (DirectXHeight - stubLength);
            else
                return;


            /* draw bit margins */
            guideLines[0].PositionRhw.X = bitStartPos;
            guideLines[0].PositionRhw.Y = DirectXHeight / 2;
            guideLines[0].PositionRhw.Z = 0.5f;
            guideLines[0].PositionRhw.W = 1;
            guideLines[0].Color = color2;

            guideLines[1].PositionRhw.X = bitStartPos;
            guideLines[1].PositionRhw.Y = yPos;
            guideLines[1].PositionRhw.Z = 0.5f;
            guideLines[1].PositionRhw.W = 1;
            guideLines[1].Color = color1;

            guideLines[2].PositionRhw.X = bitEndPos;
            guideLines[2].PositionRhw.Y = DirectXHeight / 2;
            guideLines[2].PositionRhw.Z = 0.5f;
            guideLines[2].PositionRhw.W = 1;
            guideLines[2].Color = color2;

            guideLines[3].PositionRhw.X = bitEndPos;
            guideLines[3].PositionRhw.Y = yPos;
            guideLines[3].PositionRhw.Z = 0.5f;
            guideLines[3].PositionRhw.W = 1;
            guideLines[3].Color = color1;

            Device.DrawUserPrimitives(PrimitiveType.LineList, 2, guideLines);


            /* horizontal top line */
            guideLines[0].PositionRhw.X = (float)bitStartPos;
            guideLines[0].PositionRhw.Y = yPos;
            guideLines[0].PositionRhw.Z = 0.5f;
            guideLines[0].PositionRhw.W = 1;
            guideLines[0].Color = color1;

            guideLines[1].PositionRhw.X = (float)bitEndPos;
            guideLines[1].PositionRhw.Y = yPos;
            guideLines[1].PositionRhw.Z = 0.5f;
            guideLines[1].PositionRhw.W = 1;
            guideLines[1].Color = color1;


            Device.DrawUserPrimitives(PrimitiveType.LineList, 1, guideLines);
            SmallFont.DrawString(null, (bitNum + 1).ToString(), (int)(bitStartPos + bitEndPos) / 2, (int)DirectXHeight / 2, colorText);
        }

        internal override void RenderOverlay()
        {
            float stubLength = (float)DirectXHeight / 3;
            float stubWidth = 20;
            float xPos = (float)LastMousePos.X;
            float xStartPos = (float)((XAxisSampleOffset + XAxisGridOffset) * XZoomFactor);
            int bitNum = (int)((xPos - XAxisGridOffset * XZoomFactor + DisplayXOffset) / (XAxisUnit * XZoomFactor) + 0.5f);

            int colorText = 0x7FFF3030;
            int[,] colorTable = new[,] { { 0x7FFF3030, 0x7F8F0000 }, { 0x7F9F1818, 0x7F7F1818 }, { 0x7F7F0C0C, 0x7F4F0C0C }, { 0x7F5F0000, 0x7F2F0000 }, { 0x7F3F0000, 0x7F1F0000 } };

            if (AltPressed)
            {
                for (int bit = 0; bit < BurstBits.Length; bit++)
                    DrawBit(bit, colorTable[0, 0], colorTable[0, 1], colorText);
            }
            else
            {
                for (int bit = 1; bit <= 4; bit++)
                {
                    DrawBit(bitNum - bit, colorTable[bit, 0], colorTable[bit, 1], colorText);
                    DrawBit(bitNum + bit, colorTable[bit, 0], colorTable[bit, 1], colorText);
                }
                DrawBit(bitNum, colorTable[0, 0], colorTable[0, 1], colorText);
            }

            /* center line */
            guideLines[0].PositionRhw.X = xPos;
            guideLines[0].PositionRhw.Y = 0;
            guideLines[0].PositionRhw.Z = 0.5f;
            guideLines[0].PositionRhw.W = 1;
            guideLines[0].Color = 0x7F000000;

            guideLines[1].PositionRhw.X = xPos;
            guideLines[1].PositionRhw.Y = stubLength;
            guideLines[1].PositionRhw.Z = 0.5f;
            guideLines[1].PositionRhw.W = 1;
            guideLines[1].Color = 0x7FFF3030;

            guideLines[2].PositionRhw.X = xPos;
            guideLines[2].PositionRhw.Y = stubLength;
            guideLines[2].PositionRhw.Z = 0.5f;
            guideLines[2].PositionRhw.W = 1;
            guideLines[2].Color = 0x7FFF3030;

            guideLines[3].PositionRhw.X = xPos;
            guideLines[3].PositionRhw.Y = DirectXHeight - stubLength;
            guideLines[3].PositionRhw.Z = 0.5f;
            guideLines[3].PositionRhw.W = 1;
            guideLines[3].Color = 0x7FFF3030;

            guideLines[4].PositionRhw.X = xPos;
            guideLines[4].PositionRhw.Y = DirectXHeight - stubLength;
            guideLines[4].PositionRhw.Z = 0.5f;
            guideLines[4].PositionRhw.W = 1;
            guideLines[4].Color = 0x7FFF3030;

            guideLines[5].PositionRhw.X = xPos;
            guideLines[5].PositionRhw.Y = DirectXHeight;
            guideLines[5].PositionRhw.Z = 0.5f;
            guideLines[5].PositionRhw.W = 1;
            guideLines[5].Color = 0x7F000000;

            Device.DrawUserPrimitives(PrimitiveType.LineList, 3, guideLines);


        }
    }
}
