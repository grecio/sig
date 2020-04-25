////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK loosely-coupled smartcard sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Windows.Forms;
using dxp01sdk;

public class CommandLineOptions {
    public string printerName;
    public bool print;
    public bool jobCompletion;
    public bool parkBack;
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
};

internal class Smartcard {

    // Escapes are top to area; left to area; width; height : card in portrait orientation
    private static string printBlockingEscape = "~PB%7.26 22.35 14.99 14.99?";

    private static string topcoatRemovalEscape = "~TR%7.26 22.35 14.99 14.99?";

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine(thisExeName + " demonstrates interactive mode parking a card in the smart card");
        Console.WriteLine("  station, moving the card from the station, and options to print and poll for");
        Console.WriteLine("  job completion.");
        Console.WriteLine();
        Console.WriteLine("Uses hardcoded data for printing.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-p] [-b] [-c] [-i <input hopper>] [-f <side>]");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -p Print text and graphic on front side of card.");
        Console.WriteLine("  -b use back of card for smartcard chip.");
        Console.WriteLine("  -c Poll for job completion.");
        Console.WriteLine("  -f <side>  Flip card on output.");
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\"");
        Console.WriteLine("  Parks a card in the printer smart card station, asks you to continue or");
        Console.WriteLine("  reject and then does what you requested.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -p -c");
        Console.WriteLine("  Parks a card in printer smart card station, moves the card from the station,");
        Console.WriteLine("  prints simple text and graphics on the front side of the card, and polls and");
        Console.WriteLine("  displays job status.");
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        if (args.Length == 0) Usage();

        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (string.IsNullOrEmpty(arguments["n"])) {
            Console.WriteLine("printer name is required");
            Environment.Exit(-1);
        }
        else {
            // we might have a -n with no printer name:
            bool boolVal = false;
            if (Boolean.TryParse(arguments["n"], out boolVal)) {
                Console.WriteLine("printer name is required");
                Environment.Exit(-1);
            }
            commandLineOptions.printerName = arguments["n"];
        }

        commandLineOptions.print = !string.IsNullOrEmpty(arguments["p"]);
        commandLineOptions.parkBack = !string.IsNullOrEmpty(arguments["b"]);
        commandLineOptions.jobCompletion = !string.IsNullOrEmpty(arguments["c"]);
        commandLineOptions.hopperID =
             string.IsNullOrEmpty(arguments["i"]) ? string.Empty : arguments["i"].ToLower();
        commandLineOptions.cardEjectSide =
            string.IsNullOrEmpty(arguments["f"]) ? string.Empty : arguments["f"].ToLower();

        return commandLineOptions;
    }

    private static void ResumeJob(BidiSplWrap bidiSpl, int printerJobID, int errorCode) {
        string xmlFormat = strings.PRINTER_ACTION_XML;
        string input = string.Format(xmlFormat, (int) Actions.Resume, printerJobID, errorCode);
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
    }

    /// <summary>
    /// Simulate a smartcard personalization operation.
    ///
    /// This is where calls to a third-party library would be used to communicate
    /// with the chip.
    /// </summary>
    /// <returns></returns>
    private static void PersonalizeSmartcard() {
        string caption = "A card has been parked in smartcard module";
        string message =
           "This is where smartcard personalization happens." +
           Environment.NewLine + Environment.NewLine +
           "Press 'Yes' to simulate a successful smartcard personalization." +
           Environment.NewLine + Environment.NewLine +
           "Press 'No' to simulate a failed smartcard personalization.";

        DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);

        if (result != DialogResult.Yes) {
            throw new Exception("PersonalizeSmartcard(): error occured.");
        }
    }

    private static void SmartcardPark(BidiSplWrap bidiSpl, bool parkBack) {
        var parkCommand = parkBack ? strings.SMARTCARD_PARK_BACK : strings.SMARTCARD_PARK;
        string printerStatusXML = bidiSpl.SetPrinterData(parkCommand);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            throw new BidiException(
                "SmartcardPark() fail" + printerStatusValues._errorString,
                printerStatusValues._printerJobID,
                printerStatusValues._errorCode);
        }
    }



    ////////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions commandLineOptions = GetCommandlineOptions(args);
        commandLineOptions.Validate();

        BidiSplWrap bidiSpl = null;
        int printerJobID = 0;

        try {
            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);

            string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
            PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

            if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
                throw new Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
            }

            if (commandLineOptions.print && ("Installed" != printerOptionsValues._printHead)) {
                throw new Exception(commandLineOptions.printerName + " does not have a print head installed.");
            }

            if ("Installed" != printerOptionsValues._optionSmartcard) {
                var msg = string.Format("{0} needs 'Installed' for smartcard option. '{1}' was returned.",
                    commandLineOptions.printerName,
                    printerOptionsValues._optionSmartcard);
                throw new Exception(msg);
            }

            string hopperID = "1";
            string cardEjectSide = "default";

            printerJobID = Util.StartJob(
                bidiSpl,
                (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID,
                (commandLineOptions.cardEjectSide.Length > 0) ? commandLineOptions.cardEjectSide : cardEjectSide);

            SmartcardPark(bidiSpl, commandLineOptions.parkBack);

            PersonalizeSmartcard();

            ResumeJob(bidiSpl, printerJobID, 0);

            if (commandLineOptions.print) {
                if (commandLineOptions.parkBack) {
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);
                }
                else {
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName, printBlockingEscape,
                        topcoatRemovalEscape);
                }
                Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName);
            }

            bidiSpl.SetPrinterData(strings.ENDJOB);

            if (commandLineOptions.jobCompletion) {
                Util.PollForJobCompletion(bidiSpl, printerJobID);
            }
        }
        catch (BidiException e) {
            Console.WriteLine(e.Message);
            Util.CancelJob(bidiSpl, e.PrinterJobID, e.ErrorCode);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            if (0 != printerJobID) {
                Util.CancelJob(bidiSpl, printerJobID, 0);
            }
        }
        finally {
            bidiSpl.UnbindDevice();
        }
    }
}