////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// construct mifare commands for duali reader.
//
// see "CCID_Protocol_spec_120224_.pdf"
////////////////////////////////////////////////////////////////////////////////
#include "StdAfx.h"
#include "Mifare_Common.h"
#include "MiFare_Command.h"

using namespace dxp01sdk;

std::vector <byte> MiFare_Command::CreateGetCardStatusCommand()
{
    // see pg 23, "CCID_Protocol_spec_120224_.pdf"

    const byte commandBytes[] = {0xFE, 0x17, 0xFE, 0xFE, 0x00};
    const std::vector <byte> getCardStatusCommand(commandBytes, commandBytes + ARRAYSIZE(commandBytes));
    return getCardStatusCommand;
}

std::vector <byte> MiFare_Command::CreateLoadKeysCommand(
    const KeyType       keyType,
    const byte          sector,
    std::vector <byte>  keyBytes)
{
    // see pg 11, "CCID_Protocol_spec_120224_.pdf"

    if (0 == keyBytes.size()) {
        throw std::runtime_error("keyBytes has no data");
    }

    MiFare_Common miFareCommon;
    miFareCommon.CheckSectorNumber(sector);

    byte commandBytes[] = {0xFD, 0x2F, (byte) keyType, sector, 0x06};
    std::vector <byte> loadKeysCommand(
        commandBytes,
        commandBytes + ARRAYSIZE(commandBytes));

    // append keyBytes onto the end of the commandBytes:
    loadKeysCommand.insert(loadKeysCommand.end(), keyBytes.begin(), keyBytes.end());

    return loadKeysCommand;
}

std::vector <byte> MiFare_Command::CreateReadBlockCommand(
    const KeyType   keyType,
    const byte      sector,
    const byte      block)
{
    // see pg 12, "CCID_Protocol_spec_120224_.pdf"

    MiFare_Common miFareCommon;
    miFareCommon.CheckSectorNumber(sector);
    miFareCommon.CheckBlockNumber(block);

    const byte commandBytes[] = {0xFD, 0x35, (byte) keyType, sector, 0x01, block};
    const std::vector <byte> readBlockCommand(
        commandBytes,
        commandBytes + ARRAYSIZE(commandBytes));
    return readBlockCommand;
}

std::vector <byte> MiFare_Command::CreateWriteBlockCommand(
    const KeyType       keyType,
    const byte          sector,
    const byte          block,
    std::vector <byte>  blockData)
{
    // see pg 13, "CCID_Protocol_spec_120224_.pdf"

    if (0 == blockData.size()) {
        throw std::runtime_error("blockData has no data");
    }

    MiFare_Common miFareCommon;
    miFareCommon.CheckSectorNumber(sector);
    miFareCommon.CheckBlockNumber(block);

    const byte commandBytes[] = {0xFD, 0x37, (byte) keyType, sector, 0x11, block};
    std::vector <byte> writeBlockCommand(
        commandBytes,
        commandBytes + ARRAYSIZE(commandBytes));

    // append blockData onto the end of the commandBytes:
    writeBlockCommand.insert(writeBlockCommand.end(), blockData.begin(), blockData.end());

    return writeBlockCommand;
}