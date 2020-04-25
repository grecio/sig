////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Datacard Corporation.  All Rights Reserved.
////////////////////////////////////////////////////////////////////////////////
using System;

namespace Lamination {

    public class LaminationActions {

        public enum Actions {
            doesNotApply,
            front,
            back,
            bothSides,
            frontTwice,
            invalidAction
        };

        private const char _doesNotApply = 'N';
        private const char _front = 'F';
        private const char _back = 'B';
        private const char _bothSides = 'A';
        private const char _frontTwice = 'T';

        private static string[] _actionXMLStrings = {
            PrintTicketXml.LaminationActionDoNotApply,
            PrintTicketXml.LaminationActionSide1,
            PrintTicketXml.LaminationActionSide2,
            PrintTicketXml.LaminationActionBothSides,
            PrintTicketXml.LaminationActionSide1twice
         };

        public static string GetLaminationActionXML(Actions action) {
            if (!IsValidLaminationAction(action)) {
                CommandLineOptions.Usage();
            }

            return _actionXMLStrings[(int) action];
        }

        public static Actions GetLaminationAction(string action) {
            if (!IsValidLaminationActionInput(action)) {
                CommandLineOptions.Usage();
            }

            char theAction = Char.ToUpper(action[0]);
            Actions retValue = Actions.doesNotApply;

            switch (theAction) {
                case _front:
                    retValue = Actions.front;
                    break;

                case _back:
                    retValue = Actions.back;
                    break;

                case _bothSides:
                    retValue = Actions.bothSides;
                    break;

                case _frontTwice:
                    retValue = Actions.frontTwice;
                    break;

                default:
                    break;
            }

            return retValue;
        }

        private static bool IsValidLaminationActionInput(string action) {
            if (action.Length != 1) {
                return false;
            }

            char theAction = Char.ToUpper(action[0]);

            bool validAction =
               (_doesNotApply == theAction) ||
               (_front == theAction) ||
               (_back == theAction) ||
               (_bothSides == theAction) ||
               (_frontTwice == theAction);

            return validAction;
        }

        private static bool IsValidLaminationAction(Actions action) {
            bool validAction =
               (Actions.doesNotApply == action) ||
               (Actions.front == action) ||
               (Actions.back == action) ||
               (Actions.bothSides == action) ||
               (Actions.frontTwice == action);
            return validAction;
        }
    }
}