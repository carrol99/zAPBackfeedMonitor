Imports System.Text
Imports APFeedbackMonitor
Imports System.Configuration
Imports log4net
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SICommon


<TestClass()> Public Class clsAPBackfeedMonitorTest
    Dim WithEvents _monitor As New clsAPBackfeedMonitor
    Dim log As ILog
    Dim sConnectionString As String
    Dim sConnectionStringDisplay As String


    <TestInitialize()> Public Sub setup()
        log4net.Config.XmlConfigurator.Configure()

        log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        sConnectionString = ConfigurationManager.ConnectionStrings("OdbcConnection1").ToString()
        sConnectionStringDisplay = SQLUtil.HideField(sConnectionString, "pwd")

        log.Info("Initializing test connection:" + sConnectionString)

    End Sub
    <TestMethod()> Public Sub checkForBackfillTest()

        _monitor.ConnectionString = sConnectionString

        Dim isNeedBackFill As Boolean = _monitor.checkFeedForChecksGenerated()

        log.Info("CheckPrinting records found:" + isNeedBackFill.ToString())

        isNeedBackFill = _monitor.checkForBackfeedRecords()

        log.Info("are backfeed records found:" + isNeedBackFill.ToString())

        _monitor.SendEmailInfo("subject sendemailinfo", "body")

        _monitor.sendBackfeedNotPresent()
        _monitor.sendBackfeedIsPresent()
        _monitor.sendChecksHaveBeenRunToday()
        _monitor.sendChecksNotRunToday()

        _monitor.mainBackFeed()

    End Sub
    <TestMethod()> Public Sub checkFeedForChecksGeneratedTest()
        _monitor.ConnectionString = sConnectionString

        _monitor.checkFeedForChecksGenerated()
        _monitor.checkStatusOfFeed()

    End Sub
    <TestMethod()> Public Sub checkTest()
        _monitor.ConnectionString = sConnectionString
        _monitor.mainBackFeed()

    End Sub
    Public Sub handleCheckBackEvent(vMonitor As clsAPBackfeedMonitor) Handles _monitor.CheckBackfeedEvent
        log.Debug("handleCheckBackFeedEvent checkTime:" + vMonitor.checkForUpdateTime.ToLongTimeString())
    End Sub

End Class