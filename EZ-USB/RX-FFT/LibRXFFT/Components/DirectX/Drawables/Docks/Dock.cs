using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables.Docks
{
    public class Dock : DirectXDrawBase
    {
        public virtual string Title { get; set; }
        public virtual float Height { get; set; }
        public virtual float Width { get; set; }
        public virtual bool Sticky { get; set; }
        public virtual bool HideOthers { get; set; }
        public virtual bool HideBackTitle { get; set; }
        

        public int XPosition = 0;
        public int YPosition = 0;
        public bool PositionUpdated = false;
        public eDockState State = eDockState.Collapsed;
        public eDockState WantedState = eDockState.Collapsed;
        public DockPanelPrivate Private;

        protected DockPanel Panel;

        public Dock(DockPanel panel)
        {
            Panel = panel;
            Title = "Unknown";

            Panel.AddDock(this);
        }

        public virtual bool ProcessUserEvent(InputEvent evt)
        {
            return false;
        }

        public virtual void AllocateResources()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void ReleaseResources()
        {
        }

        public DirectXPlot MainPlot
        {
            get
            {
                return Panel.MainPlot;
            }
        }
    }
}
