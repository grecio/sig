////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import java.io.IOException;
import common_java.XPS_Java_SDK;

public class Laser_Main {
    
    public static void main(String[] args) throws IOException {
        int i = 0;
        String arg;
        
    	String  printerName     = "";
        boolean nameGiven       = false;
        boolean laserExport     = false;
        boolean laserImport     = false;
        boolean setupRetreieve  = false;
        boolean staticEngrave   = false;
        boolean twoSidedEngrave = false;
        boolean encode          = false;
        boolean print           = false;
        boolean pollCompletion  = false;
        String inputHopper      = "";
        
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
            } else if (arg.equals("-u") && nameGiven) {
            	laserExport = true;
            } else if (arg.equals("-d") && nameGiven) {
            	laserImport = true;
            } else if (arg.equals("-r") && nameGiven) {
            	setupRetreieve = true;
            } else if (arg.equals("-s") && nameGiven) {
            	staticEngrave = true;
            } else if (arg.equals("-2") && nameGiven) {
            	twoSidedEngrave = true;
            } else if (arg.equals("-e") && nameGiven) {
            	encode = true;
            } else if (arg.equals("-p") && nameGiven) {
            	print = true;
            } else if (arg.equals("-c") && nameGiven) {
            	pollCompletion = true;
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
        
        Laser laser = new Laser(printerName, staticEngrave, twoSidedEngrave, encode, print, pollCompletion, inputHopper);
        
        if (setupRetreieve) {
        	laser.RetrieveLaserSetup();
        }
        if (laserImport) {
        	laser.ImportZipFilesToPrinter(Laser.SIMPLEX_SETUP_ZIP_FILE_NAME, true);
        	
        	// Allow import time to complete, as this setup is used for engraving sample
        	try {
        	    Thread.sleep(5000); // milliseconds
        	} 
        	catch(InterruptedException ex) {
        	    Thread.currentThread().interrupt();
        	}
        }
        
        if (laserExport) {
        	laser.ExportZipFilesFromPrinter(Laser.LASER_STATIC_SETUP_FILE_NAME);
        }
        
        laser.LaserDemo();
        
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
        System.out.println("laser.jar demonstrates laser engraving along with options to");
        System.out.println("perform interactive mode magnetic stripe encode, print, and poll for job completion status");
        System.out.println();
        System.out.println("Uses hardcoded data for laser engraving, magnetic stripe and printing.");
        System.out.println();
        System.out.println(" java -jar laser.jar -n <printername> [-u] [-d] [-r] [-s] [-e] [-p] [-c] [-i <inputHopper>]");
        System.out.println();
        System.out.println("options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -u Exports laser setup zip files from printer to PC.");
        System.out.println("  -d Imports laser setup zip files to the printer.");
        System.out.println("  -r Retrieves laser setup files from the printer.");
        System.out.println("  -s Laser engraves static laser setup file data (one-sided). Default is to");
        System.out.println("     laser engrave variable setup file data. Not supported with -2 option.");
        System.out.println("  -2 Laser engrave 2 sided (duplex), not supported with -s option.");
        System.out.println("  -e Encodes magnetic stripe data.");
        System.out.println("  -p Print sample text on the card.");
        System.out.println("  -c Poll for job completion.");
        System.out.println("  -i <input hopper>. Defaults to input hopper #1.");
        System.out.println();
        System.out.println("java -jar laser.jar -n \"XPS Card Printer\"");
        System.out.println("Laser engraves data");
        System.out.println();
        System.out.println("java -jar laser.jar -n \"XPS Card Printer\" -e -p");
        System.out.println("Laser engraves, prints and encodes magstripe data on all three tracks of an ISO 3-track magnetic ");
        System.out.println("stripe");
        System.out.println();
    }
}
