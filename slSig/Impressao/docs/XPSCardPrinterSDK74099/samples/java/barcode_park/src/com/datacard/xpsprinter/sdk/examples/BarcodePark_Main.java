////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;

public class BarcodePark_Main {

    public static void main(String[] args) {
        String printerName = "";
        int i = 0;
        String arg;
        boolean parkBack = false;
        boolean doPrint = false;
        boolean pollCompletion = false;
        boolean nameGiven = false;
        String commandLineHopperID = "";
        String commandLineCardEjectSide = "";
        
        new XPS_Java_SDK();  // construct and extract interop dll

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
            } else if (arg.equals("-b") && nameGiven) {
                parkBack = true;
            } else if (arg.equals("-p") && nameGiven) {
                doPrint = true;
            } else if (arg.equals("-c") && nameGiven) {
                pollCompletion = true;
            } else if (arg.equalsIgnoreCase("-f")) {
                if (i < args.length) {
                    commandLineCardEjectSide = args[i++];
                    ValidateCardEjectSide(commandLineCardEjectSide);
                } else {
                    System.err.println("-f requires a 'side'.");
                    System.exit(-1);
                    break;
                }
            } else if (arg.equalsIgnoreCase("-i")) {
                if (i < args.length) {
                    commandLineHopperID = args[i++];
                    ValidateHopperID(commandLineHopperID);
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

        final String format = "Barcode Park Demo options:" 
                            + "\n  parkback: %b" 
                            + "\n  print: %b"
                            + "\n  pollCompletion: %b" 
                            + "\n  inputHopper: %s" 
                            + "\n  cardEjectSide: %s" 
                            + "\n";
        System.out.format(format, parkBack, doPrint, pollCompletion, commandLineHopperID, commandLineCardEjectSide);
        BarcodePark barcode = new BarcodePark(printerName);
        barcode.DemoBarcodePark(parkBack, doPrint, pollCompletion, commandLineHopperID, commandLineCardEjectSide);
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
        System.out.println(" barcode_park.jar demonstrates interactive mode parking a card in the barcode");
        System.out.println(" reader, moving the card from the station, and options to print and poll");
        System.out.println(" for job completion. Uses hardcoded data for printing.");
        System.out.println();
        System.out.println("  java -jar barcode_park.jar -n <printername> [options]");
        System.out.println();
        System.out.println();
        System.out.println(" options: ");
        System.out.println("  -n <printername>. Required. Try -n \"XPS card printer\"");
        System.out.println("  -b parks the card such that the barcode on the back side of the card");
        System.out.println("     can be read. Default operation is to park the card such that the ");
        System.out.println("     barcode on the front side of the card can be read.");
        System.out.println("  -c poll for job completion");
        System.out.println("  -p Print sample text on the card.");
        System.out.println("  -f <Front | Back>. Flip card on output.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
        System.out.println(" Examples:");
        System.out.println();
        System.out.println("   java -jar barcode_park.jar -n \"XPS Card Printer\" -p");
        System.out.println();
        System.out.println(" Parks a card such that the barcode on the front side of the card can be read,");
        System.out.println(" asks you to continue (i.e., barcode read was successful) or reject (i.e. barcode");
        System.out.println(" read was not successful). If the read was successful, sample images are printed on one");
        System.out.println(" or both sides of the card.");
        System.out.println();
        System.out.println("   java -jar barcode_park.jar -n \\\"XPS Card Printer\\\" -b");
        System.out.println();
        System.out.println(" Parks a card such that the barcode on the back side of the card can be read,");
        System.out.println(" asks you to continue or reject and then does what you requested.");
    }
}
