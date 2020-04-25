////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

class CommandLineOptions {
public:

    CommandLineOptions();
    void Validate();

public:

    enum DisablePrinting {
        All, Off, Front, Back
    };

    DisablePrinting disablePrinting;
    bool            jobCompletion;
    bool            magstripe;
    short           numCopies;
    bool            portraitBack;
    bool            portraitFront;
    CString         printerName;
    bool            rotateBack;
    bool            rotateFront;
    bool            showXml;
    CString         topcoatBlockingBack;
    CString         topcoatBlockingFront;
    bool            twoPages;
	CString         hopperID;
    bool            checkHopper;
    CString         cardEjectSide;
};
