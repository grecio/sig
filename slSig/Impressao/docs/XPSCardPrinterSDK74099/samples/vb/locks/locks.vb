''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
' Copyright (c) Datacard Corporation.  All Rights Reserved.
'
' XPS Printer SDK: vb.net 'locks' sample
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.IO
Imports dxp01sdk

Public Class CommandLineOptions
    Public change As Boolean
    Public currentPassword As String = String.Empty
    Public dolock As Boolean
    Public newPassword As String = String.Empty
    Public printerName As String = String.Empty
    Public unlock As Boolean
    Public activate As Boolean
    Public deactivate As Boolean
End Class

Class locks

    Private Shared Sub Usage()
        Dim thisExeName As String = Path.GetFileName(Environment.GetCommandLineArgs()(0))
        Console.WriteLine()
        Console.WriteLine(thisExeName & " demonstrates locking, unlocking, changing the lock password, ")
        Console.WriteLine("and activating or deactivating the printer.")
        Console.WriteLine()
        Console.WriteLine(thisExeName & " -n <printername> [-a] [-d] [-l] [-u] [-c -w]")
        Console.WriteLine("  -n <printername>. Required. Try -n ""XPS Card Printer"".")
        Console.WriteLine("  -a <password>. Activate printer using the password.")
        Console.WriteLine("  -d <password>. Deactivate printer using the password.")
        Console.WriteLine("  -l <password>. Lock printer using the password.")
        Console.WriteLine("  -u <password>. Unlock printer using the password.")
        Console.WriteLine("  -c <current password> -w <new password>. Change password.")
        Console.WriteLine("     The current and new passwords are required.")
        Console.WriteLine("     In this sample, changing the password also locks")
        Console.WriteLine("     the printer.")
        Console.WriteLine("     To use a blank password, use an empty string: """"")
        Console.WriteLine("  -a and -d options cannot be used with -l, -u or -c options")
        Console.WriteLine("  -l, -u or -c option only applies to the printers that have lock option installed.")
        Console.WriteLine()
        Console.WriteLine("     password rules:")
        Console.WriteLine("       blank password ("""") is OK;")
        Console.WriteLine("       four or more valid characters;")
        Console.WriteLine("       legal characters:")
        Console.WriteLine("       A through Z; a through z; 0 through 9; '+', '/', and '$'.")
        Console.WriteLine()
        Environment.Exit(-1)
    End Sub

    Private Shared Function GetCommandlineOptions(args As String()) As CommandLineOptions
        Dim commandLineOptions As New CommandLineOptions()
        Dim arguments As New CommandLine.Utility.Arguments(args)

        If Not arguments.ContainsKey("n") Then
            Usage()
        End If

        If arguments("n") = "true" Then
            Usage()
        Else
            ' parser inserts "true" for missing args:
            commandLineOptions.printerName = arguments("n")
        End If

        If arguments.ContainsKey("c") Then
            commandLineOptions.change = True
            If arguments("c") <> "true" Then
                commandLineOptions.currentPassword = arguments("c")
            End If
        End If

        If arguments.ContainsKey("l") Then
            commandLineOptions.dolock = True
            If arguments("l") <> "true" Then
                commandLineOptions.currentPassword = arguments("l")
            End If
        End If

        If arguments.ContainsKey("u") Then
            commandLineOptions.unlock = True
            If arguments("u") <> "true" Then
                commandLineOptions.currentPassword = arguments("u")
            End If
        End If

        If arguments.ContainsKey("w") Then
            If arguments("w") <> "true" Then
                commandLineOptions.newPassword = arguments("w")
            End If
        End If

        If arguments.ContainsKey("a") Then
            commandLineOptions.activate = True
            If arguments("a") <> "true" Then
                commandLineOptions.currentPassword = arguments("a")
            End If
        End If

        If arguments.ContainsKey("d") Then
            commandLineOptions.deactivate = True
            If arguments("d") <> "true" Then
                commandLineOptions.currentPassword = arguments("d")
            End If
        End If

        If commandLineOptions.activate OrElse commandLineOptions.deactivate Then
            If commandLineOptions.dolock OrElse commandLineOptions.unlock OrElse commandLineOptions.change Then
                Usage()
            End If
            If commandLineOptions.activate AndAlso commandLineOptions.deactivate Then
                Usage()
            End If
        Else
            If Not commandLineOptions.dolock AndAlso Not commandLineOptions.unlock AndAlso Not commandLineOptions.change Then
                Usage()
            End If
        End If

        Return commandLineOptions
    End Function

    Private Shared Sub CheckStatus(printerStatusXml As String, fnName As String)
        Dim printerStatus As PrinterStatusValues = Util.ParsePrinterStatusXML(printerStatusXml)
        If printerStatus._errorCode <> 0 Then
            Dim message As String = fnName & " failed. Status XML: " & Environment.NewLine & printerStatusXml & Environment.NewLine
            Throw New Exception(message)
        End If
    End Sub

    Private Shared Sub LockOrUnlock(bidiSpl As BidiSplWrap, options As CommandLineOptions)
        Dim lockValue As Integer = If((options.dolock), 1, 0)
        Dim formattedPrinterLockXML As String = String.Format(strings.LOCK_PRINTER_XML, lockValue, options.currentPassword)

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.LOCK_PRINTER, formattedPrinterLockXML)
        CheckStatus(printerStatusXML, "LockOrUnlock")
    End Sub

    Private Shared Sub ChangeLockPassword(bidiSpl As BidiSplWrap, options As CommandLineOptions)
        ' in this sample, we always lock the printer as part of the password change
        ' operation. (Use zero to unlock the printer.)
        Dim lockValue As Integer = 1

        Dim formattedChangePasswordXml As String = String.Format(strings.CHANGE_LOCK_PASSWORD_XML, lockValue, options.currentPassword, options.newPassword)

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.CHANGE_LOCK_PASSWORD, formattedChangePasswordXml)
        CheckStatus(printerStatusXML, "ChangeLockPassword")
    End Sub

    Private Shared Sub ActivateOrDeactivate(bidiSpl As BidiSplWrap, options As CommandLineOptions)
        Dim activateValue As Integer = If((options.activate), 1, 0)
        Dim formattedActivatePrinterXML As String = String.Format(strings.ACTIVATE_PRINTER_XML, activateValue, options.currentPassword)

        Dim printerStatusXML As String = bidiSpl.SetPrinterData(strings.ACTIVATE_PRINTER, formattedActivatePrinterXML)
        CheckStatus(printerStatusXML, "ActivateOrDeactivate")
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' Main()
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    Public Shared Sub Main(args As String())

        Dim options As CommandLineOptions = GetCommandlineOptions(args)

        Dim bidiSpl As BidiSplWrap = Nothing

        Try
            Console.WriteLine("using '{0}' for current password.", options.currentPassword)
            If options.change Then
                Console.WriteLine("using '{0}' for new password.", options.newPassword)
            End If

            bidiSpl = New BidiSplWrap()
            bidiSpl.BindDevice(options.printerName)

            Dim driverVersionXml As String = bidiSpl.GetPrinterData(strings.SDK_VERSION)
            Console.WriteLine(Environment.NewLine & "driver version: " & Util.ParseDriverVersionXML(driverVersionXml) & Environment.NewLine)

            If options.dolock OrElse options.unlock OrElse options.change Then
                Dim printerOptionsXML As String = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2)
                Dim printerOptionsValues As PrinterOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML)
                If "Installed" <> printerOptionsValues._optionLocks Then
                    Dim message As String = options.printerName + " does not support the locking feature."
                    Throw New Exception(message)
                End If
            End If

            If options.activate OrElse options.deactivate Then
                ActivateOrDeactivate(bidiSpl, options)
                If options.activate Then
                    Console.WriteLine(Environment.NewLine + "activated the printer." + Environment.NewLine)
                Else
                    Console.WriteLine(Environment.NewLine + "deactivated the printer." + Environment.NewLine)
                End If
            ElseIf options.dolock OrElse options.unlock Then
                LockOrUnlock(bidiSpl, options)
                If options.dolock Then
                    Console.WriteLine(Environment.NewLine + "locked the printer." + Environment.NewLine)
                Else
                    Console.WriteLine(Environment.NewLine + "unlocked the printer." + Environment.NewLine)
                End If
            ElseIf options.change Then
                ChangeLockPassword(bidiSpl, options)
                Console.WriteLine(Environment.NewLine + "password changed." + Environment.NewLine)
            End If

        Catch e As Exception
            Console.WriteLine(e.Message)
        Finally
            bidiSpl.UnbindDevice()
        End Try
    End Sub
End Class
