''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Printer SDK: vb.net 'printer_control' sample
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports dxp01sdk

Public Enum Commands
    undefined
    cancelAllJobs
    restart
    resetCardCounts
    adjustColors
    defaultColors
End Enum

Public Class CommandLineOptions
    Public Sub New()
        command = Commands.undefined
    End Sub
    Public printerName As String
    Public command As Commands
End Class

Class control
    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine()
        Console.WriteLine(thisExeName & " controls the printer through the printer driver.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> <command>")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -c cancel all jobs: Send a command to the printer to clear any print job")
        Console.WriteLine("                    from the printer. This does not delete spooled Windows")
        Console.WriteLine("                    print jobs.")
        Console.WriteLine("  -r restart the printer.")
        Console.WriteLine("  -e reset all card counts.")
        Console.WriteLine("  -a set colorAdjust settings. This sample changes only the Red settings. Blue and Green are unchanged")
        Console.WriteLine("  -d set default colorAdjust settings. This sample only resets Red and Green.  Blue is unchanged.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions
        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

        If String.IsNullOrEmpty(arguments("n")) Then
            Usage()
        End If

        ' check for -n with no printer name:
        Dim boolVal As Boolean = False
        If Boolean.TryParse(arguments("n"), boolVal) Then
            Usage()
        End If
        commandLineOptions.printerName = arguments("n")

        If Not String.IsNullOrEmpty(arguments("c")) Then
            commandLineOptions.command = Commands.cancelAllJobs
        End If

        If Not String.IsNullOrEmpty(arguments("r")) Then
            commandLineOptions.command = Commands.restart
        End If

        If Not String.IsNullOrEmpty(arguments("e")) Then
            commandLineOptions.command = Commands.resetCardCounts
        End If

        If Not String.IsNullOrEmpty(arguments("a")) Then
            commandLineOptions.command = Commands.adjustColors
        End If

        If Not String.IsNullOrEmpty(arguments("d")) Then
            commandLineOptions.command = Commands.defaultColors
        End If

        If Commands.undefined = commandLineOptions.command Then
            Usage()
        End If

        Return commandLineOptions
    End Function


    Public Shared Sub adjustColors(bidiSpl As BidiSplWrap)
        ' create the XML and convert to bytes for the upcoming SetInputData() call.

        Dim formattedColorForRChannel As [String] = String.Format(strings.COLOR_CHANNEL, "1", "2", "-1", "0", "25",
        "6", "-19", "12", "13", "14", "15")

        Dim adjustColorXML As [String] = String.Format(strings.ADJUST_COLOR_XML, formattedColorForRChannel, "", "")

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.ADJUST_COLOR, adjustColorXML)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

        If 0 <> printerStatusValues._errorCode Then
            Console.WriteLine("Request Not supported - error={0}", printerStatusValues._errorCode)
            Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
        End If
    End Sub

    Public Shared Sub SetDefaultColors(bidiSpl As BidiSplWrap)
        ' create the XML and convert to bytes for the upcoming SetInputData() call.
        ' Setting red and green color channel values back to defaults and keeping blue unchanged
        Dim defaultColorXML As [String] = String.Format(strings.DEFAULT_COLOR_XML, "true", "true", "false")

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.SET_DEFAULT_COLOR, defaultColorXML)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

        If 0 <> printerStatusValues._errorCode Then
            Console.WriteLine("Request Not supported - error={0}", printerStatusValues._errorCode)
            Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
        End If
    End Sub



    Public Shared Function isColorAdjustCapable(bidiSpl As BidiSplWrap) As Boolean

        Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
        Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

        Console.WriteLine("printer state: {0}", printerOptionsValues._printerStatus)

        ' verify that the printer is in the READY state
        If "Ready" <> printerOptionsValues._printerStatus Then
            Throw New Exception("ERROR:  printer is not in state 'READY'.")
        End If

        Dim strPrinterVersion As String = printerOptionsValues._printerVersion
        Dim actualFirmware As FirmwareVersion = Util.ParseFirmwareRev(strPrinterVersion)
        Console.WriteLine("Printer Version = {0}", strPrinterVersion)

        If (actualFirmware._printerBase = "D3") AndAlso (actualFirmware._majorVersion > 16) Then
            If actualFirmware._majorVersion > 17 Then
                Return True
            ElseIf (actualFirmware._majorVersion = 17) AndAlso (actualFirmware._minorVersion >= 4) Then
                Return True
            End If
        End If

        Throw New Exception("ERROR - Requires printer version: D3.17.4 (or greater)")
    End Function


    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Main()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Shared Sub Main(args As String())
        Try
            Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)

            Dim bidiSpl As New BidiSplWrap()
            bidiSpl.BindDevice(commandLineOptions.printerName)

            Select Case commandLineOptions.command
                Case Commands.cancelAllJobs
                    ' using zero for the printerJobID and the errorCode causes all
                    ' printerJobs to be canceled:
                    Util.CancelJob(bidiSpl, 0, 0)
                    Console.WriteLine("all jobs canceled for '{0}'", commandLineOptions.printerName)
                    Exit Select

                Case Commands.restart
                    Dim result As String = bidiSpl.SetPrinterData(strings.RESTART_PRINTER)
                    Console.WriteLine("command issued to restart the printer.")
                    Exit Select

                Case Commands.resetCardCounts
                    Dim result As String = bidiSpl.SetPrinterData(strings.RESET_CARD_COUNTS)
                    Console.WriteLine("command issued to reset the printer's card counts.")
                    Exit Select

                Case Commands.adjustColors
                    Console.WriteLine("AdjustColors - begin.")
                    If isColorAdjustCapable(bidiSpl) Then
                        adjustColors(bidiSpl)
                    End If
                    Console.WriteLine("AdjustColors - end.")
                    Exit Select

                Case Commands.defaultColors
                    Console.WriteLine("SetDefaultColors - begin.")
                    If isColorAdjustCapable(bidiSpl) Then
                        SetDefaultColors(bidiSpl)
                    End If
                    Console.WriteLine("SetDefaultColors - end.")
                    Exit Select

                Case Else
                    Console.WriteLine("unexpected commandline option.")
                    Exit Select

            End Select

            bidiSpl.UnbindDevice()

        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Class
