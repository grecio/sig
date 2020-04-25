////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <vector>
#include <atlstr.h>

class MiFareResponse {
public:

    MiFareResponse(std::vector <byte> responseBytes);

    bool IsStatusOK();

    WORD GetStatusWord();

    std::pair <byte, byte> GetSw1Sw2();

private:

    std::vector <byte> _responseBytes;
};
