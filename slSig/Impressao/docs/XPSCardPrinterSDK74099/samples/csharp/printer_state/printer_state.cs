////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Printer SDK: csharp 'printer_state' sample
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using dxp01sdk;

public class CommandLineOptions {
    public string printerState = string.Empty;
    public string printerName = string.Empty;
}

internal class printer_state
{

    private static void Usage()
    {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.WriteLine();
        Console.WriteLine(thisExeName + " demonstrates changing the printer state to offline, online, or suspended.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> -s <on | off | suspend>");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -s <on | off | suspend>. Required. Changes printer to the specified state: ");
        Console.WriteLine("     'on' changes printer state to online.");
        Console.WriteLine("     'off' changes printer state to offline.");
        Console.WriteLine("     'suspend' changes printer state to suspended.");
        Console.WriteLine();
        Console.WriteLine("  " + thisExeName + " -n \"XPS Card Printer\" -s on");
        Console.WriteLine("  Changes the printer state to online.");
        Console.WriteLine();
        Console.WriteLine("  " + thisExeName + " -n \"XPS Card Printer\" -s off");
        Console.WriteLine("  Changes the printer state to offline.");
        Console.WriteLine();
        Console.WriteLine("  " + thisExeName + " -n \"XPS Card Printer\" -s suspend");
        Console.WriteLine("  Changes the printer state to suspended.");
        Console.WriteLine();
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args)
    {
        CommandLineOptions commandLineOptions = new CommandLineOptions();
        CommandLine.Utility.Arguments arguments = new CommandLine.Utility.Arguments(args);

        if (!arguments.ContainsKey("n"))
        {
            Usage();
        }

        if (arguments["n"] == "true")
        {
            Usage();
        } // parser inserts "true" for missing args:
        else commandLineOptions.printerName = arguments["n"];

        if (!arguments.ContainsKey("s"))
        {
            Usage();
        }

        if (arguments["s"] == "true")
        {
            Usage();
        } // parser inserts "true" for missing args:
        else commandLineOptions.printerState = arguments["s"].ToLower();

        switch (commandLineOptions.printerState)
        {
            case "on":
                break;
            case "off":
                break;
            case "suspend":
                break;
            default:
                Usage();
                break;
        }

        return commandLineOptions;
    }

    private static void CheckStatus(string printerStatusXml, string fnName)
    {
        PrinterStatusValues printerStatus = Util.ParsePrinterStatusXML(printerStatusXml);
        if (printerStatus._errorCode != 0)
        {
            string message = fnName + " failed. Status XML: " + Environment.NewLine + printerStatusXml +
                             Environment.NewLine;
            throw new Exception(message);
        }
    }

    // ------------------------------------------------------------
    //  function GetPrinterState
    //  ASSUMPTION:
    //      It has already been determined that printerState 
    //      is a valid state.
    // ------------------------------------------------------------
    private static int GetPrinterState(
        string printerState)
    {
        switch (printerState) {
            case "off":
                return (int)dxp01sdk.PrinterState.Offline;
            case "on":
                return (int)dxp01sdk.PrinterState.Online;
            default:
                return (int)dxp01sdk.PrinterState.Suspended;
        }
    }

    private static void ChangePrinterState(
        BidiSplWrap bidiSpl, 
        int         printerState)
    {
        string formattedPrinterStateXML = string.Format(
            strings.CHANGE_PRINTER_STATE_XML,
            printerState);

        string printerStatusXML = bidiSpl.SetPrinterData(strings.CHANGE_PRINTER_STATE, formattedPrinterStateXML);
        CheckStatus(printerStatusXML, "ChangePrinterState");
    }


    ////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions options = GetCommandlineOptions(args);


        BidiSplWrap bidiSpl = null;

        try {

            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(options.printerName);

            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);

            int printerState = GetPrinterState(options.printerState);
            ChangePrinterState(bidiSpl, printerState);

        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        finally {
            bidiSpl.UnbindDevice();
        }
    }
}