using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public class PlotVertsHistory : DirectXDrawable, PlotVertsSink
    {
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
                        MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineStrip, HistPlotVertsEntries[pos], HistPlotVerts[pos]);
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

                for (int pos = 0; pos < lineCount + 1; pos++)
                {
                    HistPlotVerts[HistPos][pos] = lineStripBuffer[pos];
                }

                HistPlotVertsEntries[HistPos] = lineCount;
                HistPos = (HistPos + 1) % HistLength;
            }
        }

        #endregion
    }
}
