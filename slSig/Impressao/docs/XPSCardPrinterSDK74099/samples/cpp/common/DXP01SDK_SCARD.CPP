////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// c++ class wrapper that is similar to Win32 smartcard API in WinSCard.h
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <sstream>
#include <iostream>
#include <atlenc.h>
#include "util.h"
#include "DXP01SDK.H"
#include "DXP01SDK_SCARD_STRINGS.H"
#include "DXP01SDK_SCARD.H"

using namespace std;
using namespace dxp01sdk;

////////////////////////////////////////////////////////////////////////////////
// local exception class: not thrown beyond this source file.
////////////////////////////////////////////////////////////////////////////////
class ScardException {
public:
    ScardException(const long errorCode, string errorString)
        : _exception(errorString.c_str())
        , _errorCode(errorCode)
    {}

    long errorcode() const
    {
        return _errorCode;
    }
    string what()
    {
        return _exception.what();
    }

private:

    exception _exception {};
    long _errorCode {};
};

////////////////////////////////////////////////////////////////////////////////
// SCard::SCard()
//
// the only c++ constructor.
////////////////////////////////////////////////////////////////////////////////
SCard::SCard(CComPtr <IBidiSpl>& bidiSpl) : _bidiSpl(bidiSpl)
{}

////////////////////////////////////////////////////////////////////////////////
// SCard::StringFromConnectionType()
// public static function
////////////////////////////////////////////////////////////////////////////////
wstring SCard::StringFromConnectionType(const ChipConnection connectionType)
{
    switch (connectionType) {
    case contact:     return SMARTCARD_PROTOCOL_T0_OR_T1;
    case contactless: return SMARTCARD_PROTOCOL_CONTACTLESS;
    }
    throw runtime_error("expected one of contact or contactless");
}

////////////////////////////////////////////////////////////////////////////////
// SCard::ProtocolFromString()
// public static function
// return a DWORD value as declared in WinSmCrd.h for the given string.
////////////////////////////////////////////////////////////////////////////////
DWORD SCard::ProtocolFromString(const wstring protocolString)
{
    if (protocolString == SMARTCARD_PROTOCOL_RAW)      return SCARD_PROTOCOL_RAW;
    if (protocolString == SMARTCARD_PROTOCOL_T0)       return SCARD_PROTOCOL_T0;
    if (protocolString == SMARTCARD_PROTOCOL_T1)       return SCARD_PROTOCOL_T1;
    if (protocolString == SMARTCARD_PROTOCOL_T0_OR_T1) return SCARD_PROTOCOL_Tx;
    return SCARD_PROTOCOL_UNDEFINED;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::StringsFromProtocol()
// public static function
//
// return strings for the given protocol as declared in WinSmCrd.h. Note that
// the protocol contains 'flag' bits.
////////////////////////////////////////////////////////////////////////////////
vector <wstring> SCard::StringsFromProtocol(const DWORD protocol)
{
    vector <wstring> protocols;
    if (SCARD_PROTOCOL_UNDEFINED == protocol) {
        protocols.push_back(SMARTCARD_PROTOCOL_RAW);
        return protocols;
    }

    if (SCARD_PROTOCOL_T0 & protocol) {
        protocols.push_back(SMARTCARD_PROTOCOL_T0);
    }

    if (SCARD_PROTOCOL_T1 & protocol) {
        protocols.push_back(SMARTCARD_PROTOCOL_T1);
    }

    if (SCARD_PROTOCOL_RAW & protocol) {
        protocols.push_back(SMARTCARD_PROTOCOL_RAW);
    }

    if (SCARD_PROTOCOL_DEFAULT & protocol) {
        protocols.push_back(SMARTCARD_PROTOCOL_RAW);
    }

    return protocols;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::StringFromAttr()
// public static function
// return a string corresponding to one of the attribute types in WinSmCrd.h.
////////////////////////////////////////////////////////////////////////////////
wstring SCard::StringFromAttr(const DWORD attributeID)
{
    switch (attributeID) {
    case SCARD_ATTR_VENDOR_NAME:              return SMARTCARD_ATTR_VENDOR_NAME;
    case SCARD_ATTR_VENDOR_IFD_TYPE:          return SMARTCARD_ATTR_VENDOR_IFD_TYPE;
    case SCARD_ATTR_VENDOR_IFD_VERSION:       return SMARTCARD_ATTR_VENDOR_IFD_VERSION;
    case SCARD_ATTR_VENDOR_IFD_SERIAL_NO:     return SMARTCARD_ATTR_VENDOR_IFD_SERIAL_NO;
    case SCARD_ATTR_ATR_STRING:               return SMARTCARD_ATTR_ATR_STRING;
    case SCARD_ATTR_CHANNEL_ID:               return SMARTCARD_ATTR_CHANNEL_ID;
    case SCARD_ATTR_CHARACTERISTICS:          return SMARTCARD_ATTR_CHARACTERISTICS;
    case SCARD_ATTR_CURRENT_BWT:              return SMARTCARD_ATTR_CURRENT_BWT;
    case SCARD_ATTR_CURRENT_CLK:              return SMARTCARD_ATTR_CURRENT_CLK;
    case SCARD_ATTR_CURRENT_CWT:              return SMARTCARD_ATTR_CURRENT_CWT;
    case SCARD_ATTR_CURRENT_D:                return SMARTCARD_ATTR_CURRENT_D;
    case SCARD_ATTR_CURRENT_EBC_ENCODING:     return SMARTCARD_ATTR_CURRENT_EBC_ENCODING;
    case SCARD_ATTR_CURRENT_F:                return SMARTCARD_ATTR_CURRENT_F;
    case SCARD_ATTR_CURRENT_IFSC:             return SMARTCARD_ATTR_CURRENT_IFSC;
    case SCARD_ATTR_CURRENT_IFSD:             return SMARTCARD_ATTR_CURRENT_IFSD;
    case SCARD_ATTR_CURRENT_IO_STATE:         return SMARTCARD_ATTR_CURRENT_IO_STATE;
    case SCARD_ATTR_CURRENT_N:                return SMARTCARD_ATTR_CURRENT_N;
    case SCARD_ATTR_CURRENT_PROTOCOL_TYPE:    return SMARTCARD_ATTR_CURRENT_PROTOCOL_TYPE;
    case SCARD_ATTR_CURRENT_W:                return SMARTCARD_ATTR_CURRENT_W;
    case SCARD_ATTR_DEFAULT_CLK:              return SMARTCARD_ATTR_DEFAULT_CLK;
    case SCARD_ATTR_DEFAULT_DATA_RATE:        return SMARTCARD_ATTR_DEFAULT_DATA_RATE;
    case SCARD_ATTR_DEVICE_IN_USE:            return SMARTCARD_ATTR_DEVICE_IN_USE;
    case SCARD_ATTR_DEVICE_SYSTEM_NAME:       return SMARTCARD_ATTR_DEVICE_SYSTEM_NAME;
    case SCARD_ATTR_DEVICE_UNIT:              return SMARTCARD_ATTR_DEVICE_UNIT;
    case SCARD_ATTR_ESC_AUTHREQUEST:          return SMARTCARD_ATTR_ESC_AUTHREQUEST;
    case SCARD_ATTR_ESC_CANCEL:               return SMARTCARD_ATTR_ESC_CANCEL;
    case SCARD_ATTR_ESC_RESET:                return SMARTCARD_ATTR_ESC_RESET;
    case SCARD_ATTR_EXTENDED_BWT:             return SMARTCARD_ATTR_EXTENDED_BWT;
    case SCARD_ATTR_ICC_INTERFACE_STATUS:     return SMARTCARD_ATTR_ICC_INTERFACE_STATUS;
    case SCARD_ATTR_ICC_PRESENCE:             return SMARTCARD_ATTR_ICC_PRESENCE;
    case SCARD_ATTR_ICC_TYPE_PER_ATR:         return SMARTCARD_ATTR_ICC_TYPE_PER_ATR;
    case SCARD_ATTR_MAX_CLK:                  return SMARTCARD_ATTR_MAX_CLK;
    case SCARD_ATTR_MAX_DATA_RATE:            return SMARTCARD_ATTR_MAX_DATA_RATE;
    case SCARD_ATTR_MAX_IFSD:                 return SMARTCARD_ATTR_MAX_IFSD;
    case SCARD_ATTR_MAXINPUT:                 return SMARTCARD_ATTR_MAXINPUT;
    case SCARD_ATTR_POWER_MGMT_SUPPORT:       return SMARTCARD_ATTR_POWER_MGMT_SUPPORT;
    case SCARD_ATTR_SUPRESS_T1_IFS_REQUEST:   return SMARTCARD_ATTR_SUPRESS_T1_IFS_REQUEST;
    case SCARD_ATTR_USER_AUTH_INPUT_DEVICE:   return SMARTCARD_ATTR_USER_AUTH_INPUT_DEVICE;
    case SCARD_ATTR_USER_TO_CARD_AUTH_DEVICE: return SMARTCARD_ATTR_USER_TO_CARD_AUTH_DEVICE;
    case SCARD_ATTR_DEVICE_FRIENDLY_NAME_A:   return SMARTCARD_ATTR_DEVICE_FRIENDLY_NAME;
    case SCARD_ATTR_DEVICE_FRIENDLY_NAME_W:   return SMARTCARD_ATTR_DEVICE_FRIENDLY_NAME;

    default:
        ATLASSERT(0);
        return wstring(L"SCard::StringFromAttr fail");
    }
}

////////////////////////////////////////////////////////////////////////////////
// SCard::StateFromString()
// public static function
// return a DWORD value as declared in WinSmCrd.h for the given string.
////////////////////////////////////////////////////////////////////////////////
DWORD SCard::StateFromString(const wstring stateString)
{
    if (stateString == SMARTCARD_STATE_ABSENT)      return SCARD_ABSENT;
    if (stateString == SMARTCARD_STATE_NEGOTIABLE)  return SCARD_NEGOTIABLE;
    if (stateString == SMARTCARD_STATE_POWERED)     return SCARD_POWERED;
    if (stateString == SMARTCARD_STATE_PRESENT)     return SCARD_PRESENT;
    if (stateString == SMARTCARD_STATE_SPECIFIC)    return SCARD_SPECIFIC;
    if (stateString == SMARTCARD_STATE_SWALLOWED)   return SCARD_SWALLOWED;
    if (stateString == SMARTCARD_STATE_UNKNOWN)     return SCARD_UNKNOWN;
    return SCARD_UNKNOWN;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::StringFromState()
// public static function
// return string corresponding to the state DWORD as declared in WinSmCrd.h.
////////////////////////////////////////////////////////////////////////////////
wstring SCard::StringFromState(const DWORD state)
{
    switch (state) {
    case SCARD_UNKNOWN:     return SMARTCARD_STATE_UNKNOWN;
    case SCARD_ABSENT:      return SMARTCARD_STATE_ABSENT;
    case SCARD_PRESENT:     return SMARTCARD_STATE_PRESENT;
    case SCARD_SWALLOWED:   return SMARTCARD_STATE_SWALLOWED;
    case SCARD_POWERED:     return SMARTCARD_STATE_POWERED;
    case SCARD_NEGOTIABLE:  return SMARTCARD_STATE_NEGOTIABLE;
    case SCARD_SPECIFIC:    return SMARTCARD_STATE_SPECIFIC;

    default:
        ATLASSERT(0);
        return SMARTCARD_STATE_UNKNOWN;
    }
}

////////////////////////////////////////////////////////////////////////////////
// SCard::StringFromDisposition()
// public static function
// for the given disposition, return a string used in the IBidi XML.
//
// SCARD_EJECT_CARD not supported.
////////////////////////////////////////////////////////////////////////////////
wstring SCard::StringFromDisposition(const DWORD disposition)
{
    switch (disposition) {
    case SCARD_RESET_CARD:     return SMARTCARD_DISCONNECT_RESET;
    case SCARD_UNPOWER_CARD:   return SMARTCARD_DISCONNECT_UNPOWER;
    case SCARD_LEAVE_CARD:
    default:
        return SMARTCARD_DISCONNECT_LEAVE;
    }
}

////////////////////////////////////////////////////////////////////////////////
// SCard::ErrorFromString()
// public static function
// return a long value. See
// http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=VS.85).aspx#smart_card_return_values
// for various SCARD_ ... errors
// in debug builds, assert if unknown error.
// in release build, return SCARD_F_UNKNOWN_ERROR.
////////////////////////////////////////////////////////////////////////////////
long SCard::ErrorFromString(const wstring errorString)
{
    if (errorString == SMARTCARD_ERR_S_SUCCESS) return SCARD_S_SUCCESS;
    if (errorString == SMARTCARD_ERR_E_BAD_SEEK) return SCARD_E_BAD_SEEK;
    if (errorString == SMARTCARD_ERR_E_CANCELLED) return SCARD_E_CANCELLED;
    if (errorString == SMARTCARD_ERR_E_CANT_DISPOSE) return SCARD_E_CANT_DISPOSE;
    if (errorString == SMARTCARD_ERR_E_CARD_UNSUPPORTED) return SCARD_E_CARD_UNSUPPORTED;
    if (errorString == SMARTCARD_ERR_E_CERTIFICATE_UNAVAILABLE) return SCARD_E_CERTIFICATE_UNAVAILABLE;
    if (errorString == SMARTCARD_ERR_E_COMM_DATA_LOST) return SCARD_E_COMM_DATA_LOST;
    if (errorString == SMARTCARD_ERR_E_DIR_NOT_FOUND) return SCARD_E_DIR_NOT_FOUND;
    if (errorString == SMARTCARD_ERR_E_DUPLICATE_READER) return SCARD_E_DUPLICATE_READER;
    if (errorString == SMARTCARD_ERR_E_FILE_NOT_FOUND) return SCARD_E_FILE_NOT_FOUND;
    if (errorString == SMARTCARD_ERR_E_ICC_CREATEORDER) return SCARD_E_ICC_CREATEORDER;
    if (errorString == SMARTCARD_ERR_E_ICC_INSTALLATION) return SCARD_E_ICC_INSTALLATION;
    if (errorString == SMARTCARD_ERR_E_INSUFFICIENT_BUFFER) return SCARD_E_INSUFFICIENT_BUFFER;
    if (errorString == SMARTCARD_ERR_E_INVALID_ATR) return SCARD_E_INVALID_ATR;
    if (errorString == SMARTCARD_ERR_E_INVALID_CHV) return SCARD_E_INVALID_CHV;
    if (errorString == SMARTCARD_ERR_E_INVALID_HANDLE) return SCARD_E_INVALID_HANDLE;
    if (errorString == SMARTCARD_ERR_E_INVALID_PARAMETER) return SCARD_E_INVALID_PARAMETER;
    if (errorString == SMARTCARD_ERR_E_INVALID_TARGET) return SCARD_E_INVALID_TARGET;
    if (errorString == SMARTCARD_ERR_E_INVALID_VALUE) return SCARD_E_INVALID_VALUE;
    if (errorString == SMARTCARD_ERR_E_NO_ACCESS) return SCARD_E_NO_ACCESS;
    if (errorString == SMARTCARD_ERR_E_NO_DIR) return SCARD_E_NO_DIR;
    if (errorString == SMARTCARD_ERR_E_NO_FILE) return SCARD_E_NO_FILE;
    if (errorString == SMARTCARD_ERR_E_NO_KEY_CONTAINER) return SCARD_E_NO_KEY_CONTAINER;
    if (errorString == SMARTCARD_ERR_E_NO_MEMORY) return SCARD_E_NO_MEMORY;
    if (errorString == SMARTCARD_ERR_E_NO_READERS_AVAILABLE) return SCARD_E_NO_READERS_AVAILABLE;
    if (errorString == SMARTCARD_ERR_E_NO_SERVICE) return SCARD_E_NO_SERVICE;
    if (errorString == SMARTCARD_ERR_E_NO_SMARTCARD) return SCARD_E_NO_SMARTCARD;
    if (errorString == SMARTCARD_ERR_E_NO_SUCH_CERTIFICATE) return SCARD_E_NO_SUCH_CERTIFICATE;
    if (errorString == SMARTCARD_ERR_E_NOT_READY) return SCARD_E_NOT_READY;
    if (errorString == SMARTCARD_ERR_E_NOT_TRANSACTED) return SCARD_E_NOT_TRANSACTED;
    if (errorString == SMARTCARD_ERR_E_PCI_TOO_SMALL) return SCARD_E_PCI_TOO_SMALL;
    if (errorString == SMARTCARD_ERR_E_PROTO_MISMATCH) return SCARD_E_PROTO_MISMATCH;
    if (errorString == SMARTCARD_ERR_E_READER_UNAVAILABLE) return SCARD_E_READER_UNAVAILABLE;
    if (errorString == SMARTCARD_ERR_E_READER_UNSUPPORTED) return SCARD_E_READER_UNSUPPORTED;
    if (errorString == SMARTCARD_ERR_E_SERVER_TOO_BUSY) return SCARD_E_SERVER_TOO_BUSY;
    if (errorString == SMARTCARD_ERR_E_SERVICE_STOPPED) return SCARD_E_SERVICE_STOPPED;
    if (errorString == SMARTCARD_ERR_E_SHARING_VIOLATION) return SCARD_E_SHARING_VIOLATION;
    if (errorString == SMARTCARD_ERR_E_SYSTEM_CANCELLED) return SCARD_E_SYSTEM_CANCELLED;
    if (errorString == SMARTCARD_ERR_E_TIMEOUT) return SCARD_E_TIMEOUT;
    if (errorString == SMARTCARD_ERR_E_UNEXPECTED) return SCARD_E_UNEXPECTED;
    if (errorString == SMARTCARD_ERR_E_UNKNOWN_CARD) return SCARD_E_UNKNOWN_CARD;
    if (errorString == SMARTCARD_ERR_E_UNKNOWN_READER) return SCARD_E_UNKNOWN_READER;
    if (errorString == SMARTCARD_ERR_E_UNKNOWN_RES_MNG) return SCARD_E_UNKNOWN_RES_MNG;
    if (errorString == SMARTCARD_ERR_E_UNSUPPORTED_FEATURE) return SCARD_E_UNSUPPORTED_FEATURE;
    if (errorString == SMARTCARD_ERR_E_WRITE_TOO_MANY) return SCARD_E_WRITE_TOO_MANY;
    if (errorString == SMARTCARD_ERR_F_COMM_ERROR) return SCARD_F_COMM_ERROR;
    if (errorString == SMARTCARD_ERR_F_INTERNAL_ERROR) return SCARD_F_INTERNAL_ERROR;
    if (errorString == SMARTCARD_ERR_F_UNKNOWN_ERROR) return SCARD_F_UNKNOWN_ERROR;
    if (errorString == SMARTCARD_ERR_F_WAITED_TOO_LONG) return SCARD_F_WAITED_TOO_LONG;
    if (errorString == SMARTCARD_ERR_P_SHUTDOWN) return SCARD_P_SHUTDOWN;
    if (errorString == SMARTCARD_ERR_W_CANCELLED_BY_USER) return SCARD_W_CANCELLED_BY_USER;
    if (errorString == SMARTCARD_ERR_W_CARD_NOT_AUTHENTICATED) return SCARD_W_CARD_NOT_AUTHENTICATED;
    if (errorString == SMARTCARD_ERR_W_CHV_BLOCKED) return SCARD_W_CHV_BLOCKED;
    if (errorString == SMARTCARD_ERR_W_EOF) return SCARD_W_EOF;
    if (errorString == SMARTCARD_ERR_W_REMOVED_CARD) return SCARD_W_REMOVED_CARD;
    if (errorString == SMARTCARD_ERR_W_RESET_CARD) return SCARD_W_RESET_CARD;
    if (errorString == SMARTCARD_ERR_W_SECURITY_VIOLATION) return SCARD_W_SECURITY_VIOLATION;
    if (errorString == SMARTCARD_ERR_W_UNPOWERED_CARD) return SCARD_W_UNPOWERED_CARD;
    if (errorString == SMARTCARD_ERR_W_UNRESPONSIVE_CARD) return SCARD_W_UNRESPONSIVE_CARD;
    if (errorString == SMARTCARD_ERR_W_UNSUPPORTED_CARD) return SCARD_W_UNSUPPORTED_CARD;
    if (errorString == SMARTCARD_ERR_W_WRONG_CHV) return SCARD_W_WRONG_CHV;

    ATLASSERT(false);

    return SCARD_F_UNKNOWN_ERROR;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardConnect()
//
// wrapper for the win32 ::SCardConnect() function.
//
// for the given XPS Card Printer SDK connection type (contact or contactless),
// populate the protocol DWORD with the flag[s] as in Winscard.H.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379473(v=vs.85).aspx
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardConnect(
    const ChipConnection connectionType,
    DWORD&               protocol)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);

    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        const wstring connectionTypeString = StringFromConnectionType(connectionType);
        CString xml;
        xml.Format(SMARTCARD_CONNECT_XML, connectionTypeString.c_str());

        // Data is the length of the string multiplied by size of wide char
        const ULONG dataLength = xml.GetLength() * sizeof(TCHAR);

        // Get memory that can be used with IBidiSpl:
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, dataLength, xml.GetBuffer(), dataLength);

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_CONNECT);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_CONNECT):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // pass the smartcard data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, dataLength);
        if (FAILED(hr)) {
            exceptionStream << "SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "SendRecv(BIDI_ACTION_SET): 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SmartcardConnect(): 0x" << hex << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        protocol = ProtocolFromString(smartcardResponseValues._protocol);
        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardStatus()
//
// wrapper for the win32 ::SCardStatus() function.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379803(v=VS.85).aspx
//
// populate values for state, protocol, and the ATR response bytes.
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardStatus(
    vector <DWORD>&   states,
    DWORD&            protocol,
    vector <byte>&    ATRBytes)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);

    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_STATUS);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_STATUS):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "SendRecv(BIDI_ACTION_SET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SmartcardStatus():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        ATRBytes = smartcardResponseValues._bytesFromBase64String;
        protocol = ProtocolFromString(smartcardResponseValues._protocol);

        for (size_t index = 0; index < smartcardResponseValues._states.size(); index++) {
            const wstring stateString = smartcardResponseValues._states[index];
            states.push_back(StateFromString(stateString));
        }

        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardDisConnect()
//
// wrapper for the win32 ::SCardDisConnect() function.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379475(v=VS.85).aspx
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardDisConnect(const DWORD disposition)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);
    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        wstring dispositionString = StringFromDisposition(disposition);
        CString xml;
        xml.Format(SMARTCARD_DISCONNECT_XML, dispositionString.c_str());

        // Data is the length of the string multiplied by size of wide char
        ULONG dataLength = xml.GetLength() * sizeof TCHAR;

        // Get memory that can be used with IBidiSpl
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, dataLength, xml.GetBuffer(), dataLength);

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_DISCONNECT);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_DISCONNECT):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // pass the smartcard data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, dataLength);
        if (FAILED(hr)) {
            exceptionStream << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "bidiSpl->SendRecv(BIDI_ACTION_SET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SmartcardDisConnect():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardGetAttrib()
//
// wrapper for the win32 ::SCardGetAttrib() function.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379559(v=VS.85).aspx
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardGetAttrib(const DWORD attrID, vector <byte>& attrBytes)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);
    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_GETATTRIB);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_GETATTRIB):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString xml;
        wstring scard_attribute_string = StringFromAttr(attrID);
        xml.Format(SMARTCARD_GETATTRIB_XML, scard_attribute_string.c_str());

        // Data is the length of the string multiplied by size of wide char
        ULONG dataLength = xml.GetLength() * sizeof WCHAR;

        // Get memory that can be used with IBidiSpl
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, dataLength, xml.GetBuffer(), dataLength);

        // pass the smartcard data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, dataLength);
        if (FAILED(hr)) {
            exceptionStream << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_GET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "bidiSpl->SendRecv(BIDI_ACTION_SET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SMARTCARD_GETATTRIB():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        attrBytes = smartcardResponseValues._bytesFromBase64String;
        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardReconnect()
//
// wrapper for the win32 ::SCardReconnect() function.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379797%28v=VS.85%29.aspx
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardReconnect(
    const ChipConnection connectionType,
    const DWORD          initialization,
    DWORD&               protocol)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);
    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        const wstring connectionTypeString = StringFromConnectionType(connectionType);
        wstring initializationString = StringFromDisposition(initialization);
        CString xml;
        xml.Format(
            SMARTCARD_RECONNECT_XML,
            connectionTypeString.c_str(),
            initializationString.c_str());

        // Data is the length of the string multiplied by size of wide char
        const ULONG dataLength = xml.GetLength() * sizeof(TCHAR);

        // Get memory that can be used with IBidiSpl:
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, dataLength, xml.GetBuffer(), dataLength);

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_RECONNECT);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_RECONNECT):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // pass the smartcard data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, dataLength);
        if (FAILED(hr)) {
            exceptionStream << "SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "SendRecv(BIDI_ACTION_SET): 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SmartcardConnect(): 0x" << hex << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        protocol = ProtocolFromString(smartcardResponseValues._protocol);
        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}

////////////////////////////////////////////////////////////////////////////////
// SCard::SCardTransmit()
//
// wrapper for the win32 ::SCardTransmit() function.
//
// see http://msdn.microsoft.com/en-us/library/windows/desktop/aa379804(v=VS.85).aspx
//
// the bytes we send are known as an 'APDU'. see:
// http://msdn.microsoft.com/en-us/library/windows/desktop/aa374745(v=vs.85).aspx
//
// return: a smartcard errorcode as defined in WinError.H.
// see  http://msdn.microsoft.com/en-us/library/windows/desktop/aa374738(v=vs.85).aspx#smart_card_return_values
//
// return SCARD_S_SUCCESS if no error.
////////////////////////////////////////////////////////////////////////////////
long SCard::SCardTransmit(
    const vector <byte>  sendBytes,
    vector <byte>&       receivedBytes)
{
    long scardResult(SCARD_F_UNKNOWN_ERROR);
    try {
        stringstream exceptionStream;

        CComPtr <IBidiRequest> bidiRequest;
        HRESULT hr = bidiRequest.CoCreateInstance(CLSID_BidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "CoCreateInstance(CLSID_BidiRequest):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // allocate a big buffer for the ATL::Base64Encode() function. We found
        // that Base64EncodeGetRequiredLength() did not return a large enough
        // value in some cases.
        const int maxEncodedStringSize =
            ATL::Base64EncodeGetRequiredLength((int) sendBytes.size()) * 2;

        vector <char> base64EncodedChars(maxEncodedStringSize, 0); // zero-fill
        int convertedCharCount = maxEncodedStringSize;
        BOOL brc = ATL::Base64Encode(
            &sendBytes[0],
            (int) sendBytes.size(),
            &base64EncodedChars[0],
            &convertedCharCount,
            ATL_BASE64_FLAG_NOCRLF);
        if (!brc) {
            hr = ERROR_GEN_FAILURE;
            exceptionStream << "ATL::Base64Encode() fail:" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CComBSTR base64string = &base64EncodedChars[0]; // truncates trailing zeros

        CString xml;
        xml.Format(SMARTCARD_TRANSMIT_XML, base64string);

        // Data is the length of the string multiplied by size of wide char
        ULONG dataLength = xml.GetLength() * sizeof TCHAR;

        // Get memory that can be used with IBidiSpl:
        CComAllocator XMLBytesAllocator; // automatically frees COM memory
        PBYTE XMLBytes = (PBYTE) XMLBytesAllocator.Allocate(dataLength);
        DBG_UNREFERENCED_LOCAL_VARIABLE(XMLBytesAllocator); // suppress compiler warning

        // copy the XML string...but not the terminating null:
        ::memcpy_s(XMLBytes, dataLength, xml.GetBuffer(), dataLength);

        // Setup the IBidiSpl request object using IBidiRequest ...
        hr = bidiRequest->SetSchema(SMARTCARD_TRANSMIT);
        if (FAILED(hr)) {
            exceptionStream << "SetSchema(SMARTCARD_TRANSMIT):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // pass the smartcard data:
        hr = bidiRequest->SetInputData(BIDI_BLOB, XMLBytes, dataLength);
        if (FAILED(hr)) {
            exceptionStream << "bidiRequest->SetInputData():" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        // Send the command to the printer.
        hr = _bidiSpl->SendRecv(BIDI_ACTION_SET, bidiRequest);
        if (FAILED(hr)) {
            exceptionStream << "bidiSpl->SendRecv(BIDI_ACTION_SET):" << " 0x" << hex << hr << " " << CT2A(util::Win32ErrorString(hr));
            throw ScardException(hr, exceptionStream.str());
        }

        CString printerStatusXML = util::GetPrinterStatusXML(bidiRequest);
        util::PrinterStatusValues printerStatusValues = util::ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            exceptionStream << "SmartcardTransmit():" << " 0x" << printerStatusValues._errorCode << " " << CT2A(printerStatusValues._errorString);
            throw ScardException(hr, exceptionStream.str());
        }

        util::SmartcardResponseValues smartcardResponseValues = util::ParseSmartcardResponseXML(printerStatusValues._printerData);
        receivedBytes = smartcardResponseValues._bytesFromBase64String;
        scardResult = ErrorFromString(smartcardResponseValues._status);
    }
    catch (ScardException& e) {
        cout << e.what() << endl;
    }

    return scardResult;
}