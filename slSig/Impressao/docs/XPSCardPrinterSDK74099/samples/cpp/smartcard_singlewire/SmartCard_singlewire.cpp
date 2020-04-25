////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer Driver SDK: Single-wire smartcard personalization c++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <WinSpool.h>
#include <iostream>
#include <iomanip>
#include <sstream>
#include "XGetopt.h"
#include "util.h"
#include "DXP01SDK.H"
#include "DXP01SDK_SCARD.H"

// Escapes are top to area; left to area; width; height : card in landscape orientation
const CString printBlockingEscape = "~PB%16.51 7.34 14.99 14.99?";
const CString topcoatRemovalEscape = "~TR%16.51 7.34 14.99 14.99?";

enum CardType {
    contact, contactless, undetermined
};

void usage()
{
    const auto msgFormat = LR"(
%s demonstrates smartcard personalization using the
single-wire embedded reader. Once staged, the app connects to a contact
and then a contactless chip. For each, an ATR is issued and some data bytes
are sent and received.

Optionally prints, encodes a magstripe, and waits for job completion.

Uses hardcoded data for printing.

%s -n <printername> [-p] [-c] [-b] [-f <side>] [-i <input hopper>]

options:
  -n <printername>. Required. Try -n "XPS Card Printer".
  -p Print simple black text on one or both sides of the card depending on the
     printer used. Optional.
  -c Poll for job completion. Optional.
  -b use back of card for smartcard chip.
  -f <Front | Back>. Flip card on output.
  -i <input hopper>. Defaults to input hopper #1
)";

    const CStringW shortExeName = util::GetShortExeName();
    CStringW msg;
    msg.Format(msgFormat, shortExeName, shortExeName);
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
// SCardResultMessage()
// format a nice message for display.
// SCARD errors are declared in WinError.H
////////////////////////////////////////////////////////////////////////////////
std::string SCardResultMessage(const long scardResult, const std::string message)
{
    std::stringstream exceptionText;
    exceptionText << message
        << " result: "
        << scardResult
        << "; "
        << CT2A(util::Win32ErrorString(scardResult));
    return exceptionText.str();
}

////////////////////////////////////////////////////////////////////////////////
// PersonalizeChip()
//
// talk to the chip (contact or contactless). return TRUE if we were able to
// connect to the chip at all.
////////////////////////////////////////////////////////////////////////////////
bool PersonalizeChip(dxp01sdk::SCard& scard)
{
    std::vector <DWORD> states;
    std::vector <byte> ATRBytes;
    DWORD protocol(SCARD_PROTOCOL_UNDEFINED);

    long scardResult = scard.SCardStatus(states, protocol, ATRBytes);
    std::cout << SCardResultMessage(scardResult, "SCardStatus()") << std::endl;

    if (scardResult != SCARD_S_SUCCESS) {
        // unable to get a valid SCardStatus() response; we're done.
        return false;
    }

    std::cout << "SCardStatus() states: ";
    for (size_t index = 0; index < states.size(); index++) {
        std::wcout << states[index] << " " << dxp01sdk::SCard::StringFromState(states[index]) << " ";
    } std::cout << std::endl;

    const std::vector <DWORD>::iterator cardAbsent = find(states.begin(), states.end(), SCARD_ABSENT);
    if (cardAbsent != states.end()) {
        // one of the states is SCARD_ABSENT; we're done.
        return false;
    }

    std::cout << "SCardStatus() ATRBytes (hex): ";
    for (size_t index = 0; index < ATRBytes.size(); index++) {
        std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(ATRBytes[index]) << " ";
    }
    std::cout << std::dec << std::endl;

    // try all these 'get attribute' items:
    DWORD attributesToTryArray[] = {
        SCARD_ATTR_VENDOR_NAME,
        SCARD_ATTR_VENDOR_IFD_SERIAL_NO,
        SCARD_ATTR_VENDOR_IFD_TYPE,
        SCARD_ATTR_VENDOR_IFD_VERSION
    };
    std::vector <DWORD> attributesToTry(attributesToTryArray, attributesToTryArray + ARRAYSIZE(attributesToTryArray));

    for (size_t attribIndex = 0; attribIndex < attributesToTry.size(); attribIndex++) {
        std::vector <byte> scardAttributeBytes;

        { // optional
            CString s;
            s.Format(TEXT("calling SCardGetAttrib(%d)"), attributesToTry[attribIndex]);
            util::EchoCurrentTime(s);
        }

        scardResult = scard.SCardGetAttrib(attributesToTry[attribIndex], scardAttributeBytes);
        if (SCARD_S_SUCCESS != scardResult)
            continue;

        CString msg;
        msg.Format(L"SCardGetAttrib(%ls)", dxp01sdk::SCard::StringFromAttr(attributesToTry[attribIndex]).c_str());
        std::cout << SCardResultMessage(scardResult, std::string(CT2A(msg))) << std::endl;

        switch (attributesToTry[attribIndex]) {
        case SCARD_ATTR_VENDOR_IFD_VERSION:
        {
            // vendor-supplied interface device version: DWORD in the form 0xMMmmbbbb where
            //      MM = major version;
            //      mm = minor version; and
            //      bbbb = build number:
            const size_t byteCount = scardAttributeBytes.size();
            const short minorVersion = byteCount > 0 ? scardAttributeBytes[0] : 0;
            const short majorVersion = byteCount > 1 ? scardAttributeBytes[1] : 0;
            short buildNumber = 0;
            if (byteCount > 3) {
                buildNumber = (scardAttributeBytes[2] << 8) + scardAttributeBytes[3];
            }
            std::cout << "  SCARD_ATTR_VENDOR_IFD_VERSION:" << std::endl;
            std::cout << "    major: " << majorVersion << std::endl;
            std::cout << "    minor: " << minorVersion << std::endl;
            std::cout << "    build: " << buildNumber << std::endl;
        }
        break;

        default:
            scardAttributeBytes.push_back(0); // null-terminate this std::string for display.
            std::string stringFromBytes = (char *) &scardAttributeBytes[0];
            std::cout << " " << stringFromBytes << std::endl;
        }
    }

    // create a byte std::vector for the upcoming SCardTransmit() method.
    // these particular bytes should function with this type of contact chip:
    //
    //      MPCOS-EMV 16k
    //      GEMPLUS
    //      Datacard part number 806062-002
    //
    byte bytes[] = {0x00, 0xA4, 0x00, 0x00};
    std::vector <byte> sendBytes(bytes, bytes + ARRAYSIZE(bytes));

    std::cout << "sending bytes (hex): ";
    for (size_t index = 0; index < sendBytes.size(); index++) {
        std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(sendBytes[index]) << " ";
    }
    std::cout << std::dec << std::endl;

    // for those bytes, we should receive 0x61, 0x12
    std::vector <byte> receivedBytes;

    scardResult = scard.SCardTransmit(sendBytes, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS == scardResult && receivedBytes.size()) {
        std::cout << "  " << receivedBytes.size() << " bytes received (hex): ";
        for (size_t index = 0; index < receivedBytes.size(); index++) {
            std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(receivedBytes[index]) << " ";
        }
        std::cout << std::dec << std::endl;
    }

    receivedBytes.clear();
    sendBytes = {0x00, 0xC0, 0x00, 0x00, 0x12};

    std::cout << "sending bytes (hex): ";
    for (size_t index = 0; index < sendBytes.size(); index++) {
        std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(sendBytes[index]) << " ";
    }
    std::cout << std::dec << std::endl;

    // for those bytes, we should receive
    //  0x85, 0x10, 0x80, 0x01, 0x3F, 0x00, 0x38, 0x00, 0x00, 0x00,
    //  0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x6B, 0x90, 0x00

    scardResult = scard.SCardTransmit(sendBytes, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS == scardResult && receivedBytes.size()) {
        std::cout << "  " << receivedBytes.size() << " bytes received (hex): ";
        for (size_t index = 0; index < receivedBytes.size(); index++) {
            std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(receivedBytes[index]) << " ";
        }
        std::cout << std::dec << std::endl;
    }

    return true;
}

////////////////////////////////////////////////////////////////////////////////
// PersonalizeSmartcard()
// smartcard personalization using Win32 / WinSCard.H styled wrapper class.
////////////////////////////////////////////////////////////////////////////////
CardType PersonalizeSmartcard(CComPtr <IBidiSpl>& bidiSpl)
{
    bool contactPersonalized(false);
    bool contactlessPersonalized(false);
    CardType cardType(undetermined);

    // create an instance of the wrapper for the Win32 SCard...() functions:
    dxp01sdk::SCard scard(bidiSpl);

    // try a contacted chip:
    DWORD protocol(SCARD_PROTOCOL_UNDEFINED);

    long scardResult = scard.SCardConnect(dxp01sdk::SCard::contact, protocol);
    std::cout << SCardResultMessage(scardResult, "SCardConnect(contact)") << std::endl;
    if (SCARD_S_SUCCESS == scardResult) {
        cardType = contact;
        std::cout << "SCardConnect(contact) protocol[s]: ";
        std::vector <std::wstring> protocols = dxp01sdk::SCard::StringsFromProtocol(protocol);
        for (size_t index = 0; index < protocols.size(); index++) {
            std::wcout << protocols[index] << " ";
        } std::cout << std::endl;

        contactPersonalized = PersonalizeChip(scard);

        scardResult = scard.SCardDisConnect(SCARD_LEAVE_CARD);
        std::cout << SCardResultMessage(scardResult, "SCardDisConnect()") << std::endl << std::endl;
    }

    // try a contactless chip:
    protocol = SCARD_PROTOCOL_UNDEFINED;

    scardResult = scard.SCardConnect(dxp01sdk::SCard::contactless, protocol);
    std::cout << SCardResultMessage(scardResult, "SCardConnect(contactless)") << std::endl;
    if (SCARD_S_SUCCESS == scardResult) {
        cardType = contactless;
        std::cout << "SCardConnect(contactless) protocol[s]: ";
        std::vector <std::wstring> protocols = dxp01sdk::SCard::StringsFromProtocol(protocol);
        for (size_t index = 0; index < protocols.size(); index++) {
            std::wcout << protocols[index] << " ";
        } std::cout << std::endl;

        contactlessPersonalized = PersonalizeChip(scard);

        scardResult = scard.SCardDisConnect(SCARD_LEAVE_CARD);
        std::cout << SCardResultMessage(scardResult, "SCardDisConnect()") << std::endl << std::endl;
    }

    if (!contactPersonalized && !contactlessPersonalized) {
        std::stringstream exceptionText;
        exceptionText << "neither contact nor contactless chip personalization succeeded. canceling job.";
        throw std::runtime_error(exceptionText.str());
    }
    return cardType;
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
    bool jobCompletion(false);
    bool parkBack(false);
    CString commandLineHopperID = L"";
    CString commandLineCardEjectSide = L"";

    int c(0);
    while ((c = getopt(argc, argv, L"n:pcbf:i:")) != EOF) {
        switch (c) {
        case L'n': printerName = optarg; break;
        case L'p': print = true; break;
        case L'b': parkBack = true; break;
        case L'c': jobCompletion = true; break;
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

    CComPtr <IBidiSpl> bidiSpl;
    std::stringstream exceptionText;
    int printerJobID(0);

    try {
        HRESULT hr = ::CoInitialize(nullptr);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw std::runtime_error(exceptionText.str());
        }

        // create an IBidiSpl COM smartpointer instance, and bind it to the
        // printer name. We use it for all subsequent call using the IBidi interface.
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

        // optionally display driver version
        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        std::cout << std::endl << "driver version: " << CW2A(driverVersionString) << std::endl << std::endl;

        // see if the printer is in the Printer_Ready state:
        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
        if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
            exceptionText << "printer " << CT2A(printerName) << " is not ready.";
            throw std::runtime_error(exceptionText.str());
        }

        // this application uses the single-wire method for communicating with
        // a smartcard reader in the printer. If the printer is not configured with
        // that option, we're done.
        if ("Single wire" != printerOptionsValues._optionSmartcard) {
            exceptionText << "printer '" << CT2A(printerName) << "' needs 'Single Wire' for smartcard option. ";
            exceptionText << "'" << CT2A(printerOptionsValues._optionSmartcard) << "' was returned." << std::endl;
            throw std::runtime_error(exceptionText.str());
        }

        if (print && ("Installed" != printerOptionsValues._printHead)) {
            exceptionText << "printer '" << CT2A(printerName) << "' does not have a print head installed." << std::endl;;
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

        CardType cardType = PersonalizeSmartcard(bidiSpl);

        ResumeCard(printerJobID, bidiSpl);

        if (print) {
            const bool doTwoSided =
                printerOptionsValues._optionDuplex == "Auto" ||
                printerOptionsValues._optionDuplex == "Manual";

            // Important: util::PrintText method also waits until driver gets all the print data.
            if (cardType == contact) {
                util::Escapes   escapes = {printBlockingEscape, topcoatRemovalEscape, parkBack ? util::escapeOnCardBack : util::escapeOnCardFront};
                util::PrintText(printerName, doTwoSided, escapes);
            }
            else {
                util::PrintText(printerName, doTwoSided);
            }
        }

        // notify the printer that all the data has been sent for the card:
        util::EndJob(bidiSpl);

        if (jobCompletion) {
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