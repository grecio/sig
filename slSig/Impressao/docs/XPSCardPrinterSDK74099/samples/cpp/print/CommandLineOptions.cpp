////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
//
// commandline switch values for 'Print' XPS Driver SDK sample
////////////////////////////////////////////////////////////////////////////////
#include "StdAfx.h"
#include <iostream>
#include <sstream>
#include "print.h"
#include "CommandLineOptions.h"

using namespace std;

CommandLineOptions::CommandLineOptions() : 
jobCompletion(false)
, magstripe(false)
, numCopies(1)
, portraitBack(false)
, portraitFront(false)
, rotateBack(false)
, rotateFront(false)
, showXml(false)
, twoPages(false)
, disablePrinting(Off)
, hopperID(TEXT(""))
, checkHopper(false)
, cardEjectSide(TEXT(""))
{}

void CommandLineOptions::Validate()
{
    if (printerName.IsEmpty()) {
        usage();
    }

    if (numCopies <= 0) {
        cout << "bad number of copies: " << numCopies << endl;
        ::exit(-1);
    }

    if (
        topcoatBlockingFront != L""       &&
        topcoatBlockingFront != L"all"    &&
        topcoatBlockingFront != L"chip"   &&
        topcoatBlockingFront != L"magjis" &&
        topcoatBlockingFront != L"mag2"   &&
        topcoatBlockingFront != L"mag3"   &&
        topcoatBlockingFront != L"custom") {
        cout << "invalid front topcoat / blocking option: " << CW2A(topcoatBlockingFront) << endl;
        ::exit(-1);
    }

    if (
        topcoatBlockingBack != L""       &&
        topcoatBlockingBack != L"all"    &&
        topcoatBlockingBack != L"chip"   &&
        topcoatBlockingBack != L"magjis" &&
        topcoatBlockingBack != L"mag2"   &&
        topcoatBlockingBack != L"mag3"   &&
        topcoatBlockingBack != L"custom") {
        cout << "invalid back topcoat / blocking option: " << CW2A(topcoatBlockingBack) << endl;
        ::exit(-1);
    }

    // if hopperID is an empty string, that is OK
    if (hopperID != L"") {
        if (
            hopperID != L"1" &&
            hopperID != L"2" &&
            hopperID != L"3" &&
            hopperID != L"4" &&
            hopperID != L"5" &&
            hopperID != L"6" &&
            hopperID != L"exception") 
        {
            cout << "invalid hopperID: " << CW2A(hopperID) << endl;
            ::exit(-1);
        }
    }

    // if cardEjectSide is an empty string, that is OK
    if (cardEjectSide != L"") {
        if (
            cardEjectSide != L"front" &&
            cardEjectSide != L"back" )
        {
            cout << "invalid cardEjectSide: " << CW2A(cardEjectSide) << endl;
            ::exit(-1);
        }
    }

}