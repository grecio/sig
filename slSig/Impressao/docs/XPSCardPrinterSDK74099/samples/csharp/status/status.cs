////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Card Printer SDK: csharp status sample
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using dxp01sdk;
using System.Collections.Generic;

public class CommandLineOptions {
    public string printerName;
    public bool jobStatus;
    public string hopperID = "";
    public string cardEjectSide = "";

    public void Validate() {
        // if hopperID is an empty string, that is OK
        if (hopperID != "") {
            if (
                hopperID != "1" &&
                hopperID != "2" &&
                hopperID != "3" &&
                hopperID != "4" &&
                hopperID != "5" &&
                hopperID != "6" &&
                hopperID != "exception") {
                Console.WriteLine("invalid hopperID: {0}", hopperID);
                Environment.Exit(-1);
            }
        }

        // if cardEjectSide is an empty string, that is OK
        if (cardEjectSide != "") {
            if (cardEjectSide != "front" && cardEjectSide != "back") {
                Console.WriteLine("invalid cardEjectSide: {0}", cardEjectSide);
                Environment.Exit(-1);
            }
        }
    }

}

internal class status {

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine();
        Console.WriteLine(thisExeName + " demonstrates using interactive mode to retrieve printer options,");
        Console.WriteLine("  printer messages, and job status.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-j][-i <input hopper>] [-f <side>]");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -j Optional. Submit a print job and display job status.");
        Console.WriteLine("  -f <side>  Flip card on output.");
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
        Console.WriteLine();
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        if (args.Length == 0) Usage();

        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (string.IsNullOrEmpty(arguments["n"])) { Usage(); }
        bool boolVal = false;
        if (Boolean.TryParse(arguments["n"], out boolVal)) { Usage(); }
        commandLineOptions.printerName = arguments["n"];

        if (!string.IsNullOrEmpty(arguments["j"])) {
            commandLineOptions.jobStatus = true;
        }

        commandLineOptions.hopperID =
             string.IsNullOrEmpty(arguments["i"]) ? string.Empty : arguments["i"].ToLower();
        commandLineOptions.cardEjectSide =
            string.IsNullOrEmpty(arguments["f"]) ? string.Empty : arguments["f"].ToLower();

        return commandLineOptions;
    }

    private static void DisplayPrinterOptionsValues(PrinterOptionsValues vals) {
        Console.WriteLine("Options:");
        Console.WriteLine("  ColorPrintResolution:          " + vals._colorPrintResolution);
        Console.WriteLine("  ConnectionPortType:            " + vals._connectionPortType);
        Console.WriteLine("  ConnectionProtocol:            " + vals._connectionProtocol);
        Console.WriteLine("  EmbosserVersion:               " + vals._embosserVersion);
        Console.WriteLine("  Laminator:                     " + vals._laminator);
        Console.WriteLine("  LaminatorFirmwareVersion:      " + vals._laminatorFirmwareVersion);
        Console.WriteLine("  LaminatorImpresser:            " + vals._laminatorImpresser);
        Console.WriteLine("  LaminatorScanner:              " + vals._laminatorScanner);
        Console.WriteLine("  LaserFirmwareVersion:          " + vals._laserFirmwareVersion);
        Console.WriteLine("  LockState:                     " + vals._lockState);
        Console.WriteLine("  ModuleEmbosser:                " + vals._moduleEmbosser);
        Console.WriteLine("  MonochromePrintResolution:     " + vals._monochromePrintResolution);
        Console.WriteLine("  MultiHopperVersion:            " + vals._multiHopperVersion);
        Console.WriteLine("  TopcoatPrintResolution:        " + vals._topcoatPrintResolution);
        Console.WriteLine("  OptionDuplex:                  " + vals._optionDuplex);
        Console.WriteLine("  OptionInputhopper:             " + vals._optionInputhopper);
        Console.WriteLine("  OptionLaser:                   " + vals._optionLaser);
        Console.WriteLine("  OptionLaserVisionRegistration: " + vals._optionLaserVisionRegistration);
        Console.WriteLine("  OptionObscureBlackPanel:       " + vals._optionObscureBlackPanel);
        Console.WriteLine("  OptionLocks:                   " + vals._optionLocks);
        Console.WriteLine("  OptionMagstripe:               " + vals._optionMagstripe);
        Console.WriteLine("  OptionSecondaryMagstripeJIS:   " + vals._optionSecondaryMagstripeJIS);
        Console.WriteLine("  OptionPrinterBarcodeReader:    " + vals._optionPrinterBarcodeReader);
        Console.WriteLine("  OptionRewrite:                 " + vals._optionRewrite);
        Console.WriteLine("  OptionSmartcard:               " + vals._optionSmartcard);
        Console.WriteLine("  PrinterAddress:                " + vals._printerAddress);
        Console.WriteLine("  PrintEngineType:               " + vals._printEngineType);
        Console.WriteLine("  PrinterMessageNumber:          " + vals._printerMessageNumber);
        Console.WriteLine("  PrinterModel:                  " + vals._printerModel);
        Console.WriteLine("  PrinterSerialNumber:           " + vals._printerSerialNumber);
        Console.WriteLine("  PrinterStatus:                 " + vals._printerStatus);
        Console.WriteLine("  PrinterVersion:                " + vals._printerVersion);
        Console.WriteLine("  PrintHead:                     " + vals._printHead);
        Console.WriteLine();
    }

    private static void DisplayPrinterStatusValues(PrinterStatusValues vals) {
        Console.WriteLine("Status:");
        Console.WriteLine("  ClientID:      " + vals._clientID);
        Console.WriteLine("  ErrorCode:     " + vals._errorCode);
        Console.WriteLine("  ErrorSeverity: " + vals._errorSeverity);
        Console.WriteLine("  ErrorString:   " + vals._errorString);
        Console.WriteLine("  PrinterData:   " + vals._dataFromPrinter);
        Console.WriteLine("  PrinterJobID:  " + vals._printerJobID);
        Console.WriteLine("  WindowsJobID:  " + vals._windowsJobID);
        Console.WriteLine();
    }

    private static void DisplayPrinterCounterValues(PrinterCounterStatus vals) {
        Console.WriteLine("Counts:");
        Console.WriteLine("  CardsPickedSinceCleaningCard:  " + vals._cardsPickedSinceCleaningCard);
        Console.WriteLine("  CleaningCardsRun:              " + vals._cleaningCardsRun);
        Console.WriteLine("  CurrentCompleted:              " + vals._currentCompleted);
        Console.WriteLine("  CurrentLost:                   " + vals._currentLost);
        Console.WriteLine("  CurrentPicked:                 " + vals._currentPicked);
        Console.WriteLine("  CurrentPickedExceptionSlot:    " + vals._currentPickedExceptionSlot);
        Console.WriteLine("  CurrentPickedInputHopper1:     " + vals._currentPickedInputHopper1);
        Console.WriteLine("  CurrentPickedInputHopper2:     " + vals._currentPickedInputHopper2);
        Console.WriteLine("  CurrentPickedInputHopper3:     " + vals._currentPickedInputHopper3);
        Console.WriteLine("  CurrentPickedInputHopper4:     " + vals._currentPickedInputHopper4);
        Console.WriteLine("  CurrentPickedInputHopper5:     " + vals._currentPickedInputHopper5);
        Console.WriteLine("  CurrentPickedInputHopper6:     " + vals._currentPickedInputHopper6);
        Console.WriteLine("  CurrentRejected:               " + vals._currentRejected);
        Console.WriteLine("  PrinterStatus:                 " + vals._printerStatus);
        Console.WriteLine("  TotalCompleted:                " + vals._totalCompleted);
        Console.WriteLine("  TotalLost:                     " + vals._totalLost);
        Console.WriteLine("  TotalPicked:                   " + vals._totalPicked);
        Console.WriteLine("  TotalPickedExceptionSlot:      " + vals._totalPickedExceptionSlot);
        Console.WriteLine("  TotalPickedInputHopper1:       " + vals._totalPickedInputHopper1);
        Console.WriteLine("  TotalPickedInputHopper2:       " + vals._totalPickedInputHopper2);
        Console.WriteLine("  TotalPickedInputHopper3:       " + vals._totalPickedInputHopper3);
        Console.WriteLine("  TotalPickedInputHopper4:       " + vals._totalPickedInputHopper4);
        Console.WriteLine("  TotalPickedInputHopper5:       " + vals._totalPickedInputHopper5);
        Console.WriteLine("  TotalPickedInputHopper6:       " + vals._totalPickedInputHopper6);
        Console.WriteLine("  TotalRejected:                 " + vals._totalRejected);
        Console.WriteLine();
    }

    private static void DisplaySuppliesValues(SuppliesValues vals) {
        Console.WriteLine("Supplies:");
        Console.WriteLine("  IndentRibbon:               " + vals._indentRibbon);
        Console.WriteLine("  IndentRibbonLotCode:        " + vals._indentRibbonLotCode);
        Console.WriteLine("  IndentRibbonPartNumber:     " + vals._indentRibbonPartNumber);
        Console.WriteLine("  IndentRibbonRemaining:      " + vals._indentRibbonRemaining + "%");
        Console.WriteLine("  IndentRibbonSerialNumber:   " + vals._indentRibbonSerialNumber);
        Console.WriteLine("  L1LaminateLotCode:          " + vals._laminatorL1LotCode);
        Console.WriteLine("  L1LaminatePartNumber:       " + vals._laminatorL1PartNumber);
        Console.WriteLine("  L1LaminateRemaining:        " + vals._laminatorL1PercentRemaining + "%");
        Console.WriteLine("  L1LaminateSerialNumber:     " + vals._laminatorL1SerialNumber);
        Console.WriteLine("  L1LaminateType:             " + vals._laminatorL1SupplyCode);
        Console.WriteLine("  L2LaminateLotCode:          " + vals._laminatorL2LotCode);
        Console.WriteLine("  L2LaminatePartNumber:       " + vals._laminatorL2PartNumber);
        Console.WriteLine("  L2LaminateRemaining:        " + vals._laminatorL2PercentRemaining + "%");
        Console.WriteLine("  L2LaminateSerialNumber:     " + vals._laminatorL2SerialNumber);
        Console.WriteLine("  L2LaminateType:             " + vals._laminatorL2SupplyCode);
        Console.WriteLine("  PrinterStatus:              " + vals._printerStatus);
        Console.WriteLine("  PrintRibbonLotCode:         " + vals._printRibbonLotCode);
        Console.WriteLine("  PrintRibbonPartNumber:      " + vals._printRibbonPartNumber);
        Console.WriteLine("  PrintRibbonSerialNumber:    " + vals._printRibbonSerialNumber);
        Console.WriteLine("  PrintRibbonType:            " + vals._printRibbonType);
        Console.WriteLine("  PrintRibbonRegionCode:      " + vals._printRibbonRegionCode);
        Console.WriteLine("  RibbonRemaining:            " + vals._ribbonRemaining + "%");

        Console.WriteLine("  RetransferFilmLotCode:      " + vals._retransferFilmLotCode);
        Console.WriteLine("  RetransferFilmPartNumber:   " + vals._retransferFilmPartNumber);
        Console.WriteLine("  RetransferFilmRemaining:    " + vals._retransferFilmRemaining + "%");
        Console.WriteLine("  RetransferFilmSerialNumber: " + vals._retransferFilmSerialNumber);

        Console.WriteLine("  TopperRibbonLotCode:        " + vals._topperRibbonLotCode);
        Console.WriteLine("  TopperRibbonPartNumber:     " + vals._topperRibbonPartNumber);
        Console.WriteLine("  TopperRibbonRemaining:      " + vals._topperRibbonRemaining + "%");
        Console.WriteLine("  TopperRibbonSerialNumber:   " + vals._topperRibbonSerialNumber);
        Console.WriteLine("  TopperRibbonType:           " + vals._topperRibbonType);
        Console.WriteLine();
    }

    private static void DisplayHopperStatus(List<HOPPER_INFORMATION> hopperInfoList)
    {
        Console.WriteLine();
        Console.WriteLine("HopperStatus:");
             Console.WriteLine("  Number of Hoppers:  " + hopperInfoList.Count);

        for (int i = 0; i<hopperInfoList.Count; i++) {
            Console.WriteLine();
            Console.WriteLine( "  Hopper Index:       " + i);
            Console.WriteLine( "  Name:               " + hopperInfoList[i]._name);
            Console.WriteLine( "  Type:               " + hopperInfoList[i]._type);
            Console.WriteLine( "  Status:             " + hopperInfoList[i]._status);
            Console.WriteLine( "  Card Stock:         " + hopperInfoList[i]._cardStock);
        }
    }


    private static void CardJobWithStatus(
        BidiSplWrap bidiSpl,
        CommandLineOptions commandLineOptions,
        PrinterOptionsValues printerOptionsValues) {
        if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
            throw new Exception(
                commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
        }

        string hopperID = "1";
        string cardEjectSide = "default";

        int printerJobID = Util.StartJob(
            bidiSpl,
            (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID,
            (commandLineOptions.cardEjectSide.Length > 0) ? commandLineOptions.cardEjectSide : cardEjectSide);


        string printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

        DisplayPrinterStatusValues(printerStatusValues);

        bidiSpl.SetPrinterData(strings.ENDJOB);

        Util.PollForJobCompletion(bidiSpl, printerJobID);
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions commandLineOptions = GetCommandlineOptions(args);
        commandLineOptions.Validate();

        BidiSplWrap bidiSpl = null;

        try {
            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);

            string printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
            DisplayPrinterStatusValues(printerStatusValues);

            string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
            PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);
            DisplayPrinterOptionsValues(printerOptionsValues);

            string printerCardCountXML = bidiSpl.GetPrinterData(strings.COUNTER_STATUS2);
            PrinterCounterStatus printerCounterStatusValues = Util.ParsePrinterCounterStatusXML(printerCardCountXML);
            DisplayPrinterCounterValues(printerCounterStatusValues);

            string suppliesXML = bidiSpl.GetPrinterData(strings.SUPPLIES_STATUS3);
            SuppliesValues suppliesValues = Util.ParseSuppliesXML(suppliesXML);
            DisplaySuppliesValues(suppliesValues);

            string hopperStatusXML = bidiSpl.GetPrinterData(strings.HOPPER_STATUS);
            List<HOPPER_INFORMATION> hopperInfoList = Util.ParseHopperStatusXml(hopperStatusXML);
            DisplayHopperStatus(hopperInfoList);

            if (commandLineOptions.jobStatus || 
                commandLineOptions.hopperID.Length > 0 ||
                commandLineOptions.cardEjectSide.Length > 0) {
                CardJobWithStatus(
                    bidiSpl,
                    commandLineOptions,
                    printerOptionsValues);
            }
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        finally {
            bidiSpl.UnbindDevice();
        }
    }
}