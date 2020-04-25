////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// populate a CommandLineOptions data structure from parsed command line args.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace laser {

    internal class CommandLineOptions {
        public bool encodeMagstripe;
        public bool pollForJobCompletion;
        public bool retrieveLaserSetup;
        public bool printText;
        public bool laserEngraveStatic;
        public bool laserExportFile;
        public bool laserImportFile;
        public string printerName;
        public string hopperID = "";

        private static void Usage() {
            string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine(thisExeName + " demonstrates laser engraving along with options to");
            Console.WriteLine("perform interactive mode magnetic stripe encoding, print, and poll for job completion status.");
            Console.WriteLine();
            Console.WriteLine("Uses hardcoded data for laser engraving, magnetic stripe and printing.");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n <printername> [-u] [-d] [-r] [-s] [-e] [-p] [-c] [-i <input hopper>] ");
            Console.WriteLine("options:");
            Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
            Console.WriteLine("  -u Export laser setup zip files from printer to PC.");
            Console.WriteLine("  -d Import laser setup zip files to the printer.");
            Console.WriteLine("  -r Retrieves laser setup files from the printer.");
            Console.WriteLine("  -s Laser engraves static laser setup file data. Default is to ");
            Console.WriteLine("     laser engrave variable setup file data, depending on the printer capabilities.");
            Console.WriteLine("  -e Encodes magnetic stripe data.");
            Console.WriteLine("  -p Print simple black text on front of the card.");
            Console.WriteLine("     depending on the printer capabilities.");
            Console.WriteLine("  -c Poll for job completion; needed to check for printer errors.");
            Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n \"XPS Card Printer\"");
            Console.WriteLine("  Laser engraves data.");
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -e -p");
            Console.WriteLine("  Laser engraves, prints and encodes magstripe data on all three tracks of an ISO 3-track magnetic stripe.");
            Environment.Exit(-1);
        }

        public void Validate() {
            if (string.IsNullOrEmpty(printerName)) {
               Usage();
            }

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

        static public CommandLineOptions CreateFromArguments(string[] args) {
            if (args.Length == 0) Usage();

            CommandLineOptions commandLineOptions = new CommandLineOptions();
            CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

            if (!string.IsNullOrEmpty(arguments["h"])) Usage();

            commandLineOptions.printerName = arguments["n"];
            commandLineOptions.retrieveLaserSetup = !string.IsNullOrEmpty(arguments["r"]);
            commandLineOptions.encodeMagstripe = !string.IsNullOrEmpty(arguments["e"]);
            commandLineOptions.printText = !string.IsNullOrEmpty(arguments["p"]);
            commandLineOptions.pollForJobCompletion = !string.IsNullOrEmpty(arguments["c"]);
            commandLineOptions.laserEngraveStatic = !string.IsNullOrEmpty(arguments["s"]);
            commandLineOptions.laserImportFile = !string.IsNullOrEmpty(arguments["d"]);
            commandLineOptions.laserExportFile = !string.IsNullOrEmpty(arguments["u"]);
            commandLineOptions.hopperID = string.IsNullOrEmpty(arguments["i"]) ? string.Empty : arguments["i"].ToLower();

            return commandLineOptions;
        }
    }
}