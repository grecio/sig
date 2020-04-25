////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK magstripe sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.IO;
using dxp01sdk;

public class CommandLineOptions {
    public string printerName;
    public bool encodeMagstripe;
    public bool readMagstripe;
    public bool print;
    public bool jobCompletion;
    public bool jisRequest;
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

internal class magstripe {

    private static void usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine(thisExeName + " demonstrates interactive mode magnetic stripe encoding with");
        Console.WriteLine("  options to read the magnetic stripe data, print, and poll for job");
        Console.WriteLine("  completion status.");
        Console.WriteLine();
        Console.WriteLine("Uses hardcoded data for magnetic stripe and printing.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-e] [-r] [-p] [-c] [-j] [-c] [-i <input hopper>] [-f <side>]");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -e Encodes magnetic stripe data.");
        Console.WriteLine("  -r Reads back the encoded magnetic stripe data.");
        Console.WriteLine("  -p Print simple black text on front side of card. ");
        Console.WriteLine("  -j JIS magnetic. ");
        Console.WriteLine("  -c Poll for job completion.");
        Console.WriteLine("  -f <side>  Flip card on output.");
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -e -r");
        Console.WriteLine("  Encodes data on all three tracks of an ISO 3-track magnetic stripe on the");
        Console.WriteLine("  backside of a card, reads and displays it.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -r -p -c");
        Console.WriteLine("  Reads data on all three tracks of an ISO 3-track magnetic stripe on the");
        Console.WriteLine("  backside of a card and displays it,");
        Console.WriteLine("  prints black text on the front side of the card, and polls and");
        Console.WriteLine("  displays job status.");
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        if (args.Length == 0) usage();

        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (!string.IsNullOrEmpty(arguments["h"])) usage();

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

        commandLineOptions.encodeMagstripe = !string.IsNullOrEmpty(arguments["e"]);
        commandLineOptions.readMagstripe = !string.IsNullOrEmpty(arguments["r"]);
        commandLineOptions.print = !string.IsNullOrEmpty(arguments["p"]);
        commandLineOptions.jobCompletion = !string.IsNullOrEmpty(arguments["c"]);
        commandLineOptions.jisRequest = !string.IsNullOrEmpty(arguments["j"]);
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

    public static void ReadMagstripe(BidiSplWrap bidiSpl, bool jisRequest) {
        // replace schema string MAGSTRIPE_READ to MAGSTRIPE_READ_FRONT for front side read
        string printerStatusXML = bidiSpl.GetPrinterData(strings.MAGSTRIPE_READ);

        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
        if (0 == printerStatusValues._errorCode) {
            Console.WriteLine("Magstripe data read successfully; printer job id: " + printerStatusValues._printerJobID);

            string track1 = "";
            string track2 = "";
            string track3 = "";

            Util.ParseMagstripeStrings(printerStatusXML, ref track1, ref track2, ref track3, jisRequest);

            if (track1.Length != 0) {
                // Convert the Base64 UUEncoded output.
                byte[] binaryData = System.Convert.FromBase64String(track1);
                string str = System.Text.Encoding.UTF8.GetString(binaryData);
                Console.WriteLine(" track1 Base64 decoded: " + str);
            }

            if (track2.Length != 0) {
                // Convert the Base64 UUEncoded output.
                byte[] binaryData = System.Convert.FromBase64String(track2);
                string str = System.Text.Encoding.UTF8.GetString(binaryData);
                Console.WriteLine(" track2 Base64 decoded: " + str);
            }

            if (track3.Length != 0) {
                // Convert the Base64 UUEncoded output.
                byte[] binaryData = System.Convert.FromBase64String(track3);
                string str = System.Text.Encoding.UTF8.GetString(binaryData);
                Console.WriteLine(" track3 Base64 decoded: " + str);
            }
        }
        else {
            throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }
    }

    public static void EncodeMagstripe(BidiSplWrap bidiSpl, bool jisRequest) {
        // Hardcoded XML to encode all 3 tracks in IAT mode.
        // track 1 = "TRACK1", track 2 = "1122", track 3 = "321"
        string trackDataXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + "<magstripe>"
            + "<track number=\"1\"><base64Data>VFJBQ0sx</base64Data></track>"
            + "<track number=\"2\"><base64Data>MTEyMg==</base64Data></track>"
            + "<track number=\"3\"><base64Data>MzIx</base64Data></track>"
            + "</magstripe>";

        if (jisRequest) {
            // JIS only allows track 3 = "321"
            trackDataXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + "<magstripe>"
            + "<track number=\"1\"><base64Data></base64Data></track>"
            + "<track number=\"2\"><base64Data></base64Data></track>"
            + "<track number=\"3\"><base64Data>MzIx</base64Data></track>"
            + "</magstripe>";
        }

        // replace schema string MAGSTRIPE_ENCODE to MAGSTRIPE_ENCODE_FRONT for front side encode
        string printerStatusXML = bidiSpl.SetPrinterData(strings.MAGSTRIPE_ENCODE, trackDataXML);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

        if (0 != printerStatusValues._errorCode) {
            throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
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
                throw new Exception(commandLineOptions.printerName +
                                    " does not have a print head installed.");
            }

            //  A printer may have a JIS magnetic stripe unit in addition to an ISO unit.
            //  In that case the option will be "ISO, JIS" so 'Contains' as opposed to
            //  '==' must be used...
            if (commandLineOptions.jisRequest) {
                if (!printerOptionsValues._optionMagstripe.Contains("JIS")) {
                    throw new Exception(commandLineOptions.printerName +
                                        " does not have a JIS magnetic stripe unit installed.");
                }
            }
            else if (!printerOptionsValues._optionMagstripe.Contains("ISO")) {
                throw new Exception(commandLineOptions.printerName +
                                    " does not have an ISO magnetic stripe unit installed.");
            }

            string hopperID = "1";
            string cardEjectSide = "default";

            printerJobID = Util.StartJob(
                bidiSpl,
                (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID,
                (commandLineOptions.cardEjectSide.Length > 0) ? commandLineOptions.cardEjectSide : cardEjectSide);

            if (commandLineOptions.encodeMagstripe) {
                EncodeMagstripe(bidiSpl, commandLineOptions.jisRequest);
            }

            if (commandLineOptions.readMagstripe) {
                ReadMagstripe(bidiSpl, commandLineOptions.jisRequest);
            }

            // Check if user wants print
            if (commandLineOptions.print) {
                // this also waits for the print spooling to finish:
                Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);
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