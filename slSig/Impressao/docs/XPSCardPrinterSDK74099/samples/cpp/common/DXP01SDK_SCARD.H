////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// c++ class wrapper that is similar to Win32 smartcard API in the Windows SDK's
// WinSCard.h
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <BiDiSpl.h>
#include <string>
#include <vector>
#include <winscard.h>

namespace dxp01sdk
{
    class SCard {
    public:

        SCard(CComPtr <IBidiSpl>& bidiSpl);

        enum ChipConnection {
            contact,
            contactless
        };

    public:

        long SCardConnect(
            const ChipConnection connectionType,
            DWORD&               protocol);

        long SCardStatus(
            std::vector <DWORD>& states,
            DWORD&               protocol,
            std::vector <byte>&  ATRBytes);

        long SCardReconnect(
            const ChipConnection connectionType,
            const DWORD          initialization,
            DWORD&               protocol);

        long SCardGetAttrib(
            const DWORD          attrID,
            std::vector <byte>&  attrBytes);

        long SCardTransmit(
            const std::vector <byte> sendBytes,
            std::vector <byte>&      receivedBytes);

        long SCardDisConnect(const DWORD disposition);

    public:

        // static helper functions
        static std::wstring                 StringFromConnectionType(const ChipConnection);

        static DWORD                        ProtocolFromString(const std::wstring protocolString);
        static std::vector <std::wstring>   StringsFromProtocol(const DWORD protocol);

        static DWORD                        StateFromString(const std::wstring stateString);

        static std::wstring                 StringFromState(const DWORD state);

        static long                         ErrorFromString(const std::wstring errorString);

        static std::wstring                 StringFromDisposition(const DWORD disposition);

        static std::wstring                 StringFromAttr(const DWORD attributeID);

    private:

        CComPtr <IBidiSpl>   _bidiSpl;

    private:

        // disable assignment operator (=) and copy constructor by making private:
        SCard(const SCard&) = delete;
        SCard& operator= (const SCard&) = delete;
    };
}