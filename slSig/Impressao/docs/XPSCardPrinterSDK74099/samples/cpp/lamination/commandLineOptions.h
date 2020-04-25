////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include "LaminationActions.h"

class CommandLineOptions {
public:

    CommandLineOptions();
    void Validate();

private:

    bool HasInvalidLaminatorCmdLineOption();

public:

    CString printerName;
    LaminationActions::Actions L1Action;
    LaminationActions::Actions L2Action;
    bool jobCompletion;
    CString         hopperID;
    CString         cardEjectSide;
};
