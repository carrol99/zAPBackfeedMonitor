
Imports SICommon
Imports System.Net.Mail

Public Class APFeedbackMonitorService
    Dim _SendMail As SISendMail

    Dim EventLogSource As String = "APBackfeedMonitor"
    Dim EventLogLogName As String = "APBackfeedMonitorLog"

    Dim isOkToLogTimerEvent As Boolean = False
    Dim isOkToSendEmail As Boolean = True

    Dim WithEvents _APBackfeed As APFeedbackMonitor.clsAPBackfeedMonitor
    Dim Version As String = "2.0"

    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If Not System.Diagnostics.EventLog.SourceExists(EventLogSource) Then
            System.Diagnostics.EventLog.CreateEventSource(EventLogSource, EventLogLogName)
        End If

        'APMonitorEventLog.Source = EventLogSource
        'Throw New Exception("Start up")
        'APMonitorEventLog.Log = EventLogLogName
    End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)

        'APMonitorEventLog.WriteEntry("OnStart - Service Starting:" + ServiceName +
        '                             vbCrLf + "Server Name:" + Environment.MachineName +
        '                             vbCrLf + "User Name:" + Environment.UserName +
        '                             vbCrLf + "User Domain:" + Environment.UserDomainName, EventLogEntryType.Information, 1000)

        Try
            _APBackfeed = New APFeedbackMonitor.clsAPBackfeedMonitor
            '_APBackfeed.GetRunTimeParms()

            ' _APBackfeed.StartTimer()

            'isOkToLogTimerEvent = _APBackfeed.isOkToLogTimerEvent

        Catch ex As Exception
            'APMonitorEventLog.WriteEntry("Error in OnStart-" + ex.Message, EventLogEntryType.Error, 9990)
            'MyBase.Stop()
            Throw ex
        End Try

        Dim sRunTimeParms As String = ""
        'sRunTimeParms = _APBackfeed.DisplayRunTimeParms
        sRunTimeParms += "  Service Program Version:" + Version + vbCrLf

        'APMonitorEventLog.WriteEntry(sRunTimeParms, EventLogEntryType.Information, 1002)

        Try
            SendSupportEmail("***Support*** AP Feedback Monitor Service Starting", "AP Monitor Service Starting " +
                                        vbCrLf + "Server Name:" + Environment.MachineName +
                                        vbCrLf + "User Name:" + Environment.UserName +
                                        vbCrLf + "User Domain:" + Environment.UserDomainName)
        Catch ex As Exception
            'APMonitorEventLog.WriteEntry("Error Sending Start EMail--" + ex.Message, EventLogEntryType.Warning, 9991)
        End Try

    End Sub

    Protected Overrides Sub OnStop()
        'APMonitorEventLog.WriteEntry("OnStop - Service Ending:" + ServiceName +
        '                             vbCrLf + "Server Name:" + Environment.MachineName +
        '                             vbCrLf + "User Name:" + Environment.UserName +
        '                             vbCrLf + "User Domain:" + Environment.UserDomainName, EventLogEntryType.Information, 2000)

        Try
            _APBackfeed.StopTimer()
            _APBackfeed = Nothing

        Catch ex As Exception
            'APMonitorEventLog.WriteEntry("Error Stop Timer-" + ex.Message, EventLogEntryType.Error, 9993)
        End Try

        Try
            SendSupportEmail("***Support*** AP Backfeed Monitor Service Ending", "AP Backfeed Monitor Service Ending " +
                                        vbCrLf + "Server Name:" + Environment.MachineName +
                                        vbCrLf + "User Name:" + Environment.UserName +
                                        vbCrLf + "User Domain:" + Environment.UserDomainName)
        Catch ex As Exception
            'APMonitorEventLog.WriteEntry("Error Sending Stop EMail-" + ex.Message, EventLogEntryType.Warning, 9994)
        End Try

    End Sub
    Protected Overrides Sub OnContinue()
        MyBase.OnContinue()

        APMonitorEventLog.WriteEntry("OnContinue - Service Continue:" + ServiceName)

    End Sub

    Protected Sub BackfeedResultsFound_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.BackfeedResultsFoundEvent
        APMonitorEventLog.WriteEntry("BackfeedResultsFound", EventLogEntryType.Warning, 2001)
    End Sub
    Protected Sub BackfeedResultsNotFound_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.BackfeedResultsNotFoundEvent
        APMonitorEventLog.WriteEntry("BackfeedResultsNotFound", EventLogEntryType.Warning, 9001)
    End Sub
    Protected Sub CheckBackfeed_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.CheckBackfeedEvent
        APMonitorEventLog.WriteEntry("CheckBackfeedEvent", EventLogEntryType.Information, 2000)
    End Sub
    Protected Sub ChecksGeneratedFound_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.ChecksGeneratedFoundEvent
        APMonitorEventLog.WriteEntry("ChecksGeneratedFound", EventLogEntryType.Warning, 2002)
    End Sub
    Protected Sub ChecksGeneratedNotFound_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.ChecksGeneratedNotFoundEvent
        APMonitorEventLog.WriteEntry("ChecksGeneratedNotFound", EventLogEntryType.Warning, 9002)
    End Sub
    Protected Sub DateChanged_event(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.DateChangedEvent
        APMonitorEventLog.WriteEntry("DateChanged", EventLogEntryType.Information, 2000)
    End Sub
    Protected Sub UpdatePayment_TimerTicked(ByVal vMonitor As APFeedbackMonitor.clsAPBackfeedMonitor) Handles _APBackfeed.TimerTicked
        If Not isOkToLogTimerEvent Then
            Return
        End If

        If _NextWriteTimerTick.Ticks = 0 Then
            _NextWriteTimerTick = Now
        End If

        If Now < _NextWriteTimerTick Then
            Return
        End If

        _NextWriteTimerTick = getNextTimerTickTimer()

        APMonitorEventLog.WriteEntry("TimerTicked:" + ServiceName, EventLogEntryType.Information, 1000)

    End Sub

    Private Function getNextTimerTickTimer() As DateTime
        Dim wrkTime As DateTime
        wrkTime = Now().AddMinutes(TimerTickWriteInterval)
        Return wrkTime.AddSeconds(-15)
    End Function

    'Protected Sub UpdatePayment_RecordsProcessed(ByVal vUpdatePayment As clsUpdatePayments) Handles _APBackfeed.RecordsProcessed
    '    Dim showResults As String

    '    showResults = _APBackfeed.ShowResults(vUpdatePayment)

    '    APMonitorEventLog.WriteEntry("RecordsProcessed Event Occurred:" + vbCrLf + showResults, EventLogEntryType.Information, 1200)
    'End Sub

    Private Sub SendSupportEmail(ByVal vSubject As String, ByVal vBody As String)

        Dim _SupportMail As SISendMail

        Try
            _SupportMail = New SISendMail("APMonitorSupport", "MonitorEmails")
            SendEmailInfo(vSubject, vBody, _SupportMail)
        Catch ex As Exception
            APMonitorEventLog.WriteEntry("Support Email Error:" + ex.Message, EventLogEntryType.Warning, 9999)
        End Try

    End Sub

    Private Sub SendEmailInfo(ByVal vSubject As String, ByVal vBody As String, Optional ByVal vSendMail As SISendMail = Nothing)
        If Not isOkToSendEmail Then
            Return
        End If

        Dim isSendMailDefined As Boolean = False

        APMonitorEventLog.WriteEntry("Sending Email - " + vSubject, EventLogEntryType.Information, 1100)

        If vSendMail Is Nothing Then
            Try
                _SendMail = New SISendMail("APMonitor", "MonitorEmails")
                isSendMailDefined = True
            Catch ex As Exception
                _SendMail = New SISendMail()
                isSendMailDefined = False
            End Try
        Else
            _SendMail = vSendMail
            isSendMailDefined = True
        End If

        With _SendMail
            .ToName = "AP Monitor List"
            .Subject = vSubject
            .Body = vBody
            Try
                .Transmit()
            Catch ex As Exception
                APMonitorEventLog.WriteEntry("Error Send Email:" + ex.Message, EventLogEntryType.Warning, 5099)
            End Try
        End With
    End Sub

    Private Sub SendMail(ByVal vSISendMail As SISendMail)

        Try
            vSISendMail.Transmit()

        Catch exSMTP As SmtpException
            Dim errMessage As String
            Dim errStatus As String

            errMessage = exSMTP.Message
            errStatus = exSMTP.StatusCode.ToString()
            System.Threading.Thread.Sleep(SleepIntervalOnEmailError)

            Try
                vSISendMail.Body += Environment.NewLine + "*"
                vSISendMail.Subject += "*"
                vSISendMail.Transmit()

            Catch ex2 As Exception
                Throw New Exception("ReSend Failed:" + ex2.Message)
            End Try

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private _LastTimerTickWritten As DateTime
    Public Property LastTimerTickWritten() As DateTime
        Get
            Return _LastTimerTickWritten
        End Get
        Set(ByVal value As DateTime)
            _LastTimerTickWritten = value
        End Set
    End Property

    Private _NextWriteTimerTick As DateTime
    Public Property NextWriteTimerTick() As DateTime
        Get
            Return _NextWriteTimerTick
        End Get
        Set(ByVal value As DateTime)
            _NextWriteTimerTick = value
        End Set
    End Property

    Private _TimerTickWriteInterval As Integer
    Public Property TimerTickWriteInterval() As Integer
        Get
            Return _TimerTickWriteInterval
        End Get
        Set(ByVal value As Integer)
            _TimerTickWriteInterval = value
        End Set
    End Property

    Private _SleepIntervalOnEmailError As Integer
    Public Property SleepIntervalOnEmailError() As Integer
        Get
            Return _SleepIntervalOnEmailError
        End Get
        Set(ByVal value As Integer)
            _SleepIntervalOnEmailError = value
        End Set
    End Property

End Class
