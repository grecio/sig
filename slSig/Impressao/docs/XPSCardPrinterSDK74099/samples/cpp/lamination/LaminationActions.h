////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#pragma once

#include <atlstr.h>

class LaminationActions {
public:

    enum Actions {
        doesNotApply,
        front,
        back,
        bothSides,
        frontTwice,
        invalidAction
    };

    static Actions GetLaminationAction(CString actionInput);

    static CString GetLaminationActionXML(Actions action);

private:

    static bool IsValidLaminationActionInput(const CString actionInput);
    static bool IsValidLaminationAction(Actions action);

    static const CString _doesNotApply;
    static const CString _front;
    static const CString _back;
    static const CString _bothSides;
    static const CString _frontTwice;

    static const CString _actionXMLStrings[];
};
