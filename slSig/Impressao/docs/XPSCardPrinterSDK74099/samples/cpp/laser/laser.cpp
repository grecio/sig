////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <vector>
#include <iostream>
#include <sstream>
#include <bidispl.h>
#include <atlstr.h>
#include <atlenc.h>
#include <atlpath.h>
#include "util.h"
#include "XGetopt.h"
#include <atlenc.h>
#include <fstream>      
#include "DXP01SDK.H"

using namespace std;

const CString LASER_STATIC_SETUP_FILE_NAME              = L"SampleCard_FrontOnly_Static";
const CString LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME    = L"SampleCard_FrontOnly";
const CString LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME     = L"SampleCard";
const CString STATIC_SETUP_ZIP_FILE_NAME                = L"SampleCard_FrontOnly_Static.zip";
const CString SIMPLEX_SETUP_ZIP_FILE_NAME               = L"SampleCard_FrontOnly.zip";
const CString DUPLEX_SETUP_ZIP_FILE_NAME                = L"SampleCard.zip";

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates laser engraving along with options to " << endl;
    cout << "perform interactive mode magnetic stripe encode, print, and poll for job completion status" << endl << endl;
    cout << "Uses hardcoded data for laser engraving, magnetic stripe and printing." << endl << endl;
    cout << shortExeName << " -n <printername> [-u] [-d] [-r] [-s] [-e] [-p] [-c] [-i <input hopper>]" << endl << endl;
    cout << "options:" << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -u Exports laser setup zip files from printer to PC." << endl;
    cout << "  -d Imports laser setup zip files to the printer." << endl;
    cout << "  -r Retrieves laser setup files from the printer." << endl;
    cout << "  -s Laser engraves static laser setup file data. Default is to" << endl;
    cout << "     laser engrave variable setup file data, depending on the printer capabilities." << endl;
    cout << "  -e Encodes magnetic stripe data." << endl;
    cout << "  -p Print simple black text on one or both sides of the card" << endl;
    cout << "     depending on the printer capabilities." << endl;
    cout << "  -c Poll for job completion; needed to check for printer errors." << endl;
    cout << "  -i <input hopper>. Defaults to input hopper #1" << endl;
    cout << endl << shortExeName << " -n \"XPS Card Printer\"" << endl;
    cout << "Laser engraves data" << endl << endl;
    cout << endl << shortExeName << " -n \"XPS Card Printer\" -e -p" << endl;
    cout << "Laser engraves, prints and encodes magstripe data on all three tracks of an ISO 3-track magnetic stripe" << endl << endl;;
    cout << endl;
    exit(0);
}

void EncodeMagstripe(CComPtr <IBidiSpl>&  bidiSpl)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // hardcoded XML to encode all 3 tracks in IAT mode.
    // track 1 = "TRACK1", track 2 = "1122", track 3 = "321"
    CString trackDataXML = L"<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        L"<magstripe>"
        L"<track number=\"1\"><base64Data>VFJBQ0sx</base64Data></track>"
        L"<track number=\"2\"><base64Data>MTEyMg==</base64Data></track>"
        L"<track number=\"3\"><base64Data>MzIx</base64Data></track>"
        L"</magstripe>";

    // Data is the length of the string multiplied by size of wide wchar_t
    ULONG dataLength = trackDataXML.GetLength() * sizeof WCHAR;

    // Get memory that can be used with IBidiSpl
    PBYTE trackDataXMLBytes = (PBYTE) ::CoTaskMemAlloc(dataLength);

    // copy the XML string...but not the terminating null:
    ::memcpy_s(trackDataXMLBytes, dataLength, trackDataXML.GetBuffer(), dataLength);

    // Setup the IBidiSpl request object using IBidiRequest ...
    // First, we are going to encode a magstripe.
    // replace schema string dxp01sdk::MAGSTRIPE_ENCODE to dxp01sdk::MAGSTRIPE_ENCODE_FRONT for front side encode
    hr = bidiRequest->SetSchema(dxp01sdk::MAGSTRIPE_ENCODE);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::MAGSTRIPE_ENCODE):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // pass the magstripe data:
    hr = bidiRequest->SetInputData(BIDI_BLOB, trackDataXMLBytes, dataLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Send the command to the printer.
    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiSpl->SendRecv(BIDI_ACTION_SET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "EncodeMagStripe():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    // Free the memory that was allocated for the IBidiSpl SendRecv call:
    ::CoTaskMemFree(trackDataXMLBytes);
}

// send the data to driver using Windows bidi(SendRecv) interface
CString BidiXPSDriverInterface(CComPtr <IBidiSpl>&  bidiSpl, CString schema, std::vector<byte>& inputBytes, CString actionType)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Setup the IBidiSpl request object using IBidiRequest ...
    hr = bidiRequest->SetSchema(schema.GetString());
    if (FAILED(hr)) {
        exceptionText << "SetSchema:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    PBYTE dataXMLBytes = NULL;
    if (inputBytes.size()) {
        // Get memory that can be used with IBidiSpl
        int size = inputBytes.size();
        dataXMLBytes = (PBYTE) ::CoTaskMemAlloc(size);
        ::memcpy_s((char*)dataXMLBytes, size, inputBytes.data(), size);

        // pass the data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, dataXMLBytes, size);
        if (FAILED(hr)) {
            exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }
    }

    // Send the command to the printer.
    hr = bidiSpl->SendRecv(actionType.GetString(), bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiSpl->SendRecv:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Get the response back from printer
    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);

    // Free the memory that was allocated for the IBidiSpl SendRecv call:
    ::CoTaskMemFree(dataXMLBytes);

    return printerStatusXML;
}

int ReadFileData(CStringW name, vector <byte>& fileBuffer)
{
    int ret = false;
    CPath longName;
    longName.Combine(util::GetExePath(), name);

    std::ifstream fileName(longName, std::ifstream::binary);
    int fileBufferlength = 0;

    if (fileName) {

        // get length of file:
        fileName.seekg(0, fileName.end);
        fileBufferlength = (int) fileName.tellg();
        fileName.seekg(0, fileName.beg);

        // read data as a block:
        vector <BYTE> buffer(fileBufferlength);
        fileName.read((LPSTR) &buffer[0], fileBufferlength);

        if (fileName) {
            ret = true;
            fileBuffer = buffer;
        }
        else {
            // error only fileName.gcount() could be read
            stringstream exceptionText;
            exceptionText << "error only fileName.gcount() could be read " << CT2A(name);
            throw runtime_error(exceptionText.str());
        }

        fileName.close();
    }

    return ret;
}

void WriteFile(CString fileName, const char* buf, int bufSize, CString fileExtension)
{
    if (bufSize && buf) {
        CPath longName;
        longName.Combine(util::GetExePath(), L"LaserFilesRetrievedFromPrinter");

        if (!CreateDirectory(longName, NULL)) {
            if (GetLastError() != ERROR_ALREADY_EXISTS) {
                stringstream exceptionText;
                exceptionText << "Could not create directory " << CT2A(longName);
                throw runtime_error(exceptionText.str());
            }
        }

        longName.Combine(longName, (LPCWSTR) fileName);

        if (fileExtension.GetLength())
            longName.AddExtension(fileExtension);

        std::ofstream outfile(longName, std::ofstream::out | std::ofstream::binary);
        if (outfile.is_open()) {
            outfile.write((LPCSTR) buf, bufSize);
            outfile.close();
        }
    }
}

void CreateLaserXMLFile(CString fileName, CString fileBuffer)
{
    if (fileBuffer.GetLength() < 5) { // Instead of returning empty _printerData, driver sometimes return "  "
        stringstream exceptionText; 
        exceptionText << "CreateLaserXMLFile(): empty file - " << CT2A(fileName.GetString()) << endl;
        throw runtime_error(exceptionText.str());
    }

    CStringA fileBufferA(fileBuffer);
    WriteFile(fileName, (LPCSTR) fileBufferA, fileBuffer.GetLength(), L".xml");
}

void EncodeBufferToBase64(vector <byte>& buffer, vector <char>& base64EncodedChars)
{
    int bufferLength = buffer.size();

    if (bufferLength == 0) {
        stringstream exceptionText;
        exceptionText << "EncodeBufferToBase64: empty buffer";
        throw runtime_error(exceptionText.str());
    }

    // allocate a big buffer for the ATL::Base64Encode() function. We found
    // that Base64EncodeGetRequiredLength() did not return a large enough
    // value in some cases.
    int convertedCharCount = ATL::Base64EncodeGetRequiredLength(bufferLength * 2);
    BOOL brc = ATL::Base64Encode(
        &buffer[0],
        (int) buffer.size(),
        &base64EncodedChars[0],
        &convertedCharCount,
        ATL_BASE64_FLAG_NOCRLF);
    if (!brc) {
        //error
        stringstream exceptionText;
        exceptionText << "EncodeBufferToBase64: Base64Encode failed";
        throw runtime_error(exceptionText.str());
    }
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
void RetrieveLaserSetupFileList(CComPtr <IBidiSpl>&  bidiSpl, std::vector<CString>& laserSetupFileList)
{
    // Query all laser setup files present on the system
    std::vector<byte> laserSetupNameBytes;
    laserSetupNameBytes.clear();
    CString queryXML = BidiXPSDriverInterface(bidiSpl,
        dxp01sdk::LASER_QUERY_SETUP_FILESLIST, laserSetupNameBytes, BIDI_ACTION_GET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(queryXML);
    if (0 != printerStatusValues._errorCode) {
        stringstream exceptionText;
        exceptionText << "RetrieveLaserSetup():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    /* Example LaserSetupFileList XML from laser printer in the version 7.2 driver:
    <?xml version="1.0" encoding="UTF-8"?>
    <QuerySetupsResult>
      <LaserCardSetups>
        <LaserCardSetup name="DriverSampleCardSetup" />
        <LaserCardSetup name="SampleCard" />
        <LaserCardSetup name="SampleCard_FrontOnly" />
        <LaserCardSetup name="SampleCard_FrontOnly_Static" />
      </LaserCardSetups>
    </QuerySetupsResult>
    */
    CString laserSetupNameListXML = printerStatusValues._printerData;
    CreateLaserXMLFile(L"LaserSetupFileList", laserSetupNameListXML);
    util::ParseLaserSetupFileList(laserSetupNameListXML, laserSetupFileList);
    cout << "Successfully retrieved laser setup file" << endl;
}

/*
// we're given a Printer Status XML fragment that has a CDATA section like this:
// example Printer Status XML from laser printer in the version 7.2 driver:

<?xml version="1.0"?>
<!--Printer status xml.-->
-<PrinterStatus>
<ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
<WindowsJobID>0</WindowsJobID>
<PrinterJobID>106</PrinterJobID>
<ErrorCode>0</ErrorCode>
<ErrorSeverity>0</ErrorSeverity>
<ErrorString/>
-<DataFromPrinter>
<![CDATA[<?xml version="1.0" encoding="UTF-8"?> <QueryElementsResult> <ElementInformationList>
<ElementInformation name="PHOTO" type="BINARY" side="FRONT" />
<ElementInformation name="GIVEN_NAME" type="TEXT" side="FRONT" />
<ElementInformation name="FAMILY_NAME" type="TEXT" side="FRONT" />
<ElementInformation name="DOB" type="TEXT" side="FRONT" />
<ElementInformation name="SIGNATURE" type="BINARY" side="FRONT" />
<ElementInformation name="BARCODE_1D" type="TEXT" side="BACK" />
<ElementInformation name="BARCODE_2D" type="BINARY" side="BACK" />
</ElementInformationList> </QueryElementsResult> ]]>
</DataFromPrinter>
</PrinterStatus>
*/
void RetrieveLaserSetupElements(CComPtr <IBidiSpl>&  bidiSpl, std::vector<CString>& laserSetupFileList)
{
    for (int laserCardSetupsIndex = 0; laserCardSetupsIndex < (int) laserSetupFileList.size(); laserCardSetupsIndex++) {
        // Query laser element names for a setup file
        CString laserSetupFileName = laserSetupFileList.at(laserCardSetupsIndex).GetString();
        
        string laserSetupFileNameXML = util::CreateLaserFileNameXML(laserSetupFileName.GetString());
        std::vector<byte> laserSetupFileNameBytes(&laserSetupFileNameXML[0], &laserSetupFileNameXML[0] + laserSetupFileNameXML.length());

        CString queryXML = BidiXPSDriverInterface(bidiSpl,
            dxp01sdk::LASER_QUERY_ELEMENT_LIST, laserSetupFileNameBytes, BIDI_ACTION_GET);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(queryXML);
        if (0 != printerStatusValues._errorCode) {
            stringstream exceptionText;
            exceptionText << "RetrieveLaserSetupElements():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }

        /* Example laserElementList XML from laser printer in the version 7.2 driver:
        <?xml version="1.0" encoding="UTF-8"?>
            -<QueryElementsResult>
            -<ElementInformationList>
                <ElementInformation side="FRONT" type="BINARY" name="PHOTO"/>
                <ElementInformation side="FRONT" type="TEXT" name="GIVEN_NAME"/>
                <ElementInformation side="FRONT" type="TEXT" name="FAMILY_NAME"/>
                <ElementInformation side="FRONT" type="TEXT" name="DOB"/>
                <ElementInformation side="FRONT" type="BINARY" name="SIGNATURE"/>
                <ElementInformation side="BACK" type="TEXT" name="BARCODE_1D"/>
                <ElementInformation side="BACK" type="BINARY" name="BARCODE_2D"/>
            </ElementInformationList>
            </QueryElementsResult> */
        CString laserElementListXML = printerStatusValues._printerData;
        CreateLaserXMLFile(laserSetupFileName, laserElementListXML);
        cout << "Successfully retrieved laser element file - " << CT2A(laserSetupFileName.GetString()) << endl;
    }
}

void RetrieveLaserSetup(CComPtr <IBidiSpl>&  bidiSpl)
{
    // Query all laser setup files present on the system
    std::vector<CString> laserSetupFileList;
    RetrieveLaserSetupFileList(bidiSpl, laserSetupFileList);

    // Query laser element names for all setup files
    RetrieveLaserSetupElements(bidiSpl, laserSetupFileList);
}

void CreateLaserFile(CString fileName, CString laserFile)
{
    stringstream exceptionText;

    // Parse the laser data returned from printer
    CStringA laserFileBuffer = CT2A(laserFile.GetString());
    int laserFileBytesLength = laserFileBuffer.GetLength();
    if (laserFileBytesLength > 5) {
        vector <byte> laserFileBytes(laserFileBytesLength, (BYTE) 0);
        BOOL bResult(false);
        bResult = ::Base64Decode(laserFileBuffer.GetString(),
            laserFileBuffer.GetLength(), &laserFileBytes[0], &laserFileBytesLength);
        if (bResult == FALSE) {
            exceptionText << "CreateLaserFile: decode error";
            throw runtime_error(exceptionText.str());
        }
        laserFileBytes.resize(laserFileBytesLength);
        
        unsigned int found = fileName.Find(L".zip");
        if (found != std::string::npos) {
            WriteFile(fileName.GetString(), (LPCSTR) &laserFileBytes[0], laserFileBytes.size(), L"");
        }
        else{
            // create xml file
            CString laserXMLFile(&laserFileBytes[0]);
            CreateLaserXMLFile(fileName.GetString(), laserXMLFile);
        }
    }
    else {
        exceptionText << "CreateLaserFile: empty laser data returned by printer" << endl;
        throw runtime_error(exceptionText.str());
    }
}

/*
Example ImportZipFilesToPrinter XML from laser printer in the version 7.2 driver
<?xml version="1.0"?>
<!--Printer status xml.-->
-<PrinterStatus>
<ClientID>{0BDB05E9-31B3-4060-B538-2356FA01F6D5}</ClientID>
<WindowsJobID>0</WindowsJobID>
<PrinterJobID>0</PrinterJobID>
<ErrorCode>0</ErrorCode>
<ErrorSeverity>0</ErrorSeverity>
<ErrorString/>
-<DataFromPrinter>
<![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>1</Status><Base64Data> </Base64Data></LaserResponse>]]>
</DataFromPrinter>
</PrinterStatus>
*/
void ImportZipFilesToPrinter(CComPtr <IBidiSpl>&  bidiSpl, CString laserZipFileName, bool overwrite)
{
    stringstream exceptionText;

    // Read file in a buffer
    vector <byte> laserZipFileBuffer;
    ReadFileData(laserZipFileName.GetString(), laserZipFileBuffer);

    // Format buffer to base64 as binary expects that data in base64
    // allocate a big buffer for the ATL::Base64Encode() function. 
    int laserZipFilebufferLength = laserZipFileBuffer.size();
    const int maxEncodedStringSize =
        ATL::Base64EncodeGetRequiredLength(laserZipFilebufferLength * 2);
    vector <char> base64EncodedChars(maxEncodedStringSize, 0); // zero-fill
    EncodeBufferToBase64(laserZipFileBuffer, base64EncodedChars);

    string laserZipFileXML = util::CreateImportZipFileXML(laserZipFileName.GetString(), overwrite, base64EncodedChars);
    std::vector<byte> laserZipFileXMLBytes(&laserZipFileXML[0], &laserZipFileXML[0] + laserZipFileXML.length());

    // send the data to driver using Windows bidi interface
    CString statusXML = BidiXPSDriverInterface(bidiSpl, dxp01sdk::LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER, laserZipFileXMLBytes, BIDI_ACTION_SET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(statusXML);
    if (518 == printerStatusValues._errorCode) {
        // laser module returned an error. Parse the laser data returned from driver
        cout << "laser module returned 518 error: " << laserZipFileName.GetString() << endl;
    }
    else if (0 != printerStatusValues._errorCode) {
        exceptionText << "ImportZipFilesToPrinter():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    /* Example laserStatus XML from laser printer in the version 7.2 driver:
    <?xml version="1.0"?>
    <!--laser response xml-->
    -<LaserResponse>
        <Status>1</Status>
        <Base64Data> </Base64Data>
    </LaserResponse>
    */
    CString laserStatusXML = printerStatusValues._printerData;
    util::LaserStatusValues laserStatusValues = util::ParseLaserStatusXML(laserStatusXML);
    if (laserStatusValues._success) {
        // Success. Imported zip file to printer
        cout << "Successfully imported zip file to printer - " << CT2A(laserZipFileName.GetString()) << endl;

    }
    else if (518 == printerStatusValues._errorCode && !laserStatusValues._success) {
        // Firmware issued conflict file list error
        CreateLaserFile(L"LaserFileConflictList", laserStatusValues._base64Data.GetString());
        cout << "Import failed. Created laserFileConflictList.xml file" << endl;
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
    else {
        exceptionText << "Import failed";
        throw runtime_error(exceptionText.str());
    }
}

void ExportZipFilesFromPrinter(CComPtr <IBidiSpl>&  bidiSpl, CString laserZipFileName)
{
    stringstream exceptionText;

    string laserZipFileXML = util::CreateLaserFileNameXML(laserZipFileName.GetString());
    std::vector<byte> laserExportZipFileBytes(&laserZipFileXML[0], &laserZipFileXML[0] + laserZipFileXML.length());

    // send the data to driver using Windows bidi interface
    CString statusXML = BidiXPSDriverInterface(bidiSpl, dxp01sdk::LASER_UPLOAD_ZIP_FILE_FROM_PRINTER, laserExportZipFileBytes, BIDI_ACTION_GET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(statusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "ExportZipFilesFromPrinter():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    /* Example printer status XML from laser printer in the version 7.2 driver in case of export failure:
    <?xml version="1.0"?>
    <!--Printer status xml.-->
    -<PrinterStatus>
    <ClientID>{20EAB827-59D6-43B1-89B8-03230164D174}</ClientID>
    <WindowsJobID>0</WindowsJobID>
    <PrinterJobID>0</PrinterJobID>
    <ErrorCode>522</ErrorCode>
    <ErrorSeverity>2</ErrorSeverity>
    <ErrorString>Message 522: File export failed</ErrorString>
    -<DataFromPrinter>
    <![CDATA[<?xml version="1.0"?><!--laser response xml--><LaserResponse><Status>0</Status><Base64Data> </Base64Data></LaserResponse>]]>
    </DataFromPrinter>
    </PrinterStatus
    */

    /* Example laserStatus XML from laser printer in the version 7.2 driver in case of export success:
    <?xml version="1.0"?>
    <!--laser response xml-->
    -<LaserResponse>
    <Status>1</Status>
    <Base64Data> </Base64Data>
    </LaserResponse>
    */
    CString laserStatusXML = printerStatusValues._printerData;
    util::LaserStatusValues laserStatusValues = util::ParseLaserStatusXML(laserStatusXML);
    if (laserStatusValues._success) {
        // Success. Retrieved zip file from printer
        laserZipFileName.Append(L".zip");
        CreateLaserFile(laserZipFileName, laserStatusValues._base64Data.GetString());
        cout << "Successfully exported laser setup file: " << CT2A(laserZipFileName.GetString()) << endl;
    }
    else{
        
        exceptionText << "ExportZipFilesFromPrinter: failed to export file" << CT2A(laserZipFileName.GetString()) << endl;
        throw runtime_error(exceptionText.str());
    }
}

void SetUpLaserPrinter(CComPtr <IBidiSpl>&  bidiSpl, bool staticLayout, util::PrinterOptionsValues printerOptionsValues)
{
    CString setupZipFileName;
    CString setupFileName;

    // Check if printer is setup correctly to print laser file
    if (staticLayout) {
        setupFileName = LASER_STATIC_SETUP_FILE_NAME;
        setupZipFileName = STATIC_SETUP_ZIP_FILE_NAME;
    }
    else {

        // Lets figure out if we should print simplex cards or duplex cards.
        const bool doTwoSided = printerOptionsValues._optionDuplex == "Auto" ||
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
    std::vector<CString> laserSetupFileList;
    RetrieveLaserSetupFileList(bidiSpl, laserSetupFileList);

    // Check if setupName is present in laserSetupFileListL
    bool laserSetupFilePresent = false;
    for (int laserCardSetupsIndex = 0; laserCardSetupsIndex < (int) laserSetupFileList.size(); laserCardSetupsIndex++) {
        // Query laser element names for a setup file

        CString laserSetupFileName = laserSetupFileList.at(laserCardSetupsIndex).GetString();
        if (laserSetupFileName == setupFileName) {
            laserSetupFilePresent = true;
            break;
        }
    }

    if (!laserSetupFilePresent) {
        // Send laser setup zip file if it is not present on laser printer
        ImportZipFilesToPrinter(bidiSpl, setupZipFileName, true);
    }
}


void LaserEngraveBinary(CComPtr <IBidiSpl>&  bidiSpl, CString elementName, vector <byte>& buffer)
{
    stringstream exceptionText;
    int bufferLength = buffer.size();

    if (bufferLength == 0) {
        exceptionText << "LaserEngraveBinary: empty buffer";
        throw runtime_error(exceptionText.str());
    }

    // Format buffer to base64 as binary expects that data in base64
    // allocate a big buffer for the ATL::Base64Encode() function. 
    const int maxEncodedStringSize =
        ATL::Base64EncodeGetRequiredLength(bufferLength * 2);
    vector <char> base64EncodedChars(maxEncodedStringSize, 0); // zero-fill
    EncodeBufferToBase64(buffer, base64EncodedChars);

    string laserBinaryStr = util::CreateLaserEngraveBinaryXML(elementName.GetString(), base64EncodedChars);
    std::vector<byte> laserXML(&laserBinaryStr[0], &laserBinaryStr[0] + laserBinaryStr.length());
    
    // send the data to driver using Windows bidi interface
    CString queryXML = BidiXPSDriverInterface(bidiSpl, dxp01sdk::LASER_ENGRAVE_BINARY, laserXML, BIDI_ACTION_SET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(queryXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "LaserEngraveBinary():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

}

void LaserEngraveText(CComPtr <IBidiSpl>&  bidiSpl, CString elementName, CString laserText)
{
    string laserTextStr = util::CreateLaserEngraveTextXML(elementName.GetString(), laserText.GetString());
    std::vector<byte> laserXML(&laserTextStr[0], &laserTextStr[0] + laserTextStr.length());

    // send the data to driver using Windows bidi interface
    CString queryXML = BidiXPSDriverInterface(bidiSpl, dxp01sdk::LASER_ENGRAVE_TEXT, laserXML, BIDI_ACTION_SET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(queryXML);
    if (0 != printerStatusValues._errorCode) {
        stringstream exceptionText;
        exceptionText << "LaserEngraveText():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
}

void LaserEngraveSetupFileName(CComPtr <IBidiSpl>&  bidiSpl, CString laserSetUpFileName, int count)
{
    string laserXML = util::CreateLaserSetupFileNameXML(laserSetUpFileName.GetString(), count);
    std::vector<byte> laserBytes(&laserXML[0], &laserXML[0] + laserXML.length());

    // send the data to driver using Windows bidi interface
    CString queryXML = BidiXPSDriverInterface(bidiSpl, dxp01sdk::LASER_ENGRAVE_SETUP_FILE_NAME, laserBytes, BIDI_ACTION_SET);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(queryXML);
    if (0 != printerStatusValues._errorCode) {
        stringstream exceptionText;
        exceptionText << "LaserEngraveSetupFileName():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
}


// Send hardcoded laser data
// Laser card design is specified in the "SampleCard_FrontOnly" laser setup file present in firmware.
// Laser setup file has five elements:
void LaserEngraveSimplexCard(CComPtr <IBidiSpl>&  bidiSpl)
{
    // Read photo file in a buffer
    vector <byte> photoBuffer; 
    ReadFileData(TEXT("ARMSTROT.JPG"), photoBuffer);

    // Read signature file in a buffer
    vector <byte> signatureBuffer;
    ReadFileData(TEXT("ARMSTROT.TIF"), signatureBuffer);

    // Specify the laser setup file name, and variable elements count
    LaserEngraveSetupFileName(bidiSpl, LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME, 5);

    // Specify the laser data for the elements defined in the laser setup file 
    LaserEngraveBinary(bidiSpl, L"PHOTO", photoBuffer); // element "PHOTO" has been defined as a variable binary element 
    LaserEngraveText(bidiSpl, L"GIVEN_NAME", "John M."); // element "GIVEN_NAME" has been defined as a variable text element 
    LaserEngraveText(bidiSpl, L"FAMILY_NAME", L"Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
    LaserEngraveText(bidiSpl, L"DOB", L"01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
    LaserEngraveBinary(bidiSpl, L"SIGNATURE", signatureBuffer); // element "SIGNATURE" has been defined as a variable binary element 

}

void LaserEngraveDuplexCard(CComPtr <IBidiSpl>&  bidiSpl)
{
    // Read photo file in a buffer
    vector <byte> photoBuffer;
    ReadFileData(TEXT("ARMSTROT.JPG"), photoBuffer);

    // Read signature file in a buffer
    vector <byte> signatureBuffer;
    ReadFileData(TEXT("ARMSTROT.TIF"), signatureBuffer);

    // Initialize 2D barcode buffer with ASCII data (preferably 7-bit )
    string barcode2DStr = "This is PDF417 barcode encoded sample text printed on a CL900 from the driver SDK sample. Visit: http://www.entrustdatacard.com";
    std::vector<byte> barcodeBuffer(&barcode2DStr[0], &barcode2DStr[0] + barcode2DStr.length());

    // Specify the laser setup file name, and variable elements count
    LaserEngraveSetupFileName(bidiSpl, LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME, 7);
    
    // Specify the front side of laser data for the elements defined in the laser setup file 
    LaserEngraveBinary(bidiSpl, L"PHOTO", photoBuffer); // element "PHOTO" has been defined as a variable binary element 
    LaserEngraveText(bidiSpl, L"GIVEN_NAME", L"John M."); // element "GIVEN_NAME" has been defined as a variable text element
    LaserEngraveText(bidiSpl, L"FAMILY_NAME", L"Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
    LaserEngraveText(bidiSpl, L"DOB", L"01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
    LaserEngraveBinary(bidiSpl, L"SIGNATURE", signatureBuffer); // element "SIGNATURE" has been defined as a variable binary element 

    // Specify the back side of laser data for the elements defined in the laser setup file 
    LaserEngraveText(bidiSpl, L"BARCODE_1D", L"0123456789"); // element "BARCODE_1D" has been defined as a variable text element
    LaserEngraveBinary(bidiSpl, L"BARCODE_2D", barcodeBuffer); // element "BARCODE_2D" has been defined as a variable binary element 
}

// Send hardcoded static laser setup file
void LaserEngraveStatic(CComPtr <IBidiSpl>&  bidiSpl)
{
    // Specify the static laser setup file name. Variable elements count is 0
    LaserEngraveSetupFileName(bidiSpl, LASER_STATIC_SETUP_FILE_NAME, 0);
}

void ValidateHopperID(CString hopperID)
{
    if (hopperID != L"") {
        if (
            hopperID != L"1" &&
            hopperID != L"2" &&
            hopperID != L"3" &&
            hopperID != L"4" &&
            hopperID != L"5" &&
            hopperID != L"6" &&
            hopperID != L"exception") {
            std::cout << "invalid hopperID: " << CW2A(hopperID) << std::endl;
            ::exit(-1);
        }
    }
}


////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    CString printerName;
    bool encodeMagstripe(false);
    bool printText(false);
    bool pollForJobCompletion(false);
    bool retrieveLaserSetup(false);
    bool laserEngraveStatic(false);
    bool laserImportFile(false);
    bool laserExportFile(false);
    CString commandLineHopperID = L"";

    int c(0);
    while ((c = getopt(argc, argv, _T("n:rsduepci:"))) != EOF) {
        switch (c) {
        case TEXT('n'): printerName = optarg; break;
        case TEXT('r'): retrieveLaserSetup = true; break;
        case TEXT('s'): laserEngraveStatic = true; break;
        case TEXT('d'): laserImportFile = true; break;
        case TEXT('u'): laserExportFile = true; break;
        case TEXT('e'): encodeMagstripe = true; break;
        case TEXT('p'): printText = true; break;
        case TEXT('c'): pollForJobCompletion = true; break;
        case L'i':
            commandLineHopperID = CString(optarg).MakeLower();
            ValidateHopperID(commandLineHopperID);
            break;
        default: usage();
        }
    }

    if (printerName.IsEmpty()) {
        usage();
    }

    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

    try {
        stringstream exceptionText;

        HRESULT hr = ::CoInitialize(NULL);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            exceptionText << "bidiSpl.CoCreateInstance:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl->BindDevice(printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice():" << " 0x" << hex << hr << " " << CT2A(printerName) << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        // Check if the printer is in the Printer_Ready state:
        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);

        if ("Ready" == printerOptionsValues._printerStatus ||
            "Busy" == printerOptionsValues._printerStatus) {
            // the printer can accept new jobs.
        }
        else {
            exceptionText << "printer " << CT2A(printerName) << " is not ready.";
            throw runtime_error(exceptionText.str());
        }

        // check if ISO magstripe option is installed. For brevity, JIS is ignored in this sample.
        if (encodeMagstripe && (printerOptionsValues._optionMagstripe.Find(CString("ISO")) == -1)) {
            exceptionText << "printer " << CT2A(printerName) << " does not have magstripe module installed.";
            throw runtime_error(exceptionText.str());
        }

        // check if laser option is installed
        if ("Installed" != printerOptionsValues._optionLaser) {
            exceptionText << "printer " << CT2A(printerName) << " does not have laser module installed.";
            throw runtime_error(exceptionText.str());
        }

        if (retrieveLaserSetup) {
            RetrieveLaserSetup(bidiSpl);
        }

        if (laserImportFile) {
            ImportZipFilesToPrinter(bidiSpl, SIMPLEX_SETUP_ZIP_FILE_NAME, true);
        }

        if (laserExportFile) {
            ExportZipFilesFromPrinter(bidiSpl, LASER_STATIC_SETUP_FILE_NAME);
        }

        // Optional: Check if printer is setup correctly to print laser file
        // This can be time consuming optional operation. Users can skip this step
        // as most printers are setup correctly.
        // SetUpLaserPrinter(bidiSpl, laserEngraveStatic, printerOptionsValues);

        // Start laser job
        CString hopperID = L"1";
        printerJobID = util::StartJob(
            bidiSpl,
            commandLineHopperID.GetLength() > 0 ? commandLineHopperID : hopperID);

        if (encodeMagstripe) {  
            EncodeMagstripe(bidiSpl); 
        }

        // Lets figure out if we should print simplex cards or duplex cards.
        const bool doTwoSided = printerOptionsValues._optionDuplex == "Auto" ||
            printerOptionsValues._optionDuplex == "Manual";

        if (printText) {
            // do some simple text and graphics printing.
            // Important: util::PrintTextmethod also waits until driver gets all the print data.
            util::PrintText(printerName, doTwoSided, 20);
        }

        // Send laser data
        if (laserEngraveStatic) {
            LaserEngraveStatic(bidiSpl);
        }
        else {

            if (doTwoSided)
                LaserEngraveDuplexCard(bidiSpl);
            else
                LaserEngraveSimplexCard(bidiSpl);
        }

        // End laser job
        util::EndJob(bidiSpl);

        if (pollForJobCompletion) {
            util::PollForJobCompletion(bidiSpl, printerJobID);
        }
    }
    catch (util::BidiException& e) {
        cerr << e.what() << endl;
        util::CancelJob(bidiSpl, e.printerJobID, e.errorCode);
    }
    catch (runtime_error& e) {
        cerr << e.what() << endl;
        if (0 != printerJobID) {
            util::CancelJob(bidiSpl, printerJobID, 0);
        }
    }

    if (bidiSpl) {
        bidiSpl->UnbindDevice();
        bidiSpl = NULL;
    }

    ::CoUninitialize();

    return 0;
}