using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using System.Drawing;
using LibRXFFT.Components.DirectX.Drawables.Docks;
using SlimDX;
using System.Windows.Forms;

namespace LibRXFFT.Components.DirectX.Drawables
{
    public enum eOrientation
    {
        LeftBorder,
        RightBorder,
        TopBorder,
        BottomBorder
    }

    public class DockPanelPrivate
    {
        public Vertex[] TitleBodyVertexes = new Vertex[32]; /* 4 */
        public int TitleBodyVertexesUsed = 0;
        public Vertex[] TitleBorderVertexes = new Vertex[32]; /* 8 */
        public int TitleBorderVertexesUsed = 0;
        public Vertex[] BackTitleBodyVertexes = new Vertex[32]; /* 4 */
        public int BackTitleBodyVertexesUsed = 0;
        public Vertex[] BackTitleBorderVertexes = new Vertex[32]; /* 8 */
        public int BackTitleBorderVertexesUsed = 0;
        public Rectangle TitleHitRect = new Rectangle();
        public Rectangle BackTitleHitRect = new Rectangle();
        public Sprite TitleSprite = null;
        public bool TitleHighlighted;
        public double VisiblePart = 0;
    }

    public class DockPanel : DirectXDrawable, PlotVertsSink
    {
        public eOrientation Orientation = eOrientation.RightBorder;
        public LinkedList<Dock> Docks = new LinkedList<Dock>();
        public bool FadeOutByDistance = true;
        public int FadeOutDistance = 80;

        private bool _DocksChanged = false;
        public bool DocksChanged 
        {
            get { return _DocksChanged; }
            set
            {
                if (MainPlot != null)
                {
                    MainPlot.NeedRender(this, value);
                }
                _DocksChanged = value;
            }
        }
        protected SlimDX.Direct3D9.Font DisplayFont;
        protected int FontSize = 9;
        protected int BorderSpaceVert = 2;
        protected int BorderSpaceHor = 4;

        protected int DockStartX = 0;
        protected int DockStartY = 0;

        protected double DockSlideFact = 6;
        protected bool HasDocks = false;

        protected uint TextColor = 0x00A0A0A0;
        protected uint TitleColor1 = 0x00202020;
        protected uint TitleColor2 = 0x0505050;
        protected uint TitleBorder = 0x0505050;
        protected uint TitleColorHigh1 = 0x00707070;
        protected uint TitleColorHigh2 = 0x00505050;
        protected double WantedTitleBarAlpha = 0;
        protected double CurrentTitleBarAlpha = 0;

        protected uint TitleBarAlpha
        {
            get
            {
                return ((uint)(255.0f * CurrentTitleBarAlpha)) << 24;
            }
        }

        protected int LeftMostPosition;
        protected int RightMostPosition;
        protected int TopMostPosition;
        protected int BottomMostPosition;

        protected bool SetDefaultCursor = false;

        public DockPanel(DirectXPlot mainPlot, eOrientation orientation)
            : base(mainPlot)
        {
            Orientation = orientation;
        }

        public void AddDock(Dock dock)
        {
            lock (Docks)
            {
                dock.Private = new DockPanelPrivate();
                dock.AllocateResources();

                Docks.AddLast(dock);
                DocksChanged = true;
                HasDocks = true;
            }
        }

        public override void AllocateResources()
        {
            base.AllocateResources();
            DisplayFont = new SlimDX.Direct3D9.Font(MainPlot.Device, new System.Drawing.Font("Arial", FontSize));

            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    dock.AllocateResources();
                }
                DocksChanged = true;
            }
        }

        public override void ReleaseResources()
        {
            if (DisplayFont != null)
                DisplayFont.Dispose();
            DisplayFont = null;

            base.ReleaseResources();

            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    dock.ReleaseResources();
                    if (dock.Private.TitleSprite != null)
                    {
                        dock.Private.TitleSprite.Dispose();
                        dock.Private.TitleSprite = null;
                    }
                }
            }
        }

        public void CollapseDock(Dock dock)
        {
            /* check if there are docks to expand */
            foreach (Dock d2 in Docks)
            {
                /* if this one is sticky, restore last state if required */
                if (d2.Sticky && d2.WantedState == eDockState.Expanded)
                {
                    d2.State = eDockState.Expanding;
                }
            }

            dock.State = eDockState.Collapsing;
            dock.WantedState = eDockState.Collapsed;
            DocksChanged = true;
        }

        public void ExpandDock(Dock dock)
        {
            /* collapse any other dock */
            foreach (Dock d2 in Docks)
            {
                if (d2.Sticky && dock.HideOthers)
                {
                    /* the opened dock wants to hide stickies too */
                    d2.WantedState = d2.State;
                    d2.State = eDockState.Collapsing;
                }
                else if (d2.Sticky)
                {
                    /* this one is sticky, dont close, but restore last state if required */
                    if (d2.WantedState == eDockState.Expanded)
                    {
                        d2.State = eDockState.Expanding;
                    }
                }
                else
                {
                    /* close the dock */
                    d2.State = eDockState.Collapsing;
                    d2.WantedState = eDockState.Collapsed;
                }
            }

            dock.State = eDockState.Expanding;
            dock.WantedState = eDockState.Expanded;
            DocksChanged = true;
        }

        private void ToggleDock(Dock dock)
        {
            switch (dock.State)
            {
                case eDockState.Collapsing:
                case eDockState.Collapsed:
                    ExpandDock(dock);
                    break;

                case eDockState.Expanding:
                case eDockState.Expanded:
                    CollapseDock(dock);
                    break;
            }
        }

        public override bool ProcessInputEvent(InputEvent evt)
        {
            bool docksChanged = false;

            if (!HasDocks)
            {
                DocksChanged = false;
                return false;
            }

            bool defaultCursor = false;
            bool handled = false;
            Point mousePos = evt.MousePosition;

            /* update alpha */
            if (FadeOutByDistance)
            {
                switch (evt.Type)
                {
                    case eInputEventType.MouseLeave:
                        WantedTitleBarAlpha = 0;
                        docksChanged = true;
                        break;

                    case eInputEventType.MouseMoved:
                        double distance;

                        switch (Orientation)
                        {
                            case eOrientation.RightBorder:
                                distance = LeftMostPosition - mousePos.X;
                                break;
                            case eOrientation.LeftBorder:
                                distance = RightMostPosition - mousePos.X;
                                break;
                            case eOrientation.TopBorder:
                                distance = BottomMostPosition - mousePos.Y;
                                break;
                            case eOrientation.BottomBorder:
                                distance = TopMostPosition - mousePos.Y;
                                break;
                            default:
                                distance = 0;
                                break;
                        }

                        /* calculate alpha value by distance */
                        distance = Math.Min(1, (Math.Max(0, distance)) / FadeOutDistance);
                        double newAlpha = (1 - distance);

                        if (newAlpha != WantedTitleBarAlpha)
                        {
                            WantedTitleBarAlpha = newAlpha;
                            docksChanged = true;
                        }
                        break;
                }
            }

            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    DockPanelPrivate priv = dock.Private;

                    if (priv.TitleHighlighted)
                    {
                        docksChanged = true;
                    }

                    priv.TitleHighlighted = false;

                    bool hoveringTitle = (mousePos.X > priv.TitleHitRect.X && mousePos.X < priv.TitleHitRect.X + priv.TitleHitRect.Width) && (mousePos.Y > priv.TitleHitRect.Y && mousePos.Y < priv.TitleHitRect.Y + priv.TitleHitRect.Height);
                    bool hoveringBackTitle = (mousePos.X > priv.BackTitleHitRect.X && mousePos.X < priv.BackTitleHitRect.X + priv.BackTitleHitRect.Width) && (mousePos.Y > priv.BackTitleHitRect.Y && mousePos.Y < priv.BackTitleHitRect.Y + priv.BackTitleHitRect.Height);

                    /* check if hovering over title bar */
                    if (hoveringTitle || hoveringBackTitle)
                    {
                        //defaultCursor = true;
                        priv.TitleHighlighted = true;
                        docksChanged = true;
                        handled = true;

                        /* in case of a click */
                        if (evt.Type == eInputEventType.MouseClick && evt.MouseButtons == MouseButtons.Left)
                        {
                            ToggleDock(dock);
                        }
                    }
                }
            }

            if (!handled)
            {
                lock (Docks)
                {
                    foreach (Dock dock in Docks)
                    {
                        if (dock.State == eDockState.Expanded)
                        {
                            if (mousePos.X > dock.XPosition && mousePos.X < (dock.XPosition + dock.Width)
                                && mousePos.Y > dock.YPosition && mousePos.Y < (dock.YPosition + dock.Height))
                            {
                                handled = dock.ProcessUserEvent(evt);
                            }
                        }
                    }
                }
            }

            if (handled)
            {
                defaultCursor = true;
            }

            /* need to set default cursor? */
            if (defaultCursor)
            {
                /* do that, if not already done */
                if (!SetDefaultCursor)
                {
                    SetDefaultCursor = true;
                    MainPlot.CursorType(true);
                }
            }
            else if (SetDefaultCursor)
            {
                /* set before, but do not need anymore */
                MainPlot.CursorType(false);
                SetDefaultCursor = false;
            }

            DocksChanged = docksChanged;
            return handled;
        }


        public override void Render()
        {
            bool docksChanged = false;

            /* to speed up processing */
            if (!HasDocks)
            {
                DocksChanged = false;
                return;
            }

            /* fade alpha value */
            if (WantedTitleBarAlpha >= CurrentTitleBarAlpha)
            {
                CurrentTitleBarAlpha = WantedTitleBarAlpha;
            }
            else
            {
                if (CurrentTitleBarAlpha > 0)
                {
                    CurrentTitleBarAlpha = Math.Max(0, CurrentTitleBarAlpha - 0.02f);
                    docksChanged = true;
                }
            }

            int xPos = 0;
            int yPos = 0;

            switch (Orientation)
            {
                case eOrientation.RightBorder:
                    xPos = MainPlot.DirectXWidth;
                    yPos = DockStartY;
                    break;

                case eOrientation.LeftBorder:
                    xPos = 0;
                    yPos = DockStartY;
                    break;

                case eOrientation.TopBorder:
                    xPos = DockStartX;
                    yPos = 0;
                    break;

                case eOrientation.BottomBorder:
                    xPos = DockStartX;
                    yPos = MainPlot.DirectXHeight;
                    break;
            }

            if (DocksChanged)
            {
                lock (Docks)
                {
                    Rectangle sizeRect = new Rectangle();

                    LeftMostPosition = MainPlot.DirectXWidth;
                    RightMostPosition = 0;
                    TopMostPosition = MainPlot.DirectXHeight;
                    BottomMostPosition = 0;

                    foreach (Dock dock in Docks)
                    {
                        DockPanelPrivate priv = dock.Private;

                        priv.TitleBodyVertexesUsed = 0;
                        priv.TitleBorderVertexesUsed = 0;
                        priv.BackTitleBodyVertexesUsed = 0;
                        priv.BackTitleBorderVertexesUsed = 0;

                        DisplayFont.MeasureString(null, dock.Title, DrawTextFormat.Center, ref sizeRect);

                        switch (Orientation)
                        {
                            case eOrientation.BottomBorder:
                            case eOrientation.TopBorder:
                                /* add some space around the text */
                                priv.TitleHitRect.Width = sizeRect.Width + BorderSpaceHor * 2;
                                priv.TitleHitRect.Height = sizeRect.Height + BorderSpaceVert * 2;
                                if (dock.HideBackTitle)
                                {
                                    priv.BackTitleHitRect.Width = 0;
                                    priv.BackTitleHitRect.Height = 0;
                                }
                                else
                                {
                                    priv.BackTitleHitRect.Width = sizeRect.Width + BorderSpaceHor * 2;
                                    priv.BackTitleHitRect.Height = sizeRect.Height + BorderSpaceVert * 2;
                                }
                                break;

                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                /* add some space around the text */
                                priv.TitleHitRect.Width = sizeRect.Height + BorderSpaceVert * 2;
                                priv.TitleHitRect.Height = sizeRect.Width + BorderSpaceHor * 2;
                                if (dock.HideBackTitle)
                                {
                                    priv.BackTitleHitRect.Width = 0;
                                    priv.BackTitleHitRect.Height = 0;
                                }
                                else
                                {
                                    priv.BackTitleHitRect.Width = sizeRect.Height + BorderSpaceVert * 2;
                                    priv.BackTitleHitRect.Height = sizeRect.Width + BorderSpaceHor * 2;
                                }
                                break;
                        }

                        switch (dock.State)
                        {
                            case eDockState.Expanding:
                                dock.PositionUpdated = true;

                                if (dock.Private.VisiblePart > 0.99f)
                                {
                                    dock.State = eDockState.Expanded;
                                    dock.Private.VisiblePart = 1;
                                }
                                else
                                {
                                    dock.Private.VisiblePart += (1 - dock.Private.VisiblePart) / DockSlideFact + 0.001;
                                }

                                /* force update again */
                                docksChanged = true;
                                break;

                            case eDockState.Collapsing:
                                dock.PositionUpdated = true;

                                if (dock.Private.VisiblePart < 0.01f)
                                {
                                    dock.State = eDockState.Collapsed;
                                    dock.Private.VisiblePart = 0;
                                }
                                else
                                {
                                    dock.Private.VisiblePart -= dock.Private.VisiblePart / DockSlideFact + 0.001;
                                }

                                /* force update again */
                                docksChanged = true;
                                break;

                            default:
                                break;
                        }


                        /* set dock position */
                        dock.XPosition = DockX(xPos, dock);
                        dock.YPosition = DockY(yPos, dock);
                        dock.PositionUpdated = true;

                        /* update the hit rectangle */
                        priv.TitleHitRect.X = DockTitleX(dock);
                        priv.TitleHitRect.Y = DockTitleY(dock);
                        priv.BackTitleHitRect.X = DockBackTitleX(dock);
                        priv.BackTitleHitRect.Y = DockBackTitleY(dock);

                        /* draw the title bar */
                        uint color1 = TitleColor1 | TitleBarAlpha;
                        uint color2 = TitleColor2 | TitleBarAlpha;
                        uint colorBorder = TitleBorder | TitleBarAlpha;

                        if (priv.TitleHighlighted)
                        {
                            color1 = TitleColorHigh1 | TitleBarAlpha;
                            color2 = TitleColorHigh2 | TitleBarAlpha;
                        }

                        int titleX = DockTitleX(dock);
                        int titleY = DockTitleY(dock);
                        int backTitleX = DockBackTitleX(dock);
                        int backTitleY = DockBackTitleY(dock);
                        int titleW = priv.TitleHitRect.Width;
                        int titleH = priv.TitleHitRect.Height;
                        int backTitleW = priv.BackTitleHitRect.Width;
                        int backTitleH = priv.BackTitleHitRect.Height;

                        if (FadeOutByDistance)
                        {
                            LeftMostPosition = Math.Min(titleX, LeftMostPosition);
                            RightMostPosition = Math.Max(titleX, RightMostPosition);
                            TopMostPosition = Math.Min(titleY, TopMostPosition);
                            BottomMostPosition = Math.Max(titleY, BottomMostPosition);
                        }

                        priv.TitleBodyVertexesUsed = BuildFilledRectangle(priv.TitleBodyVertexes, priv.TitleBodyVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, color1, color1, color2, color2);
                        priv.TitleBorderVertexesUsed = BuildRectangle(priv.TitleBorderVertexes, priv.TitleBorderVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, colorBorder);
                        priv.BackTitleBodyVertexesUsed = BuildFilledRectangle(priv.BackTitleBodyVertexes, priv.BackTitleBodyVertexesUsed, backTitleX, backTitleX + backTitleW, backTitleY, backTitleY + backTitleH, color1, color1, color2, color2);
                        priv.BackTitleBorderVertexesUsed = BuildRectangle(priv.BackTitleBorderVertexes, priv.BackTitleBorderVertexesUsed, backTitleX, backTitleX + backTitleW, backTitleY, backTitleY + backTitleH, colorBorder);

                        switch (Orientation)
                        {
                            case eOrientation.BottomBorder:
                            case eOrientation.TopBorder:
                                xPos += priv.TitleHitRect.Width;
                                break;

                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                yPos += priv.TitleHitRect.Height;
                                break;
                        }
                    }
                }
            }

            /* first render dock content */
            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    if (dock.State != eDockState.Collapsed && dock.State != eDockState.Hidden)
                    {
                        dock.Render();
                    }
                }
            }

            /* later render dock border stuff */
            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    if (dock.State != eDockState.Hidden)
                    {
                        DockPanelPrivate priv = dock.Private;

                        if (priv.TitleBodyVertexesUsed - 2 > 0)
                            MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, priv.TitleBodyVertexesUsed - 2, priv.TitleBodyVertexes);
                        if (priv.TitleBorderVertexesUsed > 0)
                            MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, priv.TitleBorderVertexesUsed / 2, priv.TitleBorderVertexes);

                        /* back titles are not visible for collapsed docks */
                        if (dock.State != eDockState.Collapsed)
                        {
                            if (priv.BackTitleBodyVertexesUsed - 2 > 0)
                                MainPlot.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, priv.BackTitleBodyVertexesUsed - 2, priv.BackTitleBodyVertexes);
                            if (priv.BackTitleBorderVertexesUsed > 0)
                                MainPlot.Device.DrawUserPrimitives(PrimitiveType.LineList, priv.BackTitleBorderVertexesUsed / 2, priv.BackTitleBorderVertexes);
                        }

                        if (priv.TitleSprite == null)
                        {
                            priv.TitleSprite = new Sprite(MainPlot.Device);
                        }
                        priv.TitleSprite.Begin(SpriteFlags.AlphaBlend);

                        switch (Orientation)
                        {
                            case eOrientation.BottomBorder:
                            case eOrientation.TopBorder:
                                /* draw the text */
                                DisplayFont.DrawString(priv.TitleSprite, dock.Title, priv.TitleHitRect.X + BorderSpaceHor, priv.TitleHitRect.Y + BorderSpaceVert, (int)(TextColor | TitleBarAlpha));
                                break;
                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                /* rotate drawing sprite */
                                priv.TitleSprite.Transform = Matrix.RotationZ(-(float)Math.PI / 2);

                                /* draw the text */
                                DisplayFont.DrawString(priv.TitleSprite, dock.Title, -priv.TitleHitRect.Y - priv.TitleHitRect.Height + BorderSpaceHor, priv.TitleHitRect.X + BorderSpaceVert, (int)(TextColor | TitleBarAlpha));
                                break;
                        }

                        priv.TitleSprite.End();
                    }
                }
            }

            DocksChanged = docksChanged;
        }

        private int DockTitleX(Dock dock)
        {
            switch (Orientation)
            {
                case eOrientation.RightBorder:
                    return (int)(dock.XPosition - dock.Private.TitleHitRect.Width);

                case eOrientation.LeftBorder:
                    return (int)(dock.XPosition + dock.Width);

                default:
                    return dock.XPosition;
            }
        }

        private int DockTitleY(Dock dock)
        {
            switch (Orientation)
            {
                case eOrientation.BottomBorder:
                    return (int)(dock.YPosition - dock.Private.TitleHitRect.Height);

                case eOrientation.TopBorder:
                    return (int)(dock.YPosition + dock.Height);

                default:
                    return dock.YPosition;
            }
        }

        private int DockBackTitleX(Dock dock)
        {
            switch (Orientation)
            {
                case eOrientation.RightBorder:
                    return (int)(dock.XPosition + dock.Width);

                case eOrientation.LeftBorder:
                    return (int)(dock.XPosition - dock.Private.TitleHitRect.Width);

                default:
                    return dock.XPosition;
            }
        }

        private int DockBackTitleY(Dock dock)
        {
            switch (Orientation)
            {
                case eOrientation.BottomBorder:
                    return (int)(dock.YPosition + dock.Height);

                case eOrientation.TopBorder:
                    return (int)(dock.YPosition - dock.Private.TitleHitRect.Height);

                default:
                    return dock.YPosition;
            }
        }

        private int DockX(int xPos, Dock dock)
        {
            double dockWidthTotal = dock.Width + dock.Private.BackTitleHitRect.Width;

            switch (Orientation)
            {
                case eOrientation.RightBorder:
                    return (int)(MainPlot.DirectXWidth - dockWidthTotal * dock.Private.VisiblePart);

                case eOrientation.LeftBorder:
                    return (int)(dock.Private.BackTitleHitRect.Width + dockWidthTotal * (dock.Private.VisiblePart - 1));

                default:
                    return xPos;
            }
        }

        private int DockY(int yPos, Dock dock)
        {
            double dockHeightTotal = dock.Height + dock.Private.BackTitleHitRect.Height;

            switch (Orientation)
            {
                case eOrientation.BottomBorder:
                    return (int)(MainPlot.DirectXHeight - dockHeightTotal * dock.Private.VisiblePart);

                case eOrientation.TopBorder:
                    return (int)(dock.Private.TitleHitRect.Height + dockHeightTotal * (dock.Private.VisiblePart - 1));

                default:
                    return yPos;
            }
        }


        #region PlotVertsSink Member


        public void ProcessPlotVerts(Vertex[] verts, int vertsCount)
        {
            lock (Docks)
            {
                foreach (Dock dock in Docks)
                {
                    if (dock is PlotVertsSink)
                    {
                        ((PlotVertsSink)dock).ProcessPlotVerts(verts, vertsCount);
                    }
                }
            }
        }

        #endregion

    }
}
