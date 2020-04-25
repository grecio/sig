////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import java.io.IOException;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;
import common_java.JavaPrint;
import common_java.PrinterStatusXML;
import common_java.Util;

public class Smartcard_Singlewire {

    private String mPrinterName;

    public static final int ChipConnection_Contact = 0;
    public static final int ChipConnection_Contactless = 1;

    public final int SCARD_SHARE_EXCLUSIVE = 1;
    public final int SCARD_SHARE_SHARED = 2;
    public final int SCARD_SHARE_DIRECT = 3;
    public final int SCARD_LEAVE_CARD = 0;
    public final int SCARD_RESET_CARD = 1;
    public final int SCARD_UNPOWER_CARD = 2;
    public final int SCARD_EJECT_CARD = 3;
    public final int SCARD_UNKNOWN = 0;
    public final int SCARD_ABSENT = 1;
    public final int SCARD_PRESENT = 2;
    public final int SCARD_SWALLOWED = 3;
    public final int SCARD_POWERED = 4;
    public final int SCARD_NEGOTIABLE = 5;
    public final int SCARD_SPECIFIC = 6;
    public final int SCARD_PROTOCOL_UNDEFINED = 0x00000000;
    public final int SCARD_PROTOCOL_T0 = 0x00000001;
    public final int SCARD_PROTOCOL_T1 = 0x00000002;
    public final int SCARD_PROTOCOL_RAW = 0x00010000;

    public static final int SCARD_ATTR_VENDOR_NAME = 65792;
    public static final int SCARD_ATTR_VENDOR_IFD_SERIAL_NO = 65795;
    public static final int SCARD_ATTR_VENDOR_IFD_TYPE = 65793;
    public static final int SCARD_ATTR_VENDOR_IFD_VERSION = 65794;

    static String toHex(byte[] digest, int size) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < size; i++) {
            sb.append(String.format("%1$02X ", digest[i]));
        }
        return sb.toString();
    }

    public Smartcard_Singlewire(String aPrinterName) {
        mPrinterName = aPrinterName;
    }

    public boolean DemoSmartCard(
            boolean bDoPrint,
            boolean parkBack,
            boolean pollCompletion,
            String  inputHopper,
            String  cardEjectSide) {
        final int S_OK = 0;
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        boolean bDisplayError = true;
        boolean bJobStarted = false;
        boolean bJobHasBeenCanceled = false;
        int printerJobID;

        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        String strHopperID = "1";
        String strCardEjectSide = "Default";

        int hResult = XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName, 
                !inputHopper.equals("") ? inputHopper : strHopperID, 
                !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide,
        		returnXML, sizeOfReturnXML);
        if (S_OK == hResult) {
            bJobStarted = true;
            sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
            if (S_OK == XpsDriverInteropLib.INSTANCE.SmartCardPark(
                    mPrinterName,
                    returnXML,
                    sizeOfReturnXML,
                    parkBack)) {

                // we have parked the card in the smartcard reader.
                printerStatusXml.Parse(returnXML, sizeOfReturnXML);
                printerJobID = printerStatusXml.GetPrintJobID();
                System.out.format("smartcard PARK succeed. PrintJobID: %s\n", printerJobID);

                // simulate smartcard personalization:
                System.out.println();
                System.out.println("Demo singlewire contact smartcard");
                DemoSmartCard_Singlewire(
                        bDoPrint,
                        Smartcard_Singlewire.ChipConnection_Contact);
                System.out.println();
                System.out.println("Demo singlewire contactless smartcard");
                DemoSmartCard_Singlewire(
                        bDoPrint,
                        Smartcard_Singlewire.ChipConnection_Contactless);

                byte smartcardUserResponse[] = new byte[2];
                try {
                    Thread.sleep(2000);
                    System.out.println("Press Enter to resume or 'c' to cancel the printjob: ");
                    System.in.read(smartcardUserResponse, 0, 1);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                } catch (IOException e) {
                    e.printStackTrace();
                }

                sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

                if (smartcardUserResponse[0] != 'c') {
                    System.out.println("Smartcard personalization succeeded. Resuming print job.");
                    if (XpsDriverInteropLib.INSTANCE.ResumeJob(mPrinterName,
                            printerStatusXml.GetPrintJobID(), printerStatusXml.GetErrorCode(), returnXML,
                            sizeOfReturnXML) == S_OK) {
                        bDisplayError = false;
                    }
                } else {
                    // when smartcard personalization fails, cancel the print
                    // job:
                    hResult = XpsDriverInteropLib.INSTANCE.CancelJob(mPrinterName,
                            printerStatusXml.GetPrintJobID(), printerStatusXml.GetErrorCode(), returnXML,
                            sizeOfReturnXML);
                    bDoPrint = false;
                    bJobHasBeenCanceled = true;
                    bDisplayError = (hResult != S_OK);
                }
            }
        }

        if (bDisplayError) {
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);
            System.out.format("Request failed. Reason: %s\n", printerStatusXml.GetErrorMessage());

            System.out.println("Cancelling operation.");
            XpsDriverInteropLib.INSTANCE.SendResponseToPrinter(mPrinterName,
                    printerStatusXml.PRINTERACTION_CANCEL, printerStatusXml.GetPrintJobID(),
                    printerStatusXml.GetErrorCode());
            bJobHasBeenCanceled = true;

        } else if (bDoPrint) {
        	String blockFront = null;
        	String blockBack = null;
        	if (parkBack) {
        		blockBack = "chip";
        	} else {
        		blockFront = "chip";
        	}
            JavaPrint javaPrint = new JavaPrint(
                    mPrinterName,
                    true,  // duplex
                    false, // not portrait
                    "1",   // copies
                    false, // no magStripe
                    blockFront, // blocking front
                    blockBack, // blocking back
                    true);   // print sample text
            javaPrint.PrintDemo();

            Util.WaitUntilJobSpooled(mPrinterName);
        }

        if (bJobStarted && !bJobHasBeenCanceled) {
            System.out.println("Calling EndJob():");
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }

        if (pollCompletion) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }

        return (hResult == S_OK);
    }

    public boolean PersonalizeSmartcard(int[] protocol) {
        int S_OK = 0;
        int states[] = new int[10];
        int numberStatesReturned[] = new int[1];
        numberStatesReturned[0] = 10 * 4; // DWORD is 4 bytes

        byte returnATRBytes[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int numATRBytesReturned[] = new int[1];
        numATRBytesReturned[0] = XPS_Java_SDK.BUFFSIZE;

        // get SCard status
        int sCardResult = XpsDriverInteropLib.INSTANCE.SCardStatus(mPrinterName, states,
                numberStatesReturned, protocol, returnATRBytes, numATRBytesReturned);

        if (sCardResult != 0) {
            System.out.format("SCardStatus := %d\n", sCardResult);
            return false;
        }

        System.out.format("SCardStatus numstates= %d, protocol:=%d, numATR:=%d, ATR:=%s\n",
                numberStatesReturned[0] / 4, protocol[0], numATRBytesReturned[0],
                PrinterStatusXML.cStringToJavaString(returnATRBytes, numATRBytesReturned[0]));

        if (numberStatesReturned[0] / 4 > 10 || numATRBytesReturned[0] > XPS_Java_SDK.BUFFSIZE) {
            // try get status again
            states = new int[numberStatesReturned[0]];
            returnATRBytes = new byte[numATRBytesReturned[0]];
            sCardResult = XpsDriverInteropLib.INSTANCE.SCardStatus(mPrinterName, states,
                    numberStatesReturned, protocol, returnATRBytes, numATRBytesReturned);
            System.out.format("SCardStatus numstates= %d, protocol:=%d, numATR:=%d, ATR:=%s\n",
                    numberStatesReturned[0] / 4, protocol[0], numATRBytesReturned[0],
                    PrinterStatusXML.cStringToJavaString(returnATRBytes, numATRBytesReturned[0]));
        }

        // display state information
        for (int i = 0; i < numberStatesReturned[0] / 4; i++) {

            if (states[i] == SCARD_ABSENT) {
                System.out.format("SCardStatus() state[%d] =SCARD_ABSENT\n", i);
                return false;
            }
            System.out.format("SCardStatus() state[%d] = %d\n", i, states[i]);
        }

        System.out.println("SCardStatus() ATBBytes (hex):\n");
        System.out.println(toHex(returnATRBytes, numATRBytesReturned[0]));

        // try all these "get attribute" items
        byte attrBytesBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int attrBytesBufferSize[] = new int[1];
        attrBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        sCardResult = GetSmartCardAttributes(SCARD_ATTR_VENDOR_NAME, attrBytesBuffer,
                attrBytesBufferSize);
        if (sCardResult == S_OK) {
            System.out.format("SCARD_ATTR_VENDOR_NAME := %s\n",
                    PrinterStatusXML.cStringToJavaString(attrBytesBuffer, attrBytesBufferSize[0]));
        } else {
            System.out.format("SCARD_ATTR_VENDOR_NAME := ***error*** %d\n", sCardResult);
        }

        attrBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        sCardResult = GetSmartCardAttributes(SCARD_ATTR_VENDOR_IFD_SERIAL_NO, attrBytesBuffer,
                attrBytesBufferSize);
        if (sCardResult == S_OK) {
            System.out.format("SCARD_ATTR_VENDOR_IFD_SERIAL_NO := %s\n",
                    PrinterStatusXML.cStringToJavaString(attrBytesBuffer, attrBytesBufferSize[0]));
        } else {
            System.out.format("SCARD_ATTR_VENDOR_IFD_SERIAL_NO attribute := ***error***%d\n",
                    sCardResult);
        }
        attrBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        sCardResult = GetSmartCardAttributes(SCARD_ATTR_VENDOR_IFD_TYPE, attrBytesBuffer,
                attrBytesBufferSize);
        if (sCardResult == S_OK) {
            System.out.format("SCARD_ATTR_VENDOR_IFD_TYPE := %s\n",
                    PrinterStatusXML.cStringToJavaString(attrBytesBuffer, attrBytesBufferSize[0]));
        } else {
            System.out.format("SCARD_ATTR_VENDOR_IFD_TYPE attribute :=***error*** %d\n", sCardResult);
        }
        attrBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        sCardResult = GetSmartCardAttributes(SCARD_ATTR_VENDOR_IFD_VERSION, attrBytesBuffer,
                attrBytesBufferSize);
        if (sCardResult == S_OK) {
            int byteCount = attrBytesBufferSize[0];
            short minorVersion = byteCount > 0 ? attrBytesBuffer[0] : 0;
            short majorVersion = byteCount > 1 ? attrBytesBuffer[1] : 0;
            short buildNumber = 0;
            if (byteCount > 3) {
                buildNumber = (short) ((attrBytesBuffer[3] << 8) + attrBytesBuffer[2]);

            }
            System.out.format(
                    "SCARD_ATTR_VENDOR_IFD_VERSION \n major:= %d\n minor:= %d\n build:= %d\n",
                    majorVersion, minorVersion, buildNumber);
        } else {
            System.out
                    .format("SCARD_ATTR_VENDOR_IFD_VERSION attribute :=***error***%d\n", sCardResult);
        }

        // create a byte vector for the upcoming SCardTransmit() method.
        // these particular bytes should function with this type of contact chip:
        //
        // MPCOS-EMV 16k
        // GEMPLUS
        // Datacard part number 806062-002
        //
        byte sendBytesBuffer[]
                = {0x00, (byte) 0xA4, 0x00, 0x00};
        int sendBytesBufferSize = 4;

        // for those bytes, we should receive 0x61, 0x12
        byte receivedBytesBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int receivedBytesBufferSize[] = new int[1];
        receivedBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;

        sCardResult = XpsDriverInteropLib.INSTANCE.SCardTransmit(mPrinterName, sendBytesBuffer,
                sendBytesBufferSize, receivedBytesBuffer, receivedBytesBufferSize);

        System.out.println("SCardTransmit()");

        if (sCardResult == S_OK && receivedBytesBufferSize[0] > 0) {
            System.out.format(" %d bytes received: %s", receivedBytesBufferSize[0],
                    toHex(receivedBytesBuffer, receivedBytesBufferSize[0]));
        }

        // send the bytes 0x00 0xC0 0x00 0x00 0x12:
        byte sendBytesBuffer2[]
                = {0x00, (byte) 0xC0, 0x00, 0x00, 0x12};
        sendBytesBufferSize = 5;

        receivedBytesBufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        // for those bytes, we should receive
        // 0x85, 0x10, 0x80, 0x01, 0x3F, 0x00, 0x38, 0x00, 0x00, 0x00,
        // 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x6B, 0x90, 0x00
        sCardResult = XpsDriverInteropLib.INSTANCE.SCardTransmit(mPrinterName, sendBytesBuffer2,
                sendBytesBufferSize, receivedBytesBuffer, receivedBytesBufferSize);

        System.out.println("SCardTransmit()");
        if (sCardResult == S_OK && receivedBytesBufferSize[0] > 0) {
            System.out.format(" %d bytes received: %s\n", receivedBytesBufferSize[0],
                    toHex(receivedBytesBuffer, receivedBytesBufferSize[0]));
        }

        return true;
    }

    private int GetSmartCardAttributes(int attribute, byte attrBytesBuffer[],
            int attrBytesBufferSize[]) {
        int S_OK = 0;

        int scardResult = XpsDriverInteropLib.INSTANCE.SCardGetAttrib(mPrinterName, attribute,
                attrBytesBuffer, attrBytesBufferSize);
        int returnBytes = attrBytesBufferSize[0];
        if (scardResult == S_OK && returnBytes > XPS_Java_SDK.BUFFSIZE) {
            // buffer too small, try it again
            attrBytesBuffer = new byte[returnBytes];
            attrBytesBufferSize[0] = returnBytes;
            scardResult = XpsDriverInteropLib.INSTANCE.SCardGetAttrib(mPrinterName, attribute,
                    attrBytesBuffer, attrBytesBufferSize);
        }
        return scardResult;
    }

    public boolean DemoSmartCard_Singlewire(boolean bDoPrint, int chipConnection) {

        int S_OK = 0;

        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

        // simulate smartcard personalization:
        int protocol[] = new int[1];
        protocol[0] = SCARD_PROTOCOL_UNDEFINED;

        // Connect to chips
        int sCardResult = XpsDriverInteropLib.INSTANCE.SCardConnect(mPrinterName, chipConnection,
                protocol);

        System.out.format("SCardConnect return %d, protocol %d\n", sCardResult, protocol[0]);

        if (sCardResult == S_OK) {
            if (PersonalizeSmartcard(protocol) == false) {
                // do whatever error recover
            }
        }

        // Disconnect SCard
        sCardResult = XpsDriverInteropLib.INSTANCE.SCardDisConnect(mPrinterName, SCARD_LEAVE_CARD);
        System.out.format("SCardDisConnect return %d\n", sCardResult);

        return (sCardResult == S_OK);
    }
}
