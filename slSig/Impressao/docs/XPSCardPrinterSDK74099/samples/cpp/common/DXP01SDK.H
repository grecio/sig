////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// public XPS Card Printer SDK constants and API
////////////////////////////////////////////////////////////////////////////////
#pragma once

namespace dxp01sdk
{
    // IBidiSpl request strings:
    const WCHAR STARTJOB[] = L"\\Printer.Print:StartJob:Set";
    const WCHAR ENDJOB[] = L"\\Printer.Print:EndJob:Set";
    const WCHAR MAGSTRIPE_READ[] = L"\\Printer.MagstripeUnit:Back:Read";
    const WCHAR MAGSTRIPE_ENCODE[] = L"\\Printer.MagstripeUnit:Back:Encode";
    const WCHAR MAGSTRIPE_READ_FRONT[] = L"\\Printer.MagstripeUnit:Front:Read";
    const WCHAR MAGSTRIPE_ENCODE_FRONT[] = L"\\Printer.MagstripeUnit:Front:Encode";
    const WCHAR SECONDARY_MAGSTRIPEJIS_READ[] = L"\\Printer.MagstripeUnit2:Back:Read";
    const WCHAR SECONDARY_MAGSTRIPEJIS_ENCODE[] = L"\\Printer.MagstripeUnit2:Back:Encode";
    const WCHAR SECONDARY_MAGSTRIPEJIS_READ_FRONT[] = L"\\Printer.MagstripeUnit2:Front:Read";
    const WCHAR SECONDARY_MAGSTRIPEJIS_ENCODE_FRONT[] = L"\\Printer.MagstripeUnit2:Front:Encode";
    const WCHAR SMARTCARD_PARK[] = L"\\Printer.SmartCardUnit:Front:Park";
    const WCHAR SMARTCARD_PARK_BACK[] = L"\\Printer.SmartCardUnit:Back:Park";
    const WCHAR BARCODE_PARK[] = L"\\Printer.BarcodeUnit:Front:Park";
    const WCHAR BARCODE_PARK_BACK[] = L"\\Printer.BarcodeUnit:Back:Park";
    const WCHAR PRINTER_OPTIONS2[] = L"\\Printer.PrinterOptions2:Read";
    const WCHAR COUNTER_STATUS2[] = L"\\Printer.CounterStatus2:Read";
    const WCHAR SUPPLIES_STATUS3[] = L"\\Printer.SuppliesStatus3:Read";

    const WCHAR PRINTER_MESSAGES[] = L"\\Printer.PrintMessages:Read";
    const WCHAR JOB_STATUS[] = L"\\Printer.JobStatus:Read";
    const WCHAR PRINTER_ACTION[] = L"\\Printer.Action:Set";
    const WCHAR SDK_VERSION[] = L"\\Printer.SDK:Version";
    const WCHAR SMARTCARD_CONNECT[] = L"\\Printer.SmartCardUnit:SingleWire:Connect";
    const WCHAR SMARTCARD_RECONNECT[] = L"\\Printer.SmartCardUnit:SingleWire:Reconnect";
    const WCHAR SMARTCARD_DISCONNECT[] = L"\\Printer.SmartCardUnit:SingleWire:Disconnect";
    const WCHAR SMARTCARD_TRANSMIT[] = L"\\Printer.SmartCardUnit:SingleWire:Transmit";
    const WCHAR SMARTCARD_STATUS[] = L"\\Printer.SmartCardUnit:SingleWire:Status";
    const WCHAR SMARTCARD_CONTROL[] = L"\\Printer.SmartCardUnit:SingleWire:Control";
    const WCHAR SMARTCARD_GETATTRIB[] = L"\\Printer.SmartCardUnit:SingleWire:GetAttrib";
    const WCHAR CHANGE_LOCK_PASSWORD[] = L"\\Printer.Locks:ChangePassword:Set";
    const WCHAR LOCK_PRINTER[] = L"\\Printer.Locks:ChangeLockState:Set";
    const WCHAR ACTIVATE_PRINTER[] = L"\\Printer.ActivatePrinter:Set";
    const WCHAR CHANGE_PRINTER_STATE[] = L"\\Printer.ChangePrinterState:Set";
    const WCHAR RESTART_PRINTER[] = L"\\Printer.Restart:Set";
    const WCHAR RESET_CARD_COUNTS[] = L"\\Printer.ResetCardCount:Set";

    const WCHAR LAMINATOR_BARCODE_READ[] = L"\\Printer.Laminator:BarcodeRead:Set";
    const WCHAR LAMINATOR_BARCODE_READ_AND_VERIFY[] = L"\\Printer.Laminator:BarcodeReadAndVerify:Set";
    const WCHAR LAMINATOR_BARCODE_READ_DATA[] = L"\\Printer.Laminator:BarcodeRead:Get";

    const WCHAR LASER_ENGRAVE_SETUP_FILE_NAME[] = L"\\Printer.Laser:Engrave:SetupFileName:Set";
    const WCHAR LASER_ENGRAVE_TEXT[] = L"\\Printer.Laser:Engrave:Text:Set"; 
    const WCHAR LASER_ENGRAVE_BINARY[] = L"\\Printer.Laser:Engrave:Binary:Set"; 
    const WCHAR LASER_QUERY_SETUP_FILESLIST[] = L"\\Printer.Laser:SetupFileName:Get";
    const WCHAR LASER_QUERY_ELEMENT_LIST[] = L"\\Printer.Laser:ElementList:Get";
    const WCHAR LASER_UPLOAD_ZIP_FILE_FROM_PRINTER[] = L"\\Printer.Laser:Upload:File:Get";
    const WCHAR LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER[] = L"\\Printer.Laser:Download:File:Set";
    const WCHAR ADJUST_COLOR[] = L"\\Printer.AdjustColor:Set";
    const WCHAR SET_DEFAULT_COLOR[] = L"\\Printer.SetDefaultColor:Set";

    const WCHAR HOPPER_STATUS[] = L"\\Printer.Hopper:Status:Get";
    

    // XML element names:
    const WCHAR STATUS_ELEMENT[] = L"PrinterStatus";
    const WCHAR CLIENT_ID_ELEMENT[] = L"ClientID";
    const WCHAR PRINTER_JOB_ID_ELEMENT[] = L"PrinterJobID";
    const WCHAR WINDOWS_JOB_ID_ELEMENT[] = L"WindowsJobID";
    const WCHAR ERROR_CODE_ELEMENT[] = L"ErrorCode";
    const WCHAR ERROR_SEVERITY_ELEMENT[] = L"ErrorSeverity";
    const WCHAR ERROR_STRING_ELEMENT[] = L"ErrorString";
    const WCHAR DATAFROMPRINTER_ELEMENT[] = L"DataFromPrinter";
    const WCHAR PRINTER_ACTION_ELEMENT[] = L"PrinterAction";
    const WCHAR ACTION_ELEMENT[] = L"Action";
    const WCHAR JOB_STATUS_ELEMENT[] = L"JobStatus";
    const WCHAR JOB_STATE_ELEMENT[] = L"JobState";
    const WCHAR JOB_RESTART_COUNT_ELEMENT[] = L"JobRestartCount";

    // Job status strings:
    const WCHAR JOB_NOT_AVAILABLE[] = L"NotAvailable";
    const WCHAR JOB_ACTIVE[] = L"JobActive";
    const WCHAR JOB_SUCCEEDED[] = L"JobSucceeded";
    const WCHAR JOB_FAILED[] = L"JobFailed";
    const WCHAR JOB_CANCELLED[] = L"JobCancelled";
    const WCHAR CARD_READY_TO_RETRIEVE[] = L"CardReadyToRetrieve";
    const WCHAR CARD_NOT_RETRIEVED[] = L"CardNotRetrieved";
    

    const WCHAR PRINTER_ACTION_XML[] = L"<?xml version=\"1.0\"?>"
        L"<PrinterAction>"
        L"<Action>%d</Action>"
        L"<PrinterJobID>%d</PrinterJobID>"
        L"<ErrorCode>%d</ErrorCode>"
        L"</PrinterAction>";

    const WCHAR JOB_STATUS_XML[] = L"<?xml version=\"1.0\"?>"
        L"<JobStatus>"
        L"<PrinterJobID>%d</PrinterJobID>"
        L"</JobStatus>";

    const WCHAR STARTJOB_XML[] = L"<?xml version=\"1.0\"?>"
        L"<StartJob>"
        L"<InputHopperSelection>%ls</InputHopperSelection>"
        L"<CheckPrintRibbonSupplies>%d</CheckPrintRibbonSupplies>"
        L"<CheckEmbossSupplies>%d</CheckEmbossSupplies>"
        L"<CardEjectSide>%ls</CardEjectSide>"
        L"</StartJob>";

    const WCHAR COLOR_CHANNEL[] = L"%ls, %ls, %ls, %ls, %ls, %ls, %ls, %ls, %ls, %ls, %ls";

    const WCHAR ADJUST_COLOR_XML[] = L"<?xml version=\"1.0\"?>"
        L"<AdjustColor>"
        L"<RedColorChannel>%ls</RedColorChannel>"
        L"<GreenColorChannel>%ls</GreenColorChannel>"
        L"<BlueColorChannel>%ls</BlueColorChannel>"
        L"</AdjustColor>";

    const WCHAR DEFAULT_COLOR_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SetDefaultColor>"
        L"<RedColorChannel>%ls</RedColorChannel>"
        L"<GreenColorChannel>%ls</GreenColorChannel>"
        L"<BlueColorChannel>%ls</BlueColorChannel>"
        L"</SetDefaultColor>";

    const int INFINITE_WAIT = 0x7FFFFFFF;

    const WCHAR LAMINATOR_BARCODE_READ_XML[] = L"<?xml version=\"1.0\"?>"
        L"<Barcode>"
        L"<PrinterJobID>%d</PrinterJobID>"
        L"<TimeoutMilliseconds>%d</TimeoutMilliseconds>"
        L"</Barcode>";

    const WCHAR SMARTCARD_CONNECT_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardConnect>"
        L"<PreferredProtocol>%ls</PreferredProtocol>"
        L"</SmartcardConnect>";

    const WCHAR SMARTCARD_RECONNECT_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardReconnect>"
        L"<PreferredProtocol>%ls</PreferredProtocol>"
        L"<Initialization>%ls</Initialization>"
        L"</SmartcardReconnect>";

    const WCHAR SMARTCARD_DISCONNECT_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardDisconnect>"
        L"<Disposition>%ls</Disposition>"
        L"</SmartcardDisconnect>";

    const WCHAR SMARTCARD_TRANSMIT_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardTransmit>"
        L"<SendBuffer>%ls</SendBuffer>"
        L"</SmartcardTransmit>";

    const WCHAR SMARTCARD_GETATTRIB_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardGetAttrib>"
        L"<Attr>%ls</Attr>"
        L"</SmartcardGetAttrib>";

    const WCHAR SMARTCARD_CONTROL_XML[] = L"<?xml version=\"1.0\"?>"
        L"<SmartcardControl>"
        L"<InBuffer>%ls</InBuffer>"
        L"</SmartcardControl>";

    const WCHAR LOCK_PRINTER_XML[] = L"<?xml version=\"1.0\"?>"
        L"<ChangeLocks>"
        L"<LockPrinter>%d</LockPrinter>"
        L"<CurrentPassword>%ls</CurrentPassword>"
        L"</ChangeLocks>";

    const WCHAR ACTIVATE_PRINTER_XML[] = L"<?xml version=\"1.0\"?>"
        L"<ActivatePrinter>"
        L"<Activate>%d</Activate>"
        L"<Password>%ls</Password>"
        L"</ActivatePrinter>";

    const WCHAR CHANGE_PRINTER_STATE_XML[] = L"<?xml version=\"1.0\"?>"
        L"<ChangePrinterState>"
        L"<State>%d</State>"
        L"</ChangePrinterState>";

    const WCHAR CHANGE_LOCK_PASSWORD_XML[] = L"<?xml version=\"1.0\"?>"
        L"<ChangeLocksPassword>"
        L"<LockPrinter>%d</LockPrinter>"
        L"<CurrentPassword>%ls</CurrentPassword>"
        L"<NextPassword>%ls</NextPassword>"
        L"</ChangeLocksPassword>";

    enum PrinterState {
        Online = 0, Suspended = 1, Offline = 2
    };

    enum Actions {
        Cancel = 100,
        Resume = 101,
        Restart = 102
    };

   
    enum MessageSeverity {
        _FIRST_SEVERITY_ENUM = 1,
        _ALERT = 1,
        _CRITICAL = 2,
        _ERROR = 3,
        _WARNING = 4,
        _NOTICE = 5,
        _LAST_SEVERITY_ENUM = 5,

        _UNKNOWN_SEVERITY = -1
    };
}