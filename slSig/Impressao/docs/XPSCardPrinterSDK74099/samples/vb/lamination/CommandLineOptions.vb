''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' populate a CommandLineOptions data structure from parsed command line args.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO

Namespace Lamination

    Public Class CommandLineOptions

        Public printerName As String
        Public jobCompletion As Boolean
        Public L1Action As LaminationActions.Actions = LaminationActions.Actions.doesNotApply
        Public L2Action As LaminationActions.Actions = LaminationActions.Actions.doesNotApply
        Public hopperID As String = ""
        Public cardEjectSide As String = ""

        Public Sub Validate()
            If hopperID <> "" Then

                If hopperID <> "1" AndAlso hopperID <> "2" AndAlso hopperID <> "3" AndAlso hopperID <> "4" AndAlso hopperID <> "5" AndAlso hopperID <> "6" AndAlso hopperID <> "exception" Then
                    Console.WriteLine("invalid hopperID: {0}", hopperID)
                    Environment.[Exit](-1)
                End If
            End If

            If cardEjectSide <> "" Then

                If cardEjectSide <> "front" AndAlso cardEjectSide <> "back" Then
                    Console.WriteLine("invalid cardEjectSide: {0}", cardEjectSide)
                    Environment.[Exit](-1)
                End If
            End If
        End Sub


        Public Shared Sub Usage()
            Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
            Console.WriteLine(thisExeName & " demonstrates lamination functionality of the printer and driver.")
            Console.WriteLine("Overrides the driver printing preference settings.")
            Console.WriteLine()
            Console.WriteLine("Usage:")
            Console.WriteLine(thisExeName & " -n <printername> [-x <F|B|A|T>] [-c] [-i <input hopper>] [-f <side>]")
            Console.WriteLine("where")
            Console.WriteLine("  -n <printername> specifies which printer to use. Required.")
            Console.WriteLine("  -x specifies the laminator station to use.")
            Console.WriteLine("      Valid values are ""1"" and ""2"".")
            Console.WriteLine("  Laminator actions:")
            Console.WriteLine("    F --> laminate the front.")
            Console.WriteLine("    B --> laminate the back.")
            Console.WriteLine("    A --> laminate both front and back.")
            Console.WriteLine("    T --> laminate the front twice.")
            Console.WriteLine()
            Console.WriteLine("  -c Poll for job completion.")
            Console.WriteLine("  -f <side>  Flip card on output.")
            Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""xps card printer"" -1 F -2 B")
            Console.WriteLine("  the front of the card will be laminated in station 1 and ")
            Console.WriteLine("  the back of the card will be laminated in station 2.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""xps card printer"" -1 T")
            Console.WriteLine("  the front of the card will be laminated in station 1 two times.")
            Console.WriteLine("  Station 2 is not used.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""xps card printer"" -1 F -1 B")
            Console.WriteLine("  the front of the card will be laminated in station 1.")
            Console.WriteLine("  The first specification of a station will be used.")
            Console.WriteLine()
            Environment.Exit(-1)
        End Sub

        Public Shared Function CreateFromArguments(args As String()) As CommandLineOptions

            If args.Length = 0 Then
                Usage()
            End If

            Dim commandLineOptions As New CommandLineOptions()
            Dim arguments As New CommandLine.Utility.Arguments(args)

            commandLineOptions.printerName = arguments("n")
            If (String.IsNullOrEmpty(commandLineOptions.printerName)) Then
                Usage()
            End If

            Dim laminationActionSpecified As Boolean = False

            Dim laminationActionArg As String = arguments("1")
            If Not String.IsNullOrEmpty(laminationActionArg) Then
                laminationActionSpecified = True
                commandLineOptions.L1Action = LaminationActions.GetLaminationAction(laminationActionArg)
            End If

            laminationActionArg = arguments("2")
            If Not String.IsNullOrEmpty(laminationActionArg) Then
                laminationActionSpecified = True
                commandLineOptions.L2Action = LaminationActions.GetLaminationAction(laminationActionArg)
            End If

            If Not laminationActionSpecified Then
                Usage()
            End If

            commandLineOptions.jobCompletion = Not String.IsNullOrEmpty(arguments("c"))
            commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
            commandLineOptions.cardEjectSide = If(String.IsNullOrEmpty(arguments("f")), String.Empty, arguments("f").ToLower())

            Return commandLineOptions
        End Function
    End Class
End Namespace
