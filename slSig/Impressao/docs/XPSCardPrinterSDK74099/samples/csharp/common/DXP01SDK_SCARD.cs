////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// wrapper class that is similar to Win32 smartcard API in the Windows SDK's
// WinSCard.h and WinSCard.h
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace dxp01sdk {

    /// <summary>
    /// local exception class; not thrown beyond this source file.
    /// </summary>
    /// <notes>
    /// HResult is a protected member of System.Exception
    /// </notes>
    internal class scardException : Exception {

        public scardException(string message, int hr)
            : base(message) {
            HResult = hr;
        }

        public int hresult
        {
            get { return HResult; }
        }
    }

    public class SCard {

        /// <summary>
        /// constructor.
        /// </summary>
        /// <param name="bidiSpl">
        /// an instance of a BidiSplWrap object, bound to a printer name.
        /// </param>
        public SCard(BidiSplWrap bidiSpl) {
            _bidiSpl = bidiSpl;
        }

        public enum ChipConnection {
            contact,
            contactless
        };

        /// <summary>
        /// wrapper for the win32 ::SCardConnect() function.
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379473(v=vs.85).aspx
        /// </summary>
        /// <param name="connectionType">
        /// one of SCard.contact, SCard.contactless
        /// </param>
        /// <param name="protocol">
        /// populated with one of these values:
        ///     SCARD_PROTOCOL_UNDEFINED = 0,
        ///     SCARD_PROTOCOL_T0 = 0x1,
        ///     SCARD_PROTOCOL_T1 = 0x2,
        ///     SCARD_PROTOCOL_RAW = 0x10000,
        ///     SCARD_PROTOCOL_Tx = 0x3,
        ///     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        ///     SCARD_PROTOCOL_OPTIMAL = 0
        /// </param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardConnect(ChipConnection connectionType, ref uint protocol) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var connectionTypeString = StringFromConnectionType(connectionType);
                var xmlFormat = strings.SMARTCARD_CONNECT_XML;
                var input = string.Format(xmlFormat, connectionTypeString);
                var resultXml = _bidiSpl.SetPrinterData(strings.SMARTCARD_CONNECT, input);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);

                protocol = (uint) ProtocolFromString(smartcardResponseValues._protocol);
                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        /// <summary>
        /// wrapper for the win32 ::SCardStatus() function.
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379803(v=VS.85).aspx
        /// </summary>
        /// <param name="states">
        /// populated with numeric state values like this:
        ///     2, 4, 5 for SCARD_PRESENT, SCARD_POWERED, and SCARD_NEGOTIABLE
        /// </param>
        /// <param name="protocol">
        /// populated with one of these values:
        ///     SCARD_PROTOCOL_UNDEFINED = 0,
        ///     SCARD_PROTOCOL_T0 = 0x1,
        ///     SCARD_PROTOCOL_T1 = 0x2,
        ///     SCARD_PROTOCOL_RAW = 0x10000,
        ///     SCARD_PROTOCOL_Tx = 0x3,
        ///     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        ///     SCARD_PROTOCOL_OPTIMAL = 0
        /// </param>
        /// <param name="ATRBytes">
        /// populated with the 'answer to reset' bytes
        /// </param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardStatus(ref int[] states, ref uint protocol, ref byte[] ATRBytes) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var resultXml = _bidiSpl.GetPrinterData(strings.SMARTCARD_STATUS, string.Empty);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);

                protocol = (uint) ProtocolFromString(smartcardResponseValues._protocol);
                ATRBytes = smartcardResponseValues._bytesFromBase64String;

                var intStates = new List<int>();
                foreach (string state in smartcardResponseValues._states) {
                    intStates.Add(StateFromString(state));
                }
                states = intStates.ToArray();

                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        /// <summary>
        /// wrapper for the win32 ::SCardReconnect() function.
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379797%28v=VS.85%29.aspx
        /// </summary>
        /// <param name="connectionType">
        /// one of SCard.contact, SCard.contactless
        /// </param>
        /// <param name="initialization">
        /// one of these codes:
        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        /// </param>
        /// <param name="protocol">
        /// populated with one of these values:
        ///     SCARD_PROTOCOL_UNDEFINED = 0,
        ///     SCARD_PROTOCOL_T0 = 0x1,
        ///     SCARD_PROTOCOL_T1 = 0x2,
        ///     SCARD_PROTOCOL_RAW = 0x10000,
        ///     SCARD_PROTOCOL_Tx = 0x3,
        ///     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        ///     SCARD_PROTOCOL_OPTIMAL = 0
        /// </param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardReconnect(
            ChipConnection connectionType,
            int initialization,
            ref uint protocol) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var initializationString = SCard.StringFromDisposition(initialization);
                var connectionTypeString = StringFromConnectionType(connectionType);
                var xmlFormat = strings.SMARTCARD_RECONNECT_XML;
                var input = string.Format(xmlFormat, connectionTypeString, initializationString);
                var resultXml = _bidiSpl.SetPrinterData(strings.SMARTCARD_RECONNECT, input);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);
                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        /// <summary>
        /// wrapper for the win32 ::SCardGetAttrib() function.
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=VS.85).aspx
        /// </summary>
        /// <param name="attrID">
        /// the dwAttrId here:
        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=VS.85).aspx
        /// </param>
        /// <param name="attrBytes">bytes populated as in SCardGetAttrib()</param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardGetAttrib(scard_attr attrID, ref byte[] attrBytes) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var attrString = StringFromAttr((int) attrID);
                var xmlFormat = strings.SMARTCARD_GETATTRIB_XML;
                var input = string.Format(xmlFormat, attrString);
                var resultXml = _bidiSpl.GetPrinterData(strings.SMARTCARD_GETATTRIB, input);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);

                attrBytes = smartcardResponseValues._bytesFromBase64String;
                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        /// <summary>
        /// wrapper for the win32 ::SCardTransmit() function.
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379804(v=VS.85).aspx
        /// </summary>
        /// <param name="sendBytes">
        /// these bytes are known as an 'APDU'. see:
        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa374745(v=vs.85).aspx
        /// </param>
        /// <param name="receivedBytes">
        /// populated with bytes received from the smartcard chip.
        /// </param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardTransmit(byte[] sendBytes, ref byte[] receivedBytes) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var base64String = string.Empty;
                if (sendBytes.Length >= 0) {
                    base64String = Convert.ToBase64String(sendBytes);
                }

                var xmlFormat = strings.SMARTCARD_TRANSMIT_XML;
                var input = string.Format(xmlFormat, base64String);
                var resultXml = _bidiSpl.SetPrinterData(strings.SMARTCARD_TRANSMIT, input);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);

                receivedBytes = smartcardResponseValues._bytesFromBase64String;
                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        /// <summary>
        /// wrapper for Win32 API SCardDisconnect().
        /// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        /// </summary>
        /// <param name="disposition">
        /// one of the disposition codes here:
        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        /// </param>
        /// <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        public long SCardDisConnect(int disposition) {
            long scardResult = (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            try {
                var xmlFormat = strings.SMARTCARD_DISCONNECT_XML;
                var dispositionString = SCard.StringFromDisposition(disposition);
                var input = string.Format(xmlFormat, dispositionString);
                var resultXml = _bidiSpl.SetPrinterData(strings.SMARTCARD_DISCONNECT, input);
                var smartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml);
                scardResult = ErrorFromString(smartcardResponseValues._status);
            }
            catch (scardException e) {
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.hresult);
            }
            return scardResult;
        }

        public static string StringFromConnectionType(ChipConnection connectionType) {
            switch (connectionType) {
                case ChipConnection.contact: return scard_strings.SMARTCARD_PROTOCOL_T0_OR_T1;
                case ChipConnection.contactless: return scard_strings.SMARTCARD_PROTOCOL_CONTACTLESS;
            }
            throw new Exception("expected one of contact or contactless");
        }

        public static int ProtocolFromString(string protocolString) {
            if (protocolString == scard_strings.SMARTCARD_PROTOCOL_RAW) return (int) scard_protocol.SCARD_PROTOCOL_RAW;
            if (protocolString == scard_strings.SMARTCARD_PROTOCOL_T0) return (int) scard_protocol.SCARD_PROTOCOL_T0;
            if (protocolString == scard_strings.SMARTCARD_PROTOCOL_T1) return (int) scard_protocol.SCARD_PROTOCOL_T1;
            if (protocolString == scard_strings.SMARTCARD_PROTOCOL_T0_OR_T1) return (int) scard_protocol.SCARD_PROTOCOL_Tx;
            return (int) scard_protocol.SCARD_PROTOCOL_UNDEFINED;
        }

        /// <summary>
        /// return strings for the given protocol as declared in winscard.H.
        /// Note that the protocol contains 'flag' bits.
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static string[] StringsFromProtocol(uint protocol) {
            var protocols = new List<string>();

            if ((uint) scard_protocol.SCARD_PROTOCOL_UNDEFINED == protocol) {
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW);
                return protocols.ToArray();
            }

            if (0 != ((uint) scard_protocol.SCARD_PROTOCOL_T0 & protocol)) {
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_T0);
            }

            if (0 != ((uint) scard_protocol.SCARD_PROTOCOL_T1 & protocol)) {
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_T1);
            }

            if (0 != ((uint) scard_protocol.SCARD_PROTOCOL_RAW & protocol)) {
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW);
            }

            if (0 != ((uint) scard_protocol.SCARD_PROTOCOL_DEFAULT & protocol)) {
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW);
            }

            return protocols.ToArray();
        }

        public static int StateFromString(string stateString) {
            if (stateString == scard_strings.SMARTCARD_STATE_ABSENT) return (int) scard_state.SCARD_ABSENT;
            if (stateString == scard_strings.SMARTCARD_STATE_NEGOTIABLE) return (int) scard_state.SCARD_NEGOTIABLE;
            if (stateString == scard_strings.SMARTCARD_STATE_POWERED) return (int) scard_state.SCARD_POWERED;
            if (stateString == scard_strings.SMARTCARD_STATE_PRESENT) return (int) scard_state.SCARD_PRESENT;
            if (stateString == scard_strings.SMARTCARD_STATE_SPECIFIC) return (int) scard_state.SCARD_SPECIFIC;
            if (stateString == scard_strings.SMARTCARD_STATE_SWALLOWED) return (int) scard_state.SCARD_SWALLOWED;
            if (stateString == scard_strings.SMARTCARD_STATE_UNKNOWN) return (int) scard_state.SCARD_UNKNOWN;
            return (int) scard_state.SCARD_UNKNOWN;
        }

        public static string StringFromState(int state) {
            switch (state) {
                case (int) scard_state.SCARD_UNKNOWN: return scard_strings.SMARTCARD_STATE_UNKNOWN;
                case (int) scard_state.SCARD_ABSENT: return scard_strings.SMARTCARD_STATE_ABSENT;
                case (int) scard_state.SCARD_PRESENT: return scard_strings.SMARTCARD_STATE_PRESENT;
                case (int) scard_state.SCARD_SWALLOWED: return scard_strings.SMARTCARD_STATE_SWALLOWED;
                case (int) scard_state.SCARD_POWERED: return scard_strings.SMARTCARD_STATE_POWERED;
                case (int) scard_state.SCARD_NEGOTIABLE: return scard_strings.SMARTCARD_STATE_NEGOTIABLE;
                case (int) scard_state.SCARD_SPECIFIC: return scard_strings.SMARTCARD_STATE_SPECIFIC;

                default:
                    throw new Exception(string.Format("bad state: {0}", state));
            }
        }

        /// <summary>
        /// convert the given 'error string' to a number.
        /// See
        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=VS.85).aspx#smart_card_return_values
        /// for various SCARD_ ... errors
        /// </summary>
        /// <param name="errorString">error string from printer</param>
        /// <returns>the numeric equivalent</returns>
        public static long ErrorFromString(string errorString) {
            if (errorString == scard_strings.SMARTCARD_ERR_S_SUCCESS) return (long) scard_error.SCARD_S_SUCCESS;
            if (errorString == scard_strings.SMARTCARD_ERR_E_BAD_SEEK) return (long) scard_error.SCARD_E_BAD_SEEK;
            if (errorString == scard_strings.SMARTCARD_ERR_E_CANCELLED) return (long) scard_error.SCARD_E_CANCELLED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_CANT_DISPOSE) return (long) scard_error.SCARD_E_CANT_DISPOSE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_CARD_UNSUPPORTED) return (long) scard_error.SCARD_E_CARD_UNSUPPORTED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_CERTIFICATE_UNAVAILABLE) return (long) scard_error.SCARD_E_CERTIFICATE_UNAVAILABLE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_COMM_DATA_LOST) return (long) scard_error.SCARD_E_COMM_DATA_LOST;
            if (errorString == scard_strings.SMARTCARD_ERR_E_DIR_NOT_FOUND) return (long) scard_error.SCARD_E_DIR_NOT_FOUND;
            if (errorString == scard_strings.SMARTCARD_ERR_E_DUPLICATE_READER) return (long) scard_error.SCARD_E_DUPLICATE_READER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_FILE_NOT_FOUND) return (long) scard_error.SCARD_E_FILE_NOT_FOUND;
            if (errorString == scard_strings.SMARTCARD_ERR_E_ICC_CREATEORDER) return (long) scard_error.SCARD_E_ICC_CREATEORDER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_ICC_INSTALLATION) return (long) scard_error.SCARD_E_ICC_INSTALLATION;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INSUFFICIENT_BUFFER) return (long) scard_error.SCARD_E_INSUFFICIENT_BUFFER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_ATR) return (long) scard_error.SCARD_E_INVALID_ATR;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_CHV) return (long) scard_error.SCARD_E_INVALID_CHV;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_HANDLE) return (long) scard_error.SCARD_E_INVALID_HANDLE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_PARAMETER) return (long) scard_error.SCARD_E_INVALID_PARAMETER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_TARGET) return (long) scard_error.SCARD_E_INVALID_TARGET;
            if (errorString == scard_strings.SMARTCARD_ERR_E_INVALID_VALUE) return (long) scard_error.SCARD_E_INVALID_VALUE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_ACCESS) return (long) scard_error.SCARD_E_NO_ACCESS;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_DIR) return (long) scard_error.SCARD_E_NO_DIR;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_FILE) return (long) scard_error.SCARD_E_NO_FILE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_KEY_CONTAINER) return (long) scard_error.SCARD_E_NO_KEY_CONTAINER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_MEMORY) return (long) scard_error.SCARD_E_NO_MEMORY;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_READERS_AVAILABLE) return (long) scard_error.SCARD_E_NO_READERS_AVAILABLE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_SERVICE) return (long) scard_error.SCARD_E_NO_SERVICE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_SMARTCARD) return (long) scard_error.SCARD_E_NO_SMARTCARD;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NO_SUCH_CERTIFICATE) return (long) scard_error.SCARD_E_NO_SUCH_CERTIFICATE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NOT_READY) return (long) scard_error.SCARD_E_NOT_READY;
            if (errorString == scard_strings.SMARTCARD_ERR_E_NOT_TRANSACTED) return (long) scard_error.SCARD_E_NOT_TRANSACTED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_PCI_TOO_SMALL) return (long) scard_error.SCARD_E_PCI_TOO_SMALL;
            if (errorString == scard_strings.SMARTCARD_ERR_E_PROTO_MISMATCH) return (long) scard_error.SCARD_E_PROTO_MISMATCH;
            if (errorString == scard_strings.SMARTCARD_ERR_E_READER_UNAVAILABLE) return (long) scard_error.SCARD_E_READER_UNAVAILABLE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_READER_UNSUPPORTED) return (long) scard_error.SCARD_E_READER_UNSUPPORTED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_SERVER_TOO_BUSY) return (long) scard_error.SCARD_E_SERVER_TOO_BUSY;
            if (errorString == scard_strings.SMARTCARD_ERR_E_SERVICE_STOPPED) return (long) scard_error.SCARD_E_SERVICE_STOPPED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_SHARING_VIOLATION) return (long) scard_error.SCARD_E_SHARING_VIOLATION;
            if (errorString == scard_strings.SMARTCARD_ERR_E_SYSTEM_CANCELLED) return (long) scard_error.SCARD_E_SYSTEM_CANCELLED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_TIMEOUT) return (long) scard_error.SCARD_E_TIMEOUT;
            if (errorString == scard_strings.SMARTCARD_ERR_E_UNEXPECTED) return (long) scard_error.SCARD_E_UNEXPECTED;
            if (errorString == scard_strings.SMARTCARD_ERR_E_UNKNOWN_CARD) return (long) scard_error.SCARD_E_UNKNOWN_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_E_UNKNOWN_READER) return (long) scard_error.SCARD_E_UNKNOWN_READER;
            if (errorString == scard_strings.SMARTCARD_ERR_E_UNKNOWN_RES_MNG) return (long) scard_error.SCARD_E_UNKNOWN_RES_MNG;
            if (errorString == scard_strings.SMARTCARD_ERR_E_UNSUPPORTED_FEATURE) return (long) scard_error.SCARD_E_UNSUPPORTED_FEATURE;
            if (errorString == scard_strings.SMARTCARD_ERR_E_WRITE_TOO_MANY) return (long) scard_error.SCARD_E_WRITE_TOO_MANY;
            if (errorString == scard_strings.SMARTCARD_ERR_F_COMM_ERROR) return (long) scard_error.SCARD_F_COMM_ERROR;
            if (errorString == scard_strings.SMARTCARD_ERR_F_INTERNAL_ERROR) return (long) scard_error.SCARD_F_INTERNAL_ERROR;
            if (errorString == scard_strings.SMARTCARD_ERR_F_UNKNOWN_ERROR) return (long) scard_error.SCARD_F_UNKNOWN_ERROR;
            if (errorString == scard_strings.SMARTCARD_ERR_F_WAITED_TOO_LONG) return (long) scard_error.SCARD_F_WAITED_TOO_LONG;
            if (errorString == scard_strings.SMARTCARD_ERR_P_SHUTDOWN) return (long) scard_error.SCARD_P_SHUTDOWN;
            if (errorString == scard_strings.SMARTCARD_ERR_W_CANCELLED_BY_USER) return (long) scard_error.SCARD_W_CANCELLED_BY_USER;
            if (errorString == scard_strings.SMARTCARD_ERR_W_CARD_NOT_AUTHENTICATED) return (long) scard_error.SCARD_W_CARD_NOT_AUTHENTICATED;
            if (errorString == scard_strings.SMARTCARD_ERR_W_CHV_BLOCKED) return (long) scard_error.SCARD_W_CHV_BLOCKED;
            if (errorString == scard_strings.SMARTCARD_ERR_W_EOF) return (long) scard_error.SCARD_W_EOF;
            if (errorString == scard_strings.SMARTCARD_ERR_W_REMOVED_CARD) return (long) scard_error.SCARD_W_REMOVED_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_W_RESET_CARD) return (long) scard_error.SCARD_W_RESET_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_W_SECURITY_VIOLATION) return (long) scard_error.SCARD_W_SECURITY_VIOLATION;
            if (errorString == scard_strings.SMARTCARD_ERR_W_UNPOWERED_CARD) return (long) scard_error.SCARD_W_UNPOWERED_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_W_UNRESPONSIVE_CARD) return (long) scard_error.SCARD_W_UNRESPONSIVE_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_W_UNSUPPORTED_CARD) return (long) scard_error.SCARD_W_UNSUPPORTED_CARD;
            if (errorString == scard_strings.SMARTCARD_ERR_W_WRONG_CHV) return (long) scard_error.SCARD_W_WRONG_CHV;

            throw new ArgumentException("invalid error string: " + errorString);
        }

        public static string StringFromDisposition(int disposition) {
            switch (disposition) {
                case (int) scard_disposition.SCARD_RESET_CARD: return scard_strings.SMARTCARD_DISCONNECT_RESET;
                case (int) scard_disposition.SCARD_UNPOWER_CARD: return scard_strings.SMARTCARD_DISCONNECT_UNPOWER;
                case (int) scard_disposition.SCARD_LEAVE_CARD:
                default:
                    return scard_strings.SMARTCARD_DISCONNECT_LEAVE;
            }
        }

        public static string StringFromAttr(int attributeID) {
            switch (attributeID) {
                case (int) scard_attr.SCARD_ATTR_VENDOR_NAME: return scard_strings.SMARTCARD_ATTR_VENDOR_NAME;
                case (int) scard_attr.SCARD_ATTR_VENDOR_IFD_TYPE: return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_TYPE;
                case (int) scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION: return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_VERSION;
                case (int) scard_attr.SCARD_ATTR_VENDOR_IFD_SERIAL_NO: return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_SERIAL_NO;
                case (int) scard_attr.SCARD_ATTR_ATR_STRING: return scard_strings.SMARTCARD_ATTR_ATR_STRING;
                case (int) scard_attr.SCARD_ATTR_CHANNEL_ID: return scard_strings.SMARTCARD_ATTR_CHANNEL_ID;
                case (int) scard_attr.SCARD_ATTR_CHARACTERISTICS: return scard_strings.SMARTCARD_ATTR_CHARACTERISTICS;
                case (int) scard_attr.SCARD_ATTR_CURRENT_BWT: return scard_strings.SMARTCARD_ATTR_CURRENT_BWT;
                case (int) scard_attr.SCARD_ATTR_CURRENT_CLK: return scard_strings.SMARTCARD_ATTR_CURRENT_CLK;
                case (int) scard_attr.SCARD_ATTR_CURRENT_CWT: return scard_strings.SMARTCARD_ATTR_CURRENT_CWT;
                case (int) scard_attr.SCARD_ATTR_CURRENT_D: return scard_strings.SMARTCARD_ATTR_CURRENT_D;
                case (int) scard_attr.SCARD_ATTR_CURRENT_EBC_ENCODING: return scard_strings.SMARTCARD_ATTR_CURRENT_EBC_ENCODING;
                case (int) scard_attr.SCARD_ATTR_CURRENT_F: return scard_strings.SMARTCARD_ATTR_CURRENT_F;
                case (int) scard_attr.SCARD_ATTR_CURRENT_IFSC: return scard_strings.SMARTCARD_ATTR_CURRENT_IFSC;
                case (int) scard_attr.SCARD_ATTR_CURRENT_IFSD: return scard_strings.SMARTCARD_ATTR_CURRENT_IFSD;
                case (int) scard_attr.SCARD_ATTR_CURRENT_IO_STATE: return scard_strings.SMARTCARD_ATTR_CURRENT_IO_STATE;
                case (int) scard_attr.SCARD_ATTR_CURRENT_N: return scard_strings.SMARTCARD_ATTR_CURRENT_N;
                case (int) scard_attr.SCARD_ATTR_CURRENT_PROTOCOL_TYPE: return scard_strings.SMARTCARD_ATTR_CURRENT_PROTOCOL_TYPE;
                case (int) scard_attr.SCARD_ATTR_CURRENT_W: return scard_strings.SMARTCARD_ATTR_CURRENT_W;
                case (int) scard_attr.SCARD_ATTR_DEFAULT_CLK: return scard_strings.SMARTCARD_ATTR_DEFAULT_CLK;
                case (int) scard_attr.SCARD_ATTR_DEFAULT_DATA_RATE: return scard_strings.SMARTCARD_ATTR_DEFAULT_DATA_RATE;
                case (int) scard_attr.SCARD_ATTR_DEVICE_IN_USE: return scard_strings.SMARTCARD_ATTR_DEVICE_IN_USE;
                case (int) scard_attr.SCARD_ATTR_DEVICE_SYSTEM_NAME_A: return scard_strings.SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_A;
                case (int) scard_attr.SCARD_ATTR_DEVICE_SYSTEM_NAME_W: return scard_strings.SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_W;
                case (int) scard_attr.SCARD_ATTR_DEVICE_UNIT: return scard_strings.SMARTCARD_ATTR_DEVICE_UNIT;
                case (int) scard_attr.SCARD_ATTR_ESC_AUTHREQUEST: return scard_strings.SMARTCARD_ATTR_ESC_AUTHREQUEST;
                case (int) scard_attr.SCARD_ATTR_ESC_CANCEL: return scard_strings.SMARTCARD_ATTR_ESC_CANCEL;
                case (int) scard_attr.SCARD_ATTR_ESC_RESET: return scard_strings.SMARTCARD_ATTR_ESC_RESET;
                case (int) scard_attr.SCARD_ATTR_EXTENDED_BWT: return scard_strings.SMARTCARD_ATTR_EXTENDED_BWT;
                case (int) scard_attr.SCARD_ATTR_ICC_INTERFACE_STATUS: return scard_strings.SMARTCARD_ATTR_ICC_INTERFACE_STATUS;
                case (int) scard_attr.SCARD_ATTR_ICC_PRESENCE: return scard_strings.SMARTCARD_ATTR_ICC_PRESENCE;
                case (int) scard_attr.SCARD_ATTR_ICC_TYPE_PER_ATR: return scard_strings.SMARTCARD_ATTR_ICC_TYPE_PER_ATR;
                case (int) scard_attr.SCARD_ATTR_MAX_CLK: return scard_strings.SMARTCARD_ATTR_MAX_CLK;
                case (int) scard_attr.SCARD_ATTR_MAX_DATA_RATE: return scard_strings.SMARTCARD_ATTR_MAX_DATA_RATE;
                case (int) scard_attr.SCARD_ATTR_MAX_IFSD: return scard_strings.SMARTCARD_ATTR_MAX_IFSD;
                case (int) scard_attr.SCARD_ATTR_MAXINPUT: return scard_strings.SMARTCARD_ATTR_MAXINPUT;
                case (int) scard_attr.SCARD_ATTR_POWER_MGMT_SUPPORT: return scard_strings.SMARTCARD_ATTR_POWER_MGMT_SUPPORT;
                case (int) scard_attr.SCARD_ATTR_SUPRESS_T1_IFS_REQUEST: return scard_strings.SMARTCARD_ATTR_SUPRESS_T1_IFS_REQUEST;
                case (int) scard_attr.SCARD_ATTR_USER_AUTH_INPUT_DEVICE: return scard_strings.SMARTCARD_ATTR_USER_AUTH_INPUT_DEVICE;
                case (int) scard_attr.SCARD_ATTR_USER_TO_CARD_AUTH_DEVICE: return scard_strings.SMARTCARD_ATTR_USER_TO_CARD_AUTH_DEVICE;

                default:
                    throw new Exception(string.Format("bad attributeID: {0}", attributeID));
            }
        }

        private BidiSplWrap _bidiSpl;
    }
}