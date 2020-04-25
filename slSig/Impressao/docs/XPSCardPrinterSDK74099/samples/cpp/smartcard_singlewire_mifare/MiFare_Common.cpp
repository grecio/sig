////////////////////////////////////////////////////////////////////////////////
// utility functions for mifare / duali
////////////////////////////////////////////////////////////////////////////////
#include "StdAfx.h"
#include "MiFare_Common.h"

using namespace dxp01sdk;

MiFare_Common::MiFare_Common()
{
    LoadStatusCodeStrings();
}

std::vector <byte> MiFare_Common::GetMifareTestKey()
{
    const byte bytes[] = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
    const std::vector <byte> mifare_test_key(bytes, bytes + ARRAYSIZE(bytes));
    return mifare_test_key;
}

CString MiFare_Common::ErrorStringFromStatusByte(const byte status)
{
    if (_statusCodeStrings.find(status) != _statusCodeStrings.end()) {
        return _statusCodeStrings[status];
    }
    CString errorString;
    errorString.Format(TEXT("undefined status code: %d"), status);
    return errorString;
}

CString MiFare_Common::StatusCodeBytesToString(std::vector <byte> data)
{
    if (data.size() < 3) {
        return "not enough data for status code commandBytes.";
    }

    CString dataString;
    dataString.Format(TEXT("status code: %s; SW1, SW2: %02X %02X"),
        ErrorStringFromStatusByte(data[0]),
        data[data.size() - 2],
        data[data.size() - 1]);

    return dataString;
}

void MiFare_Common::CheckStatusCode(const byte statusCode)
{
    if (statusCode != 0) {
        CString msg = ErrorStringFromStatusByte(statusCode);
        throw std::runtime_error(CT2A(msg));
    }
}

void MiFare_Common::CheckSectorNumber(const byte sector)
{
    if (sector >= 0 && sector <= 0x0f) {
        return;
    }
    throw std::runtime_error("sector must be between 0 and 15");
}

void MiFare_Common::CheckBlockNumber(const byte blockNumber)
{
    if (blockNumber >= 0 && blockNumber <= 0x0f) {
        return;
    }
    throw std::runtime_error("block must be between 0 and 15");
}

void MiFare_Common::LoadStatusCodeStrings()
{
    // see pg 45, "CCID_Protocol_spec_120224_.pdf"

    _statusCodeStrings[0x00] = TEXT("OK");
    _statusCodeStrings[0x02] = TEXT("NO TAG ERROR");
    _statusCodeStrings[0x03] = TEXT("CRC ERROR");
    _statusCodeStrings[0x04] = TEXT("EMPTY (NO IC CARD ERROR)");
    _statusCodeStrings[0x05] = TEXT("AUTHENTICATION ERROR or NO POWER");
    _statusCodeStrings[0x06] = TEXT("PARITY ERROR");
    _statusCodeStrings[0x07] = TEXT("CODE ERROR");
    _statusCodeStrings[0x08] = TEXT("SERIAL NUMBER ERROR");
    _statusCodeStrings[0x09] = TEXT("KEY ERROR");
    _statusCodeStrings[0x0A] = TEXT("NOT AUTHENTICATION ERROR");
    _statusCodeStrings[0x0B] = TEXT("BIT COUNT ERROR");
    _statusCodeStrings[0x0C] = TEXT("BYTE COUNT ERROR");
    _statusCodeStrings[0x0E] = TEXT("TRANSFER ERROR");
    _statusCodeStrings[0x0F] = TEXT("WRITE ERROR");
    _statusCodeStrings[0x10] = TEXT("INCREMENT ERROR");
    _statusCodeStrings[0x11] = TEXT("DECREMENT ERROR");
    _statusCodeStrings[0x12] = TEXT("READ ERROR");
    _statusCodeStrings[0x13] = TEXT("OVERFLOW ERROR");
    _statusCodeStrings[0x14] = TEXT("POLLING ERROR");
    _statusCodeStrings[0x15] = TEXT("FRAMING ERROR");
    _statusCodeStrings[0x16] = TEXT("ACCESS ERROR");
    _statusCodeStrings[0x17] = TEXT("UNKNOWN COMMAND ERROR");
    _statusCodeStrings[0x18] = TEXT("ANTICOLLISION ERROR");
    _statusCodeStrings[0x19] = TEXT("INITIALIZATION(RESET) ERROR");
    _statusCodeStrings[0x1A] = TEXT("INTERFACE ERROR");
    _statusCodeStrings[0x1B] = TEXT("ACCESS TIMEOUT ERROR");
    _statusCodeStrings[0x1C] = TEXT("NO BITWISE ANTICOLLISION ERROR");
    _statusCodeStrings[0x1D] = TEXT("FILE ERROR");
    _statusCodeStrings[0x20] = TEXT("INVAILD BLOCK ERROR");
    _statusCodeStrings[0x21] = TEXT("ACK COUNT ERROR");
    _statusCodeStrings[0x22] = TEXT("NACK DESELECT ERROR");
    _statusCodeStrings[0x23] = TEXT("NACK COUNT ERROR");
    _statusCodeStrings[0x24] = TEXT("SAME FRAME COUNT ERROR");
    _statusCodeStrings[0x31] = TEXT("RCV BUFFER TOO SMALL ERROR");
    _statusCodeStrings[0x32] = TEXT("RCV BUFFER OVERFLOW ERROR");
    _statusCodeStrings[0x33] = TEXT("RF ERROR");
    _statusCodeStrings[0x34] = TEXT("PROTOCOL ERROR");
    _statusCodeStrings[0x35] = TEXT("USER BUFFER FULL ERROR");
    _statusCodeStrings[0x36] = TEXT("BUADRATE NOT SUPPORTED");
    _statusCodeStrings[0x37] = TEXT("INVAILD FORMAT ERROR");
    _statusCodeStrings[0x38] = TEXT("LRC ERROR");
    _statusCodeStrings[0x39] = TEXT("FRAMERR");
    _statusCodeStrings[0x3C] = TEXT("WRONG PARAMETER VALUE");
    _statusCodeStrings[0x3D] = TEXT("INVAILD PARAMETER ERROR");
    _statusCodeStrings[0x3E] = TEXT("UNSUPPORTED PARAMETER");
    _statusCodeStrings[0x3F] = TEXT("UNSUPPORTED COMMAND");
    _statusCodeStrings[0x40] = TEXT("INTERFACE NOT ENABLED");
    _statusCodeStrings[0x41] = TEXT("ACK SUPPOSED");
    _statusCodeStrings[0x42] = TEXT("NACK RECEVIED");
    _statusCodeStrings[0x43] = TEXT("BLOCKNR NOT EQUAL");
    _statusCodeStrings[0x44] = TEXT("TARGET _SET_TOX");
    _statusCodeStrings[0x45] = TEXT("TARGET_RESET_TOX");
    _statusCodeStrings[0x46] = TEXT("TARGET_DESELECTED");
    _statusCodeStrings[0x47] = TEXT("TARGET_RELEASED");
    _statusCodeStrings[0x48] = TEXT("ID_ALREADY_IN_USE");
    _statusCodeStrings[0x49] = TEXT("INSTANCE_ALREADY_IN_USE");
    _statusCodeStrings[0x4A] = TEXT("ID_NOT_IN_USE");
    _statusCodeStrings[0x4B] = TEXT("NO_ID_AVAILABLE");
    _statusCodeStrings[0x4C] = TEXT("MI_JOINER_TEMP_ERROR or OTHER_ERROR");
    _statusCodeStrings[0x4D] = TEXT("INVALID _STATE");
    _statusCodeStrings[0x64] = TEXT("NOTYET_IMPLEMENTED");
    _statusCodeStrings[0x6D] = TEXT("FIFO ERROR");
    _statusCodeStrings[0x72] = TEXT("WRONG SELECT COUNT");
    _statusCodeStrings[0x7B] = TEXT("WRONG_VALUE");
    _statusCodeStrings[0x7C] = TEXT("VALERR");
    _statusCodeStrings[0x7E] = TEXT("RE_INIT");
    _statusCodeStrings[0x7F] = TEXT("NO_INIT");
}