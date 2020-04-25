''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' utility functions for mifare / duali
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Namespace dxp01sdk.mifare
    Public Class Mifare_Common

        Public Shared mifare_test_key As Byte() = New Byte() {&HFF, &HFF, &HFF, &HFF, &HFF, &HFF}

        Public Shared Function ErrorStringFromStatusByte(status As Byte) As String
            If _statusCodeStrings.ContainsKey(status) Then
                Return _statusCodeStrings(status)
            End If
            Return String.Format("undefined status code: {0}", status)
        End Function

        Public Shared Function StatusCodeBytesToString(data As Byte()) As String
            If data.Length < 3 Then
                Return "not enough data for status code commandBytes."
            End If

            Dim result As String = String.Format("status code: {0}; SW1, SW2: {1:X2} {2:X2}", ErrorStringFromStatusByte(data(0)), data(data.Length - 2), data(data.Length - 1))
            Return result
        End Function

        Public Shared Sub CheckStatusCode(statusCode As Byte)
            If statusCode <> 0 Then
                Dim msg As String = ErrorStringFromStatusByte(statusCode)
                Throw New Exception(msg)
            End If
        End Sub

        Public Shared Sub CheckSectorNumber(sector As Byte)
            If sector >= 0 AndAlso sector <= &HF Then
                Return
            End If
            Throw New Exception("sector must be between 0 and 15")
        End Sub

        Public Shared Sub CheckBlockNumber(blockNumber As Byte)
            If blockNumber >= 0 AndAlso blockNumber <= &HF Then
                Return
            End If
            Throw New Exception("block must be between 0 and 15")
        End Sub

        ' see pg 45, "CCID_Protocol_spec_120224_.pdf"
        Private Shared _statusCodeStrings As New Dictionary(Of Byte, String)() From {
         {&H0, "OK"},
         {&H2, "NO TAG ERROR"},
         {&H3, "CRC ERROR"},
         {&H4, "EMPTY (NO IC CARD ERROR)"},
         {&H5, "AUTHENTICATION ERROR or NO POWER"},
         {&H6, "PARITY ERROR"},
         {&H7, "CODE ERROR"},
         {&H8, "SERIAL NUMBER ERROR"},
         {&H9, "KEY ERROR"},
         {&HA, "NOT AUTHENTICATION ERROR"},
         {&HB, "BIT COUNT ERROR"},
         {&HC, "BYTE COUNT ERROR"},
         {&HE, "TRANSFER ERROR"},
         {&HF, "WRITE ERROR"},
         {&H10, "INCREMENT ERROR"},
         {&H11, "DECREMENT ERROR"},
         {&H12, "READ ERROR"},
         {&H13, "OVERFLOW ERROR"},
         {&H14, "POLLING ERROR"},
         {&H15, "FRAMING ERROR"},
         {&H16, "ACCESS ERROR"},
         {&H17, "UNKNOWN COMMAND ERROR"},
         {&H18, "ANTICOLLISION ERROR"},
         {&H19, "INITIALIZATION(RESET) ERROR"},
         {&H1A, "INTERFACE ERROR"},
         {&H1B, "ACCESS TIMEOUT ERROR"},
         {&H1C, "NO BITWISE ANTICOLLISION ERROR"},
         {&H1D, "FILE ERROR"},
         {&H20, "INVAILD BLOCK ERROR"},
         {&H21, "ACK COUNT ERROR"},
         {&H22, "NACK DESELECT ERROR"},
         {&H23, "NACK COUNT ERROR"},
         {&H24, "SAME FRAME COUNT ERROR"},
         {&H31, "RCV BUFFER TOO SMALL ERROR"},
         {&H32, "RCV BUFFER OVERFLOW ERROR"},
         {&H33, "RF ERROR"},
         {&H34, "PROTOCOL ERROR"},
         {&H35, "USER BUFFER FULL ERROR"},
         {&H36, "BUADRATE NOT SUPPORTED"},
         {&H37, "INVAILD FORMAT ERROR"},
         {&H38, "LRC ERROR"},
         {&H39, "FRAMERR"},
         {&H3C, "WRONG PARAMETER VALUE"},
         {&H3D, "INVAILD PARAMETER ERROR"},
         {&H3E, "UNSUPPORTED PARAMETER"},
         {&H3F, "UNSUPPORTED COMMAND"},
         {&H40, "INTERFACE NOT ENABLED"},
         {&H41, "ACK SUPPOSED"},
         {&H42, "NACK RECEVIED"},
         {&H43, "BLOCKNR NOT EQUAL"},
         {&H44, "TARGET _SET_TOX"},
         {&H45, "TARGET_RESET_TOX"},
         {&H46, "TARGET_DESELECTED"},
         {&H47, "TARGET_RELEASED"},
         {&H48, "ID_ALREADY_IN_USE"},
         {&H49, "INSTANCE_ALREADY_IN_USE"},
         {&H4A, "ID_NOT_IN_USE"},
         {&H4B, "NO_ID_AVAILABLE"},
         {&H4C, "MI_JOINER_TEMP_ERROR or OTHER_ERROR"},
         {&H4D, "INVALID _STATE"},
         {&H64, "NOTYET_IMPLEMENTED"},
         {&H6D, "FIFO ERROR"},
         {&H72, "WRONG SELECT COUNT"},
         {&H7B, "WRONG_VALUE"},
         {&H7C, "VALERR"},
         {&H7E, "RE_INIT"},
         {&H7F, "NO_INIT"}
        }
    End Class
End Namespace