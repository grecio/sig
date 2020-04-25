////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// populate a CommandLineOptions data structure from parsed command line args.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace Lamination {

    public class CommandLineOptions {
        public string printerName;
        public bool jobCompletion;
        public LaminationActions.Actions L1Action = LaminationActions.Actions.doesNotApply;
        public LaminationActions.Actions L2Action = LaminationActions.Actions.doesNotApply;
        public string hopperID = "";
        public string cardEjectSide = "";

        public static void Usage() {
            string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine(thisExeName + " demonstrates lamination functionality of the printer and driver.");
            Console.WriteLine("Overrides the driver printing preference settings.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine(thisExeName + " -n <printername> [-x <F|B|A|T>] [-c] [-i <input hopper>] [-f <side>]");
            Console.WriteLine("where");
            Console.WriteLine("  -n <printername> specifies which printer to use. Required.");
            Console.WriteLine("  -x specifies the laminator station to use.");
            Console.WriteLine("      Valid values are \"1\" and \"2\".");
            Console.WriteLine("  Laminator actions:");
            Console.WriteLine("    F --> laminate the front.");
            Console.WriteLine("    B --> laminate the back.");
            Console.WriteLine("    A --> laminate both front and back.");
            Console.WriteLine("    T --> laminate the front twice.");
            Console.WriteLine();
            Console.WriteLine("  -c Poll for job completion.");
            Console.WriteLine("  -f <Front | Back>. Flip card on output.");
            Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n \"xps card printer\" -1 F -2 B");
            Console.WriteLine("  the front of the card will be laminated in station 1 and ");
            Console.WriteLine("  the back of the card will be laminated in station 2.");
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n \"xps card printer\" -1 T");
            Console.WriteLine("  the front of the card will be laminated in station 1 two times.");
            Console.WriteLine("  Station 2 is not used.");
            Console.WriteLine();
            Console.WriteLine(thisExeName + " -n \"xps card printer\" -1 F -1 B");
            Console.WriteLine("  the front of the card will be laminated in station 1.");
            Console.WriteLine("  The first specification of a station will be used.");
            Console.WriteLine();
            Environment.Exit(-1);
        }

        static public CommandLineOptions CreateFromArguments(string[] args) {
            if (args.Length == 0) Usage();

            CommandLineOptions commandLineOptions = new CommandLineOptions();
            CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

            commandLineOptions.printerName = arguments["n"];
            if ((string.IsNullOrEmpty(commandLineOptions.printerName))) {
                Usage();
            }

            bool laminationActionSpecified = false;

            string laminationActionArg = arguments["1"];
            if (!string.IsNullOrEmpty(laminationActionArg)) {
                laminationActionSpecified = true;
                commandLineOptions.L1Action = LaminationActions.GetLaminationAction(laminationActionArg);
            }

            laminationActionArg = arguments["2"];
            if (!string.IsNullOrEmpty(laminationActionArg)) {
                laminationActionSpecified = true;
                commandLineOptions.L2Action = LaminationActions.GetLaminationAction(laminationActionArg);
            }

            if (!laminationActionSpecified) {
                Usage();
            }

            commandLineOptions.jobCompletion = !string.IsNullOrEmpty(arguments["c"]);
            commandLineOptions.hopperID =
                string.IsNullOrEmpty(arguments["i"]) ? string.Empty : arguments["i"].ToLower();
            commandLineOptions.cardEjectSide =
                string.IsNullOrEmpty(arguments["f"]) ? string.Empty : arguments["f"].ToLower();

            return commandLineOptions;
        }

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
}