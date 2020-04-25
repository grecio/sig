////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import java.awt.Color;
import java.awt.Font;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.geom.Line2D;
import java.awt.image.BufferedImage;
import java.awt.print.PageFormat;
import java.awt.print.Printable;
import java.awt.print.PrinterJob;
import java.awt.geom.AffineTransform;
import java.net.URL;
import javax.imageio.ImageIO;
import javax.print.PrintService;
import javax.print.PrintServiceLookup;
import javax.print.attribute.HashPrintRequestAttributeSet;
import javax.print.attribute.PrintRequestAttributeSet;
import javax.print.attribute.standard.Copies;
import javax.print.attribute.standard.MediaPrintableArea;
import javax.print.attribute.standard.MediaSize;
import javax.print.attribute.standard.OrientationRequested;
import javax.print.attribute.standard.Sides;

public class JavaPrint implements Printable {

    private final double POINTS_PER_INCH = 72;
    
    private String  mPrinterName;
    private boolean mDuplex;
    private boolean mPortrait;
    private int     mNumCopies; // converted in constructor
    private boolean mDoMagstripe;
    private String  mBlockingFront;
    private String  mBlockingBack;
    private boolean mDoSampleText;
    private boolean mLaserSampleText;

    public JavaPrint(
            String  printerName, 
            boolean duplex, 
            boolean portrait,
            String  numCopies,
            boolean doMagstripe, 
            String  blockingFront, 
            String  blockingBack,
            boolean doSampleText) 
    {
        mPrinterName     = printerName;
        mDuplex          = duplex;
        mPortrait        = portrait;
        mNumCopies       = Integer.parseInt(numCopies);
        mDoMagstripe     = doMagstripe;
        mBlockingFront   = blockingFront;
        mBlockingBack    = blockingBack;
        mDoSampleText    = doSampleText;
    }
    
    public JavaPrint(
            String  printerName, 
            boolean duplex, 
            boolean portrait,
            String  numCopies,
            boolean doMagstripe, 
            String  blockingFront, 
            String  blockingBack,
            boolean doSampleText,
            boolean laserSampleText) 
    {
        mPrinterName     = printerName;
        mDuplex          = duplex;
        mPortrait        = portrait;
        mNumCopies       = Integer.parseInt(numCopies);
        mDoMagstripe     = doMagstripe;
        mBlockingFront   = blockingFront;
        mBlockingBack    = blockingBack;
        mDoSampleText    = doSampleText;
        mLaserSampleText = laserSampleText;
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

    public void PrintDemo() {

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

            if (mPortrait) {
                aset.add(OrientationRequested.PORTRAIT);
            } else {
                aset.add(OrientationRequested.LANDSCAPE);
            }
            aset.add(new MediaPrintableArea(0.0f, 0.0f, 53.98f, 85.6f,
                    MediaSize.MM));
            aset.add(new Copies(mNumCopies));

            if (mDuplex) {
                // FYI - using "Sides.DUPLEX" results a "flip on long side" (i.e. back side of card is upside down)
            	//     - objects on the back side of the card are individually rotated in the "drawImage()" and 
            	//       "rotateText180()" methods.
            	// It would be nice to use Sides.TWO_SIDED_SHORT_EDGE to resolve the issue of the back side of
            	// the card being "upside down"
				// However, Sides.TWO_SIDED_SHORT_EDGE has no effect.
            	// - Perhaps in newer versions of Java (or a newer JNA) "Sides.TWO_SIDED_SHORT_EDGE" may 
            	//   have the desired effect.  This would alleviate the need to individually rotated in the 
            	//   "drawImage()" and "rotateText180()" methods.
                aset.add(Sides.DUPLEX);
                
                mDuplex = true;
            } else {
                aset.add(Sides.ONE_SIDED);
            }

            printJob.setPrintable(this);
            printJob.print(aset);

        } catch (Exception PrintException) {
            PrintException.printStackTrace();
        }
    }

    /**
     * Method: print
     * <p>
     *
     * This class is responsible for rendering a page using the provided
     * parameters. The result will be a grid where each cell is one half inch
     * square.
     *
     * @param g a value of type Graphics
     * @param pageFormat a value of type PageFormat
     * @param pageNumber a value of type int
     * @return an int error code.
     */
    @Override
    public int print(Graphics g, PageFormat pageFormat, int pageNumber) {
        int imgStartX = 10;
        int imgStartY = 10;

        // --- Validate the page number, we only print max two pages
        if (pageNumber > 1) {
            System.out.println("Print job has been sent to spooler.");
            return Printable.NO_SUCH_PAGE;
        }

        // --- Translate the origin to be (0,0)
        Graphics2D graphics2D = (Graphics2D) g;
        graphics2D.setFont(new Font(null, Font.PLAIN, 8));

        graphics2D.translate(pageFormat.getImageableX(),
                pageFormat.getImageableY());

        final int printerDPI = 300;

        double pageWidth = pageFormat.getWidth() * printerDPI / POINTS_PER_INCH;
        double pageHeight = pageFormat.getHeight() * printerDPI / POINTS_PER_INCH;

        if (pageNumber == 0 && !mDoSampleText) {
            graphics2D.setColor(Color.black);
            Line2D.Double line = new Line2D.Double();

            // --- Print the vertical lines
            for (int lineIndex = 0; lineIndex < pageWidth; lineIndex += POINTS_PER_INCH / 2) {
                line.setLine(lineIndex, 0, lineIndex, pageHeight);
                graphics2D.draw(line);
                graphics2D.drawString("V" + Integer.toString(lineIndex), lineIndex,
                        20);
            }

            // --- Print the horizontal lines
            for (int lineIndex = 0; lineIndex < pageHeight; lineIndex += POINTS_PER_INCH / 2) {
                line.setLine(0, lineIndex, pageWidth, lineIndex);
                graphics2D.draw(line);
                graphics2D.drawString("H" + Integer.toString(lineIndex), 0,
                        lineIndex);
            }

            // draw a color bitmap
            drawImage(graphics2D, pageFormat, "images/color.jpg", imgStartX,
                    imgStartY, (int) pageWidth, (int) pageHeight, pageNumber);

            graphics2D.drawString("Front side", (int) 60, (int) 100);

            if (mBlockingFront != null)
            {
                printBlocking(graphics2D, pageNumber);
            }

            return (Printable.PAGE_EXISTS);
        } else if (pageNumber == 0 && mDoSampleText) {
        	graphics2D.setColor(Color.black);
            String sampleTextFront = String.format("Sample printing on %s", mPrinterName); 
            if (mLaserSampleText) {
            	graphics2D.drawString(sampleTextFront, (int) 50, (int) 15);
            } else {
            	graphics2D.drawString(sampleTextFront, (int) 50, (int) 100);
            }
            return (Printable.PAGE_EXISTS);
        } else if (pageNumber == 1 && mDuplex && !mDoSampleText) {
            imgStartX = 72;
            imgStartY = 72;

            graphics2D = (Graphics2D) g;
            graphics2D.setColor(Color.black);

            // back side escape encode
            if (mDoMagstripe) {
                WriteMagstripeEscapes(graphics2D);
            }
            
            // draw a 1 bit-per-pixel bitmap to the KPanel:
            drawImage(graphics2D, pageFormat, "images/House_1bpp.png", imgStartX,
                    imgStartY, (int) pageWidth, (int) pageHeight, pageNumber);

            // Also need to rotate the string on the back side of the card.
            rotateText180(graphics2D, (int) 72, (int) 44, "back side ");
                       
            if (mBlockingBack != null)
            {
                printBlocking(graphics2D, pageNumber);
            }

            return (Printable.PAGE_EXISTS);
        }else if (pageNumber == 1 && mDuplex && mDoSampleText) {
            graphics2D.setColor(Color.black);
            String sampleTextBack = String.format("Sample back side printing on %s", mPrinterName); 
            if (mLaserSampleText) {
                rotateText180(graphics2D, (int) 200, (int) 100, sampleTextBack);
            } else {
            	rotateText180(graphics2D, (int) 200, (int) 100, sampleTextBack);
            }

            return (Printable.PAGE_EXISTS);
        } else {
            return (NO_SUCH_PAGE);
        }
    }

    private void WriteMagstripeEscapes(Graphics2D g2d) {
        final String track1Escape = "~1ABC 123";
        final String track2Escape = "~2456";
        final String track3Escape = "~3789";

        g2d.drawString(track1Escape, 0, 0);
        g2d.drawString(track2Escape, 0, 0);
        g2d.drawString(track3Escape, 0, 0);
    }

    private void printBlocking(Graphics2D g2d, int pageNumber) {
        switch ((pageNumber == 0) ? mBlockingFront : mBlockingBack) {
        case "all":    blockPrintAll(g2d);
                       break;
        case "chip":   blockPrintSmartcard(g2d);
                       break;
        case "magJIS": blockPrintMagJIS(g2d);
                       break;
        case "mag2":   blockPrintMag2(g2d);
                       break;
        case "mag3":   blockPrintMag3(g2d);
                       break;
        case "custom": blockPrintCustom(g2d, pageNumber);
                       break;
        }
    }
    
    // Topcoat and print blocking for custom
    // A 'Topcoat Add' escape will force topcoat OFF for the entire card side.
    // Units are millimeters; portrait basis; top left width height:
    
    // Topcoat entire card, no print blocking, as per usage
    private void blockPrintAll(Graphics2D g2d) {
        ;
    }
    
    // Topcoat and print blocking for smart card
    private void blockPrintSmartcard(Graphics2D g2d) {
        g2d.drawString("~TR% 8 23.6 13 14?", 0, 30);
        g2d.drawString("~PB% 8 23.6 13 14?", 0, 30);
    }
    
    // Topcoat and print blocking for magstripe JIS
    private void blockPrintMagJIS(Graphics2D g2d) {
        g2d.drawString("~TR% 0 39.1 9 120?", 0, 0);
        g2d.drawString("~PB% 0 39.1 9 120?", 0, 0);
    }

    // Topcoat and print blocking for magstripe 2-track
    private void blockPrintMag2(Graphics g2d) {
        g2d.drawString("~TR% 0 39.1 11 120?", 0, 0);
        g2d.drawString("~PB% 0 39.1 11 120?", 0, 0);
    }
    
    // Topcoat and print blocking for magstripe 3-track
    private void blockPrintMag3(Graphics g2d) {
        g2d.drawString("~TR% 0 35.4 14.5 120?", 0, 0);
        g2d.drawString("~PB% 0 35.4 14.5 120?", 0, 0);
    }
    

    private void blockPrintCustom(Graphics g2d, int pageNumber) {
        if (pageNumber == 0) {
            // Add a rectangle one inch down; two inches wide; 1 cm high.
            // Also add a 7mm x 7mm square in the lower right of card.
            g2d.drawString("~TA% 0 18 10 50.8; 61 6.5 7 7", 0, 0);
            g2d.drawString("~PB% 19 0 54 3?", 0, 0);
        } else if (pageNumber == 1) {
            g2d.drawString("~TA% 10 8 20 50.8", 0, 0);
            g2d.drawString("~PB% 19 0 54 3?", 0, 0);
        }
    }

    private void drawImage(Graphics2D graphics2D, PageFormat pageFormat,
            String aFileName, int x, int y, int pageWidth, int pageHeight, int pageNumber) {

        BufferedImage img = getSystemImage(aFileName);

        if (img == null) {
            return;
        }
        
        // Because "Sides.DUPLEX" performs a "flip on long edge" the image on the back of card will be upside down.
		// If this image is for the back side of the card, rotate the image       
        if (pageNumber ==1) {	// back side of card
        	img = rotateImage180(img);
        }

        int w = img.getWidth();
        int h = img.getHeight();
        int destW = (int) (w * 0.3) + x;
        int destH = (int) (h * 0.3) + y;

        if (w > pageWidth) {
            destW = pageWidth;
        }

        if (h > pageHeight) {
            destH = pageHeight;
        }

        graphics2D.drawImage(img, x, y, destW, destH, 0, 0, w, h, null);
    }

    private BufferedImage getSystemImage(String filename) {
        if ((filename == null) || (filename.length() == 0)) {
            return null;
        }

        URL url = ClassLoader.getSystemResource(filename);
        if (url == null) {
            System.out.println("Unable to locate file: " + filename);
            return null;
        }

        try {
            return ImageIO.read(ClassLoader.getSystemResourceAsStream(filename));
        } catch (Exception e) {
            System.out.println(e.getMessage());
            return null;
        }
    }
    
    private static void rotateText180(Graphics2D g2d, int x, int y, String text)
    {
    	g2d.translate(x, y);
    	g2d.rotate(Math.toRadians(180));
    	g2d.drawString(text,  0,  0);
    	g2d.rotate(Math.toRadians(180));
    	g2d.translate(-x, -y);    	
    }
    
    
    private static BufferedImage rotateImage180(BufferedImage image)
    {
        if (image == null)
            return null;

        float x = image.getWidth()  / 2.0f;
        float y = image.getHeight() / 2.0f;
        
        return rotateImage(image, 180, x, y);
    }
    
    private static BufferedImage rotateImage(BufferedImage image, int angle, float x, float y)
    {
        int newWidth  = (angle == 180) ? image.getWidth()  : image.getHeight();
        int newHeight = (angle == 180) ? image.getHeight() : image.getWidth();
        
        BufferedImage newImage = new BufferedImage(newWidth, newHeight,
                                                   BufferedImage.TYPE_INT_ARGB);

        double radians = 2 * Math.PI * angle / 360;
        AffineTransform tx = new AffineTransform();
        tx.rotate(radians, x, y);
        
        return applyTransform(image, newImage, tx);
    }
    
    private static BufferedImage applyTransform(BufferedImage sourceImage,
            BufferedImage destImage,
            AffineTransform tx)
	{
		Graphics2D g2 = (Graphics2D) destImage.createGraphics();
		g2.transform(tx);
		g2.drawImage(sourceImage, null, 0, 0);
		g2.dispose();
		
		return destImage;
	}
    



}
