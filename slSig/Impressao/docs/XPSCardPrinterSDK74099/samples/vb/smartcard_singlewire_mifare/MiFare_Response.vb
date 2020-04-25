''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' helper functions to receive MiFare responses from a MiFare chip via the Duali
' reader.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Namespace dxp01sdk.mifare

    Public Class MiFare_Response

        Public Sub New(responseBytes As Byte())
            _responseBytes = responseBytes
        End Sub

        Public Function GetStatusByte() As Byte
            Return _responseBytes(0)
        End Function

        Public Function IsStatusOK() As Boolean
            Return (GetStatusWord() = &H9000)
        End Function

        Public Function GetStatusWord() As UInt16
            Dim length As Integer = _responseBytes.Length
            Dim statusWord As UInt16 = CType((_responseBytes(length - 2) << 8) + (_responseBytes(length - 1)), UInt16)
            Return statusWord
        End Function

        Public Function GetSw1Sw2() As Tuple(Of Byte, Byte)
            Dim length As Integer = _responseBytes.Length
            Dim sw1 As Byte = _responseBytes(length - 2)
            Dim sw2 As Byte = _responseBytes(length - 1)
            Return New Tuple(Of Byte, Byte)(sw1, sw2)
        End Function

        Public Function BlockHasNonzeroData() As Boolean

            ' after a read of a block, length is 16 data bytes + 3 status bytes:
            If _responseBytes.Length <> 3 + 16 Then
                Console.WriteLine("error: length of data block expected to be 16 bytes.")
                Return False
            End If

            For index As Integer = 1 To 16
                If _responseBytes(index) <> 0 Then
                    Return True
                End If
            Next
            Return False
        End Function

        Private _responseBytes As Byte()
    End Class
End Namespace