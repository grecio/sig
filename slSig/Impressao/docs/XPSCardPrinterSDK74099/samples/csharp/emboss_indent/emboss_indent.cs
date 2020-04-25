////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Card Printer SDK csharp 'emboss_indent' sample
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Xml;
using dxp01sdk;

internal class CommandLineOptions {
    public string printerName = string.Empty;
    public string hopperID = string.Empty;
    public bool pollForJobCompletion = false;
    public bool displayPrintTicket = false;
    public bool disableTopping = false;

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
    }
}

internal class EmbossIndentDocument : PrintDocument {
    private CommandLineOptions _commandLineOptions;

    public EmbossIndentDocument(CommandLineOptions commandLineOptions) {
        _commandLineOptions = commandLineOptions;
        DocumentName = "emboss indent document";
        PrinterSettings.PrinterName = commandLineOptions.printerName;
        PrintController = new StandardPrintController();
        BeginPrint += new PrintEventHandler(OnBeginPrint);
        PrintPage += new PrintPageEventHandler(OnPrintPage);
    }

    public void OnBeginPrint(object sender, PrintEventArgs printEventArgs)
    {
        // prepare the PrintTicket for the entire print job.
        PrintQueue printQueue = new PrintQueue(new LocalPrintServer(), PrinterSettings.PrinterName);
        PrintTicket deltaPrintTicket = new PrintTicket();
        deltaPrintTicket.Duplexing = Duplexing.OneSided;
        deltaPrintTicket.CopyCount = 1;
        deltaPrintTicket.PageOrientation = PageOrientation.Landscape;

        ValidationResult validationResult = printQueue.MergeAndValidatePrintTicket(
           printQueue.UserPrintTicket,
           deltaPrintTicket);

        string xmlString = PrintTicketXml.Prefix;

        xmlString += _commandLineOptions.disableTopping ?
           PrintTicketXml.ToppingOff : PrintTicketXml.ToppingOn;

        xmlString += PrintTicketXml.Suffix;

        // prepare to merge our PrintTicket xml into an actual PrintTicket:
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlString);
        MemoryStream memoryStream = new MemoryStream();
        xmlDocument.Save(memoryStream);
        memoryStream.Position = 0;
        deltaPrintTicket = new PrintTicket(memoryStream);

        validationResult = printQueue.MergeAndValidatePrintTicket(
           validationResult.ValidatedPrintTicket,
           deltaPrintTicket);

        printQueue.UserPrintTicket = validationResult.ValidatedPrintTicket;

        if (_commandLineOptions.displayPrintTicket)
        {
            Util.DisplayPrintTicket(validationResult.ValidatedPrintTicket);
        }

        // IMPORTANT: this Commit() call sets the driver's 'Printing Preferences'
        // on this machine:
        printQueue.Commit();
    }
    private void OnPrintPage(object sender, PrintPageEventArgs pageEventArgs)
    {
        string text = string.Empty;

        // emit all emboss + indent data driver escape strings. the font and
        // brush are not significant; just needed for the DrawString() call.
        using (Font font = new Font("Arial", 8))
        using (SolidBrush brush = new SolidBrush(Color.Black)) {
            text = "~EM%1;301;860;Font 11111";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%2;1600;860;222222";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%3;301;1460;333333";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%4;301;1180;444444";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%5;301;690;555555";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%6;1600;690;666666";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%7;301;650;777777";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%8;301;1000;888888";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%9;301;1050;999999";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);

            text = "~EM%10;1600;1050;10 10 10";
            pageEventArgs.Graphics.DrawString(text, font, brush, 50, 50);
        }
    }
}

internal class emboss_indent {

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine(thisExeName + " demonstrates the emboss and indent escapes, input hopper");
        Console.WriteLine("selection, and the checking of print ribbon and emboss supplies.");
        Console.WriteLine();
        Console.WriteLine("Emboss and indent data is hardcoded.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-i <input hopper>] [-d] [-x] [-c] [-i <input hopper>]");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -i <input hopper>. Optional. Defaults to \"1\".");
        Console.WriteLine("  -d Disable topping.");
        Console.WriteLine("  -x Display the print ticket data. Default is no display.");
        Console.WriteLine("  -c Poll for job completion. Optional. Defaults to false.");
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
        Console.WriteLine();
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        if (args.Length == 0) Usage();

        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (string.IsNullOrEmpty(arguments["n"])) { Usage(); }
        // we might have a -n with no printer name:
        bool boolVal = false;
        if (Boolean.TryParse(arguments["n"], out boolVal)) { Usage(); }
        commandLineOptions.printerName = arguments["n"];

        if (!string.IsNullOrEmpty(arguments["i"])) {
            // we might have a -i with no input hopper:
            if (Boolean.TryParse(arguments["i"], out boolVal)) { Usage(); }
            commandLineOptions.hopperID = arguments["i"];
        }

        commandLineOptions.disableTopping = !string.IsNullOrEmpty(arguments["d"]);
        commandLineOptions.displayPrintTicket = !string.IsNullOrEmpty(arguments["x"]);
        commandLineOptions.pollForJobCompletion = !string.IsNullOrEmpty(arguments["c"]);
        return commandLineOptions;
    }

    private static void Main(string[] args) {
        CommandLineOptions commandLineOptions = GetCommandlineOptions(args);
        commandLineOptions.Validate();

        BidiSplWrap bidiSpl = null;
        int printerJobID = 0;

        try {
            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            string driverVersion = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine("driver version: " + Util.ParseDriverVersionXML(driverVersion) + Environment.NewLine);

            string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
            PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

            if ("Installed" != printerOptionsValues._moduleEmbosser) {
                throw new Exception(commandLineOptions.printerName + ": embosser is not present.");
            }

            if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
                throw new Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
            }

            string hopperID = "1";

            printerJobID = Util.StartJob(
                bidiSpl,
                (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID);

            var printDocument = new EmbossIndentDocument(commandLineOptions);
            printDocument.Print();

            Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName);
            bidiSpl.SetPrinterData(strings.ENDJOB);

            if (commandLineOptions.pollForJobCompletion) {
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