' /* Copyright (C) TIN WIN AUNG - All Rights Reserved
' * Unauthorized copying of this file, via any medium is strictly prohibited
' * Proprietary and confidential
' * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
' */

Imports System.IO
Imports Newtonsoft.Json

Public Class KeyValueFileProvider
    Public ConfigFile As String

    Dim CfgArray As String()
    Dim CfgList As New Dictionary(Of String, String)
    Public Sub New(ByVal file As String)

        ConfigFile = file
        If Not My.Computer.FileSystem.FileExists(file) Then
            My.Computer.FileSystem.WriteAllText(file, "#this is new" & vbCrLf, False)
        End If
        For Each CfgLine As String In System.IO.File.ReadLines(file, Text.Encoding.UTF8)
            If CfgLine.Trim = "" Then Continue For
            If Not CfgLine.StartsWith("#") Then
                Dim CfgItem As String() = CfgLine.Split("=")
                If CfgItem.Count = 2 Then
                    If Not CfgList.ContainsKey(CfgItem(0).Trim) Then
                        CfgList.Add(CfgItem(0).Trim, CfgItem(1).Trim)
                    End If
                End If
            End If
        Next

    End Sub

    Public Function GetValue(ByVal key As String) As String
        If Not CfgList.ContainsKey(key) Then Return ""
        Dim Arr As String()
        Dim val As String
        If CfgList(key) <> "" Then Arr = CfgList(key).Trim.Split("#") Else Return ""
        If Arr.Count > 0 Then
            val = Arr(0).Trim
        Else
            val = CfgList(key).Trim
        End If
        Return val
    End Function

    Public Function ConvBoolean(ByVal val As String) As Boolean
        If val = "" Then Return False
        If val.ToUpper = "TRUE" Or val = "1" Then Return True
        Return False
    End Function
End Class

Public Class ListFileProvider
    Public ConfigFile As String
    Public ReadOnly LineArray As List(Of String)
    Public Sub New(ByVal file As String)
        ConfigFile = file
        LineArray = New List(Of String)
        If Not My.Computer.FileSystem.FileExists(file) Then
            My.Computer.FileSystem.WriteAllText(file, "#this is new" & vbCrLf, False)
        End If
        For Each CfgLine As String In System.IO.File.ReadLines(file, Text.Encoding.UTF8)
            If CfgLine.Trim = "" Then Continue For
            If Not CfgLine.StartsWith("#") Then
                Dim CfgItem As String() = CfgLine.Split("#")
                If CfgItem.Count = 2 Then
                    LineArray.Add(CfgItem(0).Trim)
                Else
                    LineArray.Add(CfgLine.Trim)
                End If
            End If
        Next
    End Sub

End Class

Public Class AppConfig
    Public FirstTime As Boolean
    Public LastAVDBUpdateTime As DateTime
End Class

Public Class JsonConfig
    Public Config As AppConfig
    Private _file As String
    Public Sub New(ByVal file As String)
        _file = file
        If My.Computer.FileSystem.FileExists(_file) Then
            Dim config_json As String = My.Computer.FileSystem.ReadAllText(_file, Text.Encoding.UTF8)
            Config = JsonConvert.DeserializeObject(Of AppConfig)(config_json)
        Else
            'TODO: We may want to log this issue
        End If
    End Sub
    Public Sub Save()
        My.Computer.FileSystem.WriteAllText(_file, JsonConvert.SerializeObject(Config), False)
    End Sub
End Class