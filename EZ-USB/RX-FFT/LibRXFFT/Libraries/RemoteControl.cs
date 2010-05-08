using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LibRXFFT.Libraries.USB_RX.Tuners;
using LibRXFFT.Libraries.SignalProcessing;

namespace LibRXFFT.Libraries
{
    public class RemoteControl
    {
        private List<Thread> ClientThreads = new List<Thread>();
        private List<Socket> OpenSockets = new List<Socket>();
        private Socket ListenSocket;
        private bool Exiting = false;

        public ushort Port = 9999;
        public DigitalTuner Tuner;
        public int FFTSize = 32;

        private byte[] ClientBuffer;
        private Socket Client;

        public event EventHandler FFTSizeChanged;

        public RemoteControl()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ListenSocket.Bind(ip);
            ListenSocket.Listen(10);

            Console.WriteLine("Waiting for a client...");
            try
            {
                SocketAsyncEventArgs socketArgs = new SocketAsyncEventArgs();
                socketArgs.Completed += new EventHandler<SocketAsyncEventArgs>(socketArgs_Completed);
                ListenSocket.AcceptAsync(socketArgs);
            }
            catch (SocketException e)
            {
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception: " + e);
            }
        }

        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            OpenSockets.Add(s);

            IPEndPoint clientep = (IPEndPoint)s.RemoteEndPoint;
            Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);

            HandleClient(s);
        }

        public void Abort()
        {
            try
            {
                Exiting = true;
                lock (ClientThreads)
                {
                    foreach (Thread t in ClientThreads)
                    {
                        t.Abort();
                    }

                    ClientThreads.Clear();
                }
                lock (OpenSockets)
                {
                    foreach (Socket s in OpenSockets)
                    {
                        s.Shutdown(SocketShutdown.Send);
                        s.Close();
                    }

                    OpenSockets.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception " + e);
            }
        }


        private uint ReceiveFrame(Socket s)
        {
            byte[] remoteFrame = new byte[4];
            uint remoteData = 0;
            int read = s.Receive(remoteFrame);

            if (read == 4)
            {
                remoteData |= remoteFrame[0];
                remoteData <<= 8;
                remoteData |= remoteFrame[1];
                remoteData <<= 8;
                remoteData |= remoteFrame[2];
                remoteData <<= 8;
                remoteData |= remoteFrame[3];
            }
            return remoteData;
        }

        private void HandleClient(Socket s)
        {
            Thread clientThread = new Thread(() =>
            {
                long StartFreq = 0;
                bool Dragging = false;

                try
                {
                    while (!Exiting)
                    {
                        uint command = ReceiveFrame(s);

                        switch (command)
                        {
                            case 0xFF0000AA:
                                uint fftSize = ReceiveFrame(s);
                                Console.WriteLine("Remote: FFT Size {0}", fftSize);
                                FFTSize = (int)fftSize;
                                ClientBuffer = new byte[fftSize];
                                Client = s;

                                if (FFTSizeChanged != null)
                                {
                                    FFTSizeChanged(this, null);
                                }
                                break;

                            case 0xFF0000BC:
                            case 0xFF0000BB:
                                int drag = (int)ReceiveFrame(s);
                                if (!Dragging)
                                {
                                    StartFreq = Tuner.GetFrequency();
                                    Dragging = true;
                                }

                                double offset = (double)drag / FFTSize;
                                long freq = (long)(StartFreq + Tuner.SamplingRate * offset);

                                Tuner.SetFrequency(freq);

                                if (command == 0xFF0000BC)
                                {
                                    Dragging = false;
                                }
                                break;

                            case 0:
                                return;
                        }
                        /*
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            FFTSize = remoteFFTSize;
                        }));

                        */
                    }
                }
                catch (ThreadAbortException e)
                {
                }
                catch (Exception e)
                {
                    Client = null;
                    Console.WriteLine("Exception: " + e);
                }

                lock (OpenSockets)
                {
                    OpenSockets.Remove(s);
                }

            });

            lock (ClientThreads)
            {
                ClientThreads.Add(clientThread);
            }
            clientThread.Start();
        }

        internal void ProcessData(double[] FFTResult)
        {
            if (Client != null)
            {
                try
                {
                    double scale = (double)FFTResult.Length / (double)ClientBuffer.Length;

                    for (int pos = 0; pos < ClientBuffer.Length; pos++)
                    {
                        int dataPos = Math.Min(FFTResult.Length - 1, (int)(pos * scale));
                        ClientBuffer[pos] = (byte)(Math.Min(0xFF, Math.Max(0, DBTools.SquaredSampleTodB(FFTResult[dataPos]) * 2 + 255)));
                    }

                    Client.Send(ClientBuffer);
                }
                catch (Exception e)
                {
                    Client.Close();
                    Client = null;
                }
            }
        }
    }
}
