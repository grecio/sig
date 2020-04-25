////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK c++ 'Print' sample application.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <prntvpt.h>
#include <vector>
#include <atlimage.h>
#include <atlpath.h>
#include "XGetopt.h"
#include "util.h"
#include "printTicketXml.h"
#include "DXP01SDK.H"
#include "CommandLineOptions.h"
#include "print.h"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl << shortExeName << " demonstrates print functionality of the printer and driver." << endl << endl;
    cout << "Uses hardcoded data for printing, magnetic stripe, topcoat region" << endl;
    cout << "  and print blocking region." << endl << endl;
    cout << "Overrides the driver printing preference settings." << endl << endl;
    cout << shortExeName << " -n <printername> [-r front | -r back] [-2]" << endl;
    cout << "  [-o frontPort | -o backPort] [ -s <number of copies>] [-m ]" << endl;
    cout << "  [-t all | -t chip | -t magJIS | -t mag2 | -t mag3 | -t custom] " << endl;
    cout << "  [-u all | -u chip | -u magJIS | -u mag2 | -u mag3 | -u custom] [-x]" << endl;
    cout << "  [-i <input hopper>] [-e] [-f <side>]" << endl << endl;
    cout << "options: " << endl;
    cout << "  -n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  -r <front | back >. Rotates the card image by 180 degrees for " << endl;
    cout << "     front, back, or both sides. Use both '-r front' and '-r back'" << endl;
    cout << "     to rotate both sides 180 degrees." << endl;
    cout << "  -2 Prints a 2-sided (duplex) card. Default is front-side printing." << endl;
    cout << "  -o <frontPort | backPort >. Sets portrait orientation for a card side. " << endl;
    cout << "     Use both '-o frontPort' and '-o backPort' for portrait orientation" << endl;
    cout << "     on both sides of the card." << endl;
    cout << "     Default is landscape orientation for both card sides. " << endl;
    cout << "  -s <number of copies>. Default is 1." << endl;
    cout << "  -m Writes 3-track magnetic stripe data to backside of card using escapes. " << endl;
    cout << "     Default is no encoding." << endl;
    cout << "  -d < All | Off | Front | Back > Disable Printing. Default is Off." << endl;
    cout << "  -t <all | chip | magJIS | mag2 | mag3 | custom>" << endl;
    cout << "     Top coat and print blocking region for front of card. Use \'-t all\' to " << endl;
    cout << "     topcoat the entire card side with no print blocking. " << endl;
    cout << "     Default is the current driver setting." << endl;
    cout << "  -u <all | chip | magJIS | mag2 | mag3 | custom>" << endl;
    cout << "     Top coat and print blocking region for back of card. Use \'-u all\' to" << endl;
    cout << "     topcoat the entire card side with no print blocking." << endl;
    cout << "     Default is the current driver setting." << endl;
    cout << "  -x Display the print ticket data. Default is no display." << endl;
    cout << "  -i <input hopper>. Defaults to input hopper #1" << endl;
    cout << "  -e Checks the status of the input hopper if input hopper is specified." << endl;
    cout << "  -c Poll for job completion." << endl << endl;
    cout << "  -f <Front | Back>. Flip card on output." << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\"" << endl;
    cout << "  Prints a one-sided landscape card." << endl << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -i 1 -e" << endl;
    cout << "  Checks the status of input hopper 1 and prints a one-sided landscape card" << endl;
    cout << "  if cards are present." << endl << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -r front -r back -2 -o frontport" << endl;
    cout << "  Prints a two-sided card with both sides of card image" << endl;
    cout << "  rotated 180 degrees and with front side as portrait orientation" << endl;
    cout << "  and back side as landscape orientation." << endl << endl;
    cout << "  " << shortExeName << " -n \"XPS Card Printer\" -2 -t all -u mag3" << endl;
    cout << "  Prints a two-sided card with topcoat applied over all of side one" << endl;
    cout << "  and topcoat and printing blocked over the 3-track magnetic stripe " << endl;
    cout << "  area on the backside of the card." << endl << endl;
    ::exit(-1);
}

CString GetTopcoatBlockingPrintTicketXml(const CommandLineOptions& commandLineOptions)
{
    CString topcoatBlockingXml;

    // front:
    do {
        if (commandLineOptions.topcoatBlockingFront.IsEmpty()) {
            // use the current driver settings.
            break;
        }

        if (commandLineOptions.topcoatBlockingFront == L"custom") {
            // we will generate topcoat and blocking escapes for the card front.
            // escapes override the PrintTicket.
            break;
        }

        if (commandLineOptions.topcoatBlockingFront == L"all") {
            topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_All;
            break;
        }

        // we need the 'exception' markup for the remaining settings:
        topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_Except;

        if (commandLineOptions.topcoatBlockingFront == L"chip") {
            topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_ISO_7816;
            break;
        }

        if (commandLineOptions.topcoatBlockingFront == L"magjis") {
            topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_JIS;
            break;
        }

        if (commandLineOptions.topcoatBlockingFront == L"mag2") {
            topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_ISO_2Track;
            break;
        }

        if (commandLineOptions.topcoatBlockingFront == L"mag3") {
            topcoatBlockingXml += PrintTicketXml::FrontTopcoatBlockingPreset_ISO_3Track;
            break;
        }
    } while (false);

    // back:
    do {
        if (commandLineOptions.topcoatBlockingBack.IsEmpty()) {
            // use the current driver settings.
            break;
        }

        if (commandLineOptions.topcoatBlockingBack == L"custom") {
            // we will generate topcoat and blocking escapes for the card back.
            // escapes override the PrintTicket.
            break;
        }

        if (commandLineOptions.topcoatBlockingBack == L"all") {
            topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_All;
            break;
        }

        // we need the 'exception' markup for the remaining settings:
        topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_Except;

        if (commandLineOptions.topcoatBlockingBack == L"chip") {
            topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_ISO_7816;
            break;
        }

        if (commandLineOptions.topcoatBlockingBack == L"magjis") {
            topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_JIS;
            break;
        }

        if (commandLineOptions.topcoatBlockingBack == L"mag2") {
            topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_ISO_2Track;
            break;
        }

        if (commandLineOptions.topcoatBlockingBack == L"mag3") {
            topcoatBlockingXml += PrintTicketXml::BackTopcoatBlockingPreset_ISO_3Track;
            break;
        }
    } while (false);

    return topcoatBlockingXml;
}

CString GetPrintTicketXml(const CommandLineOptions& commandLineOptions)
{
    CString xml = PrintTicketXml::Prefix;

    xml += commandLineOptions.rotateFront ? PrintTicketXml::FlipFrontFlipped : PrintTicketXml::FlipFrontNone;
    xml += commandLineOptions.rotateBack ? PrintTicketXml::FlipBackFlipped : PrintTicketXml::FlipBackNone;

    switch (commandLineOptions.disablePrinting) {
    case CommandLineOptions::All:
        xml += PrintTicketXml::DisablePrintingAll;
        break;
    case CommandLineOptions::Off:
        xml += PrintTicketXml::DisablePrintingOff;
        break;
    case CommandLineOptions::Front:
        xml += PrintTicketXml::DisablePrintingFront;
        break;
    case CommandLineOptions::Back:
        xml += PrintTicketXml::DisablePrintingBack;
        break;
    }

    xml += GetTopcoatBlockingPrintTicketXml(commandLineOptions);
    xml += PrintTicketXml::Suffix;

    return xml;
}

void WriteCustomTopcoatBlockingEscapesFront(
    const HDC&                  printerDC,
    const CommandLineOptions&   commandLineOptions)
{
    stringstream exceptionText;

    if (commandLineOptions.topcoatBlockingFront != L"custom")
        return;

    // a 'topcoat Add' escape will force topcoat OFF for the entire card side.

    // units are millimeters; landscape basis; top left width height:
    // a rectangle one inch down; two inches wide; 1 cm high.
    // units are millimeters; top left width height:
    CString topCoatAddEsc = L"~TA%25.4 0 50.8 10;";
    topCoatAddEsc += L"40 60 7 7?";  // add a square, lower

    int rc = ::TextOut(printerDC, 10, 10, topCoatAddEsc, topCoatAddEsc.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    // a 'blocking' escape will override the driver settings:
    const CString blockingEsc = L"~PB% 0 19 3 54;";

    rc = ::TextOut(printerDC, 40, 40, blockingEsc, blockingEsc.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
}

void WriteCustomTopcoatBlockingEscapesBack(
    const HDC&                  printerDC,
    const CommandLineOptions&   commandLineOptions)
{
    stringstream exceptionText;

    if (commandLineOptions.topcoatBlockingBack != L"custom")
        return;

    // a 'topcoat Remove' escape will force topcoat OFF for the entire card side.
    const CString topCoatAddEsc = L"~TA%25.4 10 50.8 20;";

    int rc = ::TextOut(printerDC, 10, 10, topCoatAddEsc, topCoatAddEsc.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    // a 'blocking' escape will override the driver settings:
    const CString blockingEsc = L"~PB% 0 23 3 54;";

    rc = ::TextOut(printerDC, 40, 40, blockingEsc, blockingEsc.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
}

void WriteMagstripeEscapes(const HDC& printerDC)
{
    stringstream exceptionText;

    // emit some plain track 1, 2, 3 data. Assume IAT track configuration.
    const CString track1Escape(L"~1ABC 123");
    const CString track2Escape(L"~2456");
    const CString track3Escape(L"~3789");

    int rc = ::TextOut(printerDC, 0, 0, track1Escape, track1Escape.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = ::TextOut(printerDC, 0, 0, track2Escape, track2Escape.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    rc = ::TextOut(printerDC, 0, 0, track3Escape, track3Escape.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
}

void DrawCardBack(
    const HANDLE                printerHandle,
    const HDC&                  printerDC,
    const CommandLineOptions&   commandLineOptions,
    DEVMODE*                    devmode)
{
    stringstream exceptionText;

    // reset the devmode with proper back side orientation:
    devmode->dmOrientation = commandLineOptions.portraitBack ? DMORIENT_PORTRAIT : DMORIENT_LANDSCAPE;
    util::UpdateDevmode(printerHandle, commandLineOptions.printerName, devmode);
    if (NULL == ::ResetDC(printerDC, devmode)) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "::ResetDC(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    int rc = ::StartPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "StartPage(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    CString text;
    text.Format(L"%s back on", util::GetShortExeName());
    rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    text.Format(L"%s", commandLineOptions.printerName);
    rc = ::TextOut(printerDC, 50, 100, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
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

    rc = colorBitmap.Draw(printerDC, 350, 150);
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

    rc = UVBitmap.Draw(printerDC, 650, 150);
    if (!rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "colorBitmap.Draw(): " << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
    WriteCustomTopcoatBlockingEscapesBack(printerDC, commandLineOptions);

    rc = ::EndPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "EndPage(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
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

    if (commandLineOptions.magstripe) {
        WriteMagstripeEscapes(printerDC);
    }

    CString text;
    text.Format(L"%s front on", util::GetShortExeName());
    rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "TextOut(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

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

    WriteCustomTopcoatBlockingEscapesFront(printerDC, commandLineOptions);

    rc = ::EndPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "EndPage(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }
}

int CreatePrintJob(
    const HANDLE                printerHandle,
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
    docInfo.lpszDocName = L"XPS Driver SDK print sample document";
    int rc = ::StartDoc(printerDC, &docInfo);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        exceptionText << "StartDoc(): " << " 0x" << hex << lastErr << " " << CW2A(util::Win32ErrorString(lastErr));
        throw runtime_error(exceptionText.str());
    }

    const DWORD windowsJobID = rc;

    DrawCardFront(printerDC, commandLineOptions);

    if (commandLineOptions.twoPages) {
        DrawCardBack(printerHandle, printerDC, commandLineOptions, devmode);
    }

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
        exceptionText << "::DPTOpenProvider(" << CW2A(commandLineOptions.printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    // get a printer handle:
    HANDLE printerHandle(0);
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
    vector <byte> devmodeBytes(devmode_size, 0);
    DEVMODE* devmode = (DEVMODE*) (&devmodeBytes[0]);

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
    devmode->dmCopies = commandLineOptions.numCopies;
    devmode->dmDuplex = commandLineOptions.twoPages ? DMDUP_HORIZONTAL : DMDUP_SIMPLEX;
    devmode->dmOrientation = commandLineOptions.portraitFront ? DMORIENT_PORTRAIT : DMORIENT_LANDSCAPE;

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

    ////////////////////////////////////////////////////////////////////////////
    // construct the entire print ticket now, regardless of whether or not we
    // are printing one or two sides.
    ////////////////////////////////////////////////////////////////////////////
    CString printTicketXml = GetPrintTicketXml(commandLineOptions);

    CComPtr <IStream> deltaTicketStream;
    util::CreateIStream(deltaTicketStream);

    ULONG deltaTicketStreamBytesWritten(0);
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

    if (commandLineOptions.showXml) {
        util::RewindIStream(resultTicketStream);
        STATSTG statStorage = {0};

        rc = resultTicketStream->Stat(&statStorage, STATFLAG_DEFAULT);
        if (FAILED(rc)) {
            exceptionText << "resultTicketStream->Stat(): " << rc << CT2A(util::Win32ErrorString(rc));
            throw runtime_error(exceptionText.str());
        }

        const int ticketSize = statStorage.cbSize.LowPart;

        vector <byte> printTicketBytes(ticketSize, 0);
        ULONG bytesWritten(0);
        rc = resultTicketStream->Read(&printTicketBytes[0], ticketSize, &bytesWritten);
        if (FAILED(rc)) {
            exceptionText << "resultTicketStream->Write(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
            throw runtime_error(exceptionText.str());
        }

        CString printTicketXmlData((char*) printTicketBytes.data(), ticketSize);
        cout << endl << CT2A(printTicketXmlData) << endl;
    }

    util::RewindIStream(resultTicketStream);

    // convert the merged print ticket to a DEVMODE. we'll use the
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
    cout << "windows print job " << windowsJobID << " submitted." << endl;

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

CommandLineOptions GetCommandlineOptions(const int argc, _TCHAR* argv[])
{
    CommandLineOptions commandLineOptions;

    int c(0);

    while ((c = getopt(argc, argv, _T("n:r:o:xs:2md:t:u:ci:ef:"))) != EOF) {
        switch (c) {
        case L'n': commandLineOptions.printerName = optarg; break;

        case L'r':
            if (L"front" == CString(optarg).MakeLower()) {
                commandLineOptions.rotateFront = true;
            }
            if (L"back" == CString(optarg).MakeLower()) {
                commandLineOptions.rotateBack = true;
            }
            break;

        case L'o':
            if (L"frontport" == CString(optarg).MakeLower()) {
                commandLineOptions.portraitFront = true;
            }
            if (L"backport" == CString(optarg).MakeLower()) {
                commandLineOptions.portraitBack = true;
            }
            break;

        case L'x': commandLineOptions.showXml = true; break;
        case L's': commandLineOptions.numCopies = (short) ::_wtoi(optarg); break;
        case L'2': commandLineOptions.twoPages = true; break;
        case L'm': commandLineOptions.magstripe = true; break;

        case L'd':
            if (L"all" == CString(optarg).MakeLower()) {
                commandLineOptions.disablePrinting = CommandLineOptions::All;   break;
            }
            if (L"off" == CString(optarg).MakeLower()) {
                commandLineOptions.disablePrinting = CommandLineOptions::Off;   break;
            }
            if (L"front" == CString(optarg).MakeLower()) {
                commandLineOptions.disablePrinting = CommandLineOptions::Front; break;
            }
            if (L"back" == CString(optarg).MakeLower()) {
                commandLineOptions.disablePrinting = CommandLineOptions::Back;  break;
            }
            usage();

        case L't': commandLineOptions.topcoatBlockingFront = CString(optarg).MakeLower(); break;
        case L'u': commandLineOptions.topcoatBlockingBack = CString(optarg).MakeLower(); break;
        case L'c': commandLineOptions.jobCompletion = true; break;
        case L'i': commandLineOptions.hopperID = CString(optarg).MakeLower(); break;
        case L'e': commandLineOptions.checkHopper = true; break;
        case L'f': commandLineOptions.cardEjectSide = CString(optarg).MakeLower(); break;
        default: usage();
        }
    }

    return commandLineOptions;
}

CString GetHopperStatus(
    CComPtr <IBidiSpl>& bidiSpl,
    CString hopperId)
{
    CString hopperStatusXml = util::GetHopperStatusXML(bidiSpl);
    CString hopperStatus = util::ParseHopperStatusXML(hopperStatusXml, hopperId);
    return hopperStatus;    
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    CommandLineOptions commandLineOptions = GetCommandlineOptions(argc, argv);
    commandLineOptions.Validate();
    bool    checkHopperStatus = commandLineOptions.hopperID.GetLength() > 0 && commandLineOptions.checkHopper;

    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

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
            exceptionText << "BindDevice() error " << " 0x" << hex << hr << " '" << CT2A(commandLineOptions.printerName) << "' " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        const util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
        if ("Ready" == printerOptionsValues._printerStatus ||
            "Busy" == printerOptionsValues._printerStatus) {
            // the printer can accept new jobs.
        }
        else {
            exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " is not ready.";
            throw runtime_error(exceptionText.str());
        }

        if ("Installed" != printerOptionsValues._printHead) {
            exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " does not have a print head installed.";
            throw runtime_error(exceptionText.str());
        }

        if (commandLineOptions.magstripe && (printerOptionsValues._optionMagstripe.Find(CString("ISO")) == -1)) {
            exceptionText << "printer " << CT2A(commandLineOptions.printerName) << " does not have an ISO magnetic stripe unit installed.";
            throw runtime_error(exceptionText.str());
        }

        if (checkHopperStatus) {
            CString hopperStatus = GetHopperStatus(bidiSpl, commandLineOptions.hopperID);
            if (hopperStatus.CompareNoCase(L"Empty") == 0) {
                exceptionText << "Hopper '" << CW2A(commandLineOptions.hopperID) << "' is empty." << endl;
                throw runtime_error(exceptionText.str());
            }
            cout << "Status of hopper '" << CW2A(commandLineOptions.hopperID) << "': " << CW2A(hopperStatus) << "." << endl;
        }

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