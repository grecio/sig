////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Text;
using System.Threading;
using System.Xml;

namespace dxp01sdk {

    /// <summary>
    /// we need the printerJobID and and printer error code to properly
    /// issue a CancelJob() call.
    /// </summary>
    public class BidiException : Exception {
        public BidiException(string message, int printerJobID, int errorCode)
            : base(message) {
            this.errorCode = errorCode;
            this.printerJobID = printerJobID;
        }

        public int PrinterJobID { get { return printerJobID; } }
        public int ErrorCode { get { return errorCode; } }

        private int printerJobID;
        private int errorCode;
    }
    
    public class FirmwareVersion {
        public string _printerBase;
        public short _majorVersion;
        public short _minorVersion;
        public short _deviationVersion;
    };

    public class PrinterStatusValues {
        public string _clientID;
        public string _dataFromPrinter;
        public int _errorCode;
        public int _errorSeverity;
        public string _errorString;
        public int _printerJobID;
        public int _windowsJobID;
    }

    /// <summary>
    /// values parsed from a GetPrinterData(strings.PRINTER_OPTIONS2) call.
    /// </summary>
    public class PrinterOptionsValues {
        public string _colorPrintResolution;
        public string _connectionPortType;
        public string _connectionProtocol;
        public string _embosserVersion;
        public string _laminator;
        public string _laminatorFirmwareVersion;
        public string _laminatorImpresser;
        public string _laminatorScanner;
        public string _laserFirmwareVersion;
        public string _lockState;
        public string _moduleEmbosser;
        public string _monochromePrintResolution;
        public string _multiHopperVersion;
        public string _optionDuplex;
        public string _optionInputhopper;
        public string _optionLaser;
        public string _optionLaserVisionRegistration;
        public string _optionObscureBlackPanel;
        public string _optionLocks;
        public string _optionMagstripe;
        public string _optionSecondaryMagstripeJIS;
        public string _optionPrinterBarcodeReader;
        public string _optionRewrite;
        public string _optionSmartcard;
        public string _printEngineType;
        public string _printerAddress;
        public int    _printerMessageNumber;
        public string _printerModel;
        public string _printerSerialNumber;
        public string _printerStatus;
        public string _printerVersion;
        public string _printHead;
        public string _topcoatPrintResolution;
    }

    public class PrinterCounterStatus {
        public int _cardsPickedSinceCleaningCard;
        public int _cleaningCardsRun;
        public int _currentCompleted;
        public int _currentLost;
        public int _currentPicked;
        public int _currentPickedExceptionSlot;
        public int _currentPickedInputHopper1;
        public int _currentPickedInputHopper2;
        public int _currentPickedInputHopper3;
        public int _currentPickedInputHopper4;
        public int _currentPickedInputHopper5;
        public int _currentPickedInputHopper6;
        public int _currentRejected;
        public string _printerStatus;
        public int _totalCompleted;
        public int _totalLost;
        public int _totalPicked;
        public int _totalPickedExceptionSlot;
        public int _totalPickedInputHopper1;
        public int _totalPickedInputHopper2;
        public int _totalPickedInputHopper3;
        public int _totalPickedInputHopper4;
        public int _totalPickedInputHopper5;
        public int _totalPickedInputHopper6;
        public int _totalRejected;
    }

    public class SuppliesValues {
        public string _printerStatus;
        public string _printRibbonType;
        public short _ribbonRemaining;
        public string _printRibbonSerialNumber;
        public string _printRibbonLotCode;
        public int _printRibbonPartNumber;
        public int _printRibbonRegionCode;

        public string _retransferFilmSerialNumber;
        public long   _retransferFilmPartNumber;
        public string _retransferFilmLotCode;
        public short  _retransferFilmRemaining;

        public string _indentRibbon;
        public short _indentRibbonRemaining;
        public string _indentRibbonSerialNumber;
        public string _indentRibbonLotCode;
        public int _indentRibbonPartNumber;

        public string _topperRibbonType;
        public short _topperRibbonRemaining;
        public string _topperRibbonSerialNumber;
        public string _topperRibbonLotCode;
        public int _topperRibbonPartNumber;

        public long _laminatorL1SupplyCode;
        public short _laminatorL1PercentRemaining;
        public string _laminatorL1SerialNumber;
        public string _laminatorL1LotCode;
        public int _laminatorL1PartNumber;

        public long _laminatorL2SupplyCode;
        public short _laminatorL2PercentRemaining;
        public string _laminatorL2SerialNumber;
        public string _laminatorL2LotCode;
        public int _laminatorL2PartNumber;
    }

    public class JobStatusValues {
        public string _clientID;
        public int _jobRestartCount;
        public string _jobState;
        public int _printerJobID;
        public int _windowsJobID;
    }

    public class SmartcardResponseValues {
        public string _protocol;
        public string[] _states;
        public string _status;
        public string _base64string;
        public byte[] _bytesFromBase64String;
    }

    public class LaserStatusValues
    {
        public int _success;
        public string _base64Data;
    }

    public class HOPPER_INFORMATION {
        public string _name;
        public string _type;
        public string _status;
        public string _cardStock;
    }



    class Util {
        enum PrintSpoolerStatus {
            Other = 1,
            Unknown,
            Idle,
            Printing,
            WarmingUp,
            StoppedPrinting,
            Offline,
        }

        public static string StringFromXml(XmlDocument doc, string xmlElementName) {
            XmlNode node = doc.SelectSingleNode(xmlElementName);
            if (null != node) {
                return node.InnerText;
            }
            return string.Empty;
        }

        public static int IntFromXml(XmlDocument doc, string xmlElementName) {
            XmlNode node = doc.SelectSingleNode(xmlElementName);
            int val = 0;
            if (null != node) {
                Int32.TryParse(node.InnerText, out val);
            }
            return val;
        }
        /// <summary>
        /// GetHopperIndex
        /// ASSUMPTION: hopperId is valid hopper string, i.e.,
        ///      it is either "exception" or "1"-"6".
        /// </summary>
        /// <param name="hopperId"></param>
        /// <returns></returns>
        public static int GetHopperIndex(
            string hopperId)
        {
            if (String.Compare(hopperId, "exception", true) == 0) return 0;

            int hopperNdx = 1;
            Int32.TryParse(hopperId, out hopperNdx);
            return hopperNdx;
        }
        /// ParseHopperStatusXML
        /// 
        /// 
        public static string ParseHopperStatusXML(
            string hopperStatusXML,
            string hopperId)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(hopperStatusXML);
            XmlNodeList nodeList = doc.SelectNodes("HopperStatus/HopperInformation");

            if (nodeList == null) {
                throw new Exception("Could not parse hopper status xml.");
            }

            int numHoppers = nodeList.Count;
            int hopperIndex = GetHopperIndex(hopperId);

            if (hopperIndex >= numHoppers) {
                throw new Exception("Hopper id '" + hopperId + "' is out of range of available hoppers.");
            }

            XmlNode node = nodeList.Item(hopperIndex);
            if (node == null) {
                throw new Exception("Xml node corresponding to hopper '" + hopperId + "' could not be retrieved.");
            }
            XmlAttributeCollection xmlAttrCollection = node.Attributes;
            XmlAttribute attr = (XmlAttribute)((xmlAttrCollection != null)
                ? xmlAttrCollection.GetNamedItem("Status")
                : null);

            if (attr == null) {
                throw new Exception("attribute corresponding to hopper '" + hopperId + "' is null.");
            }

            string hopperStatus = attr.Value;
            return hopperStatus;
        }
        /// <summary>
        /// ParsePrinterOptionsXML()
        /// 
        /// we have PrinterOptions2 xml from the printer. Parse the xml and populate
        /// a new PrinterOptionsValues instance with the data items.
        /// 
        ///     <?xml version="1.0"?>
        ///     <!--Printer options2 xml file.-->
        ///     <PrinterInfo2>
        ///         <PrinterStatus>Ready</PrinterStatus>
        ///         <PrinterAddress>10.2.200.224</PrinterAddress>
        ///         <PrinterModel>CE870_Andy</PrinterModel>
        ///         <PrinterSerialNumber>E00111</PrinterSerialNumber>
        ///         <PrinterVersion>D3.17.4-1</PrinterVersion>
        ///         <PrinterMessageNumber>0</PrinterMessageNumber>
        ///         <ConnectionPortType>Network</ConnectionPortType>
        ///         <ConnectionProtocol>Version2</ConnectionProtocol>
        ///         <OptionInputhopper>MultiHopper6</OptionInputhopper>
        ///         <OptionMagstripe>ISO</OptionMagstripe>
        ///         <OptionRewritable>None</OptionRewritable>
        ///         <OptionSecondaryMagstripeJIS>None</OptionSecondaryMagstripeJIS>
        ///         <OptionSmartcard>Installed</OptionSmartcard>
        ///         <OptionDuplex>Auto</OptionDuplex>
        ///         <OptionPrinterBarcodeReader>Installed</OptionPrinterBarcodeReader>
        ///         <OptionLocks>Installed</OptionLocks>
        ///         <LockState>Locked</LockState>
        ///         <PrintEngineType>DirectToCard_DyeSub</PrintEngineType>
        ///         <PrintHead>Installed</PrintHead>
        ///         <ColorPrintResolution>300x300 | 300x600</ColorPrintResolution>
        ///         <MonochromePrintResolution>300x300 | 300x600 | 300x1200</MonochromePrintResolution>
        ///         <TopcoatPrintResolution>300x300</TopcoatPrintResolution>
        ///         <ModuleEmbosser>None</ModuleEmbosser>
        ///         <Laminator>None</Laminator>
        ///         <LaserModule>None</LaserModule>
        ///         <LaserVisionRegistration>None</LaserVisionRegistration>
        ///         <LaserVersion>LE version 2.1.20 Aug 18 2017 11:04:27</LaserVersion>    
        ///         <ObscureBlackPanel>None</ObscureBlackPanel>
        ///     </PrinterInfo2>
        /// 
        /// </summary>
        /// <param name="printerOptionsXML"></param>
        /// <returns>a populated PrinterOptionsValues struct</returns>
        public static PrinterOptionsValues ParsePrinterOptionsXML(string printerOptionsXML) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printerOptionsXML);

            PrinterOptionsValues printerOptionsValues = new PrinterOptionsValues();

            printerOptionsValues._colorPrintResolution = StringFromXml(doc, "PrinterInfo2/ColorPrintResolution");
            printerOptionsValues._connectionPortType = StringFromXml(doc, "PrinterInfo2/ConnectionPortType");
            printerOptionsValues._connectionProtocol = StringFromXml(doc, "PrinterInfo2/ConnectionProtocol");
            printerOptionsValues._embosserVersion = StringFromXml(doc, "PrinterInfo2/EmbosserVersion");
            printerOptionsValues._lockState = StringFromXml(doc, "PrinterInfo2/LockState");
            printerOptionsValues._moduleEmbosser = StringFromXml(doc, "PrinterInfo2/ModuleEmbosser");
            printerOptionsValues._monochromePrintResolution = StringFromXml(doc, "PrinterInfo2/MonochromePrintResolution");
            printerOptionsValues._optionDuplex = StringFromXml(doc, "PrinterInfo2/OptionDuplex");
            printerOptionsValues._optionInputhopper = StringFromXml(doc, "PrinterInfo2/OptionInputhopper");
            printerOptionsValues._optionLaser = StringFromXml(doc, "PrinterInfo2/LaserModule");
            printerOptionsValues._optionLaserVisionRegistration = StringFromXml(doc, "PrinterInfo2/LaserVisionRegistration");
            printerOptionsValues._optionObscureBlackPanel = StringFromXml(doc, "PrinterInfo2/ObscureBlackPanel");
            printerOptionsValues._optionLocks = StringFromXml(doc, "PrinterInfo2/OptionLocks");
            printerOptionsValues._optionMagstripe = StringFromXml(doc, "PrinterInfo2/OptionMagstripe");
            printerOptionsValues._optionSecondaryMagstripeJIS = StringFromXml(doc, "PrinterInfo2/OptionSecondaryMagstripeJIS");
            printerOptionsValues._optionPrinterBarcodeReader = StringFromXml(doc, "PrinterInfo2/OptionPrinterBarcodeReader");
            printerOptionsValues._optionSmartcard = StringFromXml(doc, "PrinterInfo2/OptionSmartcard");
            printerOptionsValues._printerAddress = StringFromXml(doc, "PrinterInfo2/PrinterAddress");
            printerOptionsValues._printerMessageNumber = IntFromXml(doc, "PrinterInfo2/PrinterMessageNumber");
            printerOptionsValues._printerModel = StringFromXml(doc, "PrinterInfo2/PrinterModel");
            printerOptionsValues._printerSerialNumber = StringFromXml(doc, "PrinterInfo2/PrinterSerialNumber");
            printerOptionsValues._printerStatus = StringFromXml(doc, "PrinterInfo2/PrinterStatus");
            printerOptionsValues._printerVersion = StringFromXml(doc, "PrinterInfo2/PrinterVersion");
            printerOptionsValues._printHead = StringFromXml(doc, "PrinterInfo2/PrintHead");
            printerOptionsValues._optionRewrite = StringFromXml(doc, "PrinterInfo2/OptionRewritable");
            printerOptionsValues._topcoatPrintResolution = StringFromXml(doc, "PrinterInfo2/TopcoatPrintResolution");

            printerOptionsValues._laminatorFirmwareVersion = StringFromXml(doc, "PrinterInfo2/LaminatorFirmwareVersion");
            printerOptionsValues._laminator = StringFromXml(doc, "PrinterInfo2/Laminator");
            printerOptionsValues._laminatorImpresser = StringFromXml(doc, "PrinterInfo2/LaminatorImpresser");
            printerOptionsValues._laminatorScanner = StringFromXml(doc, "PrinterInfo2/LaminatorScanner");

            printerOptionsValues._laserFirmwareVersion = StringFromXml(doc, "PrinterInfo2/LaserVersion");
            printerOptionsValues._multiHopperVersion = StringFromXml(doc, "PrinterInfo2/MultiHopperVersion");

            //  as of version 7.0, printEngine defaults to direct-to-card...
            printerOptionsValues._printEngineType = StringFromXml(doc, "PrinterInfo2/PrintEngineType");
                
           return printerOptionsValues;
        }

        ////////////////////////////////////////////////////////////////////////////////
        // ParsePrinterCounterStatusXML()
        //
        // the 'printer card counts' xml from the printer looks something like this:
        //
        //    <?xml version="1.0"?>
        //    <!--Printer counter2 xml file.-->
        //    <CounterStatus2>
        //       <CardsPickedSinceCleaningCard>474</CardsPickedSinceCleaningCard>
        //       <CleaningCardsRun>3</CleaningCardsRun>
        //       <CurrentCompleted>260</CurrentCompleted>
        //       <CurrentLost>0</CurrentLost>
        //       <CurrentPicked>474</CurrentPicked>
        //       <CurrentPickedExceptionSlot>2</CurrentPickedExceptionSlot>
        //       <CurrentPickedInputHopper1>242</CurrentPickedInputHopper1>
        //       <CurrentPickedInputHopper2>1</CurrentPickedInputHopper2>
        //       <CurrentPickedInputHopper3>33</CurrentPickedInputHopper3>
        //       <CurrentPickedInputHopper4>14</CurrentPickedInputHopper4>
        //       <CurrentPickedInputHopper5>3</CurrentPickedInputHopper5>
        //       <CurrentPickedInputHopper6>1</CurrentPickedInputHopper6>
        //       <CurrentRejected>36</CurrentRejected>
        //       <PrinterStatus>Ready</PrinterStatus>
        //       <TotalCompleted>260</TotalCompleted>
        //       <TotalLost>0</TotalLost>
        //       <TotalPicked>474</TotalPicked>
        //       <TotalPickedExceptionSlot>2</TotalPickedExceptionSlot>
        //       <TotalPickedInputHopper1>242</TotalPickedInputHopper1>
        //       <TotalPickedInputHopper2>1</TotalPickedInputHopper2>
        //       <TotalPickedInputHopper3>33</TotalPickedInputHopper3>
        //       <TotalPickedInputHopper4>14</TotalPickedInputHopper4>
        //       <TotalPickedInputHopper5>3</TotalPickedInputHopper5>
        //       <TotalPickedInputHopper6>1</TotalPickedInputHopper6>
        //       <TotalRejected>36</TotalRejected>
        //    </CounterStatus2>
        //
        ////////////////////////////////////////////////////////////////////////////////
        public static PrinterCounterStatus ParsePrinterCounterStatusXML(string printercardCountXML) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printercardCountXML);

            PrinterCounterStatus printerCardCountValues = new PrinterCounterStatus();

            printerCardCountValues._cardsPickedSinceCleaningCard = IntFromXml(doc, "CounterStatus2/CardsPickedSinceCleaningCard");
            printerCardCountValues._cleaningCardsRun = IntFromXml(doc, "CounterStatus2/CleaningCardsRun");
            printerCardCountValues._currentCompleted = IntFromXml(doc, "CounterStatus2/CurrentCompleted");
            printerCardCountValues._currentLost = IntFromXml(doc, "CounterStatus2/CurrentLost");
            printerCardCountValues._currentPicked = IntFromXml(doc, "CounterStatus2/CurrentPicked");
            printerCardCountValues._currentPickedExceptionSlot = IntFromXml(doc, "CounterStatus2/CurrentPickedExceptionSlot");
            printerCardCountValues._currentPickedInputHopper1 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper1");
            printerCardCountValues._currentPickedInputHopper2 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper2");
            printerCardCountValues._currentPickedInputHopper3 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper3");
            printerCardCountValues._currentPickedInputHopper4 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper4");
            printerCardCountValues._currentPickedInputHopper5 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper5");
            printerCardCountValues._currentPickedInputHopper6 = IntFromXml(doc, "CounterStatus2/CurrentPickedInputHopper6");
            printerCardCountValues._currentRejected = IntFromXml(doc, "CounterStatus2/CurrentRejected");
            printerCardCountValues._printerStatus = StringFromXml(doc, "CounterStatus2/PrinterStatus");
            printerCardCountValues._totalCompleted = IntFromXml(doc, "CounterStatus2/TotalCompleted");
            printerCardCountValues._totalLost = IntFromXml(doc, "CounterStatus2/TotalLost");
            printerCardCountValues._totalPicked = IntFromXml(doc, "CounterStatus2/TotalPicked");
            printerCardCountValues._totalPickedExceptionSlot = IntFromXml(doc, "CounterStatus2/TotalPickedExceptionSlot");
            printerCardCountValues._totalPickedInputHopper1 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper1");
            printerCardCountValues._totalPickedInputHopper2 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper2");
            printerCardCountValues._totalPickedInputHopper3 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper3");
            printerCardCountValues._totalPickedInputHopper4 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper4");
            printerCardCountValues._totalPickedInputHopper5 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper5");
            printerCardCountValues._totalPickedInputHopper6 = IntFromXml(doc, "CounterStatus2/TotalPickedInputHopper6");
            printerCardCountValues._totalRejected = IntFromXml(doc, "CounterStatus2/TotalRejected");

            return printerCardCountValues;
        }

        public static SuppliesValues ParseSuppliesXML(string suppliesXML) {

            // sample supplies xml from printer:
            //
            // <?xml version="1.0"?>
            // <!--Printer Supplies3 xml file.-->
            // <PrinterSupplies3>
            //   <PrinterStatus>Ready</PrinterStatus>
            //   <Printer>
            //     <PrintRibbon>Installed</PrintRibbon>
            //     <PrintRibbonType>Monochrome</PrintRibbonType>
            //     <RibbonLotCode>16MAR12  </RibbonLotCode>
            //     <RibbonPartNumber>533000052</RibbonPartNumber>
            //     <RibbonRemaining>43</RibbonRemaining>
            //     <RibbonSerialNumber>E0055000008E9956</RibbonSerialNumber>
            //   </Printer>
            //   <Embosser>
            //     <IndentRibbon>Installed</IndentRibbon>
            //     <IndentRibbonLotCode/>
            //     <IndentRibbonPartNumber/>
            //     <IndentRibbonRemaining>50</IndentRibbonRemaining>
            //     <IndentRibbonSerialNumber/>
            //     <IndentRibbonType>67174401</IndentRibbonType>
            //     <TopperRibbon>Installed</TopperRibbon>
            //     <TopperRibbonLotCode>29Oct11  </TopperRibbonLotCode>
            //     <TopperRibbonPartNumber>504139013</TopperRibbonPartNumber>
            //     <TopperRibbonRemaining>25</TopperRibbonRemaining>
            //     <TopperRibbonSerialNumber>25A18E00005005E0</TopperRibbonSerialNumber>
            //     <TopperRibbonType>Silver</TopperRibbonType>
            //   </Embosser>
            //   <Laminator>
            //     <L1Laminate>None</L1Laminate>
            //     <L1LaminateLotCode/>
            //     <L1LaminatePartNumber/>
            //     <L1LaminateRemaining/>
            //     <L1LaminateSerialNumber/>
            //     <L1LaminateType/>
            //     <L2Laminate>None</L2Laminate>
            //     <L2LaminateLotCode/>
            //     <L2LaminatePartNumber/>
            //     <L2LaminateRemaining/>
            //     <L2LaminateSerialNumber/>
            //     <L2LaminateType/>
            //   </Laminator>
            // </PrinterSupplies3>

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(suppliesXML);

            SuppliesValues suppliesValues = new SuppliesValues();

            suppliesValues._indentRibbon = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbon");
            suppliesValues._indentRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonLotCode");
            suppliesValues._indentRibbonPartNumber = (int) IntFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonPartNumber");
            suppliesValues._indentRibbonRemaining = (short) IntFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonRemaining");
            suppliesValues._indentRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Embosser/IndentRibbonSerialNumber");
            suppliesValues._laminatorL1LotCode = StringFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateLotCode");
            suppliesValues._laminatorL1PartNumber = (int) IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminatePartNumber");
            suppliesValues._laminatorL1PercentRemaining = (short) IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateRemaining");
            suppliesValues._laminatorL1SerialNumber = StringFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateSerialNumber");
            suppliesValues._laminatorL1SupplyCode = (long) IntFromXml(doc, "PrinterSupplies3/Laminator/L1LaminateType");
            suppliesValues._laminatorL2LotCode = StringFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateLotCode");
            suppliesValues._laminatorL2PartNumber = (int) IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminatePartNumber");
            suppliesValues._laminatorL2PercentRemaining = (short) IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateRemaining");
            suppliesValues._laminatorL2SerialNumber = StringFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateSerialNumber");
            suppliesValues._laminatorL2SupplyCode = (long) IntFromXml(doc, "PrinterSupplies3/Laminator/L2LaminateType");
            suppliesValues._printerStatus = StringFromXml(doc, "PrinterSupplies3/PrinterStatus");
            suppliesValues._printRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Printer/RibbonLotCode");
            suppliesValues._printRibbonPartNumber = (int) IntFromXml(doc, "PrinterSupplies3/Printer/RibbonPartNumber");
            suppliesValues._printRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Printer/RibbonSerialNumber");
            suppliesValues._printRibbonType = StringFromXml(doc, "PrinterSupplies3/Printer/PrintRibbonType");
            suppliesValues._printRibbonRegionCode = (int)IntFromXml(doc, "PrinterSupplies3/Printer/RibbonRegionCode");
            suppliesValues._ribbonRemaining = (short)IntFromXml(doc, "PrinterSupplies3/Printer/RibbonRemaining");
            suppliesValues._topperRibbonLotCode = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonLotCode");
            suppliesValues._topperRibbonPartNumber = (int) IntFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonPartNumber");
            suppliesValues._topperRibbonRemaining = (short) IntFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonRemaining");
            suppliesValues._topperRibbonSerialNumber = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonSerialNumber");
            suppliesValues._topperRibbonType = StringFromXml(doc, "PrinterSupplies3/Embosser/TopperRibbonType");

            // retransfer supplies...
            suppliesValues._retransferFilmSerialNumber = StringFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmSerialNumber");
            suppliesValues._retransferFilmPartNumber = (long) IntFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmPartNumber");
            suppliesValues._retransferFilmLotCode = StringFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmLotCode");
            suppliesValues._retransferFilmRemaining = (short) IntFromXml(doc, "PrinterSupplies3/Printer/RetransferFilmPercentRemaining");
            return suppliesValues;
        }

        /// <summary>
        /// populate values from this xml returned from the printer:
        /// <?xml version="1.0"?>
        /// <SDKVersion>2.1.217.0</SDKVersion>
        /// </summary>
        public static string ParseDriverVersionXML(string driverVersionXML) {
            string driverVersion = string.Empty;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(driverVersionXML);
            try {
                XmlNode node = doc.SelectSingleNode("SDKVersion");
                driverVersion = node.InnerText;
            }
            catch (Exception) {
                // note: driver version not returned in previous drivers.
            }
            return driverVersion;
        }

        /// <summary>
        /// sample xml printer status xml:
        /// 
        ///     <!--Printer status xml file.-->
        ///     <PrinterStatus>
        ///         <ClientID>fellmadwin7x64_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
        ///         <DataFromPrinter><![CDATA[  ]]></DataFromPrinter>
        ///         <ErrorCode>0</ErrorCode>
        ///         <ErrorSeverity>0</ErrorSeverity>
        ///         <ErrorString></ErrorString>
        ///         <PrinterJobID>792</PrinterJobID>
        ///         <WindowsJobID>0</WindowsJobID>
        ///     </PrinterStatus>
        ///     
        /// </summary>
        public static PrinterStatusValues ParsePrinterStatusXML(string printerStatusXML) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printerStatusXML);

            PrinterStatusValues printerStatusValues = new PrinterStatusValues();

            printerStatusValues._printerJobID = IntFromXml(doc, "PrinterStatus/PrinterJobID");
            printerStatusValues._clientID = StringFromXml(doc, "PrinterStatus/ClientID");
            printerStatusValues._dataFromPrinter = StringFromXml(doc, "PrinterStatus/DataFromPrinter");
            printerStatusValues._errorCode = IntFromXml(doc, "PrinterStatus/ErrorCode");
            printerStatusValues._errorSeverity = IntFromXml(doc, "PrinterStatus/ErrorSeverity");
            printerStatusValues._errorString = StringFromXml(doc, "PrinterStatus/ErrorString");
            printerStatusValues._windowsJobID = IntFromXml(doc, "PrinterStatus/WindowsJobID");

            return printerStatusValues;
        }

        public static JobStatusValues ParseJobStatusXML(string jobStatusValues) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(jobStatusValues);

            JobStatusValues jobStatusXML = new JobStatusValues();

            jobStatusXML._clientID = StringFromXml(doc, "JobStatus/ClientID");
            jobStatusXML._jobRestartCount = IntFromXml(doc, "JobStatus/JobRestartCount");
            jobStatusXML._jobState = StringFromXml(doc, "JobStatus/JobState");
            jobStatusXML._printerJobID = IntFromXml(doc, "JobStatus/PrinterJobID");
            jobStatusXML._windowsJobID = IntFromXml(doc, "JobStatus/WindowsJobID");

            return jobStatusXML;
        }

        ////////////////////////////////////////////////////////////////////////////////
        // ParseMagstripeStrings()
        //
        // we're given a Printer Status XML fragment that has a CDATA section like this:
        //
        //   <DataFromPrinter>
        //      <![CDATA[<?xml version="1.0" encoding="UTF-8"?>
        //      <magstripe xmlns:SOAP-ENV="http://www.w3.org/2003/05/soap-envelope"
        //         xmlns:SOAP-ENC="http://www.w3.org/2003/05/soap-encoding"
        //         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        //         xmlns:xsd="http://www.w3.org/2001/XMLSchema"
        //         xmlns:DPCLMagStripe="urn:dpcl:magstripe:2010-01-19"
        //         xsi:type="DPCLMagStripe:MagStripe"
        //         SOAP-ENV:encodingStyle="http://www.w3.org/2003/05/soap-encoding">
        //         <track number="1">
        //            <base64Data>R1JPVU5E</base64Data>
        //         </track>
        //         <track number="2">
        //            <base64Data>MTIzNA==</base64Data>
        //         </track>
        //         <track number="3">
        //            <base64Data>MzIx</base64Data>
        //         </track>
        //      </magstripe>]]>
        //   </DataFromPrinter>
        //
        // return three strings. the string data is still Base64 encoded.
        ////////////////////////////////////////////////////////////////////////////////
        public static void ParseMagstripeStrings(
           string printerStatusXML,
           ref string track1,
           ref string track2,
           ref string track3,
           bool jisRequest) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printerStatusXML);

            XmlNode node;
            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()");
            string cdataText = node.InnerText;

            if (cdataText.Length > 10) // Make sure there is valid data before going further.
            {
                XmlDocument magstripeDoc = new XmlDocument();
                magstripeDoc.LoadXml(cdataText);

                XmlNodeList nodelist = magstripeDoc.SelectNodes("magstripe/track");
                foreach (XmlNode track in nodelist) {
                    XmlAttributeCollection attrColl = track.Attributes;
                    XmlAttribute attr = (XmlAttribute) attrColl.GetNamedItem("number");
                    int trackNumber = Convert.ToInt32(attr.Value);
                    if (jisRequest && trackNumber != 3) {
                        // only track 3 is valid for JIS request
                        continue;
                    }
                    switch (trackNumber) {

                        case 1:
                            track1 = track.InnerText;
                            break;

                        case 2:
                            track2 = track.InnerText;
                            break;

                        case 3:
                            track3 = track.InnerText;
                            break;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        // ParseLaserSetupFileNames
        // 
        // Example LaserSetupFileList XML from laser printer in the version 7.2 driver:
        //  <?xml version="1.0" encoding="UTF-8"?>
        //  -<QuerySetupsResult>
        //  -<LaserCardSetups>
        //  <LaserCardSetup name="DriverStaticSetup"/>
        //  <LaserCardSetup name="DriverVariableDuplexSetup"/>
        //  <LaserCardSetup name="DriverVariableSimplexSetup"/>
        //  </QuerySetupsResult>
        ////////////////////////////////////////////////////////////////////////////////
        public static bool ParseLaserSetupFileNames(
           string printerStatusXML,
           out List<string> fileNames)
        {
            fileNames = new List<string>();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printerStatusXML);

            XmlNode node;
            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()");
            if (node == null) return false;

            string cdataText = node.InnerText;
            if (cdataText.Trim().Length == 0) return false;

            XmlDocument setupFilesDoc = new XmlDocument();
            setupFilesDoc.LoadXml(cdataText);

            XmlNodeList nodelist = setupFilesDoc.SelectNodes(@"*/*/*");
            if (nodelist == null) return false;

            foreach (XmlNode fileNode in nodelist)
            {
                XmlAttributeCollection xmlAttrCollection = fileNode.Attributes;

                XmlAttribute attr =  (XmlAttribute) ((xmlAttrCollection != null)
                    ? xmlAttrCollection.GetNamedItem("name")
                    : null);

                if (attr == null) continue;
                string fname = attr.Value.Trim();
                if (fname.Length > 0)
                    fileNames.Add(fname);
            }
            return (fileNames.Count > 0);
        }

        public static LaserStatusValues ParseLaserStatusXML(
            string laserStatusXML)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(laserStatusXML);

            LaserStatusValues laserStatusValues = new LaserStatusValues();
            laserStatusValues._success = IntFromXml(doc, "LaserResponse/Status");
            laserStatusValues._base64Data = StringFromXml(doc, "LaserResponse/Base64Data");

            return laserStatusValues;
        }
        public static string GetExePath()
        {
            // to get the location the assembly is executing from...
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            string directory = System.IO.Path.GetDirectoryName(path); 
            return directory;           
        }

        public static XmlElement CreateElement(
            ref XmlDocument doc,
            string name)
        {
            XmlElement element = doc.CreateElement(name);
            if (element == null) {
                throw new Exception(" CreateElement failed for " + name);
            }
            return element;
        }

        public static void CreateAndAddTextNode(
            ref XmlDocument doc,
            string text,
            ref XmlElement parent)
        {
            XmlText textNode = doc.CreateTextNode(text);
            if (textNode == null) {
                throw new Exception("CreateTextNode failed.");
            }

            AppendChildToParent(textNode, ref parent);
        }
        public static void AppendChildToParent(
            XmlNode child,
            ref XmlDocument parent)
        {
            try
            {
                parent.AppendChild(child);
            }
            catch (SystemException e)
            {
                throw new Exception("AppendChildtoParent failure: " + e.Message);
            }

        }
        public static void AppendChildToParent(
            XmlNode         child,
            ref XmlElement  parent)
        {
            try
            {
                parent.AppendChild(child);
            }
            catch (SystemException e)
            {
                throw new Exception("AppendChildtoParent failure: " + e.Message);
            }

        }
        public static XmlElement CreateAndAddElementNode(
            ref XmlDocument doc,
            string name,
            string newline,
            ref XmlElement parent)
        {
            XmlElement element = CreateElement(ref doc, name);
            CreateAndAddTextNode(ref doc, newline, ref parent);
            AppendChildToParent(element, ref parent);

            return element;

        }

        public static void CreateAndAddCDATANode(
            ref XmlDocument doc,
            string data,
            ref XmlElement parent)
        {
            XmlCDataSection CData;
            CData = doc.CreateCDataSection(data);
            if (CData == null) {
                throw new Exception("CreateCDataSection failed.");
            }

            AppendChildToParent(CData, ref parent);
        }
        public static string CreateImportZipFileXML(
            string fileName,
            bool overwrite,
            string base64EncodedData)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement rootElement = CreateElement(ref doc, "LaserZipFile");

            XmlElement element = CreateAndAddElementNode(ref doc, "FileName", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, fileName, ref element);

            element = CreateAndAddElementNode(ref doc, "Overwrite", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, overwrite ? "1":"0", ref element);

            element = CreateAndAddElementNode(ref doc, "FileContents", "\n\t", ref rootElement);
            CreateAndAddCDATANode(ref doc, base64EncodedData, ref element);

            CreateAndAddTextNode(ref doc, "\n", ref rootElement);
            AppendChildToParent(rootElement, ref doc);

            return doc.OuterXml;
        }

        public static string CreateLaserEngraveTextXML(
            string elementName,
            string elementValue)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = CreateElement(ref doc, "LaserEngraveText");

            XmlElement element = CreateAndAddElementNode(ref doc, "ElementName", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, elementName, ref element);

            element = CreateAndAddElementNode(ref doc, "ElementValue", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, elementValue, ref element);

            CreateAndAddTextNode(ref doc, "\n", ref rootElement);
            AppendChildToParent(rootElement, ref doc);

            return doc.OuterXml;
        }
        public static string CreateLaserSetupFileNameXML(
            string laserSetupFileName,
            int count)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = CreateElement(ref doc, "LaserEngraveSetupFileName");

            XmlElement element = CreateAndAddElementNode(ref doc, "FileName", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, laserSetupFileName, ref element);

            element = CreateAndAddElementNode(ref doc, "ElementCount", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, count.ToString(), ref element);

            CreateAndAddTextNode(ref doc, "\n", ref rootElement);
            AppendChildToParent(rootElement, ref doc);

            return doc.OuterXml;
        }
        public static string CreateLaserFileNameXML(
            string laserFileName)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement rootElement = CreateElement(ref doc, "LaserSetup");
            XmlElement element = CreateAndAddElementNode(ref doc, "FileName", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, laserFileName, ref element);

            CreateAndAddTextNode(ref doc, "\n", ref rootElement);
            AppendChildToParent(rootElement, ref doc);

            return doc.OuterXml;
        }

        public static string CreateLaserEngraveBinaryXML(
            string  elementName,
            string  base64EncodedData)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement rootElement = CreateElement(ref doc, "LaserEngraveBinary");

            XmlElement element = CreateAndAddElementNode(ref doc, "ElementName", "\n\t", ref rootElement);
            CreateAndAddTextNode(ref doc, elementName, ref element);

            element = CreateAndAddElementNode(ref doc, "ElementValue", "\n\t", ref rootElement);
            CreateAndAddCDATANode(ref doc, base64EncodedData, ref element);

            CreateAndAddTextNode(ref doc, "\n", ref rootElement);
            AppendChildToParent(rootElement, ref doc);

            return doc.OuterXml;
        }

        /// <summary>
        /// repeatedly get the printer status until with printer status contains
        /// a valid windows jobID.
        /// </summary>
        /// <returns>zero if any error detected, or the windows jobID</returns>
        public static int WaitForWindowsJobID(BidiSplWrap bidiSpl, string printerName) {

            const int sleepMilliSeconds = 1000;

            var printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
            var printerStatus = Util.ParsePrinterStatusXML(printerStatusXML);

            do {

                if (0 != printerStatus._windowsJobID) {
                    Console.WriteLine(string.Format("returning windowsJobID {0}.", printerStatus._windowsJobID));
                    return printerStatus._windowsJobID;
                }

                Console.WriteLine(string.Format("waiting {0} millseconds for windows jobID", sleepMilliSeconds));
                Thread.Sleep(sleepMilliSeconds);

                printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
                printerStatus = Util.ParsePrinterStatusXML(printerStatusXML);

            } while (0 == printerStatus._errorCode);

            Console.WriteLine("printer error:");
            Console.WriteLine(printerStatusXML);

            return 0;
        }

        static void OnPrintPage(object sender, PrintPageEventArgs pageEventArgs) {
            using (Font font = new Font("Arial", 8))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.DarkBlue)) {
                pageEventArgs.Graphics.DrawString("Sample Text", font, brush, 50, 50);
                pageEventArgs.Graphics.DrawRectangle(pen, 50, 70, 20, 25);
            }
        }

        public static void PrintTextAndGraphics(BidiSplWrap bidiSpl, string printerName) {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintController = new StandardPrintController();
            printDocument.PrinterSettings.PrinterName = printerName;
            printDocument.DocumentName = Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " printing";
            printDocument.PrintPage += new PrintPageEventHandler(OnPrintPage);
            printDocument.Print();

            // we wait here - to ensure that the job is truly in the windows
            // spooler:
            var windowsJobID = WaitForWindowsJobID(bidiSpl, printerName);
        }

        static void OnPrintPageWithBlockingEscapes(PrintPageEventArgs pageEventArgs, string printBlockingEscape, string topcoatRemovalEscape)
        {
            using (Font font = new Font("Arial", 8))
            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.DarkBlue)) {
                if (printBlockingEscape.Length > 0) {
                    pageEventArgs.Graphics.DrawString(printBlockingEscape, font, brush, 2, 2);
                }
                if (topcoatRemovalEscape.Length > 0) {
                    pageEventArgs.Graphics.DrawString(topcoatRemovalEscape, font, brush, 2, 20);
                }
                pageEventArgs.Graphics.DrawString("Sample Text", font, brush, 50, 50);
                pageEventArgs.Graphics.DrawRectangle(pen, 50, 70, 20, 25);
            }
        }
        public static void PrintTextAndGraphics(BidiSplWrap bidiSpl, string printerName, string printBlockingEscape, string topcoatRemovalEscape)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintController = new StandardPrintController();
            printDocument.PrinterSettings.PrinterName = printerName;
            printDocument.DocumentName = Path.GetFileName(Environment.GetCommandLineArgs()[0]) + " printing";
            printDocument.PrintPage += (sender, args) => OnPrintPageWithBlockingEscapes(args, printBlockingEscape, topcoatRemovalEscape);
            printDocument.Print();

            // we wait here - to ensure that the job is truly in the windows
            // spooler:
            var windowsJobID = WaitForWindowsJobID(bidiSpl, printerName);
        }

        public static void DisplayPrintTicket(PrintTicket printTicket)
        {
            MemoryStream memStream = printTicket.GetXmlStream();
            string xml = Encoding.ASCII.GetString(memStream.GetBuffer(), 0, (int) memStream.Length);
            Console.WriteLine(xml);
        }

        /// <summary>
        /// incoming xml looks like this:
        /// 
        ///  <?xml version="1.0" ?>
        ///  <!-- Printer status xml file.-->
        ///  <PrinterStatus>
        ///    <ClientID>myMachineName_{8CB9F232-7136-42BC-B0FA-EF7D734EAEAD}</ClientID>
        ///    <WindowsJobID>0</WindowsJobID>
        ///    <PrinterJobID>8446</PrinterJobID>
        ///    <ErrorCode>0</ErrorCode>
        ///    <ErrorSeverity>0</ErrorSeverity>
        ///    <ErrorString />
        ///    <DataFromPrinter>
        ///      <![CDATA[ <?xml version="1.0"?>
        ///      <!--smartcard response xml-->
        ///      <SmartcardResponse>
        ///      <Protocol>SCARD_PROTOCOL_RAW</Protocol>
        ///      <State>  </State>
        ///      <Status>SCARD_S_SUCCESS</Status>
        ///      <Base64Data> </Base64Data>
        ///      </SmartcardResponse>]]>
        ///    </DataFromPrinter>
        ///  </PrinterStatus>
        ///  
        /// we retrieve the single-wire smartcard response data from the CDATA section.
        /// 
        /// </summary>
        /// <param name="printerStatusXml"></param>
        /// <returns>a populated SmartcardResponseValues object populated from the xml markup.</returns>
        public static SmartcardResponseValues ParseSmartcardResponseXML(string printerStatusXml) {
            var smartcardResponseValues = new SmartcardResponseValues();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(printerStatusXml);

            XmlNode node;

            node = doc.SelectSingleNode("PrinterStatus/DataFromPrinter/text()");
            string cdataText = node.InnerText;

            XmlDocument smartcardResponseDoc = new XmlDocument();
            smartcardResponseDoc.LoadXml(cdataText);

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Protocol");
            smartcardResponseValues._protocol = node.InnerText.Trim();

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/State");
            var raw_statestring = node.InnerText.Trim();
            var statesList = new List<string>();
            foreach (var stateString in raw_statestring.Split(new char[] { '|', ' ' })) {
                if (!string.IsNullOrWhiteSpace(stateString))
                    statesList.Add(stateString);
            }
            smartcardResponseValues._states = statesList.ToArray();

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Status");
            smartcardResponseValues._status = node.InnerText.Trim();

            node = smartcardResponseDoc.SelectSingleNode("SmartcardResponse/Base64Data");
            smartcardResponseValues._base64string = node.InnerText.Trim();

            if (string.IsNullOrEmpty(smartcardResponseValues._base64string)) {
                smartcardResponseValues._bytesFromBase64String = new byte[0];
            }
            else {
                smartcardResponseValues._bytesFromBase64String =
                    Convert.FromBase64String(smartcardResponseValues._base64string);
            }

            return smartcardResponseValues;
        }

        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// get a string result...like the Win32 FormatMessage() function.
        /// </summary>
        /// <param name="error">a numeric win32 error code</param>
        /// <returns>a localized string corresponding to the given error code</returns>
        ////////////////////////////////////////////////////////////////////////
        public static string Win32ErrorString(int error) {
            string errorString = new Win32Exception(error).Message;
            return errorString;
        }

        public static string GetCurrentTimeString() {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy:MM:dd hh:mm:ss:fff");
        }

        /// <summary>
        /// Cancel a job started by StartJob().
        /// do not throw any exceptions.
        /// </summary>
        public static void CancelJob(BidiSplWrap bidiSpl, int printerJobID, int errorCode) {
            string xmlFormat = strings.PRINTER_ACTION_XML;
            string input = string.Format(xmlFormat, (int) Actions.Cancel, printerJobID, errorCode);
            bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
        }

        /// <summary>
        /// 
        /// Issue an IBidi StartJob with xml markup to start an IBidi print job.
        ///
        /// We do this to have the printer check for supplies, or, just to obtain the
        /// printerJobID for job completion polling later.
        ///
        /// If the printer is out of supplies, throw an exception.
        /// 
        /// </summary>
        public static int StartJob(
            BidiSplWrap bidiSpl,
            string hopperID = "1",
            string cardEjectSide = "default") {

            const int pollSleepMilliseconds = 2000;

            int printerJobID = 0;

            do {
                string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
                PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

                string startJobXml = string.Format(
                    strings.STARTJOB_XML,
                    hopperID,
                    false,  // CheckPrintRibbonSupplies is deprecated. Use \\Printer.SuppliesStatus3:Read to retrieve supplies status 
                    false,  // CheckEmbossSupplies is deprecated. Use \\Printer.SuppliesStatus3:Read to retrieve supplies status 
                    cardEjectSide);

                string printerStatusXML = bidiSpl.SetPrinterData(strings.STARTJOB, startJobXml);
                PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

                if (506 == printerStatusValues._errorCode) {

                    // Printer cannot accept another job as it is busy. Try again.
                    Console.WriteLine("StartJob(): {0} Trying again.", printerStatusValues._errorString);

                    // let the current card process in the printer:
                    Thread.Sleep(pollSleepMilliseconds);
                }
                else if (0 != printerStatusValues._errorCode) {
                    var message = string.Format("StartJob(): {0} {1}",
                        printerStatusValues._errorCode,
                        printerStatusValues._errorString);
                    throw new BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode);
                }
                else {
                    Console.WriteLine("Printer job ID: {0} started.", printerStatusValues._printerJobID);
                    printerJobID = printerStatusValues._printerJobID;
                }

            } while (0 == printerJobID);

            return printerJobID;
        }

        /// <summary>
        /// repeatedly check job status until the job is complete - failed or not.
        /// </summary>
        public static void PollForJobCompletion(BidiSplWrap bidiSpl, int printerJobID) {

            const int pollSleepMilliseconds = 2000;

            while (true) {

                string jobStatusRequest = strings.JOB_STATUS_XML;
                string jobInfoXML = string.Format(jobStatusRequest, printerJobID);
                string jobStatusXML = bidiSpl.GetPrinterData(strings.JOB_STATUS, jobInfoXML);
                JobStatusValues jobStatusValues = Util.ParseJobStatusXML(jobStatusXML);

                Console.WriteLine("Printer job ID: {0} {1}", printerJobID, jobStatusValues._jobState);

                if (jobStatusValues._jobState == strings.JOB_SUCCEEDED ||
                    jobStatusValues._jobState == strings.JOB_FAILED ||
                    jobStatusValues._jobState == strings.JOB_CANCELLED ||
                    jobStatusValues._jobState == strings.CARD_NOT_RETRIEVED ||
                    jobStatusValues._jobState == strings.JOB_NOT_AVAILABLE) {
                    return;
                }

                string printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
                PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

                if ((0 != printerStatusValues._errorCode) && (printerJobID == printerStatusValues._printerJobID)) {
                    string message = string.Format("{0} severity: {1}", printerStatusValues._errorString, printerStatusValues._errorSeverity);
                    throw new BidiException(message, printerStatusValues._printerJobID, printerStatusValues._errorCode);
                }

                Thread.Sleep(pollSleepMilliseconds);
            }
        }


        /// <summary>
        /// incoming xml looks something like this:
        /// 
        /// <?xml version = "1.0" ?>
        /// < HopperStatus >
        ///     <PrinterStatus> Ready </ PrinterStatus>
        ///     <HopperDetection> Available </HopperDetection>
        ///     <HopperInformation Name = "Exception" Type = "Input" Status = "Cannot Detect" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper1" Type = "Input" Status = "Cards Present" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper2" Type = "Input" Status = "Cards Present" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper3" Type = "Input" Status = "Cards Present" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper4" Type = "Input" Status = "Empty" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper5" Type = "Input" Status = "Empty" CardStock = "CardStock1"/>
        ///     <HopperInformation Name = "Hopper6" Type = "Input" Status = "Cards Present" CardStock = "CardStock1"/>
        /// </HopperStatus>  
        /// 
        /// 
        /// </summary>
        /// <param name="hopperStatusXml"></param>
        /// <returns>a populated HOPPER_INFORMATION list populated from the xml markup.</returns>
        public static List<HOPPER_INFORMATION> ParseHopperStatusXml(string hopperStatusXml) {

            var hopperInfoList = new List<HOPPER_INFORMATION>();
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(hopperStatusXml);

            XmlNodeList nodeList = doc.SelectNodes("HopperStatus/HopperInformation");
            if (nodeList == null) {
                throw new Exception("Could not parse hopper status xml");
            }

            int numHoppers = nodeList.Count;
            for (int hopperIndex = 0; hopperIndex < numHoppers; hopperIndex++) {
                HOPPER_INFORMATION hopperInfo = new HOPPER_INFORMATION();

                XmlNode node = nodeList.Item(hopperIndex);
                    if (node == null) {
                        throw new Exception("Xml node corresponding to hopper '" + 
                            hopperIndex + "' could not be retrieved.");
                    }

                XmlAttributeCollection attributeMap = node.Attributes;

                //  name...
                XmlAttribute nameAttributeNode = (XmlAttribute) ((attributeMap != null)
                        ? attributeMap.GetNamedItem("Name")
                        : null);
                if (nameAttributeNode == null) {
                    throw new Exception("'Name' attribute associated with hopper '"
                        + hopperIndex + "' is null");
                }

                hopperInfo._name = nameAttributeNode.Value;

                //  type...
                XmlAttribute typeAttributeNode = (XmlAttribute) ((attributeMap != null)
                        ? attributeMap.GetNamedItem("Type")
                        : null);
                if (typeAttributeNode == null) {
                    throw new Exception("'Type' attribute associated with hopper '"
                        + hopperIndex + "' is null");
                }

                hopperInfo._type = typeAttributeNode.Value;

                //  Status...
                XmlAttribute statusAttributeNode = (XmlAttribute) ((attributeMap != null)
                        ? attributeMap.GetNamedItem("Status")
                        : null);
                if (statusAttributeNode == null) {
                    throw new Exception("'Status' attribute associated with hopper '"
                        + hopperIndex + "' is null");
                }

                hopperInfo._status = statusAttributeNode.Value;

                //  CardStock...
                XmlAttribute cardStockAttributeNode = (XmlAttribute) ((attributeMap != null)
                        ? attributeMap.GetNamedItem("CardStock")
                        : null);
                if (cardStockAttributeNode == null) {
                    throw new Exception("'CardStock' attribute associated with hopper '"
                        + hopperIndex + "' is null");
                }

                hopperInfo._cardStock = cardStockAttributeNode.Value;
                hopperInfoList.Add(hopperInfo);
            }

            return hopperInfoList;
        }


        /// <summary>
        /// compare "actual" FW rev to a "base" FW rev
        /// </summary>
        /// <param name="fwBase"></param>
        /// <param name="fwActual"></param>
        /// <returns> true if "actual" FW rev is greater than or equal to "base" FW rev.</returns>
        //
        public static FirmwareVersion ParseFirmwareRev(string strFwRev) {
            // FW revision is of the following format(s)
            // D3.17.3  or D3.17.3-4

            // break strFwRev into tokens
            string[] tokens = strFwRev.Split('.');

            FirmwareVersion fwRev = new FirmwareVersion();
            fwRev._printerBase = "";
            fwRev._majorVersion = 0;
            fwRev._minorVersion = 0;
            fwRev._deviationVersion = 0;

            // PrinterType
            fwRev._printerBase = tokens[0];

            // Major Revision
            int nMajorRev = 0;
            bool bResult = int.TryParse(tokens[1], out nMajorRev);
            if (bResult) {
                fwRev._majorVersion = (short)nMajorRev;
            }

            // minor revision 
            // FYI - there could be 1 or 2 elements in this array
            string[] minors = tokens[2].Split('-');

            int nMinorRev = 0;
            bResult = int.TryParse(minors[0], out nMinorRev);
            if (bResult) {
                fwRev._minorVersion = (short) nMinorRev;
            }

            // minor revision deviation
            if (minors.Length > 1) {
                int nMinorRevDev = 0;
                bResult = int.TryParse(minors[1], out nMinorRevDev);
                if (bResult) {
                    fwRev._deviationVersion = (short) nMinorRevDev;
                }
            }

            // base minor rev is <= actual minor rev.
            return fwRev;
        }



        /// <summary>
        /// compare "actual" FW rev to a "base" FW rev
        /// </summary>
        /// <param name="fwBase"></param>
        /// <param name="fwActual"></param>
        /// <returns> true if "actual" FW rev is greater than or equal to "base" FW rev.</returns>
        //
        public static bool CompareFirmwareRev(FirmwareVersion fwBase, FirmwareVersion fwActual) {
            // FW revision is of the following format(s)
            // D3.17.3  or D3.17.3-4

            // testing the printer type (i.e. first two characters of the "original" strings)
            if (fwBase._printerBase.CompareTo(fwActual._printerBase) != 0) { 
                // comparing different printer types.  Not a valid comparison
                return false;
            }

            // testing the major revision
            if (fwBase._majorVersion < fwActual._majorVersion) {
                return true;    // no need to continue;
            }
            if (fwBase._majorVersion > fwActual._majorVersion) {
                return false;    // no need to continue;
            }

            // At this point:
            //    the PrinterType is the same, 
            //    the Major rev is the same 
            //    (i.e. D3.17.x-y)
            if (fwBase._minorVersion < fwActual._minorVersion) {
                return true;    // no need to continue;
            }
            if (fwBase._minorVersion > fwActual._minorVersion) {
                return false;    // no need to continue;
            }


            // At this point:
            //    the PrinterType is the same, 
            //    the Major rev is the same and
            //    the Minor rev is the same 
            //    (i.e. D3.17.3-y)
            if (fwBase._deviationVersion > fwActual._deviationVersion) {
                return false;    // no need to continue;
            }

            // base _deviationVersion is <= actual _deviationVersion
            return true;
        }
    }
}