////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK csharp lamination sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using dxp01sdk;

namespace Lamination {

    public class Lamination {

        public static string GetLongExeName() {
            string longExeName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return longExeName;
        }

        private static void Main(string[] args) {
            CommandLineOptions commandLineOptions = CommandLineOptions.CreateFromArguments(args);
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

                //  A printer may have both laminator station 1 and station 2 installed.
                //  In that case the option will be "L1, L2" so 'Contains' as opposed to
                //  '==' must be used...
                if (LaminationActions.Actions.doesNotApply != commandLineOptions.L1Action) {
                    if (!printerOptionsValues._laminator.Contains("L1")) {
                        throw new Exception(commandLineOptions.printerName + " does not have a station 1 laminator installed.");
                    }
                }

                if (LaminationActions.Actions.doesNotApply != commandLineOptions.L2Action) {
                    if (!printerOptionsValues._laminator.Contains("L2")) {
                        throw new Exception(commandLineOptions.printerName + " does not have a station 2 laminator installed.");
                    }
                }

                if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
                    throw new Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
                }

                if (commandLineOptions.jobCompletion ||
                    (commandLineOptions.hopperID.Length > 0) ||
                    (commandLineOptions.cardEjectSide.Length > 0)) {
                    string hopperID = "1";
                    string cardEjectSide = "default";

                    printerJobID = Util.StartJob(
                        bidiSpl,
                        (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID,
                        (commandLineOptions.cardEjectSide.Length > 0) ? commandLineOptions.cardEjectSide : cardEjectSide);
                }


                SamplePrintDocument printDocument = new SamplePrintDocument(commandLineOptions);
                printDocument.PrintController = new StandardPrintController();
                printDocument.BeginPrint += new PrintEventHandler(printDocument.OnBeginPrint);
                printDocument.PrintPage += new PrintPageEventHandler(printDocument.OnPrintPage);
                printDocument.Print();
                printDocument.RestoreUserPreferences();

                if (0 != printerJobID) {
                    // wait for the print spooling to finish and then issue an EndJob():
                    Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName);
                    bidiSpl.SetPrinterData(strings.ENDJOB);
                }

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
}