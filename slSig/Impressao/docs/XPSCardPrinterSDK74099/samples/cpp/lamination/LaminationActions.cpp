////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "printTicketXml.h"
#include "LaminationActions.h"

const CString LaminationActions::_doesNotApply(_T('N'));
const CString LaminationActions::_front(_T('F'));
const CString LaminationActions::_back(_T('B'));
const CString LaminationActions::_bothSides(_T('A'));
const CString LaminationActions::_frontTwice(_T('T'));

const CString  LaminationActions::_actionXMLStrings[] = {
    PrintTicketXml::LaminationActionDoNotApply,
    PrintTicketXml::LaminationActionSide1,
    PrintTicketXml::LaminationActionSide2,
    PrintTicketXml::LaminationActionBothSides,
    PrintTicketXml::LaminationActionSide1twice
};

LaminationActions::Actions LaminationActions::GetLaminationAction(
    CString actionInput)
{
    if (!IsValidLaminationActionInput(actionInput)) {
        return invalidAction;
    }

    actionInput.MakeUpper();

    if (_front == actionInput) {
        return front;
    }

    if (_back == actionInput) {
        return back;
    }

    if (_bothSides == actionInput) {
        return bothSides;
    }

    if (_frontTwice == actionInput) {
        return frontTwice;
    }

    return invalidAction;
}

CString LaminationActions::GetLaminationActionXML(Actions action)
{
    if (!IsValidLaminationAction(action)) {
        return PrintTicketXml::LaminationActionInvalidAction;
    }

    return _actionXMLStrings[action];
}

bool LaminationActions::IsValidLaminationActionInput(CString actionInput)
{
    actionInput.MakeUpper();

    const bool validAction =
        _doesNotApply == actionInput ||
        _front == actionInput ||
        _back == actionInput ||
        _bothSides == actionInput ||
        _frontTwice == actionInput;

    return validAction;
}

bool LaminationActions::IsValidLaminationAction(const Actions action)
{
    const bool isValidLaminationAction =
        doesNotApply == action ||
        front == action ||
        back == action ||
        bothSides == action ||
        frontTwice == action;

    return isValidLaminationAction;
}