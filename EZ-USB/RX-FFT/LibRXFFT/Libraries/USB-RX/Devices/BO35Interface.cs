using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public enum AgcSetting
    {
        Fast = 0,
        Mid = 1,
        Slow = 2,
        Off = 3
    }
    public enum AfcSetting
    {
        Off = 0,
        On = 1,
    }
    public enum IfOutputSetting
    {
        ExtIf = 0,
        ExtIf1 = 1,
        ExtIf2 = 2,
        ExtIf3 = 3
    }
    public enum AudioMuteSetting
    {
        Release = 0,
        AudioMute = 1,
        RfFullGain = 2,
        AudioMuteRfFullGain = 3
    }
    public enum RfAmpSetting
    {
        Off = 0,
        On = 1,
    }
    public enum AttSetting
    {
        Off = 0,
        Att10dB = 1,
        Att20dB = 2,
        Att30dB = 3,
        AttAuto = 4,
    }
    public enum AutoModeSetting
    {
        Off = 0,
        On = 1,
    }
    public enum LcdBackLightSetting
    {
        Off = 0,
        On = 1,
    }
    public enum IfBandwidthSetting
    {
        Bw500Hz = 0,
        Bw2400Hz = 1,
        Bw4kHz = 2,
        Bw6kHz = 3,
        Bw15kHz = 4,
        Bw30kHz = 5,
        Bw110kHz = 6,
        Bw220kHz = 7
    }
    public enum CwPitchSetting
    {
        Pitch400Hz = 0,
        Pitch500Hz = 1,
        Pitch600Hz = 2,
        Pitch700Hz = 3,
        Pitch800Hz = 4,
        Pitch900Hz = 5,
        Pitch1000Hz = 6,
        Pitch12kHz = 7
    }
    public enum DeEmpSetting
    {
        Thru = 0,
        DeEmp25us = 1,
        DeEmp50us = 2,
        DeEmp75us = 3,
        DeEmp750us = 4
    }
    public enum OperationModeSetting
    {
        Remote = 0,
        Local = 1,
        Timer = 2
    }
    public enum ClockDisplaySetting
    {
        Frequency = 0,
        Clock = 1
    }
    public enum HighPassFilterSetting
    {
        Pass50Hz = 0,
        Pass200Hz = 1,
        Pass300Hz = 2,
        Pass400Hz = 3
    }
    public enum LowPassFilterSetting
    {
        Pass3kHz = 0,
        Pass4kHz = 1,
        Pass6kHz = 2,
        Pass15kHz = 3
    }
    public enum AutomaticRssiSetting
    {
        Off = 0,
        On = 1,
        AlwaysOn = 2 /* regardless of MUTE state */
    }
    public enum SignalStrengthAcqusitionSetting
    {
        Rssi = 0,
        Afcd = 1
    }
    public enum ReceiveModeSetting
    {
        Fm = 0,
        Am = 1,
        SyncAm = 2,
        SyncUsb = 3,
        SyncLsb = 4,
        Usb = 5,
        Lsb = 6,
        Cw = 7
    }
    public enum ReferenceOscSetting
    {
        Internal = 0,
        External = 1
    }
    public enum OnTimerSetting
    {
        Disable = 0,
        Enable = 1
    }
    public enum OffTimerSetting
    {
        Disable = 0,
        Enable = 1
    }
    public enum OnTimerSourceSetting
    {
        RadioAudio = 0,
        Beep = 1
    }
    public enum PriorityOperationSetting
    {
        Off = 0,
        On = 1
    }
    public enum PowerSetting
    {
        PowerOff = 0,
        PowerOn = 1
    }
    public enum DtmfDecoderSetting
    {
        Off = 0,
        On = 1
    }
    public enum SearchRestartSetting
    {
        Restart = 0,
        HoldMuteOff = 1,
        Hold = 2
    }
    public enum NoiseBlankerSetting
    {
        Off = 0,
        On = 1
    }
    public enum StepAdjustSetting
    {
        Off = 0,
        On = 1
    }
    public enum MemoryScanLinkStateSetting
    {
        Off = 0,
        On = 1
    }
    public enum ProgramSearchLinkStateSetting
    {
        Off = 0,
        On = 1
    }

    public struct MemoryScanLinkBankSetting
    {
        public int Bank;
        public bool LinkEnabled;
    }
    public struct ProgramSearchLinkBankSetting
    {
        public int Bank;
        public bool LinkEnabled;
    }
    public struct DetectorStatus
    {
        public bool PllLocked;
        public bool CtcssDetected;
        public bool RssiLsq;
        public bool ToneEliminatorDetected;
        public bool NsqOpen;
    }
    public struct DlsqSetting
    {
        public int Strength;
        public bool DlsqEnabled;
        public bool SquelchOpen;
    }
    public struct DialPauseSetting
    {
        public int PauseTime;
        public bool DialPauseEnabled;
    }
    
    public enum DialDirection
    {
        CounterClockwise,
        Clockwise
    }
    public enum PanelButton
    {
        Number0 = 0,
        Number1 = 1,
        Number2 = 2,
        Number3 = 3,
        Number4 = 4,
        Number5 = 5,
        Number6 = 6,
        Number7 = 7,
        Number8 = 8,
        Number9 = 9,

        Down = 10,
        Enter = 11,
        Dot = 12,
        Up = 13,

        Func = 101,
        Mode = 102,
        Mem = 103,
        Step = 104,
        Search = 105,
        Pass = 106,
        Dial = 107,
        Exit = 108,

        Lock = 110,
        Power = 111,
    }

    public interface BO35Interface
    {
        #region Getters/Setters
        AttSetting Att { get; set; }
        AgcSetting Agc { get; set; }
        AfcSetting Afc { get; set; }
        RfAmpSetting RfAmp { get; set; }
        IfOutputSetting IfOutput { get; set; }
        IfBandwidthSetting IfBandwidth { get; set; }

        HighPassFilterSetting HighPassFilter { get; set; }
        LowPassFilterSetting LowPassFilter { get; set; }

        OperationModeSetting OperationMode { get; set; }
        DeEmpSetting DeEmp { get; set; }
        CwPitchSetting CwPitch { get; set; }

        AudioMuteSetting AudioMute { get; set; }
        AutoModeSetting AutoMode { get; set; }
        LcdBackLightSetting LcdBackLight { get; set; }
        PowerSetting Power { get; set; }
        int IdNumber { get; set; }

        OffTimerSetting OffTimer { get; set; }
        DateTime OffTimerTime { get; set; }
        OnTimerSetting OnTimer { get; set; }
        DateTime OnTimerTime { get; set; }
        int OnTimerAfGain { get; set; }

        ClockDisplaySetting ClockDisplay { get; set; }
        AutomaticRssiSetting AutomaticRssi { get; set; }
        SignalStrengthAcqusitionSetting SignalStrengthAcqusition { get; set; }
        ReceiveModeSetting ReceiveMode { get; set; }

        MemoryScanLinkStateSetting MemoryScanLinkState { get; set; }
        MemoryScanLinkBankSetting[] MemoryScanLink { get; set; }
        ProgramSearchLinkStateSetting ProgramSearchState { get; set; }
        ProgramSearchLinkBankSetting[] ProgramSearchLink { get; set; }

        NoiseBlankerSetting NoiseBlanker { get; set; }
        ReferenceOscSetting ReferenceOsc { get; set; }
        DtmfDecoderSetting DtmfDecoder { get; set; }
        SearchRestartSetting SearchRestart { get; set; }
        StepAdjustSetting StepAdjust { get; set; }

        int DelayTime { get; set; }
        DialPauseSetting DialPause { get; set; }
        DlsqSetting Dlsq { get; set; }
        #endregion


        #region Functions

        void DialRotate(DialDirection direction);
        void PressButton(PanelButton button, bool longPress); 

        #endregion
    }
}
