////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.Magstripe;
import common_java.XPS_Java_SDK;

public class Magstripe_Main {
    
    public static void main(String[] args) {
        int i = 0;
        String arg;
        
    	String  printerName     = "";
        boolean nameGiven       = false;
        boolean encode          = false;
        boolean magRead         = false;
        boolean print           = false;
        boolean pollCompletion  = false;
        boolean JIS             = false;
        String inputHopper 		= "";
        String cardEjectSide	= "";

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
            } else if (arg.equals("-e") && nameGiven) {
            	encode = true;
            } else if (arg.equals("-r") && nameGiven) {
            	magRead = true;
            } else if (arg.equals("-p") && nameGiven) {
            	print = true;
            } else if (arg.equals("-c") && nameGiven) {
            	pollCompletion = true;
            } else if (arg.equals("-j") && nameGiven) {
            	JIS = true;
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

        if (i < 3) {
            usage();
            System.exit(0);
        }

    	String strHopperID = "1";
    	String strCardEjectSide = "Default";

        if (magRead && !encode) {
        	 Magstripe magStripe = new Magstripe(printerName);
             magStripe.Read(print, pollCompletion,
                 !inputHopper.equals("") ? inputHopper : strHopperID, 
     	   	     !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide );

         } else if (encode) {
             Magstripe magStripe = new Magstripe(printerName);

             if (!JIS) {
                 magStripe.Encode(
                     " !#$%&\'\"()*+,-./0123456789:;<=>@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_ !#$%&\'()*+,",
                     "0123456789:;<=>0123456789:;<=>0123456",
                     "0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=>0123456789:;<=",
                     magRead, print, pollCompletion,
         	         !inputHopper.equals("") ? inputHopper : strHopperID, 
         	   	     !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide );
             } else {
                 // You need make sure your printer has JIS magstripe head and any
                 // necessary setting. The key here is the data should put into track 3
                 // only.
                 String nttData = "!\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNO";
                 magStripe.Encode("", "", nttData, magRead, print, pollCompletion,
         	         !inputHopper.equals("") ? inputHopper : strHopperID, 
         	   	     !cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide );
             }
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
        System.out.println("magstripe.jar demonstrates interactive mode magnetic stripe encoding with");
        System.out.println("options to read the magnetic stripe data, print, and poll for job completion");
        System.out.println("status.");
        System.out.println();
        System.out.println("Uses hardcoded data for magnetic stripe and printing.");
        System.out.println();
        System.out.println(" java -jar magstripe.jar -n <printername> [-e] [-r] [-p] [-c] [-j] [-f <side>] [-i <inputHopper]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -e Encodes magnetic stripe data.");
        System.out.println("  -r Reads back the encoded magnetic stripe data.");
        System.out.println("  -p Print sample text on the card.");
        System.out.println("  -c Poll for job completion.");
        System.out.println("  -j JIS magnetic.");
        System.out.println("  -f <Front | Back>. Flip card on output.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
        System.out.println("java -jar magstripe.jar -n \"XPS Card Printer\" -e -r");
        System.out.println("Encodes data on all three tracks of an ISO 3-track magnetic stripe on");
        System.out.println("the backside of a card, reads and displays it.");
        System.out.println();
        System.out.println("java -jar magstripe.jar -n \"XPS Card Printer\" -r -p -c");
        System.out.println("Reads data on all three tracks of an ISO 3-track magnetic stripe on");
        System.out.println("the backside of a card and displays it, prints black text on one or both sides of the card,");
        System.out.println("and polls and displays job status.");
        System.out.println();
    }
}
