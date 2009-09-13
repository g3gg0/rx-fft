using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using LibRXFFT.Libraries;
using LibRXFFT.Libraries.FFTW;
using Timer = System.Timers.Timer;
using SlimDX.Direct3D9;
using SlimDX;
using Font = SlimDX.Direct3D9.Font;
using System.Runtime.InteropServices;
using LibRXFFT.Libraries.SignalProcessing;
using System.IO;

namespace LibRXFFT.Components.DirectX
{
    public partial class DirectXWaterfallDisplay : DirectXFFTDisplay
    {

        /* DirectX related graphic stuff */
        protected Sprite Sprite;
        protected Surface DefaultRenderTarget;
        protected Texture WaterfallTexture;
        protected Texture TempWaterfallTexture;
        protected Texture SaveImageDisplayContext;
        protected Texture SaveWaterfallTexture;
        protected Texture SaveTempWaterfallTexture;
        protected Texture SaveImageThreadContext;
        protected Color ColorFaderBG = Color.Orange;

        protected double DisplayXOffsetPrev = 0;
        protected Object DisplayXOffsetLock = new Object();
        protected bool ResetScaleBar = true;

        /* timestamp related */
        protected DateTime TimeStamp = DateTime.Now;
        protected int LinesWithoutTimestamp = 0;
        protected int LinesWithoutTimestampMin = 20;
        public double TimeStampEveryMiliseconds = 1000;

        /* waterfall saving related */
        public string SavingName = "waterfall.png";
        protected bool _SavingEnabled = false;
        public bool SavingEnabled
        {
            get { return _SavingEnabled; }
            set
            {
                _SavingEnabled = value;

                /* notify that its disabled */
                if (!SavingEnabled)
                    SaveBufferTrigger.Release(1);
            }
        }

        protected bool SaveImage;
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

            ActionMouseDragY = eUserAction.None;
            ActionMouseWheelUp = eUserAction.None;
            ActionMouseWheelDown = eUserAction.None;
            ActionMouseWheelUpShift = eUserAction.YOffset;
            ActionMouseWheelDownShift = eUserAction.YOffset;
            ActionMouseWheelUpControl = eUserAction.YZoomIn;
            ActionMouseWheelDownControl = eUserAction.YZoomOut;

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
                while (true)
                {
                    /* wait until we should do something */
                    SaveBufferTrigger.WaitOne();

                    /* clear old file when got disabled */
                    if (!SavingEnabled)
                    {
                        if (SavedImage != null)
                            SavedImage.Dispose();
                        SavedImage = null;
                    }

                    /* if there is a new texture to save */
                    if (SaveImage)
                    {
                        SaveImage = false;

                        try
                        {
                            /* build an image from the texture */
                            SaveImageLock.WaitOne();
                            DataStream saveImageStream = Texture.ToStream(SaveImageThreadContext, ImageFileFormat.Png);
                            SaveImageLock.ReleaseMutex();

                            Image curImage = Image.FromStream(saveImageStream);


                            /* and create a new image with the new size */
                            Image newImage;
                            if (SavedImage != null && SavedImage.Width == curImage.Width)
                                newImage = new Bitmap(curImage.Width, SavedImage.Height + curImage.Height / 2);
                            else
                                newImage = new Bitmap(curImage.Width, curImage.Height / 2);

                            /* draw last image and the current texture into the new image */
                            Graphics g = Graphics.FromImage(newImage);
                            g.DrawImage(curImage, 0, -(curImage.Height / 2));
                            if (SavedImage != null)
                                g.DrawImage(SavedImage, 0, curImage.Height / 2);

                            /* and save it to disk */
                            try
                            {
                                newImage.Save(SavingName, System.Drawing.Imaging.ImageFormat.Png);
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
                            SavingName = "_" + SavingName;
                            if (SavedImage != null)
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


        internal virtual void RenderOverlay()
        {
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
                ScaleVertexesUsed = 0;

                /* center line */
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x80FFFFFF;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + 10;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + 10;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barBottom - 10;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barBottom - 10;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 20;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barBottom;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x80FFFFFF;
                ScaleVertexesUsed++;

                /* left line */
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 19;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x00FFFFFF;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 19;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 19;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 19;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barBottom;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x00FFFFFF;
                ScaleVertexesUsed++;

                /* right line */
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 21;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x00FFFFFF;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 21;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 21;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = (barTop + barBottom) / 2;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 21;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barBottom;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = color & 0x00FFFFFF;
                ScaleVertexesUsed++;

                /* draw white/black limiter */

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 15;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + whiteYPos;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = colorBarUpper;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 25;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + whiteYPos;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = colorBarUpper;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 15;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + blackYPos;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 1;
                ScaleVertexes[ScaleVertexesUsed].Color = colorBarLower;
                ScaleVertexesUsed++;

                ScaleVertexes[ScaleVertexesUsed].PositionRhw.X = 25;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Y = barTop + blackYPos;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.Z = 0.5f;
                ScaleVertexes[ScaleVertexesUsed].PositionRhw.W = 2;
                ScaleVertexes[ScaleVertexesUsed].Color = colorBarLower;
                ScaleVertexesUsed++;
            }

            Device.DrawUserPrimitives(PrimitiveType.LineList, ScaleVertexesUsed / 2, ScaleVertexes);
            SmallFont.DrawString(null, LeveldBWhite + " dB", 27, barTop + whiteYPos - 6, (int)colorBarUpper);
            SmallFont.DrawString(null, LeveldBBlack + " dB", 27, barTop + blackYPos - 6, (int)colorBarLower);


            /* draw vertical cursor line */
            color = (uint)ColorCursor.ToArgb();
            float stubLength = (float)DirectXHeight / 10.0f;
            float xPos = (float)LastMousePos.X;
            float yPos = (float)LastMousePos.Y;

            CursorVertexesVert[0].PositionRhw.X = xPos;
            CursorVertexesVert[1].PositionRhw.X = xPos;
            CursorVertexesVert[2].PositionRhw.X = xPos;
            CursorVertexesVert[3].PositionRhw.X = xPos;

            /* horizontal line */
            CursorVertexesHor[0].PositionRhw.X = xPos - 30;
            CursorVertexesHor[0].PositionRhw.Y = yPos;

            CursorVertexesHor[1].PositionRhw.X = xPos;
            CursorVertexesHor[1].PositionRhw.Y = yPos;

            CursorVertexesHor[2].PositionRhw.X = xPos + 30;
            CursorVertexesHor[2].PositionRhw.Y = yPos;

            /* if axis information changed, recalc lines (this is needed just once btw.) */
            if (UpdateOverlays)
            {
                CursorVertexesVert[0].PositionRhw.Y = 0;
                CursorVertexesVert[0].PositionRhw.Z = 0.5f;
                CursorVertexesVert[0].PositionRhw.W = 1;
                CursorVertexesVert[0].Color = 0x00FF3030;

                CursorVertexesVert[1].PositionRhw.Y = stubLength;
                CursorVertexesVert[1].PositionRhw.Z = 0.5f;
                CursorVertexesVert[1].PositionRhw.W = 1;
                CursorVertexesVert[1].Color = 0xFFFF3030;

                CursorVertexesVert[2].PositionRhw.Y = DirectXHeight - stubLength;
                CursorVertexesVert[2].PositionRhw.Z = 0.5f;
                CursorVertexesVert[2].PositionRhw.W = 1;
                CursorVertexesVert[2].Color = 0xFFFF3030;

                CursorVertexesVert[3].PositionRhw.Y = DirectXHeight;
                CursorVertexesVert[3].PositionRhw.Z = 0.5f;
                CursorVertexesVert[3].PositionRhw.W = 1;
                CursorVertexesVert[3].Color = 0x00FF3030;

                CursorVertexesHor[0].PositionRhw.Z = 0.5f;
                CursorVertexesHor[0].PositionRhw.W = 1;
                CursorVertexesHor[0].Color = color & 0x00FFFFFF;

                CursorVertexesHor[1].PositionRhw.Z = 0.5f;
                CursorVertexesHor[1].PositionRhw.W = 1;
                CursorVertexesHor[1].Color = color;

                CursorVertexesHor[2].PositionRhw.Z = 0.5f;
                CursorVertexesHor[2].PositionRhw.W = 1;
                CursorVertexesHor[2].Color = color & 0x00FFFFFF;
            }



            if (MouseHovering)
            {
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, 2, CursorVertexesHor);
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, 3, CursorVertexesVert);

                /* draw the horizontal position (preliminary) */
                string label = XLabelFromCursorPos(xPos);

                if (xPos > DirectXWidth / 2)
                    SmallFont.DrawString(null, label, (int)xPos - 40, (int)yPos, ColorFG.ToArgb());
                else
                    SmallFont.DrawString(null, label, (int)xPos + 20, (int)yPos, ColorFG.ToArgb());
            }
        }

        internal override void PrepareLinePoints()
        {
            bool resetAverage = !LinePointsUpdated;

            lock (SampleValues)
            {
                if (SampleValuesAveraged > 0)
                {
                    int samples = SampleValues.Length;

                    lock (LinePointsLock)
                    {
                        if (LinePoints == null || LinePoints.Length < samples)
                            LinePoints = new Point[samples];

                        for (int pos = 0; pos < samples; pos++)
                        {
                            double sampleValue = (double)SampleValues[pos];
                            double posX = pos;
                            double posY = sampleValue;

                            LinePoints[pos].X = posX;

                            /* some simple averaging */
                            unchecked
                            {
                                LinePoints[pos].Y *= (Averaging - 1);
                                LinePoints[pos].Y += posY / SampleValuesAveraged;
                                LinePoints[pos].Y /= Averaging;
                            }

                            if (double.IsNaN(LinePoints[pos].Y))
                                LinePoints[pos].Y = 0;
                        }
                        resetAverage = false;
                        LinePointEntries = samples;
                        LinePointsUpdated = true;
                    }
                    SampleValuesAveraged = 0;
                    EnoughData = false;
                }
            }
        }


        protected override void AllocateDevices()
        {
            base.AllocateDevices();

            /* we dont need to allocate that all the time. once is enough */
            if (SaveParameters == null)
            {
                SaveParameters = new PresentParameters();
                SaveParameters.BackBufferHeight = SaveImageBlockSize;
                SaveParameters.BackBufferWidth = Math.Min(FFTSize, 8192);
                SaveParameters.DeviceWindowHandle = Handle;
                SaveParameters.BackBufferFormat = Format.A8R8G8B8;
                SaveParameters.Multisample = MultisampleType.TwoSamples;
            }

            SaveDeviceThreadContext = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, SaveParameters);
            SaveDeviceThreadContext.SetRenderState(RenderState.AlphaBlendEnable, true);
            SaveDeviceThreadContext.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            SaveDeviceThreadContext.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

            SaveDeviceDisplayContext = new Device(Direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, SaveParameters);
            SaveDeviceDisplayContext.SetRenderState(RenderState.AlphaBlendEnable, true);
            SaveDeviceDisplayContext.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            SaveDeviceDisplayContext.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);

            SaveDeviceThreadContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            SaveDeviceDisplayContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
        }

        protected override void ResetDevices()
        {
            base.ResetDevices();

            SaveParameters.BackBufferHeight = SaveImageBlockSize;
            SaveParameters.BackBufferWidth = Math.Min(FFTSize, 8192);
            SaveDeviceDisplayContext.Reset(SaveParameters);
        }

        protected override void ReleaseDevices()
        {
            base.ReleaseDevices();

            if (SaveDeviceDisplayContext!=null)
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

            SmallFont = new Font(Device, new System.Drawing.Font("Arial", 8));

            WaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            TempWaterfallTexture = new Texture(Device, PresentParameters.BackBufferWidth, PresentParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            Sprite = new Sprite(Device);

            /* save file every screen roll-over */
            SaveWaterfallLinesToRender = SaveParameters.BackBufferHeight;
            SaveWaterfallLinesRendered = 0;

            SaveImageThreadContext = new Texture(SaveDeviceThreadContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            SaveWaterfallTexture = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            SaveTempWaterfallTexture = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            SaveImageDisplayContext = new Texture(SaveDeviceDisplayContext, SaveParameters.BackBufferWidth, SaveParameters.BackBufferHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
            
            SaveFixedFont = new Font(SaveDeviceDisplayContext, new System.Drawing.Font("Courier New", 8));
            SaveSprite = new Sprite(SaveDeviceDisplayContext);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();

            if (SaveTempWaterfallTexture != null)
                SaveTempWaterfallTexture.Dispose();
            if (SaveWaterfallTexture != null)
                SaveWaterfallTexture.Dispose();
            SaveTempWaterfallTexture = null;
            SaveWaterfallTexture = null;

            SaveImageLock.WaitOne();

            if (SaveImageDisplayContext != null)
                SaveImageDisplayContext.Dispose();
            if (SaveImageThreadContext != null)
                SaveImageThreadContext.Dispose();
            SaveImageDisplayContext = null;
            SaveImageThreadContext = null;

            SaveImageLock.ReleaseMutex();


            if (Sprite != null)
                Sprite.Dispose();
            if (SaveSprite != null)
                SaveSprite.Dispose();
            Sprite = null;
            SaveSprite = null;

            if (TempWaterfallTexture != null)
                TempWaterfallTexture.Dispose();
            if (WaterfallTexture != null)
                WaterfallTexture.Dispose();
            WaterfallTexture = null;
            TempWaterfallTexture = null;

            if (SaveFixedFont != null)
                SaveFixedFont.Dispose();
            SaveFixedFont = null;


            if (DefaultRenderTarget != null)
                DefaultRenderTarget.Dispose();
            DefaultRenderTarget = null;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            XMaximum = DirectXWidth;
        }

        protected override void KeyPressed(Keys key)
        {
            /* set main text and update overlays so the active bar gets colored */
            if (key == Keys.Shift)
            {
                MainTextPrev = MainText;
                MainText = "Change Upper Limit (White)";
                UpdateOverlays = true;
            }

            if (key == Keys.Control)
            {
                MainTextPrev = MainText;
                MainText = "Change Lower Limit (Black)";
                UpdateOverlays = true;
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
            }

            if (key == Keys.Control)
            {
                MainText = MainTextPrev;
                MainTextPrev = "";
                UpdateOverlays = true;
            }
        }

        public override void ProcessUserAction(eUserAction action, double param)
        {
            switch (action)
            {
                case eUserAction.YOffset:
                    LeveldBWhite = Math.Max(LeveldBBlack, Math.Min(0, LeveldBWhite + 2 * Math.Sign(param)));
                    UpdateAxis = true;
                    break;

                case eUserAction.YZoomIn:
                    LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack + 2));
                    UpdateAxis = true;
                    break;

                case eUserAction.YZoomOut:
                    LeveldBBlack = Math.Max(LeveldBMax, Math.Min(LeveldBWhite, LeveldBBlack - 2));
                    UpdateAxis = true;
                    break;

                default:
                    /* in any other case lock the DisplayXOffset variable */
                    lock (DisplayXOffsetLock)
                    {
                        base.ProcessUserAction(action, param);
                    }
                    break;
            }
        }


        protected override void CreateVertexBufferForPoints(Point[] points, int numPoints)
        {
            if (points == null)
                return;

            try
            {
                DirectXLock.WaitOne();

                if (Device != null)
                {
                    if (numPoints > 0)
                    {
                        if (numPoints != PlotVertsOverview.Length)
                            PlotVertsOverview = new Vertex[numPoints];

                        if (SavingEnabled)
                        {
                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                double yVal = points[pos].Y;
                                float dB = (float)(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal));
                                double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                ampl = Math.Max(0, ampl);
                                ampl = Math.Min(1, ampl);

                                int colorCode = 0;
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.R * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.G * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.B * ampl);

                                PlotVertsOverview[pos].PositionRhw.X = (float)(((float)pos / (float)numPoints) * SaveParameters.BackBufferWidth);
                                PlotVertsOverview[pos].PositionRhw.Y = 0;
                                PlotVertsOverview[pos].PositionRhw.Z = 0.5f;
                                PlotVertsOverview[pos].PositionRhw.W = 1;
                                PlotVertsOverview[pos].Color = (uint)(0xFF000000 | colorCode);
                            }
                        }

                        /* get density */
                        int density = 0;
                        for (int pos = 0; (pos < numPoints) && (((double)points[pos].X / (double)numPoints) * DirectXWidth * XZoomFactor < 1); pos++)
                            density++;

                        /* calculate average on high density */
                        if (density > 1)
                        {
                            int newNumPoints = (int)(((double)points[numPoints - 1].X / (double)numPoints) * DirectXWidth * XZoomFactor);
                            double ratio = (double)numPoints / (double)newNumPoints;

                            int startPos = 0;
                            for (int pos = 0; (pos < numPoints) && (points[pos].X * XZoomFactor < 0); pos++)
                                startPos++;

                            if (newNumPoints != PlotVerts.Length)
                                PlotVerts = new Vertex[newNumPoints];

                            PlotVertsEntries = newNumPoints - 1;


                            for (int pos = 0; pos < newNumPoints; pos++)
                            {
                                double maxAmpl = 0;

                                for (int sample = (int)(pos * ratio); sample < (pos + 1) * ratio; sample++)
                                {
                                    double yVal = points[startPos + sample].Y;
                                    float dB = (float)(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal));
                                    double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                    ampl = Math.Max(0, ampl);
                                    ampl = Math.Min(1, ampl);

                                    maxAmpl = Math.Max(ampl, maxAmpl);
                                }

                                int colorCode = 0;
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.R * maxAmpl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.G * maxAmpl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.B * maxAmpl);

                                PlotVerts[pos].PositionRhw.X = (float)(pos - DisplayXOffset);
                                PlotVerts[pos].PositionRhw.Y = 0;
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = (uint)(0xFF000000 | colorCode);
                            }
                        }
                        else
                        {
                            if (numPoints != PlotVerts.Length)
                                PlotVerts = new Vertex[numPoints];

                            PlotVertsEntries = numPoints - 1;

                            for (int pos = 0; pos < numPoints; pos++)
                            {
                                double yVal = points[pos].Y;
                                float dB = (float)(SquaredFFTData ? DBTools.SquaredSampleTodB(yVal) : DBTools.SampleTodB(yVal));
                                double ampl = 1 - ((dB - LeveldBWhite) / (LeveldBBlack - LeveldBWhite));

                                ampl = Math.Max(0, ampl);
                                ampl = Math.Min(1, ampl);

                                int colorCode = 0;
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.R * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.G * ampl);
                                colorCode <<= 8;
                                colorCode |= (int)(ColorFG.B * ampl);

                                double xPos = ((double)points[pos].X / (double)numPoints) * DirectXWidth;
                                PlotVerts[pos].PositionRhw.X = (float)((XAxisSampleOffset + xPos) * XZoomFactor - DisplayXOffset);
                                PlotVerts[pos].PositionRhw.Y = 0;
                                PlotVerts[pos].PositionRhw.Z = 0.5f;
                                PlotVerts[pos].PositionRhw.W = 1;
                                PlotVerts[pos].Color = (uint)(0xFF000000 | colorCode);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return;
            }
            finally
            {
                DirectXLock.ReleaseMutex();
            }
        }

        protected override void RenderCore()
        {
            if (UpdateAxis)
            {
                UpdateAxis = false;
                UpdateOverlays = true;
                CreateVertexBufferForAxis();
            }

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
                Device.BeginScene();

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
                Sprite.Draw(WaterfallTexture, Vector3.Zero, new Vector3(delta, 1, 0), new Color4(Color.White));
                Sprite.End();

                /* paint first line */
                Device.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsEntries, PlotVerts);
                PlotVertsEntries = 0;

                bool drawTimeStamp = false;

                LinesWithoutTimestamp++;
                /* draw a new timestamp */
                DateTime newTimeStamp = DateTime.Now;
                if (newTimeStamp.Subtract(TimeStamp).TotalMilliseconds > TimeStampEveryMiliseconds && LinesWithoutTimestamp > LinesWithoutTimestampMin)
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
                Sprite.Draw(TempWaterfallTexture, new Color4(Color.White));
                Sprite.End();

                Device.EndScene();

                /* save the view into a file */
                if (SavingEnabled)
                {
                    SaveDeviceDisplayContext.BeginScene();

                    /* render into temporary buffer */
                    SaveDeviceDisplayContext.SetRenderTarget(0, SaveTempWaterfallTexture.GetSurfaceLevel(0));

                    /* clear temp buffer */
                    SaveDeviceDisplayContext.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);
                    SaveDeviceDisplayContext.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;

                    /* draw the old waterfall image */
                    SaveSprite.Begin(SpriteFlags.None);
                    SaveSprite.Draw(SaveWaterfallTexture, Vector3.Zero, new Vector3(0, 1, 0), new Color4(Color.White));
                    SaveSprite.End();
                    
                    /* paint first line */
                    SaveDeviceDisplayContext.DrawUserPrimitives(PrimitiveType.LineStrip, PlotVertsOverview.Length - 1, PlotVertsOverview);

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


                    if (++SaveWaterfallLinesRendered >= SaveWaterfallLinesToRender)
                    {
                        /* backup the current image */
                        SaveDeviceDisplayContext.SetRenderTarget(0, SaveImageDisplayContext.GetSurfaceLevel(0));

                        SaveSprite.Begin(SpriteFlags.None);
                        SaveSprite.Draw(SaveWaterfallTexture, new Color4(Color.White));
                        SaveSprite.End();

                        /* get image into save thread device context */
                        SaveImageLock.WaitOne();
                        Surface.FromSurface(SaveImageThreadContext.GetSurfaceLevel(0), SaveImageDisplayContext.GetSurfaceLevel(0), Filter.None, 0);
                        SaveImageLock.ReleaseMutex();

                        SaveWaterfallLinesRendered = SaveWaterfallLinesToRender / 2 + 1;

                        try
                        {
                            SaveImage = true;
                            SaveBufferTrigger.Release(1);
                        }
                        catch (SemaphoreFullException e)
                        {
                        }
                    }

                    SaveDeviceDisplayContext.EndScene();
                }
            }

            #region build overlay 
            Device.SetRenderTarget(0, TempWaterfallTexture.GetSurfaceLevel(0));
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1.0f, 0);

            Device.BeginScene();

            if (XAxisVerts.Length > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, XAxisVerts.Length / 2, XAxisVerts);
            if (YAxisVerts.Length > 0)
                Device.DrawUserPrimitives(PrimitiveType.LineList, YAxisVerts.Length / 2, YAxisVerts);

            DisplayFont.DrawString(null, MainText, 20, 30, ColorBG);
            DisplayFont.DrawString(null, MainText, 21, 31, ColorFont);

            RenderOverlay();
            Device.EndScene();
            #endregion


            #region draw waterfall + overlay
            Device.SetRenderTarget(0, DefaultRenderTarget);
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ColorBG, 1.0f, 0);

            Device.BeginScene();

            Sprite.Begin(SpriteFlags.AlphaBlend);
            Sprite.Draw(WaterfallTexture, Color.White);
            Sprite.Draw(TempWaterfallTexture, Color.White);
            Sprite.End();
            Device.EndScene();
            #endregion

            Device.Present();

            UpdateOverlays = false;
        }
    }
}