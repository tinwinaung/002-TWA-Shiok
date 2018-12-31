' /* Copyright (C) TIN WIN AUNG - All Rights Reserved
' * Unauthorized copying of this file, via any medium is strictly prohibited
' * Proprietary and confidential
' * Written by TIN WIN AUNG <tinwinaung@sayargyi.com>, Dec 2018
' */

Imports System.Threading
Imports System.IO
Imports System.Text
Imports System.Net.Mail

Public Class ShiokService

    Dim logger As NLog.Logger

    Dim FreshProcInfo As New ProcessStartInfo
    Dim FreshProc As New Process

    Dim ClamdProcInfo As ProcessStartInfo
    Dim ClamdProc As Process

    Dim FreshTimer As System.Timers.Timer
    Dim FreshSchedule As DateTime
    Dim WaitForFresh As Object = New Object 'Dummy object to hold lock if freshscan is running

    Dim watchfolder As FileSystemWatcher
    Dim LockToWaitDBupdate As New Object

    Protected Overrides Sub OnStart(ByVal args() As String)
        'System.Diagnostics.Debugger.Launch()

        Try
            InitEnv()
            InitAVService()
            If ShiokConf.GetValue("ClamdConfigFilePath") = "" Then BuidlClamDConfig()
            If ShiokConf.GetValue("FreshClamConfigFilePath") = "" Then BuidlFreshClamConfig()
            InitLog()
            InitScheduleFresh()
            logger.Info("FreeAV Service is Started")
            ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunAVdbUpdate), Nothing) 'Update For the first time
            If Not AppConf.Config.FirstTime Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunCalmd), Nothing)
            InitWatch()
        Catch ex As Exception
            logger.Fatal(ex.Message)
        End Try

    End Sub

    Protected Overrides Sub OnStop()
        Try
            ClamdProc.Kill() ' Close Clamd Service
            System.Threading.Thread.Sleep(5000)
            logger.Info("FreeAV Service is Stopped")
        Catch ex As Exception
            logger.Fatal(ex.Message)
        End Try

    End Sub

#Region "INIT"
    Private Sub InitLog()
        Dim NLogConfig As NLog.Config.LoggingConfiguration
        Dim NLogFile As NLog.Targets.FileTarget
        With My.Computer.FileSystem
            If Not .DirectoryExists(.CombinePath(CurrentDir, LogFolder)) Then
                .CreateDirectory(.CombinePath(CurrentDir, LogFolder))
            End If
            NLogConfig = New NLog.Config.LoggingConfiguration()
            NLogFile = New NLog.Targets.FileTarget("NLogFile") With {
            .FileName = My.Computer.FileSystem.CombinePath(My.Computer.FileSystem.CombinePath(CurrentDir, LogFolder), FreeAvLogFile),
            .Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
            .ArchiveFileName = My.Computer.FileSystem.CombinePath(My.Computer.FileSystem.CombinePath(CurrentDir, LogFolder), FreeAvArchiveLogFile),
            .ArchiveAboveSize = "2048000",
            .ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Sequence,
            .ConcurrentWrites = "true",
            .KeepFileOpen = "false"
            }
            NLogConfig.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, NLogFile)
        End With

        NLog.LogManager.Configuration = NLogConfig
        logger = NLog.LogManager.GetCurrentClassLogger()
    End Sub
    Private Sub InitAVService()

        'If there Is no contents in database, we consider first time (whatever)
        If My.Computer.FileSystem.GetFiles(My.Computer.FileSystem.CombinePath(ClamAVFolder, "database")).Count = 0 Then
            AppConf.Config.FirstTime = True
            AppConf.Save()
        End If

        If Not My.Computer.FileSystem.DirectoryExists(My.Computer.FileSystem.CombinePath(CurrentDir, TmpFolder)) Then
            My.Computer.FileSystem.CreateDirectory(My.Computer.FileSystem.CombinePath(CurrentDir, TmpFolder))
        End If
        If Not My.Computer.FileSystem.DirectoryExists(My.Computer.FileSystem.CombinePath(CurrentDir, VirusFolder)) Then
            My.Computer.FileSystem.CreateDirectory(My.Computer.FileSystem.CombinePath(CurrentDir, VirusFolder))
        End If
        'TODO: This is not require since we install calmav in installation process
        If Not My.Computer.FileSystem.DirectoryExists(My.Computer.FileSystem.CombinePath(CurrentDir, ClamFolder)) Then
            My.Computer.FileSystem.CreateDirectory(My.Computer.FileSystem.CombinePath(CurrentDir, ClamFolder))
            'TODO:Download CLam
        Else

        End If
    End Sub
    Private Sub InitScheduleFresh()
        Try
            FreshTimer = New System.Timers.Timer()
            Dim interval As Integer
            If (Integer.TryParse(ShiokConf.GetValue("AVdbUpdateTime"), interval)) Then
            Else
                interval = 3
            End If
            FreshSchedule = DateTime.Today.AddDays(1).AddHours(interval)
            'For first time, set amount of seconds between current time And schedule time
            FreshTimer.Enabled = True
            FreshTimer.Interval = FreshSchedule.Subtract(DateTime.Now).TotalSeconds * 1000
            AddHandler FreshTimer.Elapsed, New System.Timers.ElapsedEventHandler(AddressOf FreshTimer_Elapsed)
        Catch ex As Exception
            logger.Fatal(ex.Message)
        End Try
    End Sub
    Private Sub InitWatch(Optional watch As Boolean = True)
        'Start Watching Folders
        For Each i In ArrWatchFolders.LineArray
            If IO.Directory.Exists(i) Then
                logger.Info("FreeAV start monitoring folder : " & i)
                If watch Then WatchFileSys(i)
                If ShiokConf.ConvBoolean(ShiokConf.GetValue("ScanFolderWhenStart")) AndAlso (Not AppConf.Config.FirstTime) Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunClamdScanDelay), i)
            Else
                logger.Info("FreeAV could not find folder : " & i)
            End If
            If Not ShiokConf.ConvBoolean(ShiokConf.GetValue("ScanFolderWhenStart")) Then logger.Info("Please take note that full folder scan is disabled in configuration file.")
        Next
    End Sub

#End Region

#Region "Implementations"
    Protected Sub FreshTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)
        'Update FreshDB
        ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunAVdbUpdate), Nothing) 'Update
        'Reset Timer
        If FreshTimer.Interval <> 24 * 60 * 60 * 1000 Then
            FreshTimer.Interval = 24 * 60 * 60 * 1000
            logger.Info("New Schedule is started to update AV DB")
        End If
    End Sub
#End Region

#Region "Run"
    Private Sub RunAVdbUpdate(ByVal arg As Object)
        Try
            If Not ShiokConf.ConvBoolean(ShiokConf.GetValue("FreshClamEnable")) Then
                logger.Info("Virus signature DB update is skip in the configuration file (Shiok.txt).")
                logger.Fatal("Please make sure Virus signature DB is download and updated. Otherwise scanning will fail.")
                AppConf.Config.FirstTime = False
                AppConf.Save()
                Exit Sub
            Else
                If My.Computer.FileSystem.GetFiles(My.Computer.FileSystem.CombinePath(ClamAVFolder, "database")).Count = 0 Then
                    AppConf.Config.FirstTime = True
                    AppConf.Save()
                End If
            End If
            FreshProcInfo = New ProcessStartInfo
            FreshProc = New Process

            With My.Computer.FileSystem
                FreshProcInfo.FileName = freshclamBinFilePath
                FreshProcInfo.Arguments = FreshClamArg
                FreshProcInfo.CreateNoWindow = True
                FreshProcInfo.UseShellExecute = False
                FreshProcInfo.WindowStyle = ProcessWindowStyle.Hidden
                FreshProcInfo.RedirectStandardOutput = False
                FreshProcInfo.RedirectStandardError = False
            End With
            SyncLock LockToWaitDBupdate
                FreshProc = Process.Start(FreshProcInfo)
                logger.Info("AV DB Update is Started")

                FreshProc.WaitForExit()
            End SyncLock
            If (FreshProc.ExitCode = 0) Then
                logger.Info("AV DB Update is Finished with Exit code : " + FreshProc.ExitCode.ToString())
                logger.Info("Existing AV DB is latest.")

                If AppConf.Config.FirstTime Then
                    'If the application is running for first time
                    'we hold folder scan because of new database update
                    'Now is the time to scan and switch first time key to false
                    AppConf.Config.FirstTime = False
                    ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunCalmd), Nothing)
                    InitWatch(False)
                End If
            ElseIf (FreshProc.ExitCode = 52) Then
                logger.Fatal("AV DB Update is Stopped with Exit code : " + FreshProc.ExitCode.ToString())
                logger.Fatal("Can not connect to download virus signature database.")
            ElseIf (FreshProc.ExitCode <> 0) Then
                logger.Fatal("AV DB Update is Stopped with Exit code : " + FreshProc.ExitCode.ToString())
                logger.Fatal("AV DB is not updated")
            Else
                logger.Info("AV DB Update is Stopped with Exit code : " + FreshProc.ExitCode.ToString())
                logger.Info("AV DB is updated with new DB")
            End If

            'Update last update time and make sure save the settings
            AppConf.Config.LastAVDBUpdateTime = Now()
            AppConf.Save()
        Catch ex As Exception
            logger.Fatal("AV DB Update Error : " & ex.Message)
        End Try
    End Sub
    Private Sub RunCalmd(ByVal arg As Object)

        ClamdProcInfo = New ProcessStartInfo
        ClamdProc = New Process
        Try
            With My.Computer.FileSystem
                ClamdProcInfo.FileName = ClamdBinFilePath
                ClamdProcInfo.Arguments = ClamdArg
                ClamdProcInfo.CreateNoWindow = True
                ClamdProcInfo.UseShellExecute = False
                ClamdProcInfo.WindowStyle = ProcessWindowStyle.Hidden
                ClamdProcInfo.RedirectStandardOutput = False
                ClamdProcInfo.RedirectStandardError = False
            End With

            ClamdProc = Process.Start(ClamdProcInfo)
            logger.Info("AV Service is Started")

            ClamdProc.WaitForExit()

            If (ClamdProc.ExitCode <> 0) Then
                logger.Fatal("AV Service is Stopped with Exit code : " + ClamdProc.ExitCode.ToString())
            Else
                logger.Info("AV Service is Stopped with Exit code : " + ClamdProc.ExitCode.ToString())
            End If
        Catch ex As Exception
            logger.Fatal("AV Service Error : " & ex.Message)
        End Try
    End Sub

    Private Sub RunClamdScanDelay(ByVal arg As Object)
        System.Threading.Thread.Sleep(15000)
        RunClamdScan(arg)
    End Sub
    Private Sub RunClamdScan(ByVal arg As Object)
        Dim ClamdScanProcInfo As New ProcessStartInfo
        Dim ClamdScanProc As New Process

        'SyncLock clamdScanObj
        ClamdScanProcInfo = New ProcessStartInfo
        ClamdScanProc = New Process
        Dim _path As String = CType(arg, String)

        Try
                If Not My.Computer.FileSystem.FileExists(_path) Then
                    If Not My.Computer.FileSystem.DirectoryExists(_path) Then
                        GoTo NotToScan
                    End If
                End If
                ClamdScanProcInfo.FileName = ClamdScanBinFilePath
                ClamdScanProcInfo.Arguments = ClamdScanArg & " """ & _path & """"
                ClamdScanProcInfo.CreateNoWindow = True
                ClamdScanProcInfo.UseShellExecute = False
                ClamdScanProcInfo.WindowStyle = ProcessWindowStyle.Hidden
                ClamdScanProcInfo.RedirectStandardOutput = True
                ClamdScanProcInfo.RedirectStandardError = True
            SyncLock LockToWaitDBupdate
                ClamdScanProc = Process.Start(ClamdScanProcInfo)
                logger.Info("AV Scan is Started for " & _path)

                ClamdScanProc.WaitForExit()
            End SyncLock


            'TODO:Not only here but to check each exit codes in processes
            If (ClamdScanProc.ExitCode = 0) Then
                logger.Info("AV Scan is Stopped with Exit code : " + ClamdScanProc.ExitCode.ToString())
                logger.Info("AV Scan did not found virus on " & _path)
            ElseIf (ClamdScanProc.ExitCode = 1) Then
                'TODO: Allow to run
                logger.Info("AV Scan is Stopped with Exit code : " + ClamdScanProc.ExitCode.ToString())
                logger.Info("AV Scan found virus on " & _path)

            ElseIf (ClamdScanProc.ExitCode = 2) Then
                'TODO: Allow to run
                logger.Info("AV Scan is Stopped with Exit code : " + ClamdScanProc.ExitCode.ToString())
                logger.Info("AV Scan error " & _path)
                logger.Info("AV Scan error. " & "Please check your Arg config.")

            ElseIf (ClamdScanProc.ExitCode <> 0) Then
                logger.Fatal("AV Scan is Stopped with Exit code : " + ClamdScanProc.ExitCode.ToString())
            Else
                logger.Info("AV Scan is Stopped with Exit code : " + ClamdScanProc.ExitCode.ToString())
            End If
            Notify(_path, ClamdScanProc.ExitCode)
NotToScan:

            Catch ex As Exception
                logger.Fatal("AV Scan Error : " & ex.Message)
            End Try

        'End SyncLock
    End Sub
#End Region

#Region "FileSystemWatch"
    Private Sub WatchFileSys(ByVal path As String)
        watchfolder = New System.IO.FileSystemWatcher()
        watchfolder.Path = path
        watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
        watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                                   IO.NotifyFilters.FileName
        watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                                   IO.NotifyFilters.Attributes
        watchfolder.Filter = "*.*"
        watchfolder.IncludeSubdirectories = True

        ' add the handler to each event
        AddHandler watchfolder.Changed, AddressOf evFileSysChange
        AddHandler watchfolder.Created, AddressOf evFileSysChange
        AddHandler watchfolder.Deleted, AddressOf evFileSysChange

        ' add the rename handler as the signature is different
        AddHandler watchfolder.Renamed, AddressOf evFileSysReName

        'Set this property to true to start watching
        watchfolder.EnableRaisingEvents = True
    End Sub

    Private Sub evFileSysChange(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
        If e.ChangeType = IO.WatcherChangeTypes.Changed Then
            'txt_folderactivity.Text &= "File " & e.FullPath & " has been modified" & vbCrLf
            If (Not AppConf.Config.FirstTime) Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunClamdScan), e.FullPath)
        End If
            If e.ChangeType = IO.WatcherChangeTypes.Created Then
            'txt_folderactivity.Text &= "File " & e.FullPath & " has been created" & vbCrLf
            If (Not AppConf.Config.FirstTime) Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunClamdScan), e.FullPath)
        End If
            If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
            ' txt_folderactivity.Text &= "File " & e.FullPath & " has been deleted" & vbCrLf
            If (Not AppConf.Config.FirstTime) Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunClamdScan), e.FullPath)
        End If
    End Sub
    Public Sub evFileSysReName(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
        'stxt_folderactivity.Text &= "File" & e.OldName & " has been renamed to " & e.Name & vbCrLf
        If (Not AppConf.Config.FirstTime) Then ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf RunClamdScan), e.FullPath)
    End Sub
#End Region

#Region "ClamConfigs"
    Private Sub BuidlClamDConfig()
        Dim FileName As String = My.Computer.FileSystem.CombinePath(CurrentDir, ClamdCfg) & ".ref"
        Dim CfgCont As String = My.Computer.FileSystem.ReadAllText(FileName, Encoding.ASCII)
        'CfgCont = CfgCont.Replace(vbLf, vbCrLf)
        CfgCont = CfgCont.Replace("{{ClamFolder}}", ClamFolder)
        CfgCont = CfgCont.Replace("{{LogFolder}}", LogFolder)
        CfgCont = CfgCont.Replace("{{TmpFolder}}", TmpFolder)
        CfgCont = CfgCont.Replace("{{VirusFolder}}", VirusFolder)
        CfgCont = CfgCont.Replace("{{CurrentDir}}\", CurrentDir) ' Please take note CurrentDir always have "\" at the end
        CfgCont = CfgCont.Replace("{{ClamdLogFile}}", ClamdLogFile)
        CfgCont = CfgCont.Replace("{{LogFile}}", ClamdLogFilePath)
        FileName = My.Computer.FileSystem.CombinePath(CurrentDir, ClamFolder)
        FileName = My.Computer.FileSystem.CombinePath(FileName, ClamdCfg)
        My.Computer.FileSystem.WriteAllText(FileName, CfgCont, False, Encoding.ASCII)
    End Sub

    Private Sub BuidlFreshClamConfig()
        Dim FileName As String = My.Computer.FileSystem.CombinePath(CurrentDir, FreshclamCfg) & ".ref"
        Dim CfgCont As String = My.Computer.FileSystem.ReadAllText(FileName, Encoding.ASCII)
        'CfgCont = CfgCont.Replace(vbLf, vbCrLf)
        CfgCont = CfgCont.Replace("{{ClamFolder}}", ClamFolder)
        CfgCont = CfgCont.Replace("{{LogFolder}}", LogFolder)
        CfgCont = CfgCont.Replace("{{TmpFolder}}", TmpFolder)
        CfgCont = CfgCont.Replace("{{VirusFolder}}", VirusFolder)
        CfgCont = CfgCont.Replace("{{CurrentDir}}\", CurrentDir) ' Please take note CurrentDir always have "\" at the end
        CfgCont = CfgCont.Replace("{{FreshClamLogFile}}", FreshClamLogFile)
        CfgCont = CfgCont.Replace("{{UpdateLogFile}}", freshclamLogFilePath)
        If ShiokConf.GetValue("ProxyServer") <> "" Then
            'Dim strTmp As New StringBuilder
            'strTmp.Append("#{{ProxyServerConfig}}").AppendLine()
            'strTmp.Append("#{{ProxyPortConfig}}").AppendLine()
            'strTmp.Append("#{{ProxyUserConfig}}").AppendLine()
            'strTmp.Append("#{{ProxyPwdConfig}}").AppendLine()

            CfgCont = CfgCont.Replace("#{{ProxyServerConfig}}", "HTTPProxyServer " & ShiokConf.GetValue("ProxyServer"))
            CfgCont = CfgCont.Replace("#{{ProxyPortConfig}}", "HTTPProxyPort " & ShiokConf.GetValue("ProxyPort"))
            CfgCont = CfgCont.Replace("#{{ProxyUserConfig}}", "HTTPProxyUsername " & ShiokConf.GetValue("ProxyUser"))
            CfgCont = CfgCont.Replace("#{{ProxyPwdConfig}}", "HTTPProxyPassword" & ShiokConf.GetValue("ProxyPwd"))
            'CfgCont = CfgCont.Replace("#{{ProxyPwdConfig}}", "HTTPProxyPassword" & My.Settings.ProxyPwd & vbCrLf & strTmp.ToString())
        Else

        End If
        FileName = My.Computer.FileSystem.CombinePath(CurrentDir, ClamFolder)
        FileName = My.Computer.FileSystem.CombinePath(FileName, FreshclamCfg)
        My.Computer.FileSystem.WriteAllText(FileName, CfgCont, False, Encoding.ASCII)
    End Sub


    Private Sub Notify(ByVal filename As String, ByVal ScanEvent As Integer)
        If Not ShiokConf.ConvBoolean(ShiokConf.GetValue("EnableEmailNoti")) Then
            Exit Sub
        End If

        Dim Smtp_Server As New SmtpClient
        Dim e_mail As New MailMessage()
        Try
            Smtp_Server.UseDefaultCredentials = False
            If ShiokConf.GetValue("SMTPUser") <> "" Then
                Smtp_Server.Credentials = New Net.NetworkCredential(ShiokConf.GetValue("SMTPUser"), ShiokConf.GetValue("SMTPPwd"))
            End If
            Dim _port As Integer
            If (Integer.TryParse(ShiokConf.GetValue("AVdbUpdateTime"), _port)) Then
            Else
                _port = 25
            End If
            Smtp_Server.Port = _port
            Smtp_Server.EnableSsl = ShiokConf.ConvBoolean(ShiokConf.GetValue("ScanFolderWhenStart"))
            If ShiokConf.GetValue("SMTPServer") = "" Then
                logger.Fatal("Invalid SMTP Server while trying to notify virus event.")
                Exit Sub
            End If
            Smtp_Server.Host = ShiokConf.GetValue("SMTPServer")

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(ShiokConf.GetValue("From"))


            Dim To_email As String() = Split(ShiokConf.GetValue("to"), ",")
            For Each i In To_email
                e_mail.To.Add(i)
            Next
            Dim EventName As String = ""
            If ScanEvent = 1 Then EventName = "Virus Found" Else EventName = "Virus Scan Error"
            Dim bodyMsg As New StringBuilder
            bodyMsg.Append("Hi There").AppendLine().AppendLine()
            If ScanEvent = 1 Then bodyMsg.Append("We found a virus on the following File/Folder").AppendLine() Else bodyMsg.Append("Virus scan have an issue while scanning the following File/Folder").AppendLine()
            bodyMsg.Append(filename).AppendLine().AppendLine()
            bodyMsg.Append("Date : ").Append(Now).AppendLine()

            e_mail.Subject = ShiokConf.GetValue("Subject").Replace("{{EVENT}}", EventName)
            e_mail.IsBodyHtml = False
            e_mail.Body = ShiokConf.GetValue("Body").Replace("{{MSG}}", bodyMsg.ToString)
            Smtp_Server.Send(e_mail)
        Catch ex As Exception
            logger.Fatal("AV Scan Error : " & ex.Message)
        End Try

    End Sub


#End Region

End Class
