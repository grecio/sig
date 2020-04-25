////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// common 'util' methods are used throughout the xps driver sdk samples
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include <sstream>
#include <atlenc.h>
#include <atltime.h>
#include <atlpath.h>
#include "DXP01SDK.H"
#include "util.h"

using namespace std;

////////////////////////////////////////////////////////////////////////////////
// util::Win32ErrorString()
// for the given Win32 error code, return a unicode string
////////////////////////////////////////////////////////////////////////////////
CString util::Win32ErrorString(const long errorCode)
{
    CString errorString;
    const DWORD bufsize(512);
    ::FormatMessage(
        FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_FROM_SYSTEM,
        NULL,
        errorCode,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL),
        errorString.GetBuffer(bufsize),
        bufsize,
        NULL);
    errorString.ReleaseBuffer();
    errorString.Trim();
    return errorString;
}

////////////////////////////////////////////////////////////////////////////////
// util::EchoCurrentTime()
////////////////////////////////////////////////////////////////////////////////
void util::EchoCurrentTime(CString msg)
{
    const CTime current_time(CTime::GetCurrentTime());
    const CString current_time_formatted(current_time.Format(TEXT("%Y/%m/%d %H:%M:%S")));
    cout << CT2A(current_time_formatted) << " " << CT2A(msg) << endl;
}

////////////////////////////////////////////////////////////////////////////////
// util::StringFromXml()
//
// for the given XML document, return a string value using the given xpath
// expression.
//
// if the string is not found, send a msg to the debug output, and return an
// empty string.
////////////////////////////////////////////////////////////////////////////////
CString util::StringFromXml(
    CComPtr <IXMLDOMDocument2>& doc,
    const PWCHAR                xpathExpression)
{
    CString val;
    CComPtr <IXMLDOMNode> node;
    stringstream utilExceptionText;

    HRESULT hr = doc->selectSingleNode(xpathExpression, &node);
    if (FAILED(hr) || S_FALSE == hr) {
        utilExceptionText << "doc->selectSingleNode() " << CW2A(xpathExpression)
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr)) << endl;
        AtlTrace("%s\n", utilExceptionText.str().c_str());
        return val;
    }

    CComBSTR nodeText;
    hr = node->get_text(&nodeText);
    if (FAILED(hr)) {
        utilExceptionText << "node->get_text() " << CW2A(xpathExpression)
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        AtlTrace("%s\n", utilExceptionText.str().c_str());
        return val;
    }

    val = nodeText;

    return val;
}

////////////////////////////////////////////////////////////////////////////////
// util::IntFromXml()
// return a numeric value from the xml doc using the given xpath expression
// return zero if not found.
////////////////////////////////////////////////////////////////////////////////
int util::IntFromXml(CComPtr <IXMLDOMDocument2>& doc, const PWCHAR xpathExpression)
{
    int val(0);
    CComPtr <IXMLDOMNode> node;
    stringstream utilExceptionText;

    HRESULT hr = doc->selectSingleNode(xpathExpression, &node);
    if (FAILED(hr) || S_FALSE == hr) {
        utilExceptionText << "doc->selectSingleNode() " << CW2A(xpathExpression)
            << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr)) << endl;
        AtlTrace("%s\n", utilExceptionText.str().c_str());
        return val;
    }

    CComBSTR nodeText;
    hr = node->get_text(&nodeText);
    if (FAILED(hr)) {
        utilExceptionText << "node->get_text() " << CW2A(xpathExpression)
            << " 0x" << hex << hr << " "
            << CT2A(util::Win32ErrorString(hr))
            << endl;
        AtlTrace("%s", utilExceptionText.str());
        return val;
    }

    val = ::_wtoi(nodeText);

    return val;
}

////////////////////////////////////////////////////////////////////////////////
// util::GetPrinterStatusXML()
//
// Most commands are followed with a call to this function. It returns xml
// markup that contains the most recent state of the printer.
//
// This version of
// GetPrinterStatusXML() is uses the most recently issued IBidiRequest.
////////////////////////////////////////////////////////////////////////////////
CString util::GetPrinterStatusXML(CComPtr <IBidiRequest>& bidiRequest)
{
    stringstream utilExceptionText;
    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    HRESULT hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR printerStatusString;
    hr = printerStatusString.AppendBytes((PCHAR) dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(printerStatusString);
}

////////////////////////////////////////////////////////////////////////////////
// util::GetPrinterStatusXML()
//
// Most commands are followed with a call to this function. It returns xml
// markup that contains the most recent state of the printer.
//
// This version of GetPrinterStatusXML() issues a dxp01sdk::PRINTER_MESSAGES
// request to get printer status xml.
////////////////////////////////////////////////////////////////////////////////
CString util::GetPrinterStatusXML(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream utilExceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_MESSAGES);
    if (FAILED(hr)) {
        utilExceptionText << "SetSchema(dxp01sdk::PRINTER_MESSAGES): " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "SendRecv(BIDI_ACTION_GET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    HRESULT printerStatusHresult(S_OK);
    hr = bidiRequest->GetResult(&printerStatusHresult);
    if (FAILED(hr)) {
        utilExceptionText << "GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR printerMessagesXml;
    hr = printerMessagesXml.AppendBytes((PCHAR) dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(printerMessagesXml);
}

////////////////////////////////////////////////////////////////////////////////
// util::ParsePrinterStatusXML()
//
// for the given Printer Status XML string, populate and return a
// PrinterStatusValues data structure.
//
// throw a std::runtime_error if any errors encountered.
////////////////////////////////////////////////////////////////////////////////
util::PrinterStatusValues util::ParsePrinterStatusXML(const CString printerStatusXML)
{
    // example Printer Status XML from printer:
    //
    // <?xml version="1.0" ?>
    // <!-- Printer status xml file. -->
    // <PrinterStatus>
    //   <ClientID>fellmadWin7x64_{545CF2E2-D68A-44B8-B83F-0DB07F21124F}</ClientID>
    //   <WindowsJobID>0</WindowsJobID>
    //   <PrinterJobID>8842</PrinterJobID>
    //   <ErrorCode>0</ErrorCode>
    //   <ErrorSeverity>0</ErrorSeverity>
    //   <ErrorString />
    //   <DataFromPrinter>
    //     <![CDATA[]]>
    //   </DataFromPrinter>
    // </PrinterStatus>
    //
    // we populate a PrinterStatusValues struct with the values from this XML.
    // we use xpath to retrieve things.

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(printerStatusXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty(SelectionLanguage):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    PrinterStatusValues printerStatusValues;

    printerStatusValues._clientID = StringFromXml(doc, L"PrinterStatus/ClientID");
    printerStatusValues._errorCode = IntFromXml(doc, L"PrinterStatus/ErrorCode");
    printerStatusValues._errorSeverity = IntFromXml(doc, L"PrinterStatus/ErrorSeverity");
    printerStatusValues._errorString = StringFromXml(doc, L"PrinterStatus/ErrorString");
    printerStatusValues._printerData = StringFromXml(doc, L"PrinterStatus/DataFromPrinter");
    printerStatusValues._printerJobID = IntFromXml(doc, L"PrinterStatus/PrinterJobID");
    printerStatusValues._windowsJobID = IntFromXml(doc, L"PrinterStatus/WindowsJobID");

    return printerStatusValues;
}

// Helper that allocates the BSTR param for the caller.
HRESULT CreateElement(
    IXMLDOMDocument2*   pXMLDom,
    CComBSTR            name,
    IXMLDOMElement**    ppElement)
{
    stringstream utilExceptionText;
    *ppElement = nullptr;

    HRESULT hr = pXMLDom->createElement(name, ppElement);
    
    if (FAILED(hr)) {
        utilExceptionText << "pXMLDom->createElement(%ls) %!hresult!" << CT2A(name) << hr;
        throw runtime_error(utilExceptionText.str());
    }

    return hr;
}


// Helper function to append a child to a parent node.
HRESULT AppendChildToParent(IXMLDOMNode *pChild, IXMLDOMNode *pParent)
{
    CComPtr <IXMLDOMNode> pChildOut;
    HRESULT hr = pParent->appendChild(pChild, &pChildOut);
    if (FAILED(hr)) {
        stringstream utilExceptionText;
        utilExceptionText << "pParent->appendChild() %!hresult!" << hr;
        throw runtime_error(utilExceptionText.str());
    }
    return hr;
}



// Helper function to create and append a text node to a parent node.
HRESULT CreateAndAddTextNode(
    IXMLDOMDocument2*   pDom,
    CComBSTR            text,
    IXMLDOMNode*        pParent)
{
    stringstream utilExceptionText;
    HRESULT hr = S_OK;
    CComPtr <IXMLDOMText> pText;

    hr = pDom->createTextNode(text, &pText);
    if (FAILED(hr)) {
        utilExceptionText << "pDom->createTextNode failed";
        throw runtime_error(utilExceptionText.str());
    }

    hr = AppendChildToParent(pText, pParent);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent failed";
        throw runtime_error(utilExceptionText.str());
    }

    return hr;
}

// Helper function to create and append a CDATA node to a parent node.
HRESULT CreateAndAddCDATANode(
    IXMLDOMDocument*  pDom,
    CComBSTR          data,
    IXMLDOMNode*      pParent)
{
    stringstream utilExceptionText;
    HRESULT hr = S_OK;
    CComPtr <IXMLDOMCDATASection> pCDATA;

    hr = pDom->createCDATASection(data, &pCDATA);
    if (FAILED(hr)) {
        utilExceptionText << "createCDATASection failed";
        throw runtime_error(utilExceptionText.str());
    }

    hr = AppendChildToParent(pCDATA, pParent);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent failed";
        throw runtime_error(utilExceptionText.str());
    }

    return hr;
}

////////////////////////////////////////////////////////////////////////////////
// CreateAndAddElementNode()
//
// Helper function to create and append an element node to a parent node, and
// pass the newly created element node to caller if it wants.
////////////////////////////////////////////////////////////////////////////////
HRESULT CreateAndAddElementNode(
    IXMLDOMDocument2* pDom,
    CComBSTR          wszName,
    CComBSTR          wszNewline,
    IXMLDOMNode*      pParent,
    IXMLDOMElement**  ppElement)
{
    stringstream utilExceptionText;
    HRESULT hr = S_OK;
    IXMLDOMElement* pElement = nullptr;

    hr = CreateElement(pDom, wszName, &pElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement failed";
        throw runtime_error(utilExceptionText.str());
    }

    // Add NEWLINE+TAB for identation before this element.
    hr = CreateAndAddTextNode(pDom, wszNewline, pParent);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode failed";
        throw runtime_error(utilExceptionText.str());
    }

    // Append this element to parent.
    hr = AppendChildToParent(pElement, pParent);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent failed";
        throw runtime_error(utilExceptionText.str());
    }

    if (ppElement) {
        *ppElement = pElement;  // Caller is repsonsible to release this element.
    }
    else {
        hr = pElement->Release();
        if (FAILED(hr)) {
            utilExceptionText << "pElement->Release failed";
            throw runtime_error(utilExceptionText.str());
        }
    }

    if (ppElement) {
        *ppElement = pElement;  // Caller is responsible to release this element.
    }
    else {
        pElement->Release();  // Caller is not interested on this element, so release it.
        pElement = nullptr;
    }

    return hr;
}

CStringA util::CreateLaserEngraveTextXML(__in const CString elementName, __in const CString elementValue)
{
    CComBSTR bstrStr;

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMElement> rootElement;
    hr = CreateElement(doc, L"LaserEngraveText", &rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create the nodes of the <root> element:
    CComPtr <IXMLDOMElement> nodeElement;

    hr = CreateAndAddElementNode(doc, L"ElementName", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = elementName.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    hr = CreateAndAddElementNode(doc, L"ElementValue", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = elementValue.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    // Add NEWLINE for indentation before </root>.
    hr = CreateAndAddTextNode(doc, L"\n", rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    };

    // add <root> to document
    hr = AppendChildToParent(rootElement, doc);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR xml;
    hr = doc->get_xml(&xml);
    if (FAILED(hr)) {
        utilExceptionText << "get_xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    return CStringA(xml.m_str);
}

CStringA util::CreateLaserFileNameXML(__in const CString laserFileName)
{
    CComBSTR bstrStr;

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMElement> rootElement;
    hr = CreateElement(doc, L"LaserSetup", &rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create the nodes of the <root> element:
    CComPtr <IXMLDOMElement> nodeElement;

    hr = CreateAndAddElementNode(doc, L"FileName", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = laserFileName.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    // Add NEWLINE for indentation before </root>.
    hr = CreateAndAddTextNode(doc, L"\n", rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    };

    // add <root> to document
    hr = AppendChildToParent(rootElement, doc);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR xml;
    hr = doc->get_xml(&xml);
    if (FAILED(hr)) {
        utilExceptionText << "get_xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    return CStringA(xml.m_str);
}


CStringA util::CreateLaserSetupFileNameXML(__in const CString laserSetUpFileName, __in const int count)
{
    CComBSTR bstrStr;

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMElement> rootElement;
    hr = CreateElement(doc, L"LaserEngraveSetupFileName", &rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create the nodes of the <root> element:
    CComPtr <IXMLDOMElement> nodeElement;

    hr = CreateAndAddElementNode(doc, L"FileName", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = laserSetUpFileName.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    hr = CreateAndAddElementNode(doc, L"ElementCount", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    CStringW ElementCount;
    ElementCount.Format(L"%d", count);
    bstrStr = ElementCount;
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    nodeElement = nullptr;

    // Add NEWLINE for indentation before </root>.
    hr = CreateAndAddTextNode(doc, L"\n", rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    };

    // add <root> to document
    hr = AppendChildToParent(rootElement, doc);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR xml;
    hr = doc->get_xml(&xml);
    if (FAILED(hr)) {
        utilExceptionText << "get_xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    return CStringA(xml.m_str);
}


CStringA util::CreateImportZipFileXML(__in const CString laserZipFileName, bool overwrite, __in std::vector <char>& base64EncodedChars)
{
    CComBSTR bstrStr;

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMElement> rootElement;
    hr = CreateElement(doc, L"LaserZipFile", &rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create the nodes of the <root> element:
    CComPtr <IXMLDOMElement> nodeElement;

    hr = CreateAndAddElementNode(doc, L"FileName", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = laserZipFileName.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    hr = CreateAndAddElementNode(doc, L"Overwrite", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    CStringW overWrite;
    overWrite.Format(L"%d", overwrite);
    bstrStr = overWrite;
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    nodeElement = nullptr;

    // Create an element to hold a CDATA section.
    hr = CreateAndAddElementNode(doc, L"FileContents", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    CComBSTR ccombstr(&base64EncodedChars[0]);    // Convert CStringA to BSTR
    hr = CreateAndAddCDATANode(doc, ccombstr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddCDATANode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    // Add NEWLINE for indentation before </root>.
    hr = CreateAndAddTextNode(doc, L"\n", rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    };

    // add <root> to document
    hr = AppendChildToParent(rootElement, doc);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR xml;
    hr = doc->get_xml(&xml);
    if (FAILED(hr)) {
        utilExceptionText << "get_xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    return CStringA(xml.m_str);
}

CStringA util::CreateLaserEngraveBinaryXML(__in const CString elementName, __in std::vector <char>& base64EncodedChars)
{
    CComBSTR bstrStr;

    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMElement> rootElement;
    hr = CreateElement(doc, L"LaserEngraveBinary", &rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateElement:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create the nodes of the <root> element:
    CComPtr <IXMLDOMElement> nodeElement;

    hr = CreateAndAddElementNode(doc, L"ElementName", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    bstrStr = elementName.GetString();
    hr = CreateAndAddTextNode(doc, bstrStr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    // Create an element to hold a CDATA section.
    hr = CreateAndAddElementNode(doc, L"ElementValue", L"\n\t", rootElement, &nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddElementNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    CComBSTR ccombstr(&base64EncodedChars[0]);    // Convert CStringA to BSTR
    hr = CreateAndAddCDATANode(doc, ccombstr, nodeElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddCDATANode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    nodeElement = nullptr;

    // Add NEWLINE for indentation before </root>.
    hr = CreateAndAddTextNode(doc, L"\n", rootElement);
    if (FAILED(hr)) {
        utilExceptionText << "CreateAndAddTextNode:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    };

    // add <root> to document
    hr = AppendChildToParent(rootElement, doc);
    if (FAILED(hr)) {
        utilExceptionText << "AppendChildToParent():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR xml;
    hr = doc->get_xml(&xml);
    if (FAILED(hr)) {
        utilExceptionText << "get_xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    return CStringA(xml.m_str);
}
////////////////////////////////////////////////////////////////////////////////
// util::ParseLaserStatusXML()
//
// for the given Laser Status XML string, populate and return a
// LaserStatusValues data structure.
//
// throw a std::runtime_error if any errors encountered.
////////////////////////////////////////////////////////////////////////////////
util::LaserStatusValues util::ParseLaserStatusXML(const CString laserStatusXML)
{
    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(laserStatusXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty(SelectionLanguage):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LaserStatusValues laserStatusValues;

    laserStatusValues._success = IntFromXml(doc, L"LaserResponse/Status");
    laserStatusValues._base64Data = StringFromXml(doc, L"LaserResponse/Base64Data");
   
    return laserStatusValues;
}


CString util::FormatPrinterActionXML(
    const int actionID,
    const int printerJobID,
    const int errorCode)
{
    CString formattedPrinterActionXML;

    formattedPrinterActionXML.Format(
        dxp01sdk::PRINTER_ACTION_XML,
        actionID,
        printerJobID,
        errorCode);

    return formattedPrinterActionXML;
}

////////////////////////////////////////////////////////////////////////////////
// util::ParseLaserSetupFileList()
//
// we're given a Laser XML fragment that is like this:
//
//
//  <?xml version="1.0"?>
//  <!--Printer status xml file.-->
//  <PrinterStatus>
//     <ClientID>fellmadwin7x64_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
//     <WindowsJobID>0</WindowsJobID>
//     <PrinterJobID>6028</PrinterJobID>
//     <ErrorCode>0</ErrorCode>
//     <ErrorSeverity>0</ErrorSeverity>
//     <ErrorString></ErrorString>
//     <DataFromPrinter>
//         <![CDATA[]]>
//      </DataFromPrinter>
//  </PrinterStatus>
//

////////////////////////////////////////////////////////////////////////////////

void util::ParseLaserSetupFileList(
    const CString laserSetupFileListXML,
    std::vector<CString>& laserSetupFileList)
{

    CComPtr <IXMLDOMDocument2> doc;
    stringstream utilExceptionText;

    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(laserSetupFileListXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty(SelectionLanguage):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }
    

    CComPtr <IXMLDOMNodeList> nodelist;
    hr = doc->selectNodes(L"QuerySetupsResult/LaserCardSetups/LaserCardSetup", &nodelist);
    if (FAILED(hr)) {
        utilExceptionText << "doc->selectNodes(magstripe/track):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    if (nodelist) {
        long laserCardSetupsCount(0);
        hr = nodelist->get_length(&laserCardSetupsCount);
        if (FAILED(hr)) {
            utilExceptionText << "nodelist->get_length():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        for (int laserCardSetupsIndex = 0; laserCardSetupsIndex < laserCardSetupsCount; laserCardSetupsIndex++) {
            CComPtr <IXMLDOMNode> node;
            hr = nodelist->get_item(laserCardSetupsIndex, &node);
            if (FAILED(hr)) {
                utilExceptionText << "nodelist->get_item():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComPtr <IXMLDOMNamedNodeMap> attributeMap;
            hr = node->get_attributes(&attributeMap);
            if (FAILED(hr)) {
                utilExceptionText << "node->get_attributes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComPtr <IXMLDOMNode> attributeNode;
            hr = attributeMap->getNamedItem(L"name", &attributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComBSTR attributeText;
            hr = attributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "attributeNode->get_text():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            laserSetupFileList.push_back(attributeText.m_str);
        }
    }
}
////////////////////////////////////////////////////////////////////////////////
// util::ParseMagstripeStrings()
//
// we're given a Printer Status XML fragment that has a CDATA section like this:
//
// example Printer Status XML from printer in the version 3.0 driver:
//
//  <?xml version="1.0"?>
//  <!--Printer status xml file.-->
//  <PrinterStatus>
//     <ClientID>fellmadwin7x64_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
//     <WindowsJobID>0</WindowsJobID>
//     <PrinterJobID>6028</PrinterJobID>
//     <ErrorCode>0</ErrorCode>
//     <ErrorSeverity>0</ErrorSeverity>
//     <ErrorString></ErrorString>
//     <DataFromPrinter>
//         <![CDATA[<?xml version="1.0" encoding="UTF-8"?>
//         <DPCLMagStripe:MagStripe
//             ...
//             <track number="1">
//                <base64Data>VFJBQ0sx</base64Data>
//             </track>
//             <track number="2">
//                <base64Data>MTEyMg==</base64Data>
//             </track>
//             <track number="3">
//                <base64Data>MzIx</base64Data>
//             </track>
//         </DPCLMagStripe:MagStripe>]]>
//      </DataFromPrinter>
//  </PrinterStatus>
//
// return three strings. the string data is still Base64 encoded.
////////////////////////////////////////////////////////////////////////////////
void util::ParseMagstripeStrings(
    const CString  printerStatusXML,
    CString&       track1,
    CString&       track2,
    CString&       track3,
    bool           bJISRequest)
{
    CComPtr <IXMLDOMDocument2> doc;
    stringstream utilExceptionText;

    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(printerStatusXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty(SelectionLanguage):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // get the CDATA text() node:
    CComPtr <IXMLDOMNode> node;
    hr = doc->selectSingleNode(L"PrinterStatus/DataFromPrinter/text()", &node);
    if (FAILED(hr) || S_FALSE == hr) {
        utilExceptionText << "doc->selectSingleNode():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR cdataText;
    hr = node->get_text(&cdataText);
    if (FAILED(hr)) {
        utilExceptionText << "getting CDATA text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // create a new DOM doc from the CDATA text:
    doc = NULL;
    hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance() for CDATA:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->loadXML(cdataText, &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML() CDATA text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMNodeList> nodelist;
    hr = doc->selectNodes(L"magstripe/track", &nodelist);
    if (FAILED(hr)) {
        utilExceptionText << "doc->selectNodes(magstripe/track):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    if (nodelist) {
        long trackCount(0);
        hr = nodelist->get_length(&trackCount);
        if (FAILED(hr)) {
            utilExceptionText << "nodelist->get_length():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        track1.Empty();
        track2.Empty();
        track3.Empty();

        for (int trackIndex = 0; trackIndex < trackCount; trackIndex++) {
            node = NULL;
            hr = nodelist->get_item(trackIndex, &node);
            if (FAILED(hr)) {
                utilExceptionText << "nodelist->get_item():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComPtr <IXMLDOMNamedNodeMap> attributeMap;
            hr = node->get_attributes(&attributeMap);
            if (FAILED(hr)) {
                utilExceptionText << "node->get_attributes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComPtr <IXMLDOMNode> attributeNode;
            hr = attributeMap->getNamedItem(L"number", &attributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComBSTR attributeText;
            hr = attributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "attributeNode->get_text():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComBSTR trackText;
            hr = node->get_text(&trackText);
            if (FAILED(hr)) {
                utilExceptionText << "node->get_text():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            if (!bJISRequest) {
                if (attributeText == L"1") {
                    track1 = trackText; continue;
                }
                if (attributeText == L"2") {
                    track2 = trackText; continue;
                }
            }

            if (attributeText == L"3") {
                track3 = trackText; continue;
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
// util::GetPrinterOptionsXML()
//
// get the 'printer options' xml from the printer. This is safe to call at any
// time; no print job is needed.
//
// Use ParsePrinterOptionsXML() to obtain a data structure with the values.
////////////////////////////////////////////////////////////////////////////////
CString util::GetPrinterOptionsXML(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream utilExceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_OPTIONS2);
    if (FAILED(hr)) {
        utilExceptionText << "SetSchema(dxp01sdk::PRINTER_OPTIONS2):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "SendRecv(BIDI_ACTION_GET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    HRESULT printerStatusHresult(S_OK);
    hr = bidiRequest->GetResult(&printerStatusHresult);
    if (FAILED(hr)) {
        utilExceptionText << "GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR printerOptionsXML;
    hr = printerOptionsXML.AppendBytes((PCHAR) dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(printerOptionsXML);
}

////////////////////////////////////////////////////////////////////////////////
// util::GetHopperStatusXML()
//
// get the 'hopper status' xml from the printer. This is safe to call at any
// time; no print job is needed.
////////////////////////////////////////////////////////////////////////////////
CString util::GetHopperStatusXML(CComPtr <IBidiSpl>& bidiSpl)
{
    stringstream utilExceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiRequest->SetSchema(dxp01sdk::HOPPER_STATUS);
    if (FAILED(hr)) {
        utilExceptionText << "SetSchema(dxp01sdk::HOPPER_STATUS):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "SendRecv(BIDI_ACTION_GET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    HRESULT printerStatusHresult(S_OK);
    hr = bidiRequest->GetResult(&printerStatusHresult);
    if (FAILED(hr)) {
        utilExceptionText << "GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR hopperStatusXML;
    hr = hopperStatusXML.AppendBytes((PCHAR)dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(hopperStatusXML);
}

////////////////////////////////////////////////////////////////////////////////
//  GetHopperIndex
//  ASSUMPTION: hopperId is valid hopper string, i.e.,
//      it is either "exception" or "1"-"6".
////////////////////////////////////////////////////////////////////////////////
int util::GetHopperIndex(const CString hopperId)
{
    if (hopperId.CompareNoCase(_T("exception")) == 0) return 0;
    return _ttoi(hopperId);
}

////////////////////////////////////////////////////////////////////////////////
//  ParseHopperStatusXML
//
//  Returns the status of the hopper identified in hopperId.
////////////////////////////////////////////////////////////////////////////////
CString util::ParseHopperStatusXML(
    const CString   hopperStatusXML,
    const CString   hopperId)
{
    CComPtr <IXMLDOMDocument2>  doc;
    stringstream                utilExceptionText;
    CString                     hopperStatus;

    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(hopperStatusXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMNodeList> nodelist;
    hr = doc->selectNodes(L"HopperStatus/HopperInformation", &nodelist);
    if (FAILED(hr)) {
        utilExceptionText << "doc->selectNodes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    if (nodelist) {
        long    numHoppers;
        hr = nodelist->get_length(&numHoppers);
        if (FAILED(hr)) {
            utilExceptionText << "nodelist->get_length():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        int hopperIndex = GetHopperIndex(hopperId);
        if (hopperIndex >= numHoppers) {
            utilExceptionText << "Hopper id " << hopperId << " is out of range of available hoppers (" << numHoppers-1 << ").";
            throw runtime_error(utilExceptionText.str());
        }

        CComPtr <IXMLDOMNode> node;

        hr = nodelist->get_item(hopperIndex, &node);
        if (FAILED(hr)) {
            utilExceptionText << "nodelist->get_item():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        CComPtr <IXMLDOMNamedNodeMap> attributeMap;
        hr = node->get_attributes(&attributeMap);
        if (FAILED(hr)) {
            utilExceptionText << "node->get_attributes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        CComPtr <IXMLDOMNode> statusAttributeNode;
        hr = attributeMap->getNamedItem(L"Status", &statusAttributeNode);
        if (FAILED(hr)) {
            utilExceptionText << "attributeMap->getNamedItem():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        CComBSTR attributeText;
        hr = statusAttributeNode->get_text(&attributeText);
        if (FAILED(hr)) {
            utilExceptionText << "statusAttributeNode->get_text():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        hopperStatus = attributeText;
    }

    return hopperStatus;
}

////////////////////////////////////////////////////////////////////////////////
// util::GetPrinterCardCountsXML()
//
// get the 'printer card counts' xml from the printer. This is safe to call at any
// time; no print job is needed.
//
// this function just returns the XML in a string. Use ParsePrinterCardCountsXML()
// to obtain a data structure with the values.
////////////////////////////////////////////////////////////////////////////////
CString util::GetPrinterCardCountsXML(CComPtr <IBidiSpl>& bidiSpl)
{
    return GetPrinterActionReturnXML(bidiSpl, dxp01sdk::COUNTER_STATUS2);
}

CString util::GetDriverVersionXML(CComPtr <IBidiSpl>& bidiSpl)
{
    return GetPrinterActionReturnXML(bidiSpl, dxp01sdk::SDK_VERSION);
}

CString util::GetPrinterActionReturnXML(
    CComPtr <IBidiSpl>& bidiSpl,
    CString             actionString)
{
    stringstream utilExceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiRequest->SetSchema(actionString);
    if (FAILED(hr)) {
        utilExceptionText << "SetSchema(" << CW2A(actionString) << "): 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "SendRecv(BIDI_ACTION_GET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    HRESULT printerStatusHresult(S_OK);
    hr = bidiRequest->GetResult(&printerStatusHresult);
    if (FAILED(hr)) {
        utilExceptionText << "GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR printerActionReturnXML;
    hr = printerActionReturnXML.AppendBytes((PCHAR) dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(printerActionReturnXML);
}

////////////////////////////////////////////////////////////////////////////////
// util::WaitUntilJobSpooled()
//
// this routine is needed because the windows spooler begins processing spooled
// print job before they are completely spooled.
//
// here, we ensure that the windows spool job is 100% spooled, to synchronize
// with the iBidi job.
////////////////////////////////////////////////////////////////////////////////
void util::WaitUntilJobSpooled(
    const HANDLE    printerHandle,
    const DWORD     windowsJobID)
{
    if (NULL == printerHandle)
        return;

    while (true) {
        cout << "waiting for spooling to complete for windows print job " << windowsJobID << endl;

        ::Sleep(1000);

        DWORD neededByteCount = 0;
        if (!::GetJob(printerHandle, windowsJobID, 2, NULL, 0, &neededByteCount)) {
            if (::GetLastError() == ERROR_INSUFFICIENT_BUFFER) {
                vector <byte> jobInfo2Bytes(neededByteCount, 0);
                JOB_INFO_2* pJobInfo2 = (JOB_INFO_2*) (&jobInfo2Bytes[0]);

                DWORD returnedByteCount = 0;
                if (pJobInfo2 && ::GetJob(printerHandle, windowsJobID, 2, (byte*) pJobInfo2, neededByteCount, &returnedByteCount)) {
                    if (pJobInfo2->Status == JOB_STATUS_COMPLETE) {
                        break;
                    }
                }
            }
        }
    }
}

////////////////////////////////////////////////////////////////////////////////
// calls util::WaitUntilJobSpooled() - with a printer handle that we create.
////////////////////////////////////////////////////////////////////////////////
void util::WaitUntilJobSpooled(CString printerName, const DWORD windowsJobID)
{
    stringstream exceptionText;

    HANDLE printerHandle(0);
    int rc = ::OpenPrinter(printerName.GetBuffer(), &printerHandle, NULL);
    if (!rc) {
        const DWORD lastError = ::GetLastError();
        exceptionText << "::OpenPrinter(" << CT2A(printerName) << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }

    WaitUntilJobSpooled(printerHandle, windowsJobID);

    rc = ::ClosePrinter(printerHandle);
    if (FAILED(rc)) {
        exceptionText << "::ClosePrinter(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }
}

void util::UpdateDevmode(
    const HANDLE    printerHandle,
    const CString   printerName,
    DEVMODE*        devmode)
{
    int rc = ::DocumentProperties(
        0,
        printerHandle,
        CT2W(printerName),
        devmode, // Use the same DEVMODE for input and
        devmode, // output buffers
        DM_IN_BUFFER | DM_OUT_BUFFER);

    if (IDOK != rc) {
        const DWORD lastError = ::GetLastError();
        stringstream exceptionText;
        exceptionText << ":::DocumentProperties(" << CW2A(printerName)
            << ") " << lastError << " " << CT2A(util::Win32ErrorString(lastError));
        throw runtime_error(exceptionText.str());
    }
}

void util::PrintTextAndGraphics(
    CString        printerName,
    const HANDLE   printerHandle,
    DWORD&         windowsJobID,
    const bool     duplex,
    int yLocation)
{
    stringstream utilExceptionText;

    const LONG devmode_size = ::DocumentProperties(
        NULL,
        printerHandle,
        printerName.GetBuffer(),
        NULL,
        NULL,
        0); // Set to 0 to return required buffer size.

    // this buffer will nicely self-destruct:
    vector <byte> devmodeBytes(devmode_size, 0);
    DEVMODE* devmode = (DEVMODE*) (&devmodeBytes[0]);

    LONG long_rc = ::DocumentProperties(
        NULL,
        printerHandle,
        printerName.GetBuffer(),
        devmode,
        NULL,
        DM_OUT_BUFFER);
    if (long_rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "DocumentProperties():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    devmode->dmOrientation = DMORIENT_LANDSCAPE;

    if (duplex) {
        devmode->dmDuplex = DMDUP_VERTICAL;
    }
    else {
        devmode->dmDuplex = DMDUP_SIMPLEX;
    }

    HDC printerDC = ::CreateDC(
        L"WINSPOOL",
        printerName,
        NULL,
        devmode);
    if (NULL == printerDC) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "CreateDC():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    DOCINFO docInfo = {0};
    docInfo.cbSize = sizeof DOCINFO;
    docInfo.lpszDocName = L"XPS Driver SDK document";
    int rc = ::StartDoc(printerDC, &docInfo);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "StartDoc():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    windowsJobID = rc;

    rc = ::StartPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "StartPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    CString text;
    text.Format(L"Sample printing on %s", printerName);
    rc = ::TextOut(printerDC, 50, yLocation, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    rc = ::EndPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "EndPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    if (duplex) {
        rc = ::StartPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "StartPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(utilExceptionText.str());
        }

        text.Format(L"Sample back side printing on %s", printerName);
        rc = ::TextOut(printerDC, 50, 200, text, text.GetLength());
        if (0 == rc) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "TextOut():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(utilExceptionText.str());
        }

        rc = ::EndPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "EndPage():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw runtime_error(utilExceptionText.str());
        }
    }

    rc = ::EndDoc(printerDC);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "EndDoc():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    rc = ::DeleteDC(printerDC);
    if (rc == 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "DeleteDC():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }
}

void util::PrintText(CString printerName, const bool duplex, int yLocation)
{
    stringstream utilExceptionText;
    HANDLE printerHandle(0);
    BOOL brc = ::OpenPrinter(printerName.GetBuffer(), &printerHandle, NULL);
    if (!brc) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "OpenPrinter():" << " 0x" << hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw runtime_error(utilExceptionText.str());
    }

    // do some windows printing and get the windowsJobID:
    DWORD windowsJobID(0);
    PrintTextAndGraphics(printerName, printerHandle, windowsJobID, duplex, yLocation);

    // Important: wait until driver gets all the print data:
    WaitUntilJobSpooled(printerHandle, windowsJobID);
    ::ClosePrinter(printerHandle);
}

void util::PrintTextAndGraphics(
    CString        printerName,
    const HANDLE   printerHandle,
    DWORD&         windowsJobID,
    const bool     duplex,
    const Escapes  escapes)
{
    std::stringstream utilExceptionText;

    const LONG devmode_size = ::DocumentProperties(
        NULL,
        printerHandle,
        printerName.GetBuffer(),
        NULL,
        NULL,
        0); // Set to 0 to return required buffer size.

    // this buffer will nicely self-destruct:
    std::vector <byte> devmodeBytes(devmode_size, 0);
    DEVMODE* devmode = (DEVMODE*) (&devmodeBytes[0]);

    LONG long_rc = ::DocumentProperties(
        NULL,
        printerHandle,
        printerName.GetBuffer(),
        devmode,
        NULL,
        DM_OUT_BUFFER);
    if (long_rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "DocumentProperties():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    devmode->dmOrientation = DMORIENT_LANDSCAPE;

    if (duplex) {
        devmode->dmDuplex = DMDUP_VERTICAL;
    }
    else {
        devmode->dmDuplex = DMDUP_SIMPLEX;
    }

    HDC printerDC = ::CreateDC(
        L"WINSPOOL",
        devmode->dmDeviceName,
        NULL,
        devmode);
    if (NULL == printerDC) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "CreateDC():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    DOCINFO docInfo = {0};
    docInfo.cbSize = sizeof DOCINFO;
    docInfo.lpszDocName = L"XPS Driver SDK magstripe document";
    int rc = ::StartDoc(printerDC, &docInfo);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "StartDoc():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    windowsJobID = rc;

    rc = ::StartPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "StartPage():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    CString text;
    text.Format(L"Sample printing on %s", printerName);
    rc = ::TextOut(printerDC, 50, 220, text, text.GetLength());
    if (0 == rc) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    if ((escapes.escapeSide == escapeOnCardFront) ||
        (escapes.escapeSide == escapeOnBothCardSides)) {
        rc = ::TextOut(printerDC, 10, 10, escapes.printBlockingEscape, escapes.printBlockingEscape.GetLength());
        if (0 == rc) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw std::runtime_error(utilExceptionText.str());
        }

        rc = ::TextOut(printerDC, 20, 20, escapes.topcoatRemovalEscape, escapes.topcoatRemovalEscape.GetLength());
        if (0 == rc) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw std::runtime_error(utilExceptionText.str());
        }
    }

    rc = ::EndPage(printerDC);
    if (rc <= 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "EndPage():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    if (duplex) {
        rc = ::StartPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "StartPage():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw std::runtime_error(utilExceptionText.str());
        }

        text.Format(L"Sample back side printing on %s", printerName);
        rc = ::TextOut(printerDC, 50, 220, text, text.GetLength());
        if (0 == rc) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw std::runtime_error(utilExceptionText.str());
        }

        if ((escapes.escapeSide == escapeOnCardBack) ||
            (escapes.escapeSide == escapeOnBothCardSides)) {
            rc = ::TextOut(printerDC, 10, 10, escapes.printBlockingEscape, escapes.printBlockingEscape.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw std::runtime_error(utilExceptionText.str());
            }
            rc = ::TextOut(printerDC, 20, 20, escapes.topcoatRemovalEscape, escapes.topcoatRemovalEscape.GetLength());
            if (0 == rc) {
                const DWORD lastErr = ::GetLastError();
                utilExceptionText << "TextOut():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
                throw std::runtime_error(utilExceptionText.str());
            }
        }

        rc = ::EndPage(printerDC);
        if (rc <= 0) {
            const DWORD lastErr = ::GetLastError();
            utilExceptionText << "EndPage():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
            throw std::runtime_error(utilExceptionText.str());
        }
    }

    rc = ::EndDoc(printerDC);
    if (rc < 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "EndDoc():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    rc = ::DeleteDC(printerDC);
    if (rc == 0) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "DeleteDC():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }
}

void util::PrintText(CString printerName, const bool duplex, const Escapes escapes)
{
    std::stringstream utilExceptionText;
    HANDLE printerHandle(0);
    BOOL brc = ::OpenPrinter(printerName.GetBuffer(), &printerHandle, NULL);
    if (!brc) {
        const DWORD lastErr = ::GetLastError();
        utilExceptionText << "OpenPrinter():" << " 0x" << std::hex << lastErr << " " << CT2A(util::Win32ErrorString(lastErr));
        throw std::runtime_error(utilExceptionText.str());
    }

    // do some windows printing and get the windowsJobID:
    DWORD windowsJobID(0);
    PrintTextAndGraphics(printerName, printerHandle, windowsJobID, duplex, escapes);

    // Important: wait until driver gets all the print data:
    WaitUntilJobSpooled(printerHandle, windowsJobID);
    ::ClosePrinter(printerHandle);
}

CString util::GetJobStatusXML(CComPtr <IBidiSpl>& bidiSpl, const DWORD printerJobID)
{
    stringstream utilExceptionText;

    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CString jobStatusRequestXML;
    jobStatusRequestXML.Format(dxp01sdk::JOB_STATUS_XML, printerJobID);

    ULONG dataLength = jobStatusRequestXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    CComAllocator XMLBytesAllocator;
    PBYTE jobStatusXMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
    DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

    // copy the XML string...but not the terminating null:
    ::memcpy_s(jobStatusXMLBytes, dataLength, jobStatusRequestXML.GetBuffer(), dataLength);

    // Setup the IBidiSpl request object using IBidiRequest ...
    hr = bidiRequest->SetSchema(dxp01sdk::JOB_STATUS);
    if (FAILED(hr)) {
        utilExceptionText << "SetSchema(dxp01sdk::JOB_STATUS):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // send the request (xml that includes the printerJobID):
    hr = bidiRequest->SetInputData(BIDI_BLOB, jobStatusXMLBytes, dataLength);
    if (FAILED(hr)) {
        utilExceptionText << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    // Send the command to the printer.
    hr = bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
    if (FAILED(hr)) {
        utilExceptionText << "bidiSpl->SendRecv(BIDI_ACTION_GET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    HRESULT printerStatusHresult(S_OK);
    hr = bidiRequest->GetResult(&printerStatusHresult);
    if (FAILED(hr)) {
        utilExceptionText << "GetResult():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    LPWSTR schema(nullptr);
    PBYTE dataPointer(nullptr);
    DWORD dataType = 0;
    ULONG dataSizeInBytes = 0;
    hr = bidiRequest->GetOutputData(
        0,
        &schema,
        &dataType,
        &dataPointer,
        &dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "GetOutputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComBSTR jobStatusXML;
    hr = jobStatusXML.AppendBytes((PCHAR) dataPointer, dataSizeInBytes);
    if (FAILED(hr)) {
        utilExceptionText << "AppendBytes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    ::CoTaskMemFree(schema);
    ::CoTaskMemFree(dataPointer);

    return CString(jobStatusXML);
}

////////////////////////////////////////////////////////////////////////////////
// util::ParseJobStatusXML()
//
// retrieve values from Job Status XML and populate a JobStatusValues data
// structure. Return the data structure.
//
// Job Status XML looks like this:
//
// <?xml version="1.0" ?>
// <!-- Job status xml file. -->
// <JobStatus>
//    <ClientID>fellmadwin7x64_{901F168C-B593-42B8-8602-4C715D8D658C}</ClientID>
//    <WindowsJobID>0</WindowsJobID>
//    <PrinterJobID>1327</PrinterJobID>
//    <JobState>JobActive</JobState>
//    <JobRestartCount>0</JobRestartCount>
// </JobStatus>
//
////////////////////////////////////////////////////////////////////////////////
util::JobStatusValues util::ParseJobStatusXML(const CString jobStatusXML)
{
    stringstream utilExceptionText;

    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(jobStatusXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    JobStatusValues jobStatusValues;

    jobStatusValues._clientID = StringFromXml(doc, L"JobStatus/ClientID");
    jobStatusValues._jobState = StringFromXml(doc, L"JobStatus/JobState");
    jobStatusValues._windowsJobID = IntFromXml(doc, L"JobStatus/WindowsJobID");
    jobStatusValues._printerJobID = IntFromXml(doc, L"JobStatus/PrinterJobID");
    jobStatusValues._jobRestartCount = IntFromXml(doc, L"JobStatus/JobRestartCount");

    return jobStatusValues;
}

////////////////////////////////////////////////////////////////////////////////
// util::ParsePrinterOptionsXML()
//
// parse the xml returned from GetPrinterOptionsXML(). Populate a
// PrinterOptionsValues data structure with the values and return it.
//
// sample xml markup returned from printer sorted by element name:
//
//    <?xml version="1.0"?>
//    <!--Printer options2 xml file.-->
//    <PrinterInfo2>
//       <PrinterStatus>Ready< / PrinterStatus>
//       <PrinterAddress>10.2.200.224< / PrinterAddress>
//       <PrinterModel>CE870_Andy< / PrinterModel>
//       <PrinterSerialNumber>E00111< / PrinterSerialNumber>
//       <PrinterVersion>D3.17.4 - 1< / PrinterVersion>
//       <PrinterMessageNumber>0< / PrinterMessageNumber>
//       <ConnectionPortType>Network< / ConnectionPortType>
//       <ConnectionProtocol>Version2< / ConnectionProtocol>
//       <OptionInputhopper>MultiHopper6< / OptionInputhopper>
//       <OptionMagstripe>ISO< / OptionMagstripe>
//       <OptionRewritable>None< / OptionRewritable>
//       <OptionSecondaryMagstripeJIS>None< / OptionSecondaryMagstripeJIS>
//       <OptionSmartcard>Installed< / OptionSmartcard>
//       <OptionDuplex>Auto< / OptionDuplex>
//       <OptionPrinterBarcodeReader>Installed< / OptionPrinterBarcodeReader>
//       <OptionLocks>Installed< / OptionLocks>
//       <LockState>Locked< / LockState>
//       <PrintEngineType>DirectToCard_DyeSub< / PrintEngineType>
//       <PrintHead>Installed< / PrintHead>
//       <ColorPrintResolution>300x300 | 300x600< / ColorPrintResolution>
//       <MonochromePrintResolution>300x300 | 300x600 | 300x1200< / MonochromePrintResolution>
//       <TopcoatPrintResolution>300x300< / TopcoatPrintResolution>
//       <ModuleEmbosser>None< / ModuleEmbosser>
//       <Laminator>None< / Laminator>
//       <LaserModule>None< / LaserModule>
//       <LaserVisionRegistration>None< / LaserVisionRegistration>
//       <ObscureBlackPanel>None< / ObscureBlackPanel>
//    </PrinterInfo2>
//
////////////////////////////////////////////////////////////////////////////////
util::PrinterOptionsValues util::ParsePrinterOptionsXML(const CString printerOptionsXML)
{
    stringstream utilExceptionText;

    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(printerOptionsXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    PrinterOptionsValues printerOptionsValues;

    printerOptionsValues._colorPrintResolution = StringFromXml(doc, L"PrinterInfo2/ColorPrintResolution");
    printerOptionsValues._connectionPortType = StringFromXml(doc, L"PrinterInfo2/ConnectionPortType");
    printerOptionsValues._connectionProtocol = StringFromXml(doc, L"PrinterInfo2/ConnectionProtocol");
    printerOptionsValues._embosserVersion = StringFromXml(doc, L"PrinterInfo2/EmbosserVersion");
    printerOptionsValues._laminator = StringFromXml(doc, L"PrinterInfo2/Laminator");
    printerOptionsValues._laminatorFirmwareVersion = StringFromXml(doc, L"PrinterInfo2/LaminatorFirmwareVersion");
    printerOptionsValues._laminatorImpresser = StringFromXml(doc, L"PrinterInfo2/LaminatorImpresser");
    printerOptionsValues._laminatorScanner = StringFromXml(doc, L"PrinterInfo2/LaminatorScanner");
    printerOptionsValues._laserFirmwareVersion = StringFromXml(doc, L"PrinterInfo2/LaserVersion");
    printerOptionsValues._lockState = StringFromXml(doc, L"PrinterInfo2/LockState");
    printerOptionsValues._moduleEmbosser = StringFromXml(doc, L"PrinterInfo2/ModuleEmbosser");
    printerOptionsValues._monochromePrintResolution = StringFromXml(doc, L"PrinterInfo2/MonochromePrintResolution");
    printerOptionsValues._multiHopperVersion = StringFromXml(doc, L"PrinterInfo2/MultiHopperVersion");
    printerOptionsValues._optionDuplex = StringFromXml(doc, L"PrinterInfo2/OptionDuplex");
    printerOptionsValues._optionInputhopper = StringFromXml(doc, L"PrinterInfo2/OptionInputhopper");
    printerOptionsValues._optionLocks = StringFromXml(doc, L"PrinterInfo2/OptionLocks");
    printerOptionsValues._optionMagstripe = StringFromXml(doc, L"PrinterInfo2/OptionMagstripe");
    printerOptionsValues._optionSecondaryMagstripeJIS = StringFromXml(doc, L"PrinterInfo2/OptionSecondaryMagstripeJIS");
    printerOptionsValues._optionSmartcard = StringFromXml(doc, L"PrinterInfo2/OptionSmartcard");
    printerOptionsValues._printerAddress = StringFromXml(doc, L"PrinterInfo2/PrinterAddress");
    printerOptionsValues._printerMessageNumber = IntFromXml(doc, L"PrinterInfo2/PrinterMessageNumber");
    printerOptionsValues._printerModel = StringFromXml(doc, L"PrinterInfo2/PrinterModel");
    printerOptionsValues._printerSerialNumber = StringFromXml(doc, L"PrinterInfo2/PrinterSerialNumber");
    printerOptionsValues._printerStatus = StringFromXml(doc, L"PrinterInfo2/PrinterStatus");
    printerOptionsValues._printerVersion = StringFromXml(doc, L"PrinterInfo2/PrinterVersion");
    printerOptionsValues._printHead = StringFromXml(doc, L"PrinterInfo2/PrintHead");
    printerOptionsValues._topcoatPrintResolution = StringFromXml(doc, L"PrinterInfo2/TopcoatPrintResolution");
    printerOptionsValues._optionRewrite = StringFromXml(doc, L"PrinterInfo2/OptionRewritable");
    printerOptionsValues._optionPrinterBarcodeReader = StringFromXml(doc, L"PrinterInfo2/OptionPrinterBarcodeReader");

    //  as of version 7.0, printEngine defaults to direct-to-card...
    printerOptionsValues._printEngineType = StringFromXml(doc, L"PrinterInfo2/PrintEngineType");

    printerOptionsValues._optionLaser = StringFromXml(doc, L"PrinterInfo2/LaserModule");
    printerOptionsValues._optionLaserVisionRegistration = StringFromXml(doc, L"PrinterInfo2/LaserVisionRegistration");
    printerOptionsValues._optionObscureBlackPanel = StringFromXml(doc, L"PrinterInfo2/ObscureBlackPanel");
    return printerOptionsValues;
}

////////////////////////////////////////////////////////////////////////////////
// util::ParsePrinterCardCountsXML()
//
// the 'printer card counts' xml from the printer looks something like this:
//
//    <?xml version="1.0"?>
//    <!--Printer counter2 xml file.-->
//    <CounterStatus2>
//       <CardsPickedSinceCleaningCard>141</CardsPickedSinceCleaningCard>
//       <CleaningCardsRun>0</CleaningCardsRun>
//       <CurrentCompleted>260</CurrentCompleted>
//       <CurrentLost>0</CurrentLost>
//       <CurrentPicked>474</CurrentPicked>
//       <CurrentPickedExceptionSlot>2</CurrentPickedExceptionSlot>
//       <CurrentPickedInputHopper1>242</CurrentPickedInputHopper1>
//       <CurrentPickedInputHopper2>1</CurrentPickedInputHopper2>
//       <CurrentPickedInputHopper3>33</CurrentPickedInputHopper3>
//       <CurrentPickedInputHopper4>14</CurrentPickedInputHopper4>
//       <CurrentPickedInputHopper5>3</CurrentPickedInputHopper5>
//       <CurrentPickedInputHopper6>1</CurrentPickedInputHopper6>
//       <CurrentRejected>36</CurrentRejected>
//       <PrinterStatus>Ready</PrinterStatus>
//       <TotalCompleted>260</TotalCompleted>
//       <TotalLost>0</TotalLost>
//       <TotalPicked>474</TotalPicked>
//       <TotalPickedExceptionSlot>2</TotalPickedExceptionSlot>
//       <TotalPickedInputHopper1>242</TotalPickedInputHopper1>
//       <TotalPickedInputHopper2>1</TotalPickedInputHopper2>
//       <TotalPickedInputHopper3>33</TotalPickedInputHopper3>
//       <TotalPickedInputHopper4>14</TotalPickedInputHopper4>
//       <TotalPickedInputHopper5>3</TotalPickedInputHopper5>
//       <TotalPickedInputHopper6>1</TotalPickedInputHopper6>
//       <TotalRejected>36</TotalRejected>
//    </CounterStatus2>
//
// parse the xml returned from GetPrinterCardCountsXML(). Populate a
// PrinterCardCountValues data structure with the values and return it.
//
////////////////////////////////////////////////////////////////////////////////
util::PrinterCardCountValues util::ParsePrinterCardCountsXML(const CString printerCardCountsXML)
{
    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(printerCardCountsXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty(SelectionLanguage):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    PrinterCardCountValues printerCardCountValues;

    printerCardCountValues._cardsPickedSinceCleaningCard = IntFromXml(doc, L"CounterStatus2/CardsPickedSinceCleaningCard");
    printerCardCountValues._cleaningCardsRun = IntFromXml(doc, L"CounterStatus2/CleaningCardsRun");
    printerCardCountValues._currentCompleted = IntFromXml(doc, L"CounterStatus2/CurrentCompleted");
    printerCardCountValues._currentLost = IntFromXml(doc, L"CounterStatus2/CurrentLost");
    printerCardCountValues._currentPicked = IntFromXml(doc, L"CounterStatus2/CurrentPicked");
    printerCardCountValues._currentPickedExceptionSlot = IntFromXml(doc, L"CounterStatus2/CurrentPickedExceptionSlot");
    printerCardCountValues._currentPickedInputHopper1 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper1");
    printerCardCountValues._currentPickedInputHopper2 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper2");
    printerCardCountValues._currentPickedInputHopper3 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper3");
    printerCardCountValues._currentPickedInputHopper4 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper4");
    printerCardCountValues._currentPickedInputHopper5 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper5");
    printerCardCountValues._currentPickedInputHopper6 = IntFromXml(doc, L"CounterStatus2/CurrentPickedInputHopper6");
    printerCardCountValues._currentRejected = IntFromXml(doc, L"CounterStatus2/CurrentRejected");
    printerCardCountValues._printerStatus = StringFromXml(doc, L"CounterStatus2/PrinterStatus");
    printerCardCountValues._totalCompleted = IntFromXml(doc, L"CounterStatus2/TotalCompleted");
    printerCardCountValues._totalLost = IntFromXml(doc, L"CounterStatus2/TotalLost");
    printerCardCountValues._totalPicked = IntFromXml(doc, L"CounterStatus2/TotalPicked");
    printerCardCountValues._totalPickedExceptionSlot = IntFromXml(doc, L"CounterStatus2/TotalPickedExceptionSlot");
    printerCardCountValues._totalPickedInputHopper1 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper1");
    printerCardCountValues._totalPickedInputHopper2 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper2");
    printerCardCountValues._totalPickedInputHopper3 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper3");
    printerCardCountValues._totalPickedInputHopper4 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper4");
    printerCardCountValues._totalPickedInputHopper5 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper5");
    printerCardCountValues._totalPickedInputHopper6 = IntFromXml(doc, L"CounterStatus2/TotalPickedInputHopper6");
    printerCardCountValues._totalRejected = IntFromXml(doc, L"CounterStatus2/TotalRejected");

    return printerCardCountValues;
}

CString util::ParseDriverVersionXML(const CString sdkXML)
{
    // sample driver version xml:
    //
    // <?xml version="1.0"?>
    // <!--Printer driver version xml file.-->
    // <SDKVersion>3.0.258.0</SDKVersion>

    stringstream utilExceptionText;

    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(sdkXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CString driverVersionString = util::StringFromXml(doc, L"SDKVersion");
    return driverVersionString;
}

////////////////////////////////////////////////////////////////////////////////
// util::ParseSmartcardResponseXML()
//
// incoming XML looks something like this...from the CDATA section of a
// PrinterStatus response:
//
//    <?xml version="1.0" ?>
//    <!-- smartcard response xml  -->
//    <SmartcardResponse>
//       <Protocol>SCARD_PROTOCOL_RAW</Protocol>
//       <State>SCARD_PRESENT | SCARD_POWERED | SCARD_NEGOTIABLE</State>
//       <Status>SCARD_S_SUCCESS</Status>
//       <Base64Data>O4WBAQEay1xPxg==</Base64Data>
//    </SmartcardResponse>
//
// return a SmartcardResponseValues populated with those items.
////////////////////////////////////////////////////////////////////////////////
util::SmartcardResponseValues util::ParseSmartcardResponseXML(
    const CString smartcardReponseXML)
{
    stringstream utilExceptionText;
    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(smartcardReponseXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    util::SmartcardResponseValues smartcardResponseValues;
    smartcardResponseValues._protocol = util::StringFromXml(doc, L"SmartcardResponse/Protocol").Trim();

    CString statesString = util::StringFromXml(doc, L"SmartcardResponse/State").Trim();
    CString token;
    int currentPosition(0);
    do {
        token = statesString.Tokenize(L" |", currentPosition);
        if (!token.IsEmpty()) {
            wstring stateString = token;
            smartcardResponseValues._states.push_back(stateString);
        }
    } while (!token.IsEmpty());

    smartcardResponseValues._status = util::StringFromXml(doc, L"SmartcardResponse/Status").Trim();
    smartcardResponseValues._base64string = util::StringFromXml(doc, L"SmartcardResponse/Base64Data").Trim();

    if (smartcardResponseValues._base64string.length() > 0) {
        int convertedLength = (int) smartcardResponseValues._base64string.length();
        smartcardResponseValues._bytesFromBase64String.resize(convertedLength);
        ATL::Base64Decode(
            CW2A(smartcardResponseValues._base64string.c_str()),
            (int) smartcardResponseValues._base64string.length(),
            &smartcardResponseValues._bytesFromBase64String[0],
            &convertedLength);
        smartcardResponseValues._bytesFromBase64String.resize(convertedLength);
    }

    return smartcardResponseValues;
}

CString util::GetSuppliesXML(CComPtr <IBidiSpl>& bidiSpl)
{
    CString suppliesStatusXML = GetPrinterActionReturnXML(bidiSpl, dxp01sdk::SUPPLIES_STATUS3);
    return suppliesStatusXML;
}

util::SuppliesValues util::ParseSuppliesXML(
    const CString suppliesXML)
{
    // we're parsing XML that looks something like this:
    //
    // <?xml version="1.0"?>
    // <!--Printer Supplies3 xml file.-->
    // <PrinterSupplies3>
    //   <PrinterStatus>Ready</PrinterStatus>
    //   <Printer>
    //     <PrintRibbon>Installed</PrintRibbon>
    //     <PrintRibbonType>Monochrome</PrintRibbonType>
    //     <RibbonRemaining>43</RibbonRemaining>
    //     <RibbonSerialNumber>E0055000008E9956</RibbonSerialNumber>
    //     <RibbonLotCode>16MAR12  </RibbonLotCode>
    //     <RibbonPartNumber>533000052</RibbonPartNumber>
    //     <RibbonRegionCode>2</RibbonRegionCode>
    //   </Printer>
    //   <Embosser>
    //     <IndentRibbon>Installed</IndentRibbon>
    //     <IndentRibbonType>67174401</IndentRibbonType>
    //     <IndentRibbonRemaining>50</IndentRibbonRemaining>
    //     <IndentRibbonSerialNumber></IndentRibbonSerialNumber>
    //     <IndentRibbonLotCode></IndentRibbonLotCode>
    //     <IndentRibbonPartNumber></IndentRibbonPartNumber>
    //     <TopperRibbon>Installed</TopperRibbon>
    //     <TopperRibbonType>Silver</TopperRibbonType>
    //     <TopperRibbonRemaining>25</TopperRibbonRemaining>
    //     <TopperRibbonSerialNumber>25A18E00005005E0</TopperRibbonSerialNumber>
    //     <TopperRibbonLotCode>29Oct11  </TopperRibbonLotCode>
    //     <TopperRibbonPartNumber>504139013</TopperRibbonPartNumber>
    //   </Embosser>
    //   <Laminator>
    //     <L1Laminate>None</L1Laminate>
    //     <L1LaminateType></L1LaminateType>
    //     <L1LaminateRemaining></L1LaminateRemaining>
    //     <L1LaminateSerialNumber></L1LaminateSerialNumber>
    //     <L1LaminateLotCode></L1LaminateLotCode>
    //     <L1LaminatePartNumber></L1LaminatePartNumber>
    //     <L2Laminate>None</L2Laminate>
    //     <L2LaminateType></L2LaminateType>
    //     <L2LaminateRemaining></L2LaminateRemaining>
    //     <L2LaminateSerialNumber></L2LaminateSerialNumber>
    //     <L2LaminateLotCode></L2LaminateLotCode>
    //     <L2LaminatePartNumber></L2LaminatePartNumber>
    //   </Laminator>
    // </PrinterSupplies3>

    stringstream utilExceptionText;

    CComPtr <IXMLDOMDocument2> doc;
    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(suppliesXML), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    SuppliesValues suppliesValues;

    suppliesValues._indentRibbon = StringFromXml(doc, L"PrinterSupplies3/Embosser/IndentRibbon");
    suppliesValues._indentRibbonLotCode = StringFromXml(doc, L"PrinterSupplies3/Embosser/IndentRibbonLotCode");
    suppliesValues._indentRibbonPartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Embosser/IndentRibbonPartNumber");
    suppliesValues._indentRibbonRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Embosser/IndentRibbonRemaining");
    suppliesValues._indentRibbonSerialNumber = StringFromXml(doc, L"PrinterSupplies3/Embosser/IndentRibbonSerialNumber");
    suppliesValues._laminatorL1LotCode = StringFromXml(doc, L"PrinterSupplies3/Laminator/L1LaminateLotCode");
    suppliesValues._laminatorL1PartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Laminator/L1LaminatePartNumber");
    suppliesValues._laminatorL1PercentRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Laminator/L1LaminateRemaining");
    suppliesValues._laminatorL1SerialNumber = StringFromXml(doc, L"PrinterSupplies3/Laminator/L1LaminateSerialNumber");
    suppliesValues._laminatorL1SupplyCode = (long) IntFromXml(doc, L"PrinterSupplies3/Laminator/L1LaminateType");
    suppliesValues._laminatorL2LotCode = StringFromXml(doc, L"PrinterSupplies3/Laminator/L2LaminateLotCode");
    suppliesValues._laminatorL2PartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Laminator/L2LaminatePartNumber");
    suppliesValues._laminatorL2PercentRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Laminator/L2LaminateRemaining");
    suppliesValues._laminatorL2SerialNumber = StringFromXml(doc, L"PrinterSupplies3/Laminator/L2LaminateSerialNumber");
    suppliesValues._laminatorL2SupplyCode = (long) IntFromXml(doc, L"PrinterSupplies3/Laminator/L2LaminateType");
    suppliesValues._printerStatus = StringFromXml(doc, L"PrinterSupplies3/PrinterStatus");
    suppliesValues._printRibbonLotCode = StringFromXml(doc, L"PrinterSupplies3/Printer/RibbonLotCode");
    suppliesValues._printRibbonPartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Printer/RibbonPartNumber");
    suppliesValues._printRibbonSerialNumber = StringFromXml(doc, L"PrinterSupplies3/Printer/RibbonSerialNumber");
    suppliesValues._printRibbonType = StringFromXml(doc, L"PrinterSupplies3/Printer/PrintRibbonType");
    suppliesValues._ribbonRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Printer/RibbonRemaining");
    suppliesValues._printRibbonRegionCode = (short) IntFromXml(doc, L"PrinterSupplies3/Printer/RibbonRegionCode");
    suppliesValues._topperRibbonLotCode = StringFromXml(doc, L"PrinterSupplies3/Embosser/TopperRibbonLotCode");
    suppliesValues._topperRibbonPartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Embosser/TopperRibbonPartNumber");
    suppliesValues._topperRibbonRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Embosser/TopperRibbonRemaining");
    suppliesValues._topperRibbonSerialNumber = StringFromXml(doc, L"PrinterSupplies3/Embosser/TopperRibbonSerialNumber");
    suppliesValues._topperRibbonType = StringFromXml(doc, L"PrinterSupplies3/Embosser/TopperRibbonType");

    // retransfer supplies...
    suppliesValues._retransferFilmSerialNumber = StringFromXml(doc, L"PrinterSupplies3/Printer/RetransferFilmSerialNumber");
    suppliesValues._retransferFilmPartNumber = (long) IntFromXml(doc, L"PrinterSupplies3/Printer/RetransferFilmPartNumber");
    suppliesValues._retransferFilmLotCode = StringFromXml(doc, L"PrinterSupplies3/Printer/RetransferFilmLotCode");
    suppliesValues._retransferFilmRemaining = (short) IntFromXml(doc, L"PrinterSupplies3/Printer/RetransferFilmPercentRemaining");
    return suppliesValues;
}

////////////////////////////////////////////////////////////////////////////////
// util::PollForJobCompletion()
//
// repeatedly check job status until the job is complete.
////////////////////////////////////////////////////////////////////////////////
void util::PollForJobCompletion(
    CComPtr <IBidiSpl>&  bidiSpl,
    const int            printerJobID)
{
    const int pollSleepMilliseconds(2000);

    while (true) {
        const CString jobStatusXML = util::GetJobStatusXML(bidiSpl, printerJobID);
        const util::JobStatusValues jobStatusValues = util::ParseJobStatusXML(jobStatusXML);

        cout << "Printer job ID: " << printerJobID << " " << CW2A(jobStatusValues._jobState) << endl;

        if (jobStatusValues._jobState == dxp01sdk::JOB_SUCCEEDED ||
            jobStatusValues._jobState == dxp01sdk::JOB_FAILED ||
            jobStatusValues._jobState == dxp01sdk::JOB_CANCELLED ||
            jobStatusValues._jobState == dxp01sdk::CARD_NOT_RETRIEVED ||
            jobStatusValues._jobState == dxp01sdk::JOB_NOT_AVAILABLE) {
            return;
        }
        
        const CString printerStatusXml = util::GetPrinterStatusXML(bidiSpl);
        const util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXml);

        if ((0 != printerStatusValues._errorCode) && (printerJobID == printerStatusValues._printerJobID)) {
            stringstream exceptionText;
            exceptionText << CW2A(printerStatusValues._errorString) << " severity: " << printerStatusValues._errorSeverity << endl;
            throw BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }

        ::Sleep(pollSleepMilliseconds);
    }
}

////////////////////////////////////////////////////////////////////////////////
// util::EndJob()
//
// note: we do NOT throw any exceptions from this routine.
////////////////////////////////////////////////////////////////////////////////
void util::EndJob(CComPtr <IBidiSpl>& bidiSpl)
{
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        cerr << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiRequest->SetSchema(dxp01sdk::ENDJOB);
    if (FAILED(hr)) {
        cerr << "SetSchema(dxp01sdk::ENDJOB):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        cerr << "SendRecv(BIDI_ACTION_SET) endjob:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
    }
}

////////////////////////////////////////////////////////////////////////////////
// util::CancelJob()
//
// note: we do NOT throw any exceptions from this routine.
////////////////////////////////////////////////////////////////////////////////
void util::CancelJob(
    CComPtr <IBidiSpl>& bidiSpl,
    const int           printerJobID,
    const int           errorCode)
{
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        cerr << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    // create the XML and convert to bytes for the upcoming SetInputData() call.
    // NOTE: It is IMPORTANT to set the errorcode member; it clears that
    // particular error in the printer.
    CString actionXML = util::FormatPrinterActionXML(
        dxp01sdk::Cancel,
        printerJobID,
        errorCode);

    const int xmlBytesLength = actionXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    PBYTE xmlBytes = (PBYTE) ::CoTaskMemAlloc(xmlBytesLength);

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, actionXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_ACTION);
    if (FAILED(hr)) {
        cerr << "SetSchema(dxp01sdk::PRINTER_ACTION):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        cerr << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        cerr << "SendRecv(BIDI_ACTION_SET) cancel xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    ::CoTaskMemFree(xmlBytes);
}

////////////////////////////////////////////////////////////////////////////////
// util::CancelJob()
//
// note: we do NOT throw any exceptions from this routine.
////////////////////////////////////////////////////////////////////////////////
void util::CancelJob(
    CComPtr <IBidiSpl>&                 bidiSpl,
    const util::PrinterStatusValues&    printerStatusValues)
{
    CComPtr <IBidiRequest> bidiRequest;
    HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
    if (FAILED(hr)) {
        cerr << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    // create the XML and convert to bytes for the upcoming SetInputData() call.
    // NOTE: It is IMPORTANT to set the errorcode member; it clears that
    // particular error in the printer.
    CString actionXML = util::FormatPrinterActionXML(
        dxp01sdk::Cancel,
        printerStatusValues._printerJobID,
        printerStatusValues._errorCode);

    const int xmlBytesLength = actionXML.GetLength() * sizeof WCHAR;

    // IBidiSpl compatible memory:
    PBYTE xmlBytes = (PBYTE) ::CoTaskMemAlloc(xmlBytesLength);

    // copy the XML string...without the trailing NULL:
    ::memcpy_s(xmlBytes, xmlBytesLength, actionXML.GetBuffer(), xmlBytesLength);

    hr = bidiRequest->SetSchema(dxp01sdk::PRINTER_ACTION);
    if (FAILED(hr)) {
        cerr << "SetSchema(dxp01sdk::PRINTER_ACTION):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiRequest->SetInputData(BIDI_BLOB, xmlBytes, xmlBytesLength);
    if (FAILED(hr)) {
        cerr << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
    if (FAILED(hr)) {
        cerr << "SendRecv(BIDI_ACTION_SET) cancel xml:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        return;
    }

    ::CoTaskMemFree(xmlBytes);
}

CString util::GetShortExeName()
{
    CString exeName;
    ::GetModuleFileName(NULL, exeName.GetBuffer(MAX_PATH), MAX_PATH);
    CPath shortExeName(exeName);
    shortExeName.StripPath();
    return shortExeName;
}

CString util::GetExePath()
{
    CString longExeName;
    ::GetModuleFileName(NULL, longExeName.GetBuffer(MAX_PATH), MAX_PATH);
    longExeName.ReleaseBuffer();
    CPath exePath(longExeName);
    exePath.RemoveFileSpec();
    return exePath;
}

void util::CreateIStream(CComPtr <IStream>& stream)
{
    stringstream exceptionText;

    HGLOBAL streamHandle(0);
    int rc = ::CreateStreamOnHGlobal(streamHandle, false, &stream);
    if (FAILED(rc)) {
        exceptionText << "::CreateStreamOnHGlobal(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }
}

void util::RewindIStream(CComPtr <IStream>& stream)
{
    stringstream exceptionText;

    const LARGE_INTEGER BeginningOfStream = {0};
    int rc = stream->Seek(BeginningOfStream, STREAM_SEEK_SET, NULL);
    if (FAILED(rc)) {
        exceptionText << "stream->Seek(): " << rc << " " << CT2A(util::Win32ErrorString(rc));
        throw runtime_error(exceptionText.str());
    }
}

////////////////////////////////////////////////////////////////////////////////
// util::StartJob()
//
// Issue an IBidi StartJob with xml markup to start an IBidi print job.
//
// We do this to have the printer check for supplies, or, just to obtain the
// printerJobID for job completion polling later.
//
// If the printer is out of supplies, throw an exception.
//
////////////////////////////////////////////////////////////////////////////////
int util::StartJob(
    CComPtr <IBidiSpl>& bidiSpl,
    const CString       hopperID,
    const CString       cardEjectSide
)
{
    stringstream exceptionText;

    const int pollSleepMilliseconds = 2000;
    int printerJobID(0);

    do {
        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionText << "bidiRequest.CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        // we need the printer options to construct XML passed to iBidi StartJob():
        const CString printerOptionsXML = util::GetPrinterOptionsXML(bidiSpl);
        const util::PrinterOptionsValues printerOptionsValues = util::ParsePrinterOptionsXML(printerOptionsXML);

        hr = bidiRequest->SetSchema(dxp01sdk::STARTJOB);
        if (FAILED(hr)) {
            exceptionText << "SetSchema(dxp01sdk::STARTJOB):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        CString startjobXml;
        startjobXml.Format(
            dxp01sdk::STARTJOB_XML,
            hopperID,
            false, // CheckPrintRibbonSupplies is deprecated. Use \\Printer.SuppliesStatus3:Read to retrieve supplies status 
            false, // CheckEmbossSupplies is deprecated. Use \\Printer.SuppliesStatus3:Read to retrieve supplies status 
            cardEjectSide);


        const ULONG startjobDataLength = startjobXml.GetLength() * sizeof(TCHAR);

        // Get memory that can be used with IBidiSpl:
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(startjobDataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, startjobDataLength, startjobXml.GetBuffer(), startjobDataLength);

        // pass the startjob input data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, startjobDataLength);
        if (FAILED(hr)) {
            exceptionText << "SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        hr = bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
        if (FAILED(hr)) {
            exceptionText << "SendRecv(BIDI_ACTION_SET) startjob: " << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        HRESULT startJobResultCode(S_OK);
        hr = bidiRequest->GetResult(&startJobResultCode);
        if (FAILED(hr)) {
            exceptionText << "GetResult(startjob):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(exceptionText.str());
        }

        if (FAILED(startJobResultCode)) {
            exceptionText << "startJobResultCode:" << " 0x" << hex << startJobResultCode << " " << CT2A(util::Win32ErrorString(startJobResultCode));
            throw runtime_error(exceptionText.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);

        if (506 == printerStatusValues._errorCode) {
            // Printer cannot accept another job as it is busy. Try again.
            cout << "StartJob(): " << printerStatusValues._errorCode << "  " << CT2A(printerStatusValues._errorString) << " Trying again." << endl;

            // let the current card process in the printer:
            ::Sleep(pollSleepMilliseconds);
        }
        else if (0 != printerStatusValues._errorCode) {
            exceptionText << "StartJob(): " << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw BidiException(exceptionText.str(), printerStatusValues._printerJobID, printerStatusValues._errorCode);
        }
        else {
            cout << "Printer job ID: " << printerStatusValues._printerJobID << " started." << endl;
            printerJobID = printerStatusValues._printerJobID;
        }
    } while (0 == printerJobID);

    return printerJobID;
}


// ----------------------------------------------------------------------
// function:      util::ParseHopperStatusXml
// ----------------------------------------------------------------------
util::HOPPER_INFO_LIST  util::ParseHopperStatusXml(const CString hopperStatusXml)
{
    CComPtr <IXMLDOMDocument2>  doc;
    stringstream                utilExceptionText;
    HOPPER_INFO_LIST            hopperInfoList;

    HRESULT hr = doc.CoCreateInstance(__uuidof(DOMDocument60));
    if (FAILED(hr)) {
        utilExceptionText << "doc.CoCreateInstance():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->put_async(false);
    if (FAILED(hr)) {
        utilExceptionText << "doc->put_async():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    VARIANT_BOOL loadedOK(VARIANT_FALSE);
    hr = doc->loadXML(CComBSTR(hopperStatusXml), &loadedOK);
    if (FAILED(hr)) {
        utilExceptionText << "doc->loadXML():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    hr = doc->setProperty(L"SelectionLanguage", CComVariant(L"XPath"));
    if (FAILED(hr)) {
        utilExceptionText << "doc->setProperty():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }

    CComPtr <IXMLDOMNodeList> nodelist;
    hr = doc->selectNodes(L"HopperStatus/HopperInformation", &nodelist);
    if (FAILED(hr)) {
        utilExceptionText << "doc->selectNodes():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
        throw runtime_error(utilExceptionText.str());
    }


    if (nodelist) {
        long    numHoppers;
        hr = nodelist->get_length(&numHoppers);
        if (FAILED(hr)) {
            utilExceptionText << "nodelist->get_length():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw runtime_error(utilExceptionText.str());
        }

        for (int hopperIndex = 0; hopperIndex < numHoppers; hopperIndex++) {
            CComPtr <IXMLDOMNode> node;
            HOPPER_INFORMATION  hopperInfo;

            hr = nodelist->get_item(hopperIndex, &node);
            if (FAILED(hr)) {
                utilExceptionText << "nodelist->get_item:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComPtr <IXMLDOMNamedNodeMap> attributeMap;
            hr = node->get_attributes(&attributeMap);
            if (FAILED(hr)) {
                utilExceptionText << "node->get_attributes:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            //  name...
            CComPtr <IXMLDOMNode> nameAttributeNode;
            hr = attributeMap->getNamedItem(L"Name", &nameAttributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem(Name):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            CComBSTR attributeText;
            hr = nameAttributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "nameAttributeNode->get_text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }
            hopperInfo._name = attributeText;

            //  type...
            CComPtr <IXMLDOMNode> typeAttributeNode;
            hr = attributeMap->getNamedItem(L"Type", &typeAttributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem(Type):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            hr = typeAttributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "typeAttributeNode->get_text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }
            hopperInfo._type = attributeText;

            //  status...
            CComPtr <IXMLDOMNode> statusAttributeNode;
            hr = attributeMap->getNamedItem(L"Status", &statusAttributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem(Status):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            hr = statusAttributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "statusAttributeNode->get_text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }
            hopperInfo._status = attributeText;

            //  cardstock...
            CComPtr <IXMLDOMNode> cardStockAttributeNode;
            hr = attributeMap->getNamedItem(L"CardStock", &cardStockAttributeNode);
            if (FAILED(hr)) {
                utilExceptionText << "attributeMap->getNamedItem(CardStock):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }

            hr = cardStockAttributeNode->get_text(&attributeText);
            if (FAILED(hr)) {
                utilExceptionText << "cardStockAttributeNode->get_text:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
                throw runtime_error(utilExceptionText.str());
            }
            hopperInfo._cardStock = attributeText;
            hopperInfoList.push_back(hopperInfo);
        }
    }

    return hopperInfoList;
}



// ----------------------------------------------------------------------
// function:      util::ParseFirmwareRev
// ----------------------------------------------------------------------
util::FirmwareVersion util::ParseFirmwareRev(CString strFwRev)
{
    // FW revision is of the following format(s)
    // D3.17.3  or D3.17.3-4

    // tokenize deviceType
    int currentPosition(0);
    CString token = strFwRev.Tokenize(L".", currentPosition);

    //FirmwareVersion fwRev = new FirmwareVersion();
    FirmwareVersion fwRev;
    fwRev._printerBase = "";
    fwRev._majorVersion = 0;
    fwRev._minorVersion = 0;
    fwRev._deviationVersion = 0;

    // PrinterType
    fwRev._printerBase = token;

    // tokenize the "Major" rev
    token = strFwRev.Tokenize(L".", currentPosition);
    std::string str = CW2A(token);
    int nMajorRev = atoi(str.c_str());
    fwRev._majorVersion = (short) nMajorRev;


    // minor revision 
    // FYI - there could be 1 or 2 elements in this array
    token = strFwRev.Tokenize(L"-", currentPosition);
    str = CW2A(token);
    int nMinorRev = atoi(str.c_str());
    fwRev._minorVersion = (short) nMinorRev;

    // minor revision deviation
    token = strFwRev.Tokenize(L"-", currentPosition);
    if (token.GetLength() > 0) {
        str = CW2A(token);
        int nMinorRevDev = atoi(str.c_str());
        fwRev._deviationVersion = (short) nMinorRevDev;
    }

    // base minor rev is <= actual minor rev.
    return fwRev;
}