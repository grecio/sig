Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports dxp01sdk

Namespace laser

    Friend Class laser

        Const LASER_STATIC_SETUP_FILE_NAME As String = "SampleCard_FrontOnly_Static"
        Const LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME As String = "SampleCard_FrontOnly"
        Const LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME As String = "SampleCard"
        Const STATIC_SETUP_ZIP_FILE_NAME As String = "SampleCard_FrontOnly_Static.zip"
        Const SIMPLEX_SETUP_ZIP_FILE_NAME As String = "SampleCard_FrontOnly.zip"
        Const DUPLEX_SETUP_ZIP_FILE_NAME As String = "SampleCard.zip"

        Public Shared Sub EncodeMagstripe(ByRef bidiSpl As BidiSplWrap)

            ' hardcoded XML to encode all 3 tracks in IAT mode.
            ' track 1 = "TRACK1", track 2 = "1122", track 3 = "321"

            Dim trackDataXML As String = "<?xml version=""1.0"" encoding=""utf-8""?>" & "<magstripe>" & "<track number=""1""><base64Data>VFJBQ0sx</base64Data></track>" & "<track number=""2""><base64Data>MTEyMg==</base64Data></track>" & "<track number=""3""><base64Data>MzIx</base64Data></track>" & "</magstripe>"
            Dim resultXml As String = bidiSpl.SetPrinterData(strings.MAGSTRIPE_ENCODE, trackDataXML)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)
            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If
        End Sub
        Public Shared Function GetFullyPathedFName(ByVal fileName As String, ByVal extension As String) As String
            Dim laserOutputPath As String = Path.Combine(Util.GetExePath(), "LaserFilesRetrievedFromPrinter")
            Try
                Dim dirInfo As DirectoryInfo = Directory.CreateDirectory(laserOutputPath)
            Catch e As Exception
                Throw New Exception("WriteFile(), could not create directory " & laserOutputPath)
            End Try

            fileName += extension
            Dim fullyPathedFName As String = Path.Combine(laserOutputPath, fileName)
            Return fullyPathedFName
        End Function
        Public Shared Sub WriteFile(ByVal fileName As String, ByVal fileBuffer As Byte(), ByVal bufferLen As Integer, ByVal extension As String)
            If (fileBuffer Is Nothing) OrElse (bufferLen = 0) Then Return
            Dim fullyPathedFName As String = GetFullyPathedFName(fileName, extension)
            Dim writer As BinaryWriter = New BinaryWriter(File.Open(fullyPathedFName, FileMode.Create))
            writer.Write(fileBuffer)
            writer.Close()
        End Sub

        Public Shared Sub WriteFile(ByVal fileName As String, ByVal fileBuffer As String, ByVal bufferLen As Integer, ByVal extension As String)
            If (fileBuffer Is Nothing) OrElse (bufferLen = 0) Then Return
            Dim fullyPathedFName As String = GetFullyPathedFName(fileName, extension)
            File.WriteAllText(fullyPathedFName, fileBuffer)
        End Sub


        Public Shared Sub CreateLaserXMLFile(ByVal fileName As String, ByVal fileBuffer As String)
            Dim trimmedBuffer As String = fileBuffer.Trim()
            If trimmedBuffer.Length = 0 Then
                Throw New Exception("CreateLaserXMLFile(): empty file - " & fileName)
            End If

            WriteFile(fileName, trimmedBuffer, trimmedBuffer.Length, ".xml")
        End Sub

        '
        ' RetrieveLaserSetupFileList()
        ' we're given a Printer Status XML fragment that has a CDATA section like this:
        ' example Printer Status XML from laser printer in the version 7.2 driver:
        '
        '    <?xml version = "1.0"?>
        '    <!--Printer status xml.-->
        '    <PrinterStatus>
        '    <ClientID>{0BDB05E9 - 31B3 - 4060 - B538 - 2356FA01F6D5}< / ClientID>
        '    <WindowsJobID>0< / WindowsJobID>
        '    <PrinterJobID>102< / PrinterJobID>
        '    <ErrorCode>0< / ErrorCode>
        '    <ErrorSeverity>0< / ErrorSeverity>
        '    <ErrorString / >
        '    <DataFromPrinter>
        '    <![CDATA[< ? xml version = "1.0" encoding = "UTF-8" ? >
        '        <QuerySetupsResult> <LaserCardSetups>
        '        <LaserCardSetup name = "DriverStaticSetup" / >
        '        <LaserCardSetup name = "DriverVariableDuplexSetup" / >
        '        <LaserCardSetup name = "DriverVariableSimplexSetup" / >
        '        </LaserCardSetups> </QuerySetupsResult>]]>
        '     </DataFromPrinter>
        '     </PrinterStatus>
        Public Shared Sub RetrieveLaserSetupFileList(ByRef bidiSpl As BidiSplWrap, <Out> ByRef laserSetupFileList As List(Of String))
            laserSetupFileList = New List(Of String)()
            ' Query all laser setup files present on the system
            Dim resultXml As String = bidiSpl.GetPrinterData(strings.LASER_QUERY_SETUP_FILESLIST)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)
            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If

            ' Example LaserSetupFileList XML from laser printer in the version 7.2 driver
            ' <?xml version="1.0" encoding="UTF-8"?>
            ' <QuerySetupsResult>
            '   <LaserCardSetups>
            '       <LaserCardSetup name = "DriverSampleCardSetup" />
            '       <LaserCardSetup name="SampleCard"/>
            '       <LaserCardSetup name = "SampleCard_FrontOnly" />
            '       <LaserCardSetup name="SampleCard_FrontOnly_Static"/>
            '   </LaserCardSetups>
            ' </QuerySetupsResult>

            CreateLaserXMLFile("LaserSetupFileList", printerStatusValues._dataFromPrinter)
            Util.ParseLaserSetupFileNames(resultXml, laserSetupFileList)
        End Sub

        ' we're given a Printer Status XML fragment that has a section like this:
        ' example Printer Status XML from laser printer in the version 7.2 driver:

        ' <?xml version="1.0"?>
        ' <!--Printer status xml.-->
        ' -<PrinterStatus>
        ' <ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
        ' <WindowsJobID>0</WindowsJobID>
        ' <PrinterJobID>106</PrinterJobID>
        ' <ErrorCode>0</ErrorCode>
        ' <ErrorSeverity>0</ErrorSeverity>
        ' <ErrorString/>
        ' -<DataFromPrinter>
        '      <![CDATA[<?xml version="1.0" encoding="UTF-8"?> <QueryElementsResult> <ElementInformationList>
        '          <ElementInformation name="PHOTO" type="BINARY" side="FRONT" />
        '          <ElementInformation name="GIVEN_NAME" type="TEXT" side="FRONT" />
        '          <ElementInformation name="FAMILY_NAME" type="TEXT" side="FRONT" />
        '          <ElementInformation name="DOB" type="TEXT" side="FRONT" />
        '          <ElementInformation name="SIGNATURE" type="BINARY" side="FRONT" />
        '          <ElementInformation name="BARCODE_1D" type="TEXT" side="BACK" />
        '          <ElementInformation name="BARCODE_2D" type="BINARY" side="BACK" />
        '          </ElementInformationList> </QueryElementsResult> ]]>
        ' </DataFromPrinter>
        ' </PrinterStatus>
        Public Shared Sub RetrieveLaserSetupElements(ByRef bidiSpl As BidiSplWrap, ByVal laserSetupFileList As List(Of String))
            For Each laserSetupFile In laserSetupFileList
                ' Query laser element names for a setup file
                Dim input As String = Util.CreateLaserFileNameXML(laserSetupFile)
                Dim asciiBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(input)
                Dim resultXml As String = bidiSpl.GetPrinterData(strings.LASER_QUERY_ELEMENT_LIST, asciiBytes)
                Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)
                If 0 <> printerStatusValues._errorCode Then
                    Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
                End If

                '  Example laserElementList XML from laser printer in the version 7.2 driver
                ' <? xml version="1.0" encoding="UTF-8"?>
                ' -< QueryElementsResult>
                ' -< ElementInformationList>
                '      < ElementInformation side="FRONT" type="BINARY" name="PHOTO"/>
                '      < ElementInformation side="FRONT" type="TEXT" name="GIVEN_NAME"/>
                '      < ElementInformation side="FRONT" type="TEXT" name="FAMILY_NAME"/>
                '      < ElementInformation side="FRONT" type="TEXT" name="DOB"/>
                '      < ElementInformation side="FRONT" type="BINARY" name="SIGNATURE"/>
                '      < ElementInformation side="BACK" type="TEXT" name="BARCODE_1D"/>
                '      < ElementInformation side="BACK" type="BINARY" name="BARCODE_2D"/>
                ' </ ElementInformationList>
                ' </ QueryElementsResult> 

                Dim laserElmentListXML As String = printerStatusValues._dataFromPrinter
                CreateLaserXMLFile(laserSetupFile, laserElmentListXML)
                Console.WriteLine("Successfully retrieved laser element file - " & laserSetupFile)
            Next
        End Sub

        Public Shared Sub RetrieveLaserSetup(ByRef bidiSpl As BidiSplWrap)
            Dim laserSetupFileList As List(Of String) = New List(Of String)()

            ' Query all laser setup files present on the system
            RetrieveLaserSetupFileList(bidiSpl, laserSetupFileList)

            ' Query laser element names for all setup files
            RetrieveLaserSetupElements(bidiSpl, laserSetupFileList)
        End Sub

        Public Shared Function ReadFileData(ByVal fileName As String) As Byte()
            Dim binaryData As Byte()
            Try
                Dim inFile = New System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                binaryData = New Byte(inFile.Length - 1) {}
                inFile.Read(binaryData, 0, CInt(inFile.Length))
                inFile.Close()
            Catch exp As System.Exception
                Throw New Exception("An exception occurred reading file " & fileName & "; exception: " & exp.Message)
            End Try

            Return binaryData
        End Function

        ' Example ImportZipFilesToPrinter XML from laser printer in the version 7.2 driver
        ' <?xml version="1.0"?>
        ' <!--Printer status xml.-->
        ' -<PrinterStatus>
        ' <ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
        ' <WindowsJobID>0</WindowsJobID>
        ' <PrinterJobID>0</PrinterJobID>
        ' <ErrorCode>0</ErrorCode>
        ' <ErrorSeverity>0</ErrorSeverity>
        ' <ErrorString/>
        ' -<DataFromPrinter>
        ' <![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>1</Status><Base64Data> </Base64Data></LaserResponse>]]>
        ' </DataFromPrinter>
        ' </PrinterStatus>
        Public Shared Sub ImportZipFilesToPrinter(ByRef bidiSpl As BidiSplWrap, ByVal fileName As String, ByVal overwrite As Boolean)
            ' Read file in a buffer
            Dim laserZipFileBuffer As Byte() = ReadFileData(fileName)

            ' Format buffer to base64 as binary expects that data in base64
            ' allocate a big buffer for the ATL:Base64Encode() Function. 
            Dim base64EncodedData As String = Convert.ToBase64String(laserZipFileBuffer)
            Dim laserZipFileXML As String = Util.CreateImportZipFileXML(fileName, overwrite, base64EncodedData)
            Dim laserZipFileXMLBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(laserZipFileXML)
            Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER, laserZipFileXMLBytes)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)
            If 518 = printerStatusValues._errorCode Then
                ' laser module returned an error. Parse the laser data returned from driver
                Console.WriteLine("Laser module returned 518 error.")
            ElseIf 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If

            ' Example laserStatus XML from laser printer in the version 7.2 driver
            ' <?xml version="1.0"?>
            ' < !--laser response xml-->
            ' -< LaserResponse>
            '      < Status >1</Status>
            '      < Base64Data > </Base64Data>
            ' </ LaserResponse>

            Dim laserStatusXML As String = printerStatusValues._dataFromPrinter
            Dim laserStatusValues As LaserStatusValues = Util.ParseLaserStatusXML(laserStatusXML)
            If laserStatusValues._success = 1 Then
                ' Success. Imported zip file to printer
                Console.WriteLine("Successfully imported zip file to printer - " & fileName)
            ElseIf 518 = printerStatusValues._errorCode Then
                ' Firmware issued conflict file list error
                CreateLaserFile("LaserFileConflictList", laserStatusValues._base64Data)
                Console.WriteLine("Import failed. Created laserFileConflictList.xml file")
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If
        End Sub


        Public Shared Sub ExportZipFilesFromPrinter(ByRef bidiSpl As BidiSplWrap, ByVal laserZipFileName As String)
            Dim laserZipFileXML As String = Util.CreateLaserFileNameXML(laserZipFileName)
            Dim laserExportZipFileBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(laserZipFileXML)
            Dim statusXML As String = bidiSpl.GetPrinterData(strings.LASER_UPLOAD_ZIP_FILE_FROM_PRINTER, laserExportZipFileBytes)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(statusXML)
            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If

            ' Example printer status XML from laser printer in the version 7.2 driver in case of
            ' export failure : 
            ' <?xml version="1.0"?>
            ' <!--Printer status xml.-->
            ' -<PrinterStatus>
            ' <ClientID>{20EAB827-59D6-43B1-89B8-03230164D174}</ClientID>
            ' <WindowsJobID>0</WindowsJobID>
            ' <PrinterJobID>0</PrinterJobID>
            ' <ErrorCode>522</ErrorCode>
            ' <ErrorSeverity>2</ErrorSeverity>
            ' <ErrorString>Message 522: File export failed</ErrorString>
            ' -<DataFromPrinter>
            ' <![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>0</Status><Base64Data> </Base64Data></LaserResponse>]]>
            ' </DataFromPrinter>
            ' </PrinterStatus

            ' Example laserStatus XML from laser printer in the version 7.2 driver in case of
            ' export success: 
            '<?xml version="1.0"?>
            '<!--laser response xml-->
            '-<LaserResponse>
            '<Status>1</Status>
            '<Base64Data> </Base64Data>
            '</LaserResponse>
            '

            Dim laserStatusXML As String = printerStatusValues._dataFromPrinter
            Dim laserStatusValues As LaserStatusValues = Util.ParseLaserStatusXML(laserStatusXML)
            If laserStatusValues._success = 1 Then
                ' Success. Retreived zip file from printer
                laserZipFileName += ".zip"
                CreateLaserFile(laserZipFileName, laserStatusValues._base64Data)
            Else
                Throw New Exception("ExportZipFilesFromPrinter: failed to export file")
            End If
        End Sub

        Public Shared Sub CreateLaserFile(ByVal fileName As String, ByVal laserFile As String)
            Dim trimmedBuffer As String = laserFile.Trim()
            If trimmedBuffer.Length = 0 Then
                Throw New Exception("CreateLaserFile: empty laser data returned by printer.")
            End If

            Dim decodedLaserFile As Byte() = Convert.FromBase64String(trimmedBuffer)
            Dim zipNdx As Integer = fileName.IndexOf(".zip")
            If zipNdx <> -1 Then
                WriteFile(fileName, decodedLaserFile, decodedLaserFile.Length, "")
            Else
                Dim decodedLaserFileString As String = Encoding.[Default].GetString(decodedLaserFile)
                CreateLaserXMLFile(fileName, decodedLaserFileString)
            End If
        End Sub

        Public Shared Sub SetUpLaserPrinter(ByRef bidiSpl As BidiSplWrap, ByVal staticLayout As Boolean, ByVal printerOptionsValues As PrinterOptionsValues)
            Dim setupZipFileName As String
            Dim setupFileName As String

            ' Check if printer Is setup correctly to print laser file
            If staticLayout Then
                setupFileName = LASER_STATIC_SETUP_FILE_NAME
                setupZipFileName = STATIC_SETUP_ZIP_FILE_NAME
            Else
                ' Lets figure out if we should print simplex cards Or duplex cards.
                Dim doTwoSided As Boolean = printerOptionsValues._optionDuplex = "Auto" OrElse printerOptionsValues._optionDuplex = "Manual"
                If doTwoSided Then
                    setupFileName = LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME
                    setupZipFileName = DUPLEX_SETUP_ZIP_FILE_NAME
                Else
                    setupFileName = LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME
                    setupZipFileName = SIMPLEX_SETUP_ZIP_FILE_NAME
                End If
            End If

            ' Query all laser setup files present on the system
            Dim laserSetupFileList As List(Of String) = New List(Of String)()
            RetrieveLaserSetupFileList(bidiSpl, laserSetupFileList)

            ' Check if setupName Is present in laserSetupFileList
            Dim laserSetupFilePresent As Boolean = False
            For Each laserSetupFile In laserSetupFileList
                ' Query laser element names for a setup file
                If laserSetupFile = setupFileName Then
                    laserSetupFilePresent = True
                    Exit For
                End If
            Next

            If Not laserSetupFilePresent Then
                ' Import laser setup zip file if it Is Not present on laser printer
                ImportZipFilesToPrinter(bidiSpl, setupZipFileName, True)
            End If
        End Sub

        Public Shared Sub LaserEngraveBinary(ByRef bidiSpl As BidiSplWrap, ByVal elementName As String, ByVal buffer As Byte())
            Dim bufferLength As Integer = buffer.Length
            If bufferLength = 0 Then
                Throw New Exception("LaserEngraveBinary: empty buffer.")
            End If

            ' Format buffer to base64 as binary expects that data in base64
            ' allocate a big buffer for the ATL:Base64Encode() Function. 
            Dim base64EncodedData As String = Convert.ToBase64String(buffer)
            Dim laserBinaryStr As String = Util.CreateLaserEngraveBinaryXML(elementName, base64EncodedData)
            Dim laserXML As Byte() = Encoding.UTF8.GetBytes(laserBinaryStr)
            Dim resultXml As String = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_BINARY, laserXML)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)
            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If
        End Sub

        Public Shared Sub LaserEngraveText(ByRef bidiSpl As BidiSplWrap, ByVal elementName As String, ByVal laserText As String)
            Dim laserTextStr As String = Util.CreateLaserEngraveTextXML(elementName, laserText)
            Dim laserXML As Byte() = System.Text.Encoding.UTF8.GetBytes(laserTextStr)

            Dim resultXml As String = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_TEXT, laserXML)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)

            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If
        End Sub

        Public Shared Sub LaserEngraveSetupFileName(ByRef bidiSpl As BidiSplWrap, ByVal laserSetUpFileName As String, ByVal count As Integer)
            Dim laserXML As String = Util.CreateLaserSetupFileNameXML(laserSetUpFileName, count)
            Dim laserBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(laserXML)

            Dim resultXml As String = bidiSpl.SetPrinterData(strings.LASER_ENGRAVE_SETUP_FILE_NAME, laserBytes)
            Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(resultXml)

            If 0 <> printerStatusValues._errorCode Then
                Throw New BidiException(printerStatusValues._errorString, printerStatusValues._printerJobID, printerStatusValues._errorCode)
            End If
        End Sub

        ' Send hardcoded laser data
        ' Laser card design Is specified in LaserSampleSetup_FrontOnly.ccl is present in firmware.
        ' Laser setup file has five elements
        Public Shared Sub LaserEngraveSimplexCard(ByRef bidiSpl As BidiSplWrap)

            ' Read photo file in a buffer
            Dim photoBuffer As Byte() = ReadFileData("ARMSTROT.JPG")

            ' Read signature file in a buffer
            Dim signatureBuffer As Byte() = ReadFileData("ARMSTROT.TIF")

            ' Specify the laser setup file name, And variable elements count
            LaserEngraveSetupFileName(bidiSpl, LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME, 5)

            ' Specify the laser data for the elements defined in the laser setup file 
            LaserEngraveBinary(bidiSpl, "PHOTO", photoBuffer) ' element "PHOTO" has been defined As a variable binary element 
            LaserEngraveText(bidiSpl, "GIVEN_NAME", "John M.") ' element "GIVEN_NAME" has been defined As a variable text element 
            LaserEngraveText(bidiSpl, "FAMILY_NAME", "Doe") ' element "FAMILY_NAME" has been defined As a variable text element 
            LaserEngraveText(bidiSpl, "DOB", "01 / 01 / 1985") ' element "DOB" has been defined As a variable text element
            LaserEngraveBinary(bidiSpl, "SIGNATURE", signatureBuffer) ' element "SIGNATURE" has been defined As a variable binary element 
        End Sub

        Public Shared Sub LaserEngraveDuplexCard(ByRef bidiSpl As BidiSplWrap)

            ' Read photo file in a buffer
            Dim photoBuffer As Byte() = ReadFileData("ARMSTROT.JPG")

            ' Read signature file in a buffer
            Dim signatureBuffer As Byte() = ReadFileData("ARMSTROT.TIF")

            ' Initialize 2D barcode buffer with 7-bit ASCII data
            Dim barcode2DStr As String = "This is PDF417 barcode encoded sample text printed on a CL900 from the driver SDK sample. Visit: http://www.entrustdatacard.com"
            Dim barcodeBuffer As Byte() = System.Text.Encoding.ASCII.GetBytes(barcode2DStr)

            ' Specify the laser setup file name, And variable elements count
            LaserEngraveSetupFileName(bidiSpl, LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME, 7)

            ' Specify the front side of laser data for the elements defined in the laser setup file 
            LaserEngraveBinary(bidiSpl, "PHOTO", photoBuffer) ' element "PHOTO" has been defined As a variable binary element 
            LaserEngraveText(bidiSpl, "GIVEN_NAME", "John M.") ' element "GIVEN_NAME" has been defined As a variable text element 
            LaserEngraveText(bidiSpl, "FAMILY_NAME", "Doe") ' element "FAMILY_NAME" has been defined As a variable text element 
            LaserEngraveText(bidiSpl, "DOB", "01 / 01 / 1985") ' element "DOB" has been defined As a variable text element
            LaserEngraveBinary(bidiSpl, "SIGNATURE", signatureBuffer) ' element "SIGNATURE" has been defined As a variable binary element 

            ' Specify the back side of laser data for the elements defined in the laser setup file 
            LaserEngraveText(bidiSpl, "BARCODE_1D", "0123456789") ' element "BARCODE_1D" has been defined As a variable text element
            LaserEngraveBinary(bidiSpl, "BARCODE_2D", barcodeBuffer) ' element "BARCODE_2D" has been defined As a variable binary element 
        End Sub

        ' Send hardcoded static laser setup file
        Public Shared Sub LaserEngraveStatic(ByRef bidiSpl As BidiSplWrap)
            ' Specify the static laser setup file name. Variable elements count Is 0
            LaserEngraveSetupFileName(bidiSpl, LASER_STATIC_SETUP_FILE_NAME, 0)
        End Sub

        Public Shared Sub Main(ByVal args As String())
            Dim commandLineOptions As CommandLineOptions = commandLineOptions.CreateFromArguments(args)
            commandLineOptions.Validate()
            Dim bidiSpl As BidiSplWrap = Nothing
            Dim printerJobID As Integer = 0
            Try
                bidiSpl = New BidiSplWrap()
                bidiSpl.BindDevice(commandLineOptions.printerName)
                Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
                Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

                Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
                Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)
                ' Check if the printer Is in the Printer_Ready state
                If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                    Throw New Exception(commandLineOptions.printerName & " is not ready. status: " + printerOptionsValues._printerStatus)
                End If

                ' check if ISO magstripe option Is installed. For brevity, JIS Is ignored in this sample.
                If commandLineOptions.encodeMagstripe AndAlso Not printerOptionsValues._optionMagstripe.Contains("ISO") Then
                    Throw New Exception(commandLineOptions.printerName & " does not have an ISO magnetic stripe unit installed.")
                End If

                ' check if laser option Is installed
                If "Installed" <> printerOptionsValues._optionLaser Then
                    Throw New Exception(commandLineOptions.printerName & " does not have a laser module installed.")
                End If

                If commandLineOptions.retrieveLaserSetup Then
                    RetrieveLaserSetup(bidiSpl)
                End If

                If commandLineOptions.laserImportFile Then
                    ImportZipFilesToPrinter(bidiSpl, SIMPLEX_SETUP_ZIP_FILE_NAME, True)
                End If

                If commandLineOptions.laserExportFile Then
                    ExportZipFilesFromPrinter(bidiSpl, LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME)
                End If

                ' Optional: Check If printer Is setup correctly to print laser file
                ' This can be time consuming optional operation. Users can skip this step
                ' as most printers are setup correctly.
                ' SetUpLaserPrinter(bidiSpl, commandLineOptions.laserEngraveStatic, printerOptionsValues)

                ' Start laser job
                Dim hopperID As String = "1"
                printerJobID = Util.StartJob(bidiSpl,
                                             If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID))

                If commandLineOptions.encodeMagstripe Then
                    EncodeMagstripe(bidiSpl)
                End If

                ' Lets figure out if we should print simplex cards or duplex cards.
                Dim doTwoSided As Boolean = printerOptionsValues._optionDuplex = "Auto" OrElse printerOptionsValues._optionDuplex = "Manual"
                If commandLineOptions.printText Then
                    ' do some simple text And graphics printing.
                    ' Important: Util : PrintTextmethod also waits until driver gets all the print data.
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName)
                End If

                ' Send laser data


                If commandLineOptions.laserEngraveStatic Then
                    LaserEngraveStatic(bidiSpl)
                Else
                    If doTwoSided Then
                        LaserEngraveDuplexCard(bidiSpl)
                    Else
                        LaserEngraveSimplexCard(bidiSpl)
                    End If
                End If

                bidiSpl.SetPrinterData(strings.ENDJOB)
                If commandLineOptions.pollForJobCompletion Then
                    Util.PollForJobCompletion(bidiSpl, printerJobID)
                End If
            Catch e As BidiException
                Console.WriteLine(e.Message)
                Util.CancelJob(bidiSpl, e.PrinterJobID, e.ErrorCode)
            Catch e As Exception
                Console.WriteLine(e.Message)
                If 0 <> printerJobID Then
                    Util.CancelJob(bidiSpl, printerJobID, 0)
                End If

            Finally
                bidiSpl.UnbindDevice()
            End Try
        End Sub
    End Class
End Namespace
