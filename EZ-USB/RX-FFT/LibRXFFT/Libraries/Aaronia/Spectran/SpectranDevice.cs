
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static LibRXFFT.Libraries.Aaronia.Spectran.RTSAAPI;

namespace LibRXFFT.Libraries.Aaronia.Spectran
{
    public class SpectranDevice
    {
        private AARTSAAPI_Handle Handle = new AARTSAAPI_Handle(true);
        private AARTSAAPI_Device Device = new AARTSAAPI_Device(true);
        public AARTSAAPI_DeviceInfo DeviceInfo = new AARTSAAPI_DeviceInfo(true);
        private AARTSAAPI_Config ConfigRoot = new AARTSAAPI_Config(true);
        private bool Started;

        public ConfigNode Config
        {
            get
            {
                return new ConfigNode(Device, ConfigRoot);
            }
        }

        public class ConfigNode : IEnumerable<ConfigNode>
        {
            public string Name => ConfigInfo.name;
            public string ValueString;
            public string Title => ConfigInfo.title;
            public AARTSAAPI_ConfigType Type => ConfigInfo.type;
            public double MinValue => ConfigInfo.minValue;
            public double MaxValue => ConfigInfo.maxValue;
            public double StepValue => ConfigInfo.stepValue;
            public string Unit => ConfigInfo.unit;
            public string Options => ConfigInfo.options;
            public ulong DisabledOptions => ConfigInfo.disabledOptions;

            public AARTSAAPI_ConfigInfo ConfigInfo;
            private AARTSAAPI_Device Device;
            private AARTSAAPI_Config Config;

            public ConfigNode(AARTSAAPI_Device dev, AARTSAAPI_Config cfg)
            {
                Device = dev;
                Config = cfg;
                ConfigInfo = new AARTSAAPI_ConfigInfo(false);

                AARTSAAPI_ConfigGetInfo(ref Device, ref Config, ref ConfigInfo);
                char[] str = new char[1024];
                long strLen = str.Length;

                AARTSAAPI_ConfigGetString(ref Device, ref Config, str, ref strLen);
                ValueString = new string(str, 0, (int)Math.Min(strLen, str.ToList().IndexOf((char)0)));
            }

            public bool Commit()
            {
                return AARTSAAPI_ConfigSetString(ref Device, ref Config, ValueString) == AARTSAAPI_Result.AARTSAAPI_OK;
            }

            public IEnumerator<ConfigNode> GetEnumerator()
            {
                return new ConfigEnum(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new ConfigEnum(this);
            }

            public class ConfigEnum : IEnumerator<ConfigNode>
            {
                private ConfigNode ConfigNode;
                private AARTSAAPI_Config IteratedConfig;

                private ConfigNode _Current;
                public ConfigNode Current => _Current;

                object IEnumerator.Current => _Current;

                public ConfigEnum(ConfigNode configNode)
                {
                    ConfigNode = configNode;
                    Reset();
                }

                public bool MoveNext()
                {
                    bool ret = AARTSAAPI_ConfigNext(ref ConfigNode.Device, ref ConfigNode.Config, out IteratedConfig) == AARTSAAPI_Result.AARTSAAPI_OK;
                    if(!ret)
                    {
                        return false;
                    }

                    ReadConfigItem();
                    return true;
                }

                public void Reset()
                {
                    AARTSAAPI_ConfigFirst(ref ConfigNode.Device, ref ConfigNode.Config, out IteratedConfig);
                    ReadConfigItem();
                }

                private void ReadConfigItem()
                {
                    _Current = new ConfigNode(ConfigNode.Device, IteratedConfig);
                }

                public void Dispose()
                {
                }
            }
        }

        public enum eReceiverChannel
        {
            Rx1,
            Rx2,
            Rx1PlusRx2,
            Rx1DivRx2,
            RxOff,
            auto
        }

        public enum eReceiverClock
        {
            Clock92MHz,
            Clock122MHz,
            Clock184MHz,
            Clock245MHz
        }

        static SpectranDevice()
        {
            AARTSAAPI_Init(AARTSAAPI_MemoryModel.AARTSAAPI_MEMORY_MEDIUM);
        }

        public SpectranDevice()
        {
        }

        public bool TryOpen(int startIndex = 0)
        {
            if (AARTSAAPI_Open(ref Handle) != AARTSAAPI_Result.AARTSAAPI_OK)
            {
                throw new Exception("Failed to open");
            }
            if (AARTSAAPI_RescanDevices(ref Handle, 2000) != AARTSAAPI_Result.AARTSAAPI_OK)
            {
                throw new Exception("Failed to scan");
            }

            try
            {
                bool enumerated = false;
                for (int retry = 0; retry < 50; retry++)
                {
                    if (AARTSAAPI_EnumDevice(ref Handle, "spectranv6", 0, ref DeviceInfo) != AARTSAAPI_Result.AARTSAAPI_OK)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    enumerated = true;
                    break;
                }
                if (!enumerated)
                {
                    throw new Exception("Failed to enumerate");
                }

                if (AARTSAAPI_OpenDevice(ref Handle, ref Device, "spectranv6/iqtransceiver", DeviceInfo.serialNumber) != AARTSAAPI_Result.AARTSAAPI_OK)
                {
                    throw new Exception("Failed to open");
                }

                if (AARTSAAPI_ConfigRoot(ref Device, out ConfigRoot) != AARTSAAPI_Result.AARTSAAPI_OK)
                {
                    throw new Exception("Failed to get config");
                }
                return true;
            }
            catch (Exception ex)
            {
                AARTSAAPI_Close(ref Handle);
            }

            return false;
        }

        public bool Start()
        {
            if(Started)
            {
                return true;
            }
            var res = AARTSAAPI_ConnectDevice(ref Device);

            if (res != AARTSAAPI_Result.AARTSAAPI_OK)
            {
                return false;
            }

            if (AARTSAAPI_StartDevice(ref Device) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                int retries = 100;                
                while (retries-- > 0 && AARTSAAPI_GetDeviceState(ref Device) != AARTSAAPI_Result.AARTSAAPI_RUNNING)
                {
                    Thread.Sleep(100);
                }

                if(retries > 0)
                {
                    Started = true;
                    return true;
                }
            }
            AARTSAAPI_DisconnectDevice(ref Device);
            return false;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }
            AARTSAAPI_StopDevice(ref Device);
            AARTSAAPI_DisconnectDevice(ref Device);

            Started = false;
        }

        private class TxBuffer
        {
            public double LastEndTime = 0;
            public float[] SampleBuffer = new float[0];
            public GCHandle TransmitBufferHandle;
            public GCHandle PacketHandle;
            public AARTSAAPI_Packet TxPacket = new AARTSAAPI_Packet();
        }

        private TxBuffer[] Buffers = new TxBuffer[2] { new TxBuffer(), new TxBuffer() };
        private int CurrentBuffer = 0;

        public bool SendPacket(float[] sampleBuf, double frequency, double stepFrequency)
        {
            /* first get the current device time */
            AARTSAAPI_GetMasterStreamTime(ref Device, out double currentTime);

            int prev = (CurrentBuffer + 1) % 2;

            /* if all buffer is still transmitting or queued, return */
            if (Buffers[CurrentBuffer].LastEndTime > currentTime)
            {
                return false;
            }

            /* current buffer is the one that w */
            if (Buffers[CurrentBuffer].SampleBuffer.Length < sampleBuf.Length)
            {
                Buffers[CurrentBuffer].SampleBuffer = new float[sampleBuf.Length];
                Buffers[CurrentBuffer].TransmitBufferHandle = GCHandle.Alloc(Buffers[CurrentBuffer].SampleBuffer, GCHandleType.Pinned);
                Buffers[CurrentBuffer].PacketHandle = GCHandle.Alloc(Buffers[CurrentBuffer].TxPacket, GCHandleType.Pinned);
            }

            Array.Copy(sampleBuf, Buffers[CurrentBuffer].SampleBuffer, sampleBuf.Length);

            var pkt = Buffers[CurrentBuffer].TxPacket;

            /* if the previous packet already ended, restart stream */
            if (Buffers[prev].LastEndTime < currentTime)
            {
                /* start in 200ms */
                pkt.startTime = currentTime + 0.2;
                /* and tag as stream start */
                pkt.flags |= AARTSAAPI_PACKET_STREAM_START;
                pkt.flags |= AARTSAAPI_PACKET_SEGMENT_START;

                Console.WriteLine("Start stream");
            }
            else
            {
                /* queue the next packet right after the previous ended */
                pkt.startTime = Buffers[prev].LastEndTime;
                Console.WriteLine("Continue stream");
            }

            /* update packet fields */
            pkt.startFrequency = frequency;
            pkt.endTime = pkt.startTime + sampleBuf.Length / 2 / stepFrequency;
            pkt.stepFrequency = stepFrequency;
            pkt.size = 2;
            pkt.stride = 2;
            pkt.fp32 = Buffers[CurrentBuffer].TransmitBufferHandle.AddrOfPinnedObject();
            pkt.num = sampleBuf.Length / 2;

            Buffers[CurrentBuffer].LastEndTime = pkt.endTime;

            CurrentBuffer = (CurrentBuffer + 1) % 2;

            return AARTSAAPI_SendPacket(ref Device, 0, ref pkt) == AARTSAAPI_Result.AARTSAAPI_OK;
        }

        public int GetPacket(ref float[] samples, int channel = 0, int timeout = 1000)
        {
            AARTSAAPI_Packet packet = new AARTSAAPI_Packet();
            DateTime start = DateTime.Now;

            while (AARTSAAPI_GetPacket(ref Device, channel, 0, ref packet) == AARTSAAPI_Result.AARTSAAPI_EMPTY || packet.num == 0)
            {
                Thread.Sleep(5);
                if ((DateTime.Now - start).TotalMilliseconds > timeout)
                {
                    return 0;
                }
            }

            if (samples == null || samples.Length < packet.num * 2)
            {
                samples = new float[packet.num * 2];
            }
            Marshal.Copy(packet.fp32, samples, 0, (int)(packet.num * 2));

            AARTSAAPI_ConsumePackets(ref Device, channel, 1);

            return (int)(packet.num * 2);
        }

        public int GetPacketRaw(ref byte[] samples, int channel = 0, int timeout = 1000)
        {
            AARTSAAPI_Packet packet = new AARTSAAPI_Packet();
            DateTime start = DateTime.Now;

            while (AARTSAAPI_GetPacket(ref Device, channel, 0, ref packet) == AARTSAAPI_Result.AARTSAAPI_EMPTY || packet.num == 0)
            {
                Thread.Sleep(5);
                if ((DateTime.Now - start).TotalMilliseconds > timeout)
                {
                    return 0;
                }
            }

            if (samples == null || samples.Length < packet.num * 2 * 4)
            {
                samples = new byte[packet.num * 2 * 4];
            }
            Marshal.Copy(packet.fp32, samples, 0, (int)(packet.num * 2 * 4));

            AARTSAAPI_ConsumePackets(ref Device, channel, 1);

            return (int)(packet.num * 2 * 4);
        }

        public void Flush(int channel = 0)
        {
            AARTSAAPI_ConsumePackets(ref Device, channel, 9999);
        }

        public string ReceiverChannel
        {
            get
            {
                return GetConfigString("device/receiverchannel");
            }
            set
            {
                SetConfig("device/receiverchannel", value);
            }
        }

        public string ReceiverClock
        {
            get
            {
                return GetConfigString("device/receiverclock");
            }
            set
            {
                SetConfig("device/receiverclock", value);
            }
        }

        public long Decimation
        {
            get
            {
                return GetConfigInteger("main/decimation");
            }
            set
            {
                SetConfig("main/decimation", value);
            }
        }

        public double ReferenceLevel
        {
            get
            {
                return GetConfigDouble("main/reflevel");
            }
            set
            {
                SetConfig("main/reflevel", value);
            }
        }

        public double CenterFrequency
        {
            get
            {
                return GetConfigDouble("main/centerfreq");
            }
            set
            {
                SetConfig("main/centerfreq", value);
            }
        }

        public double SpanFrequency
        {
            get
            {
                return GetConfigDouble("main/spanfreq");
            }
            set
            {
                SetConfig("main/spanfreq", value);
            }
        }

        public double DemodCenterFrequency
        {
            get
            {
                return GetConfigDouble("main/demodcenterfreq");
            }
            set
            {
                SetConfig("main/demodcenterfreq", value);
            }
        }

        public double DemodSpanFrequency
        {
            get
            {
                return GetConfigDouble("main/demodspanfreq");
            }
            set
            {
                SetConfig("main/demodspanfreq", value);
            }
        }

        public double TransmitterGain
        {
            get
            {
                return GetConfigDouble("main/transgain");
            }
            set
            {
                SetConfig("main/transgain", value);
            }
        }

        private bool SetConfig(string cfg, double value)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                return AARTSAAPI_ConfigSetFloat(ref Device, ref config, value) == AARTSAAPI_Result.AARTSAAPI_OK;
            }

            return false;
        }
        private bool SetConfig(string cfg, long value)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                return AARTSAAPI_ConfigSetInteger(ref Device, ref config, value) == AARTSAAPI_Result.AARTSAAPI_OK;
            }

            return false;
        }
        private bool SetConfig(string cfg, string value)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                return AARTSAAPI_ConfigSetString(ref Device, ref config, value) == AARTSAAPI_Result.AARTSAAPI_OK;
            }

            return false;
        }

        private double GetConfigDouble(string cfg, double defaultValue = 0)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                double ret = 0;
                AARTSAAPI_ConfigGetFloat(ref Device, ref config, out ret);
                return ret;
            }

            return defaultValue;
        }

        private long GetConfigInteger(string cfg, long defaultValue = 0)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                long ret = 0;
                AARTSAAPI_ConfigGetInteger(ref Device, ref config, out ret);
                return ret;
            }

            return defaultValue;
        }

        private string GetConfigString(string cfg, string defaultValue = "")
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                char[] ret = new char[1024];
                long length = ret.Length;
                AARTSAAPI_ConfigGetString(ref Device, ref config, ret, ref length);

                return new string(ret, 0, (int)Math.Min(length,ret.ToList().IndexOf((char)0)));
            }

            return defaultValue;
        }

        private double GetConfig(string cfg, double defaultValue = 0)
        {
            AARTSAAPI_Config config = new AARTSAAPI_Config(true);

            if (AARTSAAPI_ConfigFind(ref Device, ref ConfigRoot, ref config, cfg) == AARTSAAPI_Result.AARTSAAPI_OK)
            {
                double ret = 0;
                AARTSAAPI_ConfigGetFloat(ref Device, ref config, out ret);
                return ret;
            }

            return defaultValue;
        }

        internal void Close()
        {
            AARTSAAPI_Close(ref Handle);
        }
    }
}
