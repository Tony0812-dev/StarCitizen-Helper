﻿Imports System.IO
Imports System.Net
Imports System.Threading

Public Class WL_Update
    Public Event _Event_ListGit_List_Change_Before()
    Public Event _Event_ListGit_List_Change_After()
    Public Event _Event_ListGit_Selection_Change_Before()
    Public Event _Event_ListGit_Selection_Change_After()
    Public Event _Event_InstallFull_Button_Click_Before()
    Public Event _Event_InstallFull_Button_Click_After()
    Public Event _Event_Download_Click_Before()
    Public Event _Event_Download_Click_After()

    Public Event _Event_Controls_Enabled_Before(Enabled As Boolean)
    Public Event _Event_Controls_Enabled_After(Enabled As Boolean)

    Public Event _Event_Download_Before()
    Public Event _Event_Download_After(DownloadFrom As String, DownloadTo As String, e As WL_Download.DownloadProgressElement)

    Public Headers As New WebHeaderCollection

    Private sPackInGameVersion As String = Nothing
    Private sPackInPackVersion As String = Nothing

    Private cBackColor As Color = Me.BackColor
    Private cForeColor As Color = Me.ForeColor

    Private iUpdateGitList_Interval As Integer = 900000
    Private bUpdateGitList_Enable As Boolean = False
    Private sGitList_Value As String = Nothing

    Private sPath_Folder_Download As String = Nothing
    Private sName_File_Download As String = Nothing
    Private sPath_File_Download As String = Nothing

    Private sPath_Folder_Meta As String = Nothing
    Private sName_File_Meta As String = Nothing
    Private sPath_File_Meta As String = Nothing

    Private hashGitList As String = Nothing
    Private hashCurrentList As String = Nothing

    '<----------------------------------- Basic control
    Public Sub New()
        InitializeComponent()
        Me.Property_GitList_AutoUpdate = True
    End Sub
    '-----------------------------------> Basic control

    '<----------------------------------- Properties
    Private Sub WL_Update_BackColorChanged(sender As Object, e As EventArgs) Handles Me.BackColorChanged
        On Error Resume Next
        Me.cBackColor = Me.BackColor
        For Each Elem In Me.Controls
            Elem.BackColor = Me.cBackColor
        Next

    End Sub

    Private Sub WL_Update_ForeColorChanged(sender As Object, e As EventArgs) Handles Me.ForeColorChanged
        On Error Resume Next
        Me.cForeColor = Me.ForeColor
        For Each Elem In Me.Controls
            Elem.ForeColor = Me.cForeColor
        Next
    End Sub

    Public Property Text_Label_Bottom() As String
        Get
            Return Me.Label_TextBottom.Text
        End Get
        Set(ByVal Value As String)
            Me.Label_TextBottom.Text = Value
        End Set
    End Property

    Public Property Text_Label_Download() As String
        Get
            Return Me.Label_Download.Text
        End Get
        Set(ByVal Value As String)
            Me.Label_Download.Text = Value
        End Set
    End Property

    Public Property Text_Button_Download() As String
        Get
            Return Me.Button_Download.Text
        End Get
        Set(ByVal Value As String)
            Me.Button_Download.Text = Value
        End Set
    End Property

    Public Property Text_Button_InstallFull() As String
        Get
            Return Me.Button_InstallFull.Text
        End Get
        Set(ByVal Value As String)
            Me.Button_InstallFull.Text = Value
        End Set
    End Property

    Public Property Text_Label_InstallFull() As String
        Get
            Return Me.Label_InstallFull.Text
        End Get
        Set(ByVal Value As String)
            Me.Label_InstallFull.Text = Value
        End Set
    End Property

    Public Property Property_Path_Folder_Download() As String
        Get
            Return Me.sPath_Folder_Download
        End Get
        Set(ByVal Value As String)
            Dim result As ResultClass = _FSO.SetFolder(Value, True)
            If result.Err._Flag = True Then
                Me.sPath_Folder_Download = Nothing
                Exit Property
            End If
            If result.ValueBoolean = False Then _LOG._sAdd(Me.GetType().Name, "Создана папка для загрузки пакетов локализации", Value, 2)
            _LOG._sAdd(Me.GetType().Name, "Задана папка для загрузки пакетов локализации", Value, 2)
            Me.sPath_Folder_Download = Value
            Me.Property_Name_File_Download = GetDownloadedPackFileName()
            If Property_Name_File_Download IsNot Nothing Then
                Me.Property_Path_File_Download = _FSO._CombinePath(Me.sPath_Folder_Download, Me.Property_Name_File_Download)
            End If
        End Set
    End Property

    Public Property Property_Path_File_Download() As String
        Get
            Return Me.sPath_File_Download
        End Get
        Set(ByVal Value As String)
            Me.sPath_File_Download = Value
            If Me.sPath_File_Download Is Nothing Then
                Me.Button_InstallFull.Enabled = False
            Else
                Me.Button_InstallFull.Enabled = True
            End If
            Dim Version As String = Property_PackInPackVersion
            If Version Is Nothing Then
                Me.Text_Label_Download = "Загружена версия: не определена"
            Else
                Me.Text_Label_Download = "Загружена версия: " & Version
            End If
        End Set
    End Property

    Public Property Property_Name_File_Download() As String
        Get
            Return Me.sName_File_Download
        End Get
        Set(ByVal Value As String)
            Me.sName_File_Download = Value
        End Set
    End Property

    Public Property Property_Name_File_Meta() As String
        Get
            Return Me.sName_File_Meta
        End Get
        Set(ByVal Value As String)
            Me.sName_File_Meta = Value
            If Me.Property_Path_Folder_Meta IsNot Nothing And Me.Property_Name_File_Meta IsNot Nothing Then
                Me.Property_Path_File_Meta = _FSO._CombinePath(Me.Property_Path_Folder_Meta, Me.Property_Name_File_Meta)
            End If
        End Set
    End Property

    Public Property Property_Path_File_Meta() As String
        Get
            Return Me.sPath_File_Meta
        End Get
        Set(ByVal Value As String)
            Me.sPath_File_Meta = Value
            Me.Property_PackInGameVersion = Me.sPath_File_Meta
        End Set
    End Property
    Public Property Property_Path_Folder_Meta() As String
        Get
            Return Me.sPath_Folder_Meta
        End Get
        Set(ByVal Value As String)
            Me.sPath_Folder_Meta = Value
            If Me.Property_Path_Folder_Meta IsNot Nothing And Me.Property_Name_File_Meta IsNot Nothing Then
                Me.Property_Path_File_Meta = _FSO._CombinePath(Me.Property_Path_Folder_Meta, Me.Property_Name_File_Meta)
            End If
        End Set
    End Property

    Public Property Property_GitList_Interval() As Integer
        Get
            Return Me.iUpdateGitList_Interval
        End Get
        Set(ByVal Value As Integer)
            Me.iUpdateGitList_Interval = Value
        End Set
    End Property

    Public Property Property_GitList_AutoUpdate() As Boolean
        Get
            Return Me.bUpdateGitList_Enable
        End Get
        Set(ByVal Value As Boolean)
            Me.bUpdateGitList_Enable = Value
            On Error Resume Next
            If Me.bUpdateGitList_Enable = True Then
                Me.BackgroundWorker.RunWorkerAsync()
            Else
                Me.BackgroundWorker.CancelAsync()
            End If
        End Set
    End Property

    Public Property Property_GitList_SelString() As String
        Get
            Return Me.sGitList_Value
        End Get
        Set(ByVal Value As String)
            Dim temp As Integer = Me.List_Git.FindString(Value)
            If temp > -1 Then
                Me.Invoke(Sub() Me.List_Git.SelectedIndex = temp)
                Me.sGitList_Value = Value
            End If
        End Set
    End Property

    Public ReadOnly Property Property_GitList_List() As List(Of String)
        Get
            Dim result As New List(Of String)
            For Each elem In Me.List_Git.Items
                result.Add(elem)
            Next
            Return result
        End Get
    End Property

    Public Property Property_PackInGameVersion() As String
        Get
            Return Me.sPackInGameVersion
        End Get
        Set(ByVal Path As String)
            Me.Text_Label_InstallFull = "Установлена версия: не определена"
            If Me.Property_Path_File_Meta Is Nothing Then Exit Property
            If _FSO._ReadTextFile(MAIN_THREAD.WL_Upd.Property_Path_File_Meta, System.Text.Encoding.UTF8) IsNot Nothing Then
                Me.sPackInGameVersion = _FSO._ReadTextFile(MAIN_THREAD.WL_Upd.Property_Path_File_Meta, System.Text.Encoding.UTF8)
                _INI._Write("EXTERNAL", "PACK_GAME_VERSION", Me.sPackInGameVersion)
                If Me.sPackInGameVersion IsNot Nothing Then Me.Text_Label_InstallFull = "Установлена версия: " & Me.sPackInGameVersion
            End If
        End Set
    End Property

    Public ReadOnly Property Property_PackInPackVersion() As String
        Get
            If Me.Property_Path_File_Download Is Nothing Then Return Nothing
            Dim File As FileInfo = CType(_FSO._GetInfo(Me.Property_Path_File_Download).ValueObject, FileInfo)
            If File Is Nothing Then
                Me.Property_Name_File_Download = Nothing
                _INI._Write("EXTERNAL", "PACK_PACK_VERSION", "")
                Return Nothing
            End If
            Me.sPackInPackVersion = Strings.Left(File.Name, Len(File.Name) - Len(File.Extension))
            Me.Property_Name_File_Download = File.Name
            _INI._Write("EXTERNAL", "PACK_PACK_VERSION", Me.sPackInPackVersion)
            Return Me.sPackInPackVersion
        End Get
    End Property
    '-----------------------------------> Properties

    '<----------------------------------- Controls
    Public Sub Button_Download_Click(sender As Object, e As EventArgs) Handles Button_Download.Click
        Me._Enabled(False)
        RaiseEvent _Event_Download_Click_Before()
        RaiseEvent _Event_Download_Before()

        Dim result As New ResultClass(Me)
        If Me.Property_Path_Folder_Download Is Nothing Then result.Err._Flag = True : result.Err._Description_App = "Не удалось получить доступ к папке загрузок" : result.Err._Description_Sys = Me.Property_Path_Folder_Download : GoTo Finalize
        If _FSO._DeleteFile(Path.Combine(Me.Property_Path_Folder_Download, "*.zip")).Err._Flag = True Then result.Err._Flag = True : result.Err._Description_App = "Не удалось удалить существующий файл пакета локализации в папке загрузки" : result.Err._Description_Sys = Me.Property_Path_Folder_Download : GoTo Finalize
        If Me.List_Git.FindString(Me.Property_GitList_SelString) = -1 Then result.Err._Flag = True : result.Err._Description_App = "Не выбрана версия загружаемого пакета локализации" : GoTo Finalize
        Me.Download(_GIT._GIT_LIST._GetByName(Me.Property_GitList_SelString, _VARS.PackageGitMaster_Name)._zipball_url, _FSO._CombinePath(Me.Property_Path_Folder_Download, Me.List_Git.Text & ".zip"), _GIT._GIT_LIST._GetByName(Me.Property_GitList_SelString, _VARS.PackageGitMaster_Name)._isMaster)

Finalize: If result.Err._Flag = True Then
            result.Err._ToLOG(2)
        End If
        RaiseEvent _Event_Download_Click_After()
    End Sub

    Public Sub Button_InstallFull_Click(sender As Object, e As EventArgs) Handles Button_InstallFull.Click
        On Error Resume Next
        RaiseEvent _Event_Download_Click_Before()
        sender.Enabled = False

        If MAIN_THREAD.WL_Mod.Property_GameExeFilePath Is Nothing Then _LOG._sAdd(Me.Name, "Не указан путь к исполняемому файлу игры", Nothing, 1) : GoTo Finalize
        If _FSO._FileExits(Property_Path_File_Download) = False Then _LOG._sAdd(Me.Name, "Не удалось получить доступ к файлу обновлений:", Property_Path_File_Download, 1) : GoTo Finalize
        _FSO._DeleteFolder(MAIN_THREAD.WL_Mod.Property_GameModFolderPath)
        If _FSO.ZIP.UnzipFolderToFolder(Property_Path_File_Download, ".data", MAIN_THREAD.WL_Mod.Property_GameModFolderPath) = False Then _LOG._sAdd(Me.Name, "Не удалось распаковать пакет обновлений в папку игры", "Исходный архив: " & Property_Path_File_Download & vbNewLine & "Путь установки: " & MAIN_THREAD.WL_Mod.Property_GameModFolderPath, 1) : GoTo Finalize

        _FSO._DeleteFile(Me.Property_Path_File_Meta).Err._Flag = False
        _FSO._WriteTextFile(Me.Property_PackInPackVersion, Me.Property_Path_File_Meta, System.Text.Encoding.UTF8)
        Me.Property_PackInGameVersion = Me.Property_PackInPackVersion

Finalize: sender.Enabled = True
        RaiseEvent _Event_InstallFull_Button_Click_After()
    End Sub

    Private Sub List_Git_SelectedIndexChanged(sender As Object, e As EventArgs) Handles List_Git.SelectedIndexChanged
        On Error Resume Next
        RaiseEvent _Event_ListGit_Selection_Change_Before()
        Me.Property_GitList_SelString = Me.List_Git.Text
        _INI._Write("EXTERNAL", "PACK_GIT_SELECTED", Me.Property_GitList_SelString)
        RaiseEvent _Event_ListGit_Selection_Change_After()
    End Sub
    '-----------------------------------> Controls

    '<----------------------------------- Logic
    Public Sub Download(DownloadFrom As String, DownloadTo As String, Method As Boolean)
        Me.Headers.Clear()
        Me.Headers.Add("Accept-Encoding: gzip,deflate")
        Me.Headers.Add("Host: api.github.com")
        Me.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36")
        Me.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
        If Method = False Then
            Me.WL_Download.DownloadStart(DownloadFrom, DownloadTo, Me.Headers)
        Else
            Me.WL_Download.ProgressBar.Value = 10
            Me.WL_Download.DownloadFrom = DownloadFrom
            Me.WL_Download.DownloadTo = DownloadTo
            Me.WL_Download.ProgressBar.Refresh()
            Me.WL_Download.Refresh()
            Me.Refresh()

            Dim logLines As List(Of LOG_SubLine) = New List(Of LOG_SubLine)
            Dim logLine As LOG_SubLine = New LOG_SubLine

            Dim e As ResultClass = _INET._GetFile(DownloadFrom, DownloadTo, Net.SecurityProtocolType.Tls12, Headers)
            If e.Err._Flag = True Then
                Me.WL_Download.DownloadFrom = "Ошибка загрузки"
                Me.WL_Download.DownloadTo = e.Err._Description_Sys
            End If

            Me.WL_Download.ProgressBar.Value = 100
            Me.WL_Download.DownloadProgress.ProgressPercentage = 100
            Me.WL_Download.DownloadProgress.Err = e.Err._Exeption
            DownloadComplete(DownloadFrom, DownloadTo, Me.WL_Download.DownloadProgress)
        End If
    End Sub

    Private Sub DownloadComplete(DownloadFrom As String, DownloadTo As String, e As WL_Download.DownloadProgressElement) Handles WL_Download._Event_Complete_Event
        Me._Enabled(True)
        Dim logLines As List(Of LOG_SubLine) = New List(Of LOG_SubLine)
        Dim logLine As LOG_SubLine = New LOG_SubLine
        If e.Err IsNot Nothing Then
            logLine.Value = e.Err.Message
            logLine.List.Add("Source URL: " & DownloadFrom)
            logLine.List.Add("Destination: " & DownloadTo)
            logLines.Add(logLine)
            _LOG._Add(Me.GetType().Name, "Ошибка загрузки:", logLines, 2)
            _FSO._DeleteFile(DownloadTo)
        Else
            logLine.Value = "Name: " & _GIT._GIT_LIST._GetByName(Me.Property_GitList_SelString, _VARS.PackageGitMaster_Name)._name
            logLine.List.Add("Source URL: " & DownloadFrom)
            logLine.List.Add("Destination: " & DownloadTo)
            logLines.Add(logLine)
            _LOG._Add(Me.GetType().Name, "Загрузка пакета локализации успешно завершена:", logLines, 2)
        End If

        Property_Path_File_Download = DownloadTo

        RaiseEvent _Event_Download_After(DownloadFrom, DownloadTo, e)
    End Sub

    Private Sub _Enabled(Enabled As Boolean, Optional FirstRecurseControl As Object = Nothing)
        Dim FirstIteration As Boolean = False
        If FirstRecurseControl Is Nothing Then
            FirstIteration = True
            RaiseEvent _Event_Controls_Enabled_Before(Enabled)
            FirstRecurseControl = Me
        End If

        For Each elem In FirstRecurseControl.Controls
            Me._Enabled(Enabled, elem)
            If TypeOf elem Is Button Then elem.Enabled = Enabled
            If TypeOf elem Is ComboBox Then elem.Enabled = Enabled
        Next

        If FirstIteration = True Then
            RaiseEvent _Event_Controls_Enabled_After(Enabled)
        End If
    End Sub
    Private Function GetDownloadedPackFileName() As String
        If Me.Property_Path_Folder_Download Is Nothing Then Return Nothing
        Dim list As String() = Directory.GetFiles(Me.Property_Path_Folder_Download)
        Dim File As FileInfo
        Dim Cntr As Integer = 0
        Dim result As String = Nothing
        If list.Count > 0 Then
            File = CType(_FSO._GetInfo(list(0)).ValueObject, FileInfo)
            If LCase(File.Extension) = ".zip" Then
                Cntr += 1
                result = Trim(File.Name)
            End If
        End If

        If Cntr = 1 Then Return result
        Return Nothing
    End Function
    '-----------------------------------> Logic

    '<----------------------------------- 'Thread
    Private Sub BackgroundWorker_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker.DoWork
        Thread.Sleep(1000)
        Do
            Dim NewGitList As List(Of Module_GIT.Class_GIT.Class_GitUpdateList.Class_GitUpdateElement) = _GIT._GetGitList()
            Me.hashGitList = _GIT._GIT_LIST_HASH
            Me.hashCurrentList = Nothing
            For Each elem In Me.List_Git.Items
                Me.hashCurrentList += elem
            Next
            Me.hashCurrentList = Md5FromString(Me.hashCurrentList)
            If Me.hashGitList <> Me.hashCurrentList Then
                RaiseEvent _Event_ListGit_List_Change_Before()
                Me.Invoke(Sub() Me.List_Git.Items.Clear())
                For i = 0 To NewGitList.Count - 1
                    Me.Invoke(Sub() Me.List_Git.Items.Add(NewGitList.Item(i)._name))
                Next
                Me.Property_GitList_SelString = _INI._GET_VALUE("EXTERNAL", "PACK_GIT_SELECTED", "").Value
                RaiseEvent _Event_ListGit_List_Change_After()
            End If

            Thread.Sleep(iUpdateGitList_Interval)
        Loop
    End Sub
    '-----------------------------------> Thread
End Class
