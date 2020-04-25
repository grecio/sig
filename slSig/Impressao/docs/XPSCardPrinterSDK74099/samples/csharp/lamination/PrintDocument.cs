////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// Class derived from System.Drawing.Printing.PrintDocument.
//
// Demonstrate all the classic 'winforms'-style printing and use a PrintTicket
// to manipulate print settings.
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Xml;

namespace Lamination {

    internal class SamplePrintDocument : PrintDocument {
        private CommandLineOptions _commandLineOptions;
        private PrintTicket _userPrintTicket = new PrintTicket();
        private int _sizeFactor = 3;

        public SamplePrintDocument(CommandLineOptions commandLineOptions) {
            _commandLineOptions = commandLineOptions;
            PrinterSettings.PrinterName = commandLineOptions.printerName;
            DocumentName = "XPS Card Printer SDK C# Lamination Sample";
        }

        public void RestoreUserPreferences() {
            PrintQueue printQueue = new PrintQueue(new LocalPrintServer(), PrinterSettings.PrinterName);
            printQueue.UserPrintTicket = _userPrintTicket;
            printQueue.Commit();
        }

        public string GetPrintTicketXml(CommandLineOptions commandLineOptions) {
            string xml = PrintTicketXml.Prefix;

            xml += PrintTicketXml.FeatureNamePrefix
                + PrintTicketXml.JobLamination1FeatureName
                + LaminationActions.GetLaminationActionXML(commandLineOptions.L1Action)
                + PrintTicketXml.FeatureNameSuffix;

            xml += PrintTicketXml.FeatureNamePrefix
                + PrintTicketXml.JobLamination2FeatureName
                + LaminationActions.GetLaminationActionXML(commandLineOptions.L2Action)
                + PrintTicketXml.FeatureNameSuffix;

            xml += PrintTicketXml.Suffix;
            return xml;
        }

        public void OnBeginPrint(object sender, PrintEventArgs printEventArgs) {
            string printTicketXml = GetPrintTicketXml(_commandLineOptions);

            PrintQueue printQueue = new PrintQueue(new LocalPrintServer(), PrinterSettings.PrinterName);

            _userPrintTicket = printQueue.UserPrintTicket;

            PrintTicket deltaPrintTicket = new PrintTicket();
            deltaPrintTicket.Duplexing = Duplexing.TwoSidedLongEdge;
            deltaPrintTicket.CopyCount = 1;
            deltaPrintTicket.PageOrientation = PageOrientation.Portrait;

            ValidationResult validationResult = printQueue.MergeAndValidatePrintTicket(
                printQueue.UserPrintTicket,
                deltaPrintTicket);

            // prepare to merge our PrintTicket xml into an actual PrintTicket:
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(printTicketXml);
            MemoryStream memoryStream = new MemoryStream();
            xmlDocument.Save(memoryStream);
            memoryStream.Position = 0;
            deltaPrintTicket = new PrintTicket(memoryStream);

            validationResult = printQueue.MergeAndValidatePrintTicket(
                                 validationResult.ValidatedPrintTicket,
                                 deltaPrintTicket);

            printQueue.UserPrintTicket = validationResult.ValidatedPrintTicket;

            // IMPORTANT: this Commit() call sets the driver's 'Printing Preferences'
            // on this machine:
            printQueue.Commit();
        }

        public void DrawCardFront(PrintPageEventArgs pageEventArgs) {
            using (Font font = new Font("Arial", 8)) {
                using (SolidBrush brush = new SolidBrush(Color.Red)) {
                    using (Pen pen = new Pen(Color.YellowGreen)) {
                        pageEventArgs.Graphics.DrawString("card front", font, brush, 10, 15);
                    }
                }
            }

            string colorBitmapFilename = Path.Combine(Lamination.GetLongExeName(), "color.bmp");
            if (!File.Exists(colorBitmapFilename)) {
                throw new Exception("file not found: " + colorBitmapFilename);
            }
            Bitmap colorBitmap = new Bitmap(colorBitmapFilename);
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width / _sizeFactor, colorBitmap.Height / _sizeFactor);

            string oneBppBitmapFilename = Path.Combine(Lamination.GetLongExeName(), "mono.bmp");
            if (!File.Exists(oneBppBitmapFilename)) {
                throw new Exception(" file not found: " + oneBppBitmapFilename);
            }
            Bitmap kBitmap = new Bitmap(oneBppBitmapFilename);
            pageEventArgs.Graphics.DrawImage(kBitmap, 25, 100, kBitmap.Width / _sizeFactor, kBitmap.Height / _sizeFactor);

            const string UVBitmapFilename = "uv.bmp";
            if (!File.Exists(UVBitmapFilename)) { throw new Exception(" file not found: " + UVBitmapFilename); }
            Bitmap UVBitmap = new Bitmap(UVBitmapFilename);
            pageEventArgs.Graphics.DrawImage(UVBitmap, 25, 150, UVBitmap.Width / _sizeFactor, UVBitmap.Height / _sizeFactor);
        }

        public void OnPrintPage(object sender, PrintPageEventArgs pageEventArgs) {
            DrawCardFront(pageEventArgs);
        }
    }
}