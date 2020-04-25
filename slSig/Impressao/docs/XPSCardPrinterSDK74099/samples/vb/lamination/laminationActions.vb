''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Namespace Lamination

    Public Class LaminationActions

        Public Enum Actions
            doesNotApply
            front
            back
            bothSides
            frontTwice
            invalidAction
        End Enum

        Private Const _doesNotApply As Char = "N"c
        Private Const _front As Char = "F"c
        Private Const _back As Char = "B"c
        Private Const _bothSides As Char = "A"c
        Private Const _frontTwice As Char = "T"c
        Private Shared _actionXMLStrings As String() = {PrintTicketXml.LaminationActionDoNotApply, PrintTicketXml.LaminationActionSide1, PrintTicketXml.LaminationActionSide2, PrintTicketXml.LaminationActionBothSides, PrintTicketXml.LaminationActionSide1twice}

        Public Shared Function GetLaminationActionXML(action As Actions) As String
            If Not IsValidLaminationAction(action) Then
                CommandLineOptions.Usage()
            End If

            Return _actionXMLStrings(CInt(action))
        End Function

        Public Shared Function GetLaminationAction(action As String) As Actions
            If Not IsValidLaminationActionInput(action) Then
                CommandLineOptions.Usage()
            End If

            Dim theAction As Char = Char.ToUpper(action(0))
            Dim retValue As Actions = Actions.doesNotApply

            Select Case theAction
                Case _front
                    retValue = Actions.front
                    Exit Select
                Case _back
                    retValue = Actions.back
                    Exit Select
                Case _bothSides
                    retValue = Actions.bothSides
                    Exit Select
                Case _frontTwice
                    retValue = Actions.frontTwice
                    Exit Select
                Case Else
                    Exit Select
            End Select

            Return retValue
        End Function

        Private Shared Function IsValidLaminationActionInput(action As String) As Boolean
            If action.Length <> 1 Then
                Return False
            End If

            Dim theAction As Char = Char.ToUpper(action(0))

            Dim validAction As Boolean = (_doesNotApply = theAction) OrElse (_front = theAction) OrElse (_back = theAction) OrElse (_bothSides = theAction) OrElse (_frontTwice = theAction)

            Return validAction
        End Function

        Private Shared Function IsValidLaminationAction(action As Actions) As Boolean
            Dim validAction As Boolean = (Actions.doesNotApply = action) OrElse (Actions.front = action) OrElse (Actions.back = action) OrElse (Actions.bothSides = action) OrElse (Actions.frontTwice = action)
            Return validAction
        End Function
    End Class
End Namespace
