////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include <iostream>
#include "lamination.H"
#include "commandLineOptions.h"

CommandLineOptions::CommandLineOptions()
{
    L1Action = LaminationActions::doesNotApply;
    L2Action = LaminationActions::doesNotApply;
    jobCompletion = false;
    hopperID = L"";
    cardEjectSide = "";
}

bool CommandLineOptions::HasInvalidLaminatorCmdLineOption()
{
    if (LaminationActions::doesNotApply == L1Action &&
        LaminationActions::doesNotApply == L2Action) {
        return true;
    }

    if (LaminationActions::invalidAction == L1Action ||
        LaminationActions::invalidAction == L2Action) {
        return true;
    }

    return false;
}

void CommandLineOptions::Validate()
{
    if (printerName.IsEmpty()) {
        usage();
    }

    if (HasInvalidLaminatorCmdLineOption()) {
        usage();
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
            hopperID != L"exception") {
            std::cout << "invalid hopperID: " << CW2A(hopperID) << std::endl;
            ::exit(-1);
        }
    }

    // if cardEjectSide is an empty string, that is OK
    if (cardEjectSide != L"") {
        if (
            cardEjectSide != L"front" &&
            cardEjectSide != L"back") {
            std::cout << "invalid cardEjectSide: " << CW2A(cardEjectSide) << std::endl;
            ::exit(-1);
        }
    }
}