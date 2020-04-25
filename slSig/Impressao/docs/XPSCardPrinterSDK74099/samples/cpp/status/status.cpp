////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// xps card printer sdk c++ 'status' sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <bidispl.h>
#include "XGetopt.h"
#include "util.h"
#include "DXP01SDK.H"

using namespace std;

struct CommandLineOptions {
    CString  printerName;
    bool     jobStatus;
    CString  hopperID;
    CString  cardEjectSide;
    CommandLineOptions() : jobStatus(false)
        , hopperID(TEXT(""))
        , cardEjectSide(TEXT(""))
    {}
};

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates using interactive mode to retrieve printer options," << endl;
    cout << "printer messages, and job status." << endl << endl;
    cout << shortExeName << " -n <printername> [-j] [-i <input hopper>] [-f <side>]" << endl << endl;
    cout << "options:" << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -f <Front | Back>. Flip card on output." << endl;
    cout << "  -i <input hopper>. Defaults to input hopper #1" << endl;
    cout << "  -j Submit a card job and display job status. Optional." << endl;
    cout << shortExeName << " -n \"XPS Card Printer\" -j" << endl << endl;
    cout << "Retrieves printer options, starts a job, retrieves printer messages," << endl;
    cout << "ends the job, and retrieves job status." << endl;
    cout << endl << endl;
    ::exit(-1);
}

void DisplayPrinterStatus(const util::PrinterStatusValues& statusVals)
{
    cout << endl << "Status:" << endl;
    cout << "  ClientID:        " << CW2A(statusVals._clientID) << endl;
    cout << "  ErrorCode:       " << statusVals._errorCode << endl;
    cout << "  ErrorSeverity:   " << statusVals._errorSeverity << endl;
    cout << "  ErrorString:     " << CW2A(statusVals._errorString) << endl;
    cout << "  PrinterData:     " << CW2A(statusVals._printerData) << endl;
    cout << "  PrinterJobID:    " << statusVals._printerJobID << endl;
    cout << "  WindowsJobID:    " << statusVals._windowsJobID << endl;
}

void DisplayPrinterCardCount(const util::PrinterCardCountValues& countVals)
{
    cout << endl << "Counts:" << endl;
    cout << "  CardsPickedSinceCleaningCard: " << countVals._cardsPickedSinceCleaningCard << endl;
    cout << "  CleaningCardsRun:             " << countVals._cleaningCardsRun << endl;
    cout << "  CurrentCompleted:             " << countVals._currentCompleted << endl;
    cout << "  CurrentLost:                  " << countVals._currentLost << endl;
    cout << "  CurrentPicked:                " << countVals._currentPicked << endl;
    cout << "  CurrentPickedExceptionSlot:   " << countVals._currentPickedExceptionSlot << endl;
    cout << "  CurrentPickedInputHopper1:    " << countVals._currentPickedInputHopper1 << endl;
    cout << "  CurrentPickedInputHopper2:    " << countVals._currentPickedInputHopper2 << endl;
    cout << "  CurrentPickedInputHopper3:    " << countVals._currentPickedInputHopper3 << endl;
    cout << "  CurrentPickedInputHopper4:    " << countVals._currentPickedInputHopper4 << endl;
    cout << "  CurrentPickedInputHopper5:    " << countVals._currentPickedInputHopper5 << endl;
    cout << "  CurrentPickedInputHopper6:    " << countVals._currentPickedInputHopper6 << endl;
    cout << "  CurrentRejected:              " << countVals._currentRejected << endl;
    cout << "  PrinterStatus:                " << CW2A(countVals._printerStatus) << endl;
    cout << "  TotalCompleted:               " << countVals._totalCompleted << endl;
    cout << "  TotalLost:                    " << countVals._totalLost << endl;
    cout << "  TotalPicked:                  " << countVals._totalPicked << endl;
    cout << "  TotalPickedExceptionSlot:     " << countVals._totalPickedExceptionSlot << endl;
    cout << "  TotalPickedInputHopper1:      " << countVals._totalPickedInputHopper1 << endl;
    cout << "  TotalPickedInputHopper2:      " << countVals._totalPickedInputHopper2 << endl;
    cout << "  TotalPickedInputHopper3:      " << countVals._totalPickedInputHopper3 << endl;
    cout << "  TotalPickedInputHopper4:      " << countVals._totalPickedInputHopper4 << endl;
    cout << "  TotalPickedInputHopper5:      " << countVals._totalPickedInputHopper5 << endl;
    cout << "  TotalPickedInputHopper6:      " << countVals._totalPickedInputHopper6 << endl;
    cout << "  TotalRejected:                " << countVals._totalRejected << endl;
}

void DisplayPrinterOptions(const util::PrinterOptionsValues& optionVals)
{
    cout << endl << "Options:" << endl;
    cout << "  ColorPrintResolution:           " << CW2A(optionVals._colorPrintResolution) << endl;
    cout << "  ConnectionPortType:             " << CW2A(optionVals._connectionPortType) << endl;
    cout << "  ConnectionProtocol:             " << CW2A(optionVals._connectionProtocol) << endl;
    cout << "  EmbosserVersion:                " << CW2A(optionVals._embosserVersion) << endl;
    cout << "  Laminator:                      " << CW2A(optionVals._laminator) << endl;
    cout << "  LaminatorFirmwareVersion:       " << CW2A(optionVals._laminatorFirmwareVersion) << endl;
    cout << "  LaminatorImpresser:             " << CW2A(optionVals._laminatorImpresser) << endl;
    cout << "  LaminatorScanner:               " << CW2A(optionVals._laminatorScanner) << endl;
    cout << "  LaserFirmwareVersion:           " << CW2A(optionVals._laserFirmwareVersion) << endl;
    cout << "  LockState:                      " << CW2A(optionVals._lockState) << endl;
    cout << "  ModuleEmbosser:                 " << CW2A(optionVals._moduleEmbosser) << endl;
    cout << "  MonochromePrintResolution:      " << CW2A(optionVals._monochromePrintResolution) << endl;
    cout << "  MultiHopperVersion:             " << CW2A(optionVals._multiHopperVersion) << endl;
    cout << "  TopcoatPrintResolution:         " << CW2A(optionVals._topcoatPrintResolution) << endl;
    cout << "  OptionDuplex:                   " << CW2A(optionVals._optionDuplex) << endl;
    cout << "  OptionInputhopper:              " << CW2A(optionVals._optionInputhopper) << endl;
    cout << "  OptionLaser:                    " << CW2A(optionVals._optionLaser) << endl;
    cout << "  OptionLaserVisionRegistration:  " << CW2A(optionVals._optionLaserVisionRegistration) << endl;
    cout << "  OptionObscureBlackPanel:        " << CW2A(optionVals._optionObscureBlackPanel) << endl;
    cout << "  OptionLocks:                    " << CW2A(optionVals._optionLocks) << endl;
    cout << "  OptionMagstripe:                " << CW2A(optionVals._optionMagstripe) << endl;
    cout << "  OptionSecondaryMagstripeJIS:    " << CW2A(optionVals._optionSecondaryMagstripeJIS) << endl;
    cout << "  OptionPrinterBarcodeReader:     " << CW2A(optionVals._optionPrinterBarcodeReader) << endl;
    cout << "  OptionRewrite:                  " << CW2A(optionVals._optionRewrite) << endl;
    cout << "  OptionSmartcard:                " << CW2A(optionVals._optionSmartcard) << endl;
    cout << "  PrinterAddress:                 " << CW2A(optionVals._printerAddress) << endl;
    cout << "  PrintEngineType:                " << CW2A(optionVals._printEngineType) << endl;
    cout << "  PrinterMessageNumber:           " << optionVals._printerMessageNumber << endl;
    cout << "  PrinterModel:                   " << CW2A(optionVals._printerModel) << endl;
    cout << "  PrinterSerialNumber:            " << CW2A(optionVals._printerSerialNumber) << endl;
    cout << "  PrinterStatus:                  " << CW2A(optionVals._printerStatus) << endl;
    cout << "  PrinterVersion:                 " << CW2A(optionVals._printerVersion) << endl;
    cout << "  PrintHead:                      " << CW2A(optionVals._printHead) << endl;
}


void DisplaySupplies(const util::SuppliesValues& supplyvals)
{
    cout << endl << "Supplies:" << endl;
    cout << "  PrinterStatus:            " << CW2A(supplyvals._printerStatus) << endl;
    //
    // printer...
    cout << "  PrintRibbonType:            " << CW2A(supplyvals._printRibbonType) << endl;
    cout << "  RibbonRemaining:            " << supplyvals._ribbonRemaining << "%" << endl;
    cout << "  PrintRibbonSerialNumber:    " << CW2A(supplyvals._printRibbonSerialNumber) << endl;
    cout << "  PrintRibbonLotCode:         " << CW2A(supplyvals._printRibbonLotCode) << endl;
    cout << "  PrintRibbonPartNumber:      " << supplyvals._printRibbonPartNumber << endl;
    cout << "  PrintRibbonRegionCode:      " << supplyvals._printRibbonRegionCode << endl;
    cout << "  RetransferFilmRemaining:    " << supplyvals._retransferFilmRemaining << "%" << endl;
    cout << "  RetransferFilmSerialNumber: " << CW2A(supplyvals._retransferFilmSerialNumber) << endl;
    cout << "  RetransferFilmLotCode:      " << CW2A(supplyvals._retransferFilmLotCode) << endl;
    cout << "  RetransferFilmPartNumber:   " << supplyvals._retransferFilmPartNumber << endl;
    //
    // embosser...
    cout << "  IndentRibbon:               " << CW2A(supplyvals._indentRibbon) << endl;
    cout << "  IndentRibbonRemaining:      " << supplyvals._indentRibbonRemaining << "%" << endl;
    cout << "  IndentRibbonSerialNumber:   " << CW2A(supplyvals._indentRibbonSerialNumber) << endl;
    cout << "  IndentRibbonLotCode:        " << CW2A(supplyvals._indentRibbonLotCode) << endl;
    cout << "  IndentRibbonPartNumber:     " << supplyvals._indentRibbonPartNumber << endl;

    cout << "  TopperRibbonType:           " << CW2A(supplyvals._topperRibbonType) << endl;
    cout << "  TopperRibbonRemaining:      " << supplyvals._topperRibbonRemaining << "%" << endl;
    cout << "  TopperRibbonSerialNumber:   " << CW2A(supplyvals._topperRibbonSerialNumber) << endl;
    cout << "  TopperRibbonLotCode:        " << CW2A(supplyvals._topperRibbonLotCode) << endl;
    cout << "  TopperRibbonPartNumber:     " << supplyvals._topperRibbonPartNumber << endl;
    //
    // laminator...
    cout << "  L1LaminateType:             " << (unsigned) supplyvals._laminatorL1SupplyCode << endl;
    cout << "  L1LaminateRemaining:        " << supplyvals._laminatorL1PercentRemaining << "%" << endl;
    cout << "  L1LaminateSerialNumber:     " << CW2A(supplyvals._laminatorL1SerialNumber) << endl;
    cout << "  L1LaminateLotCode:          " << CW2A(supplyvals._laminatorL1LotCode) << endl;
    cout << "  L1LaminatePartNumber:       " << supplyvals._laminatorL1PartNumber << endl;

    cout << "  L2LaminateType:             " << (unsigned) supplyvals._laminatorL2SupplyCode << endl;
    cout << "  L2LaminateRemaining:        " << supplyvals._laminatorL2PercentRemaining << "%" << endl;
    cout << "  L2LaminateSerialNumber:     " << CW2A(supplyvals._laminatorL2SerialNumber) << endl;
    cout << "  L2LaminateLotCode:          " << CW2A(supplyvals._laminatorL2LotCode) << endl;
    cout << "  L2LaminatePartNumber:       " << supplyvals._laminatorL2PartNumber << endl;
}


void DisplayHopperStatus(const util::HOPPER_INFO_LIST& hopperInfoList)
{
	cout << endl << "HopperStatus:" << endl;
	cout << "  Number of Hoppers:  " << hopperInfoList.size() << endl;

    for (int i = 0; i < hopperInfoList.size(); i++) {
		cout << endl;
        cout << "  Hopper Index:       " << i << endl;
        cout << "  Name:               " << CW2A(hopperInfoList[i]._name) << endl;
        cout << "  Type:               " << CW2A(hopperInfoList[i]._type) << endl;
        cout << "  Status:             " << CW2A(hopperInfoList[i]._status) << endl;
        cout << "  Card Stock:         " << CW2A(hopperInfoList[i]._cardStock) << endl;
    }
}

void ValidateCommandLineOptions(CommandLineOptions& commandLineOptions)
{
    if (commandLineOptions.printerName.IsEmpty()) {
        usage();
    }

    // if hopperID is an empty string, that is OK
    if (commandLineOptions.hopperID != L"") {
        if (
            commandLineOptions.hopperID != L"1" &&
            commandLineOptions.hopperID != L"2" &&
            commandLineOptions.hopperID != L"3" &&
            commandLineOptions.hopperID != L"4" &&
            commandLineOptions.hopperID != L"5" &&
            commandLineOptions.hopperID != L"6" &&
            commandLineOptions.hopperID != L"exception") {
            cout << "invalid hopperID: " << CW2A(commandLineOptions.hopperID) << endl;
            ::exit(-1);
        }
    }

    // if cardEjectSide is an empty string, that is OK
    if (commandLineOptions.cardEjectSide != L"") {
        if (
            commandLineOptions.cardEjectSide != L"front" &&
            commandLineOptions.cardEjectSide != L"back") {
            cout << "invalid cardEjectSide: " << CW2A(commandLineOptions.cardEjectSide) << endl;
            ::exit(-1);
        }
    }
}

CommandLineOptions GetCommandlineOptions(const int argc, _TCHAR* argv[])
{
    CommandLineOptions commandLineOptions;
    int c(0);
    while ((c = getopt(argc, argv, _T("n:jf:i:"))) != EOF) {
        switch (c) {
        case L'n': commandLineOptions.printerName = optarg; break;
        case L'j': commandLineOptions.jobStatus = true; break;
        case L'i': commandLineOptions.hopperID = CString(optarg).MakeLower(); break;
        case L'f': commandLineOptions.cardEjectSide = CString(optarg).MakeLower(); break;
        default: usage();
        }
    }
    return commandLineOptions;
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int wmain(int argc, WCHAR* argv[])
{
    stringstream exceptionText;

    CommandLineOptions commandLineOptions = GetCommandlineOptions(argc, argv);
    ValidateCommandLineOptions(commandLineOptions);

    CString& printerName = commandLineOptions.printerName;  // renamed for convenience

    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

    try {
        HRESULT hr = ::CoInitialize(nullptr);
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

        hr = bidiSpl->BindDevice(printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice() error " << " 0x" << hex << hr << " '" << CT2A(printerName) << "' " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        const CString printerStatusXml = util::GetPrinterStatusXML(bidiSpl);
        const util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXml);
        DisplayPrinterStatus(printerStatusValues);

        const CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        const util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
        DisplayPrinterOptions(printerOptionsValues);

        const CString printerCardCountsXML = util::GetPrinterCardCountsXML(bidiSpl);
        const util::PrinterCardCountValues printerCardCountValues = util::ParsePrinterCardCountsXML(printerCardCountsXML);
        DisplayPrinterCardCount(printerCardCountValues);

        const CString printerSuppliesXML = util::GetSuppliesXML(bidiSpl);
        util::SuppliesValues suppliesValues = util::ParseSuppliesXML(printerSuppliesXML);
        DisplaySupplies(suppliesValues);

        const CString hopperStatusXML = util::GetHopperStatusXML(bidiSpl);
        util::HOPPER_INFO_LIST hopperInfoList = util::ParseHopperStatusXml(hopperStatusXML);
        DisplayHopperStatus(hopperInfoList);


        if (commandLineOptions.jobStatus ||
            commandLineOptions.hopperID.GetLength() > 0 ||
            commandLineOptions.cardEjectSide.GetLength() > 0)   // have already verified cardEjectSide
        {
            CString hopperID = "1";
            CString cardEjectSide = "Default";

            printerJobID = util::StartJob(
                bidiSpl,
                commandLineOptions.hopperID.GetLength() > 0 ? commandLineOptions.hopperID : hopperID,
                commandLineOptions.cardEjectSide.GetLength() > 0 ? commandLineOptions.cardEjectSide : cardEjectSide);

            util::EndJob(bidiSpl);

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