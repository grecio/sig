////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package com.datacard.xpsprinter.sdk.examples;

import common_java.PrinterStatusXML;
import common_java.Util;
import common_java.XPS_Java_SDK;
import common_java.XPS_Java_SDK.XpsDriverInteropLib;
import common_java.JavaPrint;
import common_java.Magstripe;

import java.util.*;
import java.util.Base64;
import java.io.File;
import java.nio.file.Files;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;
import java.io.PrintWriter;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.dom.DOMSource;
import org.w3c.dom.Document;
import org.w3c.dom.NodeList;
import org.w3c.dom.Element;
import org.xml.sax.InputSource;

public class Laser {
    private String  mPrinterName = null;
    private boolean mMagstripeEncode;
    private boolean mDoPrint;
    private boolean mPollCompletion;
    private boolean mStaticEngrave;
    private boolean mTwoSidedEngrave;
    private String  mInputHopper = "";
    
    private final String BIDI_GET = "Get";
    private final String BIDI_SET = "Set";
    
    private final int S_OK = 0;
    
    private final static String LASER_ENGRAVE_SETUP_FILE_NAME      = "\\Printer.Laser:Engrave:SetupFileName:Set";
    private final static String LASER_ENGRAVE_TEXT                 = "\\Printer.Laser:Engrave:Text:Set"; 
    private final static String LASER_ENGRAVE_BINARY               = "\\Printer.Laser:Engrave:Binary:Set"; 
    private final static String LASER_QUERY_SETUP_FILESLIST        = "\\Printer.Laser:SetupFileName:Get";
    private final static String LASER_QUERY_ELEMENT_LIST           = "\\Printer.Laser:ElementList:Get";
    private final static String LASER_UPLOAD_ZIP_FILE_FROM_PRINTER = "\\Printer.Laser:Upload:File:Get";
    private final static String LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER = "\\Printer.Laser:Download:File:Set";
    
    public final static String LASER_STATIC_SETUP_FILE_NAME              = "SampleCard_FrontOnly_Static";
    public final static String LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME    = "SampleCard_FrontOnly";
    public final static String LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME     = "SampleCard";
    public final static String STATIC_SETUP_ZIP_FILE_NAME                = "SampleCard_FrontOnly_Static.zip";
    public final static String SIMPLEX_SETUP_ZIP_FILE_NAME               = "SampleCard_FrontOnly.zip";
    public final static String DUPLEX_SETUP_ZIP_FILE_NAME                = "SampleCard.zip";
    

    public Laser(String printerName, boolean staticEngrave, boolean twoSidedEngrave, 
    		boolean encode, boolean print, boolean pollCompletion, String inputHopper) {
        mPrinterName     = printerName;
        mStaticEngrave   = staticEngrave;
        mTwoSidedEngrave = twoSidedEngrave;
        mMagstripeEncode = encode;
        mDoPrint         = print;
        mPollCompletion  = pollCompletion;
        mInputHopper  	 = inputHopper;
    }
    
    private void ParseLaserSetupFileList(String xmlString, Vector<String> laserSetupFileList) {
        try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
	        InputSource is = new InputSource();
            is.setCharacterStream(new StringReader(xmlString));
            
            Document doc = db.parse(is);
            doc.getDocumentElement().normalize();
            NodeList nodeList = doc.getElementsByTagName("LaserCardSetup");
            
            int laserSetupsCount = nodeList.getLength();
            //System.out.format("lasercardSetupsCount=%d\n", laserSetupsCount);
            String attributeText;
            
	        for (int laserSetupsIndex = 0; laserSetupsIndex < laserSetupsCount; laserSetupsIndex++) {
	            attributeText = nodeList.item(laserSetupsIndex).getAttributes().getNamedItem("name").getTextContent();
	            
	            laserSetupFileList.add(attributeText);
	        }
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    }
    
    private void ParseLaserStatusXML(String laserStatusXML, int[] laserStatusSuccess, String[] laserStatusBase64Data) {
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
	        InputSource is = new InputSource();
            is.setCharacterStream(new StringReader(laserStatusXML));
            
            Document doc = db.parse(is);
            doc.getDocumentElement().normalize();
            
            NodeList statusNodelist = doc.getElementsByTagName("Status");
            String laserStatus = statusNodelist.item(0).getTextContent();
            laserStatusSuccess[0] = Integer.parseInt(laserStatus);
            
            NodeList dataNodelist = doc.getElementsByTagName("Base64Data");
            laserStatusBase64Data[0] = dataNodelist.item(0).getTextContent();    
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    }
    
    private String CreateLaserFileNameXML(String laserFileName) {
    	String output = "";
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
            
            Document doc = db.newDocument();
            Element rootElement = doc.createElement("LaserSetup");
            doc.appendChild(rootElement);
            
            Element filename = doc.createElement("FileName");
            filename.appendChild(doc.createTextNode(laserFileName));
            rootElement.appendChild(filename);
            
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer = tf.newTransformer();
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            output = writer.getBuffer().toString();
            
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    	
    	return output;
    }
    
    private String CreateLaserSetupFileNameXML(String laserSetupFileName, int count) {
    	String output = "";
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
            
            Document doc = db.newDocument();
            Element rootElement = doc.createElement("LaserEngraveSetupFileName");
            doc.appendChild(rootElement);
            
            Element filename = doc.createElement("FileName");
            filename.appendChild(doc.createTextNode(laserSetupFileName));
            rootElement.appendChild(filename);
            
            Element elCount = doc.createElement("ElementCount");
            String cnt = Integer.toString(count);
            elCount.appendChild(doc.createTextNode(cnt));
            rootElement.appendChild(elCount);
            
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer = tf.newTransformer();
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            output = writer.getBuffer().toString();
            
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    	
    	return output;
    }
    
    private byte[] CreateImportZipFileXML(String laserZipFileName, String base64EncodedChars, boolean overwrite) {
    	String output = "";
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
            
            Document doc = db.newDocument();
            Element rootElement = doc.createElement("LaserZipFile");
            doc.appendChild(rootElement);
            
            Element filename = doc.createElement("FileName");
            filename.appendChild(doc.createTextNode(laserZipFileName));
            rootElement.appendChild(filename);
            
            Element overWrite = doc.createElement("Overwrite");
            String ow = overwrite?"1":"0";
            overWrite.appendChild(doc.createTextNode(ow));
            rootElement.appendChild(overWrite);
            
            Element base64Data = doc.createElement("FileContents");
            base64Data.appendChild(doc.createCDATASection(base64EncodedChars));
            rootElement.appendChild(base64Data);
            
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer = tf.newTransformer();
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            output = writer.getBuffer().toString();
          
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    	
    	return output.getBytes();
    }
    
    private byte[] CreateLaserEngraveTextXML(String elementName, String laserText) {
    	String output = "";
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
            
            Document doc = db.newDocument();
            Element rootElement = doc.createElement("LaserEngraveText");
            doc.appendChild(rootElement);
                       
            Element elName = doc.createElement("ElementName");
            elName.appendChild(doc.createTextNode(elementName));
            rootElement.appendChild(elName);
            
            Element elValue = doc.createElement("ElementValue");
            elValue.appendChild(doc.createTextNode(laserText));
            rootElement.appendChild(elValue);
            
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer = tf.newTransformer();
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            output = writer.getBuffer().toString();
          
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    	
    	return output.getBytes();
    }
    
    private byte[] CreateLaserEngraveBinaryXML(String elementName, String base64Data) {
    	String output = "";
    	try {
	    	DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
	        DocumentBuilder db = dbf.newDocumentBuilder();
            
            Document doc = db.newDocument();
            Element rootElement = doc.createElement("LaserEngraveBinary");
            doc.appendChild(rootElement);
                       
            Element elName = doc.createElement("ElementName");
            elName.appendChild(doc.createTextNode(elementName));
            rootElement.appendChild(elName);
            
            Element elValue = doc.createElement("ElementValue");
            elValue.appendChild(doc.createCDATASection(base64Data));
            rootElement.appendChild(elValue);
            
            TransformerFactory tf = TransformerFactory.newInstance();
            Transformer transformer = tf.newTransformer();
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            output = writer.getBuffer().toString();
          
	    } catch (Exception e) {
            e.printStackTrace();
	    }
    	
    	return output.getBytes();
    }
    
    private int CreateLaserXMLFile(String filename, String fileBuffer) {
    	File dir = new File("LaserFilesRetrievedFromPrinter");
    	if (!dir.exists()) {
    	    try {
    	        dir.mkdir();
    	    } catch(SecurityException se){
    	    	se.printStackTrace();
    	    }        
    	}
    	try (PrintWriter out = new PrintWriter(new File(dir + "\\" + filename + ".xml"))) {
    	    out.println(fileBuffer);
    	} catch (Exception e) {
    		e.printStackTrace(); // file not found exception
    	}
    	
    	return 0;
    }
    
    private void CreateLaserFile(String fileName, String laserFile)
    {
        // Parse the laser data returned from printer
        int laserFileBytesLength = laserFile.length();
        if (laserFileBytesLength > 5) {
             if (fileName.contains(".zip")) {
            	 File dir = new File("LaserFilesRetrievedFromPrinter");
                 if (!dir.exists()) {
             	     try {
             	         dir.mkdir();
             	     } catch(SecurityException se){
             	         se.printStackTrace();
             	     }        
             	 }
                 try {
                     FileOutputStream fos = new FileOutputStream(new File(dir + "\\" + fileName));
                     fos.write(Base64.getDecoder().decode(laserFile));
                     fos.close();
                 } catch (Exception e) {
         		     e.printStackTrace(); // file not found exception
        	     }
            	 
              } else {
            	  // create xml file
            	  byte laserFileBytes[] = Base64.getDecoder().decode(laserFile);
                  String laserXMLFile = laserFileBytes.toString();
                  CreateLaserXMLFile(fileName, laserXMLFile);
              }
        } else {
        	System.out.println("CreateLaserFile: empty laser data returned by printer");
        }
    }
    
    private int RetrieveLaserSetupFileList(Vector<String> laserSetupFileList) {
    	byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
       
        int hResult = -1; // not S_OK
    	hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_QUERY_SETUP_FILESLIST, null, 
        		0, BIDI_GET, returnXML, sizeOfReturnXML);
    	
    	PrinterStatusXML printerStatusXml = new PrinterStatusXML();
    	printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
    	String printerData = printerStatusXml.GetPrinterData();
    	
    	CreateLaserXMLFile("LaserSetupFiles", printerData);
    	System.out.println("\nSuccessfully retrieved laser setup file");
    	
    	ParseLaserSetupFileList(printerData, laserSetupFileList);
    	
    	return hResult;
    }
    
    private int RetrieveLaserSetupElements(Vector<String> laserSetupFileList) {
    	int hResult = -1; // not S_OK
    	for (int laserCardSetupsIndex = 0; laserCardSetupsIndex < laserSetupFileList.size(); laserCardSetupsIndex++) {
            // Query laser element names for a setup file
            String laserSetupFileName = laserSetupFileList.get(laserCardSetupsIndex);
            
            String laserSetupFileNameXML = CreateLaserFileNameXML(laserSetupFileName);
            byte laserSetupFileNameBytes[] = laserSetupFileNameXML.getBytes();
            int sizeOfInputBytes = laserSetupFileNameBytes.length;

	    	byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
	        int sizeOfReturnXML[] = new int[1];
	        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
	        
	    	hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_QUERY_ELEMENT_LIST, laserSetupFileNameBytes, 
	    			sizeOfInputBytes, BIDI_GET, returnXML, sizeOfReturnXML);
	    	
	    	PrinterStatusXML printerStatusXml = new PrinterStatusXML();
	    	printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
	    	String laserElementListXML = printerStatusXml.GetPrinterData();
	    	
            CreateLaserXMLFile(laserSetupFileName, laserElementListXML);
            System.out.format("Succussfully retrieved laser element file - %s\n", laserSetupFileName);
    	}
    	
    	return hResult;
    }
    
    public int RetrieveLaserSetup() {
    	int hResult = -1; // not S_OK
    	
    	// Query all laser setup files present on the system
    	Vector<String> laserSetupFileList = new Vector<String>();
    	hResult = RetrieveLaserSetupFileList(laserSetupFileList);
    	
    	// Query laser element names for all setup files
    	hResult = RetrieveLaserSetupElements(laserSetupFileList);
    	
    	return hResult;
    }
 
    public int ExportZipFilesFromPrinter(String laserZipFileName) {
    	String laserZipFileXML = CreateLaserFileNameXML(laserZipFileName);
    	byte laserExportZipFileBytes[] = laserZipFileXML.getBytes();
        int sizeOfInputBytes = laserExportZipFileBytes.length;
        
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
    	
        int hResult = -1; // not S_OK
        // first call will return needed buffer size in sizeOfReturnXML[0] if returnXML is too small
	    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_UPLOAD_ZIP_FILE_FROM_PRINTER, laserExportZipFileBytes, 
    			sizeOfInputBytes, BIDI_GET, returnXML, sizeOfReturnXML);
	    
	    if (returnXML[0] == 0) { // no data in buffer if it was too small
		    returnXML = new byte[sizeOfReturnXML[0]]; // resize buffer
		    
		    // second call with larger if needed
		    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_UPLOAD_ZIP_FILE_FROM_PRINTER, laserExportZipFileBytes, 
	    			sizeOfInputBytes, BIDI_GET, returnXML, sizeOfReturnXML);
	    }
	    
	    PrinterStatusXML printerStatusXml = new PrinterStatusXML();
    	printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
    	String laserStatusXML = printerStatusXml.GetPrinterData();
    	
  	  	int laserStatusSuccess[] = new int[1];
    	String laserStatusBase64Data[] = new String[1];
    	ParseLaserStatusXML(laserStatusXML, laserStatusSuccess, laserStatusBase64Data);
    	
    	if (laserStatusSuccess[0] != 0) {
    		// Success. Retrieved zip file from printer
            laserZipFileName += ".zip";
            CreateLaserFile(laserZipFileName, laserStatusBase64Data[0]);
            System.out.format("Successfully exported laser setup file:%s\n", laserZipFileName);
    	} else {
    		System.out.format("ExportZipFilesFromPrinter: failed to export file %s\n", laserZipFileName);
    	}
	    
	    return hResult;
    }   
    
    private String fileToBase64String(String filename) {
    	String base64EncodedString = null;
    	try {
	        base64EncodedString = Base64.getEncoder().encodeToString(Files.readAllBytes(new File(filename).toPath()));
    	} catch (IOException e) {
            e.printStackTrace();
        }
    	return base64EncodedString;
    }
    
    public int ImportZipFilesToPrinter(String laserZipFileName, boolean overwrite) {
    	// read file data to buffer as base64 encoded data
    	String base64EncodedChars = fileToBase64String(laserZipFileName);

    	// create xml for bidi message
    	byte[] laserZipFileXMLBytes = CreateImportZipFileXML(laserZipFileName, base64EncodedChars, overwrite);
    	
    	int sizeOfInputBytes = laserZipFileXMLBytes.length;
	    byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
	    int sizeOfReturnXML[] = new int[1];
	    sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

	    int hResult = -1; // not S_OK
	    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_DOWNLOAD_ZIP_FILE_TO_PRINTER, laserZipFileXMLBytes, 
				sizeOfInputBytes, BIDI_SET, returnXML, sizeOfReturnXML);
	    
	    PrinterStatusXML printerStatusXml = new PrinterStatusXML();
		printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
		String laserStatusXML = printerStatusXml.GetPrinterData();
		int laserStatusErrorCode = printerStatusXml.GetErrorCode();
		if (518 == laserStatusErrorCode) {
			System.out.format("laser module returned 518 error: %s\n", laserZipFileName);
		}
		
		int laserStatusSuccess[] = new int[1];
		String laserStatusBase64Data[] = new String[1];
		ParseLaserStatusXML(laserStatusXML, laserStatusSuccess, laserStatusBase64Data);
		
		if (laserStatusSuccess[0] != 0) {
	        System.out.format("Successfully imported zip file to printer - %s\n", laserZipFileName);
		} else if (519 == laserStatusErrorCode && laserStatusSuccess[0] != 1) {
			// Firmware issued conflict file list error
	        CreateLaserFile("LaserFileConflictList", laserStatusBase64Data[0]);
	        System.out.println("Import failed. Created laserFileConflictList.xml file");
		} else {
			System.out.format("Import failed - %s\n", laserZipFileName);
		}
	    
	    return hResult;
	}   
   
    private int LaserEngraveSetupFileName(String laserSetupFileName, int count) {
    	// Specify the static laser setup file name. Variable elements count is 0
    	String laserXML = CreateLaserSetupFileNameXML(laserSetupFileName, count);
    	byte[] laserBytes = laserXML.getBytes();
    	
    	// send the data to driver using Windows bidi interface
    	int sizeOfInputBytes = laserBytes.length;
	    byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
	    int sizeOfReturnXML[] = new int[1];
	    sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

	    int hResult = -1; // not S_OK
	    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_ENGRAVE_SETUP_FILE_NAME, laserBytes, 
				sizeOfInputBytes, BIDI_SET, returnXML, sizeOfReturnXML);
	    
	    PrinterStatusXML printerStatusXml = new PrinterStatusXML();
		printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
		int laserStatusErrorCode = printerStatusXml.GetErrorCode();
		if (0 != laserStatusErrorCode) {
			System.out.format("LaserEngraveSetupFileName(): %d\n", laserStatusErrorCode);
		}
		
		return hResult;
    }
    
    private int  LaserEngraveText(String elementName, String laserText) {
    	byte[] laserTextXML = CreateLaserEngraveTextXML(elementName, laserText);
    	
    	// send the data to driver using Windows bidi interface
    	int sizeOfInputBytes = laserTextXML.length;
	    byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
	    int sizeOfReturnXML[] = new int[1];
	    sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

	    int hResult = -1; // not S_OK
	    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_ENGRAVE_TEXT, laserTextXML, 
				sizeOfInputBytes, BIDI_SET, returnXML, sizeOfReturnXML);
	    
	    PrinterStatusXML printerStatusXml = new PrinterStatusXML();
		printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
		int laserStatusErrorCode = printerStatusXml.GetErrorCode();
		if (0 != laserStatusErrorCode) {
			System.out.format("LaserEngraveText(): %d\n", laserStatusErrorCode);
		}
		
		return hResult;
    }
    
    private int  LaserEngraveBinary(String elementName, String base64Data) {
        byte[] laserBinaryXML = CreateLaserEngraveBinaryXML(elementName, base64Data);
    	
    	// send the data to driver using Windows bidi interface
    	int sizeOfInputBytes = laserBinaryXML.length;
	    byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
	    int sizeOfReturnXML[] = new int[1];
	    sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;

	    int hResult = -1; // not S_OK
	    hResult = XpsDriverInteropLib.INSTANCE.BidiXPSDriverInterface(mPrinterName, LASER_ENGRAVE_BINARY, laserBinaryXML, 
				sizeOfInputBytes, BIDI_SET, returnXML, sizeOfReturnXML);
	    
	    PrinterStatusXML printerStatusXml = new PrinterStatusXML();
		printerStatusXml.Parse(returnXML, sizeOfReturnXML, false);
		int laserStatusErrorCode = printerStatusXml.GetErrorCode();
		if (0 != laserStatusErrorCode) {
			System.out.format("LaserEngraveText(): %d\n", laserStatusErrorCode);
		}
		
		return hResult;
    }
    
    private void LaserEngraveSimplexCard() {
    	String photo = fileToBase64String("ARMSTROT.jpg");
    	String signature = fileToBase64String("ARMSTROT_sign.jpg");

    	// Specify the laser setup file name, and variable elements count
    	int elementCount = 5;
    	if (S_OK == LaserEngraveSetupFileName(LASER_VARIABLE_SIMPLEX_SETUP_FILE_NAME, elementCount)) 
    	{
    		// Specify the laser data for the elements defined in the laser setup file 
    		LaserEngraveBinary("PHOTO", photo); // element "PHOTO" has been defined as a variable binary element 
    		LaserEngraveText("GIVEN_NAME", "John M."); // element "GIVEN_NAME" has been defined as a variable text element 
    		LaserEngraveText("FAMILY_NAME", "Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
    		LaserEngraveText("DOB", "01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
    		LaserEngraveBinary("SIGNATURE", signature); // element "SIGNATURE" has been defined as a variable binary element
    	}
    }
    
    private void LaserEngraveDuplexCard() throws IOException {
    	String photo = fileToBase64String("ARMSTROT.jpg");
    	String signature = fileToBase64String("ARMSTROT_sign.jpg");

        // Initialize 2D barcode buffer with ASCII data (preferably 7-bit )
    	String barcode2DStr = fileToBase64String("2D_BarcodeText.txt");
       
        // Specify the laser setup file name, and variable elements count
    	int elementCount = 7;
    	if (S_OK == LaserEngraveSetupFileName(LASER_VARIABLE_DUPLEX_SETUP_FILE_NAME, elementCount)) 
    	{
    		// Specify the front side of laser data for the elements defined in the laser setup file
    		LaserEngraveBinary("PHOTO", photo); // element "PHOTO" has been defined as a variable binary element 
    		LaserEngraveText("GIVEN_NAME", "John M."); // element "GIVEN_NAME" has been defined as a variable text element
    		LaserEngraveText("FAMILY_NAME", "Doe"); // element "FAMILY_NAME" has been defined as a variable text element 
    		LaserEngraveText("DOB", "01 / 01 / 1985"); // element "DOB" has been defined as a variable text element
    		LaserEngraveBinary("SIGNATURE", signature); // element "SIGNATURE" has been defined as a variable binary element 

	        // Specify the back side of laser data for the elements defined in the laser setup file 
    		LaserEngraveText("BARCODE_1D", "0123456789"); // element "BARCODE_1D" has been defined as a variable text element
	    	LaserEngraveBinary("BARCODE_2D", barcode2DStr); // element "BARCODE_2D" has been defined as a variable binary element 
    	}
    }
    
    private int LaserEngraveStatic() {
    	int hResult = -1; // not S_OK
    	hResult = LaserEngraveSetupFileName(LASER_STATIC_SETUP_FILE_NAME, 0);
    	
    	return hResult;
    }
    
    public void LaserDemo() throws IOException {
    	System.out.println("LaserDemo called");
        byte returnXML[] = new byte[XPS_Java_SDK.BUFFSIZE];
        int sizeOfReturnXML[] = new int[1];
        sizeOfReturnXML[0] = XPS_Java_SDK.BUFFSIZE;
        boolean bJobStarted = false;
        
        PrinterStatusXML printerStatusXml = new PrinterStatusXML();
        
        System.out.println("Calling StartJob2");
        int hResult = -1;
        String strHopperID = "1";
		String strCardEjectSide = "default";
        
        hResult = XpsDriverInteropLib.INSTANCE.StartJob2(mPrinterName,
                !mInputHopper.equals("") ? mInputHopper : strHopperID, 
	        	strCardEjectSide, 
                returnXML, sizeOfReturnXML);
        if (hResult == S_OK) {
        	System.out.println("StartJob2 success");
            bJobStarted = true;

            // get PrintJobID
            printerStatusXml.Parse(returnXML, sizeOfReturnXML); 
    	
	    	if (mMagstripeEncode && mDoPrint) {
	    		JavaPrint javaPrint = new JavaPrint(mPrinterName, mTwoSidedEngrave, false, "1", true, null, null, true, true);
	        	javaPrint.PrintDemo(); // does not contain StartJob2 or EndJob
	        	Util.WaitUntilJobSpooled(mPrinterName);
	    	} else if (mMagstripeEncode && !mDoPrint) {
	            Magstripe magStripe = new Magstripe(mPrinterName);
	            magStripe.LaserMagstripeEncode(); // does not contain StartJob2 or EndJob
	        } else if (mDoPrint && !mMagstripeEncode) {
	        	JavaPrint javaPrint = new JavaPrint(mPrinterName, mTwoSidedEngrave, false, "1", false, null, null, true, true);
	        	javaPrint.PrintDemo(); // does not contain StartJob2 or EndJob
	        	Util.WaitUntilJobSpooled(mPrinterName);
	        }
            
	    	// Allow import time to complete, as this setup is used for engraving sample
        	try {
        	    Thread.sleep(5000); // milliseconds
        	} 
        	catch(InterruptedException ex) {
        	    Thread.currentThread().interrupt();
        	}
        	
	        if (mStaticEngrave && !mTwoSidedEngrave) {
	        	LaserEngraveStatic();
	        } else if (mTwoSidedEngrave && !mStaticEngrave){
	        	LaserEngraveDuplexCard();
		    } else if (!mTwoSidedEngrave){
		    	LaserEngraveSimplexCard();
		    }
	        
        } else {
        	System.out.format("StartJob2 failed, hResult:%d\n", hResult);
        }
        
        if (bJobStarted) {
            System.out.println("EndJob called");
            XpsDriverInteropLib.INSTANCE.EndJob(mPrinterName);
        }

        if (mPollCompletion) {
            Util.PollForJobCompletion(mPrinterName, printerStatusXml.GetPrintJobID());
        }
    }
    
} //end class Laser
