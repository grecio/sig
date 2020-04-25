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
#include "util.h"
#include "XGetopt.h"
#include "DXP01SDK.H"

using namespace std;

static const CString c_threeTrackMagstripePrintBlocking = "~PB%5 0 86 13.5?";
static const CString c_threeTrackMagstripeTopcoatRemoval = "~TR%5 0 86 13.5?";
static const CString c_JISMagstripePrintBlocking = "~PB%5.5 0 86 8.0?";
static const CString c_JISMagstripeTopcoatRemoval = "~TR%5.5 0 86 8.0?";

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates interactive mode magnetic stripe encoding with" << endl;
    cout << "options to read the magnetic stripe data, print, and poll for job completion" << endl;
    cout << "status." << endl << endl;
    cout << "Uses hardcoded data for magnetic stripe and printing." << endl << endl;
    cout << shortExeName << " -n <printername> [-e] [-r] [-p] [-c] [-j] [-f <side>] [-i <input hopper>]" << endl << endl;
    cout << "options:" << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -e Encodes magnetic stripe data." << endl;
    cout << "  -r Reads back the encoded magnetic stripe data." << endl;
    cout << "  -p Print simple black text on one or both sides of the card" << endl;
    cout << "     depending on the printer used." << endl;
    cout << "  -c Poll for job completion; needed to check for magstripe errors." << endl;
    cout << "  -f <Front | Back>. Flip card on output." << endl;
    cout << "  -i <input hopper>. Defaults to input hopper #1" << endl;
    cout << "  -j JIS magnetic." << endl;
    cout << endl << shortExeName << " -n \"XPS Card Printer\" -e -r" << endl;
    cout << "Encodes data on all three tracks of an ISO 3-track magnetic stripe on" << endl;
    cout << "the backside of a card, reads and displays it." << endl << endl;
    cout << shortExeName << " -n \"XPS Card Printer\" -r -p -c" << endl;
    cout << "Reads data on all three tracks of an ISO 3-track magnetic stripe on" << endl;
    cout << "the backside of a card and displays it, prints black text on one or both sides of the card," << endl;
    cout << "and polls and displays job status." << endl;
    cout << endl;
    exit(0);
}

void EncodeMagstripe(
    CComPtr <IBidiSpl>&  bidiSpl,
    const bool           JISRequest)
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

    if (JISRequest) {
        // JIS only allows track 3 = "321"
        trackDataXML = L"<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            L"<magstripe>"
            L"<track number=\"1\"><base64Data></base64Data></track>"
            L"<track number=\"2\"><base64Data></base64Data></track>"
            L"<track number=\"3\"><base64Data>MzIx</base64Data></track>"
            L"</magstripe>";
    }

    // Data is the length of the string multiplied by size of wide char
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

void ReadMagstripe(
    CComPtr <IBidiSpl>&  bidiSpl,
    const bool           bJISRequest)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // replace schema string dxp01sdk::MAGSTRIPE_READ to dxp01sdk::MAGSTRIPE_READ_FRONT for front side read
    hr = bidiRequest->SetSchema(dxp01sdk::MAGSTRIPE_READ);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetSchema(dxp01sdk::MAGSTRIPE_READ):"
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiSpl->SendRecv(BIDI_ACTION_GET):"
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // Check if device encoded data and SendRecv was a success
    HRESULT MagstripeEncodeResult(S_OK);
    hr = bidiRequest->GetResult(&MagstripeEncodeResult);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(MagstripeEncodeResult)) {
        exceptionText << "MagstripeEncodeResult code:" << " 0x" << hex << MagstripeEncodeResult
            << " " << CT2A(util::Win32ErrorString(MagstripeEncodeResult));
        throw runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "ReadMagStripe():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    CString track1;
    CString track2;
    CString track3;

    util::ParseMagstripeStrings(printerStatusXML, track1, track2, track3, bJISRequest);

    int trackBytesLength = track1.GetLength();
    if (trackBytesLength) {
        wcout << L"track1:" << L" " << track1.GetBuffer() << endl;
        vector <byte> trackBytes(trackBytesLength);
        ::Base64Decode(CT2A(track1), trackBytesLength, &trackBytes[0], &trackBytesLength);
        string s;
        s.append((char*) &trackBytes[0], trackBytesLength);
        cout << "track1 Base64 decoded: " << s << endl << endl;
    }
    else cout << "no track1 data read." << endl;

    trackBytesLength = track2.GetLength();
    if (trackBytesLength) {
        wcout << L"track2:" << L" " << track2.GetBuffer() << endl;
        vector <byte> trackBytes(trackBytesLength);
        ::Base64Decode(CT2A(track2), trackBytesLength, &trackBytes[0], &trackBytesLength);
        string s;
        s.append((char*) &trackBytes[0], trackBytesLength);
        cout << "track2 Base64 decoded: " << s << endl << endl;
    }
    else cout << "no track2 data read." << endl;

    trackBytesLength = track3.GetLength();
    if (trackBytesLength) {
        wcout << L"track3:" << L" " << track3.GetBuffer() << endl;
        vector <byte> trackBytes(trackBytesLength);
        ::Base64Decode(CT2A(track3), trackBytesLength, &trackBytes[0], &trackBytesLength);
        string s;
        s.append((char*) &trackBytes[0], trackBytesLength);
        cout << "track3 Base64 decoded: " << s << endl << endl;
    }
    else cout << "no track3 data read." << endl;
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

void ValidateCardEjectSide(CString cardEjectSide)
{
    if (cardEjectSide != L"") {
        if (
            cardEjectSide != L"front" &&
            cardEjectSide != L"back") {
            std::cout << "invalid cardEjectSide: " << CW2A(cardEjectSide) << std::endl;
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
    bool readMagstripe(false);
    bool encodeMagstripe(false);
    bool print(false);
    bool pollForJobCompletion(false);
    bool jisRequest(false);
    CString commandLineHopperID = L"";
    CString commandLineCardEjectSide = L"";

    int c(0);
    while ((c = getopt(argc, argv, _T("n:erpcjf:i:"))) != EOF) {
        switch (c) {
        case TEXT('n'): printerName = optarg; break;
        case TEXT('r'): readMagstripe = true; break;
        case TEXT('e'): encodeMagstripe = true; break;
        case TEXT('p'): print = true; break;
        case TEXT('c'): pollForJobCompletion = true; break;
        case TEXT('j'): jisRequest = true; break;
        case TEXT('i'):
            commandLineHopperID = CString(optarg).MakeLower();
            ValidateHopperID(commandLineHopperID);
            break;
        case TEXT('f'): commandLineCardEjectSide = CString(optarg).MakeLower();
            ValidateCardEjectSide(commandLineCardEjectSide);
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

        // see if the printer is in the Printer_Ready state:
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

        if (print && ("Installed" != printerOptionsValues._printHead)) {
            exceptionText << "printer " << CT2A(printerName) << " does not have a print head installed.";
            throw runtime_error(exceptionText.str());
        }

        //  A printer may have a JIS magnetic stripe unit in addition to an ISO unit.
        //  In that case the option will be "ISO, JIS" so 'Find' as opposed to
        //  '==' must be used...
        if (jisRequest) {
            if (printerOptionsValues._optionMagstripe.Find(CString("JIS")) == -1) {
                exceptionText << "printer " << CT2A(printerName) << " does not have a JIS magnetic stripe unit installed.";
                throw runtime_error(exceptionText.str());
            }
        }
        else {
            if (printerOptionsValues._optionMagstripe.Find(CString("ISO")) == -1) {
                exceptionText << "printer " << CT2A(printerName) << " does not have an ISO magnetic stripe unit installed.";
                throw runtime_error(exceptionText.str());
            }
        }

        CString hopperID = L"1";
        CString cardEjectSide = L"Default";
        printerJobID = util::StartJob(
            bidiSpl,
            commandLineHopperID.GetLength() > 0 ? commandLineHopperID : hopperID,
            commandLineCardEjectSide.GetLength() > 0 ? commandLineCardEjectSide : cardEjectSide);

        if (encodeMagstripe) {
            EncodeMagstripe(bidiSpl, jisRequest);
        }

        if (readMagstripe) {
            ReadMagstripe(bidiSpl, jisRequest);
        }

        if (print) {
            // do some simple text and graphics printing.
            const bool doTwoSided =
                printerOptionsValues._optionDuplex == "Auto" ||
                printerOptionsValues._optionDuplex == "Manual";

            util::Escapes magStripeEscape;
            if (jisRequest) {
                magStripeEscape.printBlockingEscape = c_JISMagstripePrintBlocking;
                magStripeEscape.topcoatRemovalEscape = c_JISMagstripeTopcoatRemoval;
                magStripeEscape.escapeSide = util::escapeOnCardBack;
            }
            else {
                magStripeEscape.printBlockingEscape = c_threeTrackMagstripePrintBlocking;
                magStripeEscape.topcoatRemovalEscape = c_threeTrackMagstripeTopcoatRemoval;
                magStripeEscape.escapeSide = util::escapeOnCardBack;
            }
             
            // Important: util::PrintTextmethod also waits until driver gets all the print data.
            util::PrintText(printerName, doTwoSided, magStripeEscape);
        }

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