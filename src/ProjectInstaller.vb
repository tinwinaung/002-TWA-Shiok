Imports System.ComponentModel
Imports System.Configuration.Install

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent

    End Sub

    Private Sub AVServiceInstaller_AfterInstall(sender As Object, e As InstallEventArgs) Handles ShiokInstaller.AfterInstall
        'NOTE:TIN:The following code starts the services after it is installed.
        Using serviceController As New System.ServiceProcess.ServiceController(ShiokInstaller.ServiceName)
            serviceController.Start()
        End Using
    End Sub
End Class
