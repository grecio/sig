////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK csharp laser sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dxp01sdk;

namespace laser {

    internal class laser {
        const string LASER_STATIC_SETUP_FILE_NAME              = "SampleCard_FrontOnly_Static";
        const string LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME    = "SampleCard_FrontOnly";
        const string LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME     = "SampleCard";
        const string STATIC_SETUP_ZIP_FILE_NAME                = "SampleCard_FrontOnly_Static.zip";
        const string SIMPLEX_SETUP_ZIP_FILE_NAME               = "SampleCard_FrontOnly.zip";
        const string DUPLEX_SETUP_ZIP_FILE_NAME                = "SampleCard.zip";

        public static void EncodeMagstripe(
            ref BidiSplWrap bidiSpl)
        {
            // hardcoded XML to encode all 3 tracks in IAT mode.
            // track 1 = "TRACK1", track 2 = "1122", track 3 = "321"
            string trackDataXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<magstripe>"
                + "<track number=\"1\"><base64Data>VFJBQ0sx</base64Data></track>"
                + "<track number=\"2\"><base64Data>MTEyMg==</base64Data></track>"
                + "<track number=\"3\"><base64Data>MzIx</base64Data></track>"
                + "</magstripe>";

            string resultXml = bidiSpl.SetPrinterData(strings.MAGSTRIPE_ENCODE, trackDataXML);
            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
        }

        public static string GetFullyPathedFName(
            string fileName,
            string extension)
        {
            string laserOutputPath = Path.Combine(Util.GetExePath(), "LaserFilesRetrievedFromPrinter");
            try {
                DirectoryInfo dirInfo = Directory.CreateDirectory(laserOutputPath);
            }
            catch (Exception ) {
                throw new Exception("WriteFile(), could not create directory " + laserOutputPath);
            }

            fileName += extension;
            string fullyPathedFName = Path.Combine(laserOutputPath, fileName);
            return fullyPathedFName;
        }
        public static void WriteFile(
            string fileName,
            byte[] fileBuffer,
            int bufferLen,
            string extension)
        {
            if ((fileBuffer == null) || (bufferLen == 0)) return;

            string fullyPathedFName = GetFullyPathedFName(fileName, extension);

            BinaryWriter writer = new BinaryWriter(File.Open(fullyPathedFName, FileMode.Create));
            writer.Write(fileBuffer);
            writer.Close();

        }
        public static void WriteFile(
            string fileName,
            string fileBuffer,
            int bufferLen,
            string extension)
        {
            if ((fileBuffer == null) || (bufferLen == 0)) return;

            string fullyPathedFName = GetFullyPathedFName(fileName, extension);

            File.WriteAllText(fullyPathedFName, fileBuffer);

        }
        public static void CreateLaserXMLFile(
            string fileName,
            string fileBuffer)
        {
            string trimmedBuffer = fileBuffer.Trim();
            if (trimmedBuffer.Length == 0)
            {  // Instead of returning empty _printerData, driver sometimes return "  "
                throw new Exception("CreateLaserXMLFile(): empty file - " + fileName);
            }

            WriteFile(fileName, trimmedBuffer, trimmedBuffer.Length, ".xml");
        }

        ////////////////////////////////////////////////////////////////////////////////
        // RetrieveLaserSetupFileList()
        // we're given a Printer Status XML fragment that has a CDATA section like this:
        // example Printer Status XML from laser printer in the version 7.2 driver:
        //
        //    <?xml version = "1.0"?>
        //    <!--Printer status xml.-->
        //    <PrinterStatus>
        //    <ClientID>{0BDB05E9 - 31B3 - 4060 - B538 - 2356FA01F6D5}< / ClientID>
        //    <WindowsJobID>0< / WindowsJobID>
        //    <PrinterJobID>102< / PrinterJobID>
        //    <ErrorCode>0< / ErrorCode>
        //    <ErrorSeverity>0< / ErrorSeverity>
        //    <ErrorString / >
        //    <DataFromPrinter>
        //    <![CDATA[< ? xml version = "1.0" encoding = "UTF-8" ? >
        //        <QuerySetupsResult> <LaserCardSetups>
        //        <LaserCardSetup name = "DriverStaticSetup" / >
        //        <LaserCardSetup name = "DriverVariableDuplexSetup" / >
        //        <LaserCardSetup name = "DriverVariableSimplexSetup" / >
        //        </LaserCardSetups> </QuerySetupsResult>]]>
        //     </DataFromPrinter>
        //     </PrinterStatus>
        public static void RetrieveLaserSetupFileList(
            ref BidiSplWrap bidiSpl,
            out List<string> laserSetupFileList)
        {
            laserSetupFileList = new List<string>();

            // Query all laser setup files present on the system
            string resultXml = bidiSpl.GetPrinterData(strings.LASER_QUERY_SETUP_FILESLIST);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }

            // Example LaserSetupFileList XML from laser printer in the version 7.2 driver:
            //    <?xml version="1.0" encoding="UTF-8"?>
            //      <QuerySetupsResult>
            //        < LaserCardSetups >
            //          < LaserCardSetup name="DriverSampleCardSetup" />
            //          < LaserCardSetup name="SampleCard" />
            //          < LaserCardSetup name="SampleCard_FrontOnly" />
            //          < LaserCardSetup name="SampleCard_FrontOnly_Static" />
            //       </ LaserCardSetups>
            //      </ QuerySetupsResult>
            CreateLaserXMLFile("LaserSetupFileList", printerStatusValues._dataFromPrinter);
            Util.ParseLaserSetupFileNames(resultXml, out laserSetupFileList);
            Console.WriteLine("Successfully retrieved laser setup file");
        }


        // we're given a Printer Status XML fragment that has a section like this:
        // example Printer Status XML from laser printer in the version 7.2 driver:

        // <?xml version="1.0"?>
        // <!--Printer status xml.-->
        // -<PrinterStatus>
        // <ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
        // <WindowsJobID>0</WindowsJobID>
        // <PrinterJobID>106</PrinterJobID>
        // <ErrorCode>0</ErrorCode>
        // <ErrorSeverity>0</ErrorSeverity>
        // <ErrorString/>
        // -<DataFromPrinter>
        //      <![CDATA[<?xml version="1.0" encoding="UTF-8"?> <QueryElementsResult> <ElementInformationList>
        //          <ElementInformation name="PHOTO" type="BINARY" side="FRONT" />
        //          <ElementInformation name="GIVEN_NAME" type="TEXT" side="FRONT" />
        //          <ElementInformation name="FAMILY_NAME" type="TEXT" side="FRONT" />
        //          <ElementInformation name="DOB" type="TEXT" side="FRONT" />
        //          <ElementInformation name="SIGNATURE" type="BINARY" side="FRONT" />
        //          <ElementInformation name="BARCODE_1D" type="TEXT" side="BACK" />
        //          <ElementInformation name="BARCODE_2D" type="BINARY" side="BACK" />
        //          </ElementInformationList> </QueryElementsResult> ]]>
        // </DataFromPrinter>
        // </PrinterStatus>

        public static void RetrieveLaserSetupElements(
            ref BidiSplWrap bidiSpl,
            List<string> laserSetupFileList)
        {
            foreach (var laserSetupFile in laserSetupFileList) {

                // Query laser element names for a setup file
                string input = Util.CreateLaserFileNameXML(laserSetupFile);
                byte[] UTF8Bytes = Encoding.UTF8.GetBytes(input);
                string resultXml = bidiSpl.GetPrinterData(strings.LASER_QUERY_ELEMENT_LIST, UTF8Bytes);

                PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
                if (0 != printerStatusValues._errorCode) {
                    throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
                }

                //  Example laserElementList XML from laser printer in the version 7.2 driver:
                // <? xml version="1.0" encoding="UTF-8"?>
                // -< QueryElementsResult>
                // -< ElementInformationList>
                //      < ElementInformation side="FRONT" type="BINARY" name="PHOTO"/>
                //      < ElementInformation side="FRONT" type="TEXT" name="GIVEN_NAME"/>
                //      < ElementInformation side="FRONT" type="TEXT" name="FAMILY_NAME"/>
                //      < ElementInformation side="FRONT" type="TEXT" name="DOB"/>
                //      < ElementInformation side="FRONT" type="BINARY" name="SIGNATURE"/>
                //      < ElementInformation side="BACK" type="TEXT" name="BARCODE_1D"/>
                //      < ElementInformation side="BACK" type="BINARY" name="BARCODE_2D"/>
                // </ ElementInformationList>
                // </ QueryElementsResult> 

                string laserElmentListXML = printerStatusValues._dataFromPrinter;
                CreateLaserXMLFile(laserSetupFile, laserElmentListXML);
                Console.WriteLine("Successfully retrieved laser element file - " + laserSetupFile);
            }
        }
        public static void RetrieveLaserSetup(ref BidiSplWrap bidiSpl)
        {
            // Query all laser setup files present on the system
            List<string> laserSetupFileList;

            RetrieveLaserSetupFileList(ref bidiSpl, out laserSetupFileList);

            // Query laser element names for all setup files
            RetrieveLaserSetupElements(ref bidiSpl, laserSetupFileList);
        }

        public static byte[] ReadFileData(
            string fileName)
        {
            byte[] binaryData;

            try
            {
                var inFile = new System.IO.FileStream(fileName,
                    System.IO.FileMode.Open,
                    System.IO.FileAccess.Read);
                binaryData = new Byte[inFile.Length];
                inFile.Read(binaryData, 0, (int)inFile.Length);
                inFile.Close();
            }
            catch (System.Exception exp)
            {
                throw new Exception("An exception occurred reading file " + fileName + "; exception: " + exp.Message);
            }
            return binaryData;
        }

        // Example ImportZipFilesToPrinter XML from laser printer in the version 7.2 driver
        // <?xml version="1.0"?>
        // <!--Printer status xml.-->
        // -<PrinterStatus>
        // <ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
        // <WindowsJobID>0</WindowsJobID>
        // <PrinterJobID>0</PrinterJobID>
        // <ErrorCode>0</ErrorCode>
        // <ErrorSeverity>0</ErrorSeverity>
        // <ErrorString/>
        // -<DataFromPrinter>
        // <![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>1</Status><Base64Data> </Base64Data></LaserResponse>]]>
        // </DataFromPrinter>
        // </PrinterStatus>
        public static void ImportZipFilesToPrinter(
            ref BidiSplWrap bidiSpl,
            string fileName,
            bool overwrite)
        {
            // Read file in a buffer
            byte[] laserZipFileBuffer = ReadFileData(fileName);
            string base64EncodedData = Convert.ToBase64String(laserZipFileBuffer);

            string laserZipFileXML = Util.CreateImportZipFileXML(fileName, overwrite, base64EncodedData);
            byte[] laserZipFileXMLBytes = Encoding.UTF8.GetBytes(laserZipFileXML);

            // send the data to driver using Windows bidi interface
            string printerStatusXML = bidiSpl.SetPrinterData(strings.LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER, laserZipFileXMLBytes);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
            if (518 == printerStatusValues._errorCode) {
                // laser module returned an error. Parse the laser data returned from driver
                Console.WriteLine("Laser module returned 518 error: " + fileName);
            }
            else if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }

            // Example laserStatus XML from laser printer in the version 7.2 driver:
            // <?xml version="1.0"?>
            // < !--laser response xml-->
            // -< LaserResponse>
            //      < Status >1</Status>
            //      < Base64Data > </Base64Data>
            // </ LaserResponse>
            
            string laserStatusXML = printerStatusValues._dataFromPrinter;
            LaserStatusValues laserStatusValues = Util.ParseLaserStatusXML(laserStatusXML);
            if (laserStatusValues._success == 1) {
                // Success. Imported zip file to printer
                Console.WriteLine("Successfully imported zip file to printer - " + fileName);
            }
            else if (518 == printerStatusValues._errorCode) {
                // Firmware issued conflict file list error
                CreateLaserFile("LaserFileConflictList", laserStatusValues._base64Data);
                Console.WriteLine("Import failed. Created laserFileConflictList.xml file");
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
            else {
                Console.WriteLine("Import failed");
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
        }

        
        public static void ExportZipFilesFromPrinter(
            ref BidiSplWrap bidiSpl,
            string laserZipFileName)
        {
            string laserZipFileXML = Util.CreateLaserFileNameXML(laserZipFileName);
            byte[] laserExportZipFileBytes = Encoding.UTF8.GetBytes(laserZipFileXML);
            string statusXML = bidiSpl.GetPrinterData(strings.LASER_UPLOAD_ZIP_FILE_FROM_PRINTER, laserExportZipFileBytes);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(statusXML);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }

            // Example printer status XML from laser printer in the version 7.2 driver in case of
            // export failure:
            // <?xml version="1.0"?>
            // <!--Printer status xml.-->
            // -<PrinterStatus>
            // <ClientID>{20EAB827-59D6-43B1-89B8-03230164D174}</ClientID>
            // <WindowsJobID>0</WindowsJobID>
            // <PrinterJobID>0</PrinterJobID>
            // <ErrorCode>522</ErrorCode>
            // <ErrorSeverity>2</ErrorSeverity>
            // <ErrorString>Message 522: File export failed</ErrorString>
            // -<DataFromPrinter>
            // <![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>0</Status><Base64Data> </Base64Data></LaserResponse>]]>
            // </DataFromPrinter>
            // </PrinterStatus

            // Example laserStatus XML from laser printer in the version 7.2 driver in case of
            // export success:
            // <?xml version="1.0"?>
            // <!--laser response xml-->
            // -<LaserResponse>
            // <Status>1</Status>
            // <Base64Data> </Base64Data>
            // </LaserResponse>

            string laserStatusXML = printerStatusValues._dataFromPrinter;
            LaserStatusValues laserStatusValues = Util.ParseLaserStatusXML(laserStatusXML);
            if (laserStatusValues._success == 1)
            {
                // Success. Retreived zip file from printer
                laserZipFileName += ".zip";
                CreateLaserFile(laserZipFileName, laserStatusValues._base64Data);
                Console.WriteLine("Successfully exported zip file from printer - " + laserZipFileName);
            }
            else {
                throw new Exception("ExportZipFilesFromPrinter: failed to export file");
            }
        }
        public static void CreateLaserFile(
            string fileName,
            string laserFile)
        {
            string trimmedBuffer = laserFile.Trim();
            if (trimmedBuffer.Length == 0) {  
                throw new Exception("CreateLaserFile: empty laser data returned by printer.");
            }
            byte[] decodedLaserFile = Convert.FromBase64String(trimmedBuffer);

            int zipNdx = fileName.IndexOf(".zip");
            if (zipNdx != -1) {
                WriteFile(fileName, decodedLaserFile, decodedLaserFile.Length, "");
            }
            else {
                string decodedLaserFileString = Encoding.Default.GetString(decodedLaserFile);
                CreateLaserXMLFile(fileName, decodedLaserFileString);
            }
        }

        public static void SetUpLaserPrinter(
            ref BidiSplWrap bidiSpl,
            bool staticLayout,
            PrinterOptionsValues printerOptionsValues)
        {
            string setupZipFileName;
            string setupFileName;

            // Check if printer is setup correctly to print laser file
            if (staticLayout) {
                setupFileName = LASER_STATIC_SETUP_FILE_NAME;
                setupZipFileName = STATIC_SETUP_ZIP_FILE_NAME;
            }
            else {
                // Lets figure out if we should print simplex cards or duplex cards.
                bool doTwoSided = printerOptionsValues._optionDuplex == "Auto" ||
                    printerOptionsValues._optionDuplex == "Manual";

                if (doTwoSided) {
                    setupFileName = LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME;
                    setupZipFileName = DUPLEX_SETUP_ZIP_FILE_NAME;
                }
                else {
                    setupFileName = LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME;
                    setupZipFileName = SIMPLEX_SETUP_ZIP_FILE_NAME;
                }
            }

            // Query all laser setup files present on the system
            List<string> laserSetupFileList;
            RetrieveLaserSetupFileList(ref bidiSpl, out laserSetupFileList);

            // Check if setupName is present in laserSetupFileList
            bool laserSetupFilePresent = false;
            foreach (var laserSetupFile in laserSetupFileList) {
                if (laserSetupFile == setupFileName) {
                    laserSetupFilePresent = true;
                    break;
                }
            }

            if (!laserSetupFilePresent) {
                // Import laser setup zip file if it is not present on laser printer
                ImportZipFilesToPrinter(ref bidiSpl, setupZipFileName, true);
            }
        }

        public static void LaserEngraveBinary(
            ref BidiSplWrap bidiSpl,
            string elementName,
            byte[] buffer)
        {
            int bufferLength = buffer.Length;
            if (bufferLength == 0) {
                throw new Exception("LaserEngraveBinary: empty buffer.");
            }

            string  base64EncodedData = Convert.ToBase64String(buffer);
            string laserBinaryStr = Util.CreateLaserEngraveBinaryXML(elementName, base64EncodedData);
            byte[] laserXML = Encoding.UTF8.GetBytes(laserBinaryStr);
            string resultXml = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_BINARY, laserXML);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
        }

        public static void LaserEngraveText(
            ref BidiSplWrap bidiSpl,
            string elementName,
            string laserText)
        {
            string laserTextStr = Util.CreateLaserEngraveTextXML(elementName, laserText);
            byte[] laserXML = Encoding.UTF8.GetBytes(laserTextStr);
            string resultXml = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_TEXT, laserXML);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
        }

        public static void LaserEngraveSetupFileName(
            ref BidiSplWrap bidiSpl,
            string laserSetUpFileName,
            int count)
        {
            string laserXML = Util.CreateLaserSetupFileNameXML(laserSetUpFileName, count);
            byte[] laserBytes = Encoding.UTF8.GetBytes(laserXML);
            string resultXml = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_SETUP_FILE_NAME, laserBytes);

            PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(resultXml);
            if (0 != printerStatusValues._errorCode) {
                throw new BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode);
            }
        }

        // Send hardcoded laser data
        // Laser card design is specified in LaserSampleSetup_FrontOnly.ccl is present in firmware.
        // Laser setup file has five elements:
        public static void LaserEngraveSimplexCard(
            ref BidiSplWrap bidiSpl)
        {
            // Read photo file in a buffer
            byte[] photoBuffer = ReadFileData("ARMSTROT.JPG");

            // Read signature file in a buffer
            byte[] signatureBuffer = ReadFileData("ARMSTROT.TIF");

            // Specify the laser setup file name, and variable elements count
            LaserEngraveSetupFileName(ref bidiSpl, LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME, 5);

            // Specify the laser data for the elements defined in the laser setup file 
            LaserEngraveBinary(ref bidiSpl, "PHOTO", photoBuffer); // element "PHOTO" has been defined as a variable binary element 
            LaserEngraveText(ref bidiSpl, "GIVEN_NAME", "John M."); // element "GIVEN_NAME" has been defined as a variable text element 
            LaserEngraveText(ref bidiSpl, "FAMILY_NAME", "Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
            LaserEngraveText(ref bidiSpl, "DOB", "01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
            LaserEngraveBinary(ref bidiSpl, "SIGNATURE", signatureBuffer); // element "SIGNATURE" has been defined as a variable binary element 
        }

        public static void LaserEngraveDuplexCard(
            ref BidiSplWrap bidiSpl)
        {
            // Read photo file in a buffer
            byte[] photoBuffer = ReadFileData("ARMSTROT.JPG");

            // Read signature file in a buffer
            byte[] signatureBuffer = ReadFileData("ARMSTROT.TIF");

            // Initialize 2D barcode buffer with 7-bit ASCII data
            string barcode2DStr = "This is PDF417 barcode encoded sample text printed on a CL900 from the driver SDK sample. Visit: http://www.entrustdatacard.com";
            byte[] barcodeBuffer = Encoding.ASCII.GetBytes(barcode2DStr);

            // Specify the laser setup file name, and variable elements count
            LaserEngraveSetupFileName(ref bidiSpl, LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME, 7);

            // Specify the front side of laser data for the elements defined in the laser setup file 
            LaserEngraveBinary(ref bidiSpl, "PHOTO", photoBuffer); // element "PHOTO" has been defined as a variable binary element 
            LaserEngraveText(ref bidiSpl, "GIVEN_NAME", "John M."); // element "GIVEN_NAME" has been defined as a variable text element 
            LaserEngraveText(ref bidiSpl, "FAMILY_NAME", "Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
            LaserEngraveText(ref bidiSpl, "DOB", "01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
            LaserEngraveBinary(ref bidiSpl, "SIGNATURE", signatureBuffer); // element "SIGNATURE" has been defined as a variable binary element 

            // Specify the back side of laser data for the elements defined in the laser setup file 
            LaserEngraveText(ref bidiSpl, "BARCODE_1D", "0123456789"); // element "BARCODE_1D" has been defined as a variable text element
            LaserEngraveBinary(ref bidiSpl, "BARCODE_2D", barcodeBuffer); // element "BARCODE_2D" has been defined as a variable binary element 
        }

        // Send hardcoded static laser setup file
        public static void LaserEngraveStatic(
            ref BidiSplWrap bidiSpl)
        {
            // Specify the static laser setup file name. Variable elements count is 0
            LaserEngraveSetupFileName(ref bidiSpl, LASER_STATIC_SETUP_FILE_NAME, 0);
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

                // Check if the printer is in the Printer_Ready state:
                if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
                    throw new Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
                }

                if ("Installed" != printerOptionsValues._printHead) {
                    throw new Exception(commandLineOptions.printerName + " does not have a print head installed.");
                }

                // check if ISO magstripe option is installed. For brevity, JIS is ignored in this sample.
                if (commandLineOptions.encodeMagstripe && !printerOptionsValues._optionMagstripe.Contains("ISO")) {
                    throw new Exception(commandLineOptions.printerName + " does not have an ISO magnetic stripe unit installed.");
                }

                // check if laser option is installed
                if ("Installed" != printerOptionsValues._optionLaser) {
                    throw new Exception(commandLineOptions.printerName + " does not have a laser module installed.");
                }

                if (commandLineOptions.retrieveLaserSetup) {
                    RetrieveLaserSetup(ref bidiSpl);
                }

                if (commandLineOptions.laserImportFile) {
                    ImportZipFilesToPrinter(ref bidiSpl, SIMPLEX_SETUP_ZIP_FILE_NAME, true);
                }

                if (commandLineOptions.laserExportFile) {
                    ExportZipFilesFromPrinter(ref bidiSpl, LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME);
                }


                // Optional: Check if printer is setup correctly to print laser file
                // This can be time consuming optional operation. Users can skip this step
                // as most printers are setup correctly.
                // SetUpLaserPrinter(ref bidiSpl, commandLineOptions.laserEngraveStatic, printerOptionsValues);

                // Start laser job
                string hopperID = "1";

                printerJobID = Util.StartJob(
                    bidiSpl,
                    (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID);

                if (commandLineOptions.encodeMagstripe) {
                    EncodeMagstripe(ref bidiSpl);
                }

                bool doTwoSided = printerOptionsValues._optionDuplex == "Auto" ||
                    printerOptionsValues._optionDuplex == "Manual";

                if (commandLineOptions.printText) {
                    // do some simple text and graphics printing.
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);
                }

                // Send laser data
                if (commandLineOptions.laserEngraveStatic)
                {
                    LaserEngraveStatic(ref bidiSpl);
                }
                else
                {
                    if (doTwoSided) {
                        LaserEngraveDuplexCard(ref bidiSpl);
                    }
                    else {
                        LaserEngraveSimplexCard(ref bidiSpl);
                    }
                }

                // End laser job
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
}