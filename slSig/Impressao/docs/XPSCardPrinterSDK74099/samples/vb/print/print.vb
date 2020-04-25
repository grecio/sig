''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Driver SDK vb.net print sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports dxp01sdk

Namespace print

    Class print
        Private Shared Function GetHopperStatus(ByVal bidiSpl As BidiSplWrap, ByVal hopperId As String) As String
            Dim hopperStatusXml As String = bidiSpl.GetPrinterData(strings.HOPPER_STATUS)
            Dim hopperStatus As String = Util.ParseHopperStatusXML(hopperStatusXml, hopperId)
            Return hopperStatus
        End Function

        Public Shared Sub Main(args As String())

            Dim commandLineOptions As CommandLineOptions = CommandLineOptions.CreateFromArguments(args)
            commandLineOptions.Validate()
            Dim checkHopperStatus As Boolean = commandLineOptions.hopperID.Length > 0 AndAlso commandLineOptions.checkHopper

            Dim bidiSpl As BidiSplWrap = Nothing
            Dim printerJobID As Integer = 0

            Try
                bidiSpl = New BidiSplWrap()
                bidiSpl.BindDevice(commandLineOptions.printerName)

                Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
                Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

                Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
                Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

                If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                    Throw New Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus)
                End If

                If "Installed" <> printerOptionsValues._printHead Then
                    Throw New Exception(commandLineOptions.printerName + " does not have a print head installed.")
                End If

                If commandLineOptions.magstripe AndAlso Not printerOptionsValues._optionMagstripe.Contains("ISO") Then
                    Throw New Exception(commandLineOptions.printerName + " does not have an ISO magnetic stripe unit installed.")
                End If

                If checkHopperStatus Then
                    Dim hopperStatus As String = GetHopperStatus(bidiSpl, commandLineOptions.hopperID)
                    If [String].Compare(hopperStatus, "Empty", StringComparison.OrdinalIgnoreCase) = 0 Then
                        Throw New Exception("Hopper '" + commandLineOptions.hopperID + "' is empty.")
                    End If
                    Console.WriteLine((Convert.ToString("Status of hopper '" + commandLineOptions.hopperID + "': ") & hopperStatus) + ".")
                End If

                If commandLineOptions.jobCompletion OrElse (commandLineOptions.hopperID.Length > 0) OrElse (commandLineOptions.cardEjectSide.Length > 0) Then

                    Dim hopperID As String = "1"
                    Dim cardEjectSide As String = "default"
                    printerJobID = Util.StartJob(bidiSpl, 
							If((commandLineOptions.hopperID.Length > 0), commandLineOptions.hopperID, hopperID), 
							If((commandLineOptions.cardEjectSide.Length > 0), commandLineOptions.cardEjectSide, cardEjectSide))
                End If

                Dim printDocument As New SamplePrintDocument(commandLineOptions)
                printDocument.PrintController = New StandardPrintController()
                AddHandler printDocument.BeginPrint, New PrintEventHandler(AddressOf printDocument.OnBeginPrint)
                AddHandler printDocument.QueryPageSettings, New QueryPageSettingsEventHandler(AddressOf printDocument.OnQueryPageSettings)
                AddHandler printDocument.PrintPage, New PrintPageEventHandler(AddressOf printDocument.OnPrintPage)
                printDocument.Print()

                If 0 <> printerJobID Then
                    ' wait for the print spooling to finish and then issue an EndJob():
                    Util.WaitForWindowsJobID(bidiSpl, commandLineOptions.printerName)
                    bidiSpl.SetPrinterData(strings.ENDJOB)
                End If

                If commandLineOptions.jobCompletion Then
                    Util.PollForJobCompletion(bidiSpl, printerJobID)
                End If
            Catch e As BidiException
                Console.WriteLine(e.Message)
                Util.CancelJob(bidiSpl, e.PrinterJobID, e.ErrorCode)
            Catch e As Exception
                Console.WriteLine(e.Message)
                If 0 <> printerJobID Then
                    Util.CancelJob(bidiSpl, printerJobID, 0)
                End If
            Finally
                bidiSpl.UnbindDevice()
            End Try
        End Sub
    End Class

End Namespace
