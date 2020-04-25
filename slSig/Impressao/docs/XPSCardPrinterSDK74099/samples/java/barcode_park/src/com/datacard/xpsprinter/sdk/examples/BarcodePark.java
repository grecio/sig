////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import java.io.IOException;
import common_java.PrinterStatusXML;
import common_java.JavaPrint;
import common_java.Util;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class BarcodePark {

    private final String mPrinterName;

    public BarcodePark(String aPrinterName) {
        mPrinterName = aPrinterName;
    }

    public boolean BarcodeReadAction() {
        byte barcodeUserResponse[] = new byte[2];
        try {
            Thread.sleep(2000);
            System.out.println("A card has been parked in barcode reader station");
            System.out.println("This is where barcode read happens.");
            System.out.println("Type 'y' to simulate a successful barcode read.");
            System.out.println("Type 'n' to simulate a failed barcode read.");
            System.in.read(barcodeUserResponse, 0, 1);
        } catch (InterruptedException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
        if (barcodeUserResponse[0] == 'y') {
            System.out.println("Barcode read succeeded. Resuming print job.");
            return true;
        } else {
            System.out.println("Barcode read failed. Cancelling print job.");
            return false;
        }
    }

    public boolean DemoBarcodePark(boolean parkBack, boolean doPrint, boolean pollCompletion,
            String inputHopper, String cardEjectSide) {
        final int S_OK = 0;
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        boolean bJobStarted = false;
        boolean bJobHasBeenCanceled = false;
        int printerJobID;

        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        String strHopperID = "1";
        String strCardEjectSide = "Default";

        // Start job
        int hResult = XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName, 
                !inputHopper.equals("") ? inputHopper : strHopperID, 
                !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide, 
                returnXML, sizeOfReturnXML);
        if (S_OK == hResult) {
            bJobStarted = true;
            sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

            // Barcode park action
            if (S_OK == XpsDriverInteropLib.INSTANCE.DoBarcodePark(mPrinterName, returnXML, sizeOfReturnXML,
                    parkBack)) {

                // we have parked the card in the barcode reader.
                printerStatusXml.Parse(returnXML, sizeOfReturnXML);
                printerJobID = printerStatusXml.GetPrintJobID();
                System.out.format("Barcode PARK succeed. PrintJobID: %s\n", printerJobID);

                // Prompt for simulated success or failure of barcode read
                boolean readSuccess = BarcodeReadAction();
                sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

                if (readSuccess) { // simulated successful read
                    if (XpsDriverInteropLib.INSTANCE.ResumeJob(mPrinterName, printerStatusXml.GetPrintJobID(),
                            printerStatusXml.GetErrorCode(), returnXML, sizeOfReturnXML) == S_OK) {
                        if (doPrint) {
                            JavaPrint javaPrint = new JavaPrint(
                                    mPrinterName, 
                                    false, // no duplex                              
                                    false, // no portrait
                                    "1"  , // one copy
                                    false, // no magstripe
                                    null,  // no blocking front
                                    null,  // no no blocking back);
                                    true); // print sample text
                            javaPrint.PrintDemo();

                            Util.WaitUntilJobSpooled(mPrinterName);
                        }
                    }
                } else { // simulated failed barcode read, cancel the print job
                    hResult = XpsDriverInteropLib.INSTANCE.CancelJob(mPrinterName, printerStatusXml.GetPrintJobID(),
                            printerStatusXml.GetErrorCode(), returnXML, sizeOfReturnXML);
                    bJobHasBeenCanceled = true;
                }
            }
        } else {
            System.out.format("StartJob2() failed, hResult:%d", hResult);
            return false;
        }

        if (bJobStarted && !bJobHasBeenCanceled) {
            System.out.println("Calling EndJob():");
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }

        // (optional) polling Job status
        if (bJobStarted && (true == pollCompletion)) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }

        return (hResult == S_OK);
    }
}
