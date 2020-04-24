Imports System.Configuration
Imports DAL

Public MustInherit Class ConnectionBase

#Region "Atributos"

    Private _cnn As SqlClient.SqlConnection

#End Region

#Region "Propriedades"

#Region "Connections"

    Public ReadOnly Property DBConnection() As SqlClient.SqlConnection

        Get

            If IsNothing(_cnn) Then

                _cnn = New SqlClient.SqlConnection(DBConnectionString)

            End If

            Return _cnn

        End Get

    End Property

#End Region

#Region "Strings"

    Private ReadOnly Property DBConnectionString() As String

        Get

            Return DAL.ConfigConnection.GetConnection

        End Get

    End Property

#End Region

#End Region


End Class
