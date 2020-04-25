''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Card Printer SDK
'
' vb.net wrapper class for IBidiSpl COM interfaces.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Runtime.InteropServices

Namespace dxp01sdk

    Public Class BidiSplWrap
        ' static extern methods for ibidispl through interop
        <ComImport(), Guid("B9162A23-45F9-47cc-80F5-FE0FE9B9E1A2")> _
        Public Class BidiRequest
        End Class

        <ComImport(), Guid("FC5B8A24-DB05-4a01-8388-22EDF6C2BBBA")> _
        Public Class BidiRequestContainer
        End Class

        <ComImport(), Guid("2A614240-A4C5-4c33-BD87-1BC709331639")> _
        Public Class BidiSpl
        End Class

        <Guid("D580DC0E-DE39-4649-BAA8-BF0B85A03A97")> _
        <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Public Interface IBidiSpl
            Function BindDevice(<[In]()> ByVal prnName As String, <[In]()> ByVal access As Int32) As Int32
            Function UnbindDevice() As Int32
            Function SendRecv(<[In]()> ByVal action As String, <[In](), MarshalAs(UnmanagedType.[Interface])> ByVal req As IBidiRequest) As Int32
            Function MultiSendRecv(<[In]()> ByVal action As String, <[In](), MarshalAs(UnmanagedType.[Interface])> ByVal reqCont As IBidiRequestContainer) As Int32
        End Interface

        <Guid("8F348BD7-4B47-4755-8A9D-0F422DF3DC89")> _
        <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Public Interface IBidiRequest
            Function SetSchema(<[In]()> ByVal schema As String) As Int32
            Function SetInputData(<[In]()> ByVal type As UInt32, <[In]()> ByVal pData As IntPtr, <[In]()> ByVal size As UInt32) As Int32
            Function GetResult(<Out()> ByVal pHRes As IntPtr) As Int32
            Function GetOutputData(<[In]()> ByVal index As Int32, <Out()> ByVal schema As String, <Out()> ByVal type As IntPtr, <Out()> ByVal ppData As IntPtr, <Out()> ByVal size As IntPtr) As Int32
            Function GetEnumCount(<Out()> ByVal pTotal As IntPtr) As Int32
        End Interface

        <Guid("D752F6C0-94A8-4275-A77D-8F1D1A1121AE")> _
        <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Public Interface IBidiRequestContainer
            Function AddRequest(<[In](), MarshalAs(UnmanagedType.[Interface])> ByVal req As IBidiRequest) As Int32
            Function GetEnumObject(<Out(), MarshalAs(UnmanagedType.[Interface])> ByVal enumBidiReqCont As IEnumUnknown) As Int32
            Function GetRequestCount(<Out()> ByVal count As UInt32) As Int32
        End Interface

        <Guid("00000100-0000-0000-C000-000000000046")> _
        <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Public Interface IEnumUnknown
            Function [Next](
                <[In](), _
                MarshalAs(UnmanagedType.U4)> ByVal celt As UInt32, <Out(), _
                MarshalAs(UnmanagedType.U4)> ByVal rgelt As IntPtr, <Out(), _
                MarshalAs(UnmanagedType.U4)> ByVal pceltFetched As UInt32) _
            As Int32

            Function Skip(<[In](), MarshalAs(UnmanagedType.U4)> ByVal celt As UInt32) As Int32
            Function Reset() As Int32
            Function Clone(<Out(), MarshalAs(UnmanagedType.LPStruct)> ByVal enumUnk As IEnumUnknown) As Int32
        End Interface

        ' IBidiSpl strings:
        Const BIDI_ACTION_GET As String = "Get"
        Const BIDI_ACTION_SET As String = "Set"

        Const BIDI_ACCESS_ADMINISTRATOR As Int32 = &H1
        Const BIDI_ACCESS_USER As Int32 = &H2

        Public Enum BIDI_TYPE
            BIDI_NULL = 0
            BIDI_INT = 1
            BIDI_FLOAT = 2
            BIDI_BOOL = 3
            BIDI_STRING = 4
            BIDI_TEXT = 5
            BIDI_ENUM = 6
            BIDI_BLOB = 7
        End Enum

        ' retain this COM pointer instance:
        Private _iBidiSpl As IBidiSpl = Nothing

        Public Sub BindDevice(ByVal printerName As String)
            Try
                Dim bidi As BidiSpl = DirectCast(Activator.CreateInstance(GetType(BidiSpl), True), BidiSpl)
                _iBidiSpl = DirectCast(bidi, IBidiSpl)
                _iBidiSpl.BindDevice(printerName, BIDI_ACCESS_USER)
            Catch e As SystemException
                Console.WriteLine(e.Message)
                Environment.Exit(-1)
            End Try
        End Sub

        Public Sub UnbindDevice()
            If _iBidiSpl IsNot Nothing Then
                _iBidiSpl.UnbindDevice()
                _iBidiSpl = Nothing
            End If
        End Sub

        Public Function SendRecv(ByVal action As String, ByVal actionType As String, ByVal data As String) As String
            Dim dataPointer As IntPtr = Nothing
            Dim dataTypePointer As IntPtr = Nothing
            Dim sizePointer As IntPtr = Nothing
            Dim pointerToDataPointer As IntPtr = Nothing
            Dim tempResultPointer As IntPtr = Nothing
            Dim countPointer As IntPtr = Nothing
            Dim resultPointer As IntPtr = Nothing

            Dim xml As String = String.Empty

            Try
                Dim bidiRequest As BidiRequest = DirectCast(Activator.CreateInstance(GetType(BidiRequest), True), BidiRequest)
                Dim iBidiRequest As IBidiRequest = DirectCast(bidiRequest, IBidiRequest)

                iBidiRequest.SetSchema(action)

                ' Set the input data for the request, if any
                If data.Length <> 0 Then
                    dataPointer = Marshal.AllocCoTaskMem(data.Length * 2)
                    Dim test As Char() = data.ToCharArray()
                    Marshal.Copy(test, 0, dataPointer, data.Length)
                    iBidiRequest.SetInputData(CType(BIDI_TYPE.BIDI_BLOB, UInt32), dataPointer, CType(data.Length * 2, UInt32))
                End If

                ' Send request to driver
                Dim val As Int32 = _iBidiSpl.SendRecv(actionType, iBidiRequest)

                ' Check if request was a success
                resultPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(resultPointer))

                ' Call the method
                iBidiRequest.GetResult(resultPointer)

                ' Get the value
                Dim result As Int32 = CType(Marshal.PtrToStructure(resultPointer, GetType(Int32)), Int32)

                If result = 0 Then
                    ' result equals 0 means success
                    ' Check if any data was returned. Note: dxp01sdk.strings.ENDJOB
                    ' and dxp01sdk.strings.PRINTER_ACTION do not return any values.

                    ' First allocate memory
                    countPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(countPointer))

                    ' Call the method
                    iBidiRequest.GetEnumCount(countPointer)

                    ' Get the value
                    Dim count As Int32 = CType(Marshal.PtrToStructure(countPointer, GetType(Int32)), Int32)
                    If count <> 0 Then
                        ' Driver sent some data. Now retrieve the data sent by the driver

                        ' First allocate memory for type and size
                        dataTypePointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(dataTypePointer))
                        sizePointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(sizePointer))

                        ' Now allocate memory for data. Also add level of indirection.
                        Dim ptrTemp As IntPtr = Nothing
                        pointerToDataPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(pointerToDataPointer))
                        Marshal.StructureToPtr(ptrTemp, pointerToDataPointer, False)

                        ' Finally, retrieve the data sent by the driver
                        Dim schema As String = String.Empty
                        iBidiRequest.GetOutputData(0, schema, dataTypePointer, pointerToDataPointer, sizePointer)

                        ' Get the size of the data in bytes returned from SendRecv method.
                        Dim size As Int32 = CType(Marshal.PtrToStructure(sizePointer, GetType(Int32)), Int32)

                        ' Get the data itself. First remove level of indirection
                        tempResultPointer = Marshal.ReadIntPtr(pointerToDataPointer)

                        ' Get Unicode string from pointer. Unicode characters are 16-bit characters. 
                        ' size contains the number of bytes returned by SendRecv method
                        xml = Marshal.PtrToStringUni(tempResultPointer, size \ 2)
                    End If
                End If
            Catch e As Exception
                Dim badPortValue As String = "check your printer port configuration, if it points to a local file"
                Console.WriteLine(e.Message)
                Console.WriteLine(badPortValue)
            Finally
                ' Free used memory
                Marshal.FreeCoTaskMem(dataPointer)
                Marshal.FreeCoTaskMem(sizePointer)
                Marshal.FreeCoTaskMem(dataTypePointer)
                Marshal.FreeCoTaskMem(pointerToDataPointer)
                Marshal.FreeCoTaskMem(tempResultPointer)
                Marshal.FreeCoTaskMem(countPointer)
                Marshal.FreeCoTaskMem(resultPointer)
            End Try
            Return xml
        End Function

        Public Function SendRecv(ByVal action As String, ByVal actionType As String, ByVal data As Byte()) As String 

            Dim dataPointer As IntPtr = Nothing
            Dim dataTypePointer As IntPtr = Nothing
            Dim sizePointer As IntPtr = Nothing
            Dim pointerToDataPointer As IntPtr = Nothing
            Dim tempResultPointer As IntPtr = Nothing
            Dim countPointer As IntPtr = Nothing
            Dim resultPointer As IntPtr = Nothing

            Dim xml As String = String.Empty

            Try
                Dim bidiRequest As BidiRequest = DirectCast(Activator.CreateInstance(GetType(BidiRequest), True), BidiRequest)
                Dim iBidiRequest As IBidiRequest = DirectCast(bidiRequest, IBidiRequest)

                iBidiRequest.SetSchema(action)

                ' Set the input data for the request, if any
                If data.Length <> 0 Then
                    Dim size As Integer = Marshal.SizeOf(data(0)) * data.Length
                    dataPointer = Marshal.AllocCoTaskMem(size)
                    Marshal.Copy(data, 0, dataPointer, data.Length)
                    iBidiRequest.SetInputData(CType(BIDI_TYPE.BIDI_BLOB, UInt32), dataPointer, CType(size, UInt32))
                End If

                ' Send request to driver
                Dim val As Int32 = _iBidiSpl.SendRecv(actionType, iBidiRequest)

                ' Check if request was a success
                resultPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(resultPointer))

                ' Call the method
                iBidiRequest.GetResult(resultPointer)

                ' Get the value
                Dim result As Int32 = CType(Marshal.PtrToStructure(resultPointer, GetType(Int32)), Int32)

                If result = 0 Then
                    ' result equals 0 means success
                    ' Check if any data was returned. Note: dxp01sdk.strings.ENDJOB
                    ' and dxp01sdk.strings.PRINTER_ACTION do not return any values.

                    ' First allocate memory
                    countPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(countPointer))

                    ' Call the method
                    iBidiRequest.GetEnumCount(countPointer)

                    ' Get the value
                    Dim count As Int32 = CType(Marshal.PtrToStructure(countPointer, GetType(Int32)), Int32)
                    If count <> 0 Then
                        ' Driver sent some data. Now retrieve the data sent by the driver

                        ' First allocate memory for type and size
                        dataTypePointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(dataTypePointer))
                        sizePointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(sizePointer))

                        ' Now allocate memory for data. Also add level of indirection.
                        Dim ptrTemp As IntPtr = Nothing
                        pointerToDataPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(pointerToDataPointer))
                        Marshal.StructureToPtr(ptrTemp, pointerToDataPointer, False)

                        ' Finally, retrieve the data sent by the driver
                        Dim schema As String = String.Empty
                        iBidiRequest.GetOutputData(0, schema, dataTypePointer, pointerToDataPointer, sizePointer)

                        ' Get the size of the data in bytes returned from SendRecv method.
                        Dim size As Int32 = CType(Marshal.PtrToStructure(sizePointer, GetType(Int32)), Int32)

                        ' Get the data itself. First remove level of indirection
                        tempResultPointer = Marshal.ReadIntPtr(pointerToDataPointer)

                        ' Get Unicode string from pointer. Unicode characters are 16-bit characters. 
                        ' size contains the number of bytes returned by SendRecv method
                        xml = Marshal.PtrToStringUni(tempResultPointer, size \ 2)
                    End If
                End If
            Catch e As Exception
                Dim badPortValue As String = "check your printer port configuration, if it points to a local file"
                Console.WriteLine(e.Message)
                Console.WriteLine(badPortValue)
            Finally
                ' Free used memory
                Marshal.FreeCoTaskMem(dataPointer)
                Marshal.FreeCoTaskMem(sizePointer)
                Marshal.FreeCoTaskMem(dataTypePointer)
                Marshal.FreeCoTaskMem(pointerToDataPointer)
                Marshal.FreeCoTaskMem(tempResultPointer)
                Marshal.FreeCoTaskMem(countPointer)
                Marshal.FreeCoTaskMem(resultPointer)
            End Try
            Return xml
        End Function

        ''' <summary>
        ''' Use the method to read data from the printer. The action parameter specifies 
        ''' the data to be read from the printer. This method returns the xml
        ''' data back.
        '''
        ''' Use the method below for reading magstripe data (dxp01sdk.strings.MAGSTRIPE_READ), 
        ''' getting printer messages (dxp01sdk.strings.PRINTER_MESSAGES) and 
        ''' for getting printer supplies status (dxp01sdk.strings.PRINTER_OPTIONS)
        ''' </summary>
        ''' <param name="action"></param>
        ''' <returns></returns>
        Public Function GetPrinterData(ByVal action As String) As String
            Dim result As String = SendRecv(action, BIDI_ACTION_GET, "")
            Return result
        End Function

        ''' <summary>
        ''' Use the method to read data from the printer. The action parameter specifies
        ''' the data to be read from the printer.  The user also has to specify the 
        ''' input data that is required. 
        ''' This method returns the xml data back.
        ''' 
        ''' Use the method below for getting job status for a given
        ''' printer job id (dxp01sdk.strings.JOB_STATUS)
        ''' </summary>
        ''' <param name="action"></param>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function GetPrinterData(ByVal action As String, ByVal input As String) As String
            Return SendRecv(action, BIDI_ACTION_GET, input)
        End Function
        Public Function GetPrinterData(ByVal action As String, ByVal input As Byte()) As String
            Return SendRecv(action, BIDI_ACTION_GET, input)
        End Function


        ''' <summary>
        ''' Use the method to send data to printer. The user has to specify the action
        ''' it wants to take in action parameter. 
        ''' This method returns the xml data back.
        ''' 
        ''' Use the method below for starting a job (dxp01sdk.strings.STARTJOB),
        ''' ending a job (dxp01sdk.strings.ENDJOB) and for parking the card
        ''' at smartcard station (dxp01sdk.strings.SMARTCARD_PARK)
        ''' </summary>
        ''' <param name="action"></param>
        ''' <returns></returns>
        Public Function SetPrinterData(ByVal action As String) As String
            Dim result As String = SendRecv(action, BIDI_ACTION_SET, "")
            Return result
        End Function

        ''' <summary>
        ''' Use the method to send data to printer. The user has to specify the action
        ''' it wants to take in action parameter. The also user has to specify the data
        ''' it wants to send in input parameter. 
        ''' This method returns the xml data back.
        ''' 
        ''' Use the method below for encoding magstripe(dxp01sdk.strings.MAGSTRIPE_ENCODE)
        ''' and for sending actions to printer like resume, cancel or 
        ''' restart (dxp01sdk.strings.PRINTER_ACTION)
        ''' </summary>
        ''' <param name="action"></param>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function SetPrinterData(ByVal action As String, ByVal input As String) As String
            Dim result As String = SendRecv(action, BIDI_ACTION_SET, input)
            Return result
        End Function
        Public Function SetPrinterData(ByVal action As String, ByVal input As Byte()) As String
            Dim result As String = SendRecv(action, BIDI_ACTION_SET, input)
            Return result
        End Function

    End Class

End Namespace
