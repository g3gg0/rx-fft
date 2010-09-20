// --------------------------------------------------------------------------------------------
// SimExpress.cs
// SmartCard Subsembly Express
// Copyright © 2004-2005 Subsembly GmbH
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Windows.Forms;
using System.Text;

using Subsembly.SmartCard;
using Subsembly.SmartCard.PcSc;
using System.ServiceModel;
using System.Net;

namespace SIMAuthDaemon
{
    /// <summary>
    /// 
    /// </summary>

    public class SIMAuthDaemon
    {
        public event SIMAuthDaemonForm.LogMessageEvent LogMessage;

        private CardExpress m_aCard;

        /// <summary>
        /// 
        /// </summary>

        public SIMAuthDaemon(CardExpress aCard, SIMAuthDaemonForm.LogMessageEvent log)
        {
            LogMessage += log;
            m_aCard = aCard;
        }


        /// <summary>
        /// Reference to <see cref="CardExpress"/> instance that was passed to the constructor.
        /// </summary>

        public CardExpress Card
        {
            get
            {
                return m_aCard;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public byte[] ReadIccIdentification()
        {
            CardResponseAPDU aResponseAPDU;

            aResponseAPDU = _Select(0x3F00);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }

            aResponseAPDU = _Select(0x2FE2);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }

            return _ReadBinary(0, 10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public byte[] ReadImsi()
        {
            CardResponseAPDU aResponseAPDU;

            aResponseAPDU = _Select(0x7F20);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }

            aResponseAPDU = _Select(0x6F07);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }

            return _ReadBinary(0, 9);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public bool SelectDFTelecom()
        {
            CardResponseAPDU aResponseAPDU = _Select(0x7F10);
            return aResponseAPDU.IsSuccessful;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aOwner"></param>
        /// <param name="aCardForm"></param>
        /// <param name="sHeading"></param>
        /// <param name="sPrompt"></param>
        /// <param name="sSecurePrompt"></param>
        /// <returns></returns>

        public CardResponseAPDU VerifyChv(
            IWin32Window aOwner,
            ICardDialogs aCardForm,
            string sHeading,
            string sPrompt)
        {
            CardPinControl aPinControl = _GetVerifyChvPinControl();
            return m_aCard.VerifyPin(aOwner, aCardForm, aPinControl, sHeading, sPrompt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aOwner"></param>
        /// <param name="aCardForm"></param>
        /// <param name="sHeading"></param>
        /// <param name="sPrompt"></param>
        /// <returns></returns>

        public CardResponseAPDU ChangeChv(
            IWin32Window aOwner,
            ICardDialogs aCardForm,
            string sHeading,
            string sPrompt)
        {
            CardPinControl aPinControl = _GetChangeChvPinControl();
            return m_aCard.ChangePin(aOwner, aCardForm, aPinControl, sHeading, sPrompt);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aCommandAPDU"></param>
        /// <returns></returns>

        CardResponseAPDU _SendSimCommand(CardCommandAPDU aCommandAPDU)
        {
            CardResponseAPDU aResponseAPDU = m_aCard.SendCommand(aCommandAPDU);
            if ((aResponseAPDU.SW1 == 0x9F) && (aResponseAPDU.Lr == 0))
            {
                CardCommandAPDU aGetResponseAPDU = new CardCommandAPDU(0xA0, 0xC0, 0x00, 0x00, aResponseAPDU.SW2);
                aResponseAPDU = m_aCard.SendCommand(aGetResponseAPDU);
            }
            else if ((aResponseAPDU.SW1 == 0x67) && (aResponseAPDU.SW2 != 0x00))
            {
                aCommandAPDU.Le = aResponseAPDU.SW2;
                aResponseAPDU = m_aCard.SendCommand(aCommandAPDU);
            }

            return aResponseAPDU;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aCommandAPDU"></param>
        /// <returns></returns>

        public CardResponseAPDU RunGsmAlgo(byte[] rand)
        {
            _Select(0x7F20);
            CardCommandAPDU aCommandAPDU = new CardCommandAPDU(0xA0, 0x88, 0x00, 0x00, rand);
            CardResponseAPDU resp = _SendSimCommand(aCommandAPDU);

            return resp;
        }


        /// <summary>
        /// </summary>

        CardResponseAPDU _Select(int nFileID)
        {
            CardCommandAPDU aCommandAPDU;
            byte[] vbFileID = new byte[2];

            vbFileID[0] = (byte)((nFileID >> 8) & 0xFF);
            vbFileID[1] = (byte)(nFileID & 0xFF);

            aCommandAPDU = new CardCommandAPDU(0xA0, 0xA4, 0x00, 0x00, vbFileID);
            return _SendSimCommand(aCommandAPDU);
        }

        /// <summary>
        /// </summary>

        byte[] _ReadBinary(int nOffset, int nLength)
        {
            CardCommandAPDU aCommandAPDU;
            CardResponseAPDU aResponseAPDU;

            aCommandAPDU = new CardCommandAPDU(0xA0, 0xB0, (byte)((nOffset >> 8) & 0xFF), (byte)(nOffset & 0xFF), nLength);
            aResponseAPDU = _SendSimCommand(aCommandAPDU);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }
            else
            {
                return aResponseAPDU.GetData();
            }
        }

        /// <summary>
        /// </summary>

        byte[] _ReadRecord(int nRecord, int nLength)
        {
            CardCommandAPDU aCommandAPDU;
            CardResponseAPDU aResponseAPDU;

            aCommandAPDU = new CardCommandAPDU(0xA0, 0xB2, (byte)(nRecord), 0x04, nLength);
            aResponseAPDU = _SendSimCommand(aCommandAPDU);
            if (!aResponseAPDU.IsSuccessful)
            {
                return null;
            }
            else
            {
                return aResponseAPDU.GetData();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        CardPinControl _GetVerifyChvPinControl()
        {
            CardCommandAPDU aVerifyChvAPDU = new CardCommandAPDU(0xA0, 0x20, 0x00, 0x01,
                CardHex.ToByteArray("FFFFFFFFFFFFFFFF"));
            CardPinControl aVerifyChvPinControl = new CardPinControl(aVerifyChvAPDU,
                CardPinEncoding.Ascii, 0);
            aVerifyChvPinControl.MinLength = 4;
            aVerifyChvPinControl.MaxLength = 4;
            return aVerifyChvPinControl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        CardPinControl _GetChangeChvPinControl()
        {
            CardCommandAPDU aChangeChvAPDU = new CardCommandAPDU(0xA0, 0x24, 0x00, 0x01,
                CardHex.ToByteArray("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"));
            CardPinControl aChangeChvPinControl = new CardPinControl(aChangeChvAPDU,
                CardPinEncoding.Ascii, 0, 8);
            aChangeChvPinControl.MinLength = 4;
            aChangeChvPinControl.MaxLength = 4;
            return aChangeChvPinControl;
        }
    }
}
