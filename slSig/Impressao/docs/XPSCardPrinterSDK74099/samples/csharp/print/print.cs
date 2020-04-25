////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK csharp print sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using dxp01sdk;

namespace print {

    internal class print {

        private static string GetHopperStatus(
            BidiSplWrap bidiSpl,
            string hopperId)
        {
            string hopperStatusXml = bidiSpl.GetPrinterData(strings.HOPPER_STATUS);
            string hopperStatus = Util.ParseHopperStatusXML(hopperStatusXml, hopperId);
            return hopperStatus;
        }
        private static void Main(string[] args) {
            CommandLineOptions commandLineOptions = CommandLineOptions.CreateFromArguments(args);
            commandLineOptions.Validate();
            bool checkHopperStatus = commandLineOptions.hopperID.Length > 0 && 
                                     commandLineOptions.checkHopper;

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

                if ("Installed" != printerOptionsValues._printHead) {
                    throw new Exception(commandLineOptions.printerName + " does not have a print head installed.");
                }

                if (commandLineOptions.magstripe && !printerOptionsValues._optionMagstripe.Contains("ISO")) {
                    throw new Exception(commandLineOptions.printerName + " does not have an ISO magnetic stripe unit installed.");
                }

                if (checkHopperStatus) {
                    string hopperStatus = GetHopperStatus(bidiSpl, commandLineOptions.hopperID);
                    if (String.Compare(hopperStatus, "Empty", StringComparison.OrdinalIgnoreCase) == 0)
                        throw new Exception("Hopper '" + commandLineOptions.hopperID + "' is empty.");

                    Console.WriteLine("Status of hopper '" + commandLineOptions.hopperID + "': " + hopperStatus + ".");
                }

                if (commandLineOptions.jobCompletion ||
                    (commandLineOptions.hopperID.Length > 0) ||
                    (commandLineOptions.cardEjectSide.Length > 0))
                {
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
                printDocument.QueryPageSettings += new QueryPageSettingsEventHandler(printDocument.OnQueryPageSettings);
                printDocument.PrintPage += new PrintPageEventHandler(printDocument.OnPrintPage);
                printDocument.Print();

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