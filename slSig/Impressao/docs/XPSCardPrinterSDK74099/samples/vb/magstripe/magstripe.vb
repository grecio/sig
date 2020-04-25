''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Driver SDK magstripe sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Diagnostics.Eventing.Reader
Imports System.IO
Imports dxp01sdk

Public Class CommandLineOptions
    Public printerName As String
    Public encodeMagstripe As Boolean
    Public readMagstripe As Boolean
    Public print As Boolean
    Public jobCompletion As Boolean
    Public jisRequest As Boolean
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

Class magstripe

    Private Shared Sub usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine(thisExeName & " demonstrates interactive mode magnetic stripe encoding with")
        Console.WriteLine("  options to read the magnetic stripe data, print, and poll for job")
        Console.WriteLine("  completion status.")
        Console.WriteLine()
        Console.WriteLine("Uses hardcoded data for magnetic stripe and printing.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-e] [-r] [-p] [-c] [-j] [-c] [-i <input hopper>] [-f <side>]")
        Console.WriteLine()
        Console.WriteLine("options:")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -e Encodes magnetic stripe data.")
        Console.WriteLine("  -r Reads back the encoded magnetic stripe data.")
        Console.WriteLine("  -p Print simple black text on front side of card. ")
        Console.WriteLine("  -j JIS magnetic. ")
        Console.WriteLine("  -c Poll for job completion.")
        Console.WriteLine("  -f <side>  Flip card on output.")
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -e -r")
        Console.WriteLine("  Encodes data on all three tracks of an ISO 3-track magnetic stripe on the")
        Console.WriteLine("  backside of a card, reads and displays it.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -r -p -c")
        Console.WriteLine("  Reads data on all three tracks of an ISO 3-track magnetic stripe on the")
        Console.WriteLine("  backside of a card and displays it,")
        Console.WriteLine("  prints black text on the front side of the card, and polls and")
        Console.WriteLine("  displays job status.")
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

        commandLineOptions.encodeMagstripe = Not String.IsNullOrEmpty(arguments("e"))
        commandLineOptions.readMagstripe = Not String.IsNullOrEmpty(arguments("r"))
        commandLineOptions.print = Not String.IsNullOrEmpty(arguments("p"))
        commandLineOptions.jobCompletion = Not String.IsNullOrEmpty(arguments("c"))
        commandLineOptions.jisRequest = Not String.IsNullOrEmpty(arguments("j"))
        commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
        commandLineOptions.cardEjectSide = If(String.IsNullOrEmpty(arguments("f")), String.Empty, arguments("f").ToLower())

        Return commandLineOptions
    End Function


    Private Shared Sub ResumeJob(bidiSpl As BidiSplWrap, printerJobID As Integer, errorCode As Integer)
        Dim xmlFormat As String = strings.PRINTER_ACTION_XML
        Dim input As String = String.Format(xmlFormat, CInt(Actions.Resume), printerJobID, errorCode)
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
    End Sub

    Public Shared Sub ReadMagstripe(bidiSpl As BidiSplWrap, jisRequest As Boolean)

        ' replace schema string MAGSTRIPE_READ to MAGSTRIPE_READ_FRONT for front side read
        Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.MAGSTRIPE_READ)

        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
        If 0 = printerStatusValues._errorCode Then
            Console.WriteLine("Magstripe data read successfully; printer job id: " + CStr(printerStatusValues._printerJobID))

            Dim track1 As String = ""
            Dim track2 As String = ""
            Dim track3 As String = ""

            Util.ParseMagstripeStrings(printerStatusXML, track1, track2, track3, jisRequest)

            If track1.Length <> 0 Then
                ' Convert the Base64 UUEncoded output.
                Dim binaryData As Byte() = System.Convert.FromBase64String(track1)
                Dim str As String = System.Text.Encoding.UTF8.GetString(binaryData)
                Console.WriteLine(" track1 Base64 decoded: " & str)
            End If

            If track2.Length <> 0 Then
                ' Convert the Base64 UUEncoded output.
                Dim binaryData As Byte() = System.Convert.FromBase64String(track2)
                Dim str As String = System.Text.Encoding.UTF8.GetString(binaryData)
                Console.WriteLine(" track2 Base64 decoded: " & str)
            End If

            If track3.Length <> 0 Then
                ' Convert the Base64 UUEncoded output.
                Dim binaryData As Byte() = System.Convert.FromBase64String(track3)
                Dim str As String = System.Text.Encoding.UTF8.GetString(binaryData)
                Console.WriteLine(" track3 Base64 decoded: " & str)
            End If
        Else
            Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
        End If
    End Sub

    Public Shared Sub EncodeMagstripe(bidiSpl As BidiSplWrap, jisRequest As Boolean)

        ' Hardcoded XML to encode all 3 tracks in IAT mode.
        ' track 1 = "TRACK1", track 2 = "1122", track 3 = "321"
        Dim trackDataXML As String = "<?xml version=""1.0"" encoding=""utf-8""?>" & "<magstripe>" & "<track number=""1""><base64Data>VFJBQ0sx</base64Data></track>" & "<track number=""2""><base64Data>MTEyMg==</base64Data></track>" & "<track number=""3""><base64Data>MzIx</base64Data></track>" & "</magstripe>"

        If jisRequest Then
            ' JIS only allows track 3 = "321"
            trackDataXML = "<?xml version=""1.0"" encoding=""utf-8""?>" & "<magstripe>" & "<track number=""1""><base64Data></base64Data></track>" & "<track number=""2""><base64Data></base64Data></track>" & "<track number=""3""><base64Data>MzIx</base64Data></track>" & "</magstripe>"
        End If

        ' replace schema string MAGSTRIPE_ENCODE to MAGSTRIPE_ENCODE_FRONT for front side encode
        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.MAGSTRIPE_ENCODE, trackDataXML)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

        If 0 <> printerStatusValues._errorCode Then
            Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
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
                Throw New Exception((commandLineOptions.printerName & " does not have a print head installed."))
            End If

            '  A printer may have a JIS magnetic stripe unit in addition to an ISO unit.
            '  In that case the option will be "ISO, JIS" so 'Contains' as opposed to 
            '  '=' must be used... 
            If commandLineOptions.jisRequest Then
                If Not printerOptionsValues._optionMagstripe.Contains("JIS") Then
                    Throw New Exception((commandLineOptions.printerName & " does not have a JIS magnetic stripe unit installed."))
                End If
            Else
                If Not printerOptionsValues._optionMagstripe.Contains("ISO") Then
                    Throw New Exception((commandLineOptions.printerName & " does not have an ISO magnetic stripe unit installed."))
                End If
            End If

            Dim hopperID As String = "1"
            Dim cardEjectSide As String = "default"
            printerJobID = Util.StartJob(bidiSpl,
                                         If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID),
                                         If((commandLineOptions.cardEjectSide.Length > 0), commandLineOptions.cardEjectSide, cardEjectSide))

            If commandLineOptions.encodeMagstripe Then
                EncodeMagstripe(bidiSpl, commandLineOptions.jisRequest)
            End If

            If commandLineOptions.readMagstripe Then
                ReadMagstripe(bidiSpl, commandLineOptions.jisRequest)
            End If

            ' Check if user wants print
            If commandLineOptions.print Then
                ' this also waits for the print spooling to finish:
                Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)
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
