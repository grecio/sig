////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// MiFareResponse class.
//
// helper functions to send and receive MiFare responses to a MiFare chip via
// the Duali reader.
//
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "MiFare_Response.h"

MiFareResponse::MiFareResponse(std::vector <byte> responseBytes)
    : _responseBytes(responseBytes)
{}

bool MiFareResponse::IsStatusOK()
{
    const WORD statusWord = GetStatusWord();
    return (0x9000 == statusWord);
}

WORD MiFareResponse::GetStatusWord()
{
    const size_t length = _responseBytes.size();
    const WORD statusWord = (WORD) ((_responseBytes[length - 2] << 8) + (_responseBytes[length - 1]));
    return statusWord;
}

std::pair <byte, byte> MiFareResponse::GetSw1Sw2()
{
    const size_t length = _responseBytes.size();
    byte sw1 = _responseBytes[length - 2];
    byte sw2 = _responseBytes[length - 1];
    std::pair <byte, byte> sw1sw2(sw1, sw2);
    return sw1sw2;
}