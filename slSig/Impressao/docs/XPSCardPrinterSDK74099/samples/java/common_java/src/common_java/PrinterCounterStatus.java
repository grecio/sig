////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class PrinterCounterStatus {
    private String mPrinterName;

    public PrinterCounterStatus(String aPrinterName) {
        mPrinterName = aPrinterName;
    }

    public String GetPrinterCounterStatus() {
        byte returnBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int bufferSize[] = new int[1];
        bufferSize[0] = XPS_Java_SDK.BUFFSIZE;
        int S_OK = 0;
        int hResult = -1; // not S_OK
        String returnValue = "";

        // call driver to get printer options
        hResult = XpsDriverInteropLib.INSTANCE.GetPrinterCounterStatus2(mPrinterName, returnBuffer,
                bufferSize);
        returnValue = PrinterStatusXML.cStringToJavaString(returnBuffer, bufferSize[0]);
        if (S_OK == hResult) {
            System.out.format("'%s' GetPrinterCounterStatus2() return length: %d\n\n%s", mPrinterName,
                    bufferSize[0], returnValue);
        } else {
            System.out.format("GetPrinterCounterStatus2() failed. Return buffersize = %d\n %s",
                    bufferSize[0], returnValue);
        }
        return returnValue;
    }
}
