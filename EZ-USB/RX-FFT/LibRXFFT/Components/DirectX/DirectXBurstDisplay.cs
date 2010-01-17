using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX
{
    public class DirectXBurstDisplay : DirectXWaveformDisplayOld
    {
        uint[,] BitLineColorTable = new uint[,] { { 0xFFFF3030, 0xF8F0000 }, { 0xFF9F1818, 0xFF7F1818 }, { 0xFF7F0C0C, 0xFF4F0C0C }, { 0xFF5F0000, 0xFF2F0000 }, { 0xFF3F0000, 0xFF1F0000 } };

        Vertex[] CursorVertexes = new Vertex[6];
        public bool[] BurstBits = new bool[0];
        

        protected void DrawBit(int bitNum, uint color1, uint color2, uint colorText)
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
            CursorVertexes[0].PositionRhw.X = bitStartPos;
            CursorVertexes[0].PositionRhw.Y = DirectXHeight / 2;
            CursorVertexes[0].PositionRhw.Z = 0.5f;
            CursorVertexes[0].PositionRhw.W = 1;
            CursorVertexes[0].Color = color2;

            CursorVertexes[1].PositionRhw.X = bitStartPos;
            CursorVertexes[1].PositionRhw.Y = yPos;
            CursorVertexes[1].PositionRhw.Z = 0.5f;
            CursorVertexes[1].PositionRhw.W = 1;
            CursorVertexes[1].Color = color1;

            CursorVertexes[2].PositionRhw.X = bitEndPos;
            CursorVertexes[2].PositionRhw.Y = DirectXHeight / 2;
            CursorVertexes[2].PositionRhw.Z = 0.5f;
            CursorVertexes[2].PositionRhw.W = 1;
            CursorVertexes[2].Color = color2;

            CursorVertexes[3].PositionRhw.X = bitEndPos;
            CursorVertexes[3].PositionRhw.Y = yPos;
            CursorVertexes[3].PositionRhw.Z = 0.5f;
            CursorVertexes[3].PositionRhw.W = 1;
            CursorVertexes[3].Color = color1;

            Device.DrawUserPrimitives(PrimitiveType.LineList, 2, CursorVertexes);


            /* horizontal top line */
            CursorVertexes[0].PositionRhw.X = (float)bitStartPos;
            CursorVertexes[0].PositionRhw.Y = yPos;
            CursorVertexes[0].PositionRhw.Z = 0.5f;
            CursorVertexes[0].PositionRhw.W = 1;
            CursorVertexes[0].Color = color1;

            CursorVertexes[1].PositionRhw.X = (float)bitEndPos;
            CursorVertexes[1].PositionRhw.Y = yPos;
            CursorVertexes[1].PositionRhw.Z = 0.5f;
            CursorVertexes[1].PositionRhw.W = 1;
            CursorVertexes[1].Color = color1;


            Device.DrawUserPrimitives(PrimitiveType.LineList, 1, CursorVertexes);
            SmallFont.DrawString(null, (bitNum + 1).ToString(), (int)(bitStartPos + bitEndPos) / 2, (int)DirectXHeight / 2, (int)colorText);
        }

        protected override void RenderOverlay()
        {
            float stubLength = (float)DirectXHeight / 3;
            float stubWidth = 20;
            float xPos = (float)LastMousePos.X;
            float xStartPos = (float)((XAxisSampleOffset + XAxisGridOffset) * XZoomFactor);
            int bitNum = (int)((xPos - XAxisGridOffset * XZoomFactor + DisplayXOffset) / (XAxisUnit * XZoomFactor) + 0.5f);

            uint colorText = 0xFFFF3030;

            if (AltPressed)
            {
                for (int bit = 0; bit < BurstBits.Length; bit++)
                    DrawBit(bit, BitLineColorTable[0, 0], BitLineColorTable[0, 1], colorText);
            }
            else
            {
                for (int bit = 1; bit <= 4; bit++)
                {
                    DrawBit(bitNum - bit, BitLineColorTable[bit, 0], BitLineColorTable[bit, 1], colorText);
                    DrawBit(bitNum + bit, BitLineColorTable[bit, 0], BitLineColorTable[bit, 1], colorText);
                }
                DrawBit(bitNum, BitLineColorTable[0, 0], BitLineColorTable[0, 1], colorText);
            }

            /* center line */
            CursorVertexes[0].PositionRhw.X = xPos;
            CursorVertexes[0].PositionRhw.Y = 0;
            CursorVertexes[0].PositionRhw.Z = 0.5f;
            CursorVertexes[0].PositionRhw.W = 1;
            CursorVertexes[0].Color = 0x00FF3030;

            CursorVertexes[1].PositionRhw.X = xPos;
            CursorVertexes[1].PositionRhw.Y = stubLength;
            CursorVertexes[1].PositionRhw.Z = 0.5f;
            CursorVertexes[1].PositionRhw.W = 1;
            CursorVertexes[1].Color = 0xFFFF3030;

            CursorVertexes[2].PositionRhw.X = xPos;
            CursorVertexes[2].PositionRhw.Y = DirectXHeight - stubLength;
            CursorVertexes[2].PositionRhw.Z = 0.5f;
            CursorVertexes[2].PositionRhw.W = 1;
            CursorVertexes[2].Color = 0xFFFF3030;

            CursorVertexes[3].PositionRhw.X = xPos;
            CursorVertexes[3].PositionRhw.Y = DirectXHeight;
            CursorVertexes[3].PositionRhw.Z = 0.5f;
            CursorVertexes[3].PositionRhw.W = 1;
            CursorVertexes[3].Color = 0x00FF3030;

            Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexes);
        }
    }
}
