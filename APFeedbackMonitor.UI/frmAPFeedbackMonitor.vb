
Imports APFeedbackMonitor
Imports SICommon

Public Class frmAPFeedbackMonitor
    Dim WithEvents monitor As clsAPBackfeedMonitor
    Private isAutoStart As Boolean = False

    Public Sub New()

        InitializeComponent()

        isAutoStart = SIAppRoutines.RetrieveParmBoolean("isAutoStart", False)

        If isAutoStart Then
            StartBackfeedMonitor()
        End If

    End Sub
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Dim btn As Button = sender
        btn.Enabled = False
        Cursor = Cursors.WaitCursor

        StartBackfeedMonitor()

    End Sub

    Public Sub StartBackfeedMonitor()
        monitor = New clsAPBackfeedMonitor()
        monitor.GetRunTimeParms()

        lblVersion.Text = monitor.Version

        lblStatus2.Text = "Check for AP Checks Time:" + monitor.checkChecksHour.ToString() + ":" + monitor.checkChecksMinute.ToString("00")
        lblStatus3.Text = "Check for AP Backfeed Time:" + monitor.updateHour.ToString() + ":" + monitor.updateMinute.ToString("00")

        monitor.StartTimer()
        lblTimerTicked.Text = "Started - " + SQLUtil.ToDBDateTime(Now) +
            " chk int:" + monitor.TimerInterval.ToString() + " " + monitor.TimerIntervalUnit + " v:" + monitor.Version

    End Sub

    Private Sub handle_Timer_Ticked(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.TimerTicked
        ' lblTimerTicked.Text = DateTime.Now.ToLongTimeString()
        UpdateTimerTickedLabel(SICommon.SQLUtil.ToDBDateTime(Now))
        Application.DoEvents()
    End Sub

    Private Sub handle_CheckChecksGeneratedEvent(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.ChecksGeneratedFoundEvent
        UpdateLabelStatus("Checks have been generated today - " + SQLUtil.ToDBDateTime(Now))
        Application.DoEvents()
    End Sub

    Private Sub handle_CheckChecksNotGeneratedEvent(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.ChecksGeneratedNotFoundEvent
        UpdateLabelStatus("Checks have NOT been generated today - " + SQLUtil.ToDBDateTime(Now))
        Application.DoEvents()
    End Sub
    Private Sub handle_BackFeedResultsFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.BackfeedResultsFoundEvent
        UpdateLabelStatus4("Backfeed occurred successfully - " + SQLUtil.ToDBDateTime(Now))
        Application.DoEvents()
    End Sub
    Private Sub handle_BackFeedResultsNotFoundEvent(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.BackfeedResultsNotFoundEvent
        UpdateLabelStatus4("Backfeed has NOT occurred !!!!! - " + SQLUtil.ToDBDateTime(Now))
        Application.DoEvents()
    End Sub

    Private Sub handle_DateChangedEvent(ByVal vMonitor As clsAPBackfeedMonitor) Handles monitor.DateChangedEvent
        UpdateLabelStatus4("")
        UpdateLabelStatus("")
        Application.DoEvents()
    End Sub

    Private Sub ShutDown()
        Application.Exit()
    End Sub

    Private Sub mnuExit_Click(sender As Object, e As EventArgs) Handles mnuExit.Click
        ShutDown()
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        btnStart.Enabled = True
        Cursor = Cursors.Default

        monitor.StopTimer()

    End Sub

    Delegate Sub StringParameterDelegate(ByVal value As String)

    Private Sub UpdateTimerTickedLabel(ByVal value As String)
        If InvokeRequired Then
            ' We're not in the UI thread, so we need to call BeginInvoke
            BeginInvoke(New StringParameterDelegate(AddressOf UpdateTimerTickedLabel), New Object() {value})
            Return
        End If
        ' Must be on the UI thread if we've got this far
        lblTimerTicked.Text = value
    End Sub

    Private Sub UpdateLabelStatus(ByVal value As String)
        If InvokeRequired Then
            ' We're not in the UI thread, so we need to call BeginInvoke
            BeginInvoke(New StringParameterDelegate(AddressOf UpdateLabelStatus), New Object() {value})
            Return
        End If

        ' Must be on the UI thread if we've got this far
        lblStatus.Text = value

    End Sub

    Private Sub UpdateLabelStatus4(ByVal value As String)
        If InvokeRequired Then
            ' We're not in the UI thread, so we need to call BeginInvoke
            BeginInvoke(New StringParameterDelegate(AddressOf UpdateLabelStatus4), New Object() {value})
            Return
        End If

        ' Must be on the UI thread if we've got this far
        lblStatus4.Text = value

    End Sub
End Class
