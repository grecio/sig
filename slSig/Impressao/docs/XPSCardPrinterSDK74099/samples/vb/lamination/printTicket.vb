''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Namespace Lamination

    Class PrintTicketXml

        Public Const Prefix As String = "<?xml version=""1.0"" encoding=""UTF-8""?>" &
            "<psf:PrintTicket version=""1"" " &
            "xmlns:psf=""http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"" " &
            "xmlns:psk=""http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords"" " &
            "xmlns:ns0000=""http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer""> "

        Public Const Suffix As String = "</psf:PrintTicket>"

        Public Const FeatureNamePrefix As String = "<psf:Feature "

        Public Const FeatureNameSuffix As String = "</psf:Feature>"

        Public Const JobLamination1FeatureName As String =
           "name=""ns0000:JobLamination1Options"">"

        Public Const JobLamination2FeatureName As String =
           "name=""ns0000:JobLamination2Options"">"

        Public Const LaminationActionDoNotApply As String =
           "  <psf:Option name=""ns0000:Donotapply"" />"

        Public Const LaminationActionSide1 As String =
           "  <psf:Option name=""ns0000:Side1"" />"

        Public Const LaminationActionSide2 As String =
           "  <psf:Option name=""ns0000:Side2"" />"

        Public Const LaminationActionBothSides As String =
           "  <psf:Option name=""ns0000:BothSides"" />"

        Public Const LaminationActionSide1twice As String =
           "  <psf:Option name=""ns0000:Side1twice"" />"

    End Class

End Namespace
