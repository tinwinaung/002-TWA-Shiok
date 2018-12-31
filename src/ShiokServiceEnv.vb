' /* Copyright (C) TIN WIN AUNG - All Rights Reserved
' * Unauthorized copying of this file, via any medium is strictly prohibited
' * Proprietary and confidential
' * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
' */
Partial Public Class ShiokService
#Region "Default Values"
    Const ClamFolder = "clamav"

    Const ClamdBin = "clamd.exe"
    Const ClamdCfg = "clamd.conf"
    Const ClamdLogFile = "clamd.log"

    Const FreshClamBin = "freshclam.exe"
    Const FreshclamCfg = "freshclam.conf"
    Const FreshClamLogFile = "freshclam.log"

    Const FreeAvLogFile = "shiok.log"
    Const FreeAvArchiveLogFile = "shiok.{#####}.log"

    Const ClamdScanBin = "clamdscan.exe"
    Const ClamdScanLogFile = "clamdscan.log"

    Const LogFolder = "log"
    Const TmpFolder = "temp"
    Const ConfFolder = "etc"
    Const VirusFolder = "virus"
#End Region

#Region "Config Values"
    Dim CurrentDir = System.AppDomain.CurrentDomain.BaseDirectory
    Dim ClamAVFolder As String = My.Computer.FileSystem.CombinePath(CurrentDir, ClamFolder)
    Dim ClamdBinFilePath As String
    Dim ClamdConfigFilePath As String
    Dim ClamdArg As String
    Dim ClamdLogFilePath As String
    Dim freshclamBinFilePath As String
    Dim freshclamConfigFilePath As String
    Dim freshclamLogFilePath As String
    Dim FreshClamArg As String
    Dim ClamdScanBinFilePath As String
    Dim ClamdScanArg As String
    Dim ClamdScanLogFilePath As String
    Dim VirusJailFolder As String
    Dim ShiokConf As New KeyValueFileProvider(My.Computer.FileSystem.CombinePath(CurrentDir, ConfFolder & "\Shiok.txt"))
    Dim ArrWatchFolders As New ListFileProvider(My.Computer.FileSystem.CombinePath(CurrentDir, ConfFolder & "\ShiokWatchFolders.txt"))
    Dim AppConf As New JsonConfig(My.Computer.FileSystem.CombinePath(CurrentDir, ConfFolder & "\Application.txt"))
#End Region

    Private Sub InitEnv()
        'TODO:Check FileExist
        With ShiokConf
            If .GetValue("ClamdBinFilePath") = "" Then ClamdBinFilePath = My.Computer.FileSystem.CombinePath(ClamAVFolder, ClamdBin) Else ClamdBinFilePath = .GetValue("ClamdBinFilePath")
            If .GetValue("ClamdConfigFilePath") = "" Then ClamdConfigFilePath = My.Computer.FileSystem.CombinePath(ClamAVFolder, ClamdCfg) Else ClamdConfigFilePath = .GetValue("ClamdConfigFilePath")
            If .GetValue("ClamdArg") = "" Then ClamdArg = "-c """ & ClamdConfigFilePath & """" Else ClamdArg = .GetValue("ClamdArg")
            If .GetValue("ClamdLogFilePath") = "" Then ClamdLogFilePath = My.Computer.FileSystem.CombinePath(My.Computer.FileSystem.CombinePath(CurrentDir, LogFolder), ClamdLogFile) Else ClamdLogFilePath = .GetValue("ClamdLogFilePath")
            'Process freshClam Parameters
            If .GetValue("FreshClamBinFilePath") = "" Then freshclamBinFilePath = My.Computer.FileSystem.CombinePath(ClamAVFolder, FreshClamBin) Else freshclamBinFilePath = .GetValue("FreshClamBinFilePath")
            If .GetValue("FreshClamConfigFilePath") = "" Then freshclamConfigFilePath = My.Computer.FileSystem.CombinePath(ClamAVFolder, FreshclamCfg) Else freshclamConfigFilePath = .GetValue("FreshClamConfigFilePath")
            If .GetValue("FreshclamLogFilePath") = "" Then freshclamLogFilePath = My.Computer.FileSystem.CombinePath(My.Computer.FileSystem.CombinePath(CurrentDir, LogFolder), FreshClamLogFile) Else freshclamLogFilePath = .GetValue("FreshclamLogFilePath")
            If .GetValue("FreshClamArg") = "" Then FreshClamArg = "--config-file """ & freshclamConfigFilePath & """" Else FreshClamArg = .GetValue("FreshClamArg")
            'Process ClamdScan Parameters
            If .GetValue("ClamdScanBinFilePath") = "" Then ClamdScanBinFilePath = My.Computer.FileSystem.CombinePath(ClamAVFolder, ClamdScanBin) Else ClamdScanBinFilePath = .GetValue("ClamdScanBinFilePath")
            If .GetValue("ClamdScanLogFilePath") = "" Then ClamdScanLogFilePath = My.Computer.FileSystem.CombinePath(My.Computer.FileSystem.CombinePath(CurrentDir, LogFolder), ClamdScanLogFile) Else ClamdScanLogFilePath = .GetValue("ClamdScanLogFilePath")
            If .GetValue("VirusJailFolder") = "" Then VirusJailFolder = My.Computer.FileSystem.CombinePath(CurrentDir, VirusFolder) Else VirusJailFolder = .GetValue("VirusJailFolder")
            Dim ArgMoveVirus As String = ""
            Dim ArgMultiScan As String = ""
            If .ConvBoolean(.GetValue("MoveVirusFile")) Then
                ArgMoveVirus = """ --move=""" & My.Computer.FileSystem.CombinePath(CurrentDir, VirusJailFolder) & """"
            Else
                'RSVP
            End If
            If .ConvBoolean(.GetValue("ClamdScanMT")) Then
                ArgMultiScan = " -m" 'No space because this is the first option
            Else
                'RSVP
            End If
            If .GetValue("ClamdScanArg") = "" Then ClamdScanArg = ArgMultiScan & " --no-summary -i -l """ & ClamdScanLogFilePath & """ --config-file=""" & ClamdConfigFilePath & ArgMoveVirus Else ClamdScanArg = .GetValue("ClamdScanArg")

        End With
    End Sub
End Class
