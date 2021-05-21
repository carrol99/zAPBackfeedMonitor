Imports System.Text
Imports System.Configuration
Imports log4net
Imports SICommon
Imports System.Timers

Public Class clsAPBackfeedMonitor
    Public Version As String = "2.0g"
    Dim tTimer As System.Timers.Timer
    Dim _SendMail As SISendMail
    Public log4 As ILog
    Public Event TimerTicked(ByVal vMonitor As clsAPBackfeedMonitor)
    Public Event CheckBackfeedEvent(ByVal vMonitor As clsAPBackfeedMonitor)
    Public Event BackfeedResultsFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor)
    Public Event BackfeedResultsNotFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor)
    Public Event DateChangedEvent(ByVal vMonitor As clsAPBackfeedMonitor)
    Public EnableSSL As Boolean = False


    Public Event ChecksGeneratedFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor)
    Public Event ChecksGeneratedNotFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor)

    Dim isCurrentlyProcessing As Boolean = False
    Public lastCheckDate As New DateTime
    Public checkForUpdateTime As DateTime
    Public checkForChecksTime As DateTime
    Public isForceCheck As Boolean = False
    Public isBackfillNeeded As Boolean = False
    Public DateWeAreChecking As DateTime
    Public dtCheckResults As DataTable
    Public dtCheckPrinting As DataTable
    Public isAlreadyCheckedToday As Boolean = False
    Public haveChecksBeenGenerated As Boolean = False
    Public updateHour As Int32 = 23
    Public updateMinute As Int32 = 2
    Public checkChecksHour As Int32 = 21
    Public checkChecksMinute As Int32 = 22
    Public isAlreadyProcessing As Boolean = False
    Public isSendSupportEmails As Boolean = True
    Public isTesting As Boolean = False
    Public isSendEmailWhenChecksAreGenerated = True
    Public isSendEmailWhenBackfeedFound = True
    Public isBypassWeekend = False
    Public isStartupPass As Boolean = False
    Public ChecksGeneratedDateTime As DateTime
    Public ChecksResultsGeneratedDateTime As DateTime

    Public _ODBCDataRoutines As ODBCDataRoutines
    Public isLogEnabled As Boolean = True

    Public Sub New()

        _ServerName = Environment.MachineName
        UserName = Environment.UserName

        _ODBCDataRoutines = New ODBCDataRoutines()

        log4net.Config.XmlConfigurator.Configure()
        log4 = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    End Sub

#Region "Check Backfeed functions"
    Public Sub mainBackFeed()
        Dim timeNow As DateTime = Now

        log4.Debug("mainBackFeed")

        If lastCheckDate.Date = timeNow.Date Then
            log4.Debug("dates match")
            'if we have already checked the results today, no need to do it again
            If isAlreadyCheckedToday Then
                log4.Debug("isAlreadyChecked")
                Return
            End If
        Else
            'the date has changed, so set the fields to enable checking for the new day
            RaiseEvent DateChangedEvent(Me)

            log4.Debug("Date has changed")
            lastCheckDate = timeNow

            checkForUpdateTime = New DateTime(timeNow.Year, timeNow.Month, timeNow.Day, updateHour, updateMinute, 0)
            checkForChecksTime = New DateTime(timeNow.Year, timeNow.Month, timeNow.Day, checkChecksHour, checkChecksMinute, 0)

            If (checkForChecksTime.DayOfWeek = DayOfWeek.Sunday Or checkForChecksTime.DayOfWeek = DayOfWeek.Saturday) And isBypassWeekend Then
                log4.Debug("It is the weekend - not checking for updates today")
                isAlreadyCheckedToday = True
            Else
                isAlreadyCheckedToday = False
            End If

            haveChecksBeenGenerated = False
            isBackfillNeeded = False
            isForceCheck = False

            'set these flags if we are testing and it is past time to get a good read on whether the checks were generated or not.
            If isTesting Then
                haveChecksBeenGenerated = True
                isBackfillNeeded = True
                isForceCheck = True
            End If

        End If

        log4.Debug("Prior to checking for checks generated time")
        If timeNow < checkForChecksTime Then
            Return
        End If

        log4.Info("it is time to check for checks")

        If Not haveChecksBeenGenerated Then
            log4.Debug("in if and checking for generated checks")
            'see if 4warranty has generated checks for AP today
            haveChecksBeenGenerated = checkFeedForChecksGenerated()

            If haveChecksBeenGenerated Then
                log4.Info("checks have been generated - mainfeed")
                RaiseEvent ChecksGeneratedFoundEvent(Me)
                sendChecksHaveBeenRunToday()
                isBackfillNeeded = True
                isForceCheck = True
            Else
                log4.Info("Checks have not been generated - mainfeed")
                'if no checks generated, then don't need to check for results later. So we are done checking for today
                isAlreadyCheckedToday = True

                RaiseEvent ChecksGeneratedNotFoundEvent(Me)

                If isStartupPass Then
                    log4.Debug("startup pass is true - not sending not found email")
                    isStartupPass = False
                Else
                    log4.Debug("calling sendchecksnotruntoday routine")
                    sendChecksNotRunToday()
                End If

                Return

            End If
        End If

        log4.Debug("Prior to checking for  update time")

        If timeNow < checkForUpdateTime Then
            log4.Debug("Not time to check for updates yet")
            Return
        End If

        log4.Debug(" prior to checking for backfeed")

        RaiseEvent CheckBackfeedEvent(Me)

        checkStatusOfFeed()

        isAlreadyCheckedToday = True

    End Sub

    Public Function checkStatusOfFeed() As Boolean

        log4.Debug("checkstatusofFeed function")

        If isForceCheck Then
            isBackfillNeeded = True
        Else
            isBackfillNeeded = checkFeedForChecksGenerated()
        End If

        If isBackfillNeeded = False Then
            log4.Debug("isbackfill needed is false")
            Return False
        End If

        log4.Debug("is backfill need is true ")

        Dim areBackfeedRecordsFound As Boolean

        areBackfeedRecordsFound = checkForBackfeedRecords()

        If areBackfeedRecordsFound = True Then
            log4.Info("back feed records are found")

            RaiseEvent BackfeedResultsFoundEvent(Me)

            sendBackfeedIsPresent()

            Return True
        End If

        log4.Info("back feed records NOT found")

        RaiseEvent BackfeedResultsNotFoundEvent(Me)

        sendBackfeedNotPresent()

        Return areBackfeedRecordsFound
    End Function

    Public Function checkForBackfeedRecords() As Boolean
        log4.Debug("checkForBackfeedrecords function")

        Dim sDate As String
        Dim queryWhere As String

        If DateWeAreChecking.Ticks = 0 Then
            log4.Debug("setting datewearechecking to now")
            DateWeAreChecking = Now
        End If

        sDate = SQLUtil.ToDBDate(DateWeAreChecking)

        queryWhere = " convert(date, checkresult.createdate) = '" & sDate & "' "

        dtCheckResults = RetrieveCheckResults(queryWhere)

        If dtCheckResults.Rows.Count > 0 Then
            log4.Info("backfeed records are found")

            Return True
        End If

        log4.Info("backfeed records NOT found")

        Return False
    End Function

    Public Function checkFeedForChecksGenerated() As Boolean

        ChecksGeneratedDateTime = New DateTime()

        log4.Info("checkfeedforchecksgenerated function")
        dtCheckPrinting = RetrieveCheckPrinting()

        If (dtCheckPrinting.Rows.Count > 0) Then
            log4.Info("checks have been generated")
            Dim myRow As DataRow = dtCheckPrinting.Rows(0)
            ChecksGeneratedDateTime = myRow("CREATEDATE")
            Return True
        End If

        log4.Info("checks have NOT been generated")

        Return False

    End Function

    Public Function RetrieveCheckPrinting(Optional ByVal vSQLWhere As String = "") As DataTable
        Dim dt As DataTable

        log4.Debug("RetrieveCheckPrinting Function")

        Dim sbSQL As New StringBuilder
        sbSQL.Append("SELECT * FROM checkPrinting ")
        If vSQLWhere.Length > 0 Then
            sbSQL.Append(" WHERE ")
            sbSQL.Append(vSQLWhere)
        End If
        sbSQL.Append(" order by uniquekey desc ")

        _ODBCDataRoutines.SQLString = sbSQL.ToString

        Try
            dt = _ODBCDataRoutines.getTableFromDB()
            log4.Debug("After retrieving checkprinting records - records found:" + dt.Rows.Count.ToString())
            _ODBCDataRoutines.closeConnection()
        Catch ex As Exception
            log4.Debug("SQL Exception retrieving checkprinting records: " + ex.Message)
            Throw New Exception()
        End Try

        log4.Debug("Returning checkprinting records to calling function")
        Return dt

    End Function

    Private Function RetrieveCheckResults(Optional ByVal vSQLWhere As String = "") As DataTable
        Dim dt As DataTable

        log4.Debug("RetrieveCheckResults function")

        ChecksGeneratedDateTime = New DateTime

        Dim sbSQL As New StringBuilder
        sbSQL.Append("SELECT checkResult.*, servicer.ServicerName, servicer.ServiceType FROM checkResult ")
        sbSQL.Append(" LEFT JOIN servicer on servicer.uniquekey = checkResult.iservicer ")

        If vSQLWhere.Length > 0 Then
            sbSQL.Append(" WHERE ")
            sbSQL.Append(vSQLWhere)
        End If

        sbSQL.Append(" order by checkresult.createdate desc ")
        _ODBCDataRoutines.SQLString = sbSQL.ToString
        log4.Debug("Before retrieve from db:" + sbSQL.ToString())

        Try
            dt = _ODBCDataRoutines.getTableFromDB()

            If dt.Rows.Count > 0 Then
                ChecksResultsGeneratedDateTime = dt.Rows(0)("createdate")
            End If

            log4.Debug("After retrieving from db")

            _ODBCDataRoutines.closeConnection()

        Catch ex As Exception
            log4.Error("Error in RetrieveCheckResults reading db, error:" + ex.Message)
            Throw ex
        End Try

        log4.Debug("RetrieveCheckResults function - after reading database")

        Return dt

    End Function

#End Region

#Region "Emails"
    Public Function sendBackfeedNotPresent() As Boolean
        Dim subject As String
        Dim body As String
        subject = "4Warranty AP Backfeed ***HAS NOT*** occurred"
        body = "4Warranty AP Backfeed ***HAS NOT*** occurred"
        SendEmailInfo(subject, body)

        log4.Info("send backfeed NOT PRESENT function")

        Return True
    End Function

    Public Function sendBackfeedIsPresent() As Boolean
        If Not isSendEmailWhenBackfeedFound Then
            log4.Info("bypassing sendBackFeed is present email ")
            Return False
        End If

        Dim subject As String
        Dim body As String
        subject = "4Warranty AP Backfeed  HAS  occurred"
        body = "4Warranty AP Backfeed  HAS occurred "
        body += vbCrLf + "Check Results received: " + ChecksResultsGeneratedDateTime.ToString()

        SendEmailInfo(subject, body)

        log4.Info("send backfeed IS present function")

        Return True
    End Function

    Public Function sendChecksNotRunToday()

        log4.Info("sendChecksNotRunToday function")

        Dim subject As String
        Dim body As String
        subject = "4Warranty AP checks NOT GENERATED!!! Today"
        body = "4Warranty AP checks NOT GENERATED!!! Today - " + SQLUtil.ToDBDateTime(Now)

        SendEmailInfo(subject, body)

        Return False

    End Function

    Public Function sendChecksHaveBeenRunToday()

        log4.Info("sendChecksHaveBeenRunToday function")

        If Not isSendEmailWhenChecksAreGenerated Then
            log4.Debug("bypassing sending checks have been run email ")
            Return False
        End If

        Dim sDate As String = ChecksGeneratedDateTime.ToString()

        Dim subject As String
        Dim body As String
        subject = "4Warranty AP checks Were Generated Today"
        body = "4Warranty AP checks Were Generated Today - " + SQLUtil.ToDBDateTime(Now)
        body += vbCrLf + "Checks generated time: " + sDate

        log4.Info("Sending checks have run email")

        SendEmailInfo(subject, body)

        Return False

    End Function
#End Region

    Public Sub createODBC()
        _ODBCDataRoutines = New ODBCDataRoutines()
        _ODBCDataRoutines.ConnectionString = ConnectionString
    End Sub

#Region "Run Time Parms"
    Public Function DisplayRunTimeParms() As String
        Dim sbParms As New StringBuilder
        sbParms.Append("AP Feedback Monitor Run Time Parms:" + Version + "  " + vbCrLf)
        sbParms.Append("  Check Timer Interval:" + _TimerInterval.ToString + vbCrLf)
        sbParms.Append("  Interval Unit:" + _TimerIntervalUnit + vbCrLf)
        sbParms.Append("  Send Email:" + _isOkToEmail.ToString + vbCrLf)
        sbParms.Append("  Send Timer Tick Email:" + _isSendEmailOnTimerTick.ToString + vbCrLf)
        sbParms.Append("  Log Timer Event:" + _isOkToLogTimerEvent.ToString + vbCrLf)
        sbParms.Append("  Send Run Parms Email:" + _isOkToSendRunTimeParmsEmail.ToString + vbCrLf)
        sbParms.Append("  Server Name:" + Environment.MachineName + vbCrLf)
        sbParms.Append("  updateHour:" + updateHour.ToString() + vbCrLf)
        sbParms.Append("  updateMinute:" + updateMinute.ToString() + vbCrLf)
        sbParms.Append("  checkCheckHour:" + checkChecksHour.ToString() + vbCrLf)
        sbParms.Append("  checkCheckMinute:" + checkChecksMinute.ToString() + vbCrLf)
        sbParms.Append("  isSendSupportEmails:" + isSendSupportEmails.ToString() + vbCrLf)
        sbParms.Append("  isSendEmailWhenChecksAreGenerated:" + isSendEmailWhenChecksAreGenerated.ToString() + vbCrLf)
        sbParms.Append("  isSendEmailWhenBackfeedFound:" + isSendEmailWhenBackfeedFound.ToString() + vbCrLf)

        Return sbParms.ToString

    End Function

    Public Sub GetRunTimeParms()
        Dim sValue As String

        sValue = ConfigurationManager.AppSettings.Get("IntervalUnit")

        If Not sValue Is Nothing Then
            _TimerIntervalUnit = sValue
        End If

        sValue = ConfigurationManager.AppSettings.Get("CheckInterval")

        If Not sValue Is Nothing Then
            Integer.TryParse(sValue, _TimerInterval)
        End If

        sValue = ConfigurationManager.AppSettings.Get("SendEmail")
        If Not sValue Is Nothing Then
            Boolean.TryParse(sValue, isOkToEmail)
        End If

        Try
            ConnectionString = ConfigurationManager.ConnectionStrings("OdbcConnection1").ToString
        Catch ex As Exception
            _ConnectionString = ""
        End Try

        sValue = ConfigurationManager.AppSettings.Get("SendTimerTickEmail")

        If Not sValue Is Nothing Then
            Boolean.TryParse(sValue, _isSendEmailOnTimerTick)
        End If

        sValue = ConfigurationManager.AppSettings.Get("SendRunTimeParmsEmail")

        If Not sValue Is Nothing Then
            Boolean.TryParse(sValue, _isOkToSendRunTimeParmsEmail)
        End If

        sValue = ConfigurationManager.AppSettings.Get("LogTimerEvent")

        If Not sValue Is Nothing Then
            Boolean.TryParse(sValue, _isOkToLogTimerEvent)
        End If

        updateHour = SIAppRoutines.RetrieveParmString("updateHour", "14")
        updateMinute = SIAppRoutines.RetrieveParmString("updateMinute", "03")
        checkChecksHour = SIAppRoutines.RetrieveParmString("checkChecksHour", "12")
        checkChecksMinute = SIAppRoutines.RetrieveParmString("checkChecksMinute", "04")
        isSendSupportEmails = SIAppRoutines.RetrieveParmBoolean("SendSupportEmails", "true")
        isSendEmailWhenChecksAreGenerated = SIAppRoutines.RetrieveParmBoolean("isSendEmailWhenChecksAreGenerated", False)
        isSendEmailWhenBackfeedFound = SIAppRoutines.RetrieveParmBoolean("isSendEmailWhenBackfeedFound", False)
        EnableSSL = SIAppRoutines.RetrieveParmBoolean("EnableSSL", False)

        If _isOkToSendRunTimeParmsEmail Then
            SendRunTimeParmsEmail()
        End If

    End Sub
#End Region

#Region "Timer Functions"

    Private Sub OnTimedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)

        _timerLastInvoked = e.SignalTime

        log4.Debug("OnTimedEvent function")
        If isAlreadyProcessing Then
            log4.Debug("isAlreadyProcessing ")
            Return
        End If

        log4.Debug("Setting isAlreadyProcessing to true")

        isAlreadyProcessing = True

        RaiseEvent TimerTicked(Me)

        mainBackFeed()

        log4.Debug("onTimedEvent After Calling mainBackFeed")
        If _isSendEmailOnTimerTick Then
            SendTimerTickEmail()
        End If

        log4.Debug("Setting isAlreadyProcessing to false")

        isAlreadyProcessing = False

    End Sub

    Public Sub StartTimer()
        If tTimer Is Nothing Then
            tTimer = New Timer
            AddHandler tTimer.Elapsed, AddressOf OnTimedEvent
        End If

        tTimer.Interval = TimerRealInterval

        tTimer.Start()

        _isTimerRunning = True

        SendStartUpEmail()

    End Sub

    Public Sub StopTimer()

        If Not tTimer Is Nothing Then
            tTimer.Stop()
        End If

        _isTimerRunning = False

        SendStopEmail()

    End Sub
#End Region

#Region "Emails"
    Public Sub SendStartUpEmail()
        Dim Subject As String
        Dim Body As String

        Subject = "***Support*** AP Backfeed Monitor Started On:" + Now.ToString
        Body = Subject + vbCrLf
        Body += "Server Name:" + _ServerName + vbCrLf
        Body += "User Name:" + UserName + vbCrLf

        SendSupportEmail(Subject, Body)
    End Sub

    Public Sub SendStopEmail()
        Dim Subject As String
        Dim Body As String

        Subject = "***Support*** AP Backfeed Monitor Stopped On:" + Now.ToString

        Body = Subject + vbCrLf
        Body += "Server Name:" + _ServerName + vbCrLf
        Body += "User Name:" + UserName + vbCrLf

        SendSupportEmail(Subject, Body)
    End Sub

    Public Sub SendRunTimeParmsEmail()
        Dim Subject As String
        Dim Body As String
        Body = DisplayRunTimeParms()

        Subject = "***Support*** AP Backfeed Monitor Run Time Parms:" + Now.ToString

        SendSupportEmail(Subject, Body)

    End Sub

    Public Sub SendTimerTickEmail()
        Dim Subject As String
        Subject = "***Support*** AP Backfeed Monitor Timer Tick On:" + Now.ToString

        SendSupportEmail(Subject, Subject)
    End Sub

    Public Sub SendSupportEmail(ByVal vSubject As String, ByVal vBody As String)
        If Not isOkToEmail Then
            Return
        End If

        If Not isSendSupportEmails Then
            Return
        End If

        Dim _siSend As New SISendMail("Support", "MonitorEmails")

        SendEmailInfo(vSubject, vBody, _siSend)
    End Sub

    Public Sub SendEmailInfo(ByVal vSubject As String, ByVal vBody As String, Optional ByVal vSendMail As SISendMail = Nothing)

        If vSendMail Is Nothing Then
            Try
                _SendMail = New SISendMail("Monitor", "MonitorEmails")
            Catch ex As Exception
                _SendMail = New SISendMail()
            End Try
        Else
            _SendMail = vSendMail
        End If

        With _SendMail
            .ToName = "AP Backfeed Monitor List"
            .Subject = vSubject
            .Body = vBody
            .EnableSSL = EnableSSL

            Try
                .Transmit()
            Catch ex As Exception
                MsgBox("Error Send Email:" + ex.Message)
            End Try
        End With

    End Sub
#End Region

#Region "Properties"
    Private _TimerRealInterval As Long
    Public ReadOnly Property TimerRealInterval() As Long
        Get
            _TimerRealInterval = _TimerInterval

            If _TimerIntervalUnit.ToLower = "s" Then
                _TimerRealInterval *= 1000
            ElseIf _TimerIntervalUnit = "m" Then
                _TimerRealInterval *= 1000 * 60
            End If

            Return _TimerRealInterval

        End Get
    End Property
    Private _ConnectionString As String
    Public Property ConnectionString() As String
        Get
            Return _ConnectionString
        End Get
        Set(ByVal value As String)
            _ConnectionString = value
            If Not _ODBCDataRoutines Is Nothing Then
                _ODBCDataRoutines.ConnectionString = value
            End If
        End Set
    End Property

    Private sUserName As String
    Public Property UserName() As String
        Get
            Return sUserName
        End Get
        Set(ByVal value As String)
            sUserName = value
        End Set
    End Property

    Private _TimerInterval As Long = 15
    Public Property TimerInterval() As Long
        Get
            Return _TimerInterval
        End Get
        Set(ByVal value As Long)
            _TimerInterval = value
        End Set
    End Property

    Private _TimerIntervalUnit As String = "s"
    Public Property TimerIntervalUnit() As String
        Get
            Return _TimerIntervalUnit
        End Get
        Set(ByVal value As String)
            _TimerIntervalUnit = value
        End Set
    End Property

    Private _isTimerRunning As Boolean
    Public Property isTimerRunning() As Boolean
        Get
            Return _isTimerRunning
        End Get
        Set(ByVal value As Boolean)
            _isTimerRunning = value
        End Set
    End Property

    Private _timerLastInvoked As DateTime
    Public Property timerLastInvoked() As DateTime
        Get
            Return _timerLastInvoked
        End Get
        Set(ByVal value As DateTime)
            _timerLastInvoked = value
        End Set
    End Property

    Private _isOkToEmail As Boolean = True
    Public Property isOkToEmail() As Boolean
        Get
            Return _isOkToEmail
        End Get
        Set(ByVal value As Boolean)
            _isOkToEmail = value
        End Set
    End Property

    Private _isSendEmailOnTimerTick As Boolean = True
    Public Property isSendEmailOnTimerTick() As Boolean
        Get
            Return _isSendEmailOnTimerTick
        End Get
        Set(ByVal value As Boolean)
            _isSendEmailOnTimerTick = value
        End Set
    End Property

    Private _ServerName As String = ""
    Public Property ServerName() As String
        Get
            Return _ServerName
        End Get
        Set(ByVal value As String)
            _ServerName = value
        End Set
    End Property

    Private _isOkToSendRunTimeParmsEmail As Boolean = True
    Public Property isOkToSendRunTimeParmsEmail() As Boolean
        Get
            Return _isOkToSendRunTimeParmsEmail
        End Get
        Set(ByVal value As Boolean)
            _isOkToSendRunTimeParmsEmail = value
        End Set
    End Property

    Private _isOkToLogTimerEvent As Boolean = False
    Public Property isOkToLogTimerEvent() As Boolean
        Get
            Return _isOkToLogTimerEvent
        End Get
        Set(ByVal value As Boolean)
            _isOkToLogTimerEvent = value
        End Set
    End Property

#End Region

End Class
