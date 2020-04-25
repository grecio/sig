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
using dxp01sdk;

namespace print {

    internal class SamplePrintDocument : PrintDocument {
        private CommandLineOptions _commandLineOptions;
        private bool _frontOfCard = true;
        private int _sizeFactor = 3;

        public SamplePrintDocument(CommandLineOptions commandLineOptions) {
            _commandLineOptions = commandLineOptions;
            PrinterSettings.PrinterName = commandLineOptions.printerName;
            DocumentName = "XPS Driver SDK c# print sample";
        }

        public void OnBeginPrint(object sender, PrintEventArgs printEventArgs) {
            // prepare the PrintTicket for the entire print job.
            PrintQueue printQueue = new PrintQueue(new LocalPrintServer(), PrinterSettings.PrinterName);
            PrintTicket deltaPrintTicket = new PrintTicket();
            deltaPrintTicket.Duplexing =
               _commandLineOptions.twoPages ? Duplexing.TwoSidedLongEdge : Duplexing.OneSided;
            deltaPrintTicket.CopyCount = _commandLineOptions.numCopies;
            deltaPrintTicket.PageOrientation =
               _commandLineOptions.portraitFront ? PageOrientation.Portrait : PageOrientation.Landscape;

            ValidationResult validationResult = printQueue.MergeAndValidatePrintTicket(
               printQueue.UserPrintTicket,
               deltaPrintTicket);

            string xmlString = PrintTicketXml.Prefix;

            xmlString += _commandLineOptions.rotateFront ?
               PrintTicketXml.FlipFrontFlipped : PrintTicketXml.FlipFrontNone;

            switch (_commandLineOptions.disablePrinting) {
                case CommandLineOptions.DisablePrinting.All:
                    xmlString += PrintTicketXml.DisablePrintingAll;
                    break;

                case CommandLineOptions.DisablePrinting.Off:
                    xmlString += PrintTicketXml.DisablePrintingOff;
                    break;

                case CommandLineOptions.DisablePrinting.Front:
                    xmlString += PrintTicketXml.DisablePrintingFront;
                    break;

                case CommandLineOptions.DisablePrinting.Back:
                    xmlString += PrintTicketXml.DisablePrintingBack;
                    break;
            }

            if (_commandLineOptions.twoPages) {
                xmlString += _commandLineOptions.rotateBack ?
                   PrintTicketXml.FlipBackFlipped : PrintTicketXml.FlipBackNone;
            }

            xmlString += GetTopcoatBlockingPrintTicketXml();
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

            if (_commandLineOptions.showXml) {
                Util.DisplayPrintTicket(validationResult.ValidatedPrintTicket);
            }

            // IMPORTANT: this Commit() call sets the driver's 'Printing Preferences'
            // on this machine:
            printQueue.Commit();
        }

        public void DrawCardFront(PrintPageEventArgs pageEventArgs) {
            using (Font font = new Font("Arial", 8))
            using (SolidBrush brush = new SolidBrush(Color.Red))
            using (Pen pen = new Pen(Color.YellowGreen)) {
                pageEventArgs.Graphics.DrawString("card front", font, brush, 10, 15);
            }

            const string colorBitmapFilename = "color.bmp";
            if (!File.Exists(colorBitmapFilename)) { throw new Exception("file not found: " + colorBitmapFilename); }
            Bitmap colorBitmap = new Bitmap(colorBitmapFilename);
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width / _sizeFactor, colorBitmap.Height / _sizeFactor);

            const string oneBppBitmapFilename = "mono.bmp";
            if (!File.Exists(oneBppBitmapFilename)) { throw new Exception(" file not fount: " + oneBppBitmapFilename); }
            Bitmap kBitmap = new Bitmap(oneBppBitmapFilename);
            pageEventArgs.Graphics.DrawImage(kBitmap, 25, 100, kBitmap.Width / _sizeFactor, kBitmap.Height / _sizeFactor);

            const string UVBitmapFilename = "uv.bmp";
            if (!File.Exists(UVBitmapFilename)) { throw new Exception(" file not fount: " + UVBitmapFilename); }
            Bitmap UVBitmap = new Bitmap(UVBitmapFilename);
            pageEventArgs.Graphics.DrawImage(UVBitmap, 25, 150, UVBitmap.Width / _sizeFactor, UVBitmap.Height / _sizeFactor);

            WriteCustomTopcoatBlockingEscapesFront(pageEventArgs.Graphics);

            if (_commandLineOptions.magstripe) {
                WriteMagstripeEscapes(pageEventArgs.Graphics);
            }
        }

        public void DrawCardBack(PrintPageEventArgs pageEventArgs) {
            using (Font font = new Font("Courier New", 8))
            using (SolidBrush brush = new SolidBrush(Color.Red))
            using (Pen pen = new Pen(Color.YellowGreen)) {
                pageEventArgs.Graphics.DrawString("card back", font, brush, 10, 10);
            }

            Bitmap colorBitmap = new Bitmap("color.bmp");
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width / _sizeFactor, colorBitmap.Height / _sizeFactor);

            Bitmap kBitmap = new Bitmap("mono.bmp");
            pageEventArgs.Graphics.DrawImage(kBitmap, 130, 50, kBitmap.Width / _sizeFactor, kBitmap.Height / _sizeFactor);

            Bitmap UVBitmap = new Bitmap("uv.bmp");
            pageEventArgs.Graphics.DrawImage(UVBitmap, 235, 50, UVBitmap.Width / _sizeFactor, UVBitmap.Height / _sizeFactor);

            WriteCustomTopcoatBlockingEscapesBack(pageEventArgs.Graphics);
        }

        public void OnPrintPage(object sender, PrintPageEventArgs pageEventArgs) {
            if (_frontOfCard) {
                DrawCardFront(pageEventArgs);
                _frontOfCard = false;
                pageEventArgs.HasMorePages = _commandLineOptions.twoPages;
            }
            else {
                DrawCardBack(pageEventArgs);
            }
        }

        public void OnQueryPageSettings(object sender, QueryPageSettingsEventArgs queryEventArgs) {
            // use this opportunity to adjust the orientation for the back side of the card:
            if (!_frontOfCard) {
                queryEventArgs.PageSettings.Landscape = _commandLineOptions.portraitBack ? false : true;
            }
        }

        private void WriteMagstripeEscapes(Graphics graphics) {
            // emit some plain track 1, 2, 3 data. Assume IAT track configuration.
            string track1Escape = "~1ABC 123";
            string track2Escape = "~2456";
            string track3Escape = "~3789";

            using (Font font = new Font("Courier New", 6))
            using (SolidBrush brush = new SolidBrush(Color.CadetBlue))
            using (Pen pen = new Pen(Color.Red)) {
                graphics.DrawString(track1Escape, font, brush, 50, 50);
                graphics.DrawString(track2Escape, font, brush, 50, 50);
                graphics.DrawString(track3Escape, font, brush, 50, 50);
            }
        }

        private string GetTopcoatBlockingPrintTicketXml() {
            string topcoatBlockingXml = string.Empty;

            // front:
            switch (_commandLineOptions.topcoatBlockingFront) {
                case "":
                    // use the current driver settings.
                    break;

                case "custom":
                    // We will generate topcoat and blocking escapes for the card front
                    // in this application. Escapes override the PrintTicket settings.
                    break;

                case "all":
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_All;
                    break;

                // we need the 'exception' markup for the remaining settings:

                case "chip":
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_7816;
                    break;

                case "magjis":
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_JIS;
                    break;

                case "mag2":
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_2Track;
                    break;

                case "mag3":
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_3Track;
                    break;
            }

            // back:
            switch (_commandLineOptions.topcoatBlockingBack) {
                case "":
                    // use the current driver settings.
                    break;

                case "custom":
                    // We will generate topcoat and blocking escapes for the card back
                    // in this application. Escapes override the PrintTicket settings.
                    break;

                case "all":
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_All;
                    break;

                // we need the 'exception' markup for the remaining settings:

                case "chip":
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_7816;
                    break;

                case "magjis":
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_JIS;
                    break;

                case "mag2":
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_2Track;
                    break;

                case "mag3":
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except;
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_3Track;
                    break;
            }

            return topcoatBlockingXml;
        }

        private void WriteCustomTopcoatBlockingEscapesFront(Graphics graphics) {
            if (_commandLineOptions.topcoatBlockingFront != "custom")
                return;

            // a 'topcoat Add' escape will force topcoat OFF for the entire card side.

            // units are millimeters; landscape basis; top left width height:
            // a rectangle one inch down; two inches wide; 1 cm high.
            // units are millimeters; top left width height:
            string topCoatAddEsc = "~TA%25.4 0 50.8 10;";
            topCoatAddEsc += "40 60 7 7?";  // add a square, lower

            using (Font font = new Font("Courier New", 6))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.Black)) {
                graphics.DrawString(topCoatAddEsc, font, brush, 10, 10);
            }

            // a 'blocking' escape will override the driver settings:
            string blockingEsc = "~PB% 0 19 3 54;";

            using (Font font = new Font("Courier New", 6))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.Black)) {
                graphics.DrawString(blockingEsc, font, brush, 10, 20);
            }
        }

        private void WriteCustomTopcoatBlockingEscapesBack(Graphics graphics) {
            if (_commandLineOptions.topcoatBlockingBack != "custom")
                return;

            string topCoatAddEsc = "~TA%25.4 10 50.8 20;";

            using (Font font = new Font("Courier New", 6))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.Black)) {
                graphics.DrawString(topCoatAddEsc, font, brush, 10, 10);
            }

            // a 'blocking' escape will override the driver settings:
            string blockingEsc = "~PB% 0 23 3 54;";

            using (Font font = new Font("Times New Roman", 6))
            using (SolidBrush brush = new SolidBrush(Color.Pink))
            using (Pen pen = new Pen(Color.SeaShell)) {
                graphics.DrawString(blockingEsc, font, brush, 10, 20);
            }
        }
    }
}