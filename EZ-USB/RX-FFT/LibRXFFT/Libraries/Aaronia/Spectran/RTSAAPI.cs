using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibRXFFT.Libraries.Aaronia.Spectran
{
    public class RTSAAPI
    {
        public const string LibPath = "C:\\Program Files\\Aaronia AG\\Aaronia RTSA-Suite PRO\\AaroniaRTSAAPI.dll";

        public enum AARTSAAPI_Result : uint
        {
            AARTSAAPI_OK = 0x00000000,
            AARTSAAPI_EMPTY = 0x00000001,
            AARTSAAPI_RETRY = 0x00000002,
            AARTSAAPI_IDLE = 0x10000000,
            AARTSAAPI_CONNECTING = 0x10000001,
            AARTSAAPI_CONNECTED = 0x10000002,
            AARTSAAPI_STARTING = 0x10000003,
            AARTSAAPI_RUNNING = 0x10000004,
            AARTSAAPI_STOPPING = 0x10000005,
            AARTSAAPI_DISCONNECTING = 0x10000006,
            AARTSAAPI_WARNING = 0x40000000,
            AARTSAAPI_WARNING_VALUE_ADJUSTED = 0x40000001,
            AARTSAAPI_WARNING_VALUE_DISABLED = 0x40000002,
            AARTSAAPI_ERROR = 0x80000000,
            AARTSAAPI_ERROR_NOT_INITIALIZED = 0x80000001,
            AARTSAAPI_ERROR_NOT_FOUND = 0x80000002,
            AARTSAAPI_ERROR_BUSY = 0x80000003,
            AARTSAAPI_ERROR_NOT_OPEN = 0x80000004,
            AARTSAAPI_ERROR_NOT_CONNECTED = 0x80000005,
            AARTSAAPI_ERROR_INVALID_CONFIG = 0x80000006,
            AARTSAAPI_ERROR_BUFFER_SIZE = 0x80000007,
            AARTSAAPI_ERROR_INVALID_CHANNEL = 0x80000008,
            AARTSAAPI_ERROR_INVALID_PARAMETR = 0x80000009,
            AARTSAAPI_ERROR_INVALID_SIZE = 0x8000000a,
            AARTSAAPI_ERROR_MISSING_PATHS_FILE = 0x8000000b,
            AARTSAAPI_ERROR_VALUE_INVALID = 0x8000000c,
            AARTSAAPI_ERROR_VALUE_MALFORMED = 0x8000000d
        }

        public enum AARTSAAPI_MemoryModel : uint
        {
            AARTSAAPI_MEMORY_SMALL = 0,
            AARTSAAPI_MEMORY_MEDIUM = 1,
            AARTSAAPI_MEMORY_LARGE = 2,
            AARTSAAPI_MEMORY_LUDICROUS = 3
        };

        public const ulong AARTSAAPI_PACKET_STREAM_START = 0x0000000000000001;
        public const ulong AARTSAAPI_PACKET_STREAM_END = 0x0000000000000002;
        public const ulong AARTSAAPI_PACKET_SEGMENT_START = 0x0000000000000004;
        public const ulong AARTSAAPI_PACKET_SEGMENT_END = 0x0000000000000008;


        public struct AARTSAAPI_Handle
        {
            public IntPtr d;

            public AARTSAAPI_Handle(bool dummy)
            {
                d = IntPtr.Zero;
            }
        }

        public struct AARTSAAPI_Device
        {
            public IntPtr d;

            public AARTSAAPI_Device(bool dummy)
            {
                d = IntPtr.Zero;
            }
        }

        public struct AARTSAAPI_Config
        {
            public IntPtr d;

            public AARTSAAPI_Config(bool dummy)
            {
                d = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Data packet
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AARTSAAPI_Packet
        {
            /// <summary>
            /// size of data structure, filled in by caller
            /// </summary>
            public long cbsize;
            public ulong streamID;
            public ulong flags;
            /// <summary>
            /// start time in seconds since start of the unix epoch
            /// </summary>
            public double startTime;
            /// <summary>
            /// end time in seconds since start of the unix epoch
            /// </summary>
            public double endTime;
            /// <summary>
            /// start frequency of the data
            /// </summary>
            public double startFrequency;
            /// <summary>
            /// bin size or sample rate of the data
            /// </summary>
            public double stepFrequency;
            /// <summary>
            /// valid frequency range, center is start + span / 2
            /// </summary>
            public double spanFrequency;
            /// <summary>
            /// realtime bandwidth in spectrum data
            /// </summary>
            public double rbwFrequency;
            /// <summary>
            /// number of samples used in the packet
            /// </summary>
            public long num;
            /// <summary>
            /// total number of samples of the packet
            /// </summary>
            public long total;
            /// <summary>
            /// size of each sample
            /// </summary>
            public long size;
            /// <summary>
            /// offset from sample to sample in floats
            /// </summary>
            public long stride;
            /// <summary>
            /// actual sample data
            /// </summary>
            public IntPtr fp32;

            public AARTSAAPI_Packet(bool dummy = false)
            {
                cbsize = Marshal.SizeOf(typeof(AARTSAAPI_Packet));
                streamID = 0;
                flags = 0;
                startTime = 0;
                endTime = 0;
                startFrequency = 0;
                stepFrequency = 0;
                spanFrequency = 0;
                num = 0;
                total = 0;
                size = 0;
                stride = 0;
                fp32 = IntPtr.Zero;
                rbwFrequency = 0;
            }
        };

        /// <summary>
        /// Device information structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AARTSAAPI_DeviceInfo
        {
            public long cbsize;             /// size of data structure, filled in by caller
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
            public string serialNumber;  /// serial number of the device
            public bool ready;             /// device is ready and booted
            public bool boost;             /// device has a second USB connector
            public bool superspeed;            /// device uses superspeed
            public bool active;                /// device is already in use

            public AARTSAAPI_DeviceInfo(bool dummy)
            {
                cbsize = Marshal.SizeOf(typeof(AARTSAAPI_DeviceInfo));
                serialNumber = "";
                ready = false;
                boost = false;
                superspeed = false;
                active = false;
            }
        };


        /// <summary>
        /// Types of configuration items
        /// </summary>
        public enum AARTSAAPI_ConfigType : uint
        {
            AARTSAAPI_CONFIG_TYPE_OTHER = 0,
            AARTSAAPI_CONFIG_TYPE_GROUP = 1,
            AARTSAAPI_CONFIG_TYPE_BLOB = 2,
            AARTSAAPI_CONFIG_TYPE_NUMBER = 3,
            AARTSAAPI_CONFIG_TYPE_BOOL = 4,
            AARTSAAPI_CONFIG_TYPE_ENUM = 5,
            AARTSAAPI_CONFIG_TYPE_STRING = 6
        };

        /// <summary>
        /// Information for a single configuration item in the configuration tree
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct AARTSAAPI_ConfigInfo
        {
            public long cbsize;                 /// size of data structure, filled in by caller

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string name;               /// internal name of the item
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 120)]
            public string title;             /// human readable name of the item
            public AARTSAAPI_ConfigType type;                  /// data type of the item
            public double minValue;
            public double maxValue;
            public double stepValue;   /// valid value ranges for numeric types
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string unit;               /// physical unit of the data
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
            public string options;          /// semicolon separated list of values in an enum type
            public ulong disabledOptions;       /// bitset of the disabled selections in an enum type

            public AARTSAAPI_ConfigInfo(bool dummy = false)
            {
                cbsize = Marshal.SizeOf(typeof(AARTSAAPI_ConfigInfo));
                name = "";
                title = "";
                type = AARTSAAPI_ConfigType.AARTSAAPI_CONFIG_TYPE_STRING;
                minValue = 0;
                maxValue = 0;
                stepValue = 0;
                disabledOptions = 0;
                unit = "";
                options = "";
            }
        };




        /// <summary>
        /// Initialize the library, should be called onec at application startup
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_Init(AARTSAAPI_MemoryModel memory);

        /// <summary>
        /// Shutdown the library, should be called before application termination
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_Shutdown();

        /// <summary>
        /// Get the version of the library upper 16 bit version, lower 16 bit revision
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern uint AARTSAAPI_Version();


        /// <summary>
        /// Open an API access handle
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_Open(ref AARTSAAPI_Handle handle);

        /// <summary>
        /// Close an API access handle
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_Close(ref AARTSAAPI_Handle handle);

        /// <summary>
        /// Rescan all devices controlled by this library.  This call should be madde
        /// whenever the device list is changed (e.g. indicated by the OS with a device
        /// change message) or the devices are reset.
        //// </summary>
        /// The call should be repeated when it returns AARTSAAPI_RETRY.
        /// </summary>
        /// <returns></returns>
        /// <param name="handle">Handle retrieved by AARTSAAPI_Open</param>
        /// <param name="timeout"></param>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        
        public static extern AARTSAAPI_Result AARTSAAPI_RescanDevices(ref AARTSAAPI_Handle handle, int timeout);

        /// <summary>
        /// Reset all devices controlled by this library, which are currently not in
        /// used.  The AARTSAAPI_RescanDevices should be called to wait for the
        /// reset and boot of the devices to complete.
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ResetDevices(ref AARTSAAPI_Handle handle);

        /// <summary>
        /// Enumerate the devices for a given type.  The list starts with index 0 and will
        /// return AARTSAAPI_EMPTY when the end of the list is reached.
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        
        public static extern AARTSAAPI_Result AARTSAAPI_EnumDevice(ref AARTSAAPI_Handle handle, [MarshalAs(UnmanagedType.LPWStr)] string type, int index, ref AARTSAAPI_DeviceInfo dinfo);

        /// <summary>
        /// Open a device for exclusive use.  This allocates the required data structures
        /// and prepares the configuration settings, but will not access the hardware.
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_OpenDevice(ref AARTSAAPI_Handle handle, ref AARTSAAPI_Device dhandle, [MarshalAs(UnmanagedType.LPWStr)] string type, [MarshalAs(UnmanagedType.LPWStr)] string serialNumber);

        /// <summary>
        /// Close a device
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_CloseDevice(ref AARTSAAPI_Handle handle, ref AARTSAAPI_Device dhandle);

        /// <summary>
        /// Connect to the pysical device
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConnectDevice(ref AARTSAAPI_Device dhandle);

        /// <summary>
        /// Disconnect from the physical device
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_DisconnectDevice(ref AARTSAAPI_Device dhandle);

        /// <summary>
        /// Start data acqusition/transmission from/to the device
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_StartDevice(ref AARTSAAPI_Device dhandle);

        /// <summary>
        /// End data acquistion/transmission from/to the device
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_StopDevice(ref AARTSAAPI_Device dhandle);


        /// <summary>
        /// Get device state
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_GetDeviceState(ref AARTSAAPI_Device dhandle);

        /// <summary>
        /// Get the nmumber of available packets from a devices data channel
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_AvailPackets(ref AARTSAAPI_Device dhandle, int channel, out int num);

        /// <summary>
        /// Get a packet from the output queue of a devices data channel
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_GetPacket(ref AARTSAAPI_Device dhandle, int channel, int index, ref AARTSAAPI_Packet packet);

        /// <summary>
        /// Consume a number of packets from a devices data channel.  If the data in a data channel is not
        /// consumed it will block when the queue is full and new data will be dropped.
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConsumePackets(ref AARTSAAPI_Device dhandle, int channel, int num);

        /// <summary>
        /// Get the current master stream time in seconds since the start of the epoch
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_GetMasterStreamTime(ref AARTSAAPI_Device dhandle, out double stime);

        /// <summary>
        /// Send a packet to an inbound channel
        /// </summary>
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_SendPacket(ref AARTSAAPI_Device dhandle, int channel, ref AARTSAAPI_Packet packet);







        // Get the configuration tree root item
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigRoot(ref AARTSAAPI_Device dhandle, out AARTSAAPI_Config config);

        // Get the health/status tree root item and update the values
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigHealth(ref AARTSAAPI_Device dhandle, out AARTSAAPI_Config config);

        // Get the first child element of a group item (internal node)
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigFirst(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config group, out AARTSAAPI_Config config);

        // Advance to the next child element of a group item
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigNext(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config group, out AARTSAAPI_Config config);

        // Find a config item from a base node using a forward slash separated path of item names
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigFind(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config group, ref AARTSAAPI_Config config, [MarshalAs(UnmanagedType.LPWStr)] string name);

        // Get the internal name of a config item
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigGetName(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, [MarshalAs(UnmanagedType.LPWStr)] out string name);

        // Get the meta data of a config item
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigGetInfo(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, ref AARTSAAPI_ConfigInfo cinfo);

        // Set a config item with a floating point value
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigSetFloat(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, double value);

        // Get the value of a config item as floating point
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigGetFloat(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, out double value);

        // Set a config item with a string value
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigSetString(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, [MarshalAs(UnmanagedType.LPWStr)] string value);

        // Get the value of a config item as a string
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigGetString(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, char[] value, ref long size);

        // Set a config item with an integer value
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigSetInteger(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, long value);

        // Get the value of a config item as integer value
        //
        [DllImport(LibPath, CharSet = CharSet.Unicode)]
        public static extern AARTSAAPI_Result AARTSAAPI_ConfigGetInteger(ref AARTSAAPI_Device dhandle, ref AARTSAAPI_Config config, out long value);


    }
}
