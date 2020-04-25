////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;
import common_java.Util;

public class PrinterState_Main {

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
            } else if (arg.equals("-s") && nameGiven) {
                System.out.println("Set Printer State Demo\r\n");
                String state = "";
                if (i < args.length) {
                   state = args[i++];
                   if (state.equals("on") || state.equals("suspend") || state.equals("off")) {
                       System.out.format("Requested state:%s\n", state);
                   } else {
                       System.err.format("Invalid state, state:%s\n", state);
                       break;
                   }
                } else {
                   System.err.println("-z requires a state\n");
                   break;
                }
                Util.SetPrinterState(printerName, state);
            } else {
                usage();
                System.exit(0);
            }
        }

        // requires -s and state to run
        if (i < 4) {
            usage();
            System.exit(0);
        }
    }

    public static void usage() {
		System.out.println();
		System.out.println(" printer_state.jar demonstrates changing the printer state to offline, online, or suspended.");
		System.out.println();
		System.out.println(" java -jar printer_state.jar -n <printername> -s <on | off | suspend>");
		System.out.println();
		System.out.println("   -n <printername>. Required. Try -n \"XPS Card Printer\".");
		System.out.println("   -s <on | off | suspend>. Required. Changes printer to the specified state:");
		System.out.println("      'on' changes printer state to online.");
		System.out.println("      'off' changes printer state to offline.");
	    System.out.println("      'suspend' changes printer state to suspended.");
	    System.out.println();
	    System.out.println(" java -jar printer_state.jar -n \"XPS Card Printer\" -s on");
	    System.out.println(" Changes the printer state to online.");
	    System.out.println();
	    System.out.println(" java -jar printer_state.jar -n \"XPS Card Printer\" -s off");
	    System.out.println(" Changes the printer state to offline.");
	    System.out.println();
	    System.out.println(" java -jar printer_state.jar -n \"XPS Card Printer\" -s suspend");
	    System.out.println(" Changes the printer state to suspended.");
	    System.out.println();
	}
}
