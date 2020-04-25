////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include "util.h"
#include "commandLineOptions.h"

void usage();

void UpdateDevmode(
    const HANDLE   printerHandle,
    const CString  printerName,
    DEVMODE*       devmode);

CString GetPrintTicketXml(const CommandLineOptions& commandLineOptions);

void DrawCardFront(
    const HDC&                  printerDC,
    const CommandLineOptions&   commandLineOptions);

int CreatePrintJob(
    const HANDLE                printerHandle,
    const CommandLineOptions&   commandLineOptions,
    DEVMODE*                    devmode);

void EndJob(CComPtr <IBidiSpl>& bidiSpl);

int PrintTextAndGraphics(const CommandLineOptions& commandLineOptions);