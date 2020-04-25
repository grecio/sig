////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK Lamination & Read Laminination Bar Code C++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <vector>
#include <iostream>
#include <sstream>
#include <fstream>
#include <bidispl.h>
#include <atlstr.h>
#include "util.h"
#include "XGetopt.h"
#include "DXP01SDK.H"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates interactive mode laminator bar code " << endl;
    cout << "read with options to verify the bar code data, and polls for job completion." << endl << endl;
    cout << "Serialized overlay material must be loaded in the L1 laminator." << endl << endl;
    cout << "This sample uses the driver Printing Preferences/Lamination settings. Make sure" << endl;
    cout << "that the L1 Laminate card setting is not set to \"Do not apply.\" The sample" << endl;
    cout << "always prints a card. To laminate the card without printing, select" << endl;
    cout << "\"Printing Preferences/Layout/Advanced\" and change \"Disable printing\" to \"All\"." << endl << endl;
    cout << shortExeName << " -n <printername> [-v] [-t] <timeout> [-f] <filename>" << endl << endl;
    cout << "options:" << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -v Verify the laminator bar code data. <optional>" << endl;
    cout << "  -t <msec> Timeout to read bar code data. Default is no timeout. <optional>" << endl;
    cout << "  -f <filename> Save the laminator bar code read results to a file. <optional>" << endl << endl;
    cout << shortExeName << endl << endl;
    cout << "  Demonstrates interactive mode laminator bar code read with no timeout; prints" << endl;
    cout << "  black text on one or both sides of the card; polls and displays job status." << endl << endl;
    cout << shortExeName << " -v -f \"example.txt\" " << endl << endl;
    cout << "  Demonstrates interactive mode laminator bar code read and verify with no" << endl;
    cout << "  timeout; prints black text on one or both sides of the card; polls and" << endl;
    cout << "  displays job status. It also writes the bar code data to file example.txt." << endl << endl;
    exit(0);
}

void ResumeCard(const int printerJobID, CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // create the XML and convert to bytes for the upcoming SetInputData() call:
    CString actionXML = util::FormatPrinterActionXML(dxp01sdk::Resume, printerJobID, 0);

    const int xmlBytesLength = actionXML.GetLength() * sizeof WCHAR; // UNICODE

    // IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, actionXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_ACTION);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::XPS_CARDPRINTER_SCHEMA_PRINTER_ACTION):" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) resume card:" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }
}

CStringA GetCurrentDateTimeString()
{
    CStringA dateString;
    ::GetDateFormatA(LOCALE_USER_DEFAULT, 0, NULL, NULL, dateString.GetBuffer(40), 40);
    dateString.ReleaseBuffer();

    CStringA timeString;
    ::GetTimeFormatA(LOCALE_USER_DEFAULT, 0, NULL, NULL, timeString.GetBuffer(40), 40);
    timeString.ReleaseBuffer();

    CStringA currentDateTimeString;
    currentDateTimeString.Format("%s %s     ", dateString, timeString);
    return currentDateTimeString;
}

void WriteToFile(CString fileName, CStringA text)
{
    if (fileName.GetLength()) {
        ofstream myfile(fileName, std::ofstream::out | std::ofstream::app);
        if (myfile.is_open()) {
            CStringA localTime = GetCurrentDateTimeString();
            myfile << localTime << text;
            myfile.close();
        }
        else {
            cerr << "Unable to open file: " << fileName << endl << endl;
        }
    }
}

bool IsBarcodeDataGood()
{
    const CString caption(L"A laminator bar code has been read.");
    const CString message(
        L"This is where laminator bar code verify happens.\r\n\r\n"
        L"Select 'Yes' to continue the job. This simulates that the bar code data passed verification and you want to finish the card.\r\n\r\n"
        L"Select 'No' to cancel the job. This simulates that the bar code data failed verification and you want to reject the card.");
    const int rc = ::MessageBox(NULL, message, caption, MB_YESNO);
    return IDYES == rc;
}

void SetLaminatorBarcodeActions(CComPtr <IBidiSpl>& bidiSpl, const bool verify)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (verify) {
        hr = bidiRequest->SetSchema(dxp01sdk::LAMINATOR_BARCODE_READ_AND_VERIFY);
    }
    else {
        hr = bidiRequest->SetSchema(dxp01sdk::LAMINATOR_BARCODE_READ);
    }

    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::LAMINATOR_BARCODE):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiSpl->SendRecv(BIDI_ACTION_GET):"
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }
}

void ReadLaminatorBarcode(
    CComPtr <IBidiSpl>& bidiSpl,
    DWORD               printerjobid,
    int                 timeout,
    const bool          verify,
    CString             fileName)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::LAMINATOR_BARCODE_READ_DATA);

    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::LAMINATOR_BARCODE_READ_DATA):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    CString barcodejobXml;
    barcodejobXml.Format(dxp01sdk::LAMINATOR_BARCODE_READ_XML, printerjobid, timeout);

    const ULONG barcodejobDataLength = barcodejobXml.GetLength() * sizeof(TCHAR);

    // Get memory that can be used with IBidiSpl:
    CComAllocator XMLBytesAllocator; // automatically frees COM memory
    PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(barcodejobDataLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...but not the terminating null:
    ::memcpy_s(XMLBytes, barcodejobDataLength, barcodejobXml.GetBuffer(), barcodejobDataLength);

    // pass the bar code data:
    hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, barcodejobDataLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Send the command to the printer.
    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiSpl->SendRecv(BIDI_ACTION_GET):"
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Check if device read data and SendRecv was a success
    HRESULT BarcodeReadResult(S_OK);
    hr = bidiRequest->GetResult(&BarcodeReadResult);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(BarcodeReadResult)) {
        exceptionText << "BarcodeReadResult code:" << " 0x" << hex << BarcodeReadResult
            << " " << CT2A(util::Win32ErrorString(BarcodeReadResult));
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (516 == printerStatusValues._errorCode) {
        // application did not provide long enough timeout for the bar code to
        // be read. It is application's responsibility to call bar code read
        // again.

        cout << "BarcodeReadResult(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString) << endl;
        if (fileName.GetLength()) {
            CStringA text;
            text.Format("Printer job ID: %d, %s\n", printerjobid, CT2A(printerStatusValues._errorString));
            WriteToFile(fileName, text);
        }
    }
    else if (517 == printerStatusValues._errorCode) {
        exceptionText << "BarcodeRead() fail; Printer Error:" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
    else if (0 != printerStatusValues._errorCode) {
        exceptionText << "BarcodeReadResult():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    CStringA barcodeLaminator = printerStatusValues._printerData;
    if (barcodeLaminator.GetLength()) {
        cout << "Bar code: " << barcodeLaminator.GetString() << endl;
        if (fileName.GetLength()) {
            CStringA text;
            text.Format("Printer job ID: %d, bar code data: %s \n", printerjobid, barcodeLaminator.GetString());
            WriteToFile(fileName, text);
        }
    }
    else {
        cerr << "Error no data read." << endl;
        if (fileName.GetLength()) {
            CStringA text;
            text.Format("Printer job ID: %d, Error no data read.\n", printerjobid);
            WriteToFile(fileName, text);
        }
    }

    if (verify) {
        if (IsBarcodeDataGood()) {
            ResumeCard(printerjobid, bidiSpl);
        }
        else {
            exceptionText << "IsBarcodeDataGood() returned FALSE.";
            throw runtime_error(exceptionText.str());
        }
    }
}

// do some simple text and graphics printing.
void PrintText(CString printerName, CString  _optionDuplex)
{
    const bool doTwoSided =
        _optionDuplex == "Auto" ||
        _optionDuplex == "Manual";

    // Important: util::PrintTextmethod also waits until driver gets all the print data.
    util::PrintText(printerName, doTwoSided);
}

void AddMagstripeReadOrEncodeOrSmarcardOperationsHere()
{
    // Placeholder. Please refer to Magstripe and smartcard samples.
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    bool verify(false);
    int timeout = dxp01sdk::INFINITE_WAIT;
    CString printerName;
    CString fileName;

    int c(0);
    while ((c = getopt(argc, argv, _T("f:n:t:v"))) != EOF) {
        switch (c) {
        case TEXT('n'): printerName = optarg; break;
        case TEXT('f'): fileName = optarg; break;
        case TEXT('t'): timeout = ::_wtol(optarg); break;
        case TEXT('v'): verify = true; break;
        default: usage();
        }
    };

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

        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);

        if ("None" == printerOptionsValues._laminator) {
            exceptionText << "printer " << CT2A(printerName) << " does not have a laminator.";
            throw runtime_error(exceptionText.str());
        }

        if ("None" == printerOptionsValues._laminatorScanner) {
            exceptionText << "printer " << CT2A(printerName) << " does not have a lamination barcode scanner.";
            throw runtime_error(exceptionText.str());
        }

        if ("Ready" == printerOptionsValues._printerStatus ||
            "Busy" == printerOptionsValues._printerStatus) {
            // the printer can accept new jobs.
        }
        else {
            exceptionText << "printer " << CT2A(printerName) << " is not ready.";
            throw runtime_error(exceptionText.str());
        }

        CString hopperID;
        printerJobID = util::StartJob(
            bidiSpl,
            hopperID);

        // Notify the printer that a bar code read is part of the job.
        // Note: serialized laminate must be installed in L1 laminator station.
        SetLaminatorBarcodeActions(bidiSpl, verify);

        // Add code for magnetic stripe read, magnetic stripe encode and smart
        // card operations here. Other samples show how to perform these
        // operations.
        AddMagstripeReadOrEncodeOrSmarcardOperationsHere();

        // Do some simple text printing. You MUST send print data to the driver
        // to laminate. Print data is required as part of every lamination job
        // because lamination actions are a printing preference.
        PrintText(printerName, printerOptionsValues._optionDuplex);

        // NOTE: Although you must send print data to the driver to laminate,
        // you can prevent printing on the card by disabling printing in the
        // driver:
        // 1) Select “Printing Preferences/Layout/Advanced and change “Disable
        //    printing” to “All.” OR,
        // 2) Programmatically “Disable printing" using print ticket. The Print
        //    sample demontrates print ticket manipulation.

        // Call EndJob to notify the printer that all the data has been sent for
        // the card.
        util::EndJob(bidiSpl);

        // Read the bar code. This may take several minutes if the laminator is
        // warming up. If the  timeout provided is too small the function will
        // return before the bar code can be read. If you use a short timeout,
        // call this function repeatedly.
        ReadLaminatorBarcode(bidiSpl, printerJobID, timeout, verify, fileName);

        // Use job completion to monitor that the card was personalized
        // successfully or failed to complete because of an error.
        util::PollForJobCompletion(bidiSpl, printerJobID);
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