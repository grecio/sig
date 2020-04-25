////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class PrinterSuppliesStatus
{
   private String mPrinterName;
   public final String SUPPLIES_STATUS2   = "\\Printer.SuppliesStatus2:Read";

   public PrinterSuppliesStatus(String aPrinterName)
   {
      mPrinterName = aPrinterName;
   }

   public String GetPrinterSuppliesStatus()
   {
      byte returnBuffer[] = new byte[XPS_Java_SDK.BUFFSIZE];
      int bufferSize[] = new int[1];
      bufferSize[0] = XPS_Java_SDK.BUFFSIZE;
      int S_OK = 0;
      int hResult = -1; // not S_OK
      String returnValue = "";

      // get driver SDK version
      hResult = XpsDriverInteropLib.INSTANCE.GetSDKVersion(mPrinterName, returnBuffer, bufferSize);
      returnValue = PrinterStatusXML.cStringToJavaString(returnBuffer, bufferSize[0]);
      if (S_OK == hResult)
      {
         System.out.format("'%s' GetSDKVersion() return length: %d\n\n%s\n", mPrinterName,
               bufferSize[0], returnValue);
      }
      else
      {
         System.out.format("GetSDKVersion() failed. Return buffersize = %d\n %s", bufferSize[0],
               returnValue);
         return returnValue;
      }

      // demo Check your return bufferSize to ensure the buffer is big enough
      int currentBufferSize = bufferSize[0];

      do
      {
         hResult = XpsDriverInteropLib.INSTANCE.GetPrinterStatus(
               SUPPLIES_STATUS2, mPrinterName, returnBuffer, bufferSize);
         if (bufferSize[0] > currentBufferSize) // XPS_Java_SDK.BUFFSIZE)
         {
            // allocate big enough buffer then try again
            System.out.format("GetPrinterSuppliesStatus buffer too small. old :=%d, new := %d\n",
                  currentBufferSize, bufferSize[0]);
            currentBufferSize = bufferSize[0];
            returnBuffer = new byte[currentBufferSize];
         }
         else
         {
            break; // we are done
         }
      } while (true);

      if (S_OK == hResult)
      {
         returnValue = PrinterStatusXML.cStringToJavaString(returnBuffer, bufferSize[0]);

         System.out.format("'%s' GetPrinterSuppliesStatus() return length: %d\n\n%s", mPrinterName,
               bufferSize[0], returnValue);
      }
      else
      {
         System.out.format("GetPrinterSuppliesStatus() failed. Return buffersize = %d\n %s",
               bufferSize[0], returnValue);
      }

      return returnValue;
   }
}
