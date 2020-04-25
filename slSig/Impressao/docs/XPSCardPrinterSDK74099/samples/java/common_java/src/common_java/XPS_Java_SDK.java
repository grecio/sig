////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
//package com.sun.jna.examples;
package common_java;

import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.URL;

import com.sun.jna.Library;
import com.sun.jna.Native;
import java.nio.file.FileSystems;
import java.nio.file.Path;

public class XPS_Java_SDK {
    public final static int BUFFSIZE = 10240; // buffer size for all the interop communications:

    // Java interop DLL:
    public final static String INTEROPDLL = "dxp01sdk_IBidiSpl_interop.dll";

    // xps printer interface and functions
    public interface XpsDriverInteropLib extends Library {

        XpsDriverInteropLib INSTANCE = (XpsDriverInteropLib) Native.loadLibrary(
                INTEROPDLL, XpsDriverInteropLib.class);

        int GetSDKVersion(String printerName, byte returnBuffer[],
                int bufferSize[]);

        int GetPrinterOptions2(String printerName, byte returnBuffer[],
                int bufferSize[]);

        int GetPrinterCounterStatus2(String printerName, byte returnBuffer[],
                int bufferSize[]);

        int GetPrinterSuppliesStatus(String printerName, byte returnBuffer[],
                int bufferSize[]);

        int StartJob(String printerName, byte returnXML[], int sizeOfReturnXML[]);
		
		////////////////////////////////////////////////////////////
		// StartJob2 is similar to StartJob, but allows a user to specify
		//     - from which input hopper the card is to be picked.
		//     - which side of the card is "facing up" when the card is ejected.
		//
		//     arguments include:
		//        String printerName 
		//        String hopperID        - valid valued include (1-6, "exception")
		//        String cardEjectside   - valid valued include ("front" and "back")
		//        byte returnXML[] 
		//        int sizeOfReturnXML[]
		/////////////////////////////////////////////////////////////////////
        int StartJob2(String printerName, String hopperID, String cardEjectside, byte returnXML[], int sizeOfReturnXML[]);

        int MagstripeRead(String printerName, byte dataBuffer[], int bufferSize[]);

        int MagstripeEncode(String printerName, String track1Data,
                int sizeOfTrack1Data, String track2Data, int sizeOfTrack2Data,
                String track3Data, int sizeOfTrack3Data, byte returnXML[],
                int sizeOfReturnXML[]);

        boolean SendResponseToPrinter(String aPrinterName, int command,
                int printerJobID, int errorCode);

        int SmartCardPark(
                String aPrinterName,
                byte returnXML[],
                int sizeOfReturnXML[],
                boolean parkBack);

        int SCardConnect(String aPrinterName, int connectType, int protocol[]);

        int SCardDisConnect(String aPrinterName, int disposition);

        int SCardStatus(
                String aPrinterName,
                int states[],
                int numberStatesReturned[],
                int protocol[],
                byte returnATRBytes[],
                int numATRBytesReturned[]);

        int SCardGetAttrib(String aPrinterName, int attrID,
                byte attrBytesBuffer[], int attrBytesBufferSize[]);

        int SCardTransmit(String aPrinterName, byte sendBytesBuffer[],
                int sendBytesBufferSize, byte receivedBytesBuffer[],
                int receivedBytesBufferSize[]);

        int ResumeJob(String printerName, int printerJobID, int errorCode,
                byte returnXML[], int sizeOfReturnXML[]);

        int CancelJob(String printerName, int printerJobID, int errorCode,
                byte returnXML[], int sizeOfReturnXML[]);

        int EndJob(String aPrinterName);

        int GetJobStatusXML(String aPrinterName, int aPrinterJobID,
                byte returnXML[], int sizeOfReturnXML[]);

        int GetPrinterStatus(String schemaString, String printerName,
                byte returnBuffer[], int bufferSize[]);
        
        int RestartPrinter(String mPrinterName);
        
        int ChangePrinterState(String mPrinterName, int iState);

        int ResetCardCounts(String mPrinterName);
        
        int SetColorAdjust(String mPrinterName, String rChannel, String gChannel, String bChannel);
        
        int DefaultColorAdjust(String mPrinterName, boolean rChannel, boolean gChannel, boolean bChannel);

        int ActivateOrDisablePrinter(String mPrinterName, boolean activate, String password);
        
        int SetPrinterLockState(String aPrinterName, int lockState, String LockPassword);
        
        int ChangeLockPassword(String mPrinterName, int lockValue, 
              String currentPassword, String nextPassword);
        
        int DoBarcodePark(String aPrinterName, byte returnXML[],
              int sizeOfReturnXML[], boolean parkBack);
        
        int GetHopperStatus(String aPrinterName, String hopperID, 
        		byte returnXML[], int sizeOfReturnXML[]);
        
        int BidiXPSDriverInterface(String mPrinterName, String schema, byte inputBytes[], 
        		int sizeOfInputBytes, String actionType, byte returnXML[], int sizeOfReturnXML[]);
    }

    // make sure JNA can find xps printer interface dxp01sdk_IBidiSpl_interop.dll
    static {
        System.setProperty("jna.library.path", ".;" + System.getProperty("os.arch"));
        extractDll(System.getProperty("os.arch"), INTEROPDLL);
    }

    private static void DisplayInteropDllName(String dllName) {
        Path basePath = FileSystems.getDefault().getPath(dllName);
        Path absolutePath = basePath.toAbsolutePath();
        System.out.format(" basePath: %s\n absolutePath: %s",
                basePath,
                absolutePath);
        System.out.println();
    }

    private static void extractDll(String path, String name) {
        try {
            String resourcePath = path + "/" + name;
            URL url = ClassLoader.getSystemResource(resourcePath);
            if (url == null) {
                System.out.println("unable to locate interop DLL.");
                DisplayInteropDllName(resourcePath);
                System.exit(1);
            }

            InputStream in = ClassLoader.getSystemResourceAsStream(resourcePath);
            OutputStream os = new FileOutputStream(name);

            byte[] buffer = new byte[BUFFSIZE];
            int length;
            while ((length = in.read(buffer)) > 0) {
                os.write(buffer, 0, length);
            }
            os.close();
            in.close();
        } catch (Exception e) {
            System.out.format("extractDll() error: %s. Reason: %s", INTEROPDLL, e.getMessage());
            System.exit(0);
        }
    }
}
