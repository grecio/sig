''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Printer Driver SDK: Single-wire smartcard personalization vb.net sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports System.Text
Imports dxp01sdk

Public Class CommandLineOptions
    Public printerName As String
    Public print As Boolean
    Public jobCompletion As Boolean
    Public parkBack As Boolean
    Public hopperID As String = ""
    Public cardEjectSide As String = ""

    Public Sub Validate()
        If hopperID <> "" Then

            If hopperID <> "1" AndAlso hopperID <> "2" AndAlso hopperID <> "3" AndAlso hopperID <> "4" AndAlso hopperID <> "5" AndAlso hopperID <> "6" AndAlso hopperID <> "exception" Then
                Console.WriteLine("invalid hopperID: {0}", hopperID)
                Environment.[Exit](-1)
            End If
        End If

        If cardEjectSide <> "" Then

            If cardEjectSide <> "front" AndAlso cardEjectSide <> "back" Then
                Console.WriteLine("invalid cardEjectSide: {0}", cardEjectSide)
                Environment.[Exit](-1)
            End If
        End If
    End Sub
End Class

Class smartcard_singlewire
    Enum CardType
        contact
        contactless
        undetermined
    End Enum

    ' Escapes are top to area; left to area; width; height : card in portrait orientation
    Private Shared printBlockingEscape As String = "~PB%7.26 22.35 14.99 14.99?"
    Private Shared topcoatRemovalEscape As String = "~TR%7.26 22.35 14.99 14.99?"

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.Write(thisExeName)
        Console.WriteLine(" demonstrates interactive mode parking a card in the")
        Console.WriteLine("  smart card station, performing single-wire smartcard functions, moving")
        Console.WriteLine("  the card from the station, and options to print and poll for job completion.")
        Console.WriteLine()
        Console.WriteLine("Uses hardcoded data for printing.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-p] [-b] [-c] [-i <input hopper>] [-f <side>]")
        Console.WriteLine()
        Console.WriteLine("options:")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -p Print text and graphic on front side of card.")
        Console.WriteLine("  -b use back of card for smartcard chip.")
        Console.WriteLine("  -c Poll for job completion.")
        Console.WriteLine("  -f <side>  Flip card on output.")
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -p -c")
        Console.WriteLine("  Parks a card in printer smart card station, moves the card from the station,")
        Console.WriteLine("  prints simple text and graphics on the front side of the card, and polls and")
        Console.WriteLine("  displays job status.")
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
        commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
        commandLineOptions.cardEjectSide = If(String.IsNullOrEmpty(arguments("f")), String.Empty, arguments("f").ToLower())

        Return commandLineOptions
    End Function

    Private Shared Sub ResumeJob(bidiSpl As BidiSplWrap, printerJobID As Integer, errorCode As Integer)
        Dim xmlFormat As String = strings.PRINTER_ACTION_XML
        Dim input As String = String.Format(xmlFormat, CInt(Actions.[Resume]), printerJobID, errorCode)
        Console.WriteLine("issuing Resume after smartcard:")
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
    End Sub

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

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''' <summary>
    ''' a card is in the printer and stated in the smartcard module. The
    ''' SCardConnect() call has succeeded.
    ''' talk to the reader and the chip (contact or contactless).
    ''' </summary>
    ''' <param name="scard">an instance of our SCard wrapper class.</param>
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Private Shared Function PersonalizeConnectedChip(scard__1 As SCard) As Boolean
        Dim states As Integer() = New Integer(-1) {}
        Dim protocol As UInteger = CUInt(scard_protocol.SCARD_PROTOCOL_UNDEFINED)
        Dim ATRBytes As Byte() = New Byte(-1) {}

        Dim scardResult As Long = scard__1.SCardStatus(states, protocol, ATRBytes)

        Console.WriteLine("SCardStatus result: {0}: {1}", scardResult, Util.Win32ErrorString(CUInt(scardResult)))

        If CLng(scard_error.SCARD_S_SUCCESS) <> scardResult Then
            ' unable to get a valid SCardStatus() response; we're done.
            Return False
        End If

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
            ' one of the states is SCARD_ABSENT; we're done.
            Return False
        End If

        Console.Write("SCardStatus() ATRBytes (hex): ")
        For index As Integer = 0 To ATRBytes.Length - 1
            Console.Write("{0:X02} ", ATRBytes(index))
        Next
        Console.WriteLine()

        Dim attributesToTry As scard_attr() = New scard_attr() {scard_attr.SCARD_ATTR_VENDOR_NAME, scard_attr.SCARD_ATTR_VENDOR_IFD_SERIAL_NO, scard_attr.SCARD_ATTR_VENDOR_IFD_TYPE, scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION}

        For attribIndex As Integer = 0 To attributesToTry.Length - 1
            Dim scardAttributeBytes As Byte() = New Byte(-1) {}

            scardResult = scard__1.SCardGetAttrib(attributesToTry(attribIndex), scardAttributeBytes)

            If CLng(scard_error.SCARD_S_SUCCESS) <> scardResult Then
                Continue For
            End If

            Dim msg As String = String.Format("SCardGetAttrib({0})", SCard.StringFromAttr(CInt(attributesToTry(attribIndex))))
            Console.WriteLine(SCardResultMessage(scardResult, msg))

            Select Case attributesToTry(attribIndex)
                Case scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION
                    ' vendor-supplied interface device version: DWORD in the form 0xMMmmbbbb where
                    '      MM = major version;
                    '      mm = minor version; and
                    '      bbbb = build number:
                    Dim byteCount As Integer = scardAttributeBytes.Length
                    Dim minorVersion As Integer = If(byteCount > 0, scardAttributeBytes(0), 0)
                    Dim majorVersion As Integer = If(byteCount > 1, scardAttributeBytes(1), 0)
                    Dim buildNumber As Integer = 0
                    If byteCount > 3 Then
                        buildNumber = (scardAttributeBytes(2) << 8) + scardAttributeBytes(3)
                    End If

                    Console.WriteLine("  SCARD_ATTR_VENDOR_IFD_VERSION:")
                    Console.WriteLine("    major: " & majorVersion.ToString())
                    Console.WriteLine("    minor: " & minorVersion.ToString())
                    Console.WriteLine("    build: " & buildNumber.ToString())
                    Exit Select
                Case Else

                    ' null-terminate the returned byte for string display.
                    Dim bytesList As List(Of Byte) = New List(Of Byte)(scardAttributeBytes)
                    bytesList.Add(0)
                    scardAttributeBytes = bytesList.ToArray()
                    Console.WriteLine(ASCIIEncoding.UTF8.GetString(scardAttributeBytes))
                    Exit Select
            End Select
        Next

        ' create a bytes for the upcoming SCardTransmit() method.
        ' these particular bytes should function with this type of contact chip:
        '
        '      MPCOS-EMV 16k
        '      GEMPLUS
        '      Datacard part number 806062-002
        '
        Dim sendBytes As Byte() = New Byte() {&H0, &HA4, &H0, &H0}
        Console.Write("sending bytes : ")
        For index As Integer = 0 To sendBytes.Length - 1
            Console.Write("{0:X02} ", sendBytes(index))
        Next
        Console.WriteLine()

        ' for those bytes, we should receive 0x61, 0x12
        Dim receivedBytes As Byte() = New Byte(-1) {}

        scardResult = scard__1.SCardTransmit(sendBytes, receivedBytes)

        Console.WriteLine("SCardTransmit result: {0}", scardResult)

        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.Write("  {0} bytes received: ", receivedBytes.Length)
            For index As Integer = 0 To receivedBytes.Length - 1
                Console.Write("{0:X02} ", receivedBytes(index))
            Next
            Console.WriteLine()
        End If

        ' send the bytes       0x00  0xC0  0x00  0x00  0x12:
        sendBytes = New Byte() {&H0, &HC0, &H0, &H0, &H12}
        Console.Write("sending bytes : ")
        For index As Integer = 0 To sendBytes.Length - 1
            Console.Write("{0:X02} ", sendBytes(index))
        Next
        Console.WriteLine()

        ' for those bytes, we should receive
        '  0x85, 0x10, 0x80, 0x01, 0x3F, 0x00, 0x38, 0x00, 0x00, 0x00,
        '  0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x6B, 0x90, 0x00
        receivedBytes = New Byte(-1) {}

        scardResult = scard__1.SCardTransmit(sendBytes, receivedBytes)

        Console.WriteLine("SCardTransmit result: {0}", scardResult)

        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult AndAlso receivedBytes.Length > 0 Then
            Console.Write("  {0} bytes received: ", receivedBytes.Length)
            For index As Integer = 0 To receivedBytes.Length - 1
                Console.Write("{0:X02} ", receivedBytes(index))
            Next
            Console.WriteLine()
        End If
        Return True
    End Function

    ''' <summary>
    ''' Demonstrate the single-wire smartcard calls. The wrapper class we consume
    ''' is similar to the Win32 ::Scard...() functions.
    ''' </summary>
    Private Shared Function PersonalizeSmartcard(bidiSpl As BidiSplWrap) As CardType

        Dim scard As SCard = New SCard(bidiSpl)
        Dim contactPersonalized As Boolean = False
        Dim contactlessPersonalized As Boolean = False
        Dim cardType As CardType = cardType.undetermined

        ' try a contacted chip:
        Dim protocol As UInteger = CUInt(scard_protocol.SCARD_PROTOCOL_DEFAULT)

        Dim scardResult As Long = scard.SCardConnect(scard.ChipConnection.contact, protocol)
        Console.WriteLine("SCardConnect(contact): {0}: {1}", scardResult, Util.Win32ErrorString(CUInt(scardResult)))

        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult Then
            cardType = cardType.contact
            Dim protocols As String() = scard.StringsFromProtocol(protocol)
            Console.Write("   protocol[s]: ")
            For index As Integer = 0 To protocols.Length - 1
                Console.Write(protocols(index) + " ")
            Next
            Console.WriteLine()

            contactPersonalized = PersonalizeConnectedChip(scard)

            scardResult = scard.SCardDisConnect(CInt(scard_disposition.SCARD_LEAVE_CARD))

            Console.WriteLine("SCardDisConnect() result: {0}", scardResult)
        End If

        ' try a contactless chip:
        protocol = CUInt(scard_protocol.SCARD_PROTOCOL_DEFAULT)

        scardResult = scard.SCardConnect(scard.ChipConnection.contactless, protocol)
        Console.WriteLine("SCardConnect(contactless): {0}: {1}", scardResult, Util.Win32ErrorString(CUInt(scardResult)))

        If CLng(scard_error.SCARD_S_SUCCESS) = scardResult Then
            cardType = cardType.contactless
            Dim protocols As String() = scard.StringsFromProtocol(protocol)
            Console.Write("   protocol[s]: ")
            For index As Integer = 0 To protocols.Length - 1
                Console.Write(protocols(index) + " ")
            Next
            Console.WriteLine()

            contactlessPersonalized = PersonalizeConnectedChip(scard)

            scardResult = scard.SCardDisConnect(CInt(scard_disposition.SCARD_LEAVE_CARD))

            Console.WriteLine("SCardDisConnect() result: {0}", scardResult)
        End If

        If Not contactPersonalized AndAlso Not contactlessPersonalized Then
            Throw New Exception("neither contact nor contactless chip personalization succeeded. canceling job.")
        End If
        Return cardType
    End Function

    Private Shared Sub SmartcardPark(bidiSpl As BidiSplWrap, parkBack As Boolean)
        Dim parkCommand As String = If(parkBack, strings.SMARTCARD_PARK_BACK, strings.SMARTCARD_PARK)
        Dim printerStatusXML As String = bidiSpl.SetPrinterData(parkCommand)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
        If 0 <> printerStatusValues._errorCode Then
            Throw New BidiException("SmartcardPark() fail" + printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
        End If
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Main()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Shared Sub Main(args As String())

        Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)
        commandLineOptions.Validate()

        Dim bidiSpl As BidiSplWrap = Nothing
        Dim printerJobID As Integer = 0

        Try
            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(commandLineOptions.printerName)

            ' optional:
            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            ' see if the printer is in the Printer_Ready state:
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

            If commandLineOptions.print AndAlso "Installed" <> printerOptionsValues._printHead Then
                Throw New Exception(commandLineOptions.printerName & " does not have a print head installed.")
            End If

            Dim hopperID As String = "1"
            Dim cardEjectSide As String = "default"
            printerJobID = Util.StartJob(bidiSpl,
                                         If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID),
                                         If((commandLineOptions.cardEjectSide.Length > 0), commandLineOptions.cardEjectSide, cardEjectSide))

            SmartcardPark(bidiSpl, commandLineOptions.parkBack)

            Dim cardType As CardType = PersonalizeSmartcard(bidiSpl)

            ResumeJob(bidiSpl, printerJobID, 0)

            If commandLineOptions.print Then
                If cardType = cardType.contact AndAlso commandLineOptions.parkBack <> True Then
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName, printBlockingEscape, topcoatRemovalEscape)
                Else
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)
                End If
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