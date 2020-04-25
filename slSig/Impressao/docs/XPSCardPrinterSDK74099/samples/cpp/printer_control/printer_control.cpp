////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <vector>
#include "XGetopt.h"
#include "util.h"
#include "DXP01SDK.H"

using namespace std;

enum Commands {
    undefined,
    cancelAllJobs,
    restart,
    resetCardCounts,
	adjustColors,
    defaultColors
};

struct CommandLineOptions {
    CommandLineOptions() : command(undefined)
    {}

    CString  printerName;
    Commands command;
};

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl << shortExeName << " controls the printer through the printer driver." << endl << endl;
    cout << shortExeName << " -n <printername> <command>" << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -c cancel all jobs: Send a command to the printer to clear all print jobs" << endl;
    cout << "     from the printer. This does not delete spooled Windows" << endl;
    cout << "     print jobs." << endl;
    cout << "  -r restart the printer." << endl;
    cout << "  -e reset all card counts." << endl;
	cout << "  -a set colorAdjust settings.  This sample changes only the Red settings. Blue and Green are unchanged" << endl;
    cout << "  -d set default colorAdjust settings. This sample only resets Red and Green.  Blue is unchanged. " << endl;
    ::exit(-1);
}

void ValidateCommandLineOptions(CommandLineOptions& commandLineOptions)
{
    if (commandLineOptions.printerName.IsEmpty()) {
        usage();
    }
    if (undefined == commandLineOptions.command) {
        usage();
    }
}

CommandLineOptions GetCommandlineOptions(const int argc, _TCHAR* argv[])
{
    CommandLineOptions commandLineOptions;
    int c(0);
    while ((c = getopt(argc, argv, _T("n:cread"))) != EOF) {
        switch (c) {
        case L'n': commandLineOptions.printerName = optarg; break;
        case L'c': commandLineOptions.command = cancelAllJobs; break;
        case L'r': commandLineOptions.command = restart; break;
        case L'e': commandLineOptions.command = resetCardCounts; break;
		case L'a': commandLineOptions.command = adjustColors; break;
        case L'd': commandLineOptions.command = defaultColors; break;
        default:
            usage();
        }
    }
    return commandLineOptions;
}

void RestartPrinter(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::RESTART_PRINTER);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::RESTART_PRINTER)):" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) restart:" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    HRESULT restartResultCode(S_OK);
    hr = bidiRequest->GetResult(&restartResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(restart):" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(restartResultCode)) {
        exceptionText << "resetResultCode:" << " 0x" << hex << restartResultCode << " " << util::Win32ErrorString(restartResultCode).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "RestartPrinter(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
}

void ResetCardCounts(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::RESET_CARD_COUNTS);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::RESET_CARD_COUNTS)):" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) reset card counts:" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    HRESULT resetCountsResultCode(S_OK);
    hr = bidiRequest->GetResult(&resetCountsResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(reset card counts):" << " 0x" << hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(resetCountsResultCode)) {
        exceptionText << "resetResultCode:" << " 0x" << hex << resetCountsResultCode << " " << util::Win32ErrorString(resetCountsResultCode).GetBuffer();
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "ResetCardCounts(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }
}

////////////////////////////////////////////////////////////////////////////////
// CancelAllJobs()
//
// using the IBidiSpl interface, issue a 'cancel all jobs' command. This is
// simply a dxp01sdk::Cancel with both printerJobID and errorCode set to zero.
//
////////////////////////////////////////////////////////////////////////////////
void CancelAllJobs(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }
    // create the XML and convert to bytes for the upcoming SetInputData() call.
    // use zero for jobID and errorCode; this will cancel all cards in printer.
    CString actionXML = util::FormatPrinterActionXML(
        dxp01sdk::Cancel,
        0,  // printerJobID
        0); // errorcode

    const int xmlBytesLength = actionXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, actionXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_ACTION);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::PRINTER_ACTION):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) cancel xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Free the memory that was allocated for the IBidiSpl SendRecv call:
    ::CoTaskMemFree(xmlBytes);
}



////////////////////////////////////////////////////////////////////////////////
// AdjustColors()
//
// using the IBidiSpl interface, issue an 'AdjustColors' command.
// This sample changes only the Red settings. Blue and Green are unchanged.
//
////////////////////////////////////////////////////////////////////////////////

void AdjustColors(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;

    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // create the XML and convert to bytes for the upcoming SetInputData() call.
    // Changing the settings for the Red color channel while leaving the other two colors unchanged:
    CString formattedColorForRChannel;
    formattedColorForRChannel.Format(
        dxp01sdk::COLOR_CHANNEL,
        L"1", L"2", L"-1", L"0", L"25", L"6", L"-19", L"12", L"13", L"14", L"15");

    CString adjustColorXML;
    adjustColorXML.Format(
        dxp01sdk::ADJUST_COLOR_XML,
        formattedColorForRChannel, L"", L"");

    const int xmlBytesLength = adjustColorXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, adjustColorXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::ADJUST_COLOR);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::ADJUST_COLOR):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) adjust colors xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "AdjustColor():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    // Free the memory that was allocated for the IBidiSpl SendRecv call:
    ::CoTaskMemFree(xmlBytes);

}



////////////////////////////////////////////////////////////////////////////////
// SetDefaultColors()
//
// using the IBidiSpl interface, issue an 'SetDefaultColors' command.
// This sample only resets Red and Green.  Blue is unchanged.
//
////////////////////////////////////////////////////////////////////////////////
void SetDefaultColors(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // create the XML and convert to bytes for the upcoming SetInputData() call.
    // Setting red and green color channel values back to defaults and keeping blue unchanged
    CString defaultColorXML;
    defaultColorXML.Format(
        dxp01sdk::DEFAULT_COLOR_XML,
        L"true", L"true", L"false");

    const int xmlBytesLength = defaultColorXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

                                                        // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, defaultColorXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::SET_DEFAULT_COLOR);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::SET_DEFAULT_COLOR):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) default colors xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }


    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "SetDefaultColors():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    // Free the memory that was allocated for the IBidiSpl SendRecv call:
    ::CoTaskMemFree(xmlBytes);
}


////////////////////////////////////////////////////////////////////////////////
// isColorAdjustCapable()
//
// Returns true if printer is in READY state and firmware is >= D3.17.4
// otherwise returns false.
//
////////////////////////////////////////////////////////////////////////////////
bool isColorAdjustCapable(CComPtr <IBidiSpl> bidiSpl)
{
    stringstream exceptionText;

    CString printerVersionXml = util::GetPrinterOptionsXML(bidiSpl);
    util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerVersionXml);

    cout << endl << "printer state: " << CW2A(printerOptionsValues._printerStatus) << endl << endl;

    // verify that the printer is in the READY state
    if ("Ready" != printerOptionsValues._printerStatus ) {
        exceptionText << "ERROR:  printer is not in state 'READY'.";
        throw runtime_error(exceptionText.str());
    }

    cout << endl << "printer version: " << CW2A(printerOptionsValues._printerVersion) << endl << endl;
    util::FirmwareVersion actualFirmware = util::ParseFirmwareRev(printerOptionsValues._printerVersion);

    if ((actualFirmware._printerBase == "D3") && (actualFirmware._majorVersion > 16)) {
        if (actualFirmware._majorVersion > 17)
            return true;
        else if ((actualFirmware._majorVersion == 17) && (actualFirmware._minorVersion >= 4))
            return true;
    }

    exceptionText << "ERROR - Requires printer version: D3.17.4 (or greater)";
    throw runtime_error(exceptionText.str());
}



////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{

    CommandLineOptions commandLineOptions = GetCommandlineOptions(argc, argv);

    ValidateCommandLineOptions(commandLineOptions);

    CComPtr <IBidiSpl> bidiSpl;
    try {
        stringstream exceptionText;

        HRESULT hr = ::CoInitialize(NULL);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        // create an IBidiSpl COM smartpointer instance, and bind it to the
        // printer name. We use it for all subsequent call using the IBidi interface.
        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            exceptionText << "bidiSpl.CoCreateInstance:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl->BindDevice(commandLineOptions.printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice():" << " 0x" << hex << hr << " " << CT2A(commandLineOptions.printerName) << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        switch (commandLineOptions.command) {
        case cancelAllJobs:
            CancelAllJobs(bidiSpl);
            cout << "all jobs canceled for '" << CT2A(commandLineOptions.printerName) << "'" << endl;
            break;
        case restart:
            RestartPrinter(bidiSpl);
            cout << "command issued to restart the printer." << endl;
            break;
        case resetCardCounts:
            ResetCardCounts(bidiSpl);
            cout << "command issued to reset the printer's card counts." << endl;
            break;
		case adjustColors:
            cout << "AdjustColors - begin." << endl;
            if (isColorAdjustCapable(bidiSpl)) {
                AdjustColors(bidiSpl);
            }
            cout << "AdjustColors - end." << endl;
            break;
		case defaultColors:
            cout << "SetDefaultColors - begin." << endl;           
            if (isColorAdjustCapable(bidiSpl)) {
                SetDefaultColors(bidiSpl);
            }
            cout << "SetDefaultColors - end." << endl;
            break;
        default:
            throw runtime_error("unexpected commandline option");
        }
    }
    catch (runtime_error& e) {
        cerr << e.what() << endl << endl;
    }

    if (bidiSpl) {
        bidiSpl->UnbindDevice();
        bidiSpl = NULL;
    }

    ::CoUninitialize();

    return 0;
}