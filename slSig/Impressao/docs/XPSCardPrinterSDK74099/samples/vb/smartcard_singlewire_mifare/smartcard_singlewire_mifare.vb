'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' XPS Driver SDK : smartcard_singlewire + mifare chip example
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports System.Text
Imports dxp01sdk
Imports dxp01sdk.mifare

Public Class CommandLineOptions
    Public printerName As String
    Public print As Boolean
    Public jobCompletion As Boolean
    Public parkBack As Boolean
End Class

Class smartcard_singlewire_mifare
    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.Write(thisExeName)
        Console.WriteLine(" demonstrates interactive mode parking a card")
        Console.WriteLine("  in the smart card station, performing single-wire smartcard functions,")
        Console.WriteLine("  moving the card from the station, and options to print and poll for job")
        Console.WriteLine("  completion.")
        Console.WriteLine()
        Console.WriteLine("  Uses hardcoded data for printing.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-p] [-c] [-b]")
        Console.WriteLine()
        Console.WriteLine("options:")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -p Print text and graphic on front side of card.")
        Console.WriteLine("  -b use back of card for smartcard chip.")
        Console.WriteLine("  -c Poll for job completion.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer""")
        Console.WriteLine("  Parks a card in the printer smart card station, connects to the MiFare chip")
        Console.WriteLine("  and performs some MiFare chip activities.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -p -c")
        Console.WriteLine("  Parks a card in printer smart card station, moves the card from the station,")
        Console.WriteLine("  prints simple text and graphics on the front side of the card, and polls and")
        Console.WriteLine("  displays job status.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions

        If args.Length = 0 Then
            Usage()
        End If

        Dim commandLineOptions As CommandLineOptions = New CommandLineOptions()
        Dim arguments As CommandLine.Utility.Arguments = New CommandLine.Utility.Arguments(args)

        If String.IsNullOrEmpty(arguments("n")) Then
            Console.WriteLine("printer name is required")
            Environment.Exit(-1)
        Else
            ' we might have a -n with no printer name:
            Dim boolVal As Boolean = False
            If Boolean.TryParse(arguments("n"), boolVal) Then
                Console.WriteLine("printer name is required")
                Environment.Exit(-1)
            End If
            commandLineOptions.printerName = arguments("n")
        End If

        commandLineOptions.print = Not String.IsNullOrEmpty(arguments("p"))
        commandLineOptions.parkBack = Not String.IsNullOrEmpty(arguments("b"))
        commandLineOptions.jobCompletion = Not String.IsNullOrEmpty(arguments("c"))

        Return commandLineOptions
    End Function

    Private Shared Sub ResumeJob(bidiSpl As BidiSplWrap, printerJobID As Integer, errorCode As Integer)
        Dim xmlFormat As String = strings.PRINTER_ACTION_XML
        Dim input As String = String.Format(xmlFormat, CInt(Actions.[Resume]), printerJobID, errorCode)
        Console.WriteLine("issuing Resume after smartcard:")
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
    End Sub

    Private Shared Function Bytes_to_HEX_String(bytes As Byte()) As String
        Dim sb As StringBuilder = New StringBuilder()
        For Each b As Byte In bytes
            sb.Append(String.Format("{0:X02} ", b))
        Next
        Return sb.ToString()
    End Function

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' SCardResultMessage()
    ' format a message for display.
    ' SCARD errors are declared in WinError.H
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Private Shared Function SCardResultMessage(scardResult As Long, message As String) As String
        Dim sb As StringBuilder = New StringBuilder()
        sb.Append(message)
        sb.Append(" result: ")
        sb.Append(scardResult.ToString())
        sb.Append("; ")
        sb.Append(Util.Win32ErrorString(CUInt(scardResult)))
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' optional routine
    ''' </summary>
    ''' <param name="protocol"></param>
    Private Shared Sub DisplayProtocols(protocol As UInteger)
        Dim protocols As String() = SCard.StringsFromProtocol(protocol)
        Console.Write("   protocol[s]: ")
        For index As Integer = 0 To protocols.Length - 1
            Console.Write(protocols(index) + " ")
        Next
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' get mifare status. see pg. 23, "CCID_Protocol_spec_120224_.pdf"
    ''' </summary>
    Private Shared Sub DisplayChipInfo(scard As SCard)
        Dim command As Mifare_Command = New Mifare_Command()
        Dim sendBytes As Byte() = command.CreateGetCardStatusCommand()
        Dim receivedBytes As Byte() = New Byte(-1) {}
        Dim scardResult As Long = scard.SCardTransmit(sendBytes, receivedBytes)
        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.WriteLine("get card status:")
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length)
            For index As Integer = 0 To receivedBytes.Length - 1
                Console.Write("{0:X02} ", receivedBytes(index))
            Next
            Console.WriteLine()
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes))
            Mifare_Common.CheckStatusCode(receivedBytes(0))
        End If
    End Sub

    ''' <summary>
    ''' load mifare keys
    ''' </summary>
    Private Shared Sub LoadKeys(scard As SCard, sector As Byte, block As Byte, keyType As Mifare_Command.KeyType)

        Dim command As Mifare_Command = New Mifare_Command()

        Dim sendBytes As Byte() = command.CreateLoadKeysCommand(keyType, sector, Mifare_Common.mifare_test_key)

        Dim receivedBytes As Byte() = New Byte(-1) {}
        Dim scardResult As Long = scard.SCardTransmit(sendBytes, receivedBytes)
        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.WriteLine("prep for write; load key status:")
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length)
            For index As Integer = 0 To receivedBytes.Length - 1
                Console.Write("{0:X02} ", receivedBytes(index))
            Next
            Console.WriteLine()
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes))
            Mifare_Common.CheckStatusCode(receivedBytes(0))
        Else
            Throw New Exception("prep for write: load keys fail")
        End If
    End Sub

    ''' <summary>
    ''' generate some sample data. here, just get a data/timesamp.
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function create_test_data_string() As String
        Dim testDataString As String = System.DateTime.Now.ToString("yy.mm.dd.hh.mm")
        Return testDataString
    End Function

    ''' <summary>
    ''' write some sample data to the given block and sector.
    ''' </summary>
    Private Shared Sub WriteData(scard As SCard, sector As Byte, block As Byte, keyType As Mifare_Command.KeyType, testDataBytes As Byte())

        Dim command As Mifare_Command = New Mifare_Command()
        Dim sendBytes As Byte() = command.CreateWriteBlockCommand(keyType, sector, block, testDataBytes)

        Dim receivedBytes As Byte() = New Byte(-1) {}
        Dim scardResult As Long = scard.SCardTransmit(sendBytes, receivedBytes)
        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.WriteLine("write data status:")
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length)
            For index As Integer = 0 To receivedBytes.Length - 1
                Console.Write("{0:X02} ", receivedBytes(index))
            Next
            Console.WriteLine()
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes))
            Mifare_Common.CheckStatusCode(receivedBytes(0))
        Else
            Throw New Exception("write data failed.")
        End If
    End Sub

    ''' <summary>
    ''' read some data from the given block and sector.
    ''' </summary>
    Private Shared Sub ReadData(scard As SCard, sector As Byte, block As Byte, keyType As Mifare_Command.KeyType)

        ' in this app, we've already performed a LoadKeys().

        Dim command As Mifare_Command = New Mifare_Command()
        Dim sendBytes As Byte() = command.CreateReadBlockCommand(keyType, sector, block)
        Dim receivedBytes As Byte() = New Byte(-1) {}
        Dim scardResult As Long = scard.SCardTransmit(sendBytes, receivedBytes)
        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.WriteLine(String.Format("read sector {0}, block {1} status:", sector, block))
            Dim apduResponse As MiFare_Response = New MiFare_Response(receivedBytes)
            If apduResponse.BlockHasNonzeroData() Then
                Console.Write("  {0} commandBytes received: ", receivedBytes.Length)
                For index As Integer = 0 To receivedBytes.Length - 1
                    Console.Write("{0:X02} ", receivedBytes(index))
                Next
                Console.WriteLine()
                Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes))
                Mifare_Common.CheckStatusCode(receivedBytes(0))
            Else
                Console.WriteLine("sector is all zeros")
            End If
        Else
            Throw New Exception(String.Format("read sector {0}, block {1} fail", sector, block))
        End If

        Dim readDataString As String = Encoding.ASCII.GetString(receivedBytes, 1, 16)
        Console.WriteLine(String.Format("data read from sector {0}, block {1}: '{2}'", sector, block, readDataString))
    End Sub

    ''' <summary>
    ''' issue the SCardStatus() call. this is important, because the SCardConnect()
    ''' call always succeeds - as long as the card is staged.
    '''
    ''' the SCardConnect() call returns the Answer-To-Reset bytes (ATR).
    ''' </summary>
    Private Shared Sub GetSCardStatus(scard As SCard)
        Dim states As Integer() = New Integer(-1) {}
        Dim protocol As UInteger = CUInt(scard_protocol.SCARD_PROTOCOL_DEFAULT)
        Dim ATRBytes As Byte() = New Byte(-1) {}
        Dim scardResult As Long = scard.SCardStatus(states, protocol, ATRBytes)

        Console.WriteLine("SCardStatus result: {0}: {1}", scardResult, Util.Win32ErrorString(CUInt(scardResult)))
        If CLng(scard_error.SCARD_S_SUCCESS) <> scardResult Then
            Dim msg As String = SCardResultMessage(scardResult, "")
            Throw New Exception("SCardStatus() fail: " & msg)
        End If

        ' display all the 'states' returned. if ANY of the states is SCARD_ABSENT -
        ' we are done with this card.
        Dim cardAbsent As Boolean = False
        Console.Write("SCardStatus() states: ")
        For index As Integer = 0 To states.Length - 1
            Console.Write(states(index))
            Console.Write(" ")
            Console.Write(SCard.StringFromState(states(index)))
            Console.Write(" ")

            If scard_state.SCARD_ABSENT = DirectCast(states(index), scard_state) Then
                cardAbsent = True
            End If
        Next
        Console.WriteLine()

        If cardAbsent Then
            Throw New Exception("one of the states is SCARD_ABSENT.")
        End If

        Console.Write("SCardStatus() ATR: ")
        Console.WriteLine(Bytes_to_HEX_String(ATRBytes))
    End Sub

    ''' <summary>
    ''' Demonstrate the single-wire smartcard calls. The wrapper class we consume
    ''' is similar to the Win32 ::Scard...() functions.
    ''' <returns>
    ''' throw an Exception if any error.
    ''' </returns>
    ''' </summary>
    Private Shared Sub PersonalizeSmartcard(bidiSpl As BidiSplWrap)

        Dim scard As SCard = New SCard(bidiSpl)
        Dim protocol As UInteger = CUInt(scard_protocol.SCARD_PROTOCOL_DEFAULT)
        Dim scardResult As Long = scard.SCardConnect(SCard.ChipConnection.contactless, protocol)

        Console.WriteLine("SCardConnect() result: {0}", scardResult)

        If CLng(scard_error.SCARD_S_SUCCESS) <> scardResult Then
            Dim msg As String = SCardResultMessage(scardResult, "")
            Throw New Exception("SCardConnect() fail: " & msg)
        End If

        GetSCardStatus(scard)

        DisplayChipInfo(scard)

        Dim sector As Byte = 5
        Dim block As Byte = 1
        Dim keytype As Mifare_Command.KeyType = Mifare_Command.KeyType.A

        LoadKeys(scard, sector, block, keytype)

        ReadData(scard, sector, block, keytype)

        Dim testDataString As String = create_test_data_string()
        Dim testDataBytes As Byte() = Encoding.ASCII.GetBytes(testDataString)
        Console.WriteLine("writing '{0}' to chip.", testDataString)

        WriteData(scard, sector, block, keytype, testDataBytes)

        ReadData(scard, sector, block, keytype)

        scardResult = scard.SCardDisConnect(CInt(scard_disposition.SCARD_LEAVE_CARD))
        Console.WriteLine("SCardDisConnect() result: {0}", scardResult)

        If CLng(scard_error.SCARD_S_SUCCESS) <> scardResult Then
            Dim msg As String = SCardResultMessage(scardResult, "")
            Throw New Exception("SCardDisConnect() fail: " & msg)
        End If
    End Sub

    Private Shared Sub SmartcardPark(bidiSpl As BidiSplWrap, parkBack As Boolean)
        Dim parkCommand As String = If(parkBack, strings.SMARTCARD_PARK_BACK, strings.SMARTCARD_PARK)
        Dim printerStatusXML As String = bidiSpl.SetPrinterData(parkCommand)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
        If 0 <> printerStatusValues._errorCode Then
            Throw New Exception("SmartcardPark(): " + printerStatusValues._errorString)
        End If
    End Sub

    Public Shared Sub Main(args As String())

        Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)

        Dim bidiSpl As BidiSplWrap = Nothing
        Dim printerJobID As Integer = 0

        Try
            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(commandLineOptions.printerName)

            ' optional:
            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            Dim printerOptionsXml As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
            Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXml)
            If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                Throw New Exception((commandLineOptions.printerName & " is not ready. status: ") + printerOptionsValues._printerStatus)
            End If

            If "Single wire" <> printerOptionsValues._optionSmartcard Then
                Dim msg As String = String.Format("{0} needs 'Single wire' for smartcard option. '{1}' was returned.",
                    commandLineOptions.printerName, printerOptionsValues._optionSmartcard)
                Throw New Exception(msg)
            End If

            Dim hopperID As String = String.Empty
            printerJobID = Util.StartJob(bidiSpl, hopperID)

            SmartcardPark(bidiSpl, commandLineOptions.parkBack)

            PersonalizeSmartcard(bidiSpl)

            ResumeJob(bidiSpl, printerJobID, 0)

            If commandLineOptions.print Then
                Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)
                Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName)
            End If

            bidiSpl.SetPrinterData(strings.ENDJOB)

            If commandLineOptions.jobCompletion Then
                Util.PollForJobCompletion(bidiSpl, printerJobID)
            End If
        Catch e As BidiException
            Console.WriteLine(e.Message)
            Util.CancelJob(bidiSpl, e.PrinterJobID, e.ErrorCode)
        Catch e As Exception
            Console.WriteLine(e.Message)
            If 0 <> printerJobID Then
                Util.CancelJob(bidiSpl, printerJobID, 0)
            End If
        Finally
            bidiSpl.UnbindDevice()
        End Try
    End Sub
End Class