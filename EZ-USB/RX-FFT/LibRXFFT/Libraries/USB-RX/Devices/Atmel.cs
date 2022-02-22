using System;
using System.Reflection;
using System.Threading;
using LibRXFFT.Libraries.USB_RX.Interfaces;

namespace LibRXFFT.Libraries.USB_RX.Devices
{
    public struct FilterCorrectionCoeff
    {
        public bool AGCState;
        public long AGCCorrectionOffset;
        public long AGCCorrectionGain;

        public FilterCorrectionCoeff(bool agcState, long offset, long gain)
        {
            AGCState = agcState;
            AGCCorrectionOffset = offset;
            AGCCorrectionGain = gain;
        }
    }

    public class Atmel : AD6636Interface
    {

        public AD6636 AD6636;
        private I2CInterface I2CDevice;
        private int BusID;
        private static int DefaultBusID = 0x20;
        private object Lock = new object();

        private long FilterClock;
        private long FilterWidth;

        /*
            20:20:03:3380   Atmel Serial: RX22 ADS5547
            20:20:03:3390   AD6636 TCXO:  98304000
            20:24:45:2870 [AtmelDelay] Test results:
            20:24:45:2870 [AtmelDelay] ---------------------------------
            20:24:45:2870 [AtmelDelay]   Minimum SetFilterDelay: 102 ms
            20:24:45:2870 [AtmelDelay]   Minimum ReadFilterDelay: 0 ms
            20:24:45:2870 [AtmelDelay]   Minimum SetAgcDelay: 8 ms
            20:24:45:2870 [AtmelDelay]   Minimum SetAttDelay: 6 ms
            20:24:45:2870 [AtmelDelay] ---------------------------------
        */
        public int SetFilterDelay = 120;
        public int ReadFilterDelay = 0;
        public int SetAgcDelay = 30;
        public int SetAttDelay = 30;


        /*
   TWIGetCmd Commandbyte 1: AD6636 Spi-programmierung 1Byte(Adr./Anzahl1/Wert)
   Commandbyte 2: AD6636 Spi-programmierung 2Bytes(Adr./Anzahl 2/ 2 Werte)   es wird nur 1 Byte programmiert !!
   Commandbyte 3: AD6636 Spi-programmierung 3Bytes(Adr./Anzahl 3/ 3 Werte)
   Commandbyte 4: AD6636 Spi-programmierung 4Bytes(Adr./Anzahl 4/ 4 Werte)

   Commandbyte 5: AD6636 Reset(PD5 )
   Commandbyte 6: AVR Software-Reset

   Commandbyte 7: Auslesen der Seriennummer
   Commandbyte 8: Schreiben der Seriennummer

   Commandbyte 9: Setze Frequenz MAX3543(ab 40 MHz )  , nun Frequenz R820
   Commandbyte 10: Setze IF/RF VGA und TCXO-Abgleich MAX3543(1 byte: DAC-ADR, 2 byte DAC-Wert) , IF-VGA DAC0: $58 , RF-VGA DAC3: $5E , DAC1: TCXO-Abgleich= $5A, DAC2: $5C
   Init MAX3543 wenn 1.byte = $00
   Commandbyte 11: Receive RS232 von AR5000
   Commandbyte 12: RS232 Daten an AR5000 senden

   Commandbyte 13: Fifo1 Reset(PA0)   resetpuls    // früher PD6
   Commandbyte 14: Fifo2 Reset(PA3)   resetpuls    // 2. Fifo gabs früher nicht
   Commandbyte 15: setze AD6636 Frequenz, Berechnung durch AVR, Übergabe als Longword   // evtl byte  gleich mit übertragen filter ein/aus
   Commandbyte 16:  lastfilter an pc
   Commandbyte 17: Search M3543:  1.byte einstellung, 2. bis zu 5. byte Wert

   Commandbyte 20: Filterumweg, HF-Filter umgehen, Kommando darf nur bei Mouse als RX gesendet werden, sonst stimmt der HF-Weg nicht mehr !
   Commandbyte 21: ATT EIn(PB2)             // zur Kompatibilität alter Karten/Software wird der ATT 0-31dB geändert
   Commandbyte 22: ATT Aus                      // zur Kompatibilität alter Karten/Software  wird der ATT 0-31dB geändert
   Commandbyte 23: Preamplifier Ein(PB3)     // zur Kompatibilität alter Karten/Software  wird der ATT 0-31dB geändert
   Commandbyte 24: Preamp Aus                   // zur Kompatibilität alter Karten/Software  wird der ATT 0-31dB geändert
   Commandbyte 25: ADG904 RF3 Tiefpass 2,5 Mhz :       PB3 auf High , PB4 auf Low   ,PB7 auf high
   Commandbyte 27: ADG904 RF4 Bandpass 2,5-6,5 Mhz PB3 auf High, PB4 auf High, PB7 auf high
   Commandbyte 26: ADG904 RF2 Bandpass 6,5-14,5 Mhz PB3 auf Low, PB4 auf High, PB7 auf high    // Bandpässe können auch anders bestückt werden !
   Commandbyte 28: ADG904 RF1 Bandpass 14,5-40 Mhz PB3 auf Low, PB4 auf Low, PB7 auf high

   Commandbyte 29: RF2 Tunereingang ADF4350-Tuner  ,(bzw HF-Filterbypass , je nach verschaltung auf früheren Boards )  // ab RX3-7 extra SP4T  , Umweg über Cmd 20
   Commandbyte 30: Eingang auf MAX3543 VUHF-Tuner schalten, und MAX3543 initialisieren  SI5351 umprogrammieren  ----------------------------------


   Commandbyte 31: Auslesen von Einzelregistern des AD6636
   Commandbyte 32: Auslesen von 2 registern des AD6636
   Commandbyte 33: Auslesen von 3 registern des AD6636
   Commandbyte 34: Auslesen von 4 registern des AD6636
   //.......................................................................................................................

   Commandbyte 35: ATT einstellen, 1 Byte als Wert übergeben entsprechend 1..31 dB ATT


   Commandbyte 36: setze AD6636 Frequenz, Berechnung durch PC, Übergabe als FreqWord-AD6636 als Longword   // evtl byte  gleich mit übertragen filter ein/aus
                                                                       // Cmd 36 ist in früheren Karten nicht vorhanden !!!!!!     erst ab April 2018 für schnellere Programmierung des ADF4350 bei SCAN , nur 1 usbtransfer

   //.......................................................................................................................

   Commandbyte 39: Cmds für den R820 F: Frequenz des VHF-Tuners R820 einstellen,
   Commandbyte 40: Fifo1 Reset An ,PA0, früher(PD6)
   Commandbyte 41: Fifo1 Reset Aus, PA0, früher(PD6)
   Commandbyte 42: Fifo2 Reset An(PA3)
   Commandbyte 43: Fifo2 Reset Aus(PA3)

   Commandbyte 44: Fifo1 zum lesen gewählt   //   PA6 auf High
   Commandbyte 45: Fifo2 zum lesen gewählt   //   PA6 auf Low

   Commandbyte 50: MGC für CH3 einstellen, 2 Bytes als Wert übergeben entsprechend 1..96 dB
   Commandbyte 51: AGC für CH3 einstellen, AGC Slow
   Commandbyte 52: AGC für CH3 einstellen, AGC Middle
   Commandbyte 53: AGC für CH3 einstellen, AGC Fast
   Commandbyte 54: AGC für CH3 AUS         // andere Werte für Offsetabgleich müssen noch programmiert werden !!!!!!!!!!!!!!!!!!!!
   Commandbyte 55: AGC Ein : Korrekturwerte für Offsetarray des aktuellen internen Filters ins eeprom schreiben, 1 Byte offsetwert + 2 Byte Gainwert der MGC kanal3
   Commandbyte 56: AGC Aus : Korrekturwerte für Offsetarray des aktuellen internen Filters ins eeprom schreiben, 1 Byte offsetwert + 2 Byte Gainwert der MGC kanal3
   Commandbyte 57: Korrekturwerte für Offsetarray des aktuellen internen Filters aus eeprom lesen
   //   Commandbyte 58: DAC für VCXO einstellen , 2 Bytes als Wert übergeben entsprechend .....

   Commandbyte 59: MGC für CH0 einstellen, 2 Bytes als Wert übergeben entsprechend 1..96 dB
   Commandbyte 60: AGC für CH0 einstellen, AGC Slow
   Commandbyte 61: AGC für CH0 einstellen, AGC Middle
   Commandbyte 62: AGC für CH0 einstellen, AGC Fast
   Commandbyte 63: AGC für CH0 AUS         //

   //  Commandbyte 64: Relais (Filter) aus, Vollband ohne Filterung
   //  Commandbyte 65:  RF1 Tiefpass 2,5 Mhz     einschalten
   //  Commandbyte 66:  RF2 Bandpass 2,5-6,5 Mhz einschalten
   //  Commandbyte 67:  RF3 Bandpass 6,5-14,5 Mhz einschalten    // Bandpässe können auch anders bestückt werden !
   //  Commandbyte 68:  RF4 Bandpass 14,5-40 Mhz  einschalten
   //  Commandbyte 69:  (Filter) Aus, Tunereingang

   Commandbyte 99:  incl(PortC,4);   // auf high für normale einfache Filter , die direkt über den AVR programmiert werden
   // bei kaskadierten Filtern wird PortC4 in Progfilter immer auf Low gesetzt
   Commandbyte 100: Filteranzahl im AVR auslesen
   Commandbyte 101: Filter1 des AVR an AD6636      // für Programmierung muß noch zusätzlich eine  1 übergeben werden !!!!!!!
   Commandbyte 102: Filter2 des AVR an AD6636      // sonst werden von dem jeweiligen Filter nur die Parameter gelesen
   Commandbyte 103: Filter3 des AVR an AD6636m
   Commandbyte 104: Filter4 des AVR an AD6636
   Commandbyte 105: Filter5 des AVR an AD6636
   Commandbyte 106: Filter6 des AVR an AD6636
   Commandbyte 107: Filter7 des AVR an AD6636
   Commandbyte 108: Filter8 des AVR an AD6636
   usw......


   Commandbyte 190: Offsettest ADS6148   //  Offset enable an ADS6148 übergeben, Timeconstante übergeben
   Commandbyte 192: AD5620-Wert aus AVR-EEprom lesen
   Commandbyte 193: Offsetwert SI570 in AVR-EEprom schreiben
   Commandbyte 194: Offsetwert SI570 aus AVR-EEprom lesen
   Commandbyte 195: Ändern der ADC-Frequenz des SI570 , vom PC aufbereitet 195:   Freq(Longword) + Reg 7,8,9,10,11,12 (10 Bytes gesamt)
   Commandbyte 196: XO-Select, PLL ADF4002 Enable/Powerdown Übergabe 1 Byte 0= nur XO, 1: XO und PLL , 2: ext 10Mhz mit PLL , 3: ext 16Mhz mit PLL
   Commandbyte 197: Setze AD5620-Wert; // Übergabe 1 Word an DAC AD5620 zur Kalibrierung des VCTCXOs, und  merken im EEprom
   Commandbyte 198: SI570Set OffsetWert;    //
   Commandbyte 199: SI570SetInitWert;    // Defaultwerte setzen 100Mhz für SI570
   Commandbyte 200: Frequenz des SI570 aus EEprom lesen(oder des TCXO* PLL = ADCFreq je nach Karte)                             // 4 Bytes
   Commandbyte 201: gewünschte Frequenz ins EEprom des AVR schreiben und SI570 programmieren   // 4 Bytes
   Commandbyte 202: FXtal-Frequenz des SI570 aus EEprom lesen, aus SI570 HSDiv, N1Div, Rfreq lesen  // 4 Bytes
   Commandbyte 203: ermittelte/gemessene FXtal-Frequenz ins EEprom des AVR schreiben           // 4 Bytes
   // gemessen 1. SI570-XO: 99,999794 Mhz mit HSDiv:5 und N1Div:10 > FXtal: 114,28976309054785139374367404858 Mhz

   Commandbyte 204:  RBW Einstellungen PLL lesen  4 Bytes
   Commandbyte 205:  RBW Einstellungen PLL schreiben  4 Bytes

   //      Commandbyte 206:  Umschalten auf externe Ref 10 Mhz statt internem 40Mhz TCXO , 1Byte Übergabe 0: interne 40 Mhz   wird nun mit CMD 196 mit erledigt

   Commandbyte 207: Auslesen der festen Seriennummer, Longword  //  z.B. RXTyp 100x: TCXO mit NB3N3011, 200x: SI570, 300x: SI571 mit PLL ADF4002 und ADC AD5620 ,400x: AD9913-DDS, 500x: TCXO mit NB3N3011 und DAC AD5660
   Commandbyte 208: Schreiben der festen Seriennummer, Longword, nur für internen Gebrauch
   Commandbyte 209: Auslesen des Temperaturwerts;  (KTY82-110 bzw bei neueren Karten SE95)
   //     Commandbyte 210: Programmieren des ADC´s ADS5547 oder auch ADS6149 , jeweils 12 worte zu übergeben  nur bei RX22
   Commandbyte 211: Auslesen Config1  // String 32
   Commandbyte 212: Schreiben Config1
   Commandbyte 213: Auslesen letzte Frequenz  // longword
   Commandbyte 214: Schreiben zuletzt genutzte Frequenz ins Eeprom vor Beenden des Delphi-Programms, lastfilter ins Eeprom
   Commandbyte 215: Auslesen Config3  // String 32
   Commandbyte 216: Schreiben Config3
   Commandbyte 217: Auslesen Config4  // String 32
   Commandbyte 218: Schreiben Config4
   Commandbyte 219: Auslesen Config5  // String 32
   Commandbyte 220: Auslesen DDC-Version CBCZ(4) oder BBCZ(6) oder unbekannt(0)  CBCZ:$4342435A,  BBCZ: $4242435A , rest unbekannt
   Commandbyte 221: Übergabe der Filtergrenzen HF-Band1(string,11)
   Commandbyte 222: Übergabe der Filtergrenzen HF-Band2(string,11)
   Commandbyte 223: Übergabe der Filtergrenzen HF-Band3(string,11)
   Commandbyte 224: Übergabe der Filtergrenzen HF-Band4(string,11)

   Commandbyte 225: DAC-Wert für VHF-TCXO des MCP4728 sichern ins Eeprom sichern
   Commandbyte 226: DAC-Wert für VHF-TCXO des MCP4728 aus Eeprom lesen
   Commandbyte 227: Erstabgleichwerte für AD5660 und VHF-TCXO des MCP4728 aus Eeprom lesen und wieder als aktuelle Werte Speichern
   Commandbyte 228: FifoSize aus Eeprom lesen  // ab RX3-9B, da teilweise 256k*18 verbaut IDT72V2113 = 262144,  sonst SN74V293 64k*18 = 65536

   Commandbyte 240: Programmende, lastfilter im eeprom speichern   //


   Commandbyte 241: Fifo- Full Auswertung bei Aufnahmen, um evtl Aufnahmefehler zu erkennen und zu melden   // PAF1 an PC3 des AVR  PCInt19  , zukünftig Beschaltung ändern z.b PC0/AVR

   Commandbyte 244: VHFAmpATT: Amplifier EinAus bzw ATT-Wert weitergeben
   Commandbyte 245: VHFFrequenz in Mhz(Word) weitergeben
   Commandbyte 246: GPS-Sync Ein, Aus,   1 Byte übergabe $01=Ein, $00=Aus, Freigabe 1pps-Impuls im CPLD : $02 Freigabe= Aus, $03: Freigabe Ein, (bei RX3 - 8GPS PA0A genutzt)
*/
        public enum eAtmelCommand
        {
            AD6636_Prog1 = 1,
            AD6636_Prog2,
            AD6636_Prog3,
            AD6636_Prog4,
            AD6636_Reset,
            Reset,
            ReadSerial,
            WriteSerial,
            Tuner_SetFreq,
            MAX3543_SetIFRF,
            RS232_Receive,
            RS232_Transmit,
            FIFO1_Reset,
            FIFO2_Reset,
            AD6636_SetFreq,
            LastFilter,
            M3543_Search,

            HF_Bypass = 20,
            ATT_On,
            ATT_Off,
            PreAmp_On,
            PreAmp_Off,
            RFSource_RF1,
            RFSource_RF2,
            RFSource_RF3,
            RFSource_RF4,
            RFSource_ext_Tuner,
            RFSource_int_Tuner,

            Tuner_Command = 39
        };


        internal class ad6636RegCacheEntry
        {
            internal int bytes;
            internal long value;
        }

        private ad6636RegCacheEntry[] AD6636RegCache = null;


        public Atmel(I2CInterface device)
            : this(device, DefaultBusID)
        {
        }

        public Atmel(I2CInterface device, int busID)
        {
            this.I2CDevice = device;
            this.BusID = busID;
        }

        public bool Exists
        {
            get
            {
                return I2CDevice.I2CWriteByte(BusID, 0x00);
            }
        }

        // FIFO Functions
        public bool FIFOReset(bool state)
        {
            lock (Lock)
            {
                return I2CDevice.I2CWriteByte(BusID, (byte)(state ? 0x28 : 0x29));
            }
        }

        /* do not use since reset would happen asynchronously */
        private bool FIFOReset()
        {
            lock (Lock)
            {
                return I2CDevice.I2CWriteByte(BusID, 0x0d);
            }
        }

        // Filter stuff
        public int GetFilterCount()
        {
            byte[] buf = new byte[1];

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteByte(BusID, 0x64))
                    return 0;

                if (!I2CDevice.I2CReadByte(BusID, buf))
                    return 0;
            }
            return (int)buf[0];
        }

        public int GetLastFilter()
        {
            byte[] buf = new byte[1];

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteByte(BusID, 0x10))
                    return 0;

                if (!I2CDevice.I2CReadByte(BusID, buf))
                    return 0;
            }
            return (int)buf[0];
        }

        public bool SetFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];
            byte[] buf = new byte[9];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 1;

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;

                /* make sure the atmel is done */
                WaitMs(SetFilterDelay);

                if (!I2CDevice.I2CReadBytes(BusID, buf))
                    return false;
            }

            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        internal bool EnableTuner()
        {
            lock (Lock)
            {
                byte[] cmd = new byte[1];
                cmd[0] = (byte)eAtmelCommand.RFSource_int_Tuner;

                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;

                /* make sure the atmel is done */
                WaitMs(1);
            }

            return true;
        }

        internal bool TunerCommand(byte[] tunerCmd, byte[] ret)
        {
            lock (Lock)
            {
                byte[] cmd = new byte[1 + tunerCmd.Length];
                cmd[0] = (byte)eAtmelCommand.Tuner_Command;
                Array.Copy(tunerCmd, 0, cmd, 1, tunerCmd.Length);

                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;

                /* make sure the atmel is done */
                WaitMs(1);

                if (ret != null && ret.Length > 0)
                {
                    if (!I2CDevice.I2CReadBytes(BusID, ret))
                        return false;

                    /* make sure the atmel is done */
                    WaitMs(1);
                }
            }

            return true;
        }

        private void WaitMs(int ms)
        {
            try
            {
                Thread.Sleep(ms);
            }
            catch (Exception e)
            {
            }
        }

        public bool ReadFilter(int index)
        {
            if (index < 0 || index > 98)
                return false;

            byte[] cmd = new byte[2];
            byte[] buf = new byte[9];

            cmd[0] = (byte)(0x65 + index);
            cmd[1] = 0;

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;

                WaitMs(ReadFilterDelay);

                if (!I2CDevice.I2CReadBytes(BusID, buf))
                    return false;
            }
            this.FilterClock = buf[5] + buf[6] * 0x100 + buf[7] * 0x10000 + buf[8] * 0x1000000;
            this.FilterWidth = buf[1] + buf[2] * 0x100 + buf[3] * 0x10000 + buf[4] * 0x1000000;

            return true;
        }

        public long GetFilterClock()
        {
            return FilterClock;
        }

        public long GetFilterWidth()
        {
            return FilterWidth;
        }

        public long TunerFrequency
        {
            set
            {
                byte[] buf = new byte[5];

                buf[0] = (byte)eAtmelCommand.Tuner_SetFreq;
                buf[1] = (byte)(value & 0xFF);
                buf[2] = (byte)((value >> 8) & 0xFF);
                buf[3] = (byte)((value >> 16) & 0xFF);
                buf[4] = (byte)((value >> 24) & 0xFF);

                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteBytes(BusID, buf))
                        return;
                }
                return;
            }
        }


        public int TCXOFreq
        {
            get
            {
                byte[] buf = new byte[4];

                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteByte(BusID, 0xC8))
                        return 0;

                    if (!I2CDevice.I2CReadBytes(BusID, buf))
                        return 0;
                }

                int value = (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];

                return value;
            }

            set
            {
                byte[] buf = new byte[5];

                buf[0] = 0xC9;
                buf[1] = (byte)(value & 0xFF);
                buf[2] = (byte)((value >> 8) & 0xFF);
                buf[3] = (byte)((value >> 16) & 0xFF);
                buf[4] = (byte)((value >> 24) & 0xFF);

                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteBytes(BusID, buf))
                        return;
                }
                return;
            }
        }

        public int GetRBW()
        {
            byte[] buf = new byte[4];

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteByte(BusID, 0xCC))
                    return 0;

                if (!I2CDevice.I2CReadBytes(BusID, buf))
                    return 0;
            }
            return (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
        }

        public bool SetRBW(int value)
        {
            byte[] buf = new byte[5];

            buf[0] = 0xCD;
            buf[1] = (byte)((value >> 0) & 0xFF);
            buf[2] = (byte)((value >> 8) & 0xFF);
            buf[3] = (byte)((value >> 16) & 0xFF);
            buf[4] = (byte)((value >> 24) & 0xFF);

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, buf))
                    return false;
            }
            return true;
        }

        public string SerialNumber
        {
            get
            {
                byte[] buf = new byte[34];
                lock (Lock)
                {
                    if (I2CDevice.I2CWriteByte(BusID, 0x07) != true)
                        return null;

                    if (I2CDevice.I2CReadBytes(BusID, buf) != true)
                        return null;
                }
                int length = buf[0];
                if (length > 32)
                    length = 0;

                char[] array = new char[length];
                for (int i = 0; i < length; i++)
                    array[i] = (char)buf[1 + i];

                return new string(array);
            }

            set
            {
                char[] array = value.ToCharArray();
                byte[] buffer = new byte[2 + array.Length];

                buffer[0] = 0x08;
                buffer[1] = (byte)array.Length;

                for (int i = 0; i < array.Length; i++)
                    buffer[2 + i] = (byte)array[i];
                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                        return;
                }
            }
        }

        public string InternalSerialNumber
        {
            get
            {
                byte[] buf = new byte[34];
                lock (Lock)
                {
                    if (I2CDevice.I2CWriteByte(BusID, 207) != true)
                        return null;

                    if (I2CDevice.I2CReadBytes(BusID, buf) != true)
                        return null;
                }
                int length = buf[0];
                if (length > 32)
                    length = 0;

                char[] array = new char[length];
                for (int i = 0; i < length; i++)
                    array[i] = (char)buf[1 + i];

                return new string(array);
            }

            set
            {
                char[] array = value.ToCharArray();
                byte[] buffer = new byte[2 + array.Length];

                buffer[0] = 208;
                buffer[1] = (byte)array.Length;

                for (int i = 0; i < array.Length; i++)
                    buffer[2 + i] = (byte)array[i];
                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteBytes(BusID, buffer))
                        return;
                }
            }
        }
        public double Temperature
        {
            get
            {
                byte[] buf = new byte[2];
                lock (Lock)
                {
                    if (I2CDevice.I2CWriteByte(BusID, 209) != true)
                        return 0;

                    if (I2CDevice.I2CReadBytes(BusID, buf) != true)
                        return 0;
                }

                return (buf[1] << 8) | buf[0];
            }
        }

        public bool SetRfSource(USBRXDevice.eRfSource source)
        {
            lock (Lock)
            {
                switch (source)
                {
                    case USBRXDevice.eRfSource.RF1:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)eAtmelCommand.RFSource_RF1))
                            return false;
                        break;
                    case USBRXDevice.eRfSource.RF2:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)eAtmelCommand.RFSource_RF2))
                            return false;
                        break;
                    case USBRXDevice.eRfSource.RF3:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)eAtmelCommand.RFSource_RF3))
                            return false;
                        break;
                    case USBRXDevice.eRfSource.RF4:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)eAtmelCommand.RFSource_RF4))
                            return false;
                        break;
                    case USBRXDevice.eRfSource.Tuner:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)eAtmelCommand.RFSource_ext_Tuner))
                            return false;
                        break;
                    case USBRXDevice.eRfSource.InternalTuner:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte) eAtmelCommand.RFSource_int_Tuner))
                            return false;
                        break;
                }
            }
            return true;
        }

        public bool SetAtt(bool state)
        {
            lock (Lock)
            {
                return I2CDevice.I2CWriteByte(BusID, (byte)(state ? 0x17 : 0x18));
            }
        }

        public bool SetAtt(int value)
        {
            lock (Lock)
            {
                byte[] buffer = new byte[2] { 0x23, (byte)value };
                bool ret = I2CDevice.I2CWriteBytes(BusID, buffer);

                Thread.Sleep(SetAttDelay);

                return ret;
            }
        }

        public bool SetPreAmp(bool state)
        {
            lock (Lock)
            {
                return I2CDevice.I2CWriteByte(BusID, (byte)(state ? 0x15 : 0x16));
            }
        }
        /*
        public bool SetPreAmp(int value)
        {
            lock (this)
            {
                byte[] buffer = new byte[2] { 0x35, (byte)value };
                return I2CDevice.I2CWriteBytes(BusID, buffer);
            }
        }
        */
        public bool SetMgc(int dB)
        {
            if (dB < 1 || dB > 96)
                return false;

            byte[] cmd = new byte[2];

            cmd[0] = 0x32;
            cmd[1] = (byte)dB;

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;
            }
            return true;
        }

        private USBRXDevice.eAgcType CurrentAgcType = USBRXDevice.eAgcType.Off;

        public bool SetAgc(USBRXDevice.eAgcType type)
        {
            if (CurrentAgcType == type)
            {
                return true;
            }

            lock (Lock)
            {
                switch (type)
                {
                    case USBRXDevice.eAgcType.Off:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 4)))
                            return false;
                        break;
                    case USBRXDevice.eAgcType.Fast:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 3)))
                            return false;
                        break;
                    case USBRXDevice.eAgcType.Medium:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 2)))
                            return false;
                        break;
                    case USBRXDevice.eAgcType.Slow:
                        if (!I2CDevice.I2CWriteByte(BusID, (byte)(0x32 + 1)))
                            return false;
                        break;
                }
                WaitMs(SetAgcDelay);
            }
            CurrentAgcType = type;
            return true;
        }



        public FilterCorrectionCoeff FilterCorrection
        {
            get
            {
                byte[] buffer = new byte[3];

                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteByte(BusID, 0x39))
                        return new FilterCorrectionCoeff(false, 0, 0);

                    if (!I2CDevice.I2CReadBytes(BusID, buffer))
                        return new FilterCorrectionCoeff(false, 0, 0);
                }

                return new FilterCorrectionCoeff(AD6636.AgcState, buffer[0], ((buffer[2] << 8) | buffer[1]));
            }

            set
            {
                byte[] cmd = new byte[4];
                long offset = value.AGCCorrectionOffset;
                long gain = value.AGCCorrectionGain;


                if (value.AGCState)
                    cmd[0] = 0x38;
                else
                    cmd[0] = 0x37;


                cmd[1] = (byte)offset;
                cmd[2] = (byte)(gain & 0xFF);
                cmd[3] = (byte)((gain >> 8) & 0xFF);

                lock (Lock)
                {
                    if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                        return;
                    WaitMs(20);
                }

                AD6636.Offset = offset;
                AD6636.Gain = gain;

                return;
            }
        }


        // AD6636 Functions
        public bool AD6636Reset()
        {
            lock (Lock)
            {
                return I2CDevice.I2CWriteByte(BusID, 0x05);
            }
        }


        public long AD6636ReadReg(int address, int bytes)
        {
            lock (Lock)
            {
                return AD6636ReadReg(address, bytes, false);
            }
        }

        public long AD6636ReadReg(int address, int bytes, bool cache)
        {
            if (bytes < 1 || bytes > 4)
                return 0;

            // read from cache only
            if (cache)
            {
                if (AD6636RegCache != null && address < AD6636RegCache.Length)
                {
                    if (AD6636RegCache[address].bytes == bytes)
                        return AD6636RegCache[address].value;
                }
            }

            byte[] cmd = new byte[3];
            byte[] buffer = new byte[bytes];

            cmd[0] = (byte)(0x1F + (bytes - 1));
            cmd[1] = (byte)address;
            cmd[2] = (byte)(bytes | 0x80);

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return -1;

                if (!I2CDevice.I2CReadBytes(BusID, buffer))
                    return -1;
            }

            long value = 0;
            for (int i = 0; i < bytes; i++)
            {
                value <<= 8;
                value |= buffer[bytes - 1 - i];
            }

            // cache the read value
            if (AD6636RegCache != null && address < AD6636RegCache.Length)
            {
                if (AD6636RegCache[address].bytes == bytes)
                    AD6636RegCache[address].value = value;
            }
            return value;
        }

        public bool AD6636WriteReg(int address, int bytes, long value)
        {
            return AD6636WriteReg(address, bytes, value, true);
        }

        public bool AD6636WriteReg(int address, int bytes, long value, bool cache)
        {
            if (bytes < 1 || bytes > 4)
                return false;

            if (cache)
            {
                if (AD6636RegCache == null)
                {
                    AD6636RegCache = new ad6636RegCacheEntry[0x100];
                    for (int pos = 0; pos < AD6636RegCache.Length; pos++)
                        AD6636RegCache[pos] = new ad6636RegCacheEntry();
                }

                if (address < AD6636RegCache.Length)
                {
                    /* data already in this register - return 
                    if (AD6636RegCache[address].bytes == bytes && AD6636RegCache[address].value == value)
                    {
                        return true;
                    }*/
                    AD6636RegCache[address].bytes = bytes;
                    AD6636RegCache[address].value = value;
                }
            }

            byte[] cmd = new byte[3 + bytes];

            cmd[0] = (byte)bytes; /* the commands 1-4 specify the number of bytes to write */
            cmd[1] = (byte)address;
            cmd[2] = (byte)bytes;

            for (int i = 0; i < bytes; i++)
            {
                cmd[3 + i] = (byte)(value & 0xFF);
                value >>= 8;
            }

            lock (Lock)
            {
                if (!I2CDevice.I2CWriteBytes(BusID, cmd))
                    return false;
            }
            return true;
        }

        public void Register(AD6636 ad6636)
        {
            AD6636 = ad6636;
        }
    }
}
