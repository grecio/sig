''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Driver SDK loosely-coupled smartcard sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports System.Windows.Forms
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

Class Smartcard
    ' Escapes are top to area; left to area; width; height : card in portrait orientation
    Private Shared printBlockingEscape As String = "~PB%7.26 22.35 14.99 14.99?"
    Private Shared topcoatRemovalEscape As String = "~TR%7.26 22.35 14.99 14.99?"

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine(thisExeName & " demonstrates interactive mode parking a card in the smart card")
        Console.WriteLine("  station, moving the card from the station, and options to print and poll for")
        Console.WriteLine("  job completion.")
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
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer""")
        Console.WriteLine("  Parks a card in the printer smart card station, asks you to continue or")
        Console.WriteLine("  reject and then does what you requested.")
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

        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

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
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
    End Sub

    ''' <summary>
    ''' Simulate a smartcard personalization operation.
    '''
    ''' This is where calls to a third-party library would be used to communicate
    ''' with the chip.
    ''' </summary>
    ''' <returns></returns>
    Private Shared Sub PersonalizeSmartcard()
        Dim caption As String = "A card has been parked in smartcard module"
        Dim message As String = "This is where smartcard personalization happens." & Environment.NewLine & Environment.NewLine & "Press 'Yes' to simulate a successful smartcard personalization." & Environment.NewLine & Environment.NewLine & "Press 'No' to simulate a failed smartcard personalization."

        Dim result As DialogResult = MessageBox.Show(message, caption, MessageBoxButtons.YesNo)

        If result <> DialogResult.Yes Then
            Throw New Exception("PersonalizeSmartcard(): error occured.")
        End If
    End Sub
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

            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
            Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

            If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                Throw New Exception((commandLineOptions.printerName & " is not ready. status: ") + printerOptionsValues._printerStatus)
            End If

            If commandLineOptions.print AndAlso "Installed" <> printerOptionsValues._printHead Then
                Throw New Exception(commandLineOptions.printerName & " does not have a print head installed.")
            End If

            If "Installed" <> printerOptionsValues._optionSmartcard Then
                Dim msg As String = String.Format("{0} needs 'Installed' for smartcard option. '{1}' was returned.",
                    commandLineOptions.printerName, printerOptionsValues._optionSmartcard)
                Throw New Exception(msg)
            End If

            Dim hopperID As String = "1"
            Dim cardEjectSide As String = "default"
            printerJobID = Util.StartJob(bidiSpl,
                                         If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID),
                                         If((commandLineOptions.cardEjectSide.Length > 0), commandLineOptions.cardEjectSide, cardEjectSide))

            SmartcardPark(bidiSpl, commandLineOptions.parkBack)

            PersonalizeSmartcard()

            ResumeJob(bidiSpl, printerJobID, 0)

            If commandLineOptions.print Then
                If commandLineOptions.parkBack Then
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)
                Else
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName, printBlockingEscape, topcoatRemovalEscape)
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