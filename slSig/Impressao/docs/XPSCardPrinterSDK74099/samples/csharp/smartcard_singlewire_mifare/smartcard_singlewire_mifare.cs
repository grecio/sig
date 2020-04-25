////////////////////////////////////////////////////////////////////////////////
// XPS Driver SDK : smartcard_singlewire + mifare chip example
////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using dxp01sdk;
using dxp01sdk.mifare;

public class CommandLineOptions {
    public string printerName;
    public bool print;
    public bool jobCompletion;
    public bool parkBack;
};

internal class smartcard_singlewire_mifare {

    private static void Usage() {
        var thisExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        Console.Write(thisExeName);
        Console.WriteLine(" demonstrates interactive mode parking a card");
        Console.WriteLine("  in the smart card station, performing single-wire smartcard functions,");
        Console.WriteLine("  moving the card from the station, and options to print and poll for job");
        Console.WriteLine("  completion.");
        Console.WriteLine();
        Console.WriteLine("  Uses hardcoded data for printing.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n <printername> [-p] [-b] [-c]");
        Console.WriteLine();
        Console.WriteLine("options:");
        Console.WriteLine("  -n <printername>. Required. Try -n \"XPS Card Printer\".");
        Console.WriteLine("  -p Print text and graphic on front side of card.");
        Console.WriteLine("  -b use back of card for smartcard chip.");
        Console.WriteLine("  -c Poll for job completion.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\"");
        Console.WriteLine("  Parks a card in the printer smart card station, connects to the MiFare chip");
        Console.WriteLine("  and performs some MiFare chip activities.");
        Console.WriteLine();
        Console.WriteLine(thisExeName + " -n \"XPS Card Printer\" -p -c");
        Console.WriteLine("  Parks a card in printer smart card station, moves the card from the station,");
        Console.WriteLine("  prints simple text and graphics on the front side of the card, and polls and");
        Console.WriteLine("  displays job status.");
        Console.WriteLine();
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

        return commandLineOptions;
    }

    private static void ResumeJob(BidiSplWrap bidiSpl, int printerJobID, int errorCode) {
        string xmlFormat = strings.PRINTER_ACTION_XML;
        string input = string.Format(xmlFormat, (int) Actions.Resume, printerJobID, errorCode);
        Console.WriteLine("issuing Resume after smartcard:");
        bidiSpl.SetPrinterData(strings.PRINTER_ACTION, input);
    }

    private static string Bytes_to_HEX_String(byte[] bytes) {
        var sb = new StringBuilder();
        foreach (var b in bytes) {
            sb.Append(string.Format("{0:X02} ", b));
        }
        return sb.ToString();
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

    /// <summary>
    /// optional routine
    /// </summary>
    /// <param name="protocol"></param>
    private static void DisplayProtocols(uint protocol) {
        var protocols = SCard.StringsFromProtocol(protocol);
        Console.Write("   protocol[s]: ");
        for (int index = 0; index < protocols.Length; index++) {
            Console.Write(protocols[index] + " ");
        }
        Console.WriteLine();
    }

    ////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// get mifare status. see pg. 23, "CCID_Protocol_spec_120224_.pdf"
    /// </summary>
    ////////////////////////////////////////////////////////////////////////////
    private static void DisplayChipInfo(SCard scard) {
        var command = new Mifare_Command();
        var sendBytes = command.CreateGetCardStatusCommand();
        byte[] receivedBytes = new byte[0];
        var scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);
        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.WriteLine("get card status:");
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length);
            for (var index = 0; index < receivedBytes.Length; index++) {
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes));
            Mifare_Common.CheckStatusCode(receivedBytes[0]);
        }
    }

    /// <summary>
    /// load mifare keys
    /// </summary>
    private static void LoadKeys(
        SCard scard,
        byte sector,
        byte block,
        Mifare_Command.KeyType keyType) {
        var command = new Mifare_Command();

        var sendBytes = command.CreateLoadKeysCommand(
            keyType,
            sector,
            Mifare_Common.mifare_test_key);

        var receivedBytes = new byte[0];
        var scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);
        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.WriteLine("prep for write; load key status:");
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length);
            for (var index = 0; index < receivedBytes.Length; index++) {
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes));
            Mifare_Common.CheckStatusCode(receivedBytes[0]);
        }
        else {
            throw new Exception("prep for write: load keys fail");
        }
    }

    /// <summary>
    /// generate some sample data. here, just get a data/timesamp.
    /// </summary>
    /// <returns></returns>
    private static string create_test_data_string() {
        var testDataString = System.DateTime.Now.ToString("yy.mm.dd.hh.mm");
        return testDataString;
    }

    /// <summary>
    /// write some sample data to the given block and sector.
    /// </summary>
    private static void WriteData(
        SCard scard,
        byte sector,
        byte block,
        Mifare_Command.KeyType keyType,
        byte[] testDataBytes) {
        var command = new Mifare_Command();
        var sendBytes = command.CreateWriteBlockCommand(
            keyType,
            sector,
            block,
            testDataBytes);

        var receivedBytes = new byte[0];
        var scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);
        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.WriteLine("write data status:");
            Console.Write("  {0} commandBytes received: ", receivedBytes.Length);
            for (var index = 0; index < receivedBytes.Length; index++) {
                Console.Write("{0:X02} ", receivedBytes[index]);
            }
            Console.WriteLine();
            Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes));
            Mifare_Common.CheckStatusCode(receivedBytes[0]);
        }
        else {
            throw new Exception("write data failed.");
        }
    }

    /// <summary>
    /// read some data from the given block and sector.
    /// </summary>
    private static void ReadData(
        SCard scard,
        byte sector,
        byte block,
        Mifare_Command.KeyType keyType) {
        // in this app, we've already performed a LoadKeys().

        var command = new Mifare_Command();
        var sendBytes = command.CreateReadBlockCommand(keyType, sector, block);
        var receivedBytes = new byte[0];
        var scardResult = scard.SCardTransmit(sendBytes, ref receivedBytes);
        if (scard_error.SCARD_S_SUCCESS == (scard_error) scardResult && receivedBytes.Length > 0) {
            Console.WriteLine(string.Format("read sector {0}, block {1} status:", sector, block));
            var apduResponse = new MiFare_Response(receivedBytes);
            if (apduResponse.BlockHasNonzeroData()) {
                Console.Write("  {0} commandBytes received: ", receivedBytes.Length);
                for (var index = 0; index < receivedBytes.Length; index++) {
                    Console.Write("{0:X02} ", receivedBytes[index]);
                }
                Console.WriteLine();
                Console.WriteLine(Mifare_Common.StatusCodeBytesToString(receivedBytes));
                Mifare_Common.CheckStatusCode(receivedBytes[0]);
            }
            else {
                Console.WriteLine("sector is all zeros");
            }
        }
        else {
            throw new Exception(string.Format("read sector {0}, block {1} fail", sector, block));
        }

        var readDataString = Encoding.ASCII.GetString(receivedBytes, 1, 16);
        Console.WriteLine(string.Format("data read from sector {0}, block {1}: '{2}'", sector, block, readDataString));
    }

    /// <summary>
    /// issue the SCardStatus() call. this is important, because the SCardConnect()
    /// call always succeeds - as long as the card is staged.
    ///
    /// the SCardConnect() call returns the Answer-To-Reset bytes (ATR).
    /// </summary>
    private static void GetSCardStatus(SCard scard) {
        var states = new int[0];
        var protocol = (uint) scard_protocol.SCARD_PROTOCOL_UNDEFINED;
        var ATRBytes = new byte[0];

        var scardResult = scard.SCardStatus(ref states, ref protocol, ref ATRBytes);

        Console.WriteLine("SCardStatus result: {0}: {1}", scardResult, Util.Win32ErrorString((int) scardResult));
        if (scard_error.SCARD_S_SUCCESS != (scard_error) scardResult) {
            var msg = SCardResultMessage(scardResult, "");
            throw new Exception("SCardStatus() fail: " + msg);
        }

        // display all the 'states' returned. if ANY of the states is SCARD_ABSENT -
        // we are done with this card.
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
            throw new Exception("one of the states is SCARD_ABSENT.");
        }

        Console.Write("SCardStatus() ATR: ");
        Console.WriteLine(Bytes_to_HEX_String(ATRBytes));
    }

    /// <summary>
    /// Demonstrate the single-wire smartcard calls. The wrapper class we consume
    /// is similar to the Win32 ::Scard...() functions.
    /// <returns>
    /// throw an Exception if any error.
    /// </returns>
    /// </summary>
    private static void PersonalizeSmartcard(BidiSplWrap bidiSpl) {
        var scard = new SCard(bidiSpl);
        var protocol = (uint) scard_protocol.SCARD_PROTOCOL_DEFAULT;

        var scardResult = scard.SCardConnect(SCard.ChipConnection.contactless, ref protocol);

        Console.WriteLine("SCardConnect() result: {0}", scardResult);

        if (scard_error.SCARD_S_SUCCESS != (scard_error) scardResult) {
            var msg = SCardResultMessage(scardResult, "");
            throw new Exception("SCardConnect() fail: " + msg);
        }

        GetSCardStatus(scard);

        DisplayChipInfo(scard);

        byte sector = 5;
        byte block = 1;
        Mifare_Command.KeyType keytype = Mifare_Command.KeyType.A;

        LoadKeys(scard, sector, block, keytype);

        ReadData(scard, sector, block, keytype);

        var testDataString = create_test_data_string();
        var testDataBytes = Encoding.ASCII.GetBytes(testDataString);
        Console.WriteLine("writing '{0}' to chip.", testDataString);

        WriteData(scard, sector, block, keytype, testDataBytes);

        ReadData(scard, sector, block, keytype);

        scardResult = scard.SCardDisConnect((int) scard_disposition.SCARD_LEAVE_CARD);
        Console.WriteLine("SCardDisConnect() result: {0}", scardResult);

        if (scard_error.SCARD_S_SUCCESS != (scard_error) scardResult) {
            var msg = SCardResultMessage(scardResult, "");
            throw new Exception("SCardDisConnect() fail: " + msg);
        }
    }

    private static void SmartcardPark(BidiSplWrap bidiSpl, bool parkBack) {
        var parkCommand = parkBack ? strings.SMARTCARD_PARK_BACK : strings.SMARTCARD_PARK;
        string printerStatusXML = bidiSpl.SetPrinterData(parkCommand);
        PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);
        if (0 != printerStatusValues._errorCode) {
            throw new Exception("SmartcardPark(): " + printerStatusValues._errorString);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Main()
    ////////////////////////////////////////////////////////////////////////////
    private static void Main(string[] args) {
        var commandLineOptions = GetCommandlineOptions(args);

        BidiSplWrap bidiSpl = null;
        int printerJobID = 0;

        try {
            bidiSpl = new BidiSplWrap();
            bidiSpl.BindDevice(commandLineOptions.printerName);

            // optional:
            string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
            Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);

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

            string hopperID = string.Empty;
            printerJobID = Util.StartJob(
                bidiSpl,
                hopperID);

            SmartcardPark(bidiSpl, commandLineOptions.parkBack);

            PersonalizeSmartcard(bidiSpl);

            ResumeJob(bidiSpl, printerJobID, 0);

            if (commandLineOptions.print) {
                Util.PrintTextAndGraphics(bidiSpl, commandLineOptions.printerName);
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