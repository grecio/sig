''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' xml fragments used in constructing PrintTickets
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Public Structure PrintTicketXml
    Public Const Prefix As String = "<?xml version=""1.0"" encoding=""UTF-8""?>" + "<psf:PrintTicket version=""1"" " + "xmlns:psf=""http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"" " + "xmlns:psk=""http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords"" " + "xmlns:ns0000=""http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer""> "

    Public Const DisablePrintingAll As String = "<psf:Feature name=""ns0000:DocumentEncodeOnly"">" + "<psf:Option name=""ns0000:All"" />" + "</psf:Feature>"

    Public Const DisablePrintingOff As String = "<psf:Feature name=""ns0000:DocumentEncodeOnly"">" + "<psf:Option name=""ns0000:OFF"" />" + "</psf:Feature>"

    Public Const DisablePrintingFront As String = "<psf:Feature name=""ns0000:DocumentEncodeOnly"">" + "<psf:Option name=""ns0000:Front""/>" + "</psf:Feature>"

    Public Const DisablePrintingBack As String = "<psf:Feature name=""ns0000:DocumentEncodeOnly"">" + "<psf:Option name=""ns0000:Back"" /> " + "</psf:Feature>"

    Public Const FlipFrontNone As String = "<psf:Feature name=""ns0000:PageFlipFront"">" + "<psf:Option name=""ns0000:None"" />" + "</psf:Feature>"

    Public Const FlipFrontFlipped As String = "<psf:Feature name=""ns0000:PageFlipFront"">" + "<psf:Option name=""ns0000:Flipped"" />" + "</psf:Feature>"

    Public Const FlipBackNone As String = "<psf:Feature name=""ns0000:PageFlipBack"">" + "<psf:Option name=""ns0000:None"" />" + "</psf:Feature>"

    Public Const FlipBackFlipped As String = "<psf:Feature name=""ns0000:PageFlipBack"">" + "<psf:Option name=""ns0000:Flipped"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_All As String = "<psf:Feature name=""ns0000:PageFrontPrintAndTopcoat"">" + "<psf:Option name=""ns0000:All"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_Except As String = "<psf:Feature name=""ns0000:PageFrontPrintAndTopcoat"">" + "<psf:Option name=""ns0000:Except"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_ISO_7816 As String = "<psf:Feature name=""ns0000:PageFrontTopcoatException"">" + "<psf:Option name=""ns0000:ISO_7816_ContactedSmartCard"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_ISO_2Track As String = "<psf:Feature name=""ns0000:PageFrontTopcoatException"">" + "<psf:Option name=""ns0000:ISO2_TrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_ISO_3Track As String = "<psf:Feature name=""ns0000:PageFrontTopcoatException"">" + "<psf:Option name=""ns0000:ISO3_TrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const FrontTopcoatBlockingPreset_JIS As String = "<psf:Feature name=""ns0000:PageFrontTopcoatException"">" + "<psf:Option name=""ns0000:JIS_SingleTrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_All As String = "<psf:Feature name=""ns0000:PageBackPrintAndTopcoat"">" + "<psf:Option name=""ns0000:All"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_Except As String = "<psf:Feature name=""ns0000:PageBackPrintAndTopcoat"">" + "<psf:Option name=""ns0000:Except"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_ISO_7816 As String = "<psf:Feature name=""ns0000:PageBackTopcoatException"">" + "<psf:Option name=""ns0000:ISO_7816_ContactedSmartCard"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_ISO_2Track As String = "<psf:Feature name=""ns0000:PageBackTopcoatException"">" + "<psf:Option name=""ns0000:ISO2_TrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_ISO_3Track As String = "<psf:Feature name=""ns0000:PageBackTopcoatException"">" + "<psf:Option name=""ns0000:ISO3_TrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const BackTopcoatBlockingPreset_JIS As String = "<psf:Feature name=""ns0000:PageBackTopcoatException"">" + "<psf:Option name=""ns0000:JIS_SingleTrackMagneticStripe"" />" + "</psf:Feature>"

    Public Const Suffix As String = "</psf:PrintTicket>"
End Structure
