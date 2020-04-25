////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK laminator barcode sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Windows.Forms;
using dxp01sdk;

public class CommandLineOptions {
    public string printerName;
    public int timeout;
    public string fileName;
    public bool verify;
};

internal class laminator_barcode {

    private static void usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine(thisExeName + " demonstrates interactive mode laminator bar code ");
        Console.WriteLine("read with options to verify the bar code data, and polls for job completion.");
        Console.WriteLine("Serialized overlay material must be loaded in the L1 laminator.");
        Console.WriteLine();
        Console.WriteLine("This sample uses the driver Printing Preferences/Lamination settings. Make sure");
        Console.WriteLine("that the L1 Laminate card setting is not set to \"Do not apply.\" The sample");
        Console.WriteLine("always prints a card. To laminate the card without printing, select");
        Console.WriteLine("\"Printing Preferences/Layout/Advanced\" and change \"Disable printing\" to \"All\".");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-h] [-v] [-t] <timeout> [-f] <filename>");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -h Displays this information. <optional>");
        Console.WriteLine("  -v Verify the laminator bar code data. <optional>");
        Console.WriteLine("  -t <msec> Timeout to read bar code data. Default is no timeout. <optional>");
        Console.WriteLine("  -f <filename> Save the laminator bar code read results to a file. <optional>");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\"");
        Console.WriteLine("  Demonstrates interactive mode laminator bar code read with no timeout;");
        Console.WriteLine("  prints black text on one or both sides of the card; polls and displays job");
        Console.WriteLine("  status");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -v -f \"example.txt\" ");
        Console.WriteLine("  Demonstrates interactive mode laminator bar code read and verify with no");
        Console.WriteLine("  timeout; prints black text on one or both sides of the card; polls and");
        Console.WriteLine("  displays job status. It also writes the bar code data to file example.txt.");
        Console.WriteLine();
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

        commandLineOptions.verify = !string.IsNullOrEmpty(arguments["v"]);

        if (string.IsNullOrEmpty(arguments["t"])) {
            commandLineOptions.timeout = (int) laminator_barcode_timout.INFINITE_WAIT;
        }
        else {
            bool successfullyConvertedString = Int32.TryParse(arguments["t"], out commandLineOptions.timeout);
            if (!successfullyConvertedString) {
                Console.WriteLine("Cannot convert command line argument \"{0}\" to an Int32.", arguments["t"]);
                Environment.Exit(-1);
            }
        }

        if (!string.IsNullOrEmpty(arguments["f"]))
            commandLineOptions.fileName = arguments["f"];

        return commandLineOptions;
    }

    private static void ResumeJob(BidiSplWrap bidiSpl, int printerJobID, int errorCode) {
        string xmlFormat = strings.PRINTER_ACTION_XML;
        string input = string.Format(xmlFormat, (int) Actions.Resume, printerJobID, errorCode);
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
    }

    private static string GetCurrentDateTimeString() {
        string dateString = System.DateTime.Now.ToShortDateString();
        string timeString = System.DateTime.Now.ToShortTimeString();
        string currentDateTimeString = string.Format(dateString + " " + timeString + "      ");
        return currentDateTimeString;
    }

    private static void WriteToFile(string fileName, string text) {
        if (!string.IsNullOrEmpty(fileName)) {
            string lines = GetCurrentDateTimeString() + text;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fileName, true)) {
                file.WriteLine(lines);
            }
        }
    }

    private static bool IsBarcodeDataGood() {
        string caption = "A laminator bar code has been read.";
        string message = "This is where laminator bar code verify happens.\r\n\r\n" +
           "Select 'Yes' to continue the job. This simulates that the bar code data passed verification and you want to finish the card.\r\n\r\n" +
           "Select 'No' to cancel the job. This simulates that the bar code data failed verification and you want to reject the card.";

        DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);
        return (result == DialogResult.Yes);
    }

    private static void SetLaminatorBarcodeActions(BidiSplWrap bidiSpl, bool verify) {
        if (verify) {
            bidiSpl.SetPrinterData(strings.LAMINATOR_BARCODE_READ_AND_VERIFY);
        }
        else {
            bidiSpl.SetPrinterData(strings.LAMINATOR_BARCODE_READ);
        }
    }

    private static void ReadLaminatorBarcode(BidiSplWrap bidiSpl, int printerjobid, int timeout, bool verify, string fileName) {
        string barcodejobXml = string.Format(strings.LAMINATOR_BARCODE_READ_XML, printerjobid, timeout);
        string printerStatusXML = bidiSpl.GetPrinterData(strings.LAMINATOR_BARCODE_READ_DATA, barcodejobXml);

        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
        if (516 == printerStatusValues._errorCode) {
            // application did not provide long enough timeout for the bar code to
            // be read. It is application's responsibility to call bar code read
            // again.
            Console.WriteLine("BarcodeRead() fail; Printer Error: " + printerStatusValues._errorString);
            if (!string.IsNullOrEmpty(fileName)) {
                string text = string.Format("Printer Job Id: " + printerjobid + "  " + printerStatusValues._errorString);
                WriteToFile(fileName, text);
            }
        }
        else if (517 == printerStatusValues._errorCode) {
            Console.WriteLine("BarcodeRead() fail; Printer Error: " + printerStatusValues._errorString);
        }
        else if (0 != printerStatusValues._errorCode) {
            string message = string.Format("BarcodeRead() fail: " + printerStatusValues._errorString);
            throw new BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }
        else if (0 == printerStatusValues._errorCode) {
            string barcodeLaminator = printerStatusValues._dataFromPrinter;
            if (!string.IsNullOrEmpty(barcodeLaminator)) {
                Console.WriteLine("Bar code: " + barcodeLaminator);
                if (!string.IsNullOrEmpty(fileName)) {
                    string text = string.Format("Printer Job Id: " + printerjobid + ", bar code data: " + barcodeLaminator);
                    WriteToFile(fileName, text);
                }
            }
            else {
                Console.WriteLine("Error no data read.");
                if (!string.IsNullOrEmpty(fileName)) {
                    string text = string.Format("Printer Job Id: " + printerjobid + " - Error no data read.", printerjobid);
                    WriteToFile(fileName, text);
                }
            }

            if (verify) {
                if (IsBarcodeDataGood()) {
                    ResumeJob(bidiSpl, printerjobid, 0);
                }
                else {
                    string message = string.Format("Barcode verify failed");
                    throw new BidiException(message, printerStatusValues._printerJobID, 0);
                }
            }
        }
    }

    private static void AddMagstripeReadOrEncodeOrSmarcardOperationsHere() {
        // Placeholder. Please refer to Magstripe and smartcard samples.
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions commandLineOptions = GetCommandlineOptions(args);

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

            if ("None" == printerOptionsValues._laminator) {
                throw new Exception(commandLineOptions.printerName + " does not have a laminator.");
            }

            if ("None" == printerOptionsValues._laminatorScanner) {
                throw new Exception(commandLineOptions.printerName + " does not have a lamination barcode scanner.");
            }

            const string hopperID = "";
            printerJobID = Util.StartJob(
                bidiSpl,
                hopperID);

            // Notify the printer that a bar code read is part of the job.
            // Note: serialized laminate must be installed in L1 laminator station.
            SetLaminatorBarcodeActions(bidiSpl, commandLineOptions.verify);

            // Add code for magnetic stripe read, magnetic stripe encode and smart
            // card operations here. Other samples show how to perform these
            // operations.
            AddMagstripeReadOrEncodeOrSmarcardOperationsHere();

            // Do some simple text printing. You MUST send print data to the driver
            // to laminate. Print data is required as part of every lamination job
            // because lamination actions are a printing preference.
            Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);

            // NOTE: Although you must send print data to the driver to laminate,
            // you can prevent printing on the card by disabling printing in the
            // driver:
            // 1) Select “Printing Preferences/Layout/Advanced and change “Disable
            //    printing to “All.” OR,
            // 2) Programmatically “Disable printing" using print ticket. The Print
            //    sample demonstrates print ticket manipulation.

            // Call EndJob to notify the printer that all the data has been sent for
            // the card.
            bidiSpl.SetPrinterData(strings.ENDJOB);

            // Read the bar code. This may take several minutes if the laminator is
            // warming up. If the  timeout provided is too small the function will
            // return before the bar code can be read. If you use a short timeout,
            // call this function repeatedly.
            ReadLaminatorBarcode(
                bidiSpl,
                printerJobID,
                commandLineOptions.timeout,
                commandLineOptions.verify,
                commandLineOptions.fileName);

            // Use job completion to monitor that the card was personalized
            // successfully or failed to complete because of an error.
            Util.PollForJobCompletion(bidiSpl, printerJobID);
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