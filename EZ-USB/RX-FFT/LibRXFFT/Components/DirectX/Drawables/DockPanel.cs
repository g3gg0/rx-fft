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

    public class DockPanel : DirectXDrawable
    {
        public eOrientation Orientation = eOrientation.RightBorder;
        public LinkedList<Dock> Docks = new LinkedList<Dock>();
        public bool FadeOutByDistance = true;
        public int FadeOutDistance = 200;

        protected bool DocksChanged = false;
        protected SlimDX.Direct3D9.Font DisplayFont;
        protected int FontSize = 9;
        protected int BorderSpaceVert = 2;
        protected int BorderSpaceHor = 4;
        protected int DockStartY = 15;
        protected double DockSlideFact = 6;
        protected bool HasDocks = false;

        protected uint TextColor = 0x00A0A0A0;
        protected uint TitleColor1 = 0x00202020;
        protected uint TitleColor2 = 0x0505050;
        protected uint TitleBorder = 0x0505050;
        protected uint TitleColorHigh1 = 0x00707070;
        protected uint TitleColorHigh2 = 0x00505050;
        protected uint CurrentAlpha = 0xFF000000;

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
            if (!HasDocks)
            {
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
                        CurrentAlpha = 0;
                        DocksChanged = true;
                        break;

                    case eInputEventType.MouseMoved:
                        double distance = LeftMostPosition - mousePos.X;

                        /* calculate alpha value by distance */
                        distance = Math.Min(1, (Math.Max(0, distance)) / FadeOutDistance);
                        uint newAlpha = ((uint)(255.0f * (1 - distance))) << 24;

                        if (newAlpha != CurrentAlpha)
                        {
                            CurrentAlpha = newAlpha;
                            DocksChanged = true;
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
                        DocksChanged = true;
                    }

                    priv.TitleHighlighted = false;

                    bool hoveringTitle = (mousePos.X > priv.TitleHitRect.X && mousePos.X < priv.TitleHitRect.X + priv.TitleHitRect.Width) && (mousePos.Y > priv.TitleHitRect.Y && mousePos.Y < priv.TitleHitRect.Y + priv.TitleHitRect.Height);
                    bool hoveringBackTitle = (mousePos.X > priv.BackTitleHitRect.X && mousePos.X < priv.BackTitleHitRect.X + priv.BackTitleHitRect.Width) && (mousePos.Y > priv.BackTitleHitRect.Y && mousePos.Y < priv.BackTitleHitRect.Y + priv.BackTitleHitRect.Height);

                    /* check if hovering over title bar */
                    if (hoveringTitle || hoveringBackTitle)
                    {
                        defaultCursor = true;
                        priv.TitleHighlighted = true;
                        DocksChanged = true;
                        handled = true;

                        /* in case of a click */
                        if (evt.Type == eInputEventType.MouseClick && evt.MouseButtons == MouseButtons.Left)
                        {
                            ToggleDock(dock);
                        }
                    }
                }
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

            return handled;
        }


        public override void Render()
        {
            /* to speed up processing */
            if (!HasDocks)
            {
                return;
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
                    xPos = DockStartY;
                    yPos = 0;
                    break;

                case eOrientation.BottomBorder:
                    xPos = MainPlot.DirectXHeight;
                    yPos = 0;
                    break;
            }

            if (DocksChanged)
            {
                lock (Docks)
                {
                    DocksChanged = false;
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
                                priv.BackTitleHitRect.Width = sizeRect.Width + BorderSpaceHor * 2;
                                priv.BackTitleHitRect.Height = sizeRect.Height + BorderSpaceVert * 2;
                                break;

                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                /* add some space around the text */
                                priv.TitleHitRect.Width = sizeRect.Height + BorderSpaceVert * 2;
                                priv.TitleHitRect.Height = sizeRect.Width + BorderSpaceHor * 2;
                                priv.BackTitleHitRect.Width = sizeRect.Height + BorderSpaceVert * 2;
                                priv.BackTitleHitRect.Height = sizeRect.Width + BorderSpaceHor * 2;
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
                                DocksChanged = true;
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
                                DocksChanged = true;
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
                        uint color1 = TitleColor1 | CurrentAlpha;
                        uint color2 = TitleColor2 | CurrentAlpha;
                        uint colorBorder = TitleBorder | CurrentAlpha;

                        if (priv.TitleHighlighted)
                        {
                            color1 = TitleColorHigh1 | CurrentAlpha;
                            color2 = TitleColorHigh2 | CurrentAlpha;
                        }

                        int titleX = DockTitleX(dock);
                        int titleY = DockTitleY(dock);
                        int backTitleX = DockBackTitleX(dock);
                        int backTitleY = DockBackTitleY(dock);
                        int titleW = priv.TitleHitRect.Width;
                        int titleH = priv.TitleHitRect.Height;

                        if (FadeOutByDistance)
                        {
                            LeftMostPosition = Math.Min(titleX, LeftMostPosition);
                            RightMostPosition = Math.Max(titleX, RightMostPosition);
                            TopMostPosition = Math.Min(titleY, TopMostPosition);
                            BottomMostPosition = Math.Max(titleY, BottomMostPosition);
                        }

                        switch (Orientation)
                        {
                            case eOrientation.BottomBorder:
                            case eOrientation.TopBorder:
                                priv.TitleBodyVertexesUsed = BuildFilledRectangle(priv.TitleBodyVertexes, priv.TitleBodyVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, color1, color1, color2, color2);
                                priv.TitleBorderVertexesUsed = BuildRectangle(priv.TitleBorderVertexes, priv.TitleBorderVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, colorBorder);
                                priv.BackTitleBodyVertexesUsed = BuildFilledRectangle(priv.BackTitleBodyVertexes, priv.BackTitleBodyVertexesUsed, backTitleX, backTitleX + titleW, backTitleY, backTitleY + titleH, color1, color1, color2, color2);
                                priv.BackTitleBorderVertexesUsed = BuildRectangle(priv.BackTitleBorderVertexes, priv.BackTitleBorderVertexesUsed, backTitleX, backTitleX + titleW, backTitleY, backTitleY + titleH, colorBorder);

                                xPos += priv.TitleHitRect.Width;
                                break;

                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                priv.TitleBodyVertexesUsed = BuildFilledRectangle(priv.TitleBodyVertexes, priv.TitleBodyVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, color1, color2, color1, color2);
                                priv.TitleBorderVertexesUsed = BuildRectangle(priv.TitleBorderVertexes, priv.TitleBorderVertexesUsed, titleX, titleX + titleW, titleY, titleY + titleH, colorBorder);
                                priv.BackTitleBodyVertexesUsed = BuildFilledRectangle(priv.BackTitleBodyVertexes, priv.BackTitleBodyVertexesUsed, backTitleX, backTitleX + titleW, backTitleY, backTitleY + titleH, color1, color2, color1, color2);
                                priv.BackTitleBorderVertexesUsed = BuildRectangle(priv.BackTitleBorderVertexes, priv.BackTitleBorderVertexesUsed, backTitleX, backTitleX + titleW, backTitleY, backTitleY + titleH, colorBorder);

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
                                DisplayFont.DrawString(priv.TitleSprite, dock.Title, priv.TitleHitRect.X + BorderSpaceHor, priv.TitleHitRect.Y + BorderSpaceVert, (int)(TextColor | CurrentAlpha));
                                break;
                            case eOrientation.LeftBorder:
                            case eOrientation.RightBorder:
                                /* rotate drawing sprite */
                                priv.TitleSprite.Transform = Matrix.RotationZ(-(float)Math.PI / 2);

                                /* draw the text */
                                DisplayFont.DrawString(priv.TitleSprite, dock.Title, -priv.TitleHitRect.Y - priv.TitleHitRect.Height + BorderSpaceHor, priv.TitleHitRect.X + BorderSpaceVert, (int)(TextColor | CurrentAlpha));
                                break;
                        }

                        priv.TitleSprite.End();
                    }
                }
            }
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
            int dockWidthTotal = dock.Width + dock.Private.TitleHitRect.Width;

            switch (Orientation)
            {
                case eOrientation.RightBorder:
                    return (int)(MainPlot.DirectXWidth - dockWidthTotal * dock.Private.VisiblePart);

                case eOrientation.LeftBorder:
                    return (int)(dock.Private.TitleHitRect.Width + dockWidthTotal * (dock.Private.VisiblePart - 1));

                default:
                    return xPos;
            }
        }

        private int DockY(int yPos, Dock dock)
        {
            int dockHeightTotal = dock.Height + dock.Private.TitleHitRect.Height;

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

    }
}
