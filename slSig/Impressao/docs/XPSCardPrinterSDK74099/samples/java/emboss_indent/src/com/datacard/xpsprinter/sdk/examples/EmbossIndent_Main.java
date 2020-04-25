////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;

public class EmbossIndent_Main {

    public static void main(String[] args) {
        String printerName = "";
        int i = 0;
        String arg;
        boolean nameGiven = false;
        String inputHopper = "";
        boolean pollCompletion = false;
        
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
            } else if (arg.equals("-i") && nameGiven) {
                if (i < args.length) {
                    inputHopper = args[i++];
                    ValidateHopperID(inputHopper);
                } else {
                    System.err.println("-i requires a 'hopper'.");
                    System.exit(-1);
                    break;
                }
            } else if (arg.equals("-c") && nameGiven) {
                pollCompletion = true;
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

        JavaEmboss javaEmboss = new JavaEmboss(printerName);
        javaEmboss.EmbossDemo(inputHopper, pollCompletion);

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

    
    public static void usage() {
        System.out.println();
        System.out.println("emboss_indent.jar demonstrates the emboss and indent escapes, input hopper");
        System.out.println("selection, and disabling topping option.");
        System.out.println("Emboss and indent data is hardcoded. It can also display print ticket data ");
        System.out.println("and poll printer for job completion.");
        System.out.println();
        System.out.println(" java -jar emboss_indent.jar -n <printername> [-i <input hopper>] [-c]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -i <input hopper>. Optional. Defaults to \"1\"");
        System.out.println("  -c Poll for job completion.");
        System.out.println();
    }
}
