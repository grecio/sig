''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'* Arguments class: application arguments interpreter
'*
'* Authors: R. LOPES
'* Contributors: R. LOPES
'* Created: 25 October 2002
'* Modified: 28 October 2002
'*
'* Version: 1.0
'* 
'* http://www.codeproject.com/KB/recipes/command_line.aspx
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
Imports System.Collections.Specialized
Imports System.Text.RegularExpressions

Namespace CommandLine.Utility
    ''' <summary>
    ''' Arguments class
    ''' </summary>
    Public Class Arguments
        ' Variables
        Private Parameters As StringDictionary

        ' Constructor
        Public Sub New(ByVal Args As String())
            Parameters = New StringDictionary()
            Dim Spliter As New Regex("^-{1,2}|^/|=", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim Remover As New Regex("^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase Or RegexOptions.Compiled)
            Dim Parameter As String = Nothing
            Dim Parts As String()

            ' Valid parameters forms:
            ' {-,/,--}param{ ,=,:}((",')value(",'))
            ' Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
            For Each Txt As String In Args
                ' Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3)
                Select Case Parts.Length
                    ' Found a value (for the last parameter found (space separator))
                    Case 1
                        If Parameter IsNot Nothing Then
                            If Not Parameters.ContainsKey(Parameter) Then
                                Parts(0) = Remover.Replace(Parts(0), "$1")
                                Parameters.Add(Parameter, Parts(0))
                            End If
                            Parameter = Nothing
                        End If
                        ' else Error: no parameter waiting for a value (skipped)
                        Exit Select
                        ' Found just a parameter
                    Case 2
                        ' The last parameter is still waiting. With no value, set it to true.
                        If Parameter IsNot Nothing Then
                            If Not Parameters.ContainsKey(Parameter) Then
                                Parameters.Add(Parameter, "true")
                            End If
                        End If
                        Parameter = Parts(1)
                        Exit Select
                        ' Parameter with enclosed value
                    Case 3
                        ' The last parameter is still waiting. With no value, set it to true.
                        If Parameter IsNot Nothing Then
                            If Not Parameters.ContainsKey(Parameter) Then
                                Parameters.Add(Parameter, "true")
                            End If
                        End If
                        Parameter = Parts(1)
                        ' Remove possible enclosing characters (",')
                        If Not Parameters.ContainsKey(Parameter) Then
                            Parts(2) = Remover.Replace(Parts(2), "$1")
                            Parameters.Add(Parameter, Parts(2))
                        End If
                        Parameter = Nothing
                        Exit Select
                End Select
            Next
            ' In case a parameter is still waiting
            If Parameter IsNot Nothing Then
                If Not Parameters.ContainsKey(Parameter) Then
                    Parameters.Add(Parameter, "true")
                End If
            End If
        End Sub

        ' Retrieve a parameter value if it exists
        Default Public ReadOnly Property Item(ByVal Param As String) As String
            Get
                Return Parameters(Param)
            End Get
        End Property

        Public Function GetParameterCount() As Integer
            Return Parameters.Count
        End Function

        Public Function ContainsKey(key As String) As Boolean
            Return Parameters.ContainsKey(key)
        End Function

    End Class
End Namespace
