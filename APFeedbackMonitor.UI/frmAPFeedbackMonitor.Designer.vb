<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAPFeedbackMonitor
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.lblTimerTicked = New System.Windows.Forms.Label()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblStatus2 = New System.Windows.Forms.Label()
        Me.lblStatus3 = New System.Windows.Forms.Label()
        Me.lblStatus4 = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnStart
        '
        Me.btnStart.Location = New System.Drawing.Point(7, 29)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(58, 23)
        Me.btnStart.TabIndex = 0
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuExit})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(262, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'mnuExit
        '
        Me.mnuExit.Name = "mnuExit"
        Me.mnuExit.Size = New System.Drawing.Size(37, 20)
        Me.mnuExit.Text = "Exit"
        '
        'lblTimerTicked
        '
        Me.lblTimerTicked.AutoSize = True
        Me.lblTimerTicked.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblTimerTicked.CausesValidation = False
        Me.lblTimerTicked.Location = New System.Drawing.Point(12, 64)
        Me.lblTimerTicked.Name = "lblTimerTicked"
        Me.lblTimerTicked.Size = New System.Drawing.Size(12, 15)
        Me.lblTimerTicked.TabIndex = 2
        Me.lblTimerTicked.Text = "."
        '
        'btnStop
        '
        Me.btnStop.Location = New System.Drawing.Point(71, 29)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(52, 23)
        Me.btnStop.TabIndex = 3
        Me.btnStop.Text = "Stop"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblStatus.CausesValidation = False
        Me.lblStatus.Location = New System.Drawing.Point(12, 89)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(12, 15)
        Me.lblStatus.TabIndex = 4
        Me.lblStatus.Text = "."
        '
        'lblStatus2
        '
        Me.lblStatus2.AutoSize = True
        Me.lblStatus2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblStatus2.CausesValidation = False
        Me.lblStatus2.Location = New System.Drawing.Point(12, 114)
        Me.lblStatus2.Name = "lblStatus2"
        Me.lblStatus2.Size = New System.Drawing.Size(12, 15)
        Me.lblStatus2.TabIndex = 5
        Me.lblStatus2.Text = "."
        '
        'lblStatus3
        '
        Me.lblStatus3.AutoSize = True
        Me.lblStatus3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblStatus3.CausesValidation = False
        Me.lblStatus3.Location = New System.Drawing.Point(12, 138)
        Me.lblStatus3.Name = "lblStatus3"
        Me.lblStatus3.Size = New System.Drawing.Size(12, 15)
        Me.lblStatus3.TabIndex = 6
        Me.lblStatus3.Text = "."
        '
        'lblStatus4
        '
        Me.lblStatus4.AutoSize = True
        Me.lblStatus4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblStatus4.CausesValidation = False
        Me.lblStatus4.Location = New System.Drawing.Point(12, 162)
        Me.lblStatus4.Name = "lblStatus4"
        Me.lblStatus4.Size = New System.Drawing.Size(12, 15)
        Me.lblStatus4.TabIndex = 7
        Me.lblStatus4.Text = "."
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.Location = New System.Drawing.Point(154, 38)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(10, 13)
        Me.lblVersion.TabIndex = 8
        Me.lblVersion.Text = "."
        '
        'frmAPFeedbackMonitor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(262, 182)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblStatus4)
        Me.Controls.Add(Me.lblStatus3)
        Me.Controls.Add(Me.lblStatus2)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.lblTimerTicked)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmAPFeedbackMonitor"
        Me.Text = "AP Feedback Monitor"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents lblTimerTicked As System.Windows.Forms.Label
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lblStatus2 As System.Windows.Forms.Label
    Friend WithEvents lblStatus3 As System.Windows.Forms.Label
    Friend WithEvents lblStatus4 As System.Windows.Forms.Label
    Friend WithEvents lblVersion As System.Windows.Forms.Label

End Class
