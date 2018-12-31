<System.ComponentModel.RunInstaller(True)> Partial Class ProjectInstaller
    Inherits System.Configuration.Install.Installer

    'Installer overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.ShiokProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
        Me.ShiokInstaller = New System.ServiceProcess.ServiceInstaller()
        '
        'ShiokProcessInstaller
        '
        Me.ShiokProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        Me.ShiokProcessInstaller.Password = Nothing
        Me.ShiokProcessInstaller.Username = Nothing
        '
        'ShiokInstaller
        '
        Me.ShiokInstaller.Description = "Shiok by Tin (c) 2018"
        Me.ShiokInstaller.DisplayName = "Shiok Antivirus"
        Me.ShiokInstaller.ServiceName = "Shiok"
        Me.ShiokInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic
        '
        'ProjectInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.ShiokProcessInstaller, Me.ShiokInstaller})

    End Sub

    Friend WithEvents ShiokProcessInstaller As ServiceProcess.ServiceProcessInstaller
    Friend WithEvents ShiokInstaller As ServiceProcess.ServiceInstaller
End Class
