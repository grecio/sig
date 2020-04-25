''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' wrapper class that is similar to Win32 smartcard API in the Windows SDK's
' WinSCard.h and WinSCard.h
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Collections.Generic

Namespace dxp01sdk
    ''' <summary>
    ''' local exception class; not thrown beyond this source file.
    ''' </summary>
    ''' <notes>
    ''' HResult is a protected member of System.Exception
    ''' </notes>
    Class scardException
        Inherits Exception
        Public Sub New(message As String, hr As Integer)
            MyBase.New(message)
            MyBase.HResult = hr
        End Sub
        Public Function GetHResult() As Integer
            Return MyBase.HResult
        End Function
    End Class

    Public Class SCard
        ''' <summary>
        ''' constructor.
        ''' </summary>
        ''' <param name="bidiSpl">
        ''' an instance of a BidiSplWrap object, bound to a printer name.
        ''' </param>
        Public Sub New(bidiSpl As BidiSplWrap)
            _bidiSpl = bidiSpl
        End Sub

        Public Enum ChipConnection
            contact
            contactless
        End Enum

        ''' <summary>
        ''' wrapper for the win32 ::SCardConnect() function.
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379473(v=vs.85).aspx
        ''' </summary>
        ''' <param name="connectionType">
        ''' one of SCard.contact, SCard.contactless
        ''' </param>
        ''' <param name="protocol">
        ''' populated with one of these values:
        '''     SCARD_PROTOCOL_UNDEFINED = 0,
        '''     SCARD_PROTOCOL_T0 = 0x1,
        '''     SCARD_PROTOCOL_T1 = 0x2,
        '''     SCARD_PROTOCOL_RAW = 0x10000,
        '''     SCARD_PROTOCOL_Tx = 0x3,
        '''     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        '''     SCARD_PROTOCOL_OPTIMAL = 0
        ''' </param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardConnect(connectionType As ChipConnection, ByRef protocol As UInteger) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim connectionTypeString As String = StringFromConnectionType(connectionType)
                Dim xmlFormat As String = strings.SMARTCARD_CONNECT_XML
                Dim input As String = String.Format(xmlFormat, connectionTypeString)
                Dim resultXml As String = _bidiSpl.SetPrinterData(strings.SMARTCARD_CONNECT, input)
                Dim smartcardResponseValues As SmartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml)

                protocol = CUInt(ProtocolFromString(smartcardResponseValues._protocol))
                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        ''' <summary>
        ''' wrapper for the win32 ::SCardStatus() function.
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379803(v=VS.85).aspx
        ''' </summary>
        ''' <param name="states">
        ''' populated with numeric state values like this:
        '''     2, 4, 5 for SCARD_PRESENT, SCARD_POWERED, and SCARD_NEGOTIABLE
        ''' </param>
        ''' <param name="protocol">
        ''' populated with one of these values:
        '''     SCARD_PROTOCOL_UNDEFINED = 0,
        '''     SCARD_PROTOCOL_T0 = 0x1,
        '''     SCARD_PROTOCOL_T1 = 0x2,
        '''     SCARD_PROTOCOL_RAW = 0x10000,
        '''     SCARD_PROTOCOL_Tx = 0x3,
        '''     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        '''     SCARD_PROTOCOL_OPTIMAL = 0
        ''' </param>
        ''' <param name="ATRBytes">
        ''' populated with the 'answer to reset' bytes
        ''' </param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardStatus(ByRef states As Integer(), ByRef protocol As UInteger, ByRef ATRBytes As Byte()) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim resultXml As String = _bidiSpl.GetPrinterData(strings.SMARTCARD_STATUS, String.Empty)
                Dim smartcardResponseValues As SmartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml)

                protocol = CUInt(ProtocolFromString(smartcardResponseValues._protocol))
                ATRBytes = smartcardResponseValues._bytesFromBase64String

                Dim intStates As List(Of Integer) = New List(Of Integer)()
                For Each state As String In smartcardResponseValues._states
                    intStates.Add(StateFromString(state))
                Next
                states = intStates.ToArray()

                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        ''' <summary>
        ''' wrapper for the win32 ::SCardReconnect() function.
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379797%28v=VS.85%29.aspx
        ''' </summary>
        ''' <param name="connectionType">
        ''' one of SCard.contact, SCard.contactless
        ''' </param>
        ''' <param name="initialization">
        ''' one of these codes:
        '''     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        ''' </param>
        ''' <param name="protocol">
        ''' populated with one of these values:
        '''     SCARD_PROTOCOL_UNDEFINED = 0,
        '''     SCARD_PROTOCOL_T0 = 0x1,
        '''     SCARD_PROTOCOL_T1 = 0x2,
        '''     SCARD_PROTOCOL_RAW = 0x10000,
        '''     SCARD_PROTOCOL_Tx = 0x3,
        '''     SCARD_PROTOCOL_DEFAULT = 0x80000000,
        '''     SCARD_PROTOCOL_OPTIMAL = 0
        ''' </param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardReconnect(connectionType As ChipConnection, initialization As Integer, ByRef protocol As UInteger) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim initializationString As String = SCard.StringFromDisposition(initialization)
                Dim connectionTypeString As String = StringFromConnectionType(connectionType)
                Dim xmlFormat As String = strings.SMARTCARD_RECONNECT_XML
                Dim input As String = String.Format(xmlFormat, connectionTypeString, initializationString)
                Dim resultXml As String = _bidiSpl.SetPrinterData(strings.SMARTCARD_RECONNECT, input)
                Dim smartcardResponseValues As SmartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml)
                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        ''' <summary>
        ''' wrapper for the win32 ::SCardGetAttrib() function.
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=VS.85).aspx
        ''' </summary>
        ''' <param name="attrID">
        ''' the dwAttrId here:
        '''     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=VS.85).aspx
        ''' </param>
        ''' <param name="attrBytes">bytes populated as in SCardGetAttrib()</param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardGetAttrib(attrID As scard_attr, ByRef attrBytes As Byte()) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim attrString As String = StringFromAttr(CInt(attrID))
                Dim xmlFormat As String = strings.SMARTCARD_GETATTRIB_XML
                Dim input As String = String.Format(xmlFormat, attrString)
                Dim resultXml As String = _bidiSpl.GetPrinterData(strings.SMARTCARD_GETATTRIB, input)
                Dim smartcardResponseValues As SmartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml)

                attrBytes = smartcardResponseValues._bytesFromBase64String
                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        ''' <summary>
        ''' wrapper for the win32 ::SCardTransmit() function.
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379804(v=VS.85).aspx
        ''' </summary>
        ''' <param name="sendBytes">
        ''' these bytes are known as an 'APDU'. see:
        '''     http://msdn.microsoft.com/en-us/library/windows/desktop/aa374745(v=vs.85).aspx
        ''' </param>
        ''' <param name="receivedBytes">
        ''' populated with bytes received from the smartcard chip.
        ''' </param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardTransmit(sendBytes As Byte(), ByRef receivedBytes As Byte()) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim base64String As String = String.Empty
                If sendBytes.Length >= 0 Then
                    base64String = Convert.ToBase64String(sendBytes)
                End If

                Dim xmlFormat As String = strings.SMARTCARD_TRANSMIT_XML
                Dim input As String = String.Format(xmlFormat, base64String)
                Dim resultXml As String = _bidiSpl.SetPrinterData(strings.SMARTCARD_TRANSMIT, input)
                Dim smartcardResponseValues as SmartcardResponseValues= Util.ParseSmartcardResponseXML(resultXml)

                receivedBytes = smartcardResponseValues._bytesFromBase64String
                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        ''' <summary>
        ''' wrapper for Win32 API SCardDisconnect().
        ''' see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        ''' </summary>
        ''' <param name="disposition">
        ''' one of the disposition codes here:
        '''     http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
        ''' </param>
        ''' <returns>a smartcard error code as defined in WinError.H; SCARD_S_SUCCESS if no error.</returns>
        Public Function SCardDisConnect(disposition As Integer) As Long
            Dim scardResult As Long = CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            Try
                Dim xmlFormat As String = strings.SMARTCARD_DISCONNECT_XML
                Dim dispositionString As String = SCard.StringFromDisposition(disposition)
                Dim input As String = String.Format(xmlFormat, dispositionString)
                Dim resultXml As String = _bidiSpl.SetPrinterData(strings.SMARTCARD_DISCONNECT, input)
                Dim smartcardResponseValues As SmartcardResponseValues = Util.ParseSmartcardResponseXML(resultXml)
                scardResult = ErrorFromString(smartcardResponseValues._status)
            Catch e As scardException
                Console.WriteLine("{0}: ; hresult: 0x{0:x}", e.Message, e.GetHResult)
            End Try
            Return scardResult
        End Function

        Public Shared Function StringFromConnectionType(connectionType As ChipConnection) As String
            Select Case connectionType
                Case ChipConnection.contact
                    Return scard_strings.SMARTCARD_PROTOCOL_T0_OR_T1
                Case ChipConnection.contactless
                    Return scard_strings.SMARTCARD_PROTOCOL_CONTACTLESS
            End Select
            Throw New Exception("expected one of contact or contactless")
        End Function

        Public Shared Function ProtocolFromString(protocolString As String) As Integer
            If protocolString = scard_strings.SMARTCARD_PROTOCOL_RAW Then
                Return CInt(scard_protocol.SCARD_PROTOCOL_RAW)
            End If
            If protocolString = scard_strings.SMARTCARD_PROTOCOL_T0 Then
                Return CInt(scard_protocol.SCARD_PROTOCOL_T0)
            End If
            If protocolString = scard_strings.SMARTCARD_PROTOCOL_T1 Then
                Return CInt(scard_protocol.SCARD_PROTOCOL_T1)
            End If
            If protocolString = scard_strings.SMARTCARD_PROTOCOL_T0_OR_T1 Then
                Return CInt(scard_protocol.SCARD_PROTOCOL_Tx)
            End If
            Return CInt(scard_protocol.SCARD_PROTOCOL_UNDEFINED)
        End Function

        ''' <summary>
        ''' return strings for the given protocol as declared in winscard.H.
        ''' Note that the protocol contains 'flag' bits.
        ''' </summary>
        ''' <param name="protocol"></param>
        ''' <returns></returns>
        Public Shared Function StringsFromProtocol(protocol As UInteger) As String()
            Dim protocols As List(Of String) = New List(Of String)()

            If CUInt(scard_protocol.SCARD_PROTOCOL_UNDEFINED) = protocol Then
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW)
                Return protocols.ToArray()
            End If

            If 0 <> (CUInt(scard_protocol.SCARD_PROTOCOL_T0) And protocol) Then
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_T0)
            End If

            If 0 <> (CUInt(scard_protocol.SCARD_PROTOCOL_T1) And protocol) Then
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_T1)
            End If

            If 0 <> (CUInt(scard_protocol.SCARD_PROTOCOL_RAW) And protocol) Then
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW)
            End If

            If 0 <> (CUInt(scard_protocol.SCARD_PROTOCOL_DEFAULT) And protocol) Then
                protocols.Add(scard_strings.SMARTCARD_PROTOCOL_RAW)
            End If

            Return protocols.ToArray()
        End Function

        Public Shared Function StateFromString(stateString As String) As Integer
            If stateString = scard_strings.SMARTCARD_STATE_ABSENT Then
                Return CInt(scard_state.SCARD_ABSENT)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_NEGOTIABLE Then
                Return CInt(scard_state.SCARD_NEGOTIABLE)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_POWERED Then
                Return CInt(scard_state.SCARD_POWERED)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_PRESENT Then
                Return CInt(scard_state.SCARD_PRESENT)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_SPECIFIC Then
                Return CInt(scard_state.SCARD_SPECIFIC)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_SWALLOWED Then
                Return CInt(scard_state.SCARD_SWALLOWED)
            End If
            If stateString = scard_strings.SMARTCARD_STATE_UNKNOWN Then
                Return CInt(scard_state.SCARD_UNKNOWN)
            End If
            Return CInt(scard_state.SCARD_UNKNOWN)
        End Function

        Public Shared Function StringFromState(state As Integer) As String
            Select Case state
                Case CInt(scard_state.SCARD_UNKNOWN)
                    Return scard_strings.SMARTCARD_STATE_UNKNOWN
                Case CInt(scard_state.SCARD_ABSENT)
                    Return scard_strings.SMARTCARD_STATE_ABSENT
                Case CInt(scard_state.SCARD_PRESENT)
                    Return scard_strings.SMARTCARD_STATE_PRESENT
                Case CInt(scard_state.SCARD_SWALLOWED)
                    Return scard_strings.SMARTCARD_STATE_SWALLOWED
                Case CInt(scard_state.SCARD_POWERED)
                    Return scard_strings.SMARTCARD_STATE_POWERED
                Case CInt(scard_state.SCARD_NEGOTIABLE)
                    Return scard_strings.SMARTCARD_STATE_NEGOTIABLE
                Case CInt(scard_state.SCARD_SPECIFIC)
                    Return scard_strings.SMARTCARD_STATE_SPECIFIC
                Case Else

                    Throw New Exception(String.Format("bad state: {0}", state))
            End Select
        End Function
        ''' <summary>
        ''' convert the given 'error string' to a number.
        ''' See
        '''     http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=VS.85).aspx#smart_card_return_values
        ''' for various SCARD_ ... errors
        ''' </summary>
        ''' <param name="errorString">error string from printer</param>
        ''' <returns>the numeric equivalent</returns>
        Public Shared Function ErrorFromString(errorString As String) As Long
            If errorString = scard_strings.SMARTCARD_ERR_S_SUCCESS Then
                Return CLng(scard_error.SCARD_S_SUCCESS)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_BAD_SEEK Then
                Return CLng(scard_error.SCARD_E_BAD_SEEK)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_CANCELLED Then
                Return CLng(scard_error.SCARD_E_CANCELLED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_CANT_DISPOSE Then
                Return CLng(scard_error.SCARD_E_CANT_DISPOSE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_CARD_UNSUPPORTED Then
                Return CLng(scard_error.SCARD_E_CARD_UNSUPPORTED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_CERTIFICATE_UNAVAILABLE Then
                Return CLng(scard_error.SCARD_E_CERTIFICATE_UNAVAILABLE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_COMM_DATA_LOST Then
                Return CLng(scard_error.SCARD_E_COMM_DATA_LOST)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_DIR_NOT_FOUND Then
                Return CLng(scard_error.SCARD_E_DIR_NOT_FOUND)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_DUPLICATE_READER Then
                Return CLng(scard_error.SCARD_E_DUPLICATE_READER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_FILE_NOT_FOUND Then
                Return CLng(scard_error.SCARD_E_FILE_NOT_FOUND)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_ICC_CREATEORDER Then
                Return CLng(scard_error.SCARD_E_ICC_CREATEORDER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_ICC_INSTALLATION Then
                Return CLng(scard_error.SCARD_E_ICC_INSTALLATION)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INSUFFICIENT_BUFFER Then
                Return CLng(scard_error.SCARD_E_INSUFFICIENT_BUFFER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_ATR Then
                Return CLng(scard_error.SCARD_E_INVALID_ATR)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_CHV Then
                Return CLng(scard_error.SCARD_E_INVALID_CHV)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_HANDLE Then
                Return CLng(scard_error.SCARD_E_INVALID_HANDLE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_PARAMETER Then
                Return CLng(scard_error.SCARD_E_INVALID_PARAMETER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_TARGET Then
                Return CLng(scard_error.SCARD_E_INVALID_TARGET)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_INVALID_VALUE Then
                Return CLng(scard_error.SCARD_E_INVALID_VALUE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_ACCESS Then
                Return CLng(scard_error.SCARD_E_NO_ACCESS)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_DIR Then
                Return CLng(scard_error.SCARD_E_NO_DIR)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_FILE Then
                Return CLng(scard_error.SCARD_E_NO_FILE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_KEY_CONTAINER Then
                Return CLng(scard_error.SCARD_E_NO_KEY_CONTAINER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_MEMORY Then
                Return CLng(scard_error.SCARD_E_NO_MEMORY)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_READERS_AVAILABLE Then
                Return CLng(scard_error.SCARD_E_NO_READERS_AVAILABLE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_SERVICE Then
                Return CLng(scard_error.SCARD_E_NO_SERVICE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_SMARTCARD Then
                Return CLng(scard_error.SCARD_E_NO_SMARTCARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NO_SUCH_CERTIFICATE Then
                Return CLng(scard_error.SCARD_E_NO_SUCH_CERTIFICATE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NOT_READY Then
                Return CLng(scard_error.SCARD_E_NOT_READY)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_NOT_TRANSACTED Then
                Return CLng(scard_error.SCARD_E_NOT_TRANSACTED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_PCI_TOO_SMALL Then
                Return CLng(scard_error.SCARD_E_PCI_TOO_SMALL)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_PROTO_MISMATCH Then
                Return CLng(scard_error.SCARD_E_PROTO_MISMATCH)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_READER_UNAVAILABLE Then
                Return CLng(scard_error.SCARD_E_READER_UNAVAILABLE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_READER_UNSUPPORTED Then
                Return CLng(scard_error.SCARD_E_READER_UNSUPPORTED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_SERVER_TOO_BUSY Then
                Return CLng(scard_error.SCARD_E_SERVER_TOO_BUSY)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_SERVICE_STOPPED Then
                Return CLng(scard_error.SCARD_E_SERVICE_STOPPED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_SHARING_VIOLATION Then
                Return CLng(scard_error.SCARD_E_SHARING_VIOLATION)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_SYSTEM_CANCELLED Then
                Return CLng(scard_error.SCARD_E_SYSTEM_CANCELLED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_TIMEOUT Then
                Return CLng(scard_error.SCARD_E_TIMEOUT)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_UNEXPECTED Then
                Return CLng(scard_error.SCARD_E_UNEXPECTED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_UNKNOWN_CARD Then
                Return CLng(scard_error.SCARD_E_UNKNOWN_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_UNKNOWN_READER Then
                Return CLng(scard_error.SCARD_E_UNKNOWN_READER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_UNKNOWN_RES_MNG Then
                Return CLng(scard_error.SCARD_E_UNKNOWN_RES_MNG)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_UNSUPPORTED_FEATURE Then
                Return CLng(scard_error.SCARD_E_UNSUPPORTED_FEATURE)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_E_WRITE_TOO_MANY Then
                Return CLng(scard_error.SCARD_E_WRITE_TOO_MANY)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_F_COMM_ERROR Then
                Return CLng(scard_error.SCARD_F_COMM_ERROR)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_F_INTERNAL_ERROR Then
                Return CLng(scard_error.SCARD_F_INTERNAL_ERROR)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_F_UNKNOWN_ERROR Then
                Return CLng(scard_error.SCARD_F_UNKNOWN_ERROR)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_F_WAITED_TOO_LONG Then
                Return CLng(scard_error.SCARD_F_WAITED_TOO_LONG)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_P_SHUTDOWN Then
                Return CLng(scard_error.SCARD_P_SHUTDOWN)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_CANCELLED_BY_USER Then
                Return CLng(scard_error.SCARD_W_CANCELLED_BY_USER)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_CARD_NOT_AUTHENTICATED Then
                Return CLng(scard_error.SCARD_W_CARD_NOT_AUTHENTICATED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_CHV_BLOCKED Then
                Return CLng(scard_error.SCARD_W_CHV_BLOCKED)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_EOF Then
                Return CLng(scard_error.SCARD_W_EOF)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_REMOVED_CARD Then
                Return CLng(scard_error.SCARD_W_REMOVED_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_RESET_CARD Then
                Return CLng(scard_error.SCARD_W_RESET_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_SECURITY_VIOLATION Then
                Return CLng(scard_error.SCARD_W_SECURITY_VIOLATION)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_UNPOWERED_CARD Then
                Return CLng(scard_error.SCARD_W_UNPOWERED_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_UNRESPONSIVE_CARD Then
                Return CLng(scard_error.SCARD_W_UNRESPONSIVE_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_UNSUPPORTED_CARD Then
                Return CLng(scard_error.SCARD_W_UNSUPPORTED_CARD)
            End If
            If errorString = scard_strings.SMARTCARD_ERR_W_WRONG_CHV Then
                Return CLng(scard_error.SCARD_W_WRONG_CHV)
            End If

            Throw New ArgumentException("invalid error string: " & errorString)
        End Function

        Public Shared Function StringFromDisposition(disposition As Integer) As String
            Select Case disposition
                Case CInt(scard_disposition.SCARD_RESET_CARD)
                    Return scard_strings.SMARTCARD_DISCONNECT_RESET
                Case CInt(scard_disposition.SCARD_UNPOWER_CARD)
                    Return scard_strings.SMARTCARD_DISCONNECT_UNPOWER
                Case CInt(scard_disposition.SCARD_LEAVE_CARD)
                    Return scard_strings.SMARTCARD_DISCONNECT_LEAVE
            End Select
            Return scard_strings.SMARTCARD_DISCONNECT_LEAVE
        End Function

        Public Shared Function StringFromAttr(attributeID As Integer) As String
            Select Case attributeID
                Case CInt(scard_attr.SCARD_ATTR_VENDOR_NAME)
                    Return scard_strings.SMARTCARD_ATTR_VENDOR_NAME
                Case CInt(scard_attr.SCARD_ATTR_VENDOR_IFD_TYPE)
                    Return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_TYPE
                Case CInt(scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION)
                    Return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_VERSION
                Case CInt(scard_attr.SCARD_ATTR_VENDOR_IFD_SERIAL_NO)
                    Return scard_strings.SMARTCARD_ATTR_VENDOR_IFD_SERIAL_NO
                Case CInt(scard_attr.SCARD_ATTR_ATR_STRING)
                    Return scard_strings.SMARTCARD_ATTR_ATR_STRING
                Case CInt(scard_attr.SCARD_ATTR_CHANNEL_ID)
                    Return scard_strings.SMARTCARD_ATTR_CHANNEL_ID
                Case CInt(scard_attr.SCARD_ATTR_CHARACTERISTICS)
                    Return scard_strings.SMARTCARD_ATTR_CHARACTERISTICS
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_BWT)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_BWT
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_CLK)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_CLK
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_CWT)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_CWT
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_D)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_D
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_EBC_ENCODING)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_EBC_ENCODING
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_F)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_F
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_IFSC)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_IFSC
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_IFSD)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_IFSD
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_IO_STATE)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_IO_STATE
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_N)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_N
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_PROTOCOL_TYPE)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_PROTOCOL_TYPE
                Case CInt(scard_attr.SCARD_ATTR_CURRENT_W)
                    Return scard_strings.SMARTCARD_ATTR_CURRENT_W
                Case CInt(scard_attr.SCARD_ATTR_DEFAULT_CLK)
                    Return scard_strings.SMARTCARD_ATTR_DEFAULT_CLK
                Case CInt(scard_attr.SCARD_ATTR_DEFAULT_DATA_RATE)
                    Return scard_strings.SMARTCARD_ATTR_DEFAULT_DATA_RATE
                Case CInt(scard_attr.SCARD_ATTR_DEVICE_IN_USE)
                    Return scard_strings.SMARTCARD_ATTR_DEVICE_IN_USE
                Case CInt(scard_attr.SCARD_ATTR_DEVICE_SYSTEM_NAME_A)
                    Return scard_strings.SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_A
                Case CInt(scard_attr.SCARD_ATTR_DEVICE_SYSTEM_NAME_W)
                    Return scard_strings.SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_W
                Case CInt(scard_attr.SCARD_ATTR_DEVICE_UNIT)
                    Return scard_strings.SMARTCARD_ATTR_DEVICE_UNIT
                Case CInt(scard_attr.SCARD_ATTR_ESC_AUTHREQUEST)
                    Return scard_strings.SMARTCARD_ATTR_ESC_AUTHREQUEST
                Case CInt(scard_attr.SCARD_ATTR_ESC_CANCEL)
                    Return scard_strings.SMARTCARD_ATTR_ESC_CANCEL
                Case CInt(scard_attr.SCARD_ATTR_ESC_RESET)
                    Return scard_strings.SMARTCARD_ATTR_ESC_RESET
                Case CInt(scard_attr.SCARD_ATTR_EXTENDED_BWT)
                    Return scard_strings.SMARTCARD_ATTR_EXTENDED_BWT
                Case CInt(scard_attr.SCARD_ATTR_ICC_INTERFACE_STATUS)
                    Return scard_strings.SMARTCARD_ATTR_ICC_INTERFACE_STATUS
                Case CInt(scard_attr.SCARD_ATTR_ICC_PRESENCE)
                    Return scard_strings.SMARTCARD_ATTR_ICC_PRESENCE
                Case CInt(scard_attr.SCARD_ATTR_ICC_TYPE_PER_ATR)
                    Return scard_strings.SMARTCARD_ATTR_ICC_TYPE_PER_ATR
                Case CInt(scard_attr.SCARD_ATTR_MAX_CLK)
                    Return scard_strings.SMARTCARD_ATTR_MAX_CLK
                Case CInt(scard_attr.SCARD_ATTR_MAX_DATA_RATE)
                    Return scard_strings.SMARTCARD_ATTR_MAX_DATA_RATE
                Case CInt(scard_attr.SCARD_ATTR_MAX_IFSD)
                    Return scard_strings.SMARTCARD_ATTR_MAX_IFSD
                Case CInt(scard_attr.SCARD_ATTR_MAXINPUT)
                    Return scard_strings.SMARTCARD_ATTR_MAXINPUT
                Case CInt(scard_attr.SCARD_ATTR_POWER_MGMT_SUPPORT)
                    Return scard_strings.SMARTCARD_ATTR_POWER_MGMT_SUPPORT
                Case CInt(scard_attr.SCARD_ATTR_SUPRESS_T1_IFS_REQUEST)
                    Return scard_strings.SMARTCARD_ATTR_SUPRESS_T1_IFS_REQUEST
                Case CInt(scard_attr.SCARD_ATTR_USER_AUTH_INPUT_DEVICE)
                    Return scard_strings.SMARTCARD_ATTR_USER_AUTH_INPUT_DEVICE
                Case CInt(scard_attr.SCARD_ATTR_USER_TO_CARD_AUTH_DEVICE)
                    Return scard_strings.SMARTCARD_ATTR_USER_TO_CARD_AUTH_DEVICE
                Case Else

                    Throw New Exception(String.Format("bad attributeID: {0}", attributeID))
            End Select
        End Function

        Private _bidiSpl As BidiSplWrap
    End Class
End Namespace

