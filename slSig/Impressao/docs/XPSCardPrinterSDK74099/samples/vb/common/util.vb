''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Printing
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Xml

Namespace dxp01sdk

    ''' <summary>
    ''' we need the printerJobID and and printer error code to properly
    ''' issue a CancelJob() call.
    ''' </summary>
    Public Class BidiException
        Inherits Exception
        Public Sub New(message As String, printerJobID As Integer, errorCode As Integer)
            MyBase.New(message)
            Me.m_errorCode = errorCode
            Me.m_printerJobID = printerJobID
        End Sub

        Public ReadOnly Property PrinterJobID() As Integer
            Get
                Return m_printerJobID
            End Get
        End Property
        Public ReadOnly Property ErrorCode() As Integer
            Get
                Return m_errorCode
            End Get
        End Property

        Private m_printerJobID As Integer
        Private m_errorCode As Integer
    End Class

    Public Class FirmwareVersion
        Public _printerBase As String
        Public _majorVersion As Short
        Public _minorVersion As Short
        Public _deviationVersion As Short
    End Class

    Public Class PrinterStatusValues
        Public _clientID As String
        Public _dataFromPrinter As String
        Public _errorCode As Integer
        Public _errorSeverity As Integer
        Public _errorString As String
        Public _printerJobID As Integer
        Public _windowsJobID As Integer
    End Class

    ''' <summary>
    ''' values parsed from a GetPrinterData(strings.PRINTER_OPTIONS2) call.
    ''' </summary>
    Public Class PrinterOptionsValues
        Public _colorPrintResolution As String
        Public _connectionPortType As String
        Public _connectionProtocol As String
        Public _embosserVersion As String
        Public _laminator As String
        Public _laminatorFirmwareVersion As String
        Public _laminatorImpresser As String
        Public _laminatorScanner As String
        Public _laserFirmwareVersion As String
        Public _lockState As String
        Public _moduleEmbosser As String
        Public _monochromePrintResolution As String
        Public _multiHopperVersion As String
        Public _optionDuplex As String
        Public _optionInputhopper As String
        Public _optionLaser As String
        Public _optionLaserVisionRegistration As String
        Public _optionObscureBlackPanel As String
        Public _optionLocks As String
        Public _optionMagstripe As String
        Public _optionSecondaryMagstripeJIS As String
        Public _optionPrinterBarcodeReader As String
        Public _optionRewrite As String
        Public _optionSmartcard As String
        Public _printEngineType As String
        Public _printerAddress As String
        Public _printerMessageNumber As Integer
        Public _printerModel As String
        Public _printerSerialNumber As String
        Public _printerStatus As String
        Public _printerVersion As String
        Public _printHead As String
        Public _topcoatPrintResolution As String
    End Class

    Public Class PrinterCounterStatus
        Public _cardsPickedSinceCleaningCard As Integer
        Public _cleaningCardsRun As Integer
        Public _currentCompleted As Integer
        Public _currentLost As Integer
        Public _currentPicked As Integer
        Public _currentPickedExceptionSlot As Integer
        Public _currentPickedInputHopper1 As Integer
        Public _currentPickedInputHopper2 As Integer
        Public _currentPickedInputHopper3 As Integer
        Public _currentPickedInputHopper4 As Integer
        Public _currentPickedInputHopper5 As Integer
        Public _currentPickedInputHopper6 As Integer
        Public _currentRejected As Integer
        Public _printerStatus As String
        Public _totalCompleted As Integer
        Public _totalLost As Integer
        Public _totalPicked As Integer
        Public _totalPickedExceptionSlot As Integer
        Public _totalPickedInputHopper1 As Integer
        Public _totalPickedInputHopper2 As Integer
        Public _totalPickedInputHopper3 As Integer
        Public _totalPickedInputHopper4 As Integer
        Public _totalPickedInputHopper5 As Integer
        Public _totalPickedInputHopper6 As Integer
        Public _totalRejected As Integer
    End Class

    Public Class SuppliesValues
        Public _printerStatus As String
        Public _printRibbonType As String
        Public _ribbonRemaining As Short
        Public _printRibbonSerialNumber As String
        Public _printRibbonLotCode As String
        Public _printRibbonPartNumber As Integer
        Public _printRibbonRegionCode As Integer

        Public _retransferFilmSerialNumber As String
        Public _retransferFilmPartNumber As Integer
        Public _retransferFilmLotCode As String
        Public _retransferFilmRemaining As Short

        Public _indentRibbon As String
        Public _indentRibbonRemaining As Short
        Public _indentRibbonSerialNumber As String
        Public _indentRibbonLotCode As String
        Public _indentRibbonPartNumber As Integer

        Public _topperRibbonType As String
        Public _topperRibbonRemaining As Short
        Public _topperRibbonSerialNumber As String
        Public _topperRibbonLotCode As String
        Public _topperRibbonPartNumber As Integer

        Public _laminatorL1SupplyCode As Long
        Public _laminatorL1PercentRemaining As Short
        Public _laminatorL1SerialNumber As String
        Public _laminatorL1LotCode As String
        Public _laminatorL1PartNumber As Integer

        Public _laminatorL2SupplyCode As Long
        Public _laminatorL2PercentRemaining As Short
        Public _laminatorL2SerialNumber As String
        Public _laminatorL2LotCode As String
        Public _laminatorL2PartNumber As Integer
    End Class

    Public Class JobStatusValues
        Public _clientID As String
        Public _jobRestartCount As Integer
        Public _jobState As String
        Public _printerJobID As Integer
        Public _windowsJobID As Integer
    End Class

    Public Class SmartcardResponseValues
        Public _protocol As String
        Public _states As String()
        Public _status As String
        Public _base64string As String
        Public _bytesFromBase64String As Byte()
    End Class

    Public Class LaserStatusValues
        Public _success As Integer
        Public _base64Data As String
    End Class

    Public Class HOPPER_INFORMATION
        Public _name As String
        Public _type As String
        Public _status As String
        Public _cardStock As String
    End Class

    Class Util
        Private Enum PrintSpoolerStatus
            Other = 1
            Unknown
            Idle
            Printing
            WarmingUp
            StoppedPrinting
            Offline
        End Enum

        Public Shared Function StringFromXml(doc As XmlDocument, xmlElementName As String) As String
            Dim node As XmlNode = doc.SelectSingleNode(xmlElementName)
            If node IsNot Nothing Then
                Return node.InnerText
            End If
            Return String.Empty
        End Function

        Public Shared Function IntFromXml(doc As XmlDocument, xmlElementName As String) As Integer
            Dim node As XmlNode = doc.SelectSingleNode(xmlElementName)
            Dim val As Integer = 0
            If node IsNot Nothing Then
                Int32.TryParse(node.InnerText, val)
            End If
            Return val
        End Function
        ''' <summary>
        ''' GetHopperIndex
        ''' ASSUMPTION: hopperId is valid hopper string, i.e.,
        '''     it is either "exception" or "1" - "6"
        ''' </summary>
        ''' <param name="hopperId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetHopperIndex(ByVal hopperId As String) As Integer
            If String.Compare(hopperId, "exception", True) = 0 Then Return 0
            Dim hopperNdx As Integer = 1
            Int32.TryParse(hopperId, hopperNdx)
            Return hopperNdx
        End Function
        Public Shared Function ParseHopperStatusXML(ByVal hopperStatusXML As String, ByVal hopperId As String) As String
            Dim doc As XmlDocument = New XmlDocument()
            doc.LoadXml(hopperStatusXML)
            Dim nodeList As XmlNodeList = doc.SelectNodes("HopperStatus/HopperInformation")
            If nodeList Is Nothing Then
                Throw New Exception("Could not parse hopper status xml.")
            End If

            Dim numHoppers As Integer = nodeList.Count
            Dim hopperIndex As Integer = GetHopperIndex(hopperId)
            If hopperIndex >= numHoppers Then
                Throw New Exception("Hopper id '" & hopperId & "' is out of range of available hoppers.")
            End If

            Dim node As XmlNode = nodeList.Item(hopperIndex)
            If node Is Nothing Then
                Throw New Exception("Xml node corresponding to hopper '" & hopperId & "' could not be retrieved.")
            End If

            Dim xmlAttrCollection As XmlAttributeCollection = node.Attributes
            Dim attr As XmlAttribute = CType((If((xmlAttrCollection IsNot Nothing), xmlAttrCollection.GetNamedItem("Status"), Nothing)), XmlAttribute)
            If attr Is Nothing Then
                Throw New Exception("attribute corresponding to hopper '" & hopperId & "' is null.")
            End If

            Dim hopperStatus As String = attr.Value
            Return hopperStatus
        End Function



        ''' <summary>
        ''' ParsePrinterOptionsXML()
        '''
        ''' we have PrinterOptions2 xml from the printer. Parse the xml and populate
        ''' a new PrinterOptionsValues instance with the data items.
        '''
        '''     <?xml version="1.0"?>
        '''     <!--Printer options2 xml file.-->
        '''     <PrinterInfo2>
        '''         <PrinterStatus> Ready</PrinterStatus>
        '''         <PrinterAddress>10.2.200.224</PrinterAddress>
        '''         <PrinterModel> CE870_Andy</PrinterModel>
        '''         <PrinterSerialNumber> E00111</PrinterSerialNumber>
        '''         <PrinterVersion> D3.17.4-1</PrinterVersion>
        '''         <PrinterMessageNumber>0</PrinterMessageNumber>
        '''         <ConnectionPortType> Network</ConnectionPortType>
        '''         <ConnectionProtocol> Version2</ConnectionProtocol>
        '''         <OptionInputhopper> MultiHopper6</OptionInputhopper>
        '''         <OptionMagstripe> ISO</OptionMagstripe>
        '''         <OptionRewritable> None</OptionRewritable>
        '''         <OptionSecondaryMagstripeJIS> None</OptionSecondaryMagstripeJIS>
        '''         <OptionSmartcard> Installed</OptionSmartcard>
        '''         <OptionDuplex> Auto</OptionDuplex>
        '''         <OptionPrinterBarcodeReader> Installed</OptionPrinterBarcodeReader>
        '''         <OptionLocks> Installed</OptionLocks>
        '''         <LockState> Locked</LockState>
        '''         <PrintEngineType> DirectToCard_DyeSub</PrintEngineType>
        '''         <PrintHead> Installed</PrintHead>
        '''         <ColorPrintResolution>300x300 | 300x600</ColorPrintResolution>
        '''         <MonochromePrintResolution>300x300 | 300x600 | 300x1200</MonochromePrintResolution>
        '''         <TopcoatPrintResolution>300x300</TopcoatPrintResolution>
        '''         <ModuleEmbosser> None</ModuleEmbosser>
        '''         <Laminator> None</Laminator>
        '''         <LaserModule> None</LaserModule>
        '''         <LaserVisionRegistration> None</LaserVisionRegistration>
        '''         <ObscureBlackPanel> None</ObscureBlackPanel>
        '''     </PrinterInfo2>
        '''
        ''' </summary>
        ''' <param name="printerOptionsXML"></param>
        ''' <returns>a populated PrinterOptionsValues struct</returns>
        Public Shared Function ParsePrinterOptionsXML(printerOptionsXML As String) As PrinterOptionsValues
            Dim doc As New XmlDocument()
            doc.LoadXml(printerOptionsXML)

            Dim printerOptionsValues As New PrinterOptionsValues()

            printerOptionsValues._colorPrintResolution = StringFromXml(doc, "PrinterInfo2/ColorPrintResolution")
            printerOptionsValues._connectionPortType = StringFromXml(doc, "PrinterInfo2/ConnectionPortType")
            printerOptionsValues._connectionProtocol = StringFromXml(doc, "PrinterInfo2/ConnectionProtocol")
            printerOptionsValues._embosserVersion = StringFromXml(doc, "PrinterInfo2/EmbosserVersion")
            printerOptionsValues._lockState = StringFromXml(doc, "PrinterInfo2/LockState")
            printerOptionsValues._moduleEmbosser = StringFromXml(doc, "PrinterInfo2/ModuleEmbosser")
            printerOptionsValues._monochromePrintResolution = StringFromXml(doc, "PrinterInfo2/MonochromePrintResolution")
            printerOptionsValues._optionDuplex = StringFromXml(doc, "PrinterInfo2/OptionDuplex")
            printerOptionsValues._optionInputhopper = StringFromXml(doc, "PrinterInfo2/OptionInputhopper")
            printerOptionsValues._optionLaser = StringFromXml(doc, "PrinterInfo2/LaserModule")
            printerOptionsValues._optionLaserVisionRegistration = StringFromXml(doc, "PrinterInfo2/LaserVisionRegistration")
            printerOptionsValues._optionObscureBlackPanel = StringFromXml(doc, "PrinterInfo2/ObscureBlackPanel")
            printerOptionsValues._optionLocks = StringFromXml(doc, "PrinterInfo2/OptionLocks")
            printerOptionsValues._optionMagstripe = StringFromXml(doc, "PrinterInfo2/OptionMagstripe")
            printerOptionsValues._optionSecondaryMagstripeJIS = StringFromXml(doc, "PrinterInfo2/OptionSecondaryMagstripeJIS")
            printerOptionsValues._optionPrinterBarcodeReader = StringFromXml(doc, "PrinterInfo2/OptionPrinterBarcodeReader")
            printerOptionsValues._optionSmartcard = StringFromXml(doc, "PrinterInfo2/OptionSmartcard")
            printerOptionsValues._printerAddress = StringFromXml(doc, "PrinterInfo2/PrinterAddress")
            printerOptionsValues._printerMessageNumber = IntFromXml(doc, "PrinterInfo2/PrinterMessageNumber")
            printerOptionsValues._printerModel = StringFromXml(doc, "PrinterInfo2/PrinterModel")
            printerOptionsValues._printerSerialNumber = StringFromXml(doc, "PrinterInfo2/PrinterSerialNumber")
            printerOptionsValues._printerStatus = StringFromXml(doc, "PrinterInfo2/PrinterStatus")
            printerOptionsValues._printerVersion = StringFromXml(doc, "PrinterInfo2/PrinterVersion")
            printerOptionsValues._printHead = StringFromXml(doc, "PrinterInfo2/PrintHead")
            printerOptionsValues._optionRewrite = StringFromXml(doc, "PrinterInfo2/OptionRewritable")
            printerOptionsValues._optionPrinterBarcodeReader = StringFromXml(doc, "PrinterInfo2/OptionPrinterBarcodeReader")
            printerOptionsValues._topcoatPrintResolution = StringFromXml(doc, "PrinterInfo2/TopcoatPrintResolution")

            printerOptionsValues._laminatorFirmwareVersion = StringFromXml(doc, "PrinterInfo2/LaminatorFirmwareVersion")
            printerOptionsValues._laminator = StringFromXml(doc, "PrinterInfo2/Laminator")
            printerOptionsValues._laminatorImpresser = StringFromXml(doc, "PrinterInfo2/LaminatorImpresser")
            printerOptionsValues._laminatorScanner = StringFromXml(doc, "PrinterInfo2/LaminatorScanner")

            printerOptionsValues._laserFirmwareVersion = StringFromXml(doc, "PrinterInfo2/LaserVersion")
            printerOptionsValues._multiHopperVersion = StringFromXml(doc, "PrinterInfo2/MultiHopperVersion")

            printerOptionsValues._printEngineType = StringFromXml(doc, "PrinterInfo2/PrintEngineType")


            Return printerOptionsValues
        End Function

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' ParsePrinterCounterStatusXML()
        '
        ' the 'printer card counts' xml from the printer looks something like this:
        '
        '    <?xml version="1.0"?>
        '    <!--Printer counter2 xml file.-->
        '    <CounterStatus2>
        '       <CardsPickedSinceCleaningCard>474</CardsPickedSinceCleaningCard>
        '       <CleaningCardsRun>3</CleaningCardsRun>
        '       <CurrentCompleted>260</CurrentCompleted>
        '       <CurrentLost>0</CurrentLost>
        '       <CurrentPicked>474</CurrentPicked>
        '       <CurrentPickedExceptionSlot>2</CurrentPickedExceptionSlot>
        '       <CurrentPickedInputHopper1>242</CurrentPickedInputHopper1>
        '       <CurrentPickedInputHopper2>1</CurrentPickedInputHopper2>
        '       <CurrentPickedInputHopper3>33</CurrentPickedInputHopper3>
        '       <CurrentPickedInputHopper4>14</CurrentPickedInputHopper4>
        '       <CurrentPickedInputHopper5>3</CurrentPickedInputHopper5>
        '       <CurrentPickedInputHopper6>1</CurrentPickedInputHopper6>
        '       <CurrentRejected>36</CurrentRejected>
        '       <PrinterStatus>Ready</PrinterStatus>
        '       <TotalCompleted>260</TotalCompleted>
        '       <TotalLost>0</TotalLost>
        '       <TotalPicked>474</TotalPicked>
        '       <TotalPickedExceptionSlot>2</TotalPickedExceptionSlot>
        '       <TotalPickedInputHopper1>242</TotalPickedInputHopper1>
        '       <TotalPickedInputHopper2>1</TotalPickedInputHopper2>
        '       <TotalPickedInputHopper3>33</TotalPickedInputHopper3>
        '       <TotalPickedInputHopper4>14</TotalPickedInputHopper4>
        '       <TotalPickedInputHopper5>3</TotalPickedInputHopper5>
        '       <TotalPickedInputHopper6>1</TotalPickedInputHopper6>
        '       <TotalRejected>36</TotalRejected>
        '    </CounterStatus2>
        '
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Shared Function ParsePrinterCounterStatusXML(printercardCountXML As String) As PrinterCounterStatus
            Dim doc As New XmlDocument()
            doc.LoadXml(printercardCountXML)

            Dim printerCardCountValues As New PrinterCounterStatus()

            printerCardCountValues._cardsPickedSinceCleaningCard = IntFromXml(doc, "CounterStatus2/CardsPickedSinceCleaningCard")
            printerCardCountValues._cleaningCardsRun = IntFromXml(doc, "CounterStatus2/CleaningCardsRun")
            printerCardCountValues._currentCompleted = IntFromXml(doc, "CounterStatus2/CurrentCompleted")
            printerCardCountValues._currentLost = IntFromXml(doc, "CounterStatus2/CurrentLost")
            printerCardCountValues._currentPicked = IntFromXml(doc, "CounterStatus2/CurrentPicked")
            printerCardCountValues._currentPickedExceptionSlot = IntFromXml(doc, "CounterStatus2/CurrentPickedExceptionSlot")
            printerCardCountValues._currentPickedInputHopper1 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper1")
            printerCardCountValues._currentPickedInputHopper2 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper2")
            printerCardCountValues._currentPickedInputHopper3 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper3")
            printerCardCountValues._currentPickedInputHopper4 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper4")
            printerCardCountValues._currentPickedInputHopper5 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper5")
            printerCardCountValues._currentPickedInputHopper6 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper6")
            printerCardCountValues._currentRejected = IntFromXml(doc, "CounterStatus2/CurrentRejected")
            printerCardCountValues._printerStatus = StringFromXml(doc, "CounterStatus2/PrinterStatus")
            printerCardCountValues._totalCompleted = IntFromXml(doc, "CounterStatus2/TotalCompleted")
            printerCardCountValues._totalLost = IntFromXml(doc, "CounterStatus2/TotalLost")
            printerCardCountValues._totalPicked = IntFromXml(doc, "CounterStatus2/TotalPicked")
            printerCardCountValues._totalPickedExceptionSlot = IntFromXml(doc, "CounterStatus2/TotalPickedExceptionSlot")
            printerCardCountValues._totalPickedInputHopper1 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper1")
            printerCardCountValues._totalPickedInputHopper2 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper2")
            printerCardCountValues._totalPickedInputHopper3 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper3")
            printerCardCountValues._totalPickedInputHopper4 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper4")
            printerCardCountValues._totalPickedInputHopper5 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper5")
            printerCardCountValues._totalPickedInputHopper6 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper6")
            printerCardCountValues._totalRejected = IntFromXml(doc, "CounterStatus2/TotalRejected")

            Return printerCardCountValues
        End Function

        Public Shared Function ParseSuppliesXML(suppliesXML As String) As SuppliesValues

            ' sample supplies xml from printer:
            '
            ' <?xml version="1.0"?>
            ' <!--Printer Supplies3 xml file.-->
            ' <PrinterSupplies3>
            '   <PrinterStatus>Ready</PrinterStatus>
            '   <Printer>
            '     <PrintRibbon>Installed</PrintRibbon>
            '     <PrintRibbonType>Monochrome</PrintRibbonType>
            '     <RibbonLotCode>16MAR12  </RibbonLotCode>
            '     <RibbonPartNumber>533000052</RibbonPartNumber>
            '     <RibbonRemaining>43</RibbonRemaining>
            '     <RibbonSerialNumber>E0055000008E9956</RibbonSerialNumber>
            '   </Printer>
            '   <Embosser>
            '     <IndentRibbon>Installed</IndentRibbon>
            '     <IndentRibbonLotCode/>
            '     <IndentRibbonPartNumber/>
            '     <IndentRibbonRemaining>50</IndentRibbonRemaining>
            '     <IndentRibbonSerialNumber/>
            '     <IndentRibbonType>67174401</IndentRibbonType>
            '     <TopperRibbon>Installed</TopperRibbon>
            '     <TopperRibbonLotCode>29Oct11  </TopperRibbonLotCode>
            '     <TopperRibbonPartNumber>504139013</TopperRibbonPartNumber>
            '     <TopperRibbonRemaining>25</TopperRibbonRemaining>
            '     <TopperRibbonSerialNumber>25A18E00005005E0</TopperRibbonSerialNumber>
            '     <TopperRibbonType>Silver</TopperRibbonType>
            '   </Embosser>
            '   <Laminator>
            '     <L1Laminate>None</L1Laminate>
            '     <L1LaminateLotCode/>
            '     <L1LaminatePartNumber/>
            '     <L1LaminateRemaining/>
            '     <L1LaminateSerialNumber/>
            '     <L1LaminateType/>
            '     <L2Laminate>None</L2Laminate>
            '     <L2LaminateLotCode/>
            '     <L2LaminatePartNumber/>
            '     <L2LaminateRemaining/>
            '     <L2LaminateSerialNumber/>
            '     <L2LaminateType/>
            '   </Laminator>
            ' </PrinterSupplies3>

            Dim doc As New XmlDocument()
            doc.LoadXml(suppliesXML)

            Dim suppliesValues As New SuppliesValues()

            suppliesValues._indentRibbon = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbon")
            suppliesValues._indentRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonLotCode")
            suppliesValues._indentRibbonPartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonPartNumber"))
            suppliesValues._indentRibbonRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonRemaining"))
            suppliesValues._indentRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonSerialNumber")
            suppliesValues._laminatorL1LotCode = StringFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateLotCode")
            suppliesValues._laminatorL1PartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminatePartNumber"))
            suppliesValues._laminatorL1PercentRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateRemaining"))
            suppliesValues._laminatorL1SerialNumber = StringFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateSerialNumber")
            suppliesValues._laminatorL1SupplyCode = CLng(IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateType"))
            suppliesValues._laminatorL2LotCode = StringFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateLotCode")
            suppliesValues._laminatorL2PartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminatePartNumber"))
            suppliesValues._laminatorL2PercentRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateRemaining"))
            suppliesValues._laminatorL2SerialNumber = StringFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateSerialNumber")
            suppliesValues._laminatorL2SupplyCode = CLng(IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateType"))
            suppliesValues._printerStatus = StringFromXml(doc, "PrinterSupplies3/PrinterStatus")
            suppliesValues._printRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Printer/RibbonLotCode")
            suppliesValues._printRibbonPartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Printer/RibbonPartNumber"))
            suppliesValues._printRibbonRegionCode = CInt(IntFromXml(doc, "PrinterSupplies3/Printer/RibbonRegionCode"))
            suppliesValues._printRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Printer/RibbonSerialNumber")
            suppliesValues._printRibbonType = StringFromXml(doc, "PrinterSupplies3/Printer/PrintRibbonType")
            suppliesValues._ribbonRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Printer/RibbonRemaining"))
            suppliesValues._topperRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonLotCode")
            suppliesValues._topperRibbonPartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonPartNumber"))
            suppliesValues._topperRibbonRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonRemaining"))
            suppliesValues._topperRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonSerialNumber")
            suppliesValues._topperRibbonType = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonType")

            suppliesValues._retransferFilmSerialNumber = StringFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmSerialNumber")
            suppliesValues._retransferFilmPartNumber = CInt(IntFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmPartNumber"))
            suppliesValues._retransferFilmLotCode = StringFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmLotCode")
            suppliesValues._retransferFilmRemaining = CShort(IntFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmPercentRemaining"))
           Return suppliesValues
        End Function

        ''' <summary>
        ''' populate values from this xml returned from the printer:
        ''' <?xml version="1.0"?>
        ''' <SDKVersion>2.1.217.0</SDKVersion>
        ''' </summary>
        Public Shared Function ParseDriverVersionXML(driverVersionXML As String) As String
            Dim driverVersion As String = String.Empty
            Dim doc As New XmlDocument()
            doc.LoadXml(driverVersionXML)
            Try
                Dim node As XmlNode = doc.SelectSingleNode("SDKVersion")
                driverVersion = node.InnerText
                ' note: driver version not returned in previous drivers.
            Catch e As Exception
            End Try
            Return driverVersion
        End Function

        ''' <summary>
        ''' sample xml printer status xml:
        '''
        '''     <!--Printer status xml file.-->
        '''     <PrinterStatus>
        '''         <ClientID>fellmadwin7x64_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
        '''         <DataFromPrinter><![CDATA[  ]]></DataFromPrinter>
        '''         <ErrorCode>0</ErrorCode>
        '''         <ErrorSeverity>0</ErrorSeverity>
        '''         <ErrorString></ErrorString>
        '''         <PrinterJobID>792</PrinterJobID>
        '''         <WindowsJobID>0</WindowsJobID>
        '''     </PrinterStatus>
        '''
        ''' </summary>
        Public Shared Function ParsePrinterStatusXML(printerStatusXML As String) As PrinterStatusValues
            Dim doc As New XmlDocument()
            doc.LoadXml(printerStatusXML)

            Dim printerStatusValues As New PrinterStatusValues()

            printerStatusValues._printerJobID = IntFromXml(doc, "PrinterStatus/PrinterJobID")
            printerStatusValues._clientID = StringFromXml(doc, "PrinterStatus/ClientID")
            printerStatusValues._dataFromPrinter = StringFromXml(doc, "PrinterStatus/DataFromPrinter")
            printerStatusValues._errorCode = IntFromXml(doc, "PrinterStatus/ErrorCode")
            printerStatusValues._errorSeverity = IntFromXml(doc, "PrinterStatus/ErrorSeverity")
            printerStatusValues._errorString = StringFromXml(doc, "PrinterStatus/ErrorString")
            printerStatusValues._windowsJobID = IntFromXml(doc, "PrinterStatus/WindowsJobID")

            Return printerStatusValues
        End Function

        Public Shared Function ParseJobStatusXML(jobStatusValues As String) As JobStatusValues
            Dim doc As New XmlDocument()
            doc.LoadXml(jobStatusValues)

            Dim jobStatusXML As New JobStatusValues()

            jobStatusXML._clientID = StringFromXml(doc, "JobStatus/ClientID")
            jobStatusXML._jobRestartCount = IntFromXml(doc, "JobStatus/JobRestartCount")
            jobStatusXML._jobState = StringFromXml(doc, "JobStatus/JobState")
            jobStatusXML._printerJobID = IntFromXml(doc, "JobStatus/PrinterJobID")
            jobStatusXML._windowsJobID = IntFromXml(doc, "JobStatus/WindowsJobID")

            Return jobStatusXML
        End Function

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ' ParseMagstripeStrings()
        '
        ' we're given a Printer Status XML fragment that has a CDATA section like this:
        '
        '   <DataFromPrinter>
        '      <![CDATA[<?xml version="1.0" encoding="UTF-8"?>
        '      <magstripe xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope"
        '         xmlns:SOAP-ENC="http://www.w3.org/2003/05/soap-encoding"
        '         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        '         xmlns:xsd="http://www.w3.org/2001/XMLSchema"
        '         xmlns:DPCLMagStripe="urn:dpcl:magstripe:2010-01-19"
        '         xsi:type="DPCLMagStripe:MagStripe"
        '         SOAP-ENV:encodingStyle="http://www.w3.org/2003/05/soap-encoding">
        '         <track number="1">
        '            <base64Data>R1JPVU5E</base64Data>
        '         </track>
        '         <track number="2">
        '            <base64Data>MTIzNA==</base64Data>
        '         </track>
        '         <track number="3">
        '            <base64Data>MzIx</base64Data>
        '         </track>
        '      </magstripe>]]>
        '   </DataFromPrinter>
        '
        ' return three strings. the string data is still Base64 encoded.
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Public Shared Sub ParseMagstripeStrings(printerStatusXML As String, ByRef track1 As String, ByRef track2 As String, ByRef track3 As String, jisRequest As Boolean)
            Dim doc As New XmlDocument()
            doc.LoadXml(printerStatusXML)

            Dim node As XmlNode
            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()")
            Dim cdataText As String = node.InnerText

            If cdataText.Length > 10 Then
                ' Make sure there is valid data before going further.
                Dim magstripeDoc As New XmlDocument()
                magstripeDoc.LoadXml(cdataText)

                Dim nodelist As XmlNodeList = magstripeDoc.SelectNodes("magstripe/track")
                For Each track As XmlNode In nodelist
                    Dim attrColl As XmlAttributeCollection = track.Attributes
                    Dim attr As XmlAttribute = DirectCast(attrColl.GetNamedItem("number"), XmlAttribute)
                    Dim trackNumber As Integer = Convert.ToInt32(attr.Value)
                    If jisRequest AndAlso trackNumber <> 3 Then
                        ' only track 3 is valid for JIS request
                        Continue For
                    End If
                    Select Case trackNumber

                        Case 1
                            track1 = track.InnerText
                            Exit Select

                        Case 2
                            track2 = track.InnerText
                            Exit Select

                        Case 3
                            track3 = track.InnerText
                            Exit Select
                    End Select
                Next
            End If
        End Sub

        ''' <summary>
        ''' repeatedly get the printer status until with printer status contains
        ''' a valid windows jobID.
        ''' </summary>
        ''' <returns>zero if any error detected, or the windows jobID</returns>
        Public Shared Function WaitForWindowsJobID(bidiSpl As BidiSplWrap, printerName As String) As Integer

            Const sleepMilliSeconds As Integer = 1000

            Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES)
            Dim printerStatus As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

            Do

                If 0 <> printerStatus._windowsJobID Then
                    Console.WriteLine(String.Format("returning windowsJobID {0}.", printerStatus._windowsJobID))
                    Return printerStatus._windowsJobID
                End If

                Console.WriteLine(String.Format("waiting {0} millseconds for windows jobID", sleepMilliSeconds))
                Thread.Sleep(sleepMilliSeconds)

                printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES)

                printerStatus = Util.ParsePrinterStatusXML(printerStatusXML)
            Loop While 0 = printerStatus._errorCode

            Console.WriteLine("printer error:")
            Console.WriteLine(printerStatusXML)

            Return 0
        End Function

        Private Shared Sub OnPrintPage(sender As Object, pageEventArgs As PrintPageEventArgs)
            Using font As New Font("Arial", 8)
                Using brush As New SolidBrush(Color.Black)
                    Using pen As New Pen(Color.DarkBlue)
                        pageEventArgs.Graphics.DrawString("Sample Text", font, brush, 50, 50)
                        pageEventArgs.Graphics.DrawRectangle(pen, 50, 70, 20, 25)
                    End Using
                End Using
            End Using
        End Sub

        Public Shared Sub PrintTextAndGraphics(bidiSpl As BidiSplWrap, printerName As String)
            Dim printDocument As New PrintDocument()
            printDocument.PrintController = New StandardPrintController()
            printDocument.PrinterSettings.PrinterName = printerName
            printDocument.DocumentName = Path.GetFileName(Environment.GetCommandLineArgs()(0)) & " printing"
            AddHandler printDocument.PrintPage, New PrintPageEventHandler(AddressOf OnPrintPage)
            printDocument.Print()

            ' we wait here - to ensure that the job is truly in the windows
            ' spooler:
            Dim windowsJobID As Integer = WaitForWindowsJobID(bidiSpl, printerName)
        End Sub

        Private Shared Sub OnPrintPageWithBlockingEscapes(pageEventArgs As PrintPageEventArgs, printBlockingEscape As String, topcoatRemovalEscape As String)
            Using font As New Font("Arial", 8)
                Using brush As New SolidBrush(Color.Black)
                    Using pen As New Pen(Color.DarkBlue)
                        If printBlockingEscape.Length > 0 Then
                            pageEventArgs.Graphics.DrawString(printBlockingEscape, font, brush, 2, 2)
                        End If
                        If topcoatRemovalEscape.Length > 0 Then
                            pageEventArgs.Graphics.DrawString(topcoatRemovalEscape, font, brush, 2, 20)
                        End If
                        pageEventArgs.Graphics.DrawString("Sample Text", font, brush, 50, 50)
                        pageEventArgs.Graphics.DrawRectangle(pen, 50, 70, 20, 25)
                    End Using
                End Using
            End Using
        End Sub
        Public Shared Sub PrintTextAndGraphics(bidiSpl As BidiSplWrap, printerName As String, printBlockingEscape As String, topcoatRemovalEscape As String)
            Dim printDocument As New PrintDocument()
            printDocument.PrintController = New StandardPrintController()
            printDocument.PrinterSettings.PrinterName = printerName
            printDocument.DocumentName = Path.GetFileName(Environment.GetCommandLineArgs()(0)) & " printing"
            AddHandler printDocument.PrintPage, Sub(sender, args)
                                                    OnPrintPageWithBlockingEscapes(args, printBlockingEscape, topcoatRemovalEscape)
                                                End Sub
            printDocument.Print()

        ' we wait here - to ensure that the job is truly in the windows
        ' spooler:
        Dim windowsJobID As Integer = WaitForWindowsJobID(bidiSpl, printerName)
        End Sub

        Public Shared Sub DisplayPrintTicket(printTicket As PrintTicket)
            Dim memStream As MemoryStream = printTicket.GetXmlStream()
            Dim xml As String = Encoding.ASCII.GetString(memStream.GetBuffer(), 0, CInt(memStream.Length))
            Console.WriteLine(xml)
        End Sub

        ''' <summary>
        ''' incoming xml looks like this:
        '''
        '''  <?xml version="1.0" ?>
        '''  <!-- Printer status xml file.-->
        '''  <PrinterStatus>
        '''    <ClientID>myMachineName_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
        '''    <WindowsJobID>0</WindowsJobID>
        '''    <PrinterJobID>8446</PrinterJobID>
        '''    <ErrorCode>0</ErrorCode>
        '''    <ErrorSeverity>0</ErrorSeverity>
        '''    <ErrorString />
        '''    <DataFromPrinter>
        '''      <![CDATA[ <?xml version="1.0"?>
        '''      <!--smartcard response xml-->
        '''      <SmartcardResponse>
        '''      <Protocol>SCARD_PROTOCOL_RAW</Protocol>
        '''      <State>  </State>
        '''      <Status>SCARD_S_SUCCESS</Status>
        '''      <Base64Data> </Base64Data>
        '''      </SmartcardResponse>]]>
        '''    </DataFromPrinter>
        '''  </PrinterStatus>
        '''
        ''' we retrieve the single-wire smartcard response data from the CDATA section.
        '''
        ''' </summary>
        ''' <param name="printerStatusXml"></param>
        ''' <returns>a populated SmartcardResponseValues object populated from the xml markup.</returns>
        Public Shared Function ParseSmartcardResponseXML(printerStatusXml As String) As SmartcardResponseValues
            Dim smartcardResponseValues As SmartcardResponseValues = New SmartcardResponseValues()

            Dim doc As New XmlDocument()
            doc.LoadXml(printerStatusXml)

            Dim node As XmlNode

            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()")
            Dim cdataText As String = node.InnerText

            Dim smartcardResponseDoc As New XmlDocument()
            smartcardResponseDoc.LoadXml(cdataText)

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Protocol")
            smartcardResponseValues._protocol = node.InnerText.Trim()

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/State")
            Dim raw_statestring As String = node.InnerText.Trim()
            Dim statesList As List(Of String) = New List(Of String)()
            For Each stateString As String In raw_statestring.Split(New Char() {"|"c, " "c})
                If Not String.IsNullOrWhiteSpace(stateString) Then
                    statesList.Add(stateString)
                End If
            Next
            smartcardResponseValues._states = statesList.ToArray()

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Status")
            smartcardResponseValues._status = node.InnerText.Trim()

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Base64Data")
            smartcardResponseValues._base64string = node.InnerText.Trim()

            If String.IsNullOrEmpty(smartcardResponseValues._base64string) Then
                smartcardResponseValues._bytesFromBase64String = New Byte(-1) {}
            Else
                smartcardResponseValues._bytesFromBase64String = Convert.FromBase64String(smartcardResponseValues._base64string)
            End If

            Return smartcardResponseValues
        End Function

        <DllImport("kernel32.dll")>
        Private Shared Function FormatMessage(
            dwFlags As UInteger,
            lpSource As IntPtr,
            dwMessageId As UInteger,
            dwLanguageId As UInteger,
            <Out> lpBuffer As StringBuilder,
            nSize As UInteger,
            Arguments As IntPtr) As UInteger
        End Function

        ''' <summary>
        ''' get a string result...like the Win32 FormatMessage() function.
        ''' </summary>
        ''' <param name="win32error">a numeric win32 error code</param>
        ''' <returns>a localized string corresponding to the given error code</returns>
        Public Shared Function Win32ErrorString(win32error As UInteger) As String
            Const FORMAT_MESSAGE_IGNORE_INSERTS As UInteger = &H200
            Const FORMAT_MESSAGE_FROM_SYSTEM As UInteger = &H1000
            Dim sb As StringBuilder = New StringBuilder(1024)
            FormatMessage(
                FORMAT_MESSAGE_FROM_SYSTEM Or FORMAT_MESSAGE_IGNORE_INSERTS,
                IntPtr.Zero,
                win32error,
                0,
                sb,
                1024,
                IntPtr.Zero)
            Dim errorString As String = sb.ToString()
            Return errorString
        End Function

        Public Shared Function GetCurrentTimeString() As String
            Dim now As DateTime = DateTime.Now
            Return now.ToString("yyyy:MM:dd hh:mm:ss:fff")
        End Function

        ''' <summary>
        ''' Cancel a job started by StartJob().
        ''' do not throw any exceptions.
        ''' </summary>
        Public Shared Sub CancelJob(bidiSpl As BidiSplWrap, printerJobID As Integer, errorCode As Integer)
            Dim xmlFormat As String = strings.PRINTER_ACTION_XML
            Dim input As String = String.Format(xmlFormat, CInt(Actions.Cancel), printerJobID, errorCode)
            bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input)
        End Sub

        ''' <summary>
        '''
        ''' Issue an IBidi StartJob with xml markup to start an IBidi print job.
        '''
        ''' We do this to have the printer check for supplies, or, just to obtain the
        ''' printerJobID for job completion polling later.
        '''
        ''' If the printer is out of supplies, throw an exception.
        '''
        ''' </summary>
        Public Shared Function StartJob(bidiSpl As BidiSplWrap, Optional hopperID As String = "1", Optional cardEjectSide As String = "default") As Integer

            Const pollSleepMilliseconds As Integer = 2000

            Dim printerJobID As Integer = 0

            Do
                Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
                Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

                ' CheckPrintRibbonSupplies is deprecated. Use Printer.SuppliesStatus3:Read to retrieve supplies 
                ' CheckEmbossSupplies is deprecated. Use Printer.SuppliesStatus3:Read to retrieve supplies status  
                Dim startJobXml As String = String.Format(strings.STARTJOB_XML, hopperID, False, False, cardEjectSide)

                Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.STARTJOB, startJobXml)
                Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

                If 506 = printerStatusValues._errorCode Then

                    ' Printer cannot accept another job as it is busy. Try again.
                    Console.WriteLine("StartJob(): {0} Trying again.", printerStatusValues._errorString)

                    ' let the current card process in the printer:
                    Thread.Sleep(pollSleepMilliseconds)
                ElseIf 0 <> printerStatusValues._errorCode Then
                    Dim message As String = String.Format("StartJob(): {0} {1}", printerStatusValues._errorCode, printerStatusValues._errorString)
                    Throw New BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode)
                Else
                    Console.WriteLine("Printer job ID: {0} started.", printerStatusValues._printerJobID)
                    printerJobID = printerStatusValues._printerJobID

                End If
            Loop While 0 = printerJobID

            Return printerJobID
        End Function

        ''' <summary>
        ''' repeatedly check job status until the job is complete - failed or not.
        ''' </summary>
        Public Shared Sub PollForJobCompletion(bidiSpl As BidiSplWrap, printerJobID As Integer)

            Const pollSleepMilliseconds As Integer = 2000

            While True

                Dim jobStatusRequest As String = strings.JOB_STATUS_XML
                Dim jobInfoXML As String = String.Format(jobStatusRequest, printerJobID)
                Dim jobStatusXML As String = bidiSpl.GetPrinterData(strings.JOB_STATUS, jobInfoXML)
                Dim jobStatusValues As JobStatusValues = Util.ParseJobStatusXML(jobStatusXML)

                Console.WriteLine("Printer job ID: {0} {1}", printerJobID, jobStatusValues._jobState)

                If (jobStatusValues._jobState = strings.JOB_SUCCEEDED Or
                    jobStatusValues._jobState = strings.JOB_FAILED Or
                    jobStatusValues._jobState = strings.JOB_CANCELLED Or
                    jobStatusValues._jobState = strings.CARD_NOT_RETRIEVED Or
                    jobStatusValues._jobState = strings.JOB_NOT_AVAILABLE) Then
                    Return
                End If

                Dim printerStatusXML As String = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES)
                Dim printerStatusValues As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXML)

                If (0 <> printerStatusValues._errorCode) AndAlso (printerJobID = printerStatusValues._printerJobID) Then
                    Dim message As String = String.Format("{0} severity: {1}", printerStatusValues._errorString, printerStatusValues._errorSeverity)
                    Throw New BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode)
                End If

                Thread.Sleep(pollSleepMilliseconds)
            End While
        End Sub
        Public Shared Function GetExePath() As String
            Dim path As String = System.Reflection.Assembly.GetExecutingAssembly().Location
            Dim directory As String = System.IO.Path.GetDirectoryName(path)
            Return directory
        End Function
        Public Shared Function ParseLaserSetupFileNames(ByVal printerStatusXML As String, <Out> ByRef fileNames As List(Of String)) As Boolean
            fileNames = New List(Of String)()
            Dim doc As XmlDocument = New XmlDocument()
            doc.LoadXml(printerStatusXML)

            Dim node As XmlNode
            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()")
            If node Is Nothing Then Return False

            Dim cdataText As String = node.InnerText
            If cdataText.Trim().Length = 0 Then Return False

            Dim setupFilesDoc As XmlDocument = New XmlDocument()
            setupFilesDoc.LoadXml(cdataText)
            Dim nodelist As XmlNodeList = setupFilesDoc.SelectNodes("*/*/*")
            If nodelist Is Nothing Then Return False

            For Each fileNode As XmlNode In nodelist
                Dim xmlAttrCollection As XmlAttributeCollection = fileNode.Attributes
                Dim attr As XmlAttribute = CType((If((xmlAttrCollection IsNot Nothing), xmlAttrCollection.GetNamedItem("name"), Nothing)), XmlAttribute)
                If attr Is Nothing Then Continue For
                Dim fname As String = attr.Value.Trim()
                If fname.Length > 0 Then fileNames.Add(fname)
            Next

            Return(fileNames.Count > 0)
        End Function

        Public Shared Function ParseLaserStatusXML(ByVal laserStatusXML As String) As LaserStatusValues
            Dim doc As XmlDocument = New XmlDocument()
            doc.LoadXml(laserStatusXML)
            Dim laserStatusValues As LaserStatusValues = New LaserStatusValues()
            laserStatusValues._success = IntFromXml(doc, "LaserResponse/Status")
            laserStatusValues._base64Data = StringFromXml(doc, "LaserResponse/Base64Data")
            Return laserStatusValues
        End Function

        Public Shared Function CreateElement(ByRef doc As XmlDocument, ByVal name As String) As XmlElement
            Dim element As XmlElement = doc.CreateElement(name)
            If element Is Nothing Then
                Throw New Exception(" CreateElement failed for " & name)
            End If

            Return element
        End Function

        Public Shared Sub CreateAndAddTextNode(ByRef doc As XmlDocument, ByVal text As String, ByRef parent As XmlElement)
            Dim textNode As XmlText = doc.CreateTextNode(text)
            If textNode Is Nothing Then
                Throw New Exception("CreateTextNode failed.")
            End If

            AppendChildToParent(textNode, parent)
        End Sub

        Public Shared Sub AppendChildToParent(ByVal child As XmlNode, ByRef parent As XmlDocument)
            Try
                parent.AppendChild(child)
            Catch e As SystemException
                Throw New Exception("AppendChildtoParent failure: " & e.Message)
            End Try
        End Sub

        Public Shared Sub AppendChildToParent(ByVal child As XmlNode, ByRef parent As XmlElement)
            Try
                parent.AppendChild(child)
            Catch e As SystemException
                Throw New Exception("AppendChildtoParent failure: " & e.Message)
            End Try
        End Sub

        Public Shared Function CreateAndAddElementNode(ByRef doc As XmlDocument, ByVal name As String, ByVal newline As String, ByRef parent As XmlElement) As XmlElement
            Dim element As XmlElement = CreateElement(doc, name)
            CreateAndAddTextNode(doc, newline, parent)
            AppendChildToParent(element, parent)
            Return element
        End Function

        Public Shared Sub CreateAndAddCDATANode(ByRef doc As XmlDocument, ByVal data As String, ByRef parent As XmlElement)
            Dim CData As XmlCDataSection
            CData = doc.CreateCDataSection(data)
            If CData Is Nothing Then
                Throw New Exception("CreateCDataSection failed.")
            End If

            AppendChildToParent(CData, parent)
        End Sub

        Public Shared Function CreateImportZipFileXML(ByVal fileName As String, ByVal overwrite As Boolean, ByVal base64EncodedData As String) As String
            Dim doc As XmlDocument = New XmlDocument()
            Dim rootElement As XmlElement = CreateElement(doc, "LaserZipFile")
            Dim element As XmlElement = CreateAndAddElementNode(doc, "FileName", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, fileName, element)
            element = CreateAndAddElementNode(doc, "Overwrite", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, If(overwrite, "1", "0"), element)
            element = CreateAndAddElementNode(doc, "FileContents", vbLf & vbTab, rootElement)
            CreateAndAddCDATANode(doc, base64EncodedData, element)
            CreateAndAddTextNode(doc, vbLf, rootElement)
            AppendChildToParent(rootElement, doc)
            Return doc.OuterXml
        End Function

        Public Shared Function CreateLaserEngraveTextXML(ByVal elementName As String, ByVal elementValue As String) As String
            Dim doc As XmlDocument = New XmlDocument()
            Dim rootElement As XmlElement = CreateElement(doc, "LaserEngraveText")
            Dim element As XmlElement = CreateAndAddElementNode(doc, "ElementName", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, elementName, element)
            element = CreateAndAddElementNode(doc, "ElementValue", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, elementValue, element)
            CreateAndAddTextNode(doc, vbLf, rootElement)
            AppendChildToParent(rootElement, doc)
            Return doc.OuterXml
        End Function

        Public Shared Function CreateLaserSetupFileNameXML(ByVal laserSetupFileName As String, ByVal count As Integer) As String
            Dim doc As XmlDocument = New XmlDocument()
            Dim rootElement As XmlElement = CreateElement(doc, "LaserEngraveSetupFileName")
            Dim element As XmlElement = CreateAndAddElementNode(doc, "FileName", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, laserSetupFileName, element)
            element = CreateAndAddElementNode(doc, "ElementCount", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, count.ToString(), element)
            CreateAndAddTextNode(doc, vbLf, rootElement)
            AppendChildToParent(rootElement, doc)
            Return doc.OuterXml
        End Function

        Public Shared Function CreateLaserFileNameXML(ByVal laserFileName As String) As String
            Dim doc As XmlDocument = New XmlDocument()
            Dim rootElement As XmlElement = CreateElement(doc, "LaserSetup")
            Dim element As XmlElement = CreateAndAddElementNode(doc, "FileName", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, laserFileName, element)
            CreateAndAddTextNode(doc, vbLf, rootElement)
            AppendChildToParent(rootElement, doc)
            Return doc.OuterXml
        End Function

        Public Shared Function CreateLaserEngraveBinaryXML(ByVal elementName As String, ByVal base64EncodedData As String) As String
            Dim doc As XmlDocument = New XmlDocument()
            Dim rootElement As XmlElement = CreateElement(doc, "LaserEngraveBinary")
            Dim element As XmlElement = CreateAndAddElementNode(doc, "ElementName", vbLf & vbTab, rootElement)
            CreateAndAddTextNode(doc, elementName, element)
            element = CreateAndAddElementNode(doc, "ElementValue", vbLf & vbTab, rootElement)
            CreateAndAddCDATANode(doc, base64EncodedData, element)
            CreateAndAddTextNode(doc, vbLf, rootElement)
            AppendChildToParent(rootElement, doc)
            Return doc.OuterXml
        End Function

        ''' </summary>
        ''' <param name="hopperStatusXml"></param>
        ''' <returns>a populated HOPPER_INFORMATION list populated from the xml markup.</returns>
        Public Shared Function ParseHopperStatusXml(hopperStatusXml As String) As List(Of HOPPER_INFORMATION)

            Dim hopperInfoList As New List(Of HOPPER_INFORMATION)()
            Dim doc As New XmlDocument()

            doc.LoadXml(hopperStatusXml)

            Dim nodeList As XmlNodeList = doc.SelectNodes("HopperStatus/HopperInformation")
            If nodeList Is Nothing Then
                Throw New Exception("Could not parse hopper status xml")
            End If

            Dim numHoppers As Integer = nodeList.Count
            For hopperIndex As Integer = 0 To numHoppers - 1
                Dim hopperInfo As New HOPPER_INFORMATION()

                Dim node As XmlNode = nodeList.Item(hopperIndex)
                If node Is Nothing Then
                    Throw New Exception("Xml node corresponding to hopper '" & hopperIndex & "' could not be retrieved.")
                End If

                Dim attributeMap As XmlAttributeCollection = node.Attributes

                '  name...
                Dim nameAttributeNode As XmlAttribute = DirectCast(If((attributeMap IsNot Nothing), attributeMap.GetNamedItem("Name"), Nothing), XmlAttribute)
                If nameAttributeNode Is Nothing Then
                    Throw New Exception("'Name' attribute associated with hopper '" & hopperIndex & "' is null")
                End If

                hopperInfo._name = nameAttributeNode.Value

                '  type...
                Dim typeAttributeNode As XmlAttribute = DirectCast(If((attributeMap IsNot Nothing), attributeMap.GetNamedItem("Type"), Nothing), XmlAttribute)
                If typeAttributeNode Is Nothing Then
                    Throw New Exception("'Type' attribute associated with hopper '" & hopperIndex & "' is null")
                End If

                hopperInfo._type = typeAttributeNode.Value

                '  Status...
                Dim statusAttributeNode As XmlAttribute = DirectCast(If((attributeMap IsNot Nothing), attributeMap.GetNamedItem("Status"), Nothing), XmlAttribute)
                If statusAttributeNode Is Nothing Then
                    Throw New Exception("'Status' attribute associated with hopper '" & hopperIndex & "' is null")
                End If

                hopperInfo._status = statusAttributeNode.Value

                '  CardStock...
                Dim cardStockAttributeNode As XmlAttribute = DirectCast(If((attributeMap IsNot Nothing), attributeMap.GetNamedItem("CardStock"), Nothing), XmlAttribute)
                If cardStockAttributeNode Is Nothing Then
                    Throw New Exception("'CardStock' attribute associated with hopper '" & hopperIndex & "' is null")
                End If

                hopperInfo._cardStock = cardStockAttributeNode.Value
                hopperInfoList.Add(hopperInfo)
            Next

            Return hopperInfoList
        End Function

        ''' <summary>
        ''' compare "actual" FW rev to a "base" FW rev
        ''' </summary>
        ''' <param name="fwBase"></param>
        ''' <param name="fwActual"></param>
        ''' <returns> true if "actual" FW rev is greater than or equal to "base" FW rev.</returns>
        '
        Public Shared Function ParseFirmwareRev(strFwRev As String) As FirmwareVersion
            ' FW revision is of the following format(s)
            ' D3.17.3  or D3.17.3-4

            ' break strFwRev into tokens
            Dim tokens As String() = strFwRev.Split("."c)

            Dim fwRev As New FirmwareVersion()
            fwRev._printerBase = ""
            fwRev._majorVersion = 0
            fwRev._minorVersion = 0
            fwRev._deviationVersion = 0

            ' PrinterType
            fwRev._printerBase = tokens(0)

            ' Major Revision
            Dim nMajorRev As Integer = 0
            Dim bResult As Boolean = Integer.TryParse(tokens(1), nMajorRev)
            If bResult Then
                fwRev._majorVersion = CShort(nMajorRev)
            End If

            ' minor revision 
            ' FYI - there could be 1 or 2 elements in this array
            Dim minors As String() = tokens(2).Split("-"c)

            Dim nMinorRev As Integer = 0
            bResult = Integer.TryParse(minors(0), nMinorRev)
            If bResult Then
                fwRev._minorVersion = CShort(nMinorRev)
            End If

            ' minor revision deviation
            If minors.Length > 1 Then
                Dim nMinorRevDev As Integer = 0
                bResult = Integer.TryParse(minors(1), nMinorRevDev)
                If bResult Then
                    fwRev._deviationVersion = CShort(nMinorRevDev)
                End If
            End If

            ' base minor rev is <= actual minor rev.
            Return fwRev
        End Function

        ''' <summary>
        ''' compare "actual" FW rev to a "base" FW rev
        ''' </summary>
        ''' <param name="fwBase"></param>
        ''' <param name="fwActual"></param>
        ''' <returns> true if "actual" FW rev is greater than or equal to "base" FW rev.</returns>
        '
        Public Shared Function CompareFirmwareRev(fwBase As FirmwareVersion, fwActual As FirmwareVersion) As Boolean
            ' FW revision is of the following format(s)
            ' D3.17.3  or D3.17.3-4

            ' testing the printer type (i.e. first two characters of the "original" strings)
            If fwBase._printerBase.CompareTo(fwActual._printerBase) <> 0 Then
                ' comparing different printer types.  Not a valid comparison
                Return False
            End If

            ' testing the major revision
            If fwBase._majorVersion < fwActual._majorVersion Then
                ' no need to continue;
                Return True
            End If
            If fwBase._majorVersion > fwActual._majorVersion Then
                ' no need to continue;
                Return False
            End If

            ' At this point:
            '    the PrinterType is the same, 
            '    the Major rev is the same 
            '    (i.e. D3.17.x-y)
            If fwBase._minorVersion < fwActual._minorVersion Then
                ' no need to continue;
                Return True
            End If
            If fwBase._minorVersion > fwActual._minorVersion Then
                ' no need to continue;
                Return False
            End If


            ' At this point:
            '    the PrinterType is the same, 
            '    the Major rev is the same and
            '    the Minor rev is the same 
            '    (i.e. D3.17.3-y)
            If fwBase._deviationVersion > fwActual._deviationVersion Then
                ' no need to continue;
                Return False
            End If

            ' base _deviationVersion is <= actual _deviationVersion
            Return True
        End Function
    End Class
End Namespace
