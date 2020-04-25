Imports System
Imports System.IO

Namespace laser

    Friend Class CommandLineOptions

        Public encodeMagstripe As Boolean
        Public pollForJobCompletion As Boolean
        Public retrieveLaserSetup As Boolean
        Public printText As Boolean
        Public laserEngraveStatic As Boolean
        Public laserExportFile As Boolean
        Public laserImportFile As Boolean
        Public printerName As String
        Public hopperID As String = ""


        Private Shared Sub Usage()
            Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
            Console.WriteLine(thisExeName & " demonstrates laser engraving along with options to")
            Console.WriteLine("perform interactive mode magnetic stripe encoding, print, and poll for job completion status.")
            Console.WriteLine()
            Console.WriteLine("Uses hardcoded data for laser engraving, magnetic stripe and printing.")
            Console.WriteLine()
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n <printername> [-u] [-d] [-r] [-s] [-e] [-p] [-c] [-i <input hopper>]")
            Console.WriteLine("options:")
            Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
            Console.WriteLine("  -u Export laser setup zip files from printer to PC.")
            Console.WriteLine("  -d Import laser setup zip files to the printer.")
            Console.WriteLine("  -r Retrieves laser setup files from the printer.")
            Console.WriteLine("  -s Laser engraves static laser setup file data. Default is to ")
            Console.WriteLine("     laser engrave variable setup file data, depending on the printer capabilities.")
            Console.WriteLine("  -e Encodes magnetic stripe data.")
            Console.WriteLine("  -p Print simple black text on front of the card.")
            Console.WriteLine("     depending on the printer capabilities.")
            Console.WriteLine("  -c Poll for job completion; needed to check for printer errors.")
            Console.WriteLine("  -i <input hopper> Defaults to input hopper #1.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""XPS Card Printer""")
            Console.WriteLine("  Laser engraves data.")
            Console.WriteLine()
            Console.WriteLine(thisExeName & " -n ""XPS Card Printer"" -e -p")
            Console.WriteLine("  Laser engraves, prints and encodes magstripe data on all three tracks of an ISO 3-track magnetic stripe.")
            Environment.[Exit](-1)
        End Sub

        Public Sub Validate()
            If String.IsNullOrEmpty(printerName) Then
                Usage()
            End If

            'if hopperID Is an empty string, that Is OK
            If hopperID <> "" Then
                If hopperID <> "1" AndAlso hopperID <> "2" AndAlso hopperID <> "3" AndAlso hopperID <> "4" AndAlso hopperID <> "5" AndAlso hopperID <> "6" AndAlso hopperID <> "exception" Then
                    Console.WriteLine("invalid hopperID: {0}", hopperID)
                    Environment.[Exit](-1)
                End If
            End If
        End Sub

        Shared Public Function CreateFromArguments(ByVal args As String()) As CommandLineOptions
            If args.Length = 0 Then Usage()
            Dim commandLineOptions As CommandLineOptions = New CommandLineOptions()
            Dim arguments As CommandLine.Utility.Arguments = New CommandLine.Utility.Arguments(args)
            If Not String.IsNullOrEmpty(arguments("h")) Then Usage()
            commandLineOptions.printerName = arguments("n")
            commandLineOptions.retrieveLaserSetup = Not String.IsNullOrEmpty(arguments("r"))
            commandLineOptions.encodeMagstripe = Not String.IsNullOrEmpty(arguments("e"))
            commandLineOptions.printText = Not String.IsNullOrEmpty(arguments("p"))
            commandLineOptions.pollForJobCompletion = Not String.IsNullOrEmpty(arguments("c"))
            commandLineOptions.laserEngraveStatic = Not String.IsNullOrEmpty(arguments("s"))
            commandLineOptions.laserImportFile = Not String.IsNullOrEmpty(arguments("d"))
            commandLineOptions.laserExportFile = Not String.IsNullOrEmpty(arguments("u"))
            commandLineOptions.hopperID = If(String.IsNullOrEmpty(arguments("i")), String.Empty, arguments("i").ToLower())
            Return commandLineOptions
        End Function
    End Class
End Namespace
