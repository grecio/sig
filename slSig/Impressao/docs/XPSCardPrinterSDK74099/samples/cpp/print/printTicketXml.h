////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation. All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////
#pragma once

struct PrintTicketXml {
    static const CString Prefix;

    static const CString DisablePrintingAll;
    static const CString DisablePrintingOff;
    static const CString DisablePrintingFront;
    static const CString DisablePrintingBack;

    static const CString FlipFrontNone;
    static const CString FlipFrontFlipped;

    static const CString FlipBackNone;
    static const CString FlipBackFlipped;

    static const CString FrontTopcoatBlockingPreset_All;
    static const CString FrontTopcoatBlockingPreset_Except;
    static const CString FrontTopcoatBlockingPreset_ISO_7816;
    static const CString FrontTopcoatBlockingPreset_ISO_2Track;
    static const CString FrontTopcoatBlockingPreset_ISO_3Track;
    static const CString FrontTopcoatBlockingPreset_JIS;

    static const CString BackTopcoatBlockingPreset_All;
    static const CString BackTopcoatBlockingPreset_Except;
    static const CString BackTopcoatBlockingPreset_ISO_7816;
    static const CString BackTopcoatBlockingPreset_ISO_2Track;
    static const CString BackTopcoatBlockingPreset_ISO_3Track;
    static const CString BackTopcoatBlockingPreset_JIS;

    static const CString Suffix;
};

const CString PrintTicketXml::Prefix(
    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    "<psf:PrintTicket version=\"1\" "
    "xmlns:psf=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework\" "
    "xmlns:psk=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords\" "
    "xmlns:ns0000=\"http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer\"> ");

const CString PrintTicketXml::DisablePrintingAll(
    "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    "<psf:Option name=\"ns0000:All\" />"
    "</psf:Feature>");

const CString PrintTicketXml::DisablePrintingOff(
    "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    "<psf:Option name=\"ns0000:OFF\" />"
    "</psf:Feature>");

const CString PrintTicketXml::DisablePrintingFront(
    "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    "<psf:Option name=\"ns0000:Front\"/>"
    "</psf:Feature>");

const CString PrintTicketXml::DisablePrintingBack(
    "<psf:Feature name=\"ns0000:DocumentEncodeOnly\">"
    "<psf:Option name=\"ns0000:Back\" /> "
    "</psf:Feature>");

const CString PrintTicketXml::FlipFrontNone(
    "<psf:Feature name=\"ns0000:PageFlipFront\">"
    "<psf:Option name=\"ns0000:None\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FlipFrontFlipped(
    "<psf:Feature name=\"ns0000:PageFlipFront\">"
    "<psf:Option name=\"ns0000:Flipped\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FlipBackNone(
    "<psf:Feature name=\"ns0000:PageFlipBack\">"
    "<psf:Option name=\"ns0000:None\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FlipBackFlipped(
    "<psf:Feature name=\"ns0000:PageFlipBack\">"
    "<psf:Option name=\"ns0000:Flipped\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_All(
    "<psf:Feature name=\"ns0000:PageFrontPrintAndTopcoat\">"
    "<psf:Option name=\"ns0000:All\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_Except(
    "<psf:Feature name=\"ns0000:PageFrontPrintAndTopcoat\">"
    "<psf:Option name=\"ns0000:Except\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_ISO_7816(
    "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO_7816_ContactedSmartCard\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_ISO_2Track(
    "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO2_TrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_ISO_3Track(
    "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO3_TrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::FrontTopcoatBlockingPreset_JIS(
    "<psf:Feature name=\"ns0000:PageFrontTopcoatException\">"
    "<psf:Option name=\"ns0000:JIS_SingleTrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_All(
    "<psf:Feature name=\"ns0000:PageBackPrintAndTopcoat\">"
    "<psf:Option name=\"ns0000:All\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_Except(
    "<psf:Feature name=\"ns0000:PageBackPrintAndTopcoat\">"
    "<psf:Option name=\"ns0000:Except\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_ISO_7816(
    "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO_7816_ContactedSmartCard\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_ISO_2Track(
    "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO2_TrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_ISO_3Track(
    "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    "<psf:Option name=\"ns0000:ISO3_TrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::BackTopcoatBlockingPreset_JIS(
    "<psf:Feature name=\"ns0000:PageBackTopcoatException\">"
    "<psf:Option name=\"ns0000:JIS_SingleTrackMagneticStripe\" />"
    "</psf:Feature>");

const CString PrintTicketXml::Suffix("</psf:PrintTicket>");
