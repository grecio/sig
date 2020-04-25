'''/////////////////////////////////////////////////////////////////////////////
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' xml fragments used in constructing PrintTickets
'''/////////////////////////////////////////////////////////////////////////////

Public Structure PrintTicketXml

    Public Const Prefix As String = "<?xml version=""1.0"" encoding=""UTF-8""?>" + "<psf:PrintTicket version=""1"" " + "xmlns:psf=""http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework"" " + "xmlns:psk=""http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords"" " + "xmlns:ns0000=""http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer""> "

    Public Const ToppingOff As String = "<psf:Feature name =""ns0000:JobEmbosserTopping"">" + "<psf:Option name =""ns0000:Off"" />" + "</psf:Feature>"

    Public Const ToppingOn As String = "<psf:Feature name =""ns0000:JobEmbosserTopping"">" + "<psf:Option name =""ns0000:On"" />" + "</psf:Feature>"

    Public Const Suffix As String = "</psf:PrintTicket>"
End Structure
