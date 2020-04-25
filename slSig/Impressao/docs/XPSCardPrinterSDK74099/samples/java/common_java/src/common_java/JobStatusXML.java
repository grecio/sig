////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
package common_java;

import java.io.StringReader;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

// sample job status response:
//  <?xml version="1.0" ?>
//  <!-- Job status xml file. -->
//  <JobStatus>
//    <ClientID>VISTATEST_{B0C8BE36-5342-4C7E-B74F-D9FCB7F69C4A}</ClientID>
//    <WindowsJobID>5</WindowsJobID>
//    <PrinterJobID>780</PrinterJobID>
//    <JobState>JobFailed</JobState>
//    <JobRestartCount>0</JobRestartCount>
//  </JobStatus>
public class JobStatusXML {

    public final String JOB_NOT_AVAILABLE = "NotAvailable";
    public final String JOB_ACTIVE = "JobActive";
    public final String JOB_SUCCEEDED = "JobSucceeded";
    public final String JOB_FAILED = "JobFailed";
    public final String JOB_CANCELLED = "JobCancelled";
    public final String CARD_NOT_RETRIEVED = "CardNotRetrieved";
    public final String CARD_READY_TO_RETRIEVE = "CardReadyToRetrieve";
    private String mXML;
    private String mJobState;

    public JobStatusXML(byte[] returnXML, int[] sizeOfReturnXML) {
        mJobState = JOB_NOT_AVAILABLE;
        if (returnXML != null && sizeOfReturnXML != null && sizeOfReturnXML[0] > 0) {
            int dataLength = sizeOfReturnXML[0];
            mXML = PrinterStatusXML.cStringToJavaString(returnXML, dataLength);
            if (mXML.length() > 0) {
                Parse(mXML);
            }
        }
    }

    public boolean IsJobActive() {
        return mJobState.equalsIgnoreCase(JOB_ACTIVE);
    }

    public boolean IsJobCancelled() {
        return mJobState.equalsIgnoreCase(JOB_CANCELLED);
    }

    public boolean IsFailed() {
        return mJobState.equalsIgnoreCase(JOB_FAILED);
    }

    public boolean IsNotAvailable() {
        return mJobState.equalsIgnoreCase(JOB_NOT_AVAILABLE);
    }

    public boolean IsJobSucceeded() {
        return mJobState.equalsIgnoreCase(JOB_SUCCEEDED);
    }

    public boolean IsCardNotRetieved() {
        return mJobState.equalsIgnoreCase(CARD_NOT_RETRIEVED);
    }

    public boolean IsCardReadyToRetrieve() {
        return mJobState.equalsIgnoreCase(CARD_READY_TO_RETRIEVE);
    }

    public boolean IsSpoolerReceivedData() {
        return !IsNotAvailable();
    }

    public String GetJobState() {
        return mJobState;
    }

    @SuppressWarnings("unused")
    private void Parse(String xmlString) {
        try {
            DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
            DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
            InputSource is = new InputSource();
            is.setCharacterStream(new StringReader(xmlString));

            Document doc = dBuilder.parse(is);
            doc.getDocumentElement().normalize();

            NodeList nList = doc.getElementsByTagName("JobStatus");

            for (int temp = 0; temp < nList.getLength(); temp++) {

                Node nNode = nList.item(temp);
                if (nNode.getNodeType() == Node.ELEMENT_NODE) {
                    Element eElement = (Element) nNode;
                    String mClientID = PrinterStatusXML.getTagValue("ClientID", eElement);
                    int mWindowsJobID = Integer.parseInt(PrinterStatusXML.getTagValue("WindowsJobID", eElement));
                    int mPrinterJobID = Integer.parseInt(PrinterStatusXML.getTagValue("PrinterJobID", eElement));
                    mJobState = PrinterStatusXML.getTagValue("JobState", eElement);
                    int mJobRestartCount = Integer.parseInt(PrinterStatusXML.getTagValue("JobRestartCount", eElement));
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
