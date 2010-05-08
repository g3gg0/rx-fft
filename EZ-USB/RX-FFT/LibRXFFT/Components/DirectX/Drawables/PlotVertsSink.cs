using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public interface PlotVertsSink
    {
        void ProcessPlotVerts(Vertex[] lineStripBuffer, int lineCount);
    }
}
