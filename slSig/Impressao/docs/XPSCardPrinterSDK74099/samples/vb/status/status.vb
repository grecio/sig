''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Card Printer SDK: vb.net status sample
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System
Imports dxp01sdk
Imports System.IO
Imports System.Collections.Generic


Public Class CommandLineOptions
    Public printerName As String
    Public jobStatus As Boolean
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

Class status

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine()
        Console.WriteLine(thisExeName & " demonstrates using interactive mode to retrieve printer options,")
        Console.WriteLine("  printer messages, and job status.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-j] -i <input hopper>] [-f <side>]")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -j Optional. Submit a print job and display job status.")
        Console.WriteLine("  -f <side>  Flip card on output.")
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
        Dim boolVal As Boolean = False
        If Boolean.TryParse(arguments("n"), boolVal) Then
            Usage()
        End If
        commandLineOptions.printerName = arguments("n")

        If Not String.IsNullOrEmpty(arguments("j")) Then
            commandLineOptions.jobStatus = True
        End If

        commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
        commandLineOptions.cardEjectSide = If(String.IsNullOrEmpty(arguments("f")), String.Empty, arguments("f").ToLower())

        Return commandLineOptions
    End Function

    Private Shared Sub DisplayPrinterOptionsValues(vals As PrinterOptionsValues)
        Console.WriteLine("Options:")
        Console.WriteLine("  ColorPrintResolution:          " & Convert.ToString(vals._colorPrintResolution))
        Console.WriteLine("  ConnectionPortType:            " & Convert.ToString(vals._connectionPortType))
        Console.WriteLine("  ConnectionProtocol:            " & Convert.ToString(vals._connectionProtocol))
        Console.WriteLine("  EmbosserVersion:               " & Convert.ToString(vals._embosserVersion))
        Console.WriteLine("  Laminator:                     " & Convert.ToString(vals._laminator))
        Console.WriteLine("  LaminatorFirmwareVersion:      " & Convert.ToString(vals._laminatorFirmwareVersion))
        Console.WriteLine("  LaminatorImpresser:            " & Convert.ToString(vals._laminatorImpresser))
        Console.WriteLine("  LaminatorScanner:              " & Convert.ToString(vals._laminatorScanner))
        Console.WriteLine("  LaserFirmwareVersion:          " & Convert.ToString(vals._laserFirmwareVersion))
        Console.WriteLine("  LockState:                     " & Convert.ToString(vals._lockState))
        Console.WriteLine("  ModuleEmbosser:                " & Convert.ToString(vals._moduleEmbosser))
        Console.WriteLine("  MonochromePrintResolution:     " & Convert.ToString(vals._monochromePrintResolution))
        Console.WriteLine("  MultiHopperVersion:            " & Convert.ToString(vals._multiHopperVersion))
        Console.WriteLine("  TopcoatPrintResolution:        " & Convert.ToString(vals._topcoatPrintResolution))
        Console.WriteLine("  OptionDuplex:                  " & Convert.ToString(vals._optionDuplex))
        Console.WriteLine("  OptionInputhopper:             " & Convert.ToString(vals._optionInputhopper))
        Console.WriteLine("  OptionLaser:                   " & Convert.ToString(vals._optionLaser))
        Console.WriteLine("  OptionLaserVisionRegistration: " & Convert.ToString(vals._optionLaserVisionRegistration))
        Console.WriteLine("  OptionObscureBlackPanel:       " & Convert.ToString(vals._optionObscureBlackPanel))
        Console.WriteLine("  OptionLocks:                   " & Convert.ToString(vals._optionLocks))
        Console.WriteLine("  OptionMagstripe:               " & Convert.ToString(vals._optionMagstripe))
        Console.WriteLine("  OptionSecondaryMagstripeJIS:   " & Convert.ToString(vals._optionSecondaryMagstripeJIS))
        Console.WriteLine("  OptionPrinterBarcodeReader:    " & Convert.ToString(vals._optionPrinterBarcodeReader))
        Console.WriteLine("  OptionRewrite:                 " & Convert.ToString(vals._optionRewrite))
        Console.WriteLine("  OptionSmartcard:               " & Convert.ToString(vals._optionSmartcard))
        Console.WriteLine("  PrinterAddress:                " & Convert.ToString(vals._printerAddress))
        Console.WriteLine("  PrinterMessageNumber:          " & Convert.ToString(vals._printerMessageNumber))
        Console.WriteLine("  PrintEngineType:               " & Convert.ToString(vals._printEngineType))
        Console.WriteLine("  PrinterModel:                  " & Convert.ToString(vals._printerModel))
        Console.WriteLine("  PrinterSerialNumber:           " & Convert.ToString(vals._printerSerialNumber))
        Console.WriteLine("  PrinterStatus:                 " & Convert.ToString(vals._printerStatus))
        Console.WriteLine("  PrinterVersion:                " & Convert.ToString(vals._printerVersion))
        Console.WriteLine("  PrintHead:                     " & Convert.ToString(vals._printHead))
        Console.WriteLine()
    End Sub

    Private Shared Sub DisplayPrinterStatusValues(vals As PrinterStatusValues)
        Console.WriteLine("Status:")
        Console.WriteLine("  ClientID:      " & Convert.ToString(vals._clientID))
        Console.WriteLine("  ErrorCode:     " & Convert.ToString(vals._errorCode))
        Console.WriteLine("  ErrorSeverity: " & Convert.ToString(vals._errorSeverity))
        Console.WriteLine("  ErrorString:   " & Convert.ToString(vals._errorString))
        Console.WriteLine("  PrinterData:   " & Convert.ToString(vals._dataFromPrinter))
        Console.WriteLine("  PrinterJobID:  " & Convert.ToString(vals._printerJobID))
        Console.WriteLine("  WindowsJobID:  " & Convert.ToString(vals._windowsJobID))
        Console.WriteLine()
    End Sub

    Private Shared Sub DisplayPrinterCounterValues(vals As PrinterCounterStatus)
        Console.WriteLine("Counts:")
        Console.WriteLine("  CardsPickedSinceCleaningCard:  " & Convert.ToString(vals._cardsPickedSinceCleaningCard))
        Console.WriteLine("  CleaningCardsRun:              " & Convert.ToString(vals._cleaningCardsRun))
        Console.WriteLine("  CurrentCompleted:              " & Convert.ToString(vals._currentCompleted))
        Console.WriteLine("  CurrentLost:                   " & Convert.ToString(vals._currentLost))
        Console.WriteLine("  CurrentPicked:                 " & Convert.ToString(vals._currentPicked))
        Console.WriteLine("  CurrentPickedExceptionSlot:    " & Convert.ToString(vals._currentPickedExceptionSlot))
        Console.WriteLine("  CurrentPickedInputHopper1:     " & Convert.ToString(vals._currentPickedInputHopper1))
        Console.WriteLine("  CurrentPickedInputHopper2:     " & Convert.ToString(vals._currentPickedInputHopper2))
        Console.WriteLine("  CurrentPickedInputHopper3:     " & Convert.ToString(vals._currentPickedInputHopper3))
        Console.WriteLine("  CurrentPickedInputHopper4:     " & Convert.ToString(vals._currentPickedInputHopper4))
        Console.WriteLine("  CurrentPickedInputHopper5:     " & Convert.ToString(vals._currentPickedInputHopper5))
        Console.WriteLine("  CurrentPickedInputHopper6:     " & Convert.ToString(vals._currentPickedInputHopper6))
        Console.WriteLine("  CurrentRejected:               " & Convert.ToString(vals._currentRejected))
        Console.WriteLine("  PrinterStatus:                 " & Convert.ToString(vals._printerStatus))
        Console.WriteLine("  TotalCompleted:                " & Convert.ToString(vals._totalCompleted))
        Console.WriteLine("  TotalLost:                     " & Convert.ToString(vals._totalLost))
        Console.WriteLine("  TotalPicked:                   " & Convert.ToString(vals._totalPicked))
        Console.WriteLine("  TotalPickedExceptionSlot:      " & Convert.ToString(vals._totalPickedExceptionSlot))
        Console.WriteLine("  TotalPickedInputHopper1:       " & Convert.ToString(vals._totalPickedInputHopper1))
        Console.WriteLine("  TotalPickedInputHopper2:       " & Convert.ToString(vals._totalPickedInputHopper2))
        Console.WriteLine("  TotalPickedInputHopper3:       " & Convert.ToString(vals._totalPickedInputHopper3))
        Console.WriteLine("  TotalPickedInputHopper4:       " & Convert.ToString(vals._totalPickedInputHopper4))
        Console.WriteLine("  TotalPickedInputHopper5:       " & Convert.ToString(vals._totalPickedInputHopper5))
        Console.WriteLine("  TotalPickedInputHopper6:       " & Convert.ToString(vals._totalPickedInputHopper6))
        Console.WriteLine("  TotalRejected:                 " & Convert.ToString(vals._totalRejected))
        Console.WriteLine()
    End Sub

    Private Shared Sub DisplaySuppliesValues(vals As SuppliesValues)
        Console.WriteLine("Supplies:")
        Console.WriteLine("  IndentRibbon:               " & Convert.ToString(vals._indentRibbon))
        Console.WriteLine("  IndentRibbonLotCode:        " & Convert.ToString(vals._indentRibbonLotCode))
        Console.WriteLine("  IndentRibbonPartNumber:     " & Convert.ToString(vals._indentRibbonPartNumber))
        Console.WriteLine("  IndentRibbonRemaining:      " & Convert.ToString(vals._indentRibbonRemaining) & "%")
        Console.WriteLine("  IndentRibbonSerialNumber:   " & Convert.ToString(vals._indentRibbonSerialNumber))
        Console.WriteLine("  L1LaminateLotCode:          " & Convert.ToString(vals._laminatorL1LotCode))
        Console.WriteLine("  L1LaminatePartNumber:       " & Convert.ToString(vals._laminatorL1PartNumber))
        Console.WriteLine("  L1LaminateRemaining:        " & Convert.ToString(vals._laminatorL1PercentRemaining) & "%")
        Console.WriteLine("  L1LaminateSerialNumber:     " & Convert.ToString(vals._laminatorL1SerialNumber))
        Console.WriteLine("  L1LaminateType:             " & Convert.ToString(vals._laminatorL1SupplyCode))
        Console.WriteLine("  L2LaminateLotCode:          " & Convert.ToString(vals._laminatorL2LotCode))
        Console.WriteLine("  L2LaminatePartNumber:       " & Convert.ToString(vals._laminatorL2PartNumber))
        Console.WriteLine("  L2LaminateRemaining:        " & Convert.ToString(vals._laminatorL2PercentRemaining) & "%")
        Console.WriteLine("  L2LaminateSerialNumber:     " & Convert.ToString(vals._laminatorL2SerialNumber))
        Console.WriteLine("  L2LaminateType:             " & Convert.ToString(vals._laminatorL2SupplyCode))
        Console.WriteLine("  PrinterStatus:              " & Convert.ToString(vals._printerStatus))
        Console.WriteLine("  PrintRibbonLotCode:         " & Convert.ToString(vals._printRibbonLotCode))
        Console.WriteLine("  PrintRibbonPartNumber:      " & Convert.ToString(vals._printRibbonPartNumber))
        Console.WriteLine("  PrintRibbonSerialNumber:    " & Convert.ToString(vals._printRibbonSerialNumber))
        Console.WriteLine("  PrintRibbonType:            " & Convert.ToString(vals._printRibbonType))
        Console.WriteLine("  PrintRibbonRegionCode:      " & Convert.ToString(vals._printRibbonRegionCode))
        Console.WriteLine("  RibbonRemaining:            " & Convert.ToString(vals._ribbonRemaining) & "%")

        Console.WriteLine("  RetransferFilmLotCode:      " & Convert.ToString(vals._retransferFilmLotCode))
        Console.WriteLine("  RetransferFilmPartNumber:   " & Convert.ToString(vals._retransferFilmPartNumber))
        Console.WriteLine("  RetransferFilmRemaining:    " & Convert.ToString(vals._retransferFilmRemaining) & "%")
        Console.WriteLine("  RetransferFilmSerialNumber: " & Convert.ToString(vals._retransferFilmSerialNumber))

        Console.WriteLine("  TopperRibbonLotCode:        " & Convert.ToString(vals._topperRibbonLotCode))
        Console.WriteLine("  TopperRibbonPartNumber:     " & Convert.ToString(vals._topperRibbonPartNumber))
        Console.WriteLine("  TopperRibbonRemaining:      " & Convert.ToString(vals._topperRibbonRemaining) & "%")
        Console.WriteLine("  TopperRibbonSerialNumber:   " & Convert.ToString(vals._topperRibbonSerialNumber))
        Console.WriteLine("  TopperRibbonType:           " & Convert.ToString(vals._topperRibbonType))
        Console.WriteLine()
    End Sub



    Private Shared Sub DisplayHopperStatus(hopperInfoList As List(Of HOPPER_INFORMATION))
        Console.WriteLine()
        Console.WriteLine("HopperStatus:")
        Console.WriteLine("  Number of Hoppers:  " & hopperInfoList.Count)

        For i As Integer = 0 To hopperInfoList.Count - 1
            Console.WriteLine()
            Console.WriteLine("  Hopper Index:       " & i)
            Console.WriteLine("  Name:               " & hopperInfoList(i)._name)
            Console.WriteLine("  Type:               " & hopperInfoList(i)._type)
            Console.WriteLine("  Status:             " & hopperInfoList(i)._status)
            Console.WriteLine("  Card Stock:         " & hopperInfoList(i)._cardStock)
        Next
    End Sub


    Private Shared Sub CardJobWithStatus(bidiSpl As BidiSplWrap, commandLineOptions As CommandLineOptions, printerOptionsValues As PrinterOptionsValues)

        If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
            Throw New Exception(commandLineOptions.printerName & " is not ready. status: " & Convert.ToString(printerOptionsValues._printerStatus))
        End If

        ' do not check supplies
        Dim hopperID As String = "1"
        Dim cardEjectSide As String = "default"
        Dim printerJobID As Integer = Util.StartJob(bidiSpl, 
				If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID), 
				If((commandLineOptions.cardEjectSide.Length > 0), commandLineOptions.cardEjectSide, cardEjectSide))

        Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES)
        Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

        DisplayPrinterStatusValues(printerStatusValues)

        bidiSpl.SetPrinterData(strings.ENDJOB)

        Util.PollForJobCompletion(bidiSpl, printerJobID)
    End Sub

    Public Shared Sub Main(args As String())

        Dim commandLineOptions As CommandLineOptions = GetCommandlineOptions(args)
        commandLineOptions.Validate()

        Dim bidiSpl As BidiSplWrap = Nothing

        Try

            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(commandLineOptions.printerName)

            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
            DisplayPrinterStatusValues(printerStatusValues)

            Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
            Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)
            DisplayPrinterOptionsValues(printerOptionsValues)

            Dim printerCardCountXML As String = bidiSpl.GetPrinterData(strings.COUNTER_STATUS2)
            Dim printerCounterStatusValues As PrinterCounterStatus = Util.ParsePrinterCounterStatusXML(printerCardCountXML)
            DisplayPrinterCounterValues(printerCounterStatusValues)

            Dim suppliesXML As String = bidiSpl.GetPrinterData(strings.SUPPLIES_STATUS3)
            Dim suppliesValues As SuppliesValues = Util.ParseSuppliesXML(suppliesXML)
            DisplaySuppliesValues(suppliesValues)

            Dim hopperStatusXML As String = bidiSpl.GetPrinterData(strings.HOPPER_STATUS)
            Dim hopperInfoList As List(Of HOPPER_INFORMATION) = Util.ParseHopperStatusXml(hopperStatusXML)
            DisplayHopperStatus(hopperInfoList)

            If commandLineOptions.jobStatus OrElse (commandLineOptions.hopperID.Length > 0) OrElse (commandLineOptions.cardEjectSide.Length > 0) Then
                CardJobWithStatus(bidiSpl, commandLineOptions, printerOptionsValues)
            End If
        Catch e As Exception
            Console.WriteLine(e.Message)
        Finally
            bidiSpl.UnbindDevice()
        End Try
    End Sub
End Class
