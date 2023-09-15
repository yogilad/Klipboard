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
            lstClusters.Location = new Point(15, 45);
            lstClusters.Margin = new Padding(2);
            lstClusters.MultiSelect = false;
            lstClusters.Name = "lstClusters";
            lstClusters.Size = new Size(579, 231);
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
            txtSettingsPath.Location = new Point(55, 7);
            txtSettingsPath.Margin = new Padding(2);
            txtSettingsPath.Name = "txtSettingsPath";
            txtSettingsPath.Size = new Size(485, 23);
            txtSettingsPath.TabIndex = 1;
            // 
            // lblSettingsPath
            // 
            lblSettingsPath.AutoSize = true;
            lblSettingsPath.Location = new Point(15, 9);
            lblSettingsPath.Margin = new Padding(2, 0, 2, 0);
            lblSettingsPath.Name = "lblSettingsPath";
            lblSettingsPath.Size = new Size(34, 15);
            lblSettingsPath.TabIndex = 2;
            lblSettingsPath.Text = "Path:";
            // 
            // btnLoadSettings
            // 
            btnLoadSettings.Location = new Point(542, 6);
            btnLoadSettings.Margin = new Padding(2);
            btnLoadSettings.Name = "btnLoadSettings";
            btnLoadSettings.Size = new Size(78, 20);
            btnLoadSettings.TabIndex = 3;
            btnLoadSettings.Text = "Load";
            btnLoadSettings.UseVisualStyleBackColor = true;
            btnLoadSettings.Click += btnLoadSettings_Click;
            // 
            // chkStartWithWindows
            // 
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new Point(15, 364);
            chkStartWithWindows.Margin = new Padding(2);
            chkStartWithWindows.Name = "chkStartWithWindows";
            chkStartWithWindows.Size = new Size(130, 19);
            chkStartWithWindows.TabIndex = 6;
            chkStartWithWindows.Text = "Start With Windows";
            chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // txtConnectionStr
            // 
            txtConnectionStr.Location = new Point(108, 291);
            txtConnectionStr.Margin = new Padding(2);
            txtConnectionStr.Name = "txtConnectionStr";
            txtConnectionStr.Size = new Size(376, 23);
            txtConnectionStr.TabIndex = 7;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(108, 320);
            txtDatabase.Margin = new Padding(2);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(189, 23);
            txtDatabase.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 293);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(89, 15);
            label1.TabIndex = 9;
            label1.Text = "Connection Str:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 322);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(58, 15);
            label2.TabIndex = 10;
            label2.Text = "Database:";
            // 
            // btUpdate
            // 
            btUpdate.Location = new Point(316, 319);
            btUpdate.Margin = new Padding(2);
            btUpdate.Name = "btUpdate";
            btUpdate.Size = new Size(78, 20);
            btUpdate.TabIndex = 11;
            btUpdate.Text = "Update";
            btUpdate.UseVisualStyleBackColor = true;
            btUpdate.Click += btUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(405, 318);
            btnAdd.Margin = new Padding(2);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(78, 20);
            btnAdd.TabIndex = 12;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // cmbApp
            // 
            cmbApp.FormattingEnabled = true;
            cmbApp.Location = new Point(267, 363);
            cmbApp.Margin = new Padding(2);
            cmbApp.Name = "cmbApp";
            cmbApp.Size = new Size(129, 23);
            cmbApp.TabIndex = 13;
            // 
            // lblOpenWith
            // 
            lblOpenWith.AutoSize = true;
            lblOpenWith.Location = new Point(191, 367);
            lblOpenWith.Margin = new Padding(2, 0, 2, 0);
            lblOpenWith.Name = "lblOpenWith";
            lblOpenWith.Size = new Size(67, 15);
            lblOpenWith.TabIndex = 14;
            lblOpenWith.Text = "Open With:";
            // 
            // txtQuery
            // 
            txtQuery.Location = new Point(108, 413);
            txtQuery.Margin = new Padding(2);
            txtQuery.Multiline = true;
            txtQuery.Name = "txtQuery";
            txtQuery.Size = new Size(332, 137);
            txtQuery.TabIndex = 15;
            // 
            // lblPrepend
            // 
            lblPrepend.AutoSize = true;
            lblPrepend.Location = new Point(15, 415);
            lblPrepend.Margin = new Padding(2, 0, 2, 0);
            lblPrepend.Name = "lblPrepend";
            lblPrepend.Size = new Size(79, 15);
            lblPrepend.TabIndex = 16;
            lblPrepend.Text = "Prepend KQL:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(460, 367);
            btnSave.Margin = new Padding(2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(133, 56);
            btnSave.TabIndex = 17;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 616);
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
            Margin = new Padding(2);
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