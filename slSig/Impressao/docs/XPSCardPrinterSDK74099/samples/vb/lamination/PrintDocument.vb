''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' Class derived from System.Drawing.Printing.PrintDocument.
'
' Demonstrate all the classic 'winforms'-style printing and use a PrintTicket
' to manipulate print settings.
'
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Printing
Imports System.Xml
Imports dxp01sdk

Namespace Lamination

    Class SamplePrintDocument
        Inherits PrintDocument

        Private _commandLineOptions As CommandLineOptions
        Private _userPrintTicket As New PrintTicket()
        Private _sizeFactor As Integer = 3

        Public Sub New(commandLineOptions As CommandLineOptions)
            _commandLineOptions = commandLineOptions
            PrinterSettings.PrinterName = commandLineOptions.printerName
            DocumentName = "XPS Card Printer SDK VB.NET Lamination Sample"
        End Sub

        Public Sub RestoreUserPreferences()
            Dim printQueue As New PrintQueue(New LocalPrintServer(), PrinterSettings.PrinterName)
            printQueue.UserPrintTicket = _userPrintTicket
            printQueue.Commit()
        End Sub

        Public Function GetPrintTicketXml(commandLineOptions As CommandLineOptions) As String

            Dim xml As String = PrintTicketXml.Prefix

            xml += PrintTicketXml.FeatureNamePrefix + _
                PrintTicketXml.JobLamination1FeatureName + _
                LaminationActions.GetLaminationActionXML(commandLineOptions.L1Action) + _
                PrintTicketXml.FeatureNameSuffix

            xml += PrintTicketXml.FeatureNamePrefix + _
                PrintTicketXml.JobLamination2FeatureName + _
                LaminationActions.GetLaminationActionXML(commandLineOptions.L2Action) + _
                PrintTicketXml.FeatureNameSuffix

            xml += PrintTicketXml.Suffix

            Return xml

        End Function

        Public Overloads Sub OnBeginPrint(sender As Object, printEventArgs As PrintEventArgs)
            Dim xmlString As String = GetPrintTicketXml(_commandLineOptions)
            If (xmlString.Length = 0) Then
                Throw New Exception("No lamination options specified.")
            End If

            ' prepare the PrintTicket for the entire print job.
            Dim printQueue As New PrintQueue(New LocalPrintServer(), PrinterSettings.PrinterName)
            _userPrintTicket = printQueue.UserPrintTicket

            Dim deltaPrintTicket As New PrintTicket()
            deltaPrintTicket.Duplexing = Duplexing.TwoSidedLongEdge
            deltaPrintTicket.CopyCount = 1
            deltaPrintTicket.PageOrientation = PageOrientation.Portrait

            Dim validationResult As ValidationResult = printQueue.MergeAndValidatePrintTicket(printQueue.UserPrintTicket, deltaPrintTicket)

            ' prepare to merge our PrintTicket xml into an actual PrintTicket:
            Dim xmlDocument As New XmlDocument()
            xmlDocument.LoadXml(xmlString)
            Dim memoryStream As New MemoryStream()
            xmlDocument.Save(memoryStream)
            memoryStream.Position = 0
            deltaPrintTicket = New PrintTicket(memoryStream)

            validationResult = printQueue.MergeAndValidatePrintTicket(validationResult.ValidatedPrintTicket, deltaPrintTicket)

            printQueue.UserPrintTicket = validationResult.ValidatedPrintTicket

            ' IMPORTANT: this Commit() call sets the driver's 'Printing Preferences'
            ' on this machine:
            printQueue.Commit()

        End Sub

        Public Sub DrawCardFront(pageEventArgs As PrintPageEventArgs)
            Using font As New Font("Arial", 8)
                Using brush As New SolidBrush(Color.Red)
                    Using pen As New Pen(Color.YellowGreen)
                        pageEventArgs.Graphics.DrawString("card front", font, brush, 10, 15)
                    End Using
                End Using
            End Using


            Dim colorBitmapFilename As String = Path.Combine(Lamination.GetLongExeName(), "color.bmp")
            If Not File.Exists(colorBitmapFilename) Then
                Throw New Exception("file not found: " & colorBitmapFilename)
            End If
            Dim colorBitmap As New Bitmap(colorBitmapFilename)
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width \ _sizeFactor, colorBitmap.Height \ _sizeFactor)

            Dim oneBppBitmapFilename As String = Path.Combine(Lamination.GetLongExeName(), "mono.bmp")

            If Not File.Exists(oneBppBitmapFilename) Then
                Throw New Exception(" file not fount: " & oneBppBitmapFilename)
            End If
            Dim kBitmap As New Bitmap(oneBppBitmapFilename)
            pageEventArgs.Graphics.DrawImage(kBitmap, 25, 100, kBitmap.Width \ _sizeFactor, kBitmap.Height \ _sizeFactor)

            Const UVBitmapFilename As String = "uv.bmp"
            If Not File.Exists(UVBitmapFilename) Then
                Throw New Exception(" file not fount: " & oneBppBitmapFilename)
            End If
            Dim UVBitmap As New Bitmap(UVBitmapFilename)
            pageEventArgs.Graphics.DrawImage(UVBitmap, 25, 150, kBitmap.Width \ _sizeFactor, kBitmap.Height \ _sizeFactor)

        End Sub

        Public Overloads Sub OnPrintPage(sender As Object, pageEventArgs As PrintPageEventArgs)
            DrawCardFront(pageEventArgs)
        End Sub
    End Class
End Namespace
