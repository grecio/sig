''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Driver SDK vb.net lamination sample.
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing.Printing
Imports System.IO
Imports System.Reflection
Imports dxp01sdk

Namespace Lamination

    Public Class Lamination

        Public Shared Function GetLongExeName() As String
            Dim longExeName As String = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
            Return longExeName
        End Function

        Public Shared Sub Main(args As String())

            Dim commandLineOptions As CommandLineOptions = commandLineOptions.CreateFromArguments(args)
            commandLineOptions.Validate()

            Dim bidiSpl As BidiSplWrap = Nothing
            Dim printerJobID As Integer = 0

            Try
                bidiSpl = New BidiSplWrap()
                bidiSpl.BindDevice(commandLineOptions.printerName)

                Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
                Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

                Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
                Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)

                '  A printer may have both laminator station 1 and station 2 installed.
                '  In that case the option will be "L1, L2" so 'Contains' as opposed to 
                '  '=' must be used... 
                If LaminationActions.Actions.doesNotApply <> commandLineOptions.L1Action Then
                    If Not printerOptionsValues._laminator.Contains("L1") Then
                        Throw New Exception(commandLineOptions.printerName + " does not have a station 1 laminator installed.")
                    End If
                End If

                If LaminationActions.Actions.doesNotApply <> commandLineOptions.L2Action Then
                    If Not printerOptionsValues._laminator.Contains("L2") Then
                        Throw New Exception(commandLineOptions.printerName + " does not have a station 2 laminator installed.")
                    End If
                End If

                If "Ready" <> printerOptionsValues._printerStatus AndAlso "Busy" <> printerOptionsValues._printerStatus Then
                    Throw New Exception(commandLineOptions.printerName + " is not ready. status: " + printerOptionsValues._printerStatus)
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
