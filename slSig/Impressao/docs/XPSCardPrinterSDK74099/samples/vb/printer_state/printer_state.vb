''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Printer SDK: vb.net 'printer_state' sample
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports printer_state.dxp01sdk

Public Class CommandLineOptions
    Public printerState As String = String.Empty
    Public printerName As String = String.Empty
End Class

Class printer_state

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine()
        Console.WriteLine(thisExeName & " demonstrates changing the printer state to offline, online, or suspended.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> -s <on | off | suspend>")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -s <on | off | suspend>. Required. Changes printer to the specified state: ")
        Console.WriteLine("     'on' changes printer state to online.")
        Console.WriteLine("     'off' changes printer state to offline.")
        Console.WriteLine("     'suspend' changes printer state to suspended.")
        Console.WriteLine()
        Console.WriteLine("  " & thisExeName & " -n ""XPS Card Printer"" -s on")
        Console.WriteLine("  Changes the printer state to online.")
        Console.WriteLine()
        Console.WriteLine("  " & thisExeName & " -n ""XPS Card Printer"" -s off")
        Console.WriteLine("  Changes the printer state to offline.")
        Console.WriteLine()
        Console.WriteLine("  " & thisExeName & " -n ""XPS Card Printer"" -s suspend")
        Console.WriteLine("  Changes the printer state to suspended.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions
        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

        If Not arguments.ContainsKey("n") Then
            Usage()
        End If

        If arguments("n") = "true" Then
            Usage()
        Else
            ' parser inserts "true" for missing args:
            commandLineOptions.printerName = arguments("n")
        End If

        If Not arguments.ContainsKey("s") Then
            Usage()
        End If

        If arguments("s") = "true" Then
            Usage()
        Else
            ' parser inserts "true" for missing args:
            commandLineOptions.printerState = arguments("s").ToLower()
        End If

        Select Case commandLineOptions.printerState
            Case "on", "off", "suspend"
                'do nothing
            Case Else
                Usage()
        End Select

        Return commandLineOptions
    End Function

    Private Shared Sub CheckStatus(printerStatusXml As String, fnName As String)
        Dim printerStatus As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXml)
        If printerStatus._errorCode <> 0 Then
            Dim message As String = fnName & " failed. Status XML: " & Environment.NewLine & printerStatusXml & Environment.NewLine
            Throw New Exception(message)
        End If
    End Sub


    ' ------------------------------------------------------------
    '  Function GetPrinterState()
    ' ASSUMPTION:
    '      It has already been determined that printerState 
    '      is a valid state.
    ' ------------------------------------------------------------
    Private Shared Function GetPrinterState(printerState As String) As Integer
        Dim intPrinterState As Integer
        Select Case printerState
            Case "off"
                intPrinterState = dxp01sdk.PrinterState.Offline
            Case "on"
                intPrinterState = dxp01sdk.PrinterState.Online
            Case Else
                intPrinterState = dxp01sdk.PrinterState.Suspended
        End Select
        Return intPrinterState
    End Function

    Private Shared Sub ChangePrinterState(
        bidiSpl As BidiSplWrap,
        printerState As Integer)
        Dim formattedPrinterStateXML As String = String.Format(strings.CHANGE_PRINTER_STATE_XML, printerState)

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.CHANGE_PRINTER_STATE, formattedPrinterStateXML)
        CheckStatus(printerStatusXML, "ChangePrinterState")
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Main()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Shared Sub Main(args As String())

        Dim options As CommandLineOptions = GetCommandlineOptions(args)

        Dim bidiSpl As BidiSplWrap = Nothing

        Try

            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(options.printerName)

            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            Dim printerState As Integer = GetPrinterState(options.printerState)
            ChangePrinterState(bidiSpl, printerState)
        Catch e As Exception
            Console.WriteLine(e.Message)
        Finally
            bidiSpl.UnbindDevice()
        End Try
    End Sub
End Class
