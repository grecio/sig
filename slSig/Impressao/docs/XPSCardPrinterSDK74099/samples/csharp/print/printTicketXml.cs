////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// xml fragments used in constructing PrintTickets
////////////////////////////////////////////////////////////////////////////////

public struct PrintTicketXml {

    public const string Prefix = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    + "<psf:PrintTicket version=\"1\" "
    + "xmlns:psf=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework\" "
    + "xmlns:psk=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords\" "
    + "xmlns:ns0000=\"http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer\"> ";

    public const string DisablePrintingAll = "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    + "<psf:Option name=\"ns0000:All\" />"
    + "</psf:Feature>";

    public const string DisablePrintingOff = "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    + "<psf:Option name=\"ns0000:OFF\" />"
    + "</psf:Feature>";

    public const string DisablePrintingFront = "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    + "<psf:Option name=\"ns0000:Front\"/>"
    + "</psf:Feature>";

    public const string DisablePrintingBack = "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    + "<psf:Option name=\"ns0000:Back\" /> "
    + "</psf:Feature>";

    public const string FlipFrontNone = "<psf:Feature name=\"ns0000:PageFlipFront\">"
    + "<psf:Option name=\"ns0000:None\" />"
    + "</psf:Feature>";

    public const string FlipFrontFlipped = "<psf:Feature name=\"ns0000:PageFlipFront\">"
    + "<psf:Option name=\"ns0000:Flipped\" />"
    + "</psf:Feature>";

    public const string FlipBackNone = "<psf:Feature name=\"ns0000:PageFlipBack\">"
    + "<psf:Option name=\"ns0000:None\" />"
    + "</psf:Feature>";

    public const string FlipBackFlipped = "<psf:Feature name=\"ns0000:PageFlipBack\">"
    + "<psf:Option name=\"ns0000:Flipped\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_All = "<psf:Feature name=\"ns0000:PageFrontPrintAndTopcoat\">"
    + "<psf:Option name=\"ns0000:All\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_Except = "<psf:Feature name=\"ns0000:PageFrontPrintAndTopcoat\">"
    + "<psf:Option name=\"ns0000:Except\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_ISO_7816 = "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO_7816_ContactedSmartCard\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_ISO_2Track = "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO2_TrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_ISO_3Track = "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO3_TrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string FrontTopcoatBlockingPreset_JIS = "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    + "<psf:Option name=\"ns0000:JIS_SingleTrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_All = "<psf:Feature name=\"ns0000:PageBackPrintAndTopcoat\">"
    + "<psf:Option name=\"ns0000:All\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_Except = "<psf:Feature name=\"ns0000:PageBackPrintAndTopcoat\">"
    + "<psf:Option name=\"ns0000:Except\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_ISO_7816 = "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO_7816_ContactedSmartCard\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_ISO_2Track = "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO2_TrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_ISO_3Track = "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    + "<psf:Option name=\"ns0000:ISO3_TrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string BackTopcoatBlockingPreset_JIS = "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    + "<psf:Option name=\"ns0000:JIS_SingleTrackMagneticStripe\" />"
    + "</psf:Feature>";

    public const string Suffix = "</psf:PrintTicket>";
}