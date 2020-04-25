////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////

namespace Lamination {

    public class PrintTicketXml {

        public const string Prefix = ""
        + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
        + "<psf:PrintTicket version=\"1\" "
        + "xmlns:psf=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework\" "
        + "xmlns:psk=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords\" "
        + "xmlns:ns0000=\"http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer\"> ";

        public const string Suffix = "</psf:PrintTicket>";

        public const string FeatureNamePrefix = "<psf:Feature ";

        public const string FeatureNameSuffix = "</psf:Feature>";

        public const string JobLamination1FeatureName =
           "name=\"ns0000:JobLamination1Options\">";

        public const string JobLamination2FeatureName =
           "name=\"ns0000:JobLamination2Options\">";

        public const string LaminationActionDoNotApply =
           "<psf:Option name=\"ns0000:Donotapply\" />";

        public const string LaminationActionSide1 =
           "<psf:Option name=\"ns0000:Side1\" />";

        public const string LaminationActionSide2 =
           "<psf:Option name=\"ns0000:Side2\" />";

        public const string LaminationActionBothSides =
           "<psf:Option name=\"ns0000:BothSides\" />";

        public const string LaminationActionSide1twice =
           "<psf:Option name=\"ns0000:Side1twice\" />";
    }
}