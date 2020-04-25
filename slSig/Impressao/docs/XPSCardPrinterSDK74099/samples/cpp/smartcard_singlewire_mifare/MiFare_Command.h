////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// construct mifare commands for duali reader.
//
// see "CCID_Protocol_spec_120224_.pdf"
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <vector>

namespace dxp01sdk
{
    class MiFare_Command {
    public:

        enum KeyType {
            A = 0, B = 4
        };

        std::vector <byte> CreateGetCardStatusCommand();

        std::vector <byte> CreateLoadKeysCommand(
            const KeyType       keyType,
            const byte          sector,
            std::vector <byte>  keyBytes);

        std::vector <byte> CreateReadBlockCommand(
            const KeyType   keyType,
            const byte      sector,
            const byte      block);

        std::vector <byte> CreateWriteBlockCommand(
            const KeyType       keyType,
            const byte          sector,
            const byte          block,
            std::vector <byte>  blockData);
    };
}
