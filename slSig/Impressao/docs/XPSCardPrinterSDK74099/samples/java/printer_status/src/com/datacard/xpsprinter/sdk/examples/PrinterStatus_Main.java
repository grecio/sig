////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;
import common_java.PrinterOptions;
import common_java.PrinterCounterStatus;
import common_java.PrinterSuppliesStatus;
import common_java.PrinterStatusXML;
import common_java.Util;

public class PrinterStatus_Main {

    public static void main(String[] args) {
        String printerName = "";
        int i = 0;
        String arg;
        boolean runJob = false;
        boolean nameGiven = false;
        String inputHopper   = "";
        String cardEjectSide = "";

        new XPS_Java_SDK(); // construct and extract interop dll

        // parse the command line arguments
        while (i < args.length) {
            arg = args[i++];

            if (arg.equalsIgnoreCase("-n")) {
                if (i < args.length) {
                    printerName = args[i++];
                    nameGiven = true;
                } else {
                    System.err.println("-n requires a printername");
                    break;
                }
            } else if (arg.equals("-j") && nameGiven) {
                runJob = true;
            } else if (arg.equalsIgnoreCase("-f")) {
                if (i < args.length) {
                    cardEjectSide = args[i++];
                    ValidateCardEjectSide(cardEjectSide);
                } else {
                    System.err.println("-f requires a 'side'.");
                    System.exit(-1);
                    break;
                }
            } else if (arg.equalsIgnoreCase("-i")) {
                if (i < args.length) {
                    inputHopper = args[i++];
                    ValidateHopperID(inputHopper);
                } else {
                    System.err.println("-i requires a 'hopper'.");
                    System.exit(-1);
                    break;
                }
            } else {
                usage();
                System.exit(0);
            }
        }

        // does not require an option to run
        if (i < 2) {
            usage();
            System.exit(0);
        }

        PrinterOptions printerOptions = new PrinterOptions(printerName);
        printerOptions.GetPrinterOptions();
        PrinterCounterStatus printerCounters = new PrinterCounterStatus(printerName);
        printerCounters.GetPrinterCounterStatus();
        PrinterSuppliesStatus printerSuppliesStatus = new PrinterSuppliesStatus(printerName);
        printerSuppliesStatus.GetPrinterSuppliesStatus();
        if (runJob) {
            byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
            int sizeOfReturnXML[] = new int[1];
            sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
            PrinterStatusXML printerStatusXml = new PrinterStatusXML();

            // Start job, end job, poll for completion
            String strHopperID = "1";
            String strCardEjectSide = "Default";

            XpsDriverInteropLib.INSTANCE.StartJob2(printerName, 
                    !inputHopper.equals("") ? inputHopper : strHopperID, 
                    !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide,
                    returnXML, sizeOfReturnXML);
            printerStatusXml.Parse(returnXML, sizeOfReturnXML);
            XpsDriverInteropLib.INSTANCE.EndJob(printerName);
            Util.PollForJobCompletion(printerName, printerStatusXml.GetPrintJobID());
        }
    }


    private static void ValidateHopperID(String hopperID)
    {
        if (!hopperID.equals("")) {
            if (
                !hopperID.equals("1") &&
                !hopperID.equals("2") &&
                !hopperID.equals("3") &&
                !hopperID.equals("4") &&
                !hopperID.equals("5") &&
                !hopperID.equals("6") &&
                !hopperID.equals("exception") ) {
                System.out.println( "invalid hopperID=" + hopperID + ".");
                System.exit(-1);
            }
        }
    }

    private static void ValidateCardEjectSide(String cardEjectSide)
    {
        if (!cardEjectSide.equals(""))  {
            if ( !cardEjectSide.equals("front") && !cardEjectSide.equals("back") ) {
                System.out.println("invalid cardEjectSide=" + cardEjectSide);
                System.exit(-1);
            }
        }
    }    


    public static void usage() {
        System.out.println();
        System.out.println(" printer_status.jar demonstrates using interactive mode to retrieve printer options,");
        System.out.println(" printer messages, and job status.");
        System.out.println();
        System.out.println(" java -jar printer_status.jar -n <printername> [-j] [-f <side>] [-i <inputHopper>]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -j Submit a card job and display job status. Optional.");
        System.out.println("  -f <Front | Back>. Flip card on output.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
        System.out.println(" java -jar printer_status.jar -n \"XPS Card Printer\" -j");
        System.out.println();
        System.out.println(" Retrieves printer options, counts, and supplies, starts a job, ");
        System.out.println(" ends the job, and polls job status.");
        System.out.println();
    }
}