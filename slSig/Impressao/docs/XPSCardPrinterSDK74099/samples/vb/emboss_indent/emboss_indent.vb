''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Card Printer SDK vb.net 'emboss_indent' sample
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Printing
Imports System.Xml
Imports dxp01sdk

Class CommandLineOptions
    Public printerName As String = String.Empty
    'Public inputHopper As String = "1"
    Public pollForJobCompletion As Boolean = False
    Public displayPrintTicket As Boolean = False
    Public disableTopping As Boolean = False
    Public hopperID As String = ""

    Public Sub Validate()
        If hopperID <> "" Then

            If hopperID <> "1" AndAlso hopperID <> "2" AndAlso hopperID <> "3" AndAlso hopperID <> "4" AndAlso hopperID <> "5" AndAlso hopperID <> "6" AndAlso hopperID <> "exception" Then
                Console.WriteLine("invalid hopperID: {0}", hopperID)
                Environment.[Exit](-1)
            End If
        End If
    End Sub
End Class

Class EmbossIndenntDocument : Inherits PrintDocument
    Private _commandLineOptions As CommandLineOptions
    Public Sub New(commandLineOptions As CommandLineOptions)
        _commandLineOptions = commandLineOptions
        DocumentName = "emboss indent document"
        PrinterSettings.PrinterName = commandLineOptions.printerName
        PrintController = New StandardPrintController()
        AddHandler PrintPage, New PrintPageEventHandler(AddressOf OnPrintPage)
        AddHandler BeginPrint, New PrintEventHandler(AddressOf OnBeginPrint)
    End Sub

    Public Sub OnBeginPrint(sender As Object, printEventArgs As PrintEventArgs)
        ' prepare the PrintTicket for the entire print job.
        Dim printQueue As New PrintQueue(New LocalPrintServer(), PrinterSettings.PrinterName)
        Dim deltaPrintTicket As New PrintTicket()
        deltaPrintTicket.Duplexing = Duplexing.OneSided
        deltaPrintTicket.CopyCount = 1
        deltaPrintTicket.PageOrientation = PageOrientation.Landscape

        Dim validationResult As ValidationResult = printQueue.MergeAndValidatePrintTicket(printQueue.UserPrintTicket, deltaPrintTicket)

        Dim xmlString As String = PrintTicketXml.Prefix

        xmlString += If(_commandLineOptions.disableTopping, PrintTicketXml.ToppingOff, PrintTicketXml.ToppingOn)

        xmlString += PrintTicketXml.Suffix

        ' prepare to merge our PrintTicket xml into an actual PrintTicket:
        Dim xmlDocument As New XmlDocument()
        xmlDocument.LoadXml(xmlString)
        Dim memoryStream As New MemoryStream()
        xmlDocument.Save(memoryStream)
        memoryStream.Position = 0
        deltaPrintTicket = New PrintTicket(memoryStream)

        validationResult = printQueue.MergeAndValidatePrintTicket(validationResult.ValidatedPrintTicket, deltaPrintTicket)

        printQueue.UserPrintTicket = validationResult.ValidatedPrintTicket

        If _commandLineOptions.displayPrintTicket Then
            Util.DisplayPrintTicket(validationResult.ValidatedPrintTicket)
        End If

        ' IMPORTANT: this Commit() call sets the driver's 'Printing Preferences'
        ' on this machine:
        printQueue.Commit()
    End Sub

    Private Overloads Sub OnPrintPage(sender As Object, pageEventArgs As PrintPageEventArgs)
        Dim text As String = String.Empty

        ' emit all emboss + indent data driver escape strings. the font and 
        ' brush are not significant; just needed for the DrawString() call.
        Using font As New Font("Arial", 8)
            Using brush As New SolidBrush(Color.Black)
                text = "~EM%1;301;860;Font 11111"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%2;1600;860;222222"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%3;301;1460;333333"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%4;301;1180;444444"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%5;301;690;555555"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%6;1600;690;666666"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%7;301;650;777777"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%8;301;1000;888888"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%9;301;1050;999999"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)

                text = "~EM%10;1600;1050;10 10 10"
                pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50)
            End Using
        End Using
    End Sub
End Class

Class emboss_indent

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine(thisExeName & " demonstrates the emboss and indent escapes, input hopper")
        Console.WriteLine("selection, and the checking of print ribbon and emboss supplies.")
        Console.WriteLine()
        Console.WriteLine("Emboss and indent data is hardcoded.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-i <input hopper>]  [-d] [-x] [-c] [-i <input hopper>]")
        Console.WriteLine()
        Console.WriteLine("options:")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -i <input hopper>. Optional. Defaults to ""1"".")
        Console.WriteLine("  -d Disable topping.")
        Console.WriteLine("  -x Display the print ticket data. Default is no display.")
        Console.WriteLine("  -c Poll for job completion. Optional. Defaults to false.")
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions

        If args.Length = 0 Then
            Usage()
        End If

        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

        If String.IsNullOrEmpty(arguments("n")) Then
            Usage()
        End If
        ' we might have a -n with no printer name:
        Dim boolVal As Boolean = False
        If Boolean.TryParse(arguments("n"), boolVal) Then
            Usage()
        End If
        commandLineOptions.printerName = arguments("n")

        'If Not String.IsNullOrEmpty(arguments("i")) Then
        '    ' we might have a -i with no input hopper:
        '    If Boolean.TryParse(arguments("i"), boolVal) Then
        '        Usage()
        '    End If
        '    commandLineOptions.inputHopper = arguments("i")
        'End If

        commandLineOptions.disableTopping = Not String.IsNullOrEmpty(arguments("d"))
        commandLineOptions.displayPrintTicket = Not String.IsNullOrEmpty(arguments("x"))
        commandLineOptions.pollForJobCompletion = Not String.IsNullOrEmpty(arguments("c"))
        commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())

        Return commandLineOptions
    End Function

    Public Shared Sub Main(args As String())

        Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)
        commandLineOptions.Validate()

        Dim bidiSpl As BidiSplWrap = Nothing
        Dim printerJobID As Integer = 0

        Try

            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(commandLineOptions.printerName)

            Dim driverVersion As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine("driver version: " & Util.ParseDriverVersionXML(driverVersion) & Environment.NewLine)

            Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
            Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

            If "Installed" <> printerOptionsValues._moduleEmbosser Then
                Throw New Exception(commandLineOptions.printerName & ": embosser is not present.")
            End If

            If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                Throw New Exception((commandLineOptions.printerName & " is not ready. status: ") + printerOptionsValues._printerStatus)
            End If

            Dim hopperID As String = "1"
            printerJobID = Util.StartJob(bidiSpl,
                                         If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID))

            Dim printDocument As EmbossIndenntDocument = New EmbossIndenntDocument(commandLineOptions)
            printDocument.Print()

            Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName)
            bidiSpl.SetPrinterData(strings.ENDJOB)

            If commandLineOptions.pollForJobCompletion Then
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
