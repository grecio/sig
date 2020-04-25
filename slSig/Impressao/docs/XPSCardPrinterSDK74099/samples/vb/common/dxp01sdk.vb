''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' public dxp01 XPS Card Printer SDK constants
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Namespace dxp01sdk

    Public Structure strings
        ' IBidiSpl request strings:
        Public Const STARTJOB As String = "\Printer.Print:StartJob:Set"
        Public Const ENDJOB As String = "\Printer.Print:EndJob:Set"
        Public Const MAGSTRIPE_READ As String = "\Printer.MagstripeUnit:Back:Read"
        Public Const MAGSTRIPE_ENCODE As String = "\Printer.MagstripeUnit:Back:Encode"
        Public Const MAGSTRIPE_READ_FRONT As String = "\Printer.MagstripeUnit:Front:Read"
        Public Const MAGSTRIPE_ENCODE_FRONT As String = "\Printer.MagstripeUnit:Front:Encode"

        Public Const SMARTCARD_PARK As String = "\Printer.SmartCardUnit:Front:Park"
        Public Const SMARTCARD_PARK_BACK As String = "\Printer.SmartCardUnit:Back:Park"
        Public Const BARCODE_PARK As String = "\Printer.BarcodeUnit:Front:Park"
        Public Const BARCODE_PARK_BACK As String = "\Printer.BarcodeUnit:Back:Park"

        Public Const PRINTER_OPTIONS2 As String = "\Printer.PrinterOptions2:Read"
        Public Const COUNTER_STATUS2 As String = "\Printer.CounterStatus2:Read"
        Public Const SUPPLIES_STATUS As String = "\Printer.SuppliesStatus:Read"
        Public Const SUPPLIES_STATUS2 As String = "\Printer.SuppliesStatus2:Read"
        Public Const SUPPLIES_STATUS3 As String = "\Printer.SuppliesStatus3:Read"
        Public Const PRINTER_MESSAGES As String = "\Printer.PrintMessages:Read"
        Public Const JOB_STATUS As String = "\Printer.JobStatus:Read"
        Public Const PRINTER_ACTION As String = "\Printer.Action:Set"
        Public Const SDK_VERSION As String = "\Printer.SDK:Version"
        Public Const SMARTCARD_CONNECT As String = "\Printer.SmartCardUnit:SingleWire:Connect"
        Public Const SMARTCARD_RECONNECT As String = "\Printer.SmartCardUnit:SingleWire:Reconnect"
        Public Const SMARTCARD_DISCONNECT As String = "\Printer.SmartCardUnit:SingleWire:Disconnect"
        Public Const SMARTCARD_TRANSMIT As String = "\Printer.SmartCardUnit:SingleWire:Transmit"
        Public Const SMARTCARD_STATUS As String = "\Printer.SmartCardUnit:SingleWire:Status"
        Public Const SMARTCARD_CONTROL As String = "\Printer.SmartCardUnit:SingleWire:Control"
        Public Const SMARTCARD_GETATTRIB As String = "\Printer.SmartCardUnit:SingleWire:GetAttrib"
        Public Const CHANGE_LOCK_PASSWORD As String = "\Printer.Locks:ChangePassword:Set"
        Public Const LOCK_PRINTER As String = "\Printer.Locks:ChangeLockState:Set"
        Public Const ACTIVATE_PRINTER As String = "\Printer.ActivatePrinter:Set"
        Public Const CHANGE_PRINTER_STATE As String = "\Printer.ChangePrinterState:Set"
        Public Const RESTART_PRINTER As String = "\Printer.Restart:Set"
        Public Const RESET_CARD_COUNTS As String = "\Printer.ResetCardCount:Set"

        Public Const COLOR_CHANNEL As String = "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}"
        Public Const ADJUST_COLOR As String = "\Printer.AdjustColor:Set"
        Public Const SET_DEFAULT_COLOR As String = "\Printer.SetDefaultColor:Set"

        Public Const ADJUST_COLOR_XML As String = "<?xml version=""1.0""?>" + "<AdjustColor>" + "<RedColorChannel>{0}</RedColorChannel>" + "<GreenColorChannel>{1}</GreenColorChannel>" + "<BlueColorChannel>{2}</BlueColorChannel>" + "</AdjustColor>"

        Public Const DEFAULT_COLOR_XML As String = "<?xml version=""1.0""?>" + "<SetDefaultColor>" + "<RedColorChannel>{0}</RedColorChannel>" + "<GreenColorChannel>{1}</GreenColorChannel>" + "<BlueColorChannel>{2}</BlueColorChannel>" + "</SetDefaultColor>"

        Public Const LASER_ENGRAVE_SETUP_FILE_NAME As String = "\Printer.Laser:Engrave:SetupFileName:Set"
        Public Const LASER_ENGRAVE_TEXT As String = "\Printer.Laser:Engrave:Text:Set"
        Public Const LASER_ENGRAVE_BINARY As String = "\Printer.Laser:Engrave:Binary:Set"
        Public Const LASER_QUERY_SETUP_FILESLIST As String = "\Printer.Laser:SetupFileName:Get"
        Public Const LASER_QUERY_ELEMENT_LIST As String = "\Printer.Laser:ElementList:Get"
        Public Const LASER_UPLOAD_ZIP_FILE_FROM_PRINTER As String = "\Printer.Laser:Upload:File:Get"
        Public Const LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER As String = "\Printer.Laser:Download:File:Set"

        Public Const HOPPER_STATUS As String = "\Printer.Hopper:Status:Get"

        Public Const STATUS_ELEMENT As String = "PrinterStatus"
        Public Const CLIENT_ID_ELEMENT As String = "ClientID"
        Public Const PRINTER_JOB_ID_ELEMENT As String = "PrinterJobID"
        Public Const WINDOWS_JOB_ID_ELEMENT As String = "WindowsJobID"
        Public Const ERROR_CODE_ELEMENT As String = "ErrorCode"
        Public Const ERROR_SEVERITY_ELEMENT As String = "ErrorSeverity"
        Public Const ERROR_STRING_ELEMENT As String = "ErrorString"
        Public Const DATAFROMPRINTER_ELEMENT As String = "DataFromPrinter"
        Public Const PRINTER_ACTION_ELEMENT As String = "PrinterAction"
        Public Const ACTION_ELEMENT As String = "Action"
        Public Const JOB_STATUS_ELEMENT As String = "JobStatus"
        Public Const JOB_STATE_ELEMENT As String = "JobState"
        Public Const JOB_RESTART_COUNT_ELEMENT As String = "JobRestartCount"

        Public Const JOB_NOT_AVAILABLE As String = "NotAvailable"
        Public Const JOB_ACTIVE As String = "JobActive"
        Public Const JOB_SUCCEEDED As String = "JobSucceeded"
        Public Const JOB_FAILED As String = "JobFailed"
        Public Const JOB_CANCELLED As String = "JobCancelled"
        Public Const CARD_READY_TO_RETRIEVE As String = "CardReadyToRetrieve"
        Public Const CARD_NOT_RETRIEVED As String = "CardNotRetrieved"

        Public Const LAMINATOR_BARCODE_READ As String = "\Printer.Laminator:BarcodeRead:Set"
        Public Const LAMINATOR_BARCODE_READ_AND_VERIFY As String = "\Printer.Laminator:BarcodeReadAndVerify:Set"
        Public Const LAMINATOR_BARCODE_READ_DATA As String = "\Printer.Laminator:BarcodeRead:Get"

        Public Const LAMINATOR_BARCODE_READ_XML As String = "<?xml version=""1.0""?>" + "<Barcode>" + "<PrinterJobID>{0}</PrinterJobID>" + "<TimeoutMilliseconds>{1}</TimeoutMilliseconds>" + "</Barcode>"

        Public Const PRINTER_ACTION_XML As String = "<?xml version=""1.0""?>" + "<PrinterAction>" + "<Action>{0}</Action>" + "<PrinterJobID>{1}</PrinterJobID>" + "<ErrorCode>{2}</ErrorCode>" + "</PrinterAction>"

        Public Const JOB_STATUS_XML As String = "<?xml version=""1.0""?>" + "<JobStatus>" + "<PrinterJobID>{0}</PrinterJobID>" + "</JobStatus>"

        Public Const STARTJOB_XML As String = "<?xml version=""1.0""?>" + "<StartJob>" + "<InputHopperSelection>{0}</InputHopperSelection>" + "<CheckPrintRibbonSupplies>{1}</CheckPrintRibbonSupplies>" + "<CheckEmbossSupplies>{2}</CheckEmbossSupplies>" + "<CardEjectSide>{3}</CardEjectSide>" + "</StartJob>"

        Public Const SMARTCARD_CONNECT_XML As String = "<?xml version=""1.0""?>" + "<SmartcardConnect>" + "<PreferredProtocol>{0}</PreferredProtocol>" + "</SmartcardConnect>"

        Public Const SMARTCARD_RECONNECT_XML As String = "<?xml version=""1.0""?>" + "<SmartcardReconnect>" + "<PreferredProtocol>{0}</PreferredProtocol>" + "<Initialization>{1}</Initialization>" + "</SmartcardReconnect>"

        Public Const SMARTCARD_DISCONNECT_XML As String = "<?xml version=""1.0""?>" + "<SmartcardDisconnect>" + "<Disposition>{0}</Disposition>" + "</SmartcardDisconnect>"

        Public Const SMARTCARD_TRANSMIT_XML As String = "<?xml version=""1.0""?>" + "<SmartcardTransmit>" + "<SendBuffer>{0}</SendBuffer>" + "</SmartcardTransmit>"

        Public Const SMARTCARD_GETATTRIB_XML As String = "<?xml version=""1.0""?>" + "<SmartcardGetAttrib>" + "<Attr>{0}</Attr>" + "</SmartcardGetAttrib>"

        Public Const SMARTCARD_CONTROL_XML As String = "<?xml version=""1.0""?>" + "<SmartcardControl>" + "<InBuffer>{0}</InBuffer>" + "</SmartcardControl>"

        Public Const LOCK_PRINTER_XML As String = "<?xml version=""1.0""?>" + "<ChangeLocks>" + "<LockPrinter>{0}</LockPrinter>" + "<CurrentPassword>{1}</CurrentPassword>" + "</ChangeLocks>"

        Public Const ACTIVATE_PRINTER_XML As String = "<?xml version=""1.0""?>" + "<ActivatePrinter>" + "<Activate>{0}</Activate>" + "<Password>{1}</Password>" + "</ActivatePrinter>"

        Public Const CHANGE_PRINTER_STATE_XML As String = "<?xml version=""1.0""?>" + "<ChangePrinterState>" + "<State>{0}</State>" + "</ChangePrinterState>"

        Public Const CHANGE_LOCK_PASSWORD_XML As String = "<?xml version=""1.0""?>" + "<ChangeLocksPassword>" + "<LockPrinter>{0}</LockPrinter>" + "<CurrentPassword>{1}</CurrentPassword>" + "<NextPassword>{2}</NextPassword>" + "</ChangeLocksPassword>"

    End Structure

    Public Enum PrinterState
        Online = 0
        Suspended = 1
        Offline = 2
    End Enum

    Public Enum Actions
        Cancel = 100
        [Resume] = 101
        Restart = 102
    End Enum

    Public Enum laminator_barcode_timout
        INFINITE_WAIT = &H7FFFFFFF
    End Enum

End Namespace
