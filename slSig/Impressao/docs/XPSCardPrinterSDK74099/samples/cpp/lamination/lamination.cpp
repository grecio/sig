////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK lamination c++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <atlpath.h>
#include <atlimage.h>
#include <prntvpt.h>
#include "XGetopt.h"
#include "util.h"
#include "printTicketXml.h"
#include "LaminationActions.h"
#include "lamination.H"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl << shortExeName << " demonstrates lamination functionality of the printer and driver." << endl;
    cout << "Overrides the driver printing preference settings." << endl << endl;
    cout << "Usage:" << endl;
    cout << shortExeName << " -n <printername> [-x <F|B|A|T>] [-c] [-f <side>] [-i <input hopper>]" << endl;
    cout << "where" << endl;
    cout << "  -n <printername> specifies which printer to use. Required. " << endl;
    cout << "  -x specifies the laminator station to use." << endl;
    cout << "      Valid values are \"1\" and \"2\"." << endl;
    cout << "  Laminator actions:" << endl;
    cout << "      F --> laminate the front." << endl;
    cout << "      B --> laminate the back." << endl;
    cout << "      A --> laminate both front and back." << endl;
    cout << "      T --> laminate the front twice." << endl;
    cout << "      Spaces are allowed between laminator station and laminator action." << endl << endl;
    cout << "  -c Poll for job completion." << endl << endl;
    cout << "  -f <Front | Back>. Flip card on output." << endl;
    cout << "  -i <input hopper>. Defaults to input hopper #1" << endl;
    cout << shortExeName << " -n \"XPS Card Printer\" -1F -2B" << endl;
    cout << "  the front of the card will be laminated in station 1 and " << endl;
    cout << "  the back of the card will be laminated in station 2." << endl << endl;
    cout << shortExeName << " -n \"XPS Card Printer\" -1T" << endl;
    cout << "  The front of the card will be laminated in station 1 two times." << endl;
    cout << "  Station 2 is not used." << endl << endl;
    cout << shortExeName << " -n \"XPS Card Printer\" -1F -1B" << endl;
    cout << "  The back of the card will be laminated in station 1." << endl;
    cout << "  The last specification of a station will be used." << endl << endl;
    ::exit(-1);
}

CString GetPrintTicketXml(const CommandLineOptions& commandLineOptions)
{
    const CString L1ActionXML = LaminationActions::GetLaminationActionXML(commandLineOptions.L1Action);
    const CString L2ActionXML = LaminationActions::GetLaminationActionXML(commandLineOptions.L2Action);

    if (
        PrintTicketXml::LaminationActionInvalidAction == L1ActionXML ||
        PrintTicketXml::LaminationActionInvalidAction == L2ActionXML) {
        usage();
    }

    CString xml = PrintTicketXml::Prefix;

    xml += PrintTicketXml::FeatureNamePrefix
        + PrintTicketXml::JobLamination1FeatureName
        + L1ActionXML
        + PrintTicketXml::FeatureNameSuffix;

    xml += PrintTicketXml::FeatureNamePrefix
        + PrintTicketXml::JobLamination2FeatureName
        + L2ActionXML
        + PrintTicketXml::FeatureNameSuffix;

    xml += PrintTicketXml::Suffix;

    return xml;
}

void DrawCardFront(
    const HDC&                  printerDC,
    const CommandLineOptions&   commandLineOptions)
{
    stringstream exceptionText;

    int rc = ::StartPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "StartPage(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    CString text;
    text.Format(L"%s", commandLineOptions.printerName);
    rc = ::TextOut(printerDC, 50, 100, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    const CString shortKBitmapFilename(TEXT("mono.bmp"));
    CPath longKBitmapFilename;
    longKBitmapFilename.Combine(util::GetExePath(), shortKBitmapFilename);
    CImage kBitmap;
    rc = kBitmap.Load(longKBitmapFilename);
    if (FAILED(rc)) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "Load(" << CW2A(longKBitmapFilename) << ") " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = kBitmap.Draw(printerDC, 50, 150);
    if (!rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "kBitmap.Draw(): " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    const CString shortColorBitmapFilename(TEXT("color.bmp"));
    CPath longColorBitmapFilename;
    longColorBitmapFilename.Combine(util::GetExePath(), shortColorBitmapFilename);
    CImage colorBitmap;
    rc = colorBitmap.Load(longColorBitmapFilename);
    if (FAILED(rc)) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "Load(" << CW2A(longColorBitmapFilename) << ") " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = colorBitmap.Draw(printerDC, 50, 300);
    if (!rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "colorBitmap.Draw(): " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    // UV
    const CString shortUVBitmapFilename(TEXT("uv.bmp"));
    CPath longUVBitmapFilename;
    longUVBitmapFilename.Combine(util::GetExePath(), shortUVBitmapFilename);
    CImage UVBitmap;
    rc = UVBitmap.Load(longUVBitmapFilename);
    if (FAILED(rc)) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "Load(" << CW2A(longColorBitmapFilename) << ") " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = UVBitmap.Draw(printerDC, 50, 450);
    if (!rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "colorBitmap.Draw(): " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = ::EndPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "EndPage(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
}

int CreatePrintJob(
    const HANDLE                /*printerHandle*/,
    const CommandLineOptions&   commandLineOptions,
    DEVMODE*                    devmode)
{
    stringstream exceptionText;

    HDC printerDC = ::CreateDC(
        L"WINSPOOL",
        CT2W(commandLineOptions.printerName),
        NULL,
        devmode);
    if (NULL == printerDC) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "CreateDC(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    DOCINFO docInfo = {0};
    docInfo.cbSize = sizeof DOCINFO;
    docInfo.lpszDocName = L"XPS Driver SDK lamination sample document";
    int rc = ::StartDoc(printerDC, &docInfo);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "StartDoc(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    const DWORD windowsJobID = rc; // needed for job completion

    DrawCardFront(printerDC, commandLineOptions);

    rc = ::EndDoc(printerDC);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "EndDoc(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = ::DeleteDC(printerDC);
    if (rc == 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "DeleteDC(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    return windowsJobID;
}

int PrintTextAndGraphics(const CommandLineOptions& commandLineOptions)
{
    stringstream exceptionText;

    CString printTicketXml = GetPrintTicketXml(commandLineOptions);

    // for the given printer get the maxSchemaVersion needed for the
    // PTOpenProvider() call:
    DWORD maxSchemaVersion(0);
    int rc = ::PTQuerySchemaVersionSupport(commandLineOptions.printerName, &maxSchemaVersion);
    if (FAILED(rc)) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::PTQuerySchemaVersionSupport("
            << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // the the handle to the printticket printTicketProviderHandle.
    HPTPROVIDER printTicketProviderHandle(0);
    rc = ::PTOpenProvider(commandLineOptions.printerName, maxSchemaVersion, &printTicketProviderHandle);
    if (FAILED(rc)) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::PTOpenProvider(" << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // get a printer handle:
    HANDLE   printerHandle(0);
    rc = ::OpenPrinter(CT2W(commandLineOptions.printerName), &printerHandle, NULL);
    if (!rc) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::OpenPrinter(" << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // get a DEVMODE. we need to get the buffer size first.
    rc = ::DocumentProperties(
        0,
        printerHandle,
        CT2W(commandLineOptions.printerName),
        NULL,
        NULL,
        0);
    if (0 > rc) {
        const DWORD lastError = ::GetLastError();
        exceptionText << ":::DocumentProperties(" << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    ULONG devmode_size = rc;

    // this buffer will nicely self-destruct:
    vector <byte>  devmodeBytes(devmode_size, 0);
    DEVMODE*       devmode = (DEVMODE*) (&devmodeBytes[0]);

    rc = ::DocumentProperties(
        NULL,
        printerHandle,
        CT2W(commandLineOptions.printerName),
        devmode,
        NULL,
        DM_OUT_BUFFER);
    if (rc < 0) {
        const DWORD lastError = ::GetLastError();
        exceptionText << ":::DocumentProperties(" << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // use the DEVMODE for standard devmode items:
    devmode->dmCopies = 1;
    devmode->dmDuplex = DMDUP_HORIZONTAL;
    devmode->dmOrientation = DMORIENT_PORTRAIT;

    util::UpdateDevmode(printerHandle, commandLineOptions.printerName, devmode);

    CComPtr <IStream> printTicketStream;
    util::CreateIStream(printTicketStream);

    rc = ::PTConvertDevModeToPrintTicket(
        printTicketProviderHandle,
        devmode_size,
        devmode,
        kPTJobScope,
        printTicketStream);
    if (FAILED(rc)) {
        exceptionText << "::PTConvertDevModeToPrintTicket(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }

    rc = printTicketStream->Commit(0);
    if (FAILED(rc)) {
        exceptionText << "printTicketStream->Commit(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }

    /////////////////////////////////////////////////////////////////////////////
    // construct the entire print ticket now, regardless of whether or not we
    // are printing one or two sides.
    /////////////////////////////////////////////////////////////////////////////
    CComPtr <IStream> deltaTicketStream;
    ULONG deltaTicketStreamBytesWritten(0);

    util::CreateIStream(deltaTicketStream);
    rc = deltaTicketStream->Write(
        CW2A(printTicketXml.GetBuffer()),
        printTicketXml.GetLength(),
        &deltaTicketStreamBytesWritten);
    if (FAILED(rc)) {
        exceptionText << "deltaTicketStream->Write(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }

    CComPtr <IStream> resultTicketStream;
    util::CreateIStream(resultTicketStream);
    util::RewindIStream(printTicketStream);
    util::RewindIStream(deltaTicketStream);

    CComBSTR printTicketErrorMessage;
    rc = ::PTMergeAndValidatePrintTicket(
        printTicketProviderHandle,
        printTicketStream,
        deltaTicketStream,
        kPTJobScope,
        resultTicketStream,
        &printTicketErrorMessage);
    if (S_PT_NO_CONFLICT != rc) {
        exceptionText << "::PTMergeAndValidatePrintTicket(): " << rc << " "
            << CT2A(util::Win32ErrorString(rc)) << " " << CW2T(printTicketErrorMessage);
        throw runtime_error(exceptionText.str());
    }

    util::RewindIStream(resultTicketStream);

    // convert the merged printticket to a DEVMODE. we'll use the
    // DEVMODE as a parameter to the CreateDC() call.
    rc = ::PTConvertPrintTicketToDevMode(
        printTicketProviderHandle,
        resultTicketStream,
        kUserDefaultDevmode,
        kPTJobScope,
        &devmode_size,
        &devmode,
        &printTicketErrorMessage);
    if (FAILED(rc)) {
        exceptionText << "::PTConvertPrintTicketToDevMode(): " << rc << " "
            << CT2A(util::Win32ErrorString(rc)) << " " << CW2T(printTicketErrorMessage);
        throw runtime_error(exceptionText.str());
    }

    const int windowsJobID = CreatePrintJob(printerHandle, commandLineOptions, devmode);

    rc = ::PTCloseProvider(printTicketProviderHandle);
    if (FAILED(rc)) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::PTCloseProvider() " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    rc = ::ClosePrinter(printerHandle);
    if (FAILED(rc)) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::ClosePrinter() " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // free the devmode from the PTConvertPrintTicketToDevMode() call:
    rc = ::PTReleaseMemory(devmode);
    if (FAILED(rc)) {
        const DWORD lastError = ::GetLastError();
        cerr << "::PTReleaseMemory() " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        // not treating this as fatal.
    }

    return windowsJobID;
}

CommandLineOptions GetCommandlineOptions(const int argc, _TCHAR*argv[])
{
    int c(0);

    CommandLineOptions commandLineOptions;

    while ((c = getopt(argc, argv, TEXT("n:1:2:cf:i:"))) != EOF) {
        switch (c) {
        case L'n':
            commandLineOptions.printerName = optarg;
            break;
        case L'1':
            commandLineOptions.L1Action = LaminationActions::GetLaminationAction(optarg);
            break;
        case L'2':
            commandLineOptions.L2Action = LaminationActions::GetLaminationAction(optarg);
            break;

        case L'c': commandLineOptions.jobCompletion = true; break;
        case L'i': commandLineOptions.hopperID = CString(optarg).MakeLower(); break;
        case L'f': commandLineOptions.cardEjectSide = CString(optarg).MakeLower(); break;

        default:
            usage();
        }
    }

    return commandLineOptions;
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    CommandLineOptions commandLineOptions = GetCommandlineOptions(argc, argv);
    commandLineOptions.Validate();

    CComPtr <IBidiSpl> bidiSpl;
    stringstream exceptionText;
    int printerJobID(0);

    try {
        HRESULT hr = ::CoInitialize(NULL);
        if (FAILED(hr)) {
            exceptionText << "::CoInitialize:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        // create an IBidiSpl COM smartpointer instance, and bind it to the
        // printer name. We use it for all subsequent calls using the IBidi interface.
        hr = bidiSpl.CoCreateInstance(CLSID_BidiSpl);
        if (FAILED(hr)) {
            exceptionText << "bidiSpl.CoCreateInstance:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl->BindDevice(commandLineOptions.printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice() error " << " 0x" << hex << hr << " '" <<
                CT2A(commandLineOptions.printerName) << "' " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        const util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
        if ("Ready" != printerOptionsValues._printerStatus &&
            "Busy" != printerOptionsValues._printerStatus) {
            exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " is not ready.";
            throw runtime_error(exceptionText.str());
        }

        //  A printer may have both laminator station 1 and station 2 installed.
        //  In that case the option will be "L1, L2" so 'Find' as opposed to
        //  '==' must be used...
        if (LaminationActions::doesNotApply != commandLineOptions.L1Action) {
            if (-1 == printerOptionsValues._laminator.Find(CString("L1"))) {
                exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " does not have a station 1 laminator installed.";
                throw runtime_error(exceptionText.str());
            }
        }

        if (LaminationActions::doesNotApply != commandLineOptions.L2Action) {
            if (-1 == printerOptionsValues._laminator.Find(CString("L2"))) {
                exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " does not have a station 2 laminator installed.";
                throw runtime_error(exceptionText.str());
            }
        }

        // to poll for job completion we need the printerJobID:
    // to poll for job completion we need the printerJobID: or start job using the 
    // hopperID from the command line arguments
            if (commandLineOptions.jobCompletion ||
                commandLineOptions.hopperID.GetLength() > 0 ||
                commandLineOptions.cardEjectSide.GetLength() > 0)   // have already verified cardEjectSide
            {
                CString hopperID = "1";
                CString cardEjectSide = "Default";

                printerJobID = util::StartJob(
                    bidiSpl,
                    commandLineOptions.hopperID.GetLength() > 0 ? commandLineOptions.hopperID : hopperID,
                    commandLineOptions.cardEjectSide.GetLength() > 0 ? commandLineOptions.cardEjectSide : cardEjectSide);
            }

        const int windowsJobID = PrintTextAndGraphics(commandLineOptions);

        if (0 != printerJobID) {
            // We issued an IBidi StartJob() to check for supplies or poll for
            // job completion. Wait for all the print data to be spooled:
            util::WaitUntilJobSpooled(commandLineOptions.printerName, windowsJobID);

            // notify the printer that all the data has been sent for the card:
            util::EndJob(bidiSpl);
        }

        if (commandLineOptions.jobCompletion) {
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