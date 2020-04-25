////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// xps driver sdk loosely-coupled c++ smartcard sample
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <WinSpool.h>
#include <iostream>
#include <sstream>
#include "XGetopt.h"
#include "util.h"
#include "DXP01SDK.H"

// Escapes are top to area; left to area; width; height : card in landscape orientation
const CString printBlockingEscape = "~PB%16.51 7.34 14.99 14.99?";
const CString topcoatRemovalEscape = "~TR%16.51 7.34 14.99 14.99?";

void usage()
{
    const auto msgFormat = LR"(
%s demonstrates interactive mode parking a card in the smart card
station, moving the card from the station, and options to print and poll for
job completion. This sample uses hardcoded data for printing.

%s -n <printername> [-p] [-b] [-c] [-f <side>] [-i <input hopper>]

options:
  -n <printername>. Required. Try -n "XPS Card Printer".
  -p Print simple black text on one or both sides of the card
     depending on the printer used.
  -b use back of card for smartcard chip.
  -c Poll for job completion.
  -f <Front | Back>. Flip card on output.
  -i <input hopper>. Defaults to input hopper #1

        %s -n "XPS Card Printer"

        Parks a card in the printer smart card station, asks you to continue or reject
and then does what you requested.

%s -n "XPS Card Printer" -p -c

Parks a card in printer smart card station, moves the card from the station
prints black text on one or both sides of the card, and polls and displays
job status.
)";

    const CStringW shortExeName = util::GetShortExeName();

    CStringW msg;
    msg.Format(msgFormat, shortExeName, shortExeName, shortExeName, shortExeName);
    std::wcout << msg.GetBuffer() << std::endl;
    std::exit(0);
}

util::PrinterStatusValues ParkCard(CComPtr <IBidiSpl>& bidiSpl, const bool parkBack)
{
    std::stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw std::runtime_error(exceptionText.str());
    }

    const auto parkCommand = parkBack ? dxp01sdk::SMARTCARD_PARK_BACK : dxp01sdk::SMARTCARD_PARK;
    hr = bidiRequest->SetSchema(parkCommand);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::SMARTCARD_PARK)):" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) park:" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    HRESULT parkCardResultCode(S_OK);
    hr = bidiRequest->GetResult(&parkCardResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(park):" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    if (FAILED(parkCardResultCode)) {
        exceptionText << "parkCardResultCode:" << " 0x" << std::hex << parkCardResultCode << " " << util::Win32ErrorString(parkCardResultCode).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
    util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
    if (0 != printerStatusValues._errorCode) {
        exceptionText << "ParkCard(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    return printerStatusValues;
}

void ResumeCard(const int printerJobID, CComPtr <IBidiSpl>&  bidiSpl)
{
    std::stringstream exceptionText;
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw std::runtime_error(exceptionText.str());
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
        exceptionText << "SetSchema(dxp01sdk::XPS_CARDPRINTER_SCHEMA_PRINTER_ACTION):" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        exceptionText << "bidiRequest->SetInputData():" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        exceptionText << "SendRecv(BIDI_ACTION_SET) resume card:" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
        throw std::runtime_error(exceptionText.str());
    }
}

////////////////////////////////////////////////////////////////////////////////
// PersonalizeSmartcard()
////////////////////////////////////////////////////////////////////////////////
bool PersonalizeSmartcard()
{
    std::cout << "displaying messagebox:" << std::endl;

    CStringW caption(L"A card has been parked in smartcard module");
    CStringW message(
        L"This is where smartcard personalization happens.\r\n\r\n"
        L"Press 'Yes' to simulate a successful smartcard personalization.\r\n\r\n"
        L"Press 'No' to simulate a failed smartcard personalization.");
    int rc = ::MessageBoxW(NULL, message, caption, MB_YESNO);

    return IDYES == rc;
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
int wmain(int argc, WCHAR* argv[])
{
    CString printerName;
    bool print(false);
    bool pollForJobCompletion(false);
    bool parkBack(false);
    CString commandLineHopperID = L"";
    CString commandLineCardEjectSide = L"";

    int c(0);
    while ((c = getopt(argc, argv, L"n:pbcf:i:")) != EOF) {
        switch (c) {
        case L'n': printerName = optarg; break;
        case L'p': print = true; break;
        case L'b': parkBack = true; break;
        case L'c': pollForJobCompletion = true; break;
        case L'i':
            commandLineHopperID = CString(optarg).MakeLower();
            ValidateHopperID(commandLineHopperID);
            break;
        case L'f': commandLineCardEjectSide = CString(optarg).MakeLower();
            ValidateCardEjectSide(commandLineCardEjectSide);
            break;
        default: usage();
        }
    }

    if (printerName.IsEmpty()) {
        usage();
    }

    std::stringstream exceptionText;
    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

    try {
        HRESULT hr = ::CoInitialize(nullptr);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw std::runtime_error(exceptionText.str());
        }

        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            exceptionText << "bidiSpl.CoCreateInstance:" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw std::runtime_error(exceptionText.str());
        }

        hr = bidiSpl->BindDevice(printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice():" << " 0x" << std::hex << hr << " " << CT2A(printerName) << " " << CT2A(util::Win32ErrorString(hr));
            throw std::runtime_error(exceptionText.str());
        }

        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        std::cout << std::endl << "driver version: " << CW2A(driverVersionString) << std::endl << std::endl;

        // see if the printer is in the Printer_Ready state:
        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);

        if (print && ("Installed" != printerOptionsValues._printHead)) {
            exceptionText << "printer " << CT2A(printerName) << " does not have a print head installed.";
            throw std::runtime_error(exceptionText.str());
        }

        if ("Installed" != printerOptionsValues._optionSmartcard) {
            exceptionText << "printer " << CT2A(printerName) << " does not have the smartcard option installed.";
            throw std::runtime_error(exceptionText.str());
        }

        if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
            exceptionText << "printer " << CT2A(printerName) << " is not ready.";
            throw std::runtime_error(exceptionText.str());
        }

        CString hopperID = L"1";
        CString cardEjectSide = L"Default";
        printerJobID = util::StartJob(
            bidiSpl,
            commandLineHopperID.GetLength() > 0 ? commandLineHopperID : hopperID,
            commandLineCardEjectSide.GetLength() > 0 ? commandLineCardEjectSide : cardEjectSide);

        // park the card in the smartcard reader:
        util::PrinterStatusValues printerStatusValues = ParkCard(bidiSpl, parkBack);

        // the card has been parked in the smartcard reader:
        const bool personalizedOK = PersonalizeSmartcard();

        if (!personalizedOK) {
            exceptionText << "smartcard personalization failed.";
            throw std::runtime_error(exceptionText.str());
        }

        std::cout << "smartcard personalization succeeded." << std::endl;

        ResumeCard(printerJobID, bidiSpl);

        if (print) {
            const bool doTwoSided =
                printerOptionsValues._optionDuplex == "Auto" ||
                printerOptionsValues._optionDuplex == "Manual";

            // Important: util::PrintTextmethod also waits until driver gets all the print data.
            util::Escapes   escapes = {printBlockingEscape, topcoatRemovalEscape, parkBack ? util::escapeOnCardBack : util::escapeOnCardFront};
            util::PrintText(printerName, doTwoSided, escapes);
        }

        if (personalizedOK) {
            util::EndJob(bidiSpl);
        }

        if (pollForJobCompletion) {
            util::PollForJobCompletion(bidiSpl, printerJobID);
        }
    }
    catch (util::BidiException& e) {
        std::cerr << e.what() << std::endl;
        util::CancelJob(bidiSpl, e.printerJobID, e.errorCode);
    }
    catch (std::runtime_error& e) {
        std::cerr << e.what() << std::endl;
        if (0 != printerJobID) {
            util::CancelJob(bidiSpl, printerJobID, 0);
        }
    }

    if (bidiSpl) {
        bidiSpl->UnbindDevice();
        bidiSpl = nullptr;
    }

    ::CoUninitialize();

    return 0;
}