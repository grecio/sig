''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' Class derived from System.Drawing.Printing.PrintDocument.
'
' Demonstrate classic 'winforms' style printing and using a PrintTicket to
' manipulate print settings.
'
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.IO
Imports System.Printing
Imports System.Xml
Imports dxp01sdk

Namespace print
    Class SamplePrintDocument
        Inherits PrintDocument
        Private _commandLineOptions As CommandLineOptions
        Private _frontOfCard As Boolean = True
        Private _sizeFactor As Integer = 3

        Public Sub New(commandLineOptions As CommandLineOptions)
            _commandLineOptions = commandLineOptions
            PrinterSettings.PrinterName = commandLineOptions.printerName
            DocumentName = "xps driver sdk c# print sample"
        End Sub

        Public Overloads Sub OnBeginPrint(sender As Object, printEventArgs As PrintEventArgs)
            ' prepare the PrintTicket for the entire print job.
            Dim printQueue As New PrintQueue(New LocalPrintServer(), PrinterSettings.PrinterName)
            Dim deltaPrintTicket As New PrintTicket()
            deltaPrintTicket.Duplexing = If(_commandLineOptions.twoPages, Duplexing.TwoSidedLongEdge, Duplexing.OneSided)
            deltaPrintTicket.CopyCount = _commandLineOptions.numCopies
            deltaPrintTicket.PageOrientation = If(_commandLineOptions.portraitFront, PageOrientation.Portrait, PageOrientation.Landscape)

            Dim validationResult As ValidationResult = printQueue.MergeAndValidatePrintTicket(printQueue.UserPrintTicket, deltaPrintTicket)

            Dim xmlString As String = PrintTicketXml.Prefix

            xmlString += If(_commandLineOptions.rotateFront, PrintTicketXml.FlipFrontFlipped, PrintTicketXml.FlipFrontNone)

            Select Case _commandLineOptions.disablePrinting
                Case CommandLineOptions.DisablePrintingEnum.All
                    xmlString += PrintTicketXml.DisablePrintingAll
                    Exit Select
                Case CommandLineOptions.DisablePrintingEnum.Off
                    xmlString += PrintTicketXml.DisablePrintingOff
                    Exit Select
                Case CommandLineOptions.DisablePrintingEnum.Front
                    xmlString += PrintTicketXml.DisablePrintingFront
                    Exit Select
                Case CommandLineOptions.DisablePrintingEnum.Back
                    xmlString += PrintTicketXml.DisablePrintingBack
                    Exit Select
            End Select

            If _commandLineOptions.twoPages Then
                xmlString += If(_commandLineOptions.rotateBack, PrintTicketXml.FlipBackFlipped, PrintTicketXml.FlipBackNone)
            End If

            xmlString += GetTopcoatBlockingPrintTicketXml()
            xmlString += PrintTicketXml.Suffix

            ' prepare to merge our PrintTicket xml into an actual PrintTicket:
            Dim xmlDocument As New XmlDocument()
            xmlDocument.LoadXml(xmlString)
            Dim memoryStream As New MemoryStream()
            xmlDocument.Save(memoryStream)
            memoryStream.Position = 0
            deltaPrintTicket = New PrintTicket(memoryStream)

            validationResult = printQueue.MergeAndValidatePrintTicket(validationResult.ValidatedPrintTicket, deltaPrintTicket)

            printQueue.UserPrintTicket = validationResult.ValidatedPrintTicket

            If _commandLineOptions.showXml Then
                Util.DisplayPrintTicket(validationResult.ValidatedPrintTicket)
            End If

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

            Const colorBitmapFilename As String = "color.bmp"
            If Not File.Exists(colorBitmapFilename) Then
                Throw New Exception("file not found: " & colorBitmapFilename)
            End If
            Dim colorBitmap As New Bitmap(colorBitmapFilename)
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width \ _sizeFactor, colorBitmap.Height \ _sizeFactor)

            Const oneBppBitmapFilename As String = "mono.bmp"
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

            WriteCustomTopcoatBlockingEscapesFront(pageEventArgs.Graphics)

            If _commandLineOptions.magstripe Then
                WriteMagstripeEscapes(pageEventArgs.Graphics)
            End If
        End Sub

        Public Sub DrawCardBack(pageEventArgs As PrintPageEventArgs)
            Using font As New Font("Courier New", 8)
                Using brush As New SolidBrush(Color.Red)
                    Using pen As New Pen(Color.YellowGreen)
                        pageEventArgs.Graphics.DrawString("card back", font, brush, 10, 10)
                    End Using
                End Using
            End Using

            Dim colorBitmap As New Bitmap("color.bmp")
            pageEventArgs.Graphics.DrawImage(colorBitmap, 25, 50, colorBitmap.Width \ _sizeFactor, colorBitmap.Height \ _sizeFactor)

            Dim kBitmap As New Bitmap("mono.bmp")
            pageEventArgs.Graphics.DrawImage(kBitmap, 130, 50, kBitmap.Width \ _sizeFactor, kBitmap.Height \ _sizeFactor)

            Dim UVBitmap As New Bitmap("uv.bmp")
            pageEventArgs.Graphics.DrawImage(UVBitmap, 235, 50, kBitmap.Width \ _sizeFactor, kBitmap.Height \ _sizeFactor)

            WriteCustomTopcoatBlockingEscapesBack(pageEventArgs.Graphics)
        End Sub

        Public Overloads Sub OnPrintPage(sender As Object, pageEventArgs As PrintPageEventArgs)
            If _frontOfCard Then
                DrawCardFront(pageEventArgs)
                _frontOfCard = False
                pageEventArgs.HasMorePages = _commandLineOptions.twoPages
            Else
                DrawCardBack(pageEventArgs)
            End If
        End Sub

        Public Overloads Sub OnQueryPageSettings(sender As Object, queryEventArgs As QueryPageSettingsEventArgs)
            ' use this opportunity to adjust the orientation for the back side of the card:
            If Not _frontOfCard Then
                queryEventArgs.PageSettings.Landscape = If(_commandLineOptions.portraitBack, False, True)
            End If
        End Sub

        Private Sub WriteMagstripeEscapes(graphics As Graphics)
            ' emit some plain track 1, 2, 3 data. Assume IAT track configuration.
            Dim track1Escape As String = "~1ABC 123"
            Dim track2Escape As String = "~2456"
            Dim track3Escape As String = "~3789"

            Using font As New Font("Courier New", 6)
                Using brush As New SolidBrush(Color.CadetBlue)
                    Using pen As New Pen(Color.Red)
                        graphics.DrawString(track1Escape, font, brush, 50, 50)
                        graphics.DrawString(track2Escape, font, brush, 50, 50)
                        graphics.DrawString(track3Escape, font, brush, 50, 50)
                    End Using
                End Using
            End Using
        End Sub

        Private Function GetTopcoatBlockingPrintTicketXml() As String
            Dim topcoatBlockingXml As String = String.Empty

            ' front:
            Select Case _commandLineOptions.topcoatBlockingFront
                Case ""
                    ' use the current driver settings.
                    Exit Select
                Case "custom"
                    ' We will generate topcoat and blocking escapes for the card front
                    ' in this application. Escapes override the PrintTicket settings.
                    Exit Select
                Case "all"
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_All
                    Exit Select

                    ' we need the 'exception' markup for the remaining settings:

                Case "chip"
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_7816
                    Exit Select
                Case "magjis"
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_JIS
                    Exit Select
                Case "mag2"
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_2Track
                    Exit Select
                Case "mag3"
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.FrontTopcoatBlockingPreset_ISO_3Track
                    Exit Select
            End Select

            ' back:
            Select Case _commandLineOptions.topcoatBlockingBack
                Case ""
                    ' use the current driver settings.
                    Exit Select
                Case "custom"
                    ' We will generate topcoat and blocking escapes for the card back
                    ' in this application. Escapes override the PrintTicket settings.
                    Exit Select
                Case "all"
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_All
                    Exit Select

                    ' we need the 'exception' markup for the remaining settings:

                Case "chip"
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_7816
                    Exit Select
                Case "magjis"
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_JIS
                    Exit Select
                Case "mag2"
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_2Track
                    Exit Select
                Case "mag3"
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_Except
                    topcoatBlockingXml += PrintTicketXml.BackTopcoatBlockingPreset_ISO_3Track
                    Exit Select
            End Select

            Return topcoatBlockingXml
        End Function

        Private Sub WriteCustomTopcoatBlockingEscapesFront(graphics As Graphics)
            If _commandLineOptions.topcoatBlockingFront <> "custom" Then
                Return
            End If

            ' a 'topcoat Add' escape will force topcoat OFF for the entire card side.

            ' units are millimeters; landscape basis; top left width height:
            ' a rectangle one inch down; two inches wide; 1 cm high.
            ' units are millimeters; top left width height:
            Dim topCoatAddEsc As String = "~TA%25.4 0 50.8 10;"
            topCoatAddEsc += "40 60 7 7?"
            ' add a square, lower
            Using font As New Font("Courier New", 6)
                Using brush As New SolidBrush(Color.Black)
                    Using pen As New Pen(Color.Black)
                        graphics.DrawString(topCoatAddEsc, font, brush, 10, 10)
                    End Using
                End Using
            End Using

            ' a 'blocking' escape will override the driver settings:
            Dim blockingEsc As String = "~PB% 0 19 3 54;"

            Using font As New Font("Courier New", 6)
                Using brush As New SolidBrush(Color.Black)
                    Using pen As New Pen(Color.Black)
                        graphics.DrawString(blockingEsc, font, brush, 10, 20)
                    End Using
                End Using
            End Using
        End Sub

        Private Sub WriteCustomTopcoatBlockingEscapesBack(graphics As Graphics)
            If _commandLineOptions.topcoatBlockingBack <> "custom" Then
                Return
            End If

            Dim topCoatAddEsc As String = "~TA%25.4 10 50.8 20;"

            Using font As New Font("Courier New", 6)
                Using brush As New SolidBrush(Color.Black)
                    Using pen As New Pen(Color.Black)
                        graphics.DrawString(topCoatAddEsc, font, brush, 10, 10)
                    End Using
                End Using
            End Using

            ' a 'blocking' escape will override the driver settings:
            Dim blockingEsc As String = "~PB% 0 23 3 54;"

            Using font As New Font("Times New Roman", 6)
                Using brush As New SolidBrush(Color.Pink)
                    Using pen As New Pen(Color.SeaShell)
                        graphics.DrawString(blockingEsc, font, brush, 10, 20)
                    End Using
                End Using
            End Using
        End Sub
    End Class
End Namespace
