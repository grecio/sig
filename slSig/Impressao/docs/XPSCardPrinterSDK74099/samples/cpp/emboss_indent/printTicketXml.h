////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation. All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////
#pragma once

struct PrintTicketXml {
    static const CString Prefix;

    static const CString ToppingOn;
    static const CString ToppingOff;

    static const CString Suffix;
};

const CString PrintTicketXml::Prefix(
    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    "<psf:PrintTicket version=\"1\" "
    "xmlns:psf=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework\" "
    "xmlns:psk=\"http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords\" "
    "xmlns:ns0000=\"http://schemas.datacard.com/2009/09/printing/XPS_Card_Printer\"> ");

const CString PrintTicketXml::ToppingOn(
	"<psf:Feature name =\"ns0000:JobEmbosserTopping\">"
	"<psf:Option name =\"ns0000:On\" />"
	"</psf:Feature>");

const CString PrintTicketXml::ToppingOff(
	"<psf:Feature name =\"ns0000:JobEmbosserTopping\">"
	"<psf:Option name =\"ns0000:Off\" />"
	"</psf:Feature>");

const CString PrintTicketXml::Suffix("</psf:PrintTicket>");
