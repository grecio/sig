////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

//import com.sun.jna.examples.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class ColorAdjust
{
   private String mPrinterName;

   public ColorAdjust(String aPrinterName)
   {
      mPrinterName = aPrinterName;
   }
   
   public void SetColorAdjust()
   {
      int S_OK = 0;
      long hResult = -1; // not S_OK
      String rChannel = "1,2,-1,0,25,6,-19,12,13,14,15";
      String gChannel = "";
      String bChannel = "";

      // call driver to set color adjust values for red color channel
      hResult = XpsDriverInteropLib.INSTANCE.SetColorAdjust(mPrinterName, rChannel, gChannel, bChannel);

      if (S_OK == hResult)
      {
         System.out.format("'%s' SetColorAdjust: success", mPrinterName);
      }
      else
      {
         System.out.format("'%s' SetColorAdjust: failed, hResult = %d", mPrinterName, hResult);
      }

   }
   
   public void DefaultColorAdjust()
   {
      int S_OK = 0;
      long hResult = -1; // not S_OK
      boolean rChannel = true;
      boolean gChannel = true;
      boolean bChannel = false;

      // call driver to set default color adjust values red and green color channels
      hResult = XpsDriverInteropLib.INSTANCE.DefaultColorAdjust(mPrinterName, rChannel, gChannel, bChannel);

      if (S_OK == hResult)
      {
         System.out.format("'%s' DefaultColorAdjust: success", mPrinterName);
      }
      else
      {
         System.out.format("'%s' DefaultColorAdjust: failed, hResult = %d", mPrinterName, hResult);
      }

   }

}
