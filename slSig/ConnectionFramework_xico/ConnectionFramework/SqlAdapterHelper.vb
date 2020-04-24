Imports System.Reflection
Imports System.Data
Imports System.Data.SqlClient

Public Class SqlAdapterHelper

    Public Shared Function BeginTransaction(ByVal tableAdapter As Object) As SqlTransaction

        Return BeginTransaction(tableAdapter, IsolationLevel.ReadUncommitted)

    End Function

    Public Shared Function BeginTransaction(ByVal tableAdapter As Object, ByVal isolationLevel As IsolationLevel) As SqlTransaction

        Dim type As Type = tableAdapter.GetType
        Dim connection As SqlConnection = GetConnection(tableAdapter)

        If connection.State = ConnectionState.Closed Then
            connection.Open()
        End If

        Dim transaction As SqlTransaction = connection.BeginTransaction(isolationLevel)

        SetTransaction(tableAdapter, transaction)

        Return transaction

    End Function

    Private Shared Function GetConnection(ByVal tableAdapter As Object) As SqlConnection

        Dim type As Type = tableAdapter.GetType
        Dim connectionProperty As PropertyInfo = type.GetProperty("Connection", BindingFlags.NonPublic Or BindingFlags.Instance)
        Dim connection As SqlConnection = CType(connectionProperty.GetValue(tableAdapter, Nothing), SqlConnection)

        Return connection

    End Function

    Private Shared Sub SetConnection(ByVal tableAdapter As Object, ByVal connection As SqlConnection)

        Dim type As Type = tableAdapter.GetType
        Dim connectionProperty As PropertyInfo = type.GetProperty("Connection", BindingFlags.NonPublic Or BindingFlags.Instance)

        connectionProperty.SetValue(tableAdapter, connection, Nothing)

    End Sub

    Public Shared Sub SetTransaction(ByVal tableAdapter As Object, ByVal transaction As SqlTransaction)

        Dim type As Type = tableAdapter.GetType
        Dim commandsProperty As PropertyInfo = type.GetProperty("CommandCollection", BindingFlags.NonPublic Or BindingFlags.Instance)
        Dim commands As SqlCommand() = CType(commandsProperty.GetValue(tableAdapter, Nothing), SqlCommand())

        For Each command As SqlCommand In commands
            command.Transaction = transaction
        Next

        SetConnection(tableAdapter, transaction.Connection)

    End Sub

End Class
