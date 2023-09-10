namespace Klipboard
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lstClusters = new ListView();
            clmConnectionString = new ColumnHeader();
            clmDb = new ColumnHeader();
            txtSettingsPath = new TextBox();
            lblSettingsPath = new Label();
            btnLoadSettings = new Button();
            chkStartWithWindows = new CheckBox();
            txtConnectionStr = new TextBox();
            txtDatabase = new TextBox();
            label1 = new Label();
            label2 = new Label();
            btUpdate = new Button();
            btnAdd = new Button();
            cmbApp = new ComboBox();
            lblOpenWith = new Label();
            txtQuery = new TextBox();
            lblPrepend = new Label();
            btnSave = new Button();
            SuspendLayout();
            // 
            // lstClusters
            // 
            lstClusters.AllowColumnReorder = true;
            lstClusters.Columns.AddRange(new ColumnHeader[] { clmConnectionString, clmDb });
            lstClusters.FullRowSelect = true;
            lstClusters.GridLines = true;
            lstClusters.Location = new Point(22, 75);
            lstClusters.MultiSelect = false;
            lstClusters.Name = "lstClusters";
            lstClusters.Size = new Size(825, 383);
            lstClusters.TabIndex = 0;
            lstClusters.UseCompatibleStateImageBehavior = false;
            lstClusters.View = View.Details;
            lstClusters.SelectedIndexChanged += lstClusters_SelectedIndexChanged;
            // 
            // clmConnectionString
            // 
            clmConnectionString.Text = "Connection String";
            clmConnectionString.Width = 700;
            // 
            // clmDb
            // 
            clmDb.Text = "DataBase";
            clmDb.Width = 100;
            // 
            // txtSettingsPath
            // 
            txtSettingsPath.Location = new Point(78, 12);
            txtSettingsPath.Name = "txtSettingsPath";
            txtSettingsPath.Size = new Size(691, 31);
            txtSettingsPath.TabIndex = 1;
            // 
            // lblSettingsPath
            // 
            lblSettingsPath.AutoSize = true;
            lblSettingsPath.Location = new Point(22, 15);
            lblSettingsPath.Name = "lblSettingsPath";
            lblSettingsPath.Size = new Size(50, 25);
            lblSettingsPath.TabIndex = 2;
            lblSettingsPath.Text = "Path:";
            // 
            // btnLoadSettings
            // 
            btnLoadSettings.Location = new Point(775, 10);
            btnLoadSettings.Name = "btnLoadSettings";
            btnLoadSettings.Size = new Size(112, 34);
            btnLoadSettings.TabIndex = 3;
            btnLoadSettings.Text = "Load";
            btnLoadSettings.UseVisualStyleBackColor = true;
            btnLoadSettings.Click += btnLoadSettings_Click;
            // 
            // chkStartWithWindows
            // 
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new Point(22, 607);
            chkStartWithWindows.Name = "chkStartWithWindows";
            chkStartWithWindows.Size = new Size(195, 29);
            chkStartWithWindows.TabIndex = 6;
            chkStartWithWindows.Text = "Start With Windows";
            chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // txtConnectionStr
            // 
            txtConnectionStr.Location = new Point(155, 485);
            txtConnectionStr.Name = "txtConnectionStr";
            txtConnectionStr.Size = new Size(536, 31);
            txtConnectionStr.TabIndex = 7;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(155, 533);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(268, 31);
            txtDatabase.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 488);
            label1.Name = "label1";
            label1.Size = new Size(132, 25);
            label1.TabIndex = 9;
            label1.Text = "Connection Str:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 536);
            label2.Name = "label2";
            label2.Size = new Size(90, 25);
            label2.TabIndex = 10;
            label2.Text = "Database:";
            // 
            // btUpdate
            // 
            btUpdate.Location = new Point(451, 531);
            btUpdate.Name = "btUpdate";
            btUpdate.Size = new Size(112, 34);
            btUpdate.TabIndex = 11;
            btUpdate.Text = "Update";
            btUpdate.UseVisualStyleBackColor = true;
            btUpdate.Click += btUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(579, 530);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(112, 34);
            btnAdd.TabIndex = 12;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // cmbApp
            // 
            cmbApp.FormattingEnabled = true;
            cmbApp.Location = new Point(381, 605);
            cmbApp.Name = "cmbApp";
            cmbApp.Size = new Size(182, 33);
            cmbApp.TabIndex = 13;
            // 
            // lblOpenWith
            // 
            lblOpenWith.AutoSize = true;
            lblOpenWith.Location = new Point(273, 611);
            lblOpenWith.Name = "lblOpenWith";
            lblOpenWith.Size = new Size(102, 25);
            lblOpenWith.TabIndex = 14;
            lblOpenWith.Text = "Open With:";
            // 
            // txtQuery
            // 
            txtQuery.Location = new Point(155, 688);
            txtQuery.Multiline = true;
            txtQuery.Name = "txtQuery";
            txtQuery.Size = new Size(473, 225);
            txtQuery.TabIndex = 15;
            // 
            // lblPrepend
            // 
            lblPrepend.AutoSize = true;
            lblPrepend.Location = new Point(22, 691);
            lblPrepend.Name = "lblPrepend";
            lblPrepend.Size = new Size(119, 25);
            lblPrepend.TabIndex = 16;
            lblPrepend.Text = "Prepend KQL:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(657, 611);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(190, 94);
            btnSave.TabIndex = 17;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(892, 1026);
            Controls.Add(btnSave);
            Controls.Add(lblPrepend);
            Controls.Add(txtQuery);
            Controls.Add(lblOpenWith);
            Controls.Add(cmbApp);
            Controls.Add(btnAdd);
            Controls.Add(btUpdate);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtDatabase);
            Controls.Add(txtConnectionStr);
            Controls.Add(chkStartWithWindows);
            Controls.Add(btnLoadSettings);
            Controls.Add(lblSettingsPath);
            Controls.Add(txtSettingsPath);
            Controls.Add(lstClusters);
            Name = "Settings";
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView lstClusters;
        private ColumnHeader clmConnectionString;
        private ColumnHeader clmDb;
        private TextBox txtSettingsPath;
        private Label lblSettingsPath;
        private Button btnLoadSettings;
        private CheckBox chkStartWithWindows;
        private TextBox txtConnectionStr;
        private TextBox txtDatabase;
        private Label label1;
        private Label label2;
        private Button btUpdate;
        private Button btnAdd;
        private ComboBox cmbApp;
        private Label lblOpenWith;
        private TextBox txtQuery;
        private Label lblPrepend;
        private Button btnSave;
    }
}