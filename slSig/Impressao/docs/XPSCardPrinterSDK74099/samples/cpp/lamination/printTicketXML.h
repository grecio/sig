////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <atlstr.h>

namespace PrintTicketXml
{
    const CString Prefix(
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
        "<psf:PrintTicket version=\"1\" "
        "xmlns:psf=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework\" "
        "xmlns:psk=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords\" "
        "xmlns:ns0000=\"http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer\">");

    const CString Suffix("</psf:PrintTicket>");

    const CString FeatureNamePrefix("<psf:Feature ");
    const CString FeatureNameSuffix("</psf:Feature>");

    const CString JobLamination1FeatureName("name=\"ns0000:JobLamination1Options\">");
    const CString JobLamination2FeatureName("name=\"ns0000:JobLamination2Options\">");

    const CString LaminationActionDoNotApply("<psf:Option name=\"ns0000:Donotapply\" />");
    const CString LaminationActionSide1("<psf:Option name=\"ns0000:Side1\" />");
    const CString LaminationActionSide2("<psf:Option name=\"ns0000:Side2\" />");
    const CString LaminationActionBothSides("<psf:Option name=\"ns0000:BothSides\" />");
    const CString LaminationActionSide1twice("<psf:Option name=\"ns0000:Side1twice\" />");

    const CString LaminationActionInvalidAction;
}
