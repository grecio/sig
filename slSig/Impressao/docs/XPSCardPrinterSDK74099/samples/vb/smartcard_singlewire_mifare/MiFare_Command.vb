''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' construct mifare commands for duali reader.
'
' see "CCID_Protocol_spec_120224_.pdf"
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Namespace dxp01sdk.mifare
    Public Class Mifare_Command

        Public Enum KeyType As Byte
            A = 0
            B = 4
        End Enum

        ' see pg 23, "CCID_Protocol_spec_120224_.pdf"
        Public Function CreateGetCardStatusCommand() As Byte()
            Dim commandBytes As Byte() = New Byte() {&HFE, &H17, &HFE, &HFE, &H0}
            Return commandBytes
        End Function

        ' see pg 11, "CCID_Protocol_spec_120224_.pdf"
        Public Function CreateLoadKeysCommand(keyType As KeyType, sector As Byte, keyBytes As Byte()) As Byte()
            Mifare_Common.CheckSectorNumber(sector)
            Debug.Assert(keyBytes.Length = 6)
            Dim commandBytes As List(Of Byte) = New List(Of Byte)(New Byte() {&HFD, &H2F, CByte(keyType), sector, &H6})
            commandBytes.AddRange(keyBytes)
            Return commandBytes.ToArray()
        End Function

        ' see pg 12, "CCID_Protocol_spec_120224_.pdf"
        Public Function CreateReadBlockCommand(keyType As KeyType, sector As Byte, block As Byte) As Byte()
            Mifare_Common.CheckSectorNumber(sector)
            Mifare_Common.CheckBlockNumber(block)
            Dim commandBytes As Byte() = New Byte() {&HFD, &H35, CByte(keyType), sector, &H1, block}
            Return commandBytes
        End Function

        ' see pg 13, "CCID_Protocol_spec_120224_.pdf"
        Public Function CreateWriteBlockCommand(keyType As KeyType, sector As Byte, block As Byte, blockData As Byte()) As Byte()
            Mifare_Common.CheckSectorNumber(sector)
            Mifare_Common.CheckBlockNumber(block)
            Dim commandBytes As List(Of Byte) = New List(Of Byte)(New Byte() {&HFD, &H37, CByte(keyType), sector, &H11, block})
            commandBytes.AddRange(blockData)
            Return commandBytes.ToArray()
        End Function
    End Class
End Namespace