////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer Driver SDK: Single-wire smartcard personalization c++ sample
// for a mifare chip.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <iostream>
#include <iomanip>
#include <sstream>
#include <atltime.h>
#include "DXP01SDK.H"
#include "DXP01SDK_SCARD.H"
#include "XGetopt.h"
#include "util.h"
#include "CommandlineOptions.h"
#include "MiFare_Command.h"
#include "MiFare_Common.h"
#include "MiFare_Response.h"

void usage()
{
    const auto msgFormat = LR"(
%s demonstrates interactive mode parking a card
in the smart card station, performing single-wire smartcard functions,
moving the card from the station, and options to print and poll for job
completion.

Uses hardcoded data for printing.

%s -n <printername> [-p] [-b] [-c]

options:
  -n <printername>. Required. Try -n "XPS Card Printer".
  -p Print simple black text on one or both sides of the card depending on the
     printer used.
  -b use back of card for smartcard chip.
  -c Poll for job completion.

        %s -n "XPS Card Printer"
  Parks a card in the printer smart card station, connects to the MiFare chip
  and performs some MiFare chip activities.

        %s -n "XPS Card Printer" -p -c
  Parks a card in printer smart card station, moves the card from the station,
  prints simple text and graphics on the front side of the card, and polls and
  displays job status.
)";

    const CStringW shortExeName = util::GetShortExeName();
    CStringW msg;
    msg.Format(msgFormat, shortExeName, shortExeName, shortExeName, shortExeName);
    std::wcout << msg.GetBuffer() << std::endl;
    std::exit(0);
}

void DisplayProtocols(const UINT protocol)
{
    auto protocolStrings = dxp01sdk::SCard::StringsFromProtocol(protocol);
    std::vector <std::wstring>::iterator iter;
    std::cout << "   protocol[s]:" << std::endl;
    for (iter = protocolStrings.begin(); iter < protocolStrings.end(); iter++) {
        std::wcout << "  " << iter->c_str();
    }
    std::cout << std::endl;
}

void DisplayByteVectorAsHex(std::string title, std::vector <byte> bytes)
{
    std::cout << title << ":" << std::endl;
    for (size_t index = 0; index < bytes.size(); index++) {
        std::cout << std::setw(2) << std::setfill('0') << std::uppercase << std::hex << int(bytes[index]) << " ";
    }
    std::cout << std::dec << std::endl;
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
        exceptionText << "SetSchema(dxp01sdk::XPS_CARDPRINTER_SDK_SCHEMA_SMARTCARD_PARK):" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
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
        exceptionText << "parkcard(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
        throw util::BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
    }

    return printerStatusValues;
}

void ResumeCard(const int printerJobID, CComPtr <IBidiSpl>& bidiSpl)
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
        exceptionText << "SendRecv(BIDI_ACTION_SET) ResumeCard:" << " 0x" << std::hex << hr << " " << util::Win32ErrorString(hr).GetBuffer();
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
// GetSCardStatus()
//
// issue the SCardStatus() call. this is important, because the SCardConnect()
// call always succeeds - as long as the card is staged.
//
// the SCardConnect() call returns the Answer-To-Reset bytes (ATR).
////////////////////////////////////////////////////////////////////////////////
void GetSCardStatus(dxp01sdk::SCard& scard)
{
    std::stringstream exceptionText;
    std::vector <DWORD> states;
    std::vector <byte> ATRBytes;
    DWORD protocol(SCARD_PROTOCOL_UNDEFINED);

    long scardResult = scard.SCardStatus(states, protocol, ATRBytes);
    if (scardResult != SCARD_S_SUCCESS) {
        exceptionText << SCardResultMessage(scardResult, "SCardStatus()");
        throw std::runtime_error(exceptionText.str());
    }

    std::cout << "SCardStatus() states: ";
    for (size_t index = 0; index < states.size(); index++) {
        std::wcout << states[index] << " " << dxp01sdk::SCard::StringFromState(states[index]) << " ";
    } std::cout << std::endl;

    const std::vector <DWORD>::iterator cardAbsent = find(states.begin(), states.end(), SCARD_ABSENT);
    if (cardAbsent != states.end()) {
        // one of the states is SCARD_ABSENT; we're done.
        throw std::runtime_error("one of the states is SCARD_ABSENT.");
    }

    DisplayByteVectorAsHex("SCardStatus() ATRBytes (std::hex)", ATRBytes);
}

////////////////////////////////////////////////////////////////////////////////
//  DisplayChipInfo()
//
// get mifare status. see pg. 23, "CCID_Protocol_spec_120224_.pdf"
////////////////////////////////////////////////////////////////////////////////
void DisplayChipInfo(dxp01sdk::SCard& scard)
{
    std::stringstream exceptionText;

    dxp01sdk::MiFare_Command miFareCommand;
    std::vector <byte> getCardStatusCommand = miFareCommand.CreateGetCardStatusCommand();

    std::vector <byte> receivedBytes;

    long scardResult = scard.SCardTransmit(getCardStatusCommand, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardTransmit()");
        throw std::runtime_error(exceptionText.str());
    }

    if (SCARD_S_SUCCESS == scardResult && receivedBytes.size()) {
        DisplayByteVectorAsHex("bytes received (std::hex)", receivedBytes);

        { // optional
            MiFareResponse miFareResponse(receivedBytes);
            std::cout << "mifare status OK?: " << (miFareResponse.IsStatusOK() ? "true" : "false") << std::endl;
        }
    }
}

void LoadKeys(
    dxp01sdk::SCard& scard,
    const byte sector,
    const dxp01sdk::MiFare_Command::KeyType keyType)
{
    std::stringstream exceptionText;

    dxp01sdk::MiFare_Common miFareCommon;
    const std::vector <byte> miFareTestKey = miFareCommon.GetMifareTestKey();

    dxp01sdk::MiFare_Command miFareCommand;
    const std::vector <byte> loadKeysCommand = miFareCommand.CreateLoadKeysCommand(
        keyType,
        sector,
        miFareTestKey);

    std::vector <byte> receivedBytes;

    long scardResult = scard.SCardTransmit(loadKeysCommand, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardTransmit()");
        throw std::runtime_error(exceptionText.str());
    }

    if (0 == receivedBytes.size()) {
        throw std::runtime_error("no response from LoadKeys()");
    }

    DisplayByteVectorAsHex("LoadKeys() response", receivedBytes);

    // optional:
    {
        CStringW statusCodeString = miFareCommon.StatusCodeBytesToString(receivedBytes);
        std::wcout << statusCodeString.GetBuffer() << std::endl;
        miFareCommon.CheckStatusCode(receivedBytes[0]);
    }
}

////////////////////////////////////////////////////////////////////////////////
// WriteData()
//
// write some sample data to the given block and sector.
////////////////////////////////////////////////////////////////////////////////
void WriteData(
    dxp01sdk::SCard&    scard,
    const byte          sector,
    const byte          block,
    std::vector <byte>  testDataBytes)
{
    std::stringstream exceptionText;

    dxp01sdk::MiFare_Command miFareCommand;
    const std::vector <byte> writeBlockCommand = miFareCommand.CreateWriteBlockCommand(
        dxp01sdk::MiFare_Command::A,
        sector,
        block,
        testDataBytes);

    std::vector <byte> receivedBytes;

    long scardResult = scard.SCardTransmit(writeBlockCommand, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardTransmit()");
        throw std::runtime_error(exceptionText.str());
    }

    if (0 == receivedBytes.size()) {
        throw std::runtime_error("write data failed.");
    }

    DisplayByteVectorAsHex("Write Block response", receivedBytes);

    // optional:
    {
        dxp01sdk::MiFare_Common miFareCommon;
        CString statusCodeString = miFareCommon.StatusCodeBytesToString(receivedBytes);
        std::cout << CT2A(statusCodeString) << std::endl;
        miFareCommon.CheckStatusCode(receivedBytes[0]);
    }
}

////////////////////////////////////////////////////////////////////////////////
// ReadData()
//
// read some data from the given block and sector.
////////////////////////////////////////////////////////////////////////////////
void ReadData(
    dxp01sdk::SCard&    scard,
    const byte          sector,
    const byte          block)
{
    std::stringstream exceptionText;

    dxp01sdk::MiFare_Command myFareCommand;
    const std::vector <byte> readBlockCommand = myFareCommand.CreateReadBlockCommand(
        dxp01sdk::MiFare_Command::A,
        sector,
        block);

    std::vector <byte> receivedBytes;

    long scardResult = scard.SCardTransmit(readBlockCommand, receivedBytes);
    std::cout << SCardResultMessage(scardResult, "SCardTransmit()") << std::endl;
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardTransmit()");
        throw std::runtime_error(exceptionText.str());
    }

    if (0 == receivedBytes.size()) {
        throw std::runtime_error("no response from read block command");
    }

    DisplayByteVectorAsHex("read block response", receivedBytes);

    { // optional
        dxp01sdk::MiFare_Common miFareCommon;
        CString statusCodeString = miFareCommon.StatusCodeBytesToString(receivedBytes);
        std::cout << CT2A(statusCodeString) << std::endl;
        miFareCommon.CheckStatusCode(receivedBytes[0]);
    }

    // start at byte[1]; byte[0] is zero, and there are two trailing statuscode bytes:
    CStringA receivedDataString((PCHAR) &receivedBytes[1], (int) receivedBytes.size() - 3);
    std::cout
        << "received '" << (PCHAR) receivedDataString.GetBuffer()
        << "' from sector " << (short) sector
        << ", block " << (short) block
        << std::endl;
}

std::string create_test_data_string()
{
    const CTime now(CTime::GetCurrentTime());
    const CString testDataCString(now.Format(TEXT("%Y.%m.%d.%H.%M")));
    const std::string testDataString = CT2A(testDataCString);
    return testDataString;
}

////////////////////////////////////////////////////////////////////////////////
// PersonalizeSmartcard()
// smartcard personalization using Win32 / WinSCard.H styled wrapper class.
////////////////////////////////////////////////////////////////////////////////
void PersonalizeSmartcard(CComPtr <IBidiSpl>& bidiSpl)
{
    std::stringstream exceptionText;

    dxp01sdk::SCard scard(bidiSpl);

    // prepare for a contactless mifare chip.
    DWORD protocol = SCARD_PROTOCOL_UNDEFINED;

    long scardResult = scard.SCardConnect(dxp01sdk::SCard::contactless, protocol);
    std::cout << SCardResultMessage(scardResult, "SCardConnect()") << std::endl;
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardConnect()");
        throw std::runtime_error(exceptionText.str());
    }

    DisplayProtocols(protocol); // optional

    try {
        GetSCardStatus(scard);

        DisplayChipInfo(scard);

        const byte sector = 5;
        const byte block = 1;
        const auto keyType = dxp01sdk::MiFare_Command::A;

        LoadKeys(scard, sector, keyType);

        ReadData(scard, sector, block);

        const std::string testDataString = create_test_data_string();
        std::vector <byte> testDataBytes(testDataString.c_str(), testDataString.c_str() + testDataString.length());
        std::cout << "writing '" << testDataString << "' to chip." << std::endl;

        WriteData(scard, sector, block, testDataBytes);

        ReadData(scard, sector, block);
    }
    catch (std::runtime_error& e) {
        scardResult = scard.SCardDisConnect(SCARD_LEAVE_CARD);
        throw std::runtime_error(e);
    }

    scardResult = scard.SCardDisConnect(SCARD_LEAVE_CARD);
    if (SCARD_S_SUCCESS != scardResult) {
        exceptionText << SCardResultMessage(scardResult, "SCardDisConnect()");
        throw std::runtime_error(exceptionText.str());
    }

    std::cout << SCardResultMessage(scardResult, "SCardDisConnect()") << std::endl << std::endl;
}

CommandlineOptions GetCommandLineOptions(const int argc, WCHAR* argv[])
{
    CommandlineOptions commandlineOptions;
    int c(0);
    while ((c = getopt(argc, argv, L"n:pcb")) != EOF) {
        switch (c) {
        case L'n': commandlineOptions.printerName = optarg; break;
        case L'p': commandlineOptions.print = true; break;
        case L'c': commandlineOptions.cardCompletion = true; break;
        case L'b': commandlineOptions.parkBack = true; break;
        default: usage();
        }
    }

    if (commandlineOptions.printerName.IsEmpty()) {
        usage();
    }

    return commandlineOptions;
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int wmain(int argc, WCHAR* argv[])
{
    const CommandlineOptions cmdLineOpts = GetCommandLineOptions(argc, argv);

    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

    try {
        HRESULT hr = ::CoInitialize(nullptr);
        if (FAILED(hr)) {
            std::cerr << "::CoInitialize:" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            return -1;
        }

        // create an IBidiSpl COM smartpointer instance, and bind it to the
        // printer name. We use it for all subsequent call using the IBidi interface.
        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            std::cerr << "bidiSpl.CoCreateInstance:" << " 0x" << std::hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            return -1;
        }

        hr = bidiSpl->BindDevice(cmdLineOpts.printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            std::cerr << "BindDevice():" << " 0x" << std::hex << hr << " " << CT2A(cmdLineOpts.printerName) << " " << CT2A(util::Win32ErrorString(hr));
            return -1;
        }

        // optionally display driver version
        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        std::cout << std::endl << "driver version: " << CW2A(driverVersionString) << std::endl << std::endl;

        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
        if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
            std::cerr << "printer " << CT2A(cmdLineOpts.printerName) << " is not ready.";
            return -1;
        }

        // this application uses the single-wire method for communicating with
        // a smartcard reader in the printer. If the printer is not configured with
        // that option, we're done.
        if ("Single wire" != printerOptionsValues._optionSmartcard) {
            std::cerr << "printer '" << CT2A(cmdLineOpts.printerName) << "' does not have single-wire smartcard option. Exiting." << std::endl;;
            return -1;
        }

        CString hopperID;
        printerJobID = util::StartJob(
            bidiSpl,
            hopperID);

        const auto printerStatusValues = ParkCard(bidiSpl, cmdLineOpts.parkBack);

        PersonalizeSmartcard(bidiSpl);

        ResumeCard(printerJobID, bidiSpl);

        if (cmdLineOpts.print) {
            const bool doTwoSided =
                printerOptionsValues._optionDuplex == "Auto" ||
                printerOptionsValues._optionDuplex == "Manual";

            // Important: util::PrintText() waits until driver gets all the print data.
            util::PrintText(cmdLineOpts.printerName, doTwoSided);
        }

        util::EndJob(bidiSpl);

        if (cmdLineOpts.cardCompletion) {
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