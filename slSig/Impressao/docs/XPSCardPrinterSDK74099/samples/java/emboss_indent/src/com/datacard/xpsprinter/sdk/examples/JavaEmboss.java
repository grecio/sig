////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import java.awt.Font;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.print.PageFormat;
import java.awt.print.Printable;
import java.awt.print.PrinterJob;

import javax.print.PrintService;
import javax.print.PrintServiceLookup;
import javax.print.attribute.HashPrintRequestAttributeSet;
import javax.print.attribute.PrintRequestAttributeSet;
import javax.print.attribute.standard.MediaPrintableArea;
import javax.print.attribute.standard.MediaSize;
import javax.print.attribute.standard.OrientationRequested;
import javax.print.attribute.standard.Sides;

import common_java.PrinterStatusXML;
import common_java.Util;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;

public class JavaEmboss implements Printable {
    
    private String mPrinterName = null;
    
    public JavaEmboss(String aPrinterName) {
        mPrinterName = aPrinterName;
    }
    
    private PrintService GetPrinterService() {
        PrintService[] printServices = PrintServiceLookup.lookupPrintServices(
                null, null);

        for (PrintService printer : printServices) {
            if (printer.getName().compareToIgnoreCase(mPrinterName) == 0) {
                System.out.println("Found Printer: " + printer.getName());
                return printer;
            }
        }
        System.out.println("Did not match any printer name as : " + mPrinterName);
        return null;
    }

    public void EmbossDemo(String inputHopper, boolean pollCompletion) {
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        final int S_OK = 0;
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        boolean bJobStarted = false;
		String strHopperID = "1";
		String strCardEjectSide = "default";
        
        // set up interactive StartJob2
        if (!inputHopper.equals("") || pollCompletion) {
            if (S_OK == XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName, 
	                !inputHopper.equals("") ? inputHopper : strHopperID, 
	        	    strCardEjectSide, 
				    returnXML, sizeOfReturnXML)) {
                bJobStarted = true;
                // get PrintJobID
                printerStatusXml.Parse(returnXML, sizeOfReturnXML);
            }
        }
        
        PrintService printService = GetPrinterService();
        if (printService == null) {
            return;
        }

        try {
            PrinterJob printJob = PrinterJob.getPrinterJob();
            printJob.setPrintService(printService);

            // Set the printable class to this one since we are implementing the
            // Printable interface:
            PrintRequestAttributeSet aset = new HashPrintRequestAttributeSet();

            aset.add(OrientationRequested.LANDSCAPE);
            aset.add(new MediaPrintableArea(0.0f, 0.0f, 53.98f, 85.6f,MediaSize.MM));
            aset.add(Sides.ONE_SIDED);

            printJob.setPrintable(this);
            printJob.print(aset);

        } catch (Exception PrintException) {
            PrintException.printStackTrace();
        }
        
        // ensure windows print job is completely spooled
        Util.WaitUntilJobSpooled(mPrinterName);
        
        if (bJobStarted) {
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }
        
        if (pollCompletion && bJobStarted) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }
    }

    /**
     * Method: print
     * <p>
     *
     * This class is responsible for rendering a page using the provided
     * parameters.
     *
     * @param g a value of type Graphics
     * @param pageFormat a value of type PageFormat
     * @param pageNumber a value of type int
     * @return an int error code.
     */
    @Override
    public int print(Graphics g, PageFormat pageFormat, int pageNumber) {

        // --- Validate the page number, we only print max one pages
        if (pageNumber > 0) {
            System.out.println("Print job has been sent to spooler.");
            return Printable.NO_SUCH_PAGE;
        }

        // --- Translate the origin to be (0,0)
        Graphics2D graphics2D = (Graphics2D) g;
        graphics2D.setFont(new Font(null, Font.PLAIN, 8));

        graphics2D.translate(pageFormat.getImageableX(),
                pageFormat.getImageableY());


        if (pageNumber == 0) {
            WriteEmbossEscapes(graphics2D);

            return (Printable.PAGE_EXISTS);
        } else {
            return (NO_SUCH_PAGE);
        }
    }
    
    private void WriteEmbossEscapes(Graphics2D g2d) {

//        final String emboss1Escape = "~EM%2;401;843;0001 3416 7890 1286";
//        final String emboss2Escape = "~EM%1;800;480;2007";
//        final String emboss3Escape = "~EM%1;401;328;Janice Holloway";
      
        final String emboss1Escape = "~EM%1;301;860;Font 11111";
        final String emboss2Escape = "~EM%2;1600;860;222222";
        final String emboss3Escape = "~EM%3;301;1460;333333";
        final String emboss4Escape = "~EM%4;301;1180;444444";
        final String emboss5Escape = "~EM%5;301;690;555555";
        final String emboss6Escape = "~EM%6;1600;690;666666";
        final String emboss7Escape = "~EM%7;301;650;777777";
        final String emboss8Escape = "~EM%8;301;1000;888888";
        final String emboss9Escape = "~EM%9;301;1050;999999";
        final String emboss10Escape = "~EM%10;1600;1050;10 10 10";
         
        g2d.drawString(emboss1Escape, 0, 0);
        g2d.drawString(emboss2Escape, 0, 0);
        g2d.drawString(emboss3Escape, 0, 0);
        g2d.drawString(emboss4Escape, 0, 0);
        g2d.drawString(emboss5Escape, 0, 0);
        g2d.drawString(emboss6Escape, 0, 0);
        g2d.drawString(emboss7Escape, 0, 0);
        g2d.drawString(emboss8Escape, 0, 0);
        g2d.drawString(emboss9Escape, 0, 0);
        g2d.drawString(emboss10Escape, 0, 0);
    }
}
    