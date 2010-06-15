using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class TestDock : Dock
    {
        public override string Title { get; set; }
        public override float Height { get; set; }
        public override float Width { get; set; }
        public override bool Sticky { get; set; }
        public override bool HideOthers { get; set; }

        public TestDock(DockPanel panel) : base(panel)
        {
            Title = "Test";
        }

    }
}
