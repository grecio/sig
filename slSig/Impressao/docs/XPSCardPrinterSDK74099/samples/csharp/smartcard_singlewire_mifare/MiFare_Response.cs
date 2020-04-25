////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// MiFare_Response class.
//
// helper functions to receive MiFare responses from a MiFare chip via the Duali
// reader.
//
////////////////////////////////////////////////////////////////////////////////
using System;

namespace dxp01sdk.mifare {

    public class MiFare_Response {

        public MiFare_Response(byte[] responseBytes) {
            _responseBytes = responseBytes;
        }

        public byte GetStatusByte() {
            return _responseBytes[0];
        }

        public bool IsStatusOK() {
            return (GetStatusWord() == 0x9000);
        }

        public UInt16 GetStatusWord() {
            int length = _responseBytes.Length;
            UInt16 statusWord = (UInt16) ((_responseBytes[length - 2] << 8) + (_responseBytes[length - 1]));
            return statusWord;
        }

        public Tuple<byte, byte> GetSw1Sw2() {
            int length = _responseBytes.Length;
            byte sw1 = _responseBytes[length - 2];
            byte sw2 = _responseBytes[length - 1];
            return new Tuple<byte, byte>(sw1, sw2);
        }

        public bool BlockHasNonzeroData() {
            // after a read of a block, length is 16 data bytes + 3 status bytes:
            if (_responseBytes.Length != 3 + 16) {
                Console.WriteLine("error: length of data block expected to be 16 bytes.");
                return false;
            }

            for (var index = 1; index < 17; index++) {
                if (_responseBytes[index] != 0)
                    return true;
            }
            return false;
        }

        private byte[] _responseBytes;
    }
}