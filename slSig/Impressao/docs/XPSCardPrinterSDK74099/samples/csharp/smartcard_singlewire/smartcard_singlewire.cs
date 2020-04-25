////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// XPS Driver SDK: Single-wire smartcard personalization c# sample.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dxp01sdk;

public class CommandLineOptions {
    public string printerName;
    public bool print;
    public bool jobCompletion;
    public bool parkBack;
    public string hopperID = "";
    public string cardEjectSide = "";

    public void Validate() {
        // if hopperID is an empty string, that is OK
        if (hopperID != "") {
            if (
                hopperID != "1" &&
                hopperID != "2" &&
                hopperID != "3" &&
                hopperID != "4" &&
                hopperID != "5" &&
                hopperID != "6" &&
                hopperID != "exception") {
                Console.WriteLine("invalid hopperID: {0}", hopperID);
                Environment.Exit(-1);
            }
        }

        // if cardEjectSide is an empty string, that is OK
        if (cardEjectSide != "") {
            if (cardEjectSide != "front" && cardEjectSide != "back") {
                Console.WriteLine("invalid cardEjectSide: {0}", cardEjectSide);
                Environment.Exit(-1);
            }
        }
    }

};

internal class smartcard_singlewire {

    // Escapes are top to area; left to area; width; height : card in landscape orientation
    private static string printBlockingEscape = "~PB%7.26 22.35 14.99 14.99?";

    private static string topcoatRemovalEscape = "~TR%7.26 22.35 14.99 14.99?";

    private enum CardType { contact, contactless, undetermined };

    private static void Usage() {
        string thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.Write(thisExeName);
        Console.WriteLine(" demonstrates interactive mode parking a card in the");
        Console.WriteLine("  smart card station, performing single-wire smartcard functions, moving");
        Console.WriteLine("  the card from the station, and options to print and poll for job completion.");
        Console.WriteLine();
        Console.WriteLine("Uses hardcoded data for printing.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-p] [-b] [-c] [-i <input hopper>] [-f <side>]");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -p Print text and graphic on front side of card.");
        Console.WriteLine("  -b use back of card for smartcard chip.");
        Console.WriteLine("  -c Poll for job completion.");
        Console.WriteLine("  -f <side>  Flip card on output.");
        Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -p -c");
        Console.WriteLine("  Parks a card in printer smart card station, moves the card from the station,");
        Console.WriteLine("  prints simple text and graphics on the front side of the card, and polls and");
        Console.WriteLine("  displays job status.");
        Environment.Exit(-1);
    }

    private static CommandLineOptions GetCommandlineOptions(string[] args) {
        if (args.Length == 0) Usage();

        var commandLineOptions = new CommandLineOptions();
        var arguments = new CommandLine.Utility.Arguments(args);

        if (string.IsNullOrEmpty(arguments["n"])) {
            Console.WriteLine("printer name is required");
            Environment.Exit(-1);
        }
        else {
            // we might have a -n with no printer name:
            bool boolVal = false;
            if (Boolean.TryParse(arguments["n"], out boolVal)) {
                Console.WriteLine("printer name is required");
                Environment.Exit(-1);
            }
            commandLineOptions.printerName = arguments["n"];
        }

        commandLineOptions.print = !string.IsNullOrEmpty(arguments["p"]);

        commandLineOptions.jobCompletion = !string.IsNullOrEmpty(arguments["c"]);
        commandLineOptions.parkBack = !string.IsNullOrEmpty(arguments["b"]);
        commandLineOptions.hopperID =
             string.IsNullOrEmpty(arguments["i"]) ? string.Empty : arguments["i"].ToLower();
        commandLineOptions.cardEjectSide =
            string.IsNullOrEmpty(arguments["f"]) ? string.Empty : arguments["f"].ToLower();

        return commandLineOptions;
    }

    private static void ResumeJob(BidiSplWrap bidiSpl, int printerJobID, int errorCode) {
        string xmlFormat = strings.PRINTER_ACTION_XML;
        string input = string.Format(xmlFormat, (int) Actions.Resume, printerJobID, errorCode);
        Console.WriteLine("issuing Resume after smartcard:");
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
    }

    ////////////////////////////////////////////////////////////////////////////////
    // SCardResultMessage()
    // format a message for display.
    // SCARD errors are declared in WinError.H
    ////////////////////////////////////////////////////////////////////////////////
    private static string SCardResultMessage(long scardResult, string message) {
        var sb = new StringBuilder();
        sb.Append(message);
        sb.Append(" result: ");
        sb.Append(scardResult.ToString());
        sb.Append("; ");
        sb.Append(Util.Win32ErrorString((int) scardResult));
        return sb.ToString();
    }

    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// a card is in the printer and stated in the smartcard module. The
    /// SCardConnect() call has succeeded.
    /// talk to the reader and the chip (contact or contactless).
    /// </summary>
    /// <param name="scard">an instance of our SCard wrapper class.</param>
    ////////////////////////////////////////////////////////////////////////////
    private static bool PersonalizeConnectedChip(SCard scard) {
        int[] states = new int[0];
        uint protocol = (uint) scard_protocol.SCARD_PROTOCOL_UNDEFINED;
        byte[] ATRBytes = new byte[0];

        var scardResult = scard.SCardStatus(ref states, ref protocol, ref ATRBytes);

        Console.WriteLine("SCardStatus result: {0}: {1}", scardResult, Util.Win32ErrorString((int) scardResult));
        if (scard_error.SCARD_S_SUCCESS != (scard_error) scardResult) {
            // unable to get a valid SCardStatus() response; we're done.
            return false;
        }

        bool cardAbsent = false;
        Console.Write("SCardStatus() states: ");
        for (var index = 0; index < states.Length; index++) {
            Console.Write(states[index]);
            Console.Write(" ");
            Console.Write(SCard.StringFromState(states[index]));
            Console.Write(" ");

            if (scard_state.SCARD_ABSENT == (scard_state) states[index])
                cardAbsent = true;
        }
        Console.WriteLine();

        if (cardAbsent) {
            // one of the states is SCARD_ABSENT; we're done.
            return false;
        }

        Console.Write("SCardStatus() ATRBytes (hex): ");
        for (var index = 0; index < ATRBytes.Length; index++) {
            Console.Write("{0:X02} ", ATRBytes[index]);
        }
        Console.WriteLine();

        var attributesToTry = new scard_attr[] {
            scard_attr.SCARD_ATTR_VENDOR_NAME,
            scard_attr.SCARD_ATTR_VENDOR_IFD_SERIAL_NO,
            scard_attr.SCARD_ATTR_VENDOR_IFD_TYPE,
            scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION
        };

        for (var attribIndex = 0; attribIndex < attributesToTry.Length; attribIndex++) {
            byte[] scardAttributeBytes = new byte[0];

            scardResult = scard.SCardGetAttrib(attributesToTry[attribIndex], ref scardAttributeBytes);

            if (scard_error.SCARD_S_SUCCESS != (scard_error) scardResult)
                continue;

            string msg = string.Format("SCardGetAttrib({0})", SCard.StringFromAttr((int) attributesToTry[attribIndex]));
            Console.WriteLine(SCardResultMessage(scardResult, msg));

            switch (attributesToTry[attribIndex]) {
                case scard_attr.SCARD_ATTR_VENDOR_IFD_VERSION:
                    // vendor-supplied interface device version: DWORD in the form 0xMMmmbbbb where
                    //      MM = major version;
                    //      mm = minor version; and
                    //      bbbb = build number:
                    var byteCount = scardAttributeBytes.Length;
                    var minorVersion = byteCount > 0 ? scardAttributeBytes[0] : 0;
                    var majorVersion = byteCount > 1 ? scardAttributeBytes[1] : 0;
                    var buildNumber = 0;
                    if (byteCount > 3) {
                        buildNumber = (scardAttributeBytes[2] << 8) + scardAttributeBytes[3];
                    }

                    Console.WriteLine("  SCARD_ATTR_VENDOR_IFD_VERSION:");
                    Console.WriteLine("    major: " + majorVersion.ToString());
                    Console.WriteLine("    minor: " + minorVersion.ToString());
                    Console.WriteLine("    build: " + buildNumber.ToString());
                    break;

                default:
                    // null-terminate the returned byte for string display.
                    var bytesList = new List<byte>(scardAttributeBytes);
                    bytesList.Add(0);
                    scardAttributeBytes = bytesList.ToArray();
                    Console.WriteLine(ASCIIEncoding.UTF8.GetString(scardAttributeBytes));
                    break;
            }
        }

        // create a bytes for the upcoming SCardTransmit() method.
        // these particular bytes should function with this type of contact chip:
        //
        //      MPCOS-EMV 16k
        //      GEMPLUS
        //      Datacard part number 806062-002
        //
        byte[] sendBytes = new byte[] { 0x00, 0xA4, 0x00, 0x00 };
        Console.Write("sending bytes : ");
        for (var index = 0; index < sendBytes.Length; index++) {
            Console.Write("{0:X02} ", sendBytes[index]);
        }
        Console.WriteLine();

        // for those bytes, we should receive 0x61, 0x12
        byte[] receivedBytes = new byte[0];

        scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);

        Console.WriteLine("SCardTransmit result: {0}", scardResult);

        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.Write("  {0} bytes received: ", receivedBytes.Length);
            for (var index = 0; index < receivedBytes.Length; index++) {
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
        }

        // send the bytes       0x00  0xC0  0x00  0x00  0x12:
        sendBytes = new byte[] { 0x00, 0xC0, 0x00, 0x00, 0x12 };
        Console.Write("sending bytes : ");
        for (var index = 0; index < sendBytes.Length; index++) {
            Console.Write("{0:X02} ", sendBytes[index]);
        }
        Console.WriteLine();

        // for those bytes, we should receive
        //  0x85, 0x10, 0x80, 0x01, 0x3F, 0x00, 0x38, 0x00, 0x00, 0x00,
        //  0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x6B, 0x90, 0x00
        receivedBytes = new byte[0];

        scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);

        Console.WriteLine("SCardTransmit result: {0}", scardResult);

        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.Write("  {0} bytes received: ", receivedBytes.Length);
            for (var index = 0; index < receivedBytes.Length; index++) {
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
        }
        return true;
    }

    /// <summary>
    /// Demonstrate the single-wire smartcard calls. The wrapper class we consume
    /// is similar to the Win32 ::Scard...() functions.
    /// </summary>
    private static CardType PersonalizeSmartcard(BidiSplWrap bidiSpl) {
        var scard = new SCard(bidiSpl);

        bool contactPersonalized = false;
        bool contactlessPersonalized = false;
        CardType cardType = CardType.undetermined;

        // try a contacted chip:
        uint protocol = (uint) scard_protocol.SCARD_PROTOCOL_DEFAULT;

        long scardResult = scard.SCardConnect(SCard.ChipConnection.contact, ref protocol);
        Console.WriteLine("SCardConnect(contact): {0}: {1}", scardResult, Util.Win32ErrorString((int) scardResult));

        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult) {
            cardType = CardType.contact;
            var protocols = SCard.StringsFromProtocol(protocol);
            Console.Write("   protocol[s]: ");
            for (int index = 0; index < protocols.Length; index++) {
                Console.Write(protocols[index] + " ");
            }
            Console.WriteLine();

            contactPersonalized = PersonalizeConnectedChip(scard);

            scardResult = scard.SCardDisConnect((int) scard_disposition.SCARD_LEAVE_CARD);

            Console.WriteLine("SCardDisConnect() result: {0}", scardResult);
        }

        // try a contactless chip:
        protocol = (uint) scard_protocol.SCARD_PROTOCOL_DEFAULT;

        scardResult = scard.SCardConnect(SCard.ChipConnection.contactless, ref protocol);
        Console.WriteLine("SCardConnect(contactless): {0}: {1}", scardResult, Util.Win32ErrorString((int) scardResult));

        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult) {
            cardType = CardType.contactless;
            var protocols = SCard.StringsFromProtocol(protocol);
            Console.Write("   protocol[s]: ");
            for (int index = 0; index < protocols.Length; index++) {
                Console.Write(protocols[index] + " ");
            }
            Console.WriteLine();

            contactlessPersonalized = PersonalizeConnectedChip(scard);

            scardResult = scard.SCardDisConnect((int) scard_disposition.SCARD_LEAVE_CARD);

            Console.WriteLine("SCardDisConnect() result: {0}", scardResult);
        }

        if (!contactPersonalized && !contactlessPersonalized) {
            throw new Exception("neither contact nor contactless chip personalization succeeded. canceling job.");
        }
        return cardType;
    }

    private static void SmartcardPark(BidiSplWrap bidiSpl, bool parkBack) {
        var parkCommand = parkBack ? strings.SMARTCARD_PARK_BACK : strings.SMARTCARD_PARK;
        string printerStatusXML = bidiSpl.SetPrinterData(parkCommand);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            throw new BidiException(
                "SmartcardPark() fail" + printerStatusValues._errorString,
                printerStatusValues._printerJobID,
                printerStatusValues._errorCode);
        }
    }



    ////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        CommandLineOptions commandLineOptions = GetCommandlineOptions(args);
        commandLineOptions.Validate();

        BidiSplWrap bidiSpl = null;
        int printerJobID = 0;

        try {
            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            // optional:
            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);

            // see if the printer is in the Printer_Ready state:
            string printerOptionsXml = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);

            PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXml);
            if ("Ready" != printerOptionsValues._printerStatus && "Busy" != printerOptionsValues._printerStatus) {
                throw new Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus);
            }

            if ("Single wire" != printerOptionsValues._optionSmartcard) {
                var msg = string.Format("{0} needs 'Single wire' for smartcard option. '{1}' was returned.",
                    commandLineOptions.printerName,
                    printerOptionsValues._optionSmartcard);
                throw new Exception(msg);
            }

            if (commandLineOptions.print && ("Installed" != printerOptionsValues._printHead)) {
                throw new Exception(commandLineOptions.printerName + " does not have a print head installed.");
            }

            string hopperID = "1";
            string cardEjectSide = "default";

            printerJobID = Util.StartJob(
                bidiSpl,
                (commandLineOptions.hopperID.Length > 0) ? commandLineOptions.hopperID : hopperID,
                (commandLineOptions.cardEjectSide.Length > 0) ? commandLineOptions.cardEjectSide : cardEjectSide);

            SmartcardPark(bidiSpl, commandLineOptions.parkBack);

            CardType cardType = PersonalizeSmartcard(bidiSpl);

            ResumeJob(bidiSpl, printerJobID, 0);

            if (commandLineOptions.print) {
                if (cardType == CardType.contact &&
                    !commandLineOptions.parkBack) {
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName, printBlockingEscape, topcoatRemovalEscape);
                }
                else {
                    Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);
                }
                Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName);
            }

            bidiSpl.SetPrinterData(strings.ENDJOB);

            if (commandLineOptions.jobCompletion) {
                Util.PollForJobCompletion(bidiSpl, printerJobID);
            }
        }
        catch (BidiException e) {
            Console.WriteLine(e.Message);
            Util.CancelJob(bidiSpl, e.PrinterJobID, e.ErrorCode);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            if (0 != printerJobID) {
                Util.CancelJob(bidiSpl, printerJobID, 0);
            }
        }
        finally {
            bidiSpl.UnbindDevice();
        }
    }
}