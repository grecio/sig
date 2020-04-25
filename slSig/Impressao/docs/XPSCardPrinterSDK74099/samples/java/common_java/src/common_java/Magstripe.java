////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import common_java.JavaPrint;
import common_java.PrinterStatusXML;
import common_java.Util;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class Magstripe {
    private String mPrinterName;

    public Magstripe(String printerName) {
        mPrinterName = printerName;
    }

    public String Read(boolean doPrintDemo, boolean pollCompletion, String inputHopper, String cardEjectSide) {
        // allocate buffer for 3 tracks of magstripe data:
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        final int S_OK = 0;
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        boolean bDisplayError = true;
        boolean bJobStarted = false;
        String returnValue = "";

        if (S_OK == XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName, inputHopper, cardEjectSide,
                returnXML, sizeOfReturnXML)) {
            // reset the buffer:
            bJobStarted = true;
            // get PrintJobID
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);

            sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
            if (S_OK == XpsDriverInteropLib.INSTANCE.MagstripeRead(mPrinterName,
                    returnXML, sizeOfReturnXML)) {
                returnValue = PrinterStatusXML.cStringToJavaString(returnXML,
                        sizeOfReturnXML[0]);
                System.out.format(
                        "'%s' MagStripe Read return length: %d\n\n%s\n\n",
                        mPrinterName, sizeOfReturnXML[0], returnValue);
                System.out.println("Magstripe read succeed.");
                bDisplayError = false;
            }
        }

        if (bDisplayError) {
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);

            // always cancel error condition:
            printerStatusXml.SetCommand(printerStatusXml.PRINTERACTION_CANCEL);

            returnValue = printerStatusXml.GetErrorMessage();
            System.out.format(
                    "Magstripe Read error. Printer return: %s\n Cancel operation\n\n",
                    returnValue);

            XpsDriverInteropLib.INSTANCE.SendResponseToPrinter(mPrinterName,
                    printerStatusXml.GetCommand(), printerStatusXml.GetPrintJobID(),
                    printerStatusXml.GetErrorCode());
        } else if (doPrintDemo) {
        	JavaPrint javaPrint = new JavaPrint(
            		mPrinterName,
            		true,
                    false,
                    "1",
                    false,
                    null,
                    null,
                    true);
            javaPrint.PrintDemo();

            // need to wait for data get spooler before calling EndJob
            // printer Status will fill up Windows Job Id when spooler
            // completed the data
            Util.WaitUntilJobSpooled(mPrinterName);
        }

        if (bJobStarted) {
            System.out.println("EndJob called");
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }

        if (pollCompletion) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }
        
        return returnValue;
    }
    
    public void Encode(String track1Data, String track2Data, String track3Data,
            boolean doMagstripeRead, boolean doPrintDemo, boolean pollCompletion, String inputHopper, String cardEjectSide) {
        final int S_OK = 0;
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        boolean bDisplayError = true;
        boolean bJobStarted = false;
        String returnValue;

        PrinterStatusXML printerStatusXml = new PrinterStatusXML();

        if (S_OK == XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName, inputHopper, cardEjectSide,
                returnXML, sizeOfReturnXML)) {
            bJobStarted = true;

            // get PrintJobID
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);

            sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
            if (S_OK == XpsDriverInteropLib.INSTANCE.MagstripeEncode(mPrinterName,
                    track1Data, track1Data.length(), track2Data,
                    track2Data.length(), track3Data, track3Data.length(), returnXML,
                    sizeOfReturnXML)) {
                System.out.format("'%s' Magstripe Encode Succeed\n", mPrinterName);
                bDisplayError = false;

                // Do Magstripe Read
                if (doMagstripeRead) {
                    // reset the buffer:
                    sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
                    if (S_OK == XpsDriverInteropLib.INSTANCE.MagstripeRead(
                            mPrinterName, returnXML, sizeOfReturnXML)) {
                        returnValue = PrinterStatusXML.cStringToJavaString(returnXML,
                                sizeOfReturnXML[0]);
                        System.out.format(
                                "'%s' MagStripe Read return length: %d\n\n%s\n\n",
                                mPrinterName, sizeOfReturnXML[0], returnValue);
                        bDisplayError = false;
                    } else {
                        // Magstripe Read has error
                        bDisplayError = true;
                    }
                }
            }
        }

        // Any error needs to display
        if (bDisplayError) {
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);

            // always cancel error condition
            printerStatusXml.SetCommand(printerStatusXml.PRINTERACTION_CANCEL);

            returnValue = printerStatusXml.GetErrorMessage();
            System.out.format(
                    "Magstripe operation error. Printer return: %s\n Cancel Operation\n\n",
                    returnValue);

            XpsDriverInteropLib.INSTANCE.SendResponseToPrinter(mPrinterName,
                    printerStatusXml.GetCommand(), printerStatusXml.GetPrintJobID(),
                    printerStatusXml.GetErrorCode());
        } else if (doPrintDemo) {
            // test print
            JavaPrint javaPrint = new JavaPrint(
            		mPrinterName,
            		true,
                    false,
                    "1",
                    false,
                    null,
                    null,
                    true);
            javaPrint.PrintDemo();

            // need to wait for data get spooler before calling EndJob
            // printer Status will fill up Windows Job Id when spooler
            // completed the data
            Util.WaitUntilJobSpooled(mPrinterName);
        }

        // this sample always cancel job when it has error
        // thus only call EndJob on succeed job
        if (bJobStarted && !bDisplayError) {
            System.out.println("EndJob called");
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }

        if (pollCompletion) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }
    }
    
    public int LaserMagstripeEncode () {
    	final int S_OK = 0;
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        boolean bDisplayError = true;
        String returnValue;
        
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();

        String track1Data = " !#$%&\'\"()*+,-./0123456789:;<=>@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_ !#$%&\'()*+,";
        String track2Data ="0123456789:;<=>0123456789:;<=>0123456";
        String track3Data = "0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=";
        
        if (S_OK == XpsDriverInteropLib.INSTANCE.MagstripeEncode(mPrinterName,
                track1Data, track1Data.length(), track2Data,
                track2Data.length(), track3Data, track3Data.length(), returnXML,
                sizeOfReturnXML)) {
            System.out.format("'%s' Magstripe Encode Succeed\n", mPrinterName);
            bDisplayError = false;
        }
        
        // Any error needs to display
        if (bDisplayError) {
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);

            // always cancel error condition
            printerStatusXml.SetCommand(printerStatusXml.PRINTERACTION_CANCEL);

            returnValue = printerStatusXml.GetErrorMessage();
            System.out.format(
                    "Magstripe operation error. Printer return: %s\n Cancel Operation\n\n",
                    returnValue);

            XpsDriverInteropLib.INSTANCE.SendResponseToPrinter(mPrinterName,
                    printerStatusXml.GetCommand(), printerStatusXml.GetPrintJobID(),
                    printerStatusXml.GetErrorCode());
        }
    	
    	return 0;
    }
}
