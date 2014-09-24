using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using LibRXFFT.Libraries.FFTW;
using LibRXFFT.Libraries.SignalProcessing;
using SlimDX;
using SlimDX.Direct3D9;
using Font = SlimDX.Direct3D9.Font;
using LibRXFFT.Components.Generic;
using System.Collections;
using System.Collections.Generic;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaterfallDisplay : DirectXFFTDisplay
    {
        public bool DynamicLimits = false;
        public double DynamicLimitFact = 0.1f;
        public double DynamicBaseLevel = 0;
        private bool DynamicBaseLevelChanged = false;
        public bool DrawTimeStamps = true;

        /* DirectX related graphic stuff */
        protected override MultisampleType SuggestedMultisample { get { return MultisampleType.None; } }

        protected Sprite Sprite;
        protected Surface DefaultRenderTarget;
        protected Texture WaterfallTexture;
        protected Texture TempWaterfallTexture;

        protected struct TextureInfo
        {
            public Texture texture;
            public int lines;
        }

        protected Queue<TextureInfo> SaveImageDisplayContextsFree = new Queue<TextureInfo>();
        protected Queue<TextureInfo> SaveImageDisplayContextsFilled = new Queue<TextureInfo>();
        protected Texture SaveWaterfallTexture;
        protected Texture SaveTempWaterfallTexture;
        protected Color ColorFaderBG = Color.Orange;

        public ColorLookupTable ColorTable = new MultiColorMap(8192, Color.Black, Color.FromArgb(0, 0, 128), Color.FromArgb(192, 0, 255), Color.White);

        protected double DisplayXOffsetPrev = 0;
        protected Object DisplayXOffsetLock = new Object();
        protected bool ResetScaleBar = true;

        /* timestamp related */
        protected DateTime TimeStamp = DateTime.Now;
        protected int LinesWithoutTimestamp = 0;
        protected int LinesWithoutTimestampMin = 20;
        public double TimeStampEveryMiliseconds = 1000;

        /* waterfall saving related */
        private bool SaveThreadRunning = true;
        public string _SavingName = "waterfall.png";
        private string SavingNameExtended = "waterfall.png";
        private int SavingNamePostfix = 0;
        public string SavingName
        {
            get { return _SavingName; }
            set
            {
                _SavingName = value;
                SavingNameExtended = value;
                SavingNamePostfix = 0;
            }
        }

        protected bool _SavingEnabled = false;
        public bool SavingEnabled
        {
            get { return _SavingEnabled; }
            set
            {
                if (value)
                {
                    /* reset parameters */
                    _SavingEnabled = true;
                }
                else
                {
                    _SavingEnabled = false;
                }

                /* notify thread */
                lock (SaveBufferTrigger)
                {
                    Monitor.Pulse(SaveBufferTrigger);
                }
            }
        }

        public bool LevelBarActive = false;

        protected Mutex SaveImageLock = new Mutex();
        protected Semaphore SaveBufferTrigger = new Semaphore(0, 1);
        protected PresentParameters SaveParameters;
        protected Device SaveDeviceDisplayContext;
        protected Device SaveDeviceThreadContext;
        protected Sprite SaveSprite;
        protected Thread SaveThread;
        protected Image SavedImage;
        protected Font SaveFixedFont;
        protected int SaveWaterfallLinesRendered = 0;
        protected int SaveWaterfallLinesToRender = 0;
        protected int SaveImageBlockSize = 512;

        public double LeveldBWhite = -10;
        public double LeveldBBlack = -100;
        public double LeveldBMax = -150;
        public double ApproxMaxStrength;




        public DirectXWaterfallDisplay()
            : this(false)
        {
        }

        public DirectXWaterfallDisplay(bool slaveMode)
            : base(slaveMode)
        {
            ColorFG = Color.Cyan;
            ColorBG = Color.Black;
            ColorFont = Color.DarkCyan;
            ColorCursor = Color.Red;

            YAxisCentered = false;
            YZoomFactor = 0.01f;
            XZoomFactor = 1.0f;

            EventActions[eUserEvent.MouseDragY] = eUserAction.None;
            EventActions[eUserEvent.MouseWheelUp] = eUserAction.None;
            EventActions[eUserEvent.MouseWheelDown] = eUserAction.None;
            EventActions[eUserEvent.MouseWheelUpShift] = eUserAction.YOffset;
            EventActions[eUserEvent.MouseWheelDownShift] = eUserAction.YOffset;
            EventActions[eUserEvent.MouseWheelUpControl] = eUserAction.YZoomIn;
            EventActions[eUserEvent.MouseWheelDownControl] = eUserAction.YZoomOut;

            /* thats not possible with this display */
            OverviewModeEnabled = false;

            /* periodically run the save routine that checks if an texture should get
             * appended to the saved waterfall image
             */
            SaveThread = new Thread(new ThreadStart(SaveThreadFunc));
            SaveThread.Name = "WaterfallSaveThread";
            SaveThread.Start();
        }


        private void SaveThreadFunc()
        {
            try
            {
                while (SaveThreadRunning)
                {
                    lock (SaveBufferTrigger)
                    {
                        /* wait until we should do something */
                        Monitor.Wait(SaveBufferTrigger, 250);
                    }

                    /* if there is a new texture to save */
                    if (SaveImageDisplayContextsFilled.Count == 0)
                    {
                        /* clear old file when got disabled */
                        if (!SavingEnabled && SavedImage != null)
                        {
                            SavedImage.Dispose();
                            SavedImage = null;
                        }
                        continue;
                    }

                    try
                    {
                        /* build an image from the texture */
                        TextureInfo front = SaveImageDisplayContextsFilled.Dequeue();

                        DataStream saveImageStream = Texture.ToStream(front.texture, ImageFileFormat.Png);
                        Image curImage = Image.FromStream(saveImageStream);

                        SaveImageDisplayContextsFree.Enqueue(front);

                        /* and create a new image with the new size */
                        Image newImage;
                        if (SavedImage != null && SavedImage.Width == curImage.Width)
                        {
                            newImage = new Bitmap(curImage.Width, SavedImage.Height + front.lines);
                        }
                        else
                        {
                            newImage = new Bitmap(curImage.Width, front.lines);
                        }

                        /* draw last image and the current texture into the new image */
                        Graphics g = Graphics.FromImage(newImage);

                        /* but skip the first few lines to make sure timestamp gets drawn */
                        int offset = -(FixedFontHeight + 1);

                        /* dont skip when saving got disabled and this is our last run */
                        if (!SavingEnabled)
                        {
                            offset = 0;
                        }
                        offset = 0;

                        g.DrawImage(curImage, 0, 0 + offset);
                        if (SavedImage != null)
                        {
                            g.DrawImage(SavedImage, 0, front.lines + offset);
                        }

                        /* save it to disk */
                        try
                        {
                            newImage.Save(SavingNameExtended, ImageFormat.Png);
                        }
                        catch (Exception e)
                        {
                        }

                        /* dispose old image */
                        if (SavedImage != null)
                            SavedImage.Dispose();

                        /* and keep for the next time */
                        SavedImage = newImage;
                    }
                    catch (Exception e)
                    {
                        /* clear current image and save to a new file
                         * TODO: correct filename ;)
                         */
                        SavingNamePostfix++;
                        string savingName = "";

                        string[] parts = _SavingName.Split('.');
                        if (parts.Length > 1)
                        {
                            for (int part = 0; part < parts.Length - 1; part++)
                            {
                                savingName += parts[part] + ".";
                            }
                            savingName += "_" + SavingNamePostfix + "." + parts[parts.Length - 1];
                        }
                        else
                        {
                            savingName = _SavingName + "_" + SavingNamePostfix;
                        }

                        SavingNameExtended = savingName;

                        if (SavedImage != null)
                        {
                            SavedImage.Dispose();
                            SavedImage = null;
                        }
                    }
                }
            }
            catch (ThreadAbortException e)
            {
            }
        }

        public int FFTSize
        {
            get { return _FFTSize; }
            set
            {
                lock (FFTLock)
                {
                    lock (SampleValues)
                    {
                        _FFTSize = value;
                        InitializeDirectX();
                        SampleValuesAveraged = 0;
                        EnoughData = false;
                        FFT = new FFTTransformer(value);
                    }
                }
            }
        }

        protected override void RenderCursor()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();

            if (UpdateCursor)
            {
                UpdateCursor = false;

                /* draw vertical cursor line */
                float stubLength = (float)DirectXHeight / 10.0f;
                float horLineHeight = (float)DirectXHeight / 10.0f;
                float xPos = (float)LastMousePos.X;
                float yPos = (float)LastMousePos.Y;

                CursorVertexesVert[0].PositionRhw.X = xPos;
                CursorVertexesVert[1].PositionRhw.X = xPos;
                CursorVertexesVert[2].PositionRhw.X = xPos;
                CursorVertexesVert[3].PositionRhw.X = xPos;

                /* recalc lines (this is needed just once btw.) */
                CursorVertexesVert[0].PositionRhw.Y = 0;
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = colorCursor;

                CursorVertexesVert[1].PositionRhw.Y = stubLength;
                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = colorCursor;

                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = colorCursor;

                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;
                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = colorCursor & 0x00FFFFFF;

                if (HorLineFixed)
                {
                    float absoluteWidthStart = (float)(XPosFromFrequency(CenterFrequency + HorLineStart) - XPosFromFrequency(CenterFrequency));
                    float absoluteWidthEnd = (float)(XPosFromFrequency(CenterFrequency + HorLineEnd) - XPosFromFrequency(CenterFrequency));

                    /* horizontal line */
                    CursorVertexesHor[0].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHor[0].PositionRhw.Y = yPos;
                    CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[0].PositionRhw.W = 1;
                    CursorVertexesHor[0].Color = colorCursor;

                    CursorVertexesHor[1].PositionRhw.X = xPos;
                    CursorVertexesHor[1].PositionRhw.Y = yPos;
                    CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[1].PositionRhw.W = 1;
                    CursorVertexesHor[1].Color = colorCursor;

                    CursorVertexesHor[2].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHor[2].PositionRhw.Y = yPos;
                    CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[2].PositionRhw.W = 1;
                    CursorVertexesHor[2].Color = colorCursor;

                    /* 1 */
                    CursorVertexesHorSide[0].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[0].PositionRhw.Y = yPos - horLineHeight;
                    CursorVertexesHorSide[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[0].PositionRhw.W = 1;
                    CursorVertexesHorSide[0].Color = colorCursor & 0x00FFFFFF;

                    CursorVertexesHorSide[1].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[1].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[1].PositionRhw.W = 1;
                    CursorVertexesHorSide[1].Color = colorCursor;

                    /* 2 */
                    CursorVertexesHorSide[2].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[2].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[2].PositionRhw.W = 1;
                    CursorVertexesHorSide[2].Color = colorCursor;

                    CursorVertexesHorSide[3].PositionRhw.X = xPos + absoluteWidthStart;
                    CursorVertexesHorSide[3].PositionRhw.Y = yPos + horLineHeight;
                    CursorVertexesHorSide[3].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[3].PositionRhw.W = 1;
                    CursorVertexesHorSide[3].Color = colorCursor & 0x00FFFFFF;

                    /* 3 */
                    CursorVertexesHorSide[4].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[4].PositionRhw.Y = yPos - horLineHeight;
                    CursorVertexesHorSide[4].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[4].PositionRhw.W = 1;
                    CursorVertexesHorSide[4].Color = colorCursor & 0x00FFFFFF;

                    CursorVertexesHorSide[5].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[5].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[5].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[5].PositionRhw.W = 1;
                    CursorVertexesHorSide[5].Color = colorCursor;

                    /* 4 */
                    CursorVertexesHorSide[6].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[6].PositionRhw.Y = yPos;
                    CursorVertexesHorSide[6].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[6].PositionRhw.W = 1;
                    CursorVertexesHorSide[6].Color = colorCursor;

                    CursorVertexesHorSide[7].PositionRhw.X = xPos + absoluteWidthEnd;
                    CursorVertexesHorSide[7].PositionRhw.Y = yPos + horLineHeight;
                    CursorVertexesHorSide[7].PositionRhw.Z = 0.5f;
                    CursorVertexesHorSide[7].PositionRhw.W = 1;
                    CursorVertexesHorSide[7].Color = colorCursor & 0x00FFFFFF;
                }
                else
                {
                    /* horizontal line */
                    CursorVertexesHor[0].PositionRhw.X = xPos - DirectXWidth / 20;
                    CursorVertexesHor[0].PositionRhw.Y = yPos;
                    CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[0].PositionRhw.W = 1;

                    CursorVertexesHor[0].Color = colorCursor & 0x00FFFFFF;
                    CursorVertexesHor[1].PositionRhw.X = xPos;
                    CursorVertexesHor[1].PositionRhw.Y = yPos;
                    CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[1].PositionRhw.W = 1;
                    CursorVertexesHor[1].Color = colorCursor;

                    CursorVertexesHor[2].PositionRhw.X = xPos + DirectXWidth / 20;
                    CursorVertexesHor[2].PositionRhw.Y = yPos;
                    CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                    CursorVertexesHor[2].PositionRhw.W = 1;
                    CursorVertexesHor[2].Color = colorCursor & 0x00FFFFFF;
                }
            }

            if (MouseHovering || ShowVerticalCursor)
            {
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexesVert);

                if (MouseHovering)
                {
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);
                }
            }
        }


        protected override void RenderAxis()
        {
        }

        protected override void RenderOverlay()
        {
            uint colorCursor = (uint)ColorCursor.ToArgb();

            /* draw white/black bar */
            uint color = (uint)ColorFaderBG.ToArgb();
            uint colorBarUpper = (uint)Color.White.ToArgb();
            uint colorBarLower = (uint)Color.White.ToArgb();
            int barLength = 50;
            int barTop = DirectXHeight - barLength - 20;
            int barBottom = barTop + barLength;
            int whiteYPos = (int)((LeveldBWhite / LeveldBMax) * barLength);
            int blackYPos = (int)((LeveldBBlack / LeveldBMax) * barLength);

            if (ShiftPressed)
                colorBarUpper = (uint)Color.Green.ToArgb();
            if (ControlPressed)
                colorBarLower = (uint)Color.Green.ToArgb();

            if (UpdateOverlays)
            {
                UpdateOverlays = false;
                UpdateDrawablePositions();

                OverlayVertexesUsed = 0;

                /* center line */
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x80FFFFFF;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + 10;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + 10;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barBottom - 10;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barBottom - 10;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 20;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barBottom;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x80FFFFFF;
                OverlayVertexesUsed++;

                /* left line */
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 19;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x00FFFFFF;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 19;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 19;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 19;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barBottom;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x00FFFFFF;
                OverlayVertexesUsed++;

                /* right line */
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 21;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x00FFFFFF;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 21;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 21;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 21;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barBottom;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = color & 0x00FFFFFF;
                OverlayVertexesUsed++;

                /* draw white/black limiter */

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 15;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + whiteYPos;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorBarUpper;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 25;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + whiteYPos;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = colorBarUpper;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 15;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + blackYPos;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 1;
                OverlayVertexes[OverlayVertexesUsed].Color = colorBarLower;
                OverlayVertexesUsed++;

                OverlayVertexes[OverlayVertexesUsed].PositionRhw.X = 25;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Y = barTop + blackYPos;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.Z = 0.5f;
                OverlayVertexes[OverlayVertexesUsed].PositionRhw.W = 2;
                OverlayVertexes[OverlayVertexesUsed].Color = colorBarLower;
                OverlayVertexesUsed++;
            }

            if (OverlayVertexesUsed > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, OverlayVertexesUsed / 2, OverlayVertexes);
            SmallFont.DrawString(null, string.Format("{0:0.0} dB", LeveldBWhite), 27, barTop + whiteYPos - 6, (int)colorBarUpper);
            SmallFont.DrawString(null, string.Format("{0:0.0} dB", LeveldBBlack), 27, barTop + blackYPos - 6, (int)colorBarLower);
        }

        public override void PrepareLinePoints()
        {
            bool resetAverage = !LinePointsUpdated;
            double maxLevel = double.MinValue;
            double minLevel = double.MaxValue;

            lock (SampleValues)
            {
                if (SampleValuesAveraged > 0)
                {
                    int samples = SampleValues.Length;

                    lock (LinePointsLock)
                    {
                        if (LinePoints == null || LinePoints.Length < samples)
                        {
                            Array.Resize(ref LinePoints, samples);
                        }

                        for (int pos = 0; pos < samples; pos++)
                        {
                            double posY = SampleValues[pos];
                            double posX = pos;

                            LinePoints[pos].X = posX;

                            /* if configured to track peaks, dont average values */
                            if (SampleValuesTrackPeaks)
                            {
                                SampleValuesAveraged = 1;
                            }

                            unchecked
                            {
                                if (VerticalSmooth != 1.0f)
                                {
                                    /* some simple averaging */
                                    LinePoints[pos].Y *= (VerticalSmooth - 1);
                                    LinePoints[pos].Y += posY / SampleValuesAveraged;
                                    LinePoints[pos].Y /= VerticalSmooth;
                                }
                                else
                                {
                                    LinePoints[pos].Y = posY / SampleValuesAveraged;
                                }
                            }

                            if (double.IsNaN(LinePoints[pos].Y))
                            {
                                LinePoints[pos].Y = 0;
                            }

                            if (DynamicLimits)
                            {
                                maxLevel = Math.Max(maxLevel, LinePoints[pos].Y);
                                minLevel = Math.Min(minLevel, LinePoints[pos].Y);
                            }
                        }
                        resetAverage = false;
                        LinePointEntries = samples;
                        LinePointsUpdated = true;
                    }
                    SampleValuesAveraged = 0;
                }
                EnoughDataReset = true;
                EnoughData = false;
            }

            if (DynamicLimits)
            {
                LeveldBBlack -= DynamicBaseLevel;
                LeveldBWhite -= 10;

                float dBmax = (float)(maxLevel - BaseAmplification);
                float dBmin = (float)(minLevel - BaseAmplification);

                LeveldBWhite = (LeveldBWhite + dBmax * DynamicLimitFact) / (1 + DynamicLimitFact);
                ApproxMaxStrength = LeveldBWhite;

                if (DynamicBaseLevelChanged)
                {
                    DynamicBaseLevelChanged = false;
                    LeveldBBlack = dBmin;
                }
                else
                {
                    LeveldBBlack = (LeveldBBlack + dBmin * DynamicLimitFact) / (1 + DynamicLimitFact);
                }

                if (double.IsInfinity(LeveldBWhite) || double.IsNaN(LeveldBWhite))
                {
                    LeveldBWhite = 0;
                }
                if (double.IsInfinity(LeveldBBlack) || double.IsNaN(LeveldBBlack))
                {
                    LeveldBBlack = 0;
                }

                UpdateOverlays = true;
                LeveldBWhite += 10;
                LeveldBBlack += DynamicBaseLevel;

                /* inform that smth has changed */
                if (UserEventCallback != null)
                    UserEventCallback(eUserEvent.StatusUpdated, 0);
            }
        }


        protected override void AllocateDevices()
        {
            base.AllocateDevices();

            /* we dont need to allocate that all the time. once is enough */
            if (SaveParameters == null)
            {
                SaveParameters = new PresentParameters();
                SaveParameters.BackBufferHeight = Math.Min(SaveImageBlockSize, DeviceCaps.MaxTextureHeight);
                SaveParameters.BackBufferWidth = Math.Min(FFTSize, DeviceCaps.MaxTextureWidth);
                SaveParameters.DeviceWindowHandle = PresentParameters.DeviceWindowHandle;
                SaveParameters.BackBufferFormat = PresentParameters.BackBufferFormat;
                SaveParameters.Multisample = PresentParameters.Multisample;
            }

            SaveDeviceThreadContext = new Device(Direct3D, DefaultAdapter, DeviceType.Hardware, SaveParameters.DeviceWindowHandle, DeviceCreateFlags, SaveParameters);
            SaveDeviceThreadContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

            SaveDeviceThreadContext.SetRenderState(RenderState.AlphaBlendEnable, true);
            SaveDeviceThreadContext.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            SaveDeviceThreadContext.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            SaveDeviceThreadContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

            SaveDeviceDisplayContext = new Device(Direct3D, DefaultAdapter, DeviceType.Hardware, SaveParameters.DeviceWindowHandle, DeviceCreateFlags, SaveParameters);
            SaveDeviceDisplayContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

            SaveDeviceDisplayContext.SetRenderState(RenderState.AlphaBlendEnable, true);
            SaveDeviceDisplayContext.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            SaveDeviceDisplayContext.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            SaveDeviceDisplayContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        }

        protected override void ResetDevices()
        {
            base.ResetDevices();

            SaveParameters.BackBufferHeight = Math.Min(SaveImageBlockSize, DeviceCaps.MaxTextureHeight);
            SaveParameters.BackBufferWidth = Math.Min(FFTSize, DeviceCaps.MaxTextureWidth);

            SaveDeviceThreadContext.Reset(SaveParameters);
            SaveDeviceThreadContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
            SaveDeviceDisplayContext.Reset(SaveParameters);
            SaveDeviceDisplayContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
        }

        protected override void ReleaseDevices()
        {
            base.ReleaseDevices();

            if (SaveDeviceDisplayContext != null)
                SaveDeviceDisplayContext.Dispose();
            if (SaveDeviceThreadContext != null)
                SaveDeviceThreadContext.Dispose();
            SaveDeviceDisplayContext = null;
            SaveDeviceThreadContext = null;
        }


        protected override void AllocateResources()
        {
            base.AllocateResources();

            DefaultRenderTarget = Device.GetRenderTarget(0);
            AddAllocatedResource(DefaultRenderTarget);

            WaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            AddAllocatedResource(WaterfallTexture);

            TempWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            AddAllocatedResource(TempWaterfallTexture);

            Sprite = new Sprite(Device);
            AddAllocatedResource(Sprite);

            /* save file every screen roll-over */
            SaveWaterfallLinesToRender = SaveParameters.BackBufferHeight;
            SaveWaterfallLinesRendered = 0;

            SaveWaterfallTexture = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            AddAllocatedResource(SaveWaterfallTexture);

            SaveTempWaterfallTexture = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            AddAllocatedResource(SaveTempWaterfallTexture);

            for (int pos = 0; pos < 20; pos++)
            {
                TextureInfo info = new TextureInfo();
                info.texture = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                ClearTexture(SaveDeviceDisplayContext, info.texture);
                AddAllocatedResource(info.texture);

                SaveImageDisplayContextsFree.Enqueue(info);
            }

            SaveFixedFont = new Font(SaveDeviceDisplayContext, new System.Drawing.Font("Courier New", 8));
            AddAllocatedResource(SaveFixedFont);

            SaveSprite = new Sprite(SaveDeviceDisplayContext);
            AddAllocatedResource(SaveSprite);

            /* clear textures */
            ClearTexture(Device, WaterfallTexture);
            ClearTexture(Device, TempWaterfallTexture);
            ClearTexture(SaveDeviceDisplayContext, SaveWaterfallTexture);
            ClearTexture(SaveDeviceDisplayContext, SaveTempWaterfallTexture);
        }


        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            SaveImageDisplayContextsFree.Clear();
            SaveImageDisplayContextsFilled.Clear();
            SaveTempWaterfallTexture = null;
            SaveWaterfallTexture = null;
            Sprite = null;
            SaveSprite = null;
            WaterfallTexture = null;
            TempWaterfallTexture = null;
            SaveFixedFont = null;
            DefaultRenderTarget = null;
        }


        private void ClearTexture(Device device, Texture texture)
        {
            device.BeginScene();
            device.SetRenderTarget(0, texture.GetSurfaceLevel(0));
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
            device.EndScene();
            device.Present();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
        }

        protected override void ResetModifiers(bool forceUnhover)
        {
            LevelBarActive = false;

            base.ResetModifiers(forceUnhover);
        }

        protected override void RefreshLinePoints()
        {
            /* dont do that - else the waterfall scrolls during mouse move etc */
        }

        protected override void KeyPressed(Keys key)
        {
            /* set main text and update overlays so the active bar gets colored */
            if (key == Keys.Shift && !DynamicLimits)
            {
                MainTextPrev = MainText;
                MainText = "Change Upper Limit (White)";
                UpdateOverlays = true;
                LevelBarActive = true;

                if (UserEventCallback != null)
                    UserEventCallback(eUserEvent.StatusUpdated, 0);
            }

            if (key == Keys.Control)
            {
                MainTextPrev = MainText;
                MainText = "Change Lower Limit (Black)";
                UpdateOverlays = true;
                LevelBarActive = true;

                if (UserEventCallback != null)
                    UserEventCallback(eUserEvent.StatusUpdated, 0);
            }
        }

        protected override void KeyReleased(Keys key)
        {
            /* reset main text and update overlays so the active bar gets colored */
            if (key == Keys.Shift)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
                UpdateOverlays = true;
                LevelBarActive = false;

                if (UserEventCallback != null)
                    UserEventCallback(eUserEvent.StatusUpdated, 0);
            }

            if (key == Keys.Control)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
                UpdateOverlays = true;
                LevelBarActive = false;

                if (UserEventCallback != null)
                    UserEventCallback(eUserEvent.StatusUpdated, 0);
            }
        }

        public override void ProcessUserAction(eUserAction action, double param)
        {
            switch (action)
            {
                case eUserAction.YOffset:
                    if (!DynamicLimits)
                    {
                        LeveldBWhite = Math.Max(LeveldBBlack, Math.Min(0, LeveldBWhite + 2 * Math.Sign(param)));
                        UpdateOverlays = true;

                        /* inform that smth has changed */
                        if (UserEventCallback != null)
                            UserEventCallback(eUserEvent.StatusUpdated, 0);
                    }
                    break;

                case eUserAction.YZoomIn:
                    if (!DynamicLimits)
                    {
                        LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack + 2));
                    }
                    else
                    {
                        DynamicBaseLevel = Math.Max(LeveldBMax, Math.Min(-LeveldBMax, DynamicBaseLevel + 2));
                        DynamicBaseLevelChanged = true;
                    }

                    UpdateOverlays = true;

                    /* inform that smth has changed */
                    if (UserEventCallback != null)
                        UserEventCallback(eUserEvent.StatusUpdated, 0);
                    break;

                case eUserAction.YZoomOut:
                    if (!DynamicLimits)
                    {
                        LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack - 2));
                    }
                    else
                    {
                        DynamicBaseLevel = Math.Max(LeveldBMax, Math.Min(-LeveldBMax, DynamicBaseLevel - 2));
                        DynamicBaseLevelChanged = true;
                    }
                    UpdateOverlays = true;

                    /* inform that smth has changed */
                    if (UserEventCallback != null)
                        UserEventCallback(eUserEvent.StatusUpdated, 0);
                    break;

                default:
                    /* in any other case lock the DisplayXOffset variable */
                    lock (DisplayXOffsetLock)
                    {
                        base.ProcessUserAction(action, param);
                    }
                    break;
            }

            NeedsRender = true;
        }


        protected override void CreateVertexBufferForPoints(Point[] points, int numPoints)
        {
            if (points == null)
                return;

            try
            {
                if (numPoints > 0)
                {
                    if (numPoints != PlotVertsOverview.Length)
                    {
                        Array.Resize(ref PlotVertsOverview, numPoints);
                        //PlotVertsOverview = new Vertex[numPoints];
                    }

                    if (SavingEnabled)
                    {
                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            double yVal = points[pos].Y;
                            float dB = (float)(yVal - BaseAmplification);
                            double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                            ampl = Math.Max(0, ampl);
                            ampl = Math.Min(1, ampl);

                            PlotVertsOverview[pos].PositionRhw.X = (float)Math.Min(SaveParameters.BackBufferWidth, Math.Max(0, (((float)pos / (float)numPoints) * SaveParameters.BackBufferWidth)));
                            PlotVertsOverview[pos].PositionRhw.Y = 0;
                            PlotVertsOverview[pos].PositionRhw.Z = 0.5f;
                            PlotVertsOverview[pos].PositionRhw.W = 1;
                            PlotVertsOverview[pos].Color = (uint)(0xFF000000 | ColorTable.Lookup(ampl));
                        }
                    }

                    /* get density */
                    int density = 0;
                    for (int pos = 0; (pos < numPoints) && (((double)points[pos].X / (double)numPoints) * DirectXWidth * XZoomFactor < 1); pos++)
                    {
                        density++;
                    }

                    /* calculate average on high density */
                    if (density > 1)
                    {
                        int newNumPoints = (int)(((double)points[numPoints - 1].X / (double)numPoints) * DirectXWidth * XZoomFactor);
                        double ratio = (double)numPoints / (double)newNumPoints;

                        int startPos = 0;
                        for (int pos = 0; (pos < numPoints) && (points[pos].X * XZoomFactor < 0); pos++)
                            startPos++;

                        if (newNumPoints != PlotVerts.Length)
                        {
                            Array.Resize(ref PlotVerts, newNumPoints);
                            //PlotVerts = new Vertex[newNumPoints];
                        }

                        PlotVertsEntries = newNumPoints - 1;


                        for (int pos = 0; pos < newNumPoints; pos++)
                        {
                            double maxAmpl = 0;

                            for (int sample = (int)(pos * ratio); sample < (pos + 1) * ratio; sample++)
                            {
                                double yVal = points[startPos + sample].Y;
                                float dB = (float)(yVal - BaseAmplification);
                                double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                ampl = Math.Max(0, ampl);
                                ampl = Math.Min(1, ampl);

                                maxAmpl = Math.Max(ampl, maxAmpl);
                            }

                            PlotVerts[pos].PositionRhw.X = (float)(pos - DisplayXOffset);
                            PlotVerts[pos].PositionRhw.Y = 0;
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = (uint)(0xFF000000 | ColorTable.Lookup(maxAmpl));
                        }
                    }
                    else
                    {
                        if (numPoints != PlotVerts.Length)
                        {
                            Array.Resize(ref PlotVerts, numPoints);
                            //PlotVerts = new Vertex[numPoints];
                        }

                        PlotVertsEntries = numPoints - 1;

                        for (int pos = 0; pos < numPoints; pos++)
                        {
                            double yVal = points[pos].Y;
                            float dB = (float)(yVal - BaseAmplification);
                            double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                            ampl = Math.Max(0, ampl);
                            ampl = Math.Min(1, ampl);

                            double xPos = ((double)points[pos].X / (double)numPoints) * DirectXWidth;
                            PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset);
                            PlotVerts[pos].PositionRhw.Y = 0;
                            PlotVerts[pos].PositionRhw.Z = 0.5f;
                            PlotVerts[pos].PositionRhw.W = 1;
                            PlotVerts[pos].Color = (uint)(0xFF000000 | ColorTable.Lookup(ampl));
                        }
                    }

                }
            }
            catch (Exception e)
            {
                return;
            }
        }


        protected Vector3 OldImageCenter = Vector3.Zero;
        protected Vector3 OldImagePos = new Vector3(0, 1, 0);
        protected Color4 ColorWhite = new Color4(Color.White);

        protected override void RenderCore()
        {
            bool drawTimeStamp = false;

            if (LinePointsUpdated)
            {
                LinePointsUpdated = false;
                lock (LinePointsLock)
                {
                    CreateVertexBufferForPoints(LinePoints, LinePointEntries);
                }
            }

            if (PlotVertsEntries > 0)
            {
                /* render into temporary buffer */
                Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));

                /* clear temp buffer */
                Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
                Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                /* move the old image when XOffset has changed */
                float delta = 0;
                lock (DisplayXOffsetLock)
                {
                    delta = (float)(DisplayXOffsetPrev - DisplayXOffset);
                    DisplayXOffsetPrev = DisplayXOffset;
                }

                /* draw the old waterfall image */
                Sprite.Begin(SpriteFlags.None);
                if (delta == 0)
                {
                    Sprite.Draw(WaterfallTexture, OldImageCenter, OldImagePos, ColorWhite);
                }
                else
                {
                    Sprite.Draw(WaterfallTexture, OldImageCenter, new Vector3(delta, 1, 0), ColorWhite);
                }
                Sprite.End();

                /* paint first line */
                if (PlotVertsEntries > 0)
                    Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
                PlotVertsEntries = 0;

                LinesWithoutTimestamp++;
                /* draw a new timestamp */
                DateTime newTimeStamp = DateTime.Now;
                if (DrawTimeStamps && newTimeStamp.Subtract(TimeStamp).TotalMilliseconds > TimeStampEveryMiliseconds && LinesWithoutTimestamp > LinesWithoutTimestampMin)
                {
                    drawTimeStamp = true;
                    TimeStamp = newTimeStamp;
                    LinesWithoutTimestamp = 0;

                    Vertex[] lineVertexes = new Vertex[2];
                    lineVertexes[0].PositionRhw.X = 0;
                    lineVertexes[0].PositionRhw.Y = 0;
                    lineVertexes[0].PositionRhw.Z = 0.5f;
                    lineVertexes[0].PositionRhw.W = 1;
                    lineVertexes[0].Color = (uint)ColorCursor.ToArgb();
                    lineVertexes[1].PositionRhw.X = 20;
                    lineVertexes[1].PositionRhw.Y = 0;
                    lineVertexes[1].PositionRhw.Z = 0.5f;
                    lineVertexes[1].PositionRhw.W = 1;
                    lineVertexes[1].Color = (uint)ColorCursor.ToArgb();

                    Device.DrawUserPrimitives(PrimitiveType.LineList, 1, lineVertexes);
                    FixedFont.DrawString(null, TimeStamp.ToString(), 5, 1, (int)(ColorCursor.ToArgb()));
                }

                /* now write the temp buffer into the real image buffer */
                Device.SetRenderTarget(0, WaterfallTexture.GetSurfaceLevel(0));


                Sprite.Begin(SpriteFlags.None);
                Sprite.Draw(TempWaterfallTexture, ColorWhite);
                Sprite.End();


                /* save the view into a file */
                if (SavingEnabled || SaveWaterfallLinesRendered > 0)
                {
                    SaveDeviceDisplayContext.BeginScene();

                    if (SavingEnabled)
                    {
                        /* render into temporary buffer */
                        SaveDeviceDisplayContext.SetRenderTarget(0, SaveTempWaterfallTexture.GetSurfaceLevel(0));

                        /* clear temp buffer first */
                        SaveDeviceDisplayContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
                        SaveDeviceDisplayContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                        /* draw the old waterfall image */
                        SaveSprite.Begin(SpriteFlags.None);
                        SaveSprite.Draw(SaveWaterfallTexture, Vector3.Zero, new Vector3(0, 1, 0), new Color4(Color.White));
                        SaveSprite.End();

                        /* finally paint first line */
                        if (PlotVertsOverview.Length - 1 > 0)
                            SaveDeviceDisplayContext.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsOverview.Length - 1, PlotVertsOverview);

                        /* here also draw timestamps */
                        if (drawTimeStamp)
                        {
                            Vertex[] lineVertexes = new Vertex[2];
                            lineVertexes[0].PositionRhw.X = 0;
                            lineVertexes[0].PositionRhw.Y = 0;
                            lineVertexes[0].PositionRhw.Z = 0.5f;
                            lineVertexes[0].PositionRhw.W = 1;
                            lineVertexes[0].Color = (uint)ColorCursor.ToArgb();
                            lineVertexes[1].PositionRhw.X = 20;
                            lineVertexes[1].PositionRhw.Y = 0;
                            lineVertexes[1].PositionRhw.Z = 0.5f;
                            lineVertexes[1].PositionRhw.W = 1;
                            lineVertexes[1].Color = (uint)ColorCursor.ToArgb();

                            SaveDeviceDisplayContext.DrawUserPrimitives(PrimitiveType.LineList, 1, lineVertexes);

                            SaveFixedFont.DrawString(null, TimeStamp.ToString(), 5, 1, (int)(ColorCursor.ToArgb()));
                        }

                        /* now write the temp buffer into the real image buffer */
                        SaveDeviceDisplayContext.SetRenderTarget(0, SaveWaterfallTexture.GetSurfaceLevel(0));

                        SaveSprite.Begin(SpriteFlags.None);
                        SaveSprite.Draw(SaveTempWaterfallTexture, new Color4(Color.White));
                        SaveSprite.End();

                        SaveWaterfallLinesRendered++;
                    }

                    if (!SavingEnabled || (SaveWaterfallLinesRendered >= SaveWaterfallLinesToRender))
                    {
                        /* backup the current image */
                        if (SaveImageDisplayContextsFree.Count > 0)
                        {
                            TextureInfo front = SaveImageDisplayContextsFree.Dequeue();

                            front.lines = SaveWaterfallLinesRendered;
                            SaveDeviceDisplayContext.SetRenderTarget(0, front.texture.GetSurfaceLevel(0));

                            SaveSprite.Begin(SpriteFlags.None);
                            SaveSprite.Draw(SaveWaterfallTexture, new Color4(Color.White));
                            SaveSprite.End();

                            SaveImageDisplayContextsFilled.Enqueue(front);

                            lock (SaveBufferTrigger)
                            {
                                Monitor.Pulse(SaveBufferTrigger);
                            }
                        }
                        else
                        {
                            SavingEnabled = false;
                            SaveFixedFont.DrawString(null, "Saving is too slow", 5, 1, (int)(ColorCursor.ToArgb()));
                        }
                        
                        SaveWaterfallLinesRendered = 0;
                    }
                    SaveDeviceDisplayContext.EndScene();
                }
            }

            #region render axis and overlay
            Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

            RenderAxis();

            DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
            DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);

            RenderOverlay();
            RenderCursor();
            #endregion


            #region draw waterfall + overlay
            Device.SetRenderTarget(0, DefaultRenderTarget);
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

            Sprite.Begin(SpriteFlags.AlphaBlend);
            Sprite.Draw(WaterfallTexture, Color.White);
            Sprite.Draw(TempWaterfallTexture, Color.White);
            Sprite.End();
            #endregion

        }
    }
}