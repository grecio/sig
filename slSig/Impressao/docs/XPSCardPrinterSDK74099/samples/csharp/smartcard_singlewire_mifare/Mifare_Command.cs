////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// construct mifare commands for duali reader.
//
// see "CCID_Protocol_spec_120224_.pdf"
////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Diagnostics;

namespace dxp01sdk.mifare {

    public class Mifare_Command {

        public enum KeyType : byte { A = 0, B = 4 }

        // see pg 23, "CCID_Protocol_spec_120224_.pdf"
        public byte[] CreateGetCardStatusCommand() {
            var commandBytes = new byte[] { 0xFE, 0x17, 0xFE, 0xFE, 0x00 };
            return commandBytes;
        }

        // see pg 11, "CCID_Protocol_spec_120224_.pdf"
        public byte[] CreateLoadKeysCommand(KeyType keyType, byte sector, byte[] keyBytes) {
            Mifare_Common.CheckSectorNumber(sector);
            Debug.Assert(keyBytes.Length == 6);
            var commandBytes = new List<byte>(new byte[] { 0xFD, 0x2F, (byte) keyType, sector, 0x06 });
            commandBytes.AddRange(keyBytes);
            return commandBytes.ToArray();
        }

        // see pg 12, "CCID_Protocol_spec_120224_.pdf"
        public byte[] CreateReadBlockCommand(KeyType keyType, byte sector, byte block) {
            Mifare_Common.CheckSectorNumber(sector);
            Mifare_Common.CheckBlockNumber(block);
            var commandBytes = new byte[] { 0xFD, 0x35, (byte) keyType, sector, 0x01, block };
            return commandBytes;
        }

        // see pg 13, "CCID_Protocol_spec_120224_.pdf"
        public byte[] CreateWriteBlockCommand(KeyType keyType, byte sector, byte block, byte[] blockData) {
            Mifare_Common.CheckSectorNumber(sector);
            Mifare_Common.CheckBlockNumber(block);
            var commandBytes = new List<byte>(new byte[] { 0xFD, 0x37, (byte) keyType, sector, 0x11, block });
            commandBytes.AddRange(blockData);
            return commandBytes.ToArray();
        }
    }
}