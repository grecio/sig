////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class Util {
    static final int S_OK = 0;
    static final int PRINTERACTION_CANCEL = 100;

    // example of how to poll for job status:
    static public void PollForJobCompletion(String aPrinterName, int printerJobID) {
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

        while (XpsDriverInteropLib.INSTANCE.GetJobStatusXML(aPrinterName, printerJobID, returnXML,
                sizeOfReturnXML) == S_OK) {
            try {
                JobStatusXML jobStatusXml = new JobStatusXML(returnXML, sizeOfReturnXML);
                System.out.format("JobID: %s %s\n", printerJobID, jobStatusXml.GetJobState());
                if (jobStatusXml.IsJobSucceeded() || jobStatusXml.IsFailed() ||
                	jobStatusXml.IsJobCancelled() || jobStatusXml.IsCardNotRetieved() ||
                	jobStatusXml.IsNotAvailable() ) {
                    return;
                }
                sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
                Thread.sleep(2000);

                // use PrinterStatusXML object to retrieve the printer status
                // use printerJobID value to retrieve the right job.
                PrinterStatusXML printerStatusXml = new PrinterStatusXML();
                printerStatusXml.SetPrintJobID(printerJobID);
                printerStatusXml.GetPrinterMessages(aPrinterName);
                if (printerStatusXml.GetErrorCode() != 0) {
                    System.out.format("         Error code %d, msg:=%s\n", printerStatusXml.GetErrorCode(),
                            printerStatusXml.GetErrorMessage());
					return;		// could / should throw exception, but "return" also gets us out of the loop
                }
                // Job cannot be completed until any error condition be resolved
                // do something for the error condition, otherwise this loop will not be end.

            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    // example of how to ensure window print job has been complete spooled
    static public void WaitUntilJobSpooled(String aPrinterName) {
        int windowJobId = -1;
        int errorCode = -1;
        int printerJobId = -1;
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        try {
            do {
                printerStatusXml.GetPrinterMessages(aPrinterName);
                windowJobId = printerStatusXml.getWindowsJobID();
                errorCode = printerStatusXml.GetErrorCode();
                printerJobId = printerStatusXml.GetPrintJobID();
                System.out.format("PrinterJobID:= %d, WindowJobId:=%d,  ErrorCode:=%d\n", printerJobId, windowJobId,
                        errorCode);

                Thread.sleep(500);

            } while (windowJobId == 0 && errorCode == 0);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    static public void CancelAllJobs(String aPrinterName) {
        // when cancel job with printer job id:= 0
        // and error code:=s 0 will trigger cancel All jobs
        // in the printer. Use it carefully
        XpsDriverInteropLib.INSTANCE.SendResponseToPrinter(aPrinterName, PRINTERACTION_CANCEL, 0, 0);

        System.out.format("CancelAllJobs in %s printer\n", aPrinterName);
    }

    static public void SetPrinterState(String mPrinterName, String state) {
        int S_OK = 0;
        long hResult = -1; // not S_OK
        int iState = 0;

        if (state.equals("on")) {
            iState = 0;
        } else if (state.equals("suspend")) {
            iState = 1;
        } else if (state.equals("off")) {
            iState = 2;
        }

        System.out.format("iState = %d\n", iState);

        // call driver to set printer state
        hResult = XpsDriverInteropLib.INSTANCE.ChangePrinterState(mPrinterName, iState);

        if (S_OK == hResult) {
            System.out.format("'%s' SetPrinterState: success", mPrinterName);
        } else {
            System.out.format("'%s' SetPrinterState: failed, hResult = %d", mPrinterName, hResult);
        }
    }

    static public void RestartPrinter(String mPrinterName) {
        int S_OK = 0;
        long hResult = -1; // not S_OK

        // call driver to restart printer
        hResult = XpsDriverInteropLib.INSTANCE.RestartPrinter(mPrinterName);

        if (S_OK == hResult) {
            System.out.format("'%s' RestartPrinter: success", mPrinterName);
        } else {
            System.out.format("'%s' RestartPrinter: failed, hResult = %d", mPrinterName, hResult);
        }
    }

    static public void ResetCardCounters(String mPrinterName) {
        int S_OK = 0;
        long hResult = -1; // not S_OK

        // call driver to reset card counters
        hResult = XpsDriverInteropLib.INSTANCE.ResetCardCounts(mPrinterName);

        if (S_OK == hResult) {
            System.out.format("'%s' ResetCardCounters: success", mPrinterName);
        } else {
            System.out.format("'%s' ResetCardCounters: failed, hResult = %d", mPrinterName, hResult);
        }
    }
}
