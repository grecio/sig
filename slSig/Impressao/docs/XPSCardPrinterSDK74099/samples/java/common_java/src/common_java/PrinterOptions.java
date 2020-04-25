////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class PrinterOptions {
    private String mPrinterName;

    public PrinterOptions(String aPrinterName) {
        mPrinterName = aPrinterName;
    }

    public String GetPrinterOptions() {
        byte returnBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int bufferSize[] = new int[1];
        bufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        int S_OK = 0;
        int hResult = -1; // not S_OK
        String returnValue = "";

        // get driver SDK version
        hResult = XpsDriverInteropLib.INSTANCE.GetSDKVersion(mPrinterName, returnBuffer, bufferSize);
        returnValue = PrinterStatusXML.cStringToJavaString(returnBuffer, bufferSize[0]);
        if (S_OK == hResult) {
            System.out.format("'%s' GetSDKVersion() return length: %d\n\n%s\n", mPrinterName,
                    bufferSize[0], returnValue);
        } else {
            System.out.format("GetSDKVersion() failed. Return buffersize = %d\n %s", bufferSize[0],
                    returnValue);
            return returnValue;
        }
        // reset buffersize
        bufferSize[0] = bufferSize[0] = XPS_Java_SDK.BUFFSIZE;

        // call driver to get printer options
        hResult = XpsDriverInteropLib.INSTANCE.GetPrinterOptions2(mPrinterName, returnBuffer,
                bufferSize);

        returnValue = PrinterStatusXML.cStringToJavaString(returnBuffer, bufferSize[0]);
        if (S_OK == hResult) {
            System.out.format("'%s' GetPrinterOptions() return length: %d\n\n%s", mPrinterName,
                    bufferSize[0], returnValue);
        } else {
            System.out.format("GetPrinterOptions() failed. Return buffersize = %d\n %s",
                    bufferSize[0], returnValue);
        }

        return returnValue;
    }
}
