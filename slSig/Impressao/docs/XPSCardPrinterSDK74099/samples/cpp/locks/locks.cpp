////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer Driver SDK: 'locks' c++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <iostream>
#include <sstream>
#include "XGetopt.h"
#include "util.h"
#include "DXP01SDK.H"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl;
    cout << shortExeName << " demonstrates locking, unlocking, changing the lock password, and " << endl;
    cout << "activating or deactivating the printer." << endl << endl;
    cout << shortExeName << " -n <printername> [-a] [-d] [-l] [-u] [-c -w]" << endl << endl;
    cout << "options:" << endl;
    cout << "  " << "-n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  " << "-a <password>. Activate printer using the password." << endl;
    cout << "  " << "-d <password>. Deactivate printer using the password." << endl;
    cout << "  " << "-l <password>. Lock printer using the password." << endl;
    cout << "  " << "-u <password>. Unlock printer using the password." << endl;
    cout << "  " << "-c <current password> -w <new password>. Change lock password." << endl;
    cout << "  " << "   The current and new passwords are required." << endl;
    cout << "  " << "   In this sample, changing the password also locks" << endl;
    cout << "  " << "   the printer." << endl;
    cout << "  " << "   To use a blank password, use an empty string: \"\"" << endl;
    cout << "  " << "-a and -d options cannot be used with -l, -u or -c options" << endl;
    cout << "  " << "-l, -u or -c option only applies to the printers that have lock option installed." << endl;
    cout << endl;
    cout << "  password rules:" << endl;
    cout << "    blank password (\"\") is OK;" << endl;
    cout << "    four or more valid characters;" << endl;
    cout << "    legal characters:" << endl;
    cout << "      A through Z; a through z; 0 through 9; '+', '/', and '$'." << endl;
    cout << endl;
    exit(0);
}

struct CommandLineOptions {
    bool     change;
    bool     lock;
    bool     unlock;

    bool     activate;
    bool     deactivate;

    CString  currentPassword;
    CString  newPassword;
    CString  printerName;

    CommandLineOptions() : change(false), lock(false), unlock(false), activate(false), deactivate(false)
    {}

    void Validate()
    {
        if (printerName.IsEmpty()) {
            usage();
        }

        if (activate || deactivate) {

            // -a and -d options cannot be used with locking/unlocking options
            if (lock || unlock || change) {
                usage();
            }

            // both -a and -d options cannot be selected at the same time
            if (activate && deactivate) {
                usage();
            }
        }
        else {
            if (!lock && !unlock && !change) {
                usage();
            }
        }
    }
};

util::PrinterStatusValues ActivateOrDisablePrinter(
    CComPtr<IBidiSpl>&         bidiSpl,
    CString  password,
    bool activate)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }
    CString formattedPrinterActivateXML;
    formattedPrinterActivateXML.Format(
        dxp01sdk::ACTIVATE_PRINTER_XML,
        activate,
        password);

    const int xmlBytesLength = formattedPrinterActivateXML.GetLength() * sizeof WCHAR;

    // allocate IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, formattedPrinterActivateXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::ACTIVATE_PRINTER);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::ACTIVATE_PRINTER): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
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

    HRESULT ActivateJobResultCode(S_OK);
    hr = bidiRequest->GetResult(&ActivateJobResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(ActivateJobResultCode):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(ActivateJobResultCode)) {
        exceptionText << "ActivateJobResultCode:" << " 0x" << hex << ActivateJobResultCode << " " << CT2A(util::Win32ErrorString(ActivateJobResultCode));
        throw runtime_error(exceptionText.str());
    }

    return printerStatusValues;

}

util::PrinterStatusValues LockOrUnlock(
    CComPtr<IBidiSpl>&         bidiSpl,
    const CommandLineOptions&  options)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    const short lockValue = options.lock ? 1 : 0;
    CString formattedPrinterLockXML;
    formattedPrinterLockXML.Format(
        dxp01sdk::LOCK_PRINTER_XML,
        lockValue,
        options.currentPassword);

    const int xmlBytesLength = formattedPrinterLockXML.GetLength() * sizeof WCHAR;

    // allocate IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, formattedPrinterLockXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::LOCK_PRINTER);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::LOCK_PRINTER): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
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
        exceptionText << "LockOrUnlock() failed. Status XML:\n" << CT2A(printerStatusXML);
        throw runtime_error(exceptionText.str());
    }

    HRESULT lockJobResultCode(S_OK);
    hr = bidiRequest->GetResult(&lockJobResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(lockJobResultCode):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(lockJobResultCode)) {
        exceptionText << "lockJobResultCode:" << " 0x" << hex << lockJobResultCode << " " << CT2A(util::Win32ErrorString(lockJobResultCode));
        throw runtime_error(exceptionText.str());
    }

    return printerStatusValues;
}

util::PrinterStatusValues ChangeLockPassword(
    CComPtr<IBidiSpl>&         bidiSpl,
    const CommandLineOptions&  options)
{
    stringstream exceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        exceptionText << "CoCreateInstance(CLSID_BidiRequest): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    // in this sample, we always lock the printer as part of the password change
    // operation. (Use zero to unlock the printer.)
    const short lockValue = 1;

    CString formattedChangePasswordXML;
    formattedChangePasswordXML.Format(
        dxp01sdk::CHANGE_LOCK_PASSWORD_XML,
        lockValue,
        options.currentPassword,
        options.newPassword);

    const int xmlBytesLength = formattedChangePasswordXML.GetLength() * sizeof WCHAR;

    // allocate IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE xmlBytes = (PBYTE) XMLBytesAllocator.Allocate(xmlBytesLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, formattedChangePasswordXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::CHANGE_LOCK_PASSWORD);
    if (FAILED(hr)) {
        exceptionText << "SetSchema(dxp01sdk::CHANGE_LOCK_PASSWORD): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
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
        exceptionText << "ChangeLockPassword() failed. Status XML:\n" << CT2A(printerStatusXML);
        throw runtime_error(exceptionText.str());
    }

    HRESULT lockJobResultCode(S_OK);
    hr = bidiRequest->GetResult(&lockJobResultCode);
    if (FAILED(hr)) {
        exceptionText << "GetResult(lockJobResultCode):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(exceptionText.str());
    }

    if (FAILED(lockJobResultCode)) {
        exceptionText << "lockJobResultCode:" << " 0x" << hex << lockJobResultCode << " " << CT2A(util::Win32ErrorString(lockJobResultCode));
        throw runtime_error(exceptionText.str());
    }

    return printerStatusValues;
}

////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    stringstream exceptionText;
    CommandLineOptions options;

    int c(0);
    while ((c = getopt(argc, argv, _T("c:l:w:n:u:a:d:"))) != EOF) {
        switch (c) {
        case L'c': options.currentPassword = optarg; options.change = true; break;
        case L'l': options.currentPassword = optarg; options.lock = true; break;
        case L'w': options.newPassword = optarg; break;
        case L'n': options.printerName = optarg; break;
        case L'u': options.currentPassword = optarg; options.unlock = true; break;
        case L'a': options.currentPassword = optarg; options.activate = true; break;
        case L'd': options.currentPassword = optarg; options.deactivate = true; break;
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

        if (options.lock || options.unlock || options.change) {
            // get the printer options. See if we can lock/unlock this printer.
            const CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
            const util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);
            if (printerOptionsValues._optionLocks != TEXT("Installed")) {
                exceptionText << "this printer does not support the locking feature.";
                throw runtime_error(exceptionText.str());
            }
        }

        if (options.activate || options.deactivate) {
            ActivateOrDisablePrinter(bidiSpl, options.currentPassword, options.activate);
            if (options.activate) {
                cout << endl << "activated the printer." << endl;
            }
            else {
                cout << endl << "deactivated the printer." << endl;

            }
        }
        else if (options.lock || options.unlock) {
            LockOrUnlock(bidiSpl, options);
            if (options.lock) {
                cout << endl << "locked the printer." << endl;
            }
            else {
                cout << endl << "unlocked the printer." << endl;
            }
        }
        else if (options.change) {
            ChangeLockPassword(bidiSpl, options);
            cout << endl << "password changed." << endl;
        }

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