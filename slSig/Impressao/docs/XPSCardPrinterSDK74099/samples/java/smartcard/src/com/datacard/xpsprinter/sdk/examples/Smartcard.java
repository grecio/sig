////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import java.io.IOException;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;
import common_java.PrinterStatusXML;
import common_java.JavaPrint;
import common_java.Util;

public class Smartcard {

    private final String mPrinterName;

    public Smartcard(String aPrinterName) {
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
                    true); // do sample text
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
}
