////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer SDK: csharp 'locks' sample
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Runtime.Remoting;
using dxp01sdk;

public class CommandLineOptions {
    public bool change;
    public string currentPassword = string.Empty;
    public bool dolock;
    public string newPassword = string.Empty;
    public string printerName = string.Empty;
    public bool unlock;
    public bool activate;
    public bool deactivate;
}

internal class locks {

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine();
        Console.WriteLine(thisExeName + " demonstrates locking, unlocking, changing the lock password, ");
        Console.WriteLine("and activating or deactivating the printer.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-a] [-d] [-l] [-u] [-c -w]");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -a <password>. Activate printer using the password.");
        Console.WriteLine("  -d <password>. Deactivate printer using the password.");
        Console.WriteLine("  -l <password>. Lock printer using the password.");
        Console.WriteLine("  -u <password>. Unlock printer using the password.");
        Console.WriteLine("  -c <current password> -w <new password>. Change password.");
        Console.WriteLine("     The current and new passwords are required.");
        Console.WriteLine("     In this sample, changing the password also locks");
        Console.WriteLine("     the printer.");
        Console.WriteLine("     To use a blank password, use an empty string: \"\"");
        Console.WriteLine("  -a and -d options cannot be used with -l, -u or -c options");
        Console.WriteLine("  -l, -u or -c option only applies to the printers that have lock option installed.");
        Console.WriteLine();
        Console.WriteLine("     password rules:");
        Console.WriteLine("       blank password (\"\") is OK;");
        Console.WriteLine("       four or more valid characters;");
        Console.WriteLine("       legal characters:");
        Console.WriteLine("       A through Z; a through z; 0 through 9; '+', '/', and '$'.");
        Console.WriteLine();
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (!arguments.ContainsKey("n")) { Usage(); }

        if (arguments["n"] == "true") { Usage(); } // parser inserts "true" for missing args:
        else commandLineOptions.printerName = arguments["n"];

        if (arguments.ContainsKey("c")) {
            commandLineOptions.change = true;
            if (arguments["c"] != "true") { commandLineOptions.currentPassword = arguments["c"]; }
        }

        if (arguments.ContainsKey("l")) {
            commandLineOptions.dolock = true;
            if (arguments["l"] != "true") { commandLineOptions.currentPassword = arguments["l"]; }
        }

        if (arguments.ContainsKey("u")) {
            commandLineOptions.unlock = true;
            if (arguments["u"] != "true") { commandLineOptions.currentPassword = arguments["u"]; }
        }

        if (arguments.ContainsKey("w")) {
            if (arguments["w"] != "true") { commandLineOptions.newPassword = arguments["w"]; }
        }

        if (arguments.ContainsKey("a")) {
            commandLineOptions.activate = true;
            if (arguments["a"] != "true") { commandLineOptions.currentPassword = arguments["a"]; }
        }

        if (arguments.ContainsKey("d")) {
            commandLineOptions.deactivate = true;
            if (arguments["d"] != "true") { commandLineOptions.currentPassword = arguments["d"]; }
        }

        if (commandLineOptions.activate || commandLineOptions.deactivate) {
            if (commandLineOptions.dolock || commandLineOptions.unlock || commandLineOptions.change) {
                Usage();
            }
            if (commandLineOptions.activate && commandLineOptions.deactivate) {
                Usage();
            }
        }
        else {
            if (!commandLineOptions.dolock && !commandLineOptions.unlock && !commandLineOptions.change) {
                Usage();
            }
        }

        return commandLineOptions;
    }

    private static void CheckStatus(string printerStatusXml, string fnName) {
        PrinterStatusValues printerStatus = Util.ParsePrinterStatusXML(printerStatusXml);
        if (printerStatus._errorCode != 0) {
            string message = fnName + " failed. Status XML: " + Environment.NewLine + printerStatusXml + Environment.NewLine;
            throw new Exception(message);
        }
    }

    private static void LockOrUnlock(BidiSplWrap bidiSpl, CommandLineOptions options) {
        int lockValue = (options.dolock) ? 1 : 0;
        string formattedPrinterLockXML = string.Format(
            strings.LOCK_PRINTER_XML,
            lockValue,
            options.currentPassword);

        string printerStatusXML = bidiSpl.SetPrinterData(strings.LOCK_PRINTER, formattedPrinterLockXML);
        CheckStatus(printerStatusXML, "LockOrUnlock");
    }

    private static void ChangeLockPassword(BidiSplWrap bidiSpl, CommandLineOptions options) {
        // in this sample, we always lock the printer as part of the password change
        // operation. (Use zero to unlock the printer.)
        int lockValue = 1;

        string formattedChangePasswordXml = string.Format(
            strings.CHANGE_LOCK_PASSWORD_XML,
            lockValue,
            options.currentPassword,
            options.newPassword);

        string printerStatusXML = bidiSpl.SetPrinterData(strings.CHANGE_LOCK_PASSWORD, formattedChangePasswordXml);
        CheckStatus(printerStatusXML, "ChangeLockPassword");
    }

    private static void ActivateOrDeactivate(BidiSplWrap bidiSpl, CommandLineOptions options)
    {
        int activateValue = (options.activate) ? 1 : 0;
        string formattedActivatePrinterXML = string.Format(
            strings.ACTIVATE_PRINTER_XML,
            activateValue,
            options.currentPassword);

        string printerStatusXML = bidiSpl.SetPrinterData(strings.ACTIVATE_PRINTER, formattedActivatePrinterXML);
        CheckStatus(printerStatusXML, "ActivateOrDeactivate");
    }

    ////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions options = GetCommandlineOptions(args);

        BidiSplWrap bidiSpl = null;

        try {
            Console.WriteLine("using '{0}' for current password.", options.currentPassword);
            if (options.change) {
                Console.WriteLine("using '{0}' for new password.", options.newPassword);
            }

            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(options.printerName);

            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);


            if (options.dolock || options.unlock || options.change) {
                var printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
                var printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);
                if ("Installed" != printerOptionsValues._optionLocks) {
                    string message = options.printerName + " does not support the locking feature.";
                    throw new Exception(message);
                }
            }

            if (options.activate || options.deactivate) {
                ActivateOrDeactivate(bidiSpl, options);
                if (options.activate) {
                    Console.WriteLine(Environment.NewLine + "activated the printer." + Environment.NewLine);
                }
                else {
                    Console.WriteLine(Environment.NewLine + "deactivated the printer." + Environment.NewLine);
                }
            }
            else if (options.dolock || options.unlock) {
                LockOrUnlock(bidiSpl, options);
                if (options.dolock) {
                    Console.WriteLine(Environment.NewLine + "locked the printer." + Environment.NewLine);
                }
                else {
                    Console.WriteLine(Environment.NewLine + "unlocked the printer." + Environment.NewLine);
                }
            }
            else if (options.change) {
                ChangeLockPassword(bidiSpl, options);
                Console.WriteLine(Environment.NewLine + "password changed." + Environment.NewLine);
            }
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        finally {
            bidiSpl.UnbindDevice();
        }
    }
}