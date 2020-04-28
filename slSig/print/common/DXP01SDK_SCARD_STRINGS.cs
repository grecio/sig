////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// single-wire smartcard strings.
////////////////////////////////////////////////////////////////////////////////

namespace dxp01sdk {

    public struct scard_strings {
        public const string SMARTCARD_PROTOCOL_CONTACTLESS = "SCARD_PROTOCOL_CL";
        public const string SMARTCARD_PROTOCOL_RAW = "SCARD_PROTOCOL_RAW";
        public const string SMARTCARD_PROTOCOL_T0 = "SCARD_PROTOCOL_T0";
        public const string SMARTCARD_PROTOCOL_T0_OR_T1 = "SCARD_PROTOCOL_T0_OR_T1"; // printer returns this for contact
        public const string SMARTCARD_PROTOCOL_T1 = "SCARD_PROTOCOL_T1";

        public const string SMARTCARD_DISCONNECT_LEAVE = "SCARD_LEAVE_CARD";
        public const string SMARTCARD_DISCONNECT_RESET = "SCARD_RESET_CARD";
        public const string SMARTCARD_DISCONNECT_UNPOWER = "SCARD_UNPOWER_CARD";

        public const string SMARTCARD_STATE_ABSENT = "SCARD_ABSENT";
        public const string SMARTCARD_STATE_NEGOTIABLE = "SCARD_NEGOTIABLE";
        public const string SMARTCARD_STATE_POWERED = "SCARD_POWERED";
        public const string SMARTCARD_STATE_PRESENT = "SCARD_PRESENT";
        public const string SMARTCARD_STATE_SPECIFIC = "SCARD_SPECIFIC";
        public const string SMARTCARD_STATE_SWALLOWED = "SCARD_SWALLOWED";
        public const string SMARTCARD_STATE_UNKNOWN = "SCARD_UNKNOWN";

        public const string SMARTCARD_ATTR_VENDOR_NAME = "SCARD_ATTR_VENDOR_NAME";
        public const string SMARTCARD_ATTR_VENDOR_IFD_TYPE = "SCARD_ATTR_VENDOR_IFD_TYPE";
        public const string SMARTCARD_ATTR_VENDOR_IFD_VERSION = "SCARD_ATTR_VENDOR_IFD_VERSION";
        public const string SMARTCARD_ATTR_VENDOR_IFD_SERIAL_NO = "SCARD_ATTR_VENDOR_IFD_SERIAL_NO";
        public const string SMARTCARD_ATTR_ATR_STRING = "SCARD_ATTR_ATR_STRING";
        public const string SMARTCARD_ATTR_CHANNEL_ID = "SCARD_ATTR_CHANNEL_ID";
        public const string SMARTCARD_ATTR_CHARACTERISTICS = "SCARD_ATTR_CHARACTERISTICS";
        public const string SMARTCARD_ATTR_CURRENT_BWT = "SCARD_ATTR_CURRENT_BWT";
        public const string SMARTCARD_ATTR_CURRENT_CLK = "SCARD_ATTR_CURRENT_CLK";
        public const string SMARTCARD_ATTR_CURRENT_CWT = "SCARD_ATTR_CURRENT_CWT";
        public const string SMARTCARD_ATTR_CURRENT_D = "SCARD_ATTR_CURRENT_D";
        public const string SMARTCARD_ATTR_CURRENT_EBC_ENCODING = "SCARD_ATTR_CURRENT_EBC_ENCODING";
        public const string SMARTCARD_ATTR_CURRENT_F = "SCARD_ATTR_CURRENT_F";
        public const string SMARTCARD_ATTR_CURRENT_IFSC = "SCARD_ATTR_CURRENT_IFSC";
        public const string SMARTCARD_ATTR_CURRENT_IFSD = "SCARD_ATTR_CURRENT_IFSD";
        public const string SMARTCARD_ATTR_CURRENT_IO_STATE = "SCARD_ATTR_CURRENT_IO_STATE";
        public const string SMARTCARD_ATTR_CURRENT_N = "SCARD_ATTR_CURRENT_N";
        public const string SMARTCARD_ATTR_CURRENT_PROTOCOL_TYPE = "SCARD_ATTR_CURRENT_PROTOCOL_TYPE";
        public const string SMARTCARD_ATTR_CURRENT_W = "SCARD_ATTR_CURRENT_W";
        public const string SMARTCARD_ATTR_DEFAULT_CLK = "SCARD_ATTR_DEFAULT_CLK";
        public const string SMARTCARD_ATTR_DEFAULT_DATA_RATE = "SCARD_ATTR_DEFAULT_DATA_RATE";
        public const string SMARTCARD_ATTR_DEVICE_IN_USE = "SCARD_ATTR_DEVICE_IN_USE";
        public const string SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_A = "SCARD_ATTR_DEVICE_SYSTEM_NAME";
        public const string SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_W = SMARTCARD_ATTR_DEVICE_SYSTEM_NAME_A;
        public const string SMARTCARD_ATTR_DEVICE_UNIT = "SCARD_ATTR_DEVICE_UNIT";
        public const string SMARTCARD_ATTR_ESC_AUTHREQUEST = "SCARD_ATTR_ESC_AUTHREQUEST";
        public const string SMARTCARD_ATTR_ESC_CANCEL = "SCARD_ATTR_ESC_CANCEL";
        public const string SMARTCARD_ATTR_ESC_RESET = "SCARD_ATTR_ESC_RESET";
        public const string SMARTCARD_ATTR_EXTENDED_BWT = "SCARD_ATTR_EXTENDED_BWT";
        public const string SMARTCARD_ATTR_ICC_INTERFACE_STATUS = "SCARD_ATTR_ICC_INTERFACE_STATUS";
        public const string SMARTCARD_ATTR_ICC_PRESENCE = "SCARD_ATTR_ICC_PRESENCE";
        public const string SMARTCARD_ATTR_ICC_TYPE_PER_ATR = "SCARD_ATTR_ICC_TYPE_PER_ATR";
        public const string SMARTCARD_ATTR_MAX_CLK = "SCARD_ATTR_MAX_CLK";
        public const string SMARTCARD_ATTR_MAX_DATA_RATE = "SCARD_ATTR_MAX_DATA_RATE";
        public const string SMARTCARD_ATTR_MAX_IFSD = "SCARD_ATTR_MAX_IFSD";
        public const string SMARTCARD_ATTR_MAXINPUT = "SCARD_ATTR_MAXINPUT";
        public const string SMARTCARD_ATTR_POWER_MGMT_SUPPORT = "SCARD_ATTR_POWER_MGMT_SUPPORT";
        public const string SMARTCARD_ATTR_SUPRESS_T1_IFS_REQUEST = "SCARD_ATTR_SUPRESS_T1_IFS_REQUEST";
        public const string SMARTCARD_ATTR_USER_AUTH_INPUT_DEVICE = "SCARD_ATTR_USER_AUTH_INPUT_DEVICE";
        public const string SMARTCARD_ATTR_USER_TO_CARD_AUTH_DEVICE = "SCARD_ATTR_USER_TO_CARD_AUTH_DEVICE";

        public const string SMARTCARD_ERR_E_BAD_SEEK = "SCARD_E_BAD_SEEK";
        public const string SMARTCARD_ERR_E_CANCELLED = "SCARD_E_CANCELLED";
        public const string SMARTCARD_ERR_E_CANT_DISPOSE = "SCARD_E_CANT_DISPOSE";
        public const string SMARTCARD_ERR_E_CARD_UNSUPPORTED = "SCARD_E_CARD_UNSUPPORTED";
        public const string SMARTCARD_ERR_E_CERTIFICATE_UNAVAILABLE = "SCARD_E_CERTIFICATE_UNAVAILABLE";
        public const string SMARTCARD_ERR_E_COMM_DATA_LOST = "SCARD_E_COMM_DATA_LOST";
        public const string SMARTCARD_ERR_E_DIR_NOT_FOUND = "SCARD_E_DIR_NOT_FOUND";
        public const string SMARTCARD_ERR_E_DUPLICATE_READER = "SCARD_E_DUPLICATE_READER";
        public const string SMARTCARD_ERR_E_FILE_NOT_FOUND = "SCARD_E_FILE_NOT_FOUND";
        public const string SMARTCARD_ERR_E_ICC_CREATEORDER = "SCARD_E_ICC_CREATEORDER";
        public const string SMARTCARD_ERR_E_ICC_INSTALLATION = "SCARD_E_ICC_INSTALLATION";
        public const string SMARTCARD_ERR_E_INSUFFICIENT_BUFFER = "SCARD_E_INSUFFICIENT_BUFFER";
        public const string SMARTCARD_ERR_E_INVALID_ATR = "SCARD_E_INVALID_ATR";
        public const string SMARTCARD_ERR_E_INVALID_CHV = "SCARD_E_INVALID_CHV";
        public const string SMARTCARD_ERR_E_INVALID_HANDLE = "SCARD_E_INVALID_HANDLE";
        public const string SMARTCARD_ERR_E_INVALID_PARAMETER = "SCARD_E_INVALID_PARAMETER";
        public const string SMARTCARD_ERR_E_INVALID_TARGET = "SCARD_E_INVALID_TARGET";
        public const string SMARTCARD_ERR_E_INVALID_VALUE = "SCARD_E_INVALID_VALUE";
        public const string SMARTCARD_ERR_E_NO_ACCESS = "SCARD_E_NO_ACCESS";
        public const string SMARTCARD_ERR_E_NO_DIR = "SCARD_E_NO_DIR";
        public const string SMARTCARD_ERR_E_NO_FILE = "SCARD_E_NO_FILE";
        public const string SMARTCARD_ERR_E_NO_KEY_CONTAINER = "SCARD_E_NO_KEY_CONTAINER";
        public const string SMARTCARD_ERR_E_NO_MEMORY = "SCARD_E_NO_MEMORY";
        public const string SMARTCARD_ERR_E_NO_READERS_AVAILABLE = "SCARD_E_NO_READERS_AVAILABLE";
        public const string SMARTCARD_ERR_E_NO_SERVICE = "SCARD_E_NO_SERVICE";
        public const string SMARTCARD_ERR_E_NO_SMARTCARD = "SCARD_E_NO_SMARTCARD";
        public const string SMARTCARD_ERR_E_NO_SUCH_CERTIFICATE = "SCARD_E_NO_SUCH_CERTIFICATE";
        public const string SMARTCARD_ERR_E_NOT_READY = "SCARD_E_NOT_READY";
        public const string SMARTCARD_ERR_E_NOT_TRANSACTED = "SCARD_E_NOT_TRANSACTED";
        public const string SMARTCARD_ERR_E_PCI_TOO_SMALL = "SCARD_E_PCI_TOO_SMALL";
        public const string SMARTCARD_ERR_E_PROTO_MISMATCH = "SCARD_E_PROTO_MISMATCH";
        public const string SMARTCARD_ERR_E_READER_UNAVAILABLE = "SCARD_E_READER_UNAVAILABLE";
        public const string SMARTCARD_ERR_E_READER_UNSUPPORTED = "SCARD_E_READER_UNSUPPORTED";
        public const string SMARTCARD_ERR_E_SERVER_TOO_BUSY = "SCARD_E_SERVER_TOO_BUSY";
        public const string SMARTCARD_ERR_E_SERVICE_STOPPED = "SCARD_E_SERVICE_STOPPED";
        public const string SMARTCARD_ERR_E_SHARING_VIOLATION = "SCARD_E_SHARING_VIOLATION";
        public const string SMARTCARD_ERR_E_SYSTEM_CANCELLED = "SCARD_E_SYSTEM_CANCELLED";
        public const string SMARTCARD_ERR_E_TIMEOUT = "SCARD_E_TIMEOUT";
        public const string SMARTCARD_ERR_E_UNEXPECTED = "SCARD_E_UNEXPECTED";
        public const string SMARTCARD_ERR_E_UNKNOWN_CARD = "SCARD_E_UNKNOWN_CARD";
        public const string SMARTCARD_ERR_E_UNKNOWN_READER = "SCARD_E_UNKNOWN_READER";
        public const string SMARTCARD_ERR_E_UNKNOWN_RES_MNG = "SCARD_E_UNKNOWN_RES_MNG";
        public const string SMARTCARD_ERR_E_UNSUPPORTED_FEATURE = "SCARD_E_UNSUPPORTED_FEATURE";
        public const string SMARTCARD_ERR_E_WRITE_TOO_MANY = "SCARD_E_WRITE_TOO_MANY";
        public const string SMARTCARD_ERR_F_COMM_ERROR = "SCARD_F_COMM_ERROR";
        public const string SMARTCARD_ERR_F_INTERNAL_ERROR = "SCARD_F_INTERNAL_ERROR";
        public const string SMARTCARD_ERR_F_UNKNOWN_ERROR = "SCARD_F_UNKNOWN_ERROR";
        public const string SMARTCARD_ERR_F_WAITED_TOO_LONG = "SCARD_F_WAITED_TOO_LONG";
        public const string SMARTCARD_ERR_P_SHUTDOWN = "SCARD_P_SHUTDOWN";
        public const string SMARTCARD_ERR_S_SUCCESS = "SCARD_S_SUCCESS";
        public const string SMARTCARD_ERR_W_CANCELLED_BY_USER = "SCARD_W_CANCELLED_BY_USER";
        public const string SMARTCARD_ERR_W_CARD_NOT_AUTHENTICATED = "SCARD_W_CARD_NOT_AUTHENTICATED";
        public const string SMARTCARD_ERR_W_CHV_BLOCKED = "SCARD_W_CHV_BLOCKED";
        public const string SMARTCARD_ERR_W_EOF = "SCARD_W_EOF";
        public const string SMARTCARD_ERR_W_REMOVED_CARD = "SCARD_W_REMOVED_CARD";
        public const string SMARTCARD_ERR_W_RESET_CARD = "SCARD_W_RESET_CARD";
        public const string SMARTCARD_ERR_W_SECURITY_VIOLATION = "SCARD_W_SECURITY_VIOLATION";
        public const string SMARTCARD_ERR_W_UNPOWERED_CARD = "SCARD_W_UNPOWERED_CARD";
        public const string SMARTCARD_ERR_W_UNRESPONSIVE_CARD = "SCARD_W_UNRESPONSIVE_CARD";
        public const string SMARTCARD_ERR_W_UNSUPPORTED_CARD = "SCARD_W_UNSUPPORTED_CARD";
        public const string SMARTCARD_ERR_W_WRONG_CHV = "SCARD_W_WRONG_CHV";
    }
}