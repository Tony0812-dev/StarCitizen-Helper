﻿Option Strict On
Option Explicit On
'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports Microsoft.VisualBasic.ApplicationServices

Namespace My

    'NOTE: This file is auto-generated; do not modify it directly.  To make changes,
    ' or if you encounter build errors in this file, go to the Project Designer
    ' (go to Project Properties or double-click the My Project node in
    ' Solution Explorer), and make changes on the Application tab.
    '
    Partial Friend Class MyApplication

        <Global.System.Diagnostics.DebuggerStepThroughAttribute()>
        Public Sub New()
            MyBase.New(Global.Microsoft.VisualBasic.ApplicationServices.AuthenticationMode.Windows)
            Me.IsSingleInstance = True
            Me.EnableVisualStyles = True
            Me.SaveMySettingsOnExit = True
            Me.ShutdownStyle = Global.Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses
        End Sub

        <Global.System.Diagnostics.DebuggerStepThroughAttribute()>
        Protected Overrides Sub OnCreateMainForm()
            On Error GoTo fin
            Initialization = True
            InitializeStart()
            MAIN_THREAD = Global.SC.MainForm
            Me.MainForm = MAIN_THREAD
            InitializeEnd()
            Initialization = False
fin:        Exit Sub
        End Sub

        Private Sub MyApplication_Shutdown(sender As Object, e As System.EventArgs) Handles Me.Shutdown
            On Error GoTo fin
            Unload()
fin:        Exit Sub
        End Sub
    End Class
End Namespace