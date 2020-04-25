''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' populate a CommandLineOptions data structure from parsed command line args.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO

Namespace print

    Class CommandLineOptions
        Public magstripe As Boolean
        Public jobCompletion As Boolean
        Public portraitBack As Boolean
        Public portraitFront As Boolean
        Public rotateBack As Boolean
        Public rotateFront As Boolean
        Public showXml As Boolean
        Public twoPages As Boolean
        Public numCopies As Short = 1
        Public printerName As String
        Public topcoatBlockingBack As String
        Public topcoatBlockingFront As String
        Public hopperID As String = ""
        Public cardEjectSide As String = ""
        Public checkHopper As Boolean

        Public Enum DisablePrintingEnum
            All
            Off
            Front
            Back
        End Enum

        Public disablePrinting As DisablePrintingEnum = DisablePrintingEnum.Off

        Private Shared Sub Usage()
            Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
            Console.WriteLine(thisExeName & " demonstrates print functionality of the printer and driver.")
            Console.WriteLine()
            Console.WriteLine("Uses hardcoded data for printing, magnetic stripe, topcoat region and")
            Console.WriteLine("print blocking region.")
            Console.WriteLine()
            Console.WriteLine("This sample changes the driver printing preference settings.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n <printername> [-r front | -r back | -r both] [-2]")
            Console.WriteLine("  [-o frontPort | -o backPort | -o bothPort] [ -s <number of copies>]")
            Console.WriteLine("  [-m ] [-d] [-x]")
            Console.WriteLine("  [-t all | -t chip | -t magJIS | -t mag2 | -t mag3 | -t custom]")
            Console.WriteLine("  [-u all | -u chip | -u magJIS | -u mag2 | -u mag3 | -u custom]")
            Console.WriteLine("  [-i <input hopper>] [-e] [-f <side>]")
            Console.WriteLine()
            Console.WriteLine("options:")
            Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
            Console.WriteLine("  -r <front | back | both >. Rotates the card image by 180 degrees for")
            Console.WriteLine("     front, back, or both sides.")
            Console.WriteLine("  -2 Prints a 2-sided (duplex) card. Default is front-side printing.")
            Console.WriteLine("  -o <frontPort | backPort | bothPort>. Sets portrait orientation")
            Console.WriteLine("     for a card side. Default is landscape orientation for both card sides.")
            Console.WriteLine("  -s <number of copies>. Default is 1.")
            Console.WriteLine("  -m Writes 3-track magnetic stripe data to backside of card using escapes.")
            Console.WriteLine("     Default is no encoding.")
            Console.WriteLine("  -d < All | Off | Front | Back > Disable Printing. Default is Off.")
            Console.WriteLine("  -x Display the print ticket data. Default is no display.")
            Console.WriteLine("  -t <all | chip | magJIS | mag2 | mag3 | custom> Top coat and print blocking")
            Console.WriteLine("     region for front of card. Use '-t all' to topcoat the entire card side")
            Console.WriteLine("     with no print blocking. Default is the current driver setting.")
            Console.WriteLine("  -u <all | chip | magJIS | mag2 | mag3 | custom> Top coat and print blocking")
            Console.WriteLine("     region for for back of card. Use '-u all' to topcoat the entire card side")
            Console.WriteLine("     with no print blocking. Default is the current driver setting.")
            Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
            Console.WriteLine("  -e Checks the status of the input hopper if input hopper is specified.")
            Console.WriteLine("  -c Poll for job completion.")
            Console.WriteLine("  -f <Front | Back>. Flip card on output.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""XPS Card Printer""")
            Console.WriteLine("  Prints a one-sided landscape card.")
            Console.WriteLine()
            Console.WriteLine(thisExeName + " -n ""XPS Card Printer"" -i 1 -e")
            Console.WriteLine("  Checks the status of input hopper 1 and prints a one-sided landscape card")
            Console.WriteLine("  if cards are present.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -r both -2 -o frontport")
            Console.WriteLine("  Prints a two-sided card with both sides of card image rotated 180 degrees and")
            Console.WriteLine("  with front side as portrait orientation and back side as landscape")
            Console.WriteLine("  orientation.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -2 -t all -u mag3")
            Console.WriteLine("  Prints a two-sided card with topcoat applied over all of side one and topcoat")
            Console.WriteLine("  and printing blocked over the 3-track magnetic stripe area on the back of the")
            Console.WriteLine("  card.")
            Environment.Exit(-1)
        End Sub

        Public Sub Validate()

            If String.IsNullOrEmpty(printerName) Then
                Console.WriteLine("printer name is required")
                Environment.Exit(-1)
            End If

            If numCopies <= 0 Then
                Console.WriteLine("invalid number of copies: {0}", numCopies)
                Environment.Exit(-1)
            End If

            Select Case topcoatBlockingFront.ToLower()
                Case ""
                    Exit Select
                Case "all"
                    Exit Select
                Case "chip"
                    Exit Select
                Case "magjis"
                    Exit Select
                Case "mag2"
                    Exit Select
                Case "mag3"
                    Exit Select
                Case "custom"
                    Exit Select
                Case Else
                    Console.WriteLine("invalid front topcoat / blocking option: {0}", topcoatBlockingFront)
                    Environment.Exit(-1)
                    Exit Select
            End Select

            Select Case topcoatBlockingBack.ToLower()
                Case ""
                    Exit Select
                Case "all"
                    Exit Select
                Case "chip"
                    Exit Select
                Case "magjis"
                    Exit Select
                Case "mag2"
                    Exit Select
                Case "mag3"
                    Exit Select
                Case "custom"
                    Exit Select
                Case Else
                    Console.WriteLine("invalid back topcoat / blocking option: {0}", topcoatBlockingBack)
                    Environment.Exit(-1)
                    Exit Select
            End Select

            'if hopperID Is an empty string, that Is OK
            If hopperID <> "" Then
                If hopperID <> "1" AndAlso hopperID <> "2" AndAlso hopperID <> "3" AndAlso hopperID <> "4" AndAlso hopperID <> "5" AndAlso hopperID <> "6" AndAlso hopperID <> "exception" Then
                    Console.WriteLine("invalid hopperID: {0}", hopperID)
                    Environment.[Exit](-1)
                End If
            End If

            'if cardEjectSide Is an empty string, that Is OK
            If cardEjectSide <> "" Then
                If cardEjectSide <> "front" AndAlso cardEjectSide <> "back" Then
                    Console.WriteLine("invalid cardEjectSide: {0}", cardEjectSide)
                    Environment.[Exit](-1)
                End If
            End If
        End Sub

        Public Shared Function CreateFromArguments(args As String()) As CommandLineOptions

            If args.Length = 0 Then
                Usage()
            End If

            Dim commandLineOptions As New CommandLineOptions()
            Dim arguments As New CommandLine.Utility.Arguments(args)

            If Not String.IsNullOrEmpty(arguments("h")) Then
                Usage()
            End If

            commandLineOptions.printerName = arguments("n")
            commandLineOptions.showXml = Not String.IsNullOrEmpty(arguments("x"))
            commandLineOptions.twoPages = Not String.IsNullOrEmpty(arguments("2"))
            commandLineOptions.magstripe = Not String.IsNullOrEmpty(arguments("m"))
            commandLineOptions.jobCompletion = Not String.IsNullOrEmpty(arguments("c"))
            commandLineOptions.checkHopper = Not String.IsNullOrEmpty(arguments("e"))
            commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
            commandLineOptions.cardEjectSide = If(String.IsNullOrEmpty(arguments("f")), String.Empty, arguments("f").ToLower())

            If Not String.IsNullOrEmpty(arguments("d")) Then
                Select Case arguments("d").ToLower()
                    Case "all"
                        commandLineOptions.disablePrinting = DisablePrintingEnum.All
                        Exit Select
                    Case "off"
                        commandLineOptions.disablePrinting = DisablePrintingEnum.Off
                        Exit Select
                    Case "front"
                        commandLineOptions.disablePrinting = DisablePrintingEnum.Front
                        Exit Select
                    Case "back"
                        commandLineOptions.disablePrinting = DisablePrintingEnum.Back
                        Exit Select
                End Select
            End If

            commandLineOptions.topcoatBlockingFront = If(String.IsNullOrEmpty(arguments("t")), String.Empty, arguments("t").ToLower())
            commandLineOptions.topcoatBlockingBack = If(String.IsNullOrEmpty(arguments("u")), String.Empty, arguments("u").ToLower())

            If Not String.IsNullOrEmpty(arguments("s")) Then
                commandLineOptions.numCopies = Short.Parse(arguments("s"))
            End If

            If Not String.IsNullOrEmpty(arguments("r")) Then
                Select Case arguments("r").ToLower()
                    Case "front"
                        commandLineOptions.rotateFront = True
                        Exit Select
                    Case "back"
                        commandLineOptions.rotateBack = True
                        Exit Select
                    Case "both"
                        commandLineOptions.rotateFront = True
                        commandLineOptions.rotateBack = True
                        Exit Select
                End Select
            End If

            If Not String.IsNullOrEmpty(arguments("o")) Then
                Select Case arguments("o").ToLower()
                    Case "frontport"
                        commandLineOptions.portraitFront = True
                        Exit Select
                    Case "backport"
                        commandLineOptions.portraitBack = True
                        Exit Select
                    Case "bothport"
                        commandLineOptions.portraitFront = True
                        commandLineOptions.portraitBack = True
                        Exit Select
                End Select
            End If
            Return commandLineOptions
        End Function
    End Class
End Namespace
