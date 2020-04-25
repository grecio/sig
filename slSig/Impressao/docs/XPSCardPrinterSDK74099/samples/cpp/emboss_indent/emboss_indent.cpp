////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer Driver SDK: emboss + indent + hopper selection c++ sample.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <bidispl.h>
#include <WinSpool.h>
#include <iostream>
#include <sstream>
#include <prntvpt.h>
#include "XGetopt.h"
#include "printTicketXml.h"
#include "util.h"

using namespace std;

void usage()
{
    const string shortExeName = CT2A(util::GetShortExeName());
    cout << endl << shortExeName << " demonstrates the emboss and indent escapes, input hopper" << endl;
    cout << "selection, and disabling topping option." << endl << endl;
    cout << "Emboss and indent data is hardcoded. It can also display print ticket data " << endl << endl;
    cout << "and poll printer for job completion. " << endl << endl;
    cout << shortExeName << " -n <printername> [-i <input hopper>] [-d] [-x] [-c]" << endl << endl;
    cout << "options:" << endl;
    cout << "  " << "-n <printername>. Required. Try -n \"XPS Card Printer\"." << endl;
    cout << "  " << "-i <input hopper>. Optional. Defaults to \"1\"." << endl;
    cout << "  " << "-d Disable topping." << endl;
	cout << "  " << "-x Display the print ticket data. Default is no display." << endl;
	cout << "  " << "-c Poll for job completion." << endl;
    exit(0);
}


////////////////////////////////////////////////////////////////////////////////
// GetPrintTicketXml()
// Generate PrintTicket xml to turn disableTopping on or off
////////////////////////////////////////////////////////////////////////////////
CString GetPrintTicketXml(bool disableTopping)
{
	CString xml = PrintTicketXml::Prefix;

	if (disableTopping)
	{
		xml += PrintTicketXml::ToppingOff;
	}
	else
	{
		xml += PrintTicketXml::ToppingOn;
	}

	xml += PrintTicketXml::Suffix;
	return xml;
}


////////////////////////////////////////////////////////////////////////////////
// ProcessPrintTicket()
// Generate a DEVMODE that sets the topping accordingly.
////////////////////////////////////////////////////////////////////////////////
DEVMODE* ProcessPrintTicket(CString printerName, bool disableTopping, bool displayPrintTicket)
{
	stringstream exceptionText;
	HANDLE printerHandle(0);

	// for the given printer get the maxSchemaVersion needed for the
	// PTOpenProvider() call:
	DWORD maxSchemaVersion(0);
	int rc = ::PTQuerySchemaVersionSupport(printerName, &maxSchemaVersion);
	if (FAILED(rc)) {
		const DWORD lastError = ::GetLastError();
		exceptionText << "::PTQuerySchemaVersionSupport("
			<< CW2A(printerName)
			<< ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
		throw runtime_error(exceptionText.str());
	}

	// get the handle to the printticket printTicketProviderHandle.
	HPTPROVIDER printTicketProviderHandle(0);
	rc = ::PTOpenProvider(printerName, maxSchemaVersion, &printTicketProviderHandle);
	if (FAILED(rc)) {
		const DWORD lastError = ::GetLastError();
		exceptionText << "::DPTOpenProvider(" << CW2A(printerName)
			<< ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
		throw runtime_error(exceptionText.str());
	}

	// get a printer handle:
	rc = ::OpenPrinter(CT2W(printerName), &printerHandle, NULL);
	if (!rc) {
		const DWORD lastError = ::GetLastError();
		exceptionText << "::OpenPrinter(" << CW2A(printerName)
			<< ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
		throw runtime_error(exceptionText.str());
	}

	// get a DEVMODE. we need to get the buffer size first.
	rc = ::DocumentProperties(
		0,
		printerHandle,
		CT2W(printerName),
		NULL,
		NULL,
		0);
	if (0 > rc) {
		const DWORD lastError = ::GetLastError();
		exceptionText << ":::DocumentProperties(" << CW2A(printerName)
			<< ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
		throw runtime_error(exceptionText.str());
	}

	ULONG devmode_size = rc;

	// this buffer will nicely self-destruct:
	vector <byte> devmodeBytes(devmode_size, 0);
	DEVMODE* devmode = (DEVMODE*)(&devmodeBytes[0]);

	rc = ::DocumentProperties(
		NULL,
		printerHandle,
		CT2W(printerName),
		devmode,
		NULL,
		DM_OUT_BUFFER);
	if (rc < 0) {
		const DWORD lastError = ::GetLastError();
		exceptionText << ":::DocumentProperties(" << CW2A(printerName)
			<< ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
		throw runtime_error(exceptionText.str());
	}

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

	CString printTicketXml = GetPrintTicketXml(disableTopping);

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

	// If the -x switch was specified, display the PrintTicket XML
	if (displayPrintTicket) {
		util::RewindIStream(resultTicketStream);
		STATSTG statStorage = { 0 };

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

		CString printTicketXmlData((char*)printTicketBytes.data(), ticketSize);
		cout << endl << CT2A(printTicketXmlData) << endl;
	}

	util::RewindIStream(resultTicketStream);

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

	return devmode;
}

////////////////////////////////////////////////////////////////////////////////
// SendEmbossIndentData()
// use escapes in a windows print job document to emboss and indent a card.
////////////////////////////////////////////////////////////////////////////////
void SendEmbossIndentData(CString printerName, DEVMODE* devmode)
{
	stringstream exceptionText;
	HANDLE printerHandle(0);
	HDC printerDC(0);

    try {
        int rc = ::OpenPrinter(printerName.GetBuffer(), &printerHandle, NULL);
        if (!rc) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "OpenPrinter():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        printerDC = ::CreateDC(L"WINSPOOL", printerName, NULL, devmode);
        if (NULL == printerDC) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "CreateDC():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        ::DOCINFO docInfo = {0};
        docInfo.cbSize = sizeof DOCINFO;
        docInfo.lpszDocName = L"emboss indent job";
        rc = ::StartDoc(printerDC, &docInfo);
        if (rc < 0) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "StartDoc():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        const DWORD windowsJobID = rc;

        rc = ::StartPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "StartPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        CString text;

        // emit all emboss + indent data driver escape strings:
        {
            text = L"~EM%1;301;860;Font 11111";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%2;1600;860;222222";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%3;301;1460;333333";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%4;301;1180;444444";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%5;301;690;555555";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%6;1600;690;666666";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%7;301;650;777777";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%8;301;1000;888888";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%9;301;1050;999999";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
            text = L"~EM%10;1600;1050;10 10 10";
            rc = ::TextOut(printerDC, 50, 50, text, text.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                exceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw runtime_error(exceptionText.str());
            }
        }

        rc = ::EndPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "EndPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        rc = ::EndDoc(printerDC);
        if (rc < 0) {
            const DWORD lastErr = ::GetLastError();
            exceptionText << "EndDoc():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(exceptionText.str());
        }

        util::WaitUntilJobSpooled(printerHandle, windowsJobID);
    }
    catch (runtime_error&) {
        if (0 != printerDC) {
            ::DeleteDC(printerDC);
        }
        if (0 != printerHandle) {
            ::ClosePrinter(printerHandle);
        }
        throw;
    }

    if (0 != printerDC) {
        ::DeleteDC(printerDC);
    }

    if (0 != printerHandle) {
        ::ClosePrinter(printerHandle);
    }
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


////////////////////////////////////////////////////////////////////////////////
// main()
////////////////////////////////////////////////////////////////////////////////
int _tmain(int argc, _TCHAR* argv[])
{
    stringstream exceptionText;
    CString printerName;
    CString commandLineHopperID = L"";
    bool pollForJobCompletion(false);
	bool disableTopping(false);
	bool displayPrintTicket(false);
    int c(0);
    CString commandLineCardEjectSide = L"";

    while ((c = getopt(argc, argv, _T("n:cxdi:"))) != EOF) {
        switch (c) {
        case L'n': printerName = optarg; break;
        case L'c': pollForJobCompletion = true; break;
		case L'd': disableTopping = true; break;
		case L'x': displayPrintTicket = true; break;
        case L'i':
            commandLineHopperID = CString(optarg).MakeLower();
            ValidateHopperID(commandLineHopperID);
            break;
        default:
            usage();
        }
    }

    if (printerName.IsEmpty()) {
        cout << "printername required." << endl;
        usage();
    }


    CComPtr <IBidiSpl> bidiSpl;
    int printerJobID(0);

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

        hr = bidiSpl->BindDevice(printerName, BIDI_ACCESS_USER);
        if (FAILED(hr)) {
            exceptionText << "BindDevice():" << " 0x" << hex << hr << " " << CT2A(printerName) << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        const CString driverVersionXml = util::GetDriverVersionXML(bidiSpl);
        const CString driverVersionString = util::ParseDriverVersionXML(driverVersionXml);
        cout << endl << "driver version: " << CW2A(driverVersionString) << endl << endl;

        // is the printer is in the ready state?
        const CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);

		if ("Installed" != printerOptionsValues._moduleEmbosser) {
            exceptionText << CT2A(printerName) << ": embosser is not present.";
            throw runtime_error(exceptionText.str());
        }

        if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
            exceptionText << "printer " << CT2A(printerName) << " is not ready.";
            throw runtime_error(exceptionText.str());
        }

        CString hopperID = L"1";
        printerJobID = util::StartJob(
            bidiSpl,
            commandLineHopperID.GetLength() > 0 ? commandLineHopperID : hopperID);

        if (0 == printerJobID) {
            exceptionText << "StartJob() failed.";
            throw runtime_error(exceptionText.str());
        }
		
		DEVMODE* devmode = ProcessPrintTicket(printerName, disableTopping, displayPrintTicket);

        SendEmbossIndentData(printerName, devmode);

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