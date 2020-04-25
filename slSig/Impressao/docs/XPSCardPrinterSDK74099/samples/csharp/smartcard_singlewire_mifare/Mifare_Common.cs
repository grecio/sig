////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// utility functions for mifare / duali
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace dxp01sdk.mifare {

    public class Mifare_Common {

        public static byte[] mifare_test_key = new byte[] {
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF
        };

        static public string ErrorStringFromStatusByte(byte status) {
            if (_statusCodeStrings.ContainsKey(status)) {
                return _statusCodeStrings[status];
            }
            return string.Format("undefined status code: {0}", status);
        }

        static public string StatusCodeBytesToString(byte[] data) {
            if (data.Length < 3)
                return "not enough data for status code commandBytes.";

            var result = string.Format("status code: {0}; SW1, SW2: {1:X2} {2:X2}",
                ErrorStringFromStatusByte(data[0]),
                data[data.Length - 2],
                data[data.Length - 1]);
            return result;
        }

        static public void CheckStatusCode(byte statusCode) {
            if (statusCode != 0) {
                var msg = ErrorStringFromStatusByte(statusCode);
                throw new Exception(msg);
            }
        }

        static public void CheckSectorNumber(byte sector) {
            if (sector >= 0 && sector <= 0x0f) return;
            throw new Exception("sector must be between 0 and 15");
        }

        static public void CheckBlockNumber(byte blockNumber) {
            if (blockNumber >= 0 && blockNumber <= 0x0f) return;
            throw new Exception("block must be between 0 and 15");
        }

        // see pg 45, "CCID_Protocol_spec_120224_.pdf"
        static private Dictionary<byte, string> _statusCodeStrings = new Dictionary<byte, string> {
            {0x00, "OK"},
            {0x02, "NO TAG ERROR" },
            {0x03, "CRC ERROR" },
            {0x04, "EMPTY (NO IC CARD ERROR)" },
            {0x05, "AUTHENTICATION ERROR or NO POWER" },
            {0x06, "PARITY ERROR" },
            {0x07, "CODE ERROR" },
            {0x08, "SERIAL NUMBER ERROR" },
            {0x09, "KEY ERROR" },
            {0x0A, "NOT AUTHENTICATION ERROR" },
            {0x0B, "BIT COUNT ERROR" },
            {0x0C, "BYTE COUNT ERROR" },
            {0x0E, "TRANSFER ERROR" },
            {0x0F, "WRITE ERROR" },
            {0x10, "INCREMENT ERROR" },
            {0x11, "DECREMENT ERROR" },
            {0x12, "READ ERROR" },
            {0x13, "OVERFLOW ERROR" },
            {0x14, "POLLING ERROR" },
            {0x15, "FRAMING ERROR" },
            {0x16, "ACCESS ERROR" },
            {0x17, "UNKNOWN COMMAND ERROR" },
            {0x18, "ANTICOLLISION ERROR" },
            {0x19, "INITIALIZATION(RESET) ERROR" },
            {0x1A, "INTERFACE ERROR" },
            {0x1B, "ACCESS TIMEOUT ERROR" },
            {0x1C, "NO BITWISE ANTICOLLISION ERROR" },
            {0x1D, "FILE ERROR" },
            {0x20, "INVAILD BLOCK ERROR" },
            {0x21, "ACK COUNT ERROR" },
            {0x22, "NACK DESELECT ERROR" },
            {0x23, "NACK COUNT ERROR" },
            {0x24, "SAME FRAME COUNT ERROR" },
            {0x31, "RCV BUFFER TOO SMALL ERROR" },
            {0x32, "RCV BUFFER OVERFLOW ERROR" },
            {0x33, "RF ERROR" },
            {0x34, "PROTOCOL ERROR" },
            {0x35, "USER BUFFER FULL ERROR" },
            {0x36, "BUADRATE NOT SUPPORTED" },
            {0x37, "INVAILD FORMAT ERROR" },
            {0x38, "LRC ERROR" },
            {0x39, "FRAMERR" },
            {0x3C, "WRONG PARAMETER VALUE" },
            {0x3D, "INVAILD PARAMETER ERROR" },
            {0x3E, "UNSUPPORTED PARAMETER" },
            {0x3F, "UNSUPPORTED COMMAND" },
            {0x40, "INTERFACE NOT ENABLED" },
            {0x41, "ACK SUPPOSED" },
            {0x42, "NACK RECEVIED" },
            {0x43, "BLOCKNR NOT EQUAL" },
            {0x44, "TARGET _SET_TOX" },
            {0x45, "TARGET_RESET_TOX" },
            {0x46, "TARGET_DESELECTED" },
            {0x47, "TARGET_RELEASED" },
            {0x48, "ID_ALREADY_IN_USE" },
            {0x49, "INSTANCE_ALREADY_IN_USE" },
            {0x4A, "ID_NOT_IN_USE" },
            {0x4B, "NO_ID_AVAILABLE" },
            {0x4C, "MI_JOINER_TEMP_ERROR or OTHER_ERROR" },
            {0x4D, "INVALID _STATE" },
            {0x64, "NOTYET_IMPLEMENTED" },
            {0x6D, "FIFO ERROR" },
            {0x72, "WRONG SELECT COUNT" },
            {0x7B, "WRONG_VALUE" },
            {0x7C, "VALERR" },
            {0x7E, "RE_INIT" },
            {0x7F, "NO_INIT" }
        };
    }
}