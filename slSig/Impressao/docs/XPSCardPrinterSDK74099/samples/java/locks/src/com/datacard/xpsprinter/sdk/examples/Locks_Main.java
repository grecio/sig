////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.XPS_Java_SDK;

public class Locks_Main {

    public static void main(String[] args) {
        String printerName = "";
        int i = 0;
        String arg;
        String password = "";
        String nextPassword = "";
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
            } else if (arg.equals("-a") && nameGiven) {
                System.out.println("Activate Printer Demo");
                if (i < args.length) {
                    password = args[i++];
                } else {
                    System.err.println("-a requires password");
                    break;
                }
                Locks locks = new Locks(printerName);
                locks.ActivatePrinter(true, password);
            } else if (arg.equals("-d") && nameGiven) {
                System.out.println("Deactivate Printer Demo");
                if (i < args.length) {
                    password = args[i++];
                } else {
                    System.err.println("-d requires password");
                    break;
                }
                Locks locks = new Locks(printerName);
                locks.ActivatePrinter(false, password);
            } else if (arg.equals("-l") && nameGiven) {
                System.out.println("Lock Printer Demo");
                if (i < args.length) {
                    password = args[i++];
                } else {
                    System.err.println("-l requires password");
                    break;
                }
                Locks locks = new Locks(printerName);
                locks.SetLockState(1, password);
            } else if (arg.equals("-u") && nameGiven) {
                System.out.println("Unlock Printer Demo");
                if (i < args.length) {
                    password = args[i++];
                } else {
                    System.err.println("-u requires password");
                    break;
                }
                Locks locks = new Locks(printerName);
                locks.SetLockState(0, password);
            } else if (arg.equals("-c") && nameGiven) {
                System.out.println("Change Lock Password Demo");
                if (i < args.length) {
                    password = args[i++];
                    if (i < args.length) {
                        nextPassword = args[i++];
                    } else {
                        System.err.println("-c requires passwords");
                        break;
                    }
                } else {
                    System.err.println("-c requires passwords");
                    break;
                }
                Locks locks = new Locks(printerName);
                locks.ChangePassword(password, nextPassword);
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
        System.out.println(" locks.jar demonstrates locking, unlocking, changing the lock password, and");
        System.out.println(" activating or deactivating the printer.");
        System.out.println();
        System.out.println(" locks.jar -n <printername> [-a] [-d] [-l] [-u] [-c]");
        System.out.println();
        System.out.println(" options:");
        System.out.println("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        System.out.println("  -a <password>. Activate printer using the password.");
        System.out.println("  -d <password>. Deactivate printer using the password.");
        System.out.println("  -l <password>. Lock printer using the password.");
        System.out.println("  -u <password>. Unlock printer using the password.");
        System.out.println("  -c <current password> <new password>. Change lock password.");
        System.out.println("     The current and new passwords are required.");
        System.out.println("     In this sample, changing the password also locks");
        System.out.println("     the printer.");
        System.out.println("     To use a blank password, use an empty string: \"\"");
        System.out.println("  -a and -d options cannot be used with -l, -u or -c options");
        System.out.println("  -l, -u or -c option only applies to the printers that have lock option installed.");
        System.out.println();
        System.out.println("  password rules:");
        System.out.println("    blank password (\"\") is OK;");
        System.out.println("    four or more valid characters;");
        System.out.println("    legal characters:");
        System.out.println("      A through Z; a through z; 0 through 9; '+', '/', and '$'.");
    }
}