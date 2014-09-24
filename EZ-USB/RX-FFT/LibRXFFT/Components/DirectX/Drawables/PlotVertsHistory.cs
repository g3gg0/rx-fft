using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class PlotVertsHistory : DirectXDrawable, PlotVertsSink
    {
        public decimal SampleDist = 1;
        public decimal SamplePos = 0;

        public bool Enabled = true;
        public long HistLength
        {
            get
            {
                return HistPlotVertsEntries.Length;
            }
            set
            {
                lock (this)
                {
                    HistPos = 0;
                    HistPlotVertsEntries = new int[Math.Max(1, value)];
                    HistPlotVerts = new Vertex[Math.Max(1, value)][];
                }
            }
        }

        protected long HistPos = 0;
        protected int[] HistPlotVertsEntries = new int[1];
        protected Vertex[][] HistPlotVerts = new Vertex[1][];

        public PlotVertsHistory(DirectXPlot mainPlot)
            : base(mainPlot)
        {
            HistLength = 1;
        }

        public override void Render()
        {
            if (!Enabled)
            {
                return;
            }

            lock (this)
            {
                for (int pos = 0; pos < HistLength; pos++)
                {
                    if (HistPlotVertsEntries[pos] > 0)
                    {
                        if (SampleDist == 1)
                        {
                            MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineStrip, HistPlotVertsEntries[pos], HistPlotVerts[pos]);
                        }
                        else
                        {
                            MainPlot.Device.DrawUserPrimitives(PrimitiveType.PointList, HistPlotVertsEntries[pos], HistPlotVerts[pos]);
                        }
                    }
                }
            }
        }

        #region PlotVertsSink Member

        public void ProcessPlotVerts(Vertex[] lineStripBuffer, int lineCount)
        {
            if (!Enabled)
            {
                return;
            }

            lock (this)
            {
                if (HistPlotVerts[HistPos] == null || lineCount + 1 > HistPlotVerts[HistPos].Length)
                {
                    HistPlotVerts[HistPos] = new Vertex[lineCount + 1];
                }

                decimal dist = SampleDist;
                int outPos = 0;

                if (dist < 0.01m)
                {
                    dist = 0.01m;
                }

                for (decimal pos = SamplePos; pos < lineCount + 1; pos += dist)
                {
                    HistPlotVerts[HistPos][outPos] = lineStripBuffer[(int)pos];
                    HistPlotVerts[HistPos][outPos].Color = 0xFFFF0000;
                    outPos++;
                }

                HistPlotVertsEntries[HistPos] = outPos - 1;
                HistPos = (HistPos + 1) % HistLength;
            }
        }

        #endregion
    }
}
