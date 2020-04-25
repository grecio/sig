////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import java.io.StringReader;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

// sample PrinterStatus xml:
// <PrinterStatus>
//   <ClientID>dcc8233_{89DEA66D-E536-4E8D-BF27-817F35B91005}</ClientID>
//   <WindowsJobID>0</WindowsJobID>
//   <PrinterJobID>8374</PrinterJobID>
//   <ErrorCode>108</ErrorCode>
//   <ErrorSeverity>2</ErrorSeverity>
//   <ErrorString>Message 108: Magstripe read no data.</ErrorString>
//   <DataFromPrinter><![CDATA[  ]]></DataFromPrinter>
// </PrinterStatus>
public class PrinterStatusXML {

    private String mClientID;
    private int mCommand;
    private int mWindowsJobID;
    private int mPrintJobID;
    private int mErrorCode;
    private int mSeverity;
    private String mErrorString;
    private String mXML;
    private String mPrinterData;

    public static String cStringToJavaString(byte b[], int bufferLength) {
        // Converts C string to Java String
        int len = 0;
        while (len < bufferLength && b[len] != 0) {
            ++len;
        }

        if (len > bufferLength) {
            len = bufferLength;
        }

        return new String(b, 0, len);
    }

    public final int PRINTERACTION_CANCEL = 100;
    public final int PRINTERACTION_RESUME = 101;
    public final int PRINTERACTION_RESTART = 102;

    public final String PRINTER_MESSAGES = "\\Printer.PrintMessages:Read";

    public String getClientID() {
        return mClientID;
    }

    public void setClientID(String aClientID) {
        this.mClientID = aClientID;
    }

    public int getWindowsJobID() {
        return mWindowsJobID;
    }

    public void setWindowsJobID(int aWindowsJobID) {
        this.mWindowsJobID = aWindowsJobID;
    }

    public int getSeverity() {
        return mSeverity;
    }

    public void setSeverity(int aSeverity) {
        this.mSeverity = aSeverity;
    }

    public PrinterStatusXML() {
        mCommand = 0;
        mPrintJobID = 0;
        mErrorCode = 0;
        mErrorString = "";
    }

    public void SetCommand(int aCommand) {
        mCommand = aCommand;
    }

    public void SetPrintJobID(int aPrintJobID) {
        mPrintJobID = aPrintJobID;
    }

    public void SetErrorCode(int aErrorCode) {
        mErrorCode = aErrorCode;
    }

    public int GetCommand() {
        return mCommand;
    }

    public int GetPrintJobID() {
        return mPrintJobID;
    }

    public int GetErrorCode() {
        return mErrorCode;
    }

    public String GetErrorMessage() {
        return mErrorString;
    }

    public String toString() {
        return mXML;
    }
    
    public String GetPrinterData() {
    	return mPrinterData;
    }

    public void Parse(byte returnXML[], int sizeOfReturnXML[]) {
        Parse(returnXML, sizeOfReturnXML, true);
    }

    public void Parse(byte returnXML[], int sizeOfReturnXML[], boolean bOutput) {
        if (returnXML != null && sizeOfReturnXML != null && sizeOfReturnXML[0] > 0) {
            mXML = cStringToJavaString(returnXML, sizeOfReturnXML[0]);
            if (mXML.length() > 0) {
                Parse(mXML, bOutput);
            }
        }
    }

    public void Parse(String xmlString, boolean bOutput) {
        try {
            DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
            DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
            InputSource is = new InputSource();
            is.setCharacterStream(new StringReader(xmlString));

            Document doc = dBuilder.parse(is);
            doc.getDocumentElement().normalize();
            NodeList nodeList = doc.getElementsByTagName("PrinterStatus");

            for (int nodeListIndex = 0; nodeListIndex < nodeList.getLength(); nodeListIndex++) {
                Node node = nodeList.item(nodeListIndex);
                if (node.getNodeType() == Node.ELEMENT_NODE) {
                    Element eElement = (Element) node;
                    mClientID = getTagValue("ClientID", eElement);
                    mPrinterData = getTagValue("DataFromPrinter", eElement);
                    mWindowsJobID = Integer.parseInt(getTagValue("WindowsJobID", eElement));
                    mPrintJobID = Integer.parseInt(getTagValue("PrinterJobID", eElement));
                    mErrorCode = Integer.parseInt(getTagValue("ErrorCode", eElement));
                    mSeverity = Integer.parseInt(getTagValue("ErrorSeverity", eElement));
                    if (mErrorCode == 0) {
                        mErrorString = "";
                    } else {
                        mErrorString = getTagValue("ErrorString", eElement);
                    }
                    if (bOutput) {
                        System.out.println("-----------------------");
                        System.out.println("ClientID : " + mClientID);
                        System.out.println("WindowsJobID : " + Integer.toString(mWindowsJobID));
                        System.out.println("PrinterJobID : " + Integer.toString(mPrintJobID));
                        System.out.println("ErrorCode : " + Integer.toString(mErrorCode));
                        System.out.println("ErrorSeverity : " + Integer.toString(mSeverity));
                        System.out.println("ErrorString : " + mErrorString);
                        System.out.println("PrinterData : " + mPrinterData);
                    }
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static String getTagValue(String sTag, Element eElement) {
        NodeList nodeList = eElement.getElementsByTagName(sTag).item(0).getChildNodes();
        Node nodeValue = (Node) nodeList.item(0);
        return nodeValue.getNodeValue();
    }

    public boolean GetPrinterMessages(String printerName) {
        byte returnBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int bufferSize[] = new int[1];
        bufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        int S_OK = 0;
        int hResult = -1; // not S_OK

        // reset buffersize
        bufferSize[0] = bufferSize[0] = XPS_Java_SDK.BUFFSIZE;

        // call driver to get printer messages
        hResult = XpsDriverInteropLib.INSTANCE.GetPrinterStatus(PRINTER_MESSAGES, printerName, returnBuffer,
                bufferSize);

        if (hResult == S_OK) {
            Parse(returnBuffer, bufferSize, false);
        }

        return (hResult == S_OK);
    }
}
