////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import com.datacard.xpsprinter.sdk.examples.Smartcard;
import common_java.XPS_Java_SDK;

public class Smartcard_Main {
    
    public static void main(String[] args) {
        int i = 0;
        String arg;
        
    	String  printerName     = "";
        boolean nameGiven       = false;
        boolean print           = false;
        boolean parkBack        = false;
        boolean pollCompletion  = false;
        String inputHopper      = "";
        String cardEjectSide    = "";
        
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
            } else if (arg.equals("-p") && nameGiven) {
            	print = true;
            } else if (arg.equals("-b") && nameGiven) {
            	parkBack = true;
            } else if (arg.equals("-c") && nameGiven) {
            	pollCompletion = true;
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

        if (i < 2) {
            usage();
            System.exit(0);
        }

        Smartcard smartCard = new Smartcard(printerName);
        smartCard.DemoSmartCard(print, parkBack, pollCompletion, inputHopper, cardEjectSide);

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
        System.out.println("smartcard.jar demonstrates interactive mode parking a card in the smart card");
        System.out.println("station, moving the card from the station, and options to print and poll for");
        System.out.println("job completion. This sample uses hardcoded data for printing.");
        System.out.println();
        System.out.println(" java -jar smartcard.jar -n <printername> [-p] [-b] [-c] [-f <side>] [-i <inputhopper>]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -p Print sample images on one or both sides of the card");
        System.out.println("     depending on the printer used.");
        System.out.println("  -b use back of card for smartcard chip.");
        System.out.println("  -c Poll for job completion.");
        System.out.println("  -f <Front | Back>. Flip card on output.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
        System.out.println("java -jar smartcard.jar -n \"XPS Card Printer\"");
        System.out.println("Parks a card in the printer smart card station, asks you to continue or reject");
        System.out.println("and then does what you requested.");
        System.out.println();
        System.out.println("java -jar smartcard.jar -n \"XPS Card Printer\" -p -c");
        System.out.println("Parks a card in printer smart card station, moves the card from the station,");
        System.out.println("prints sample images on one or both sides of the card, and polls and displays");
        System.out.println("job status.");
        System.out.println();
    }
}
