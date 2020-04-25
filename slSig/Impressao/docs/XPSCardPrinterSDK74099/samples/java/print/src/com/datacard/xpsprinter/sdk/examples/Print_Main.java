////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;
import common_java.PrinterStatusXML;
import common_java.JavaPrint;
import common_java.Util;

public class Print_Main {

    
    private static String cStringToJavaString(byte b[], int bufferLength) {
        // Converts C string to Java String
        int len = 0;
        while (len < bufferLength && b[len] != 0) {
            ++len;
        }

        if (len > bufferLength) {
            len = bufferLength;
        }

        return new String(b, 0, len);
    }
    
    private static String ParseXML(byte returnXML[], int sizeOfReturnXML[]) {
    	String mXML = null;
        if (returnXML != null && sizeOfReturnXML != null && sizeOfReturnXML[0] > 0) {
            mXML = cStringToJavaString(returnXML, sizeOfReturnXML[0]);
            System.out.println("Parsing XML");
        }
        
        return mXML;
    }
    
    public static void main(String[] args) {
        int i = 0;
        String arg;
        
    	String  printerName     = "";
        boolean nameGiven       = false;
        boolean duplex          = false;
        boolean portrait        = false;
        String  numCopies       = "1";
        boolean doMagstripe     = false;
        String  blockingFront   = null;
        String  blockingBack    = null;
        String  inputHopper     = "";
        boolean getHopperStatus = false;
        boolean pollCompletion  = false;
        String  cardEjectSide   = "";
        
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
            } else if (arg.equals("-2") && nameGiven) {
                duplex = true;
            } else if (arg.equals("-o") && nameGiven) {
                portrait = true;
            } else if (arg.equals("-s") && nameGiven) {
                numCopies = args[i++];
            } else if (arg.equals("-m") && nameGiven) {
                doMagstripe = true;
            } else if (arg.equals("-t") && nameGiven) {
                blockingFront = args[i++];
            } else if (arg.equals("-u") && nameGiven) {
                blockingBack = args[i++];
            } else if (arg.equals("-i") && nameGiven) {           	
                if (i < args.length) {
                	inputHopper = args[i++];
                    ValidateHopperID(inputHopper);
                } else {
                    System.err.println("-i requires a 'hopper'.");
                    System.exit(-1);
                    break;
                }
            } else if (arg.equals("-e") && nameGiven) {
                getHopperStatus = true;
            } else if (arg.equals("-c") && nameGiven) {
                pollCompletion = true;
            } else if (arg.equals("-f") && nameGiven) {
                if (i < args.length) {
                    cardEjectSide = args[i++];
                    ValidateCardEjectSide(cardEjectSide);
                } else {
                    System.err.println("-f requires a 'side'.");
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

        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        final int S_OK = 0;
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        boolean bJobStarted = false;
        String hopperStatus = null;
        
        // Get hopper status
        if (getHopperStatus && !inputHopper.equals("")) {
        	if (S_OK == XpsDriverInteropLib.INSTANCE.GetHopperStatus(printerName, inputHopper, returnXML, sizeOfReturnXML)) {
        		hopperStatus = ParseXML(returnXML, sizeOfReturnXML);
        		System.out.println();
            	System.out.format("Status of hopper %s:%s.", inputHopper, hopperStatus);
            	System.out.println();
        	} else {
        		System.out.println("GetHopperStatus() call failed."); 
        	}
        }
    	
        // reset buffer
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        
    	// set up interactive StartJob2 if needed
        if (!inputHopper.equals("") || pollCompletion || !cardEjectSide.equals("") ) {
        	
        	String strHopperID = "1";
        	String strCardEjectSide = "Default";
        	
	        if (S_OK == XpsDriverInteropLib.INSTANCE.StartJob2(printerName, 
	            !inputHopper.equals("") ? inputHopper : strHopperID, 
	        	!cardEjectSide.equals("") ? cardEjectSide : strCardEjectSide, 
	        	returnXML, sizeOfReturnXML)) 
	        {
		          bJobStarted = true;
		          // get PrintJobID
		          printerStatusXml.Parse(returnXML, sizeOfReturnXML);
		     }      	             	
        }
        
        JavaPrint javaPrint = new JavaPrint(
      		printerName,
      		duplex,
      		portrait,
      		numCopies,
      		doMagstripe,
      		blockingFront,
      		blockingBack,
      		false);

      javaPrint.PrintDemo();
      
      // ensure windows print job is completely spooled
      Util.WaitUntilJobSpooled(printerName);
      
      if (bJobStarted) {
          XpsDriverInteropLib.INSTANCE.EndJob(printerName);
      }
      
      if (pollCompletion && bJobStarted) {
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
        System.out.println("print.jar demonstrates print functionality of the printer and driver.");
        System.out.println();
        System.out.println("Uses hardcoded data for printing, magnetic stripe, topcoat region");
        System.out.println("  and print blocking region.");
        System.out.println();
        System.out.println("Overrides the driver printing preference settings.");
        System.out.println();
        System.out.println("java -jar print.jar -n <printername>  [-2]");
        System.out.println("  [-o] [ -s <number of copies>] [-m]");
        System.out.println("  [-t all | -t chip | -t magJIS | -t mag2 | -t mag3 | -t custom] ");
        System.out.println("  [-u all | -u chip | -u magJIS | -u mag2 | -u mag3 | -u custom] ");
        System.out.println("  [-i <input hopper>] [-e] [-c] [-f <side>]");
        System.out.println();
        System.out.println("options: ");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -2 Prints a 2-sided (duplex) card. Default is front-side printing.");
        System.out.println("  -o Sets portrait orientation for both sides of card. ");
        System.out.println("     Default is landscape orientation for both card sides. ");
        System.out.println("  -s <number of copies>. Default is 1.");
        System.out.println("  -m Writes 3-track magnetic stripe data to backside of card using escapes. ");
        System.out.println("     Default is no encoding.");
        System.out.println("  -t <all | chip | magJIS | mag2 | mag3 | custom>");
        System.out.println("     Top coat and print blocking region for front of card. Use \'-t all\' to ");
        System.out.println("     topcoat the entire card side with no print blocking. ");
        System.out.println("     Default is the current driver setting.");
        System.out.println("  -u <all | chip | magJIS | mag2 | mag3 | custom>");
        System.out.println("     Top coat and print blocking region for back of card. Use \'-u all\' to");
        System.out.println("     topcoat the entire card side with no print blocking.");
        System.out.println("     Default is the current driver setting.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1");
        System.out.println("  -e Checks the status of the input hopper if input hopper is specified.");
        System.out.println("  -c Poll for job completion.");
        System.out.println("  -f <side>. Select which side to eject card on.");
        System.out.println();
        System.out.println("  java -jar print.jar -n \"XPS Card Printer\"");
        System.out.println("  Prints a one-sided landscape card.");
        System.out.println();
        System.out.println("  java -jar print.jar -n \"XPS Card Printer\" -i 1 -e");
        System.out.println("  Checks the status of input hopper 1 and prints a one-sided landscape card");
        System.out.println("  if cards are present.");
        System.out.println();
        System.out.println("  java -jar print.jar -n \"XPS Card Printer\" -2 -o");
        System.out.println("  Prints a two-sided card with portrait orientation");
        System.out.println();
        System.out.println("  java -jar print.jar -n \"XPS Card Printer\" -2 -t all -u mag3");
        System.out.println("  Prints a two-sided card with topcoat applied over all of side one");
        System.out.println("  and topcoat and printing blocked over the 3-track magnetic stripe ");
        System.out.println("  area on the backside of the card.");
        System.out.println();
    }
}
