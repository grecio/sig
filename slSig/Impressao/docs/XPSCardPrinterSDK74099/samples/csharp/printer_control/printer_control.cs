////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer SDK: csharp 'printer_control' sample
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using dxp01sdk;

public enum Commands {
    undefined,
    cancelAllJobs,
    restart,
    resetCardCounts,
    adjustColors,
    defaultColors
}

public class CommandLineOptions {

    public CommandLineOptions() {
        command = Commands.undefined;
    }

    public string printerName;
    public Commands command;
}

internal class control {

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine();
        Console.WriteLine(thisExeName + " controls the printer through the printer driver.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> <command>");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -c cancel all jobs: Send a command to the printer to clear any print job");
        Console.WriteLine("                    from the printer. This does not delete spooled Windows");
        Console.WriteLine("                    print jobs.");
        Console.WriteLine("  -r restart the printer.");
        Console.WriteLine("  -e reset all card counts.");
        Console.WriteLine("  -a set colorAdjust settings. This sample changes only the Red settings. Blue and Green are unchanged");
        Console.WriteLine("  -d set default colorAdjust settings. This sample only resets Red and Green.  Blue is unchanged.");
        Console.WriteLine();
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (string.IsNullOrEmpty(arguments["n"])) {
            Usage();
        }

        // check for -n with no printer name:
        bool boolVal = false;
        if (Boolean.TryParse(arguments["n"], out boolVal)) {
            Usage();
        }
        commandLineOptions.printerName = arguments["n"];

        if (!string.IsNullOrEmpty(arguments["c"])) {
            commandLineOptions.command = Commands.cancelAllJobs;
        }

        if (!string.IsNullOrEmpty(arguments["r"])) {
            commandLineOptions.command = Commands.restart;
        }

        if (!string.IsNullOrEmpty(arguments["e"])) {
            commandLineOptions.command = Commands.resetCardCounts;
        }

        if (!string.IsNullOrEmpty(arguments["a"])) {
            commandLineOptions.command = Commands.adjustColors;
        }

        if (!string.IsNullOrEmpty(arguments["d"])) {
            commandLineOptions.command = Commands.defaultColors;
        }

        if (Commands.undefined == commandLineOptions.command) {
            Usage();
        }

        return commandLineOptions;
    }




    public static void adjustColors(BidiSplWrap bidiSpl) {
        // create the XML and convert to bytes for the upcoming SetInputData() call.

        String formattedColorForRChannel = string.Format(strings.COLOR_CHANNEL,
            "1", "2", "-1", "0", "25", "6", "-19", "12", "13", "14", "15");

        String adjustColorXML = string.Format( strings.ADJUST_COLOR_XML, formattedColorForRChannel, "", "");

        string printerStatusXML = bidiSpl.SetPrinterData(strings.ADJUST_COLOR, adjustColorXML);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

        if (0 != printerStatusValues._errorCode) {
            Console.WriteLine("Request Not supported - error={0}", printerStatusValues._errorCode );
            throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }
    } 


    public static void SetDefaultColors(BidiSplWrap bidiSpl) {

        // create the XML and convert to bytes for the upcoming SetInputData() call.
        // Setting red and green color channel values back to defaults and keeping blue unchanged
        String defaultColorXML = string.Format(strings.DEFAULT_COLOR_XML, "true", "true", "false");

        string printerStatusXML = bidiSpl.SetPrinterData(strings.SET_DEFAULT_COLOR, defaultColorXML);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

        if (0 != printerStatusValues._errorCode) {
            Console.WriteLine("Request Not supported - error={0}", printerStatusValues._errorCode);
            throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }
    }


    ////////////////////////////////////////////////////////////////////////////////
    // isColorAdjustCapable()
    //
    // Returns true if printer is in READY state and firmware is >= D3.17.4
    // otherwise returns false.
    //
    ////////////////////////////////////////////////////////////////////////////////
    public static bool isColorAdjustCapable(BidiSplWrap bidiSpl) {

        string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
        PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

        Console.WriteLine("printer state: {0}", printerOptionsValues._printerStatus);

        // verify that the printer is in the READY state
        if ("Ready" != printerOptionsValues._printerStatus) {
            throw new Exception("ERROR:  printer is not in state 'READY'.");
        }

        string strPrinterVersion = printerOptionsValues._printerVersion;
        FirmwareVersion actualFirmware = Util.ParseFirmwareRev(strPrinterVersion);
        Console.WriteLine("Printer Version = {0}", strPrinterVersion);

        if ((actualFirmware._printerBase == "D3") && (actualFirmware._majorVersion > 16)) {
            if (actualFirmware._majorVersion > 17)
                return true;
            else if ((actualFirmware._majorVersion == 17) && (actualFirmware._minorVersion >= 4))
                return true;
        }

        throw new Exception("ERROR - Requires printer version: D3.17.4 (or greater)");
    }

    ////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {

        try {
            CommandLineOptions commandLineOptions = GetCommandlineOptions(args);

            BidiSplWrap bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            switch (commandLineOptions.command) {
                case Commands.cancelAllJobs:
                    // using zero for the printerJobID and the errorCode causes all
                    // printerJobs to be canceled:
                    Util.CancelJob(bidiSpl, 0, 0);
                    Console.WriteLine("all jobs canceled for '{0}'", commandLineOptions.printerName);
                    break;

                case Commands.restart:
                    bidiSpl.SetPrinterData(strings.RESTART_PRINTER);
                    Console.WriteLine("command issued to restart the printer.");
                    break;

                case Commands.resetCardCounts:
                    bidiSpl.SetPrinterData(strings.RESET_CARD_COUNTS);
                    Console.WriteLine("command issued to reset the printer's card counts.");
                    break;

                case Commands.adjustColors:
                    Console.WriteLine("AdjustColors - begin.");
                    if (isColorAdjustCapable(bidiSpl)) {
                        adjustColors(bidiSpl);
                    }
                    Console.WriteLine("AdjustColors - end.");
                    break;

                case Commands.defaultColors:
                    Console.WriteLine("SetDefaultColors - begin.");
                    if (isColorAdjustCapable(bidiSpl)) {
                        SetDefaultColors(bidiSpl);
                    }
                    Console.WriteLine("SetDefaultColors - end.");
                    break;

                default:
                    Console.WriteLine("unexpected commandline option.");
                    break;
            }

            bidiSpl.UnbindDevice();
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }
}