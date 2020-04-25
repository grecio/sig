////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;
import common_java.Util;

public class PrinterControl_Main {

    public static void main(String[] args) {
        String printerName = "";
        int i = 0;
        String arg;
        boolean nameGiven = false;
        
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
            } else if (arg.equals("-c") && nameGiven) {
                System.out.println("Cancel All Jobs Demo");
                Util.CancelAllJobs(printerName);
            } else if (arg.equals("-r") && nameGiven) {
                System.out.println("Restart Printer Demo");
                Util.RestartPrinter(printerName);
            } else if (arg.equals("-e") && nameGiven) {
                System.out.println("Reset Card Counts Demo");
                Util.ResetCardCounters(printerName);
            } else if (arg.equals("-a") && nameGiven) {
                System.out.println("Set ColorAdjust Settings Demo");
                ColorAdjust colorAdjust = new ColorAdjust(printerName);
                colorAdjust.SetColorAdjust();
            } else if (arg.equals("-d") && nameGiven) {
                System.out.println("Set Default ColorAdjust Settings Demo");
                ColorAdjust colorAdjust = new ColorAdjust(printerName);
                colorAdjust.DefaultColorAdjust();
            } else {
                usage();
                System.exit(0);
            }
        }

        // requires an option to run
        if (i < 3) {
            usage();
            System.exit(0);
        }
    }

    public static void usage() {
        System.out.println();
        System.out.println(" printer_control.jar controls the printer through the printer driver.");
        System.out.println();
        System.out.println(" java -jar printer_control.jar -n <printername> <command>");
        System.out.println();
        System.out.println("   -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("   -c cancel all jobs: Send a command to the printer to clear all print jobs");
        System.out.println("      from the printer. This does not delete spooled Windows");
        System.out.println("      print jobs.");
        System.out.println("   -r restart the printer.");
        System.out.println("   -e reset all card counts.");
        System.out.println("   -a set colorAdjust settings.  This sample changes only the Red settings. Blue and Green are unchanged");
        System.out.println("   -d set default colorAdjust settings. This sample only resets Red and Green.  Blue is unchanged.");
        System.out.println();
    }
}