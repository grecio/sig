////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// utility functions for mifare / duali
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <vector>
#include <map>

namespace dxp01sdk
{
    class MiFare_Common {
    public:

        MiFare_Common();
        std::vector <byte> GetMifareTestKey();
        CString ErrorStringFromStatusByte(const byte status);
        CString StatusCodeBytesToString(std::vector <byte> data);
        void CheckStatusCode(const byte statusCode);
        void CheckSectorNumber(const byte sector);
        void CheckBlockNumber(const byte blockNumber);

    private:

        std::map <byte, CString> _statusCodeStrings;

        void LoadStatusCodeStrings();
    };
}