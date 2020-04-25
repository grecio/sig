''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Driver SDK vb.net lamination barcode read sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports System.Windows.Forms
Imports dxp01sdk

Public Class CommandLineOptions
    Public printerName As String
    Public timeout As Integer
    Public fileName As String
    Public verify As Boolean
End Class

Class laminator_barcode
    Private Shared Sub usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine(thisExeName & " demonstrates interactive mode laminator bar code ")
        Console.WriteLine("read with options to verify the bar code data, and polls for job completion.")
        Console.WriteLine("Serialized overlay material must be loaded in the L1 laminator.")
        Console.WriteLine()
        Console.WriteLine("This sample uses the driver Printing Preferences/Lamination settings. Make sure")
        Console.WriteLine("that the L1 Laminate card setting is not set to ""Do not apply."" The sample")
        Console.WriteLine("always prints a card. To laminate the card without printing, select")
        Console.WriteLine("""Printing Preferences/Layout/Advanced"" and change ""Disable printing"" to ""All"".")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-h] [-v] [-t] <timeout> [-f] <filename>")
        Console.WriteLine()
        Console.WriteLine("options:")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -h Displays this information. <optional>")
        Console.WriteLine("  -v Verify the laminator bar code data. <optional>")
        Console.WriteLine("  -t <msec> Timeout to read bar code data. Default is no timeout. <optional>")
        Console.WriteLine("  -f <filename> Save the laminator bar code read results to a file. <optional>")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer""")
        Console.WriteLine("  Demonstrates interactive mode laminator bar code read with no timeout;")
        Console.WriteLine("  prints black text on one or both sides of the card; polls and displays job")
        Console.WriteLine("  status")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -v -f ""example.txt"" ")
        Console.WriteLine("  Demonstrates interactive mode laminator bar code read and verify with no")
        Console.WriteLine("  timeout; prints black text on one or both sides of the card; polls and")
        Console.WriteLine("  displays job status. It also writes the bar code data to file example.txt.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions

        If args.Length = 0 Then
            usage()
        End If

        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

        If Not String.IsNullOrEmpty(arguments("h")) Then
            usage()
        End If

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

        commandLineOptions.verify = Not String.IsNullOrEmpty(arguments("v"))

        If String.IsNullOrEmpty(arguments("t")) Then
            commandLineOptions.timeout = CInt(laminator_barcode_timout.INFINITE_WAIT)
        Else
            Dim successfullyConvertedString As Boolean = Int32.TryParse(arguments("t"), commandLineOptions.timeout)
            If Not successfullyConvertedString then
                Console.WriteLine("Cannot convert command line argument '{0}' to an Int32.", arguments("t"))
                Environment.Exit(-1)
            End If
        End If

        If Not String.IsNullOrEmpty(arguments("f")) Then
            commandLineOptions.fileName = arguments("f")
        End If

        Return commandLineOptions
    End Function

    Private Shared Sub ResumeJob(bidiSpl As BidiSplWrap, printerJobID As Integer, errorCode As Integer)
        Dim xmlFormat As String = strings.PRINTER_ACTION_XML
        Dim input As String = String.Format(xmlFormat, CInt(Actions.Resume), printerJobID, errorCode)
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
    End Sub

    Private Shared Function GetCurrentDateTimeString() As String
        Dim dateString As String = System.DateTime.Now.ToShortDateString()
        Dim timeString As String = System.DateTime.Now.ToShortTimeString()
        Dim currentDateTimeString As String = String.Format(dateString & " " & timeString & "      ")
        Return currentDateTimeString
    End Function

    Private Shared Sub WriteToFile(fileName As String, text As String)
        If Not String.IsNullOrEmpty(fileName) Then

            Dim lines As String = GetCurrentDateTimeString() & text
            Using file As New System.IO.StreamWriter(fileName, True)
                file.WriteLine(lines)
            End Using
        End If
    End Sub

    Private Shared Function IsBarcodeDataGood() As Boolean
        Dim caption As String = "A laminator bar code has been read."
        Dim message As String = "This is where laminator bar code verify happens." & vbCr & vbLf & vbCr & vbLf & "Select 'Yes' to continue the job. This simulates that the bar code data passed verification and you want to finish the card." & vbCr & vbLf & vbCr & vbLf & "Select 'No' to cancel the job. This simulates that the bar code data failed verification and you want to reject the card."

        Dim result As DialogResult = MessageBox.Show(message, caption, MessageBoxButtons.YesNo)
        Return (result = DialogResult.Yes)
    End Function

    Private Shared Sub SetLaminatorBarcodeActions(bidiSpl As BidiSplWrap, verify As Boolean)

        If verify Then
            bidiSpl.SetPrinterData(strings.LAMINATOR_BARCODE_READ_AND_VERIFY)
        Else
            bidiSpl.SetPrinterData(strings.LAMINATOR_BARCODE_READ)
        End If
    End Sub

    Private Shared Sub ReadLaminatorBarcode(bidiSpl As BidiSplWrap, printerjobid As Integer, timeout As Integer, verify As Boolean, fileName As String)

        Dim barcodejobXml As String = String.Format(strings.LAMINATOR_BARCODE_READ_XML, printerjobid, timeout)
        Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.LAMINATOR_BARCODE_READ_DATA, barcodejobXml)

        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
        If 516 = printerStatusValues._errorCode Then
            ' application did not provide long enough timeout for the bar code to
            ' be read. It is application's responsibility to call bar code read
            ' again.
            Console.WriteLine("BarcodeRead() fail; Printer Error: " + printerStatusValues._errorString)
            If Not String.IsNullOrEmpty(fileName) Then
                Dim text As String = String.Format(("Printer Job Id: " & printerjobid & "  ") + printerStatusValues._errorString)
                WriteToFile(fileName, text)
            End If
        ElseIf 517 = printerStatusValues._errorCode Then
            Console.WriteLine("BarcodeRead() fail; Printer Error: " + printerStatusValues._errorString)
        ElseIf 0 <> printerStatusValues._errorCode Then
            Dim message As String = String.Format("BarcodeRead() fail: " + printerStatusValues._errorString)
            Throw New BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode)
        ElseIf 0 = printerStatusValues._errorCode Then

            Dim barcodeLaminator As String = printerStatusValues._dataFromPrinter
            If Not String.IsNullOrEmpty(barcodeLaminator) Then
                Console.WriteLine("Bar code: " & barcodeLaminator)
                If Not String.IsNullOrEmpty(fileName) Then
                    Dim text As String = String.Format("Printer Job Id: " & printerjobid & ", bar code data: " & barcodeLaminator)
                    WriteToFile(fileName, text)
                End If
            Else
                Console.WriteLine("Error no data read.")
                If Not String.IsNullOrEmpty(fileName) Then
                    Dim text As String = String.Format("Printer Job Id: " & printerjobid & " - Error no data read.", printerjobid)
                    WriteToFile(fileName, text)
                End If
            End If

            If verify Then
                If IsBarcodeDataGood() Then
                    ResumeJob(bidiSpl, printerjobid, 0)
                Else
                    Dim message As String = String.Format("Barcode verify failed")
                    Throw New BidiException(message, printerStatusValues._printerJobID, 0)
                End If
            End If
        End If
    End Sub

    Private Shared Sub AddMagstripeReadOrEncodeOrSmarcardOperationsHere()
        ' Placeholder. Please refer to Magstripe and smartcard samples.
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Main()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Shared Sub Main(args As String())

        Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)

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

            If "None" = printerOptionsValues._laminator Then
                Throw New Exception(commandLineOptions.printerName & " does not have a laminator.")
            End If

            If "None" = printerOptionsValues._laminatorScanner Then
                Throw New Exception(commandLineOptions.printerName & " does not have a lamination barcode scanner.")
            End If

            Const hopperID As String = ""
            printerJobID = Util.StartJob(bidiSpl, hopperID)

            ' Notify the printer that a bar code read is part of the job.
            ' Note: serialized laminate must be installed in L1 laminator station.
            SetLaminatorBarcodeActions(bidiSpl, commandLineOptions.verify)

            ' Add code for magnetic stripe read, magnetic stripe encode and smart
            ' card operations here. Other samples show how to perform these
            ' operations.
            AddMagstripeReadOrEncodeOrSmarcardOperationsHere()

            ' Do some simple text printing. You MUST send print data to the driver
            ' to laminate. Print data is required as part of every lamination job
            ' because lamination actions are a printing preference.
            Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)

            ' NOTE: Although you must send print data to the driver to laminate,
            ' you can prevent printing on the card by disabling printing in the
            ' driver:
            ' 1) Select “Printing Preferences/Layout/Advanced and change “Disable
            '    printing to “All.” OR,
            ' 2) Programmatically “Disable printing" using print ticket. The Print
            '    sample demonstrates print ticket manipulation.

            ' Call EndJob to notify the printer that all the data has been sent for
            ' the card.
            bidiSpl.SetPrinterData(strings.ENDJOB)

            ' Read the bar code. This may take several minutes if the laminator is
            ' warming up. If the  timeout provided is too small the function will
            ' return before the bar code can be read. If you use a short timeout,
            ' call this function repeatedly.
            ReadLaminatorBarcode(bidiSpl, printerJobID, commandLineOptions.timeout, commandLineOptions.verify, commandLineOptions.fileName)

            ' Use job completion to monitor that the card was personalized
            ' successfully or failed to complete because of an error.
            Util.PollForJobCompletion(bidiSpl, printerJobID)
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
