////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import com.datacard.xpsprinter.sdk.examples.Smartcard_Singlewire;
import common_java.XPS_Java_SDK;

public class Smartcard_Singlewire_Main {
    
    public static void main(String[] args) {
        int i = 0;
        String arg;
        
    	String  printerName     = "";
        boolean nameGiven       = false;
        boolean print          = false;
        boolean parkBack         = false;
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

        Smartcard_Singlewire smartCard = new Smartcard_Singlewire(printerName);
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
        System.out.println("smartcard_singlewire.jar demonstrates smartcard personalization using the");
        System.out.println("single-wire embedded reader. Once staged, the app connects to a contact");
        System.out.println("and then a contactless chip. For each, an ATR is issued and some data bytes");
        System.out.println("are sent and received.");
        System.out.println("Optionally prints, and/or waits for job completion.");
        System.out.println();
        System.out.println(" java -jar smartcard_singlewire.jar -n <printername> [-p] [-b] [-c] [-f <side>] [-i <inputHopper>]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -p Print sample text on the card.");
        System.out.println("  -b use back of card for smartcard chip.");
        System.out.println("  -c Poll for job completion.");
        System.out.println("  -f <Front | Back>. Flip card on output.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
    }
}
