////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer Driver SDK: 'printer_state' c++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <iostream>
#include <sstream>
#include "XGetopt.h"
#include "DXP01SDK.H"
#include "util.h"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates changing the printer state to offline, online, or suspended." << endl << endl;
    cout << shortExeName << " -n <printername> -s <on | off | suspend>" << endl << endl;
    cout << "options:" << endl;
    cout << "  " << "-n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  " << "-s <on | off | suspend>. Required. Changes printer to the specified state:" << endl;
    cout << "  " << "   'on' changes printer state to online." << endl;
    cout << "  " << "   'off' changes printer state to offline." << endl;
    cout << "  " << "   'suspend' changes printer state to suspended." << endl;
    cout << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -s on" << endl;
    cout << "  Changes the printer state to online." << endl << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -s off" << endl;
    cout << "  Changes the printer state to offline." << endl << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -s suspend" << endl;
    cout << "  Changes the printer state to suspended." << endl << endl;

    cout << endl;
    exit(0);
}

struct CommandLineOptions {
    CString  printerName;
    CString  printerState;

    CommandLineOptions() 
    {}

    void Validate()
    {
        if (printerName.IsEmpty()) {
            usage();
        }

        if (printerState.IsEmpty()) {
            usage();
        }

        if (L"off" != printerState &&
            L"on" != printerState  &&
            L"suspend" != printerState) {
            usage();
        }
    }
};

// ------------------------------------------------------------
//  function GetPrinterState
//  ASSUMPTION:
//      It has already been determined that printerState 
//      is a valid state.
// ------------------------------------------------------------
int GetPrinterState(
    CString printerState)
{
    if (L"off" == printerState) return dxp01sdk::Offline;
    if (L"on" == printerState ) return dxp01sdk::Online;

    return dxp01sdk::Suspended;
}


util::PrinterStatusValues ChangePrinterState(
    CComPtr<IBidiSpl>&  bidiSpl,
    int                 printerState)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }
    CString formattedPrinterChangeStateXML;
    formattedPrinterChangeStateXML.Format(
        dxp01sdk::CHANGE_PRINTER_STATE_XML,
        printerState);

    const int xmlBytesLength = formattedPrinterChangeStateXML.GetLength() * sizeof WCHAR;

    // allocate IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, formattedPrinterChangeStateXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::CHANGE_PRINTER_STATE);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::CHANGE_PRINTER_STATE): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData(): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) cancel xml: " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "ActivatePrinter() failed. Status XML:\n" << CT2A(printerStatusXML);
        throw runtime_error(exceptionText.str());
    }

    HRESULT ChangeStateJobResultCode(S_OK);
    hr = bidiRequest->GetResult(&ChangeStateJobResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(ChangeStateJobResultCode):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(ChangeStateJobResultCode)) {
        exceptionText << "ChangeStateJobResultCode:" << " 0x" << hex << ChangeStateJobResultCode << " " << CT2A(util::Win32ErrorString(ChangeStateJobResultCode));
        throw runtime_error(exceptionText.str());
    }

    return printerStatusValues;

}


///////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    stringstream exceptionText;
    CommandLineOptions options;

    int c(0);
    while ((c = getopt(argc, argv, _T("s:n:"))) != EOF) {
        switch (c) {
        case L's': options.printerState = CString(optarg).MakeLower(); break;
        case L'n': options.printerName = optarg; break;
        default: usage();
        }
    }

    options.Validate();

    CComPtr <IBidiSpl> bidiSpl;
    try {
        HRESULT hr = ::CoInitialize(NULL);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        // create an IBidiSpl COM smartpointer instance and bind it to the printer
        // name. We need it for job completion status - not embossing or indenting.
        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            exceptionText << "bidiSpl.CoCreateInstance:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl->BindDevice(options.printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice():" << " 0x" << hex << hr << " " << CT2A(options.printerName) << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        const CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        const CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        int printerState = GetPrinterState(options.printerState);
        ChangePrinterState(bidiSpl, printerState);

    }
    catch (runtime_error& e) {
        cerr << e.what() << endl;
    }

    if (bidiSpl) {
        bidiSpl->UnbindDevice();
        bidiSpl = NULL;
    }

    ::CoUninitialize();

    return 0;
}