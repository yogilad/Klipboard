namespace Klipboard.WorkersUi.QuickActionsUiWorker
{
    partial class QuickActionsTargetSelector
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
            label1 = new Label();
            label2 = new Label();
            cancelButton = new Button();
            selectButton = new Button();
            databaseTextBox = new TextBox();
            clusterComboBox = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(32, 23);
            label1.Name = "label1";
            label1.Size = new Size(44, 15);
            label1.TabIndex = 0;
            label1.Text = "Cluster";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(32, 60);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 1;
            label2.Text = "Database";
            // 
            // cancelButton
            // 
            cancelButton.Location = new Point(459, 63);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += closeButton_Click;
            // 
            // selectButton
            // 
            selectButton.Location = new Point(378, 63);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(75, 23);
            selectButton.TabIndex = 3;
            selectButton.Text = "Select";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += selectButton_Click;
            // 
            // databaseTextBox
            // 
            databaseTextBox.Location = new Point(93, 52);
            databaseTextBox.Name = "databaseTextBox";
            databaseTextBox.Size = new Size(262, 23);
            databaseTextBox.TabIndex = 4;
            databaseTextBox.TextChanged += databaseNameTextBox_TextChanged;
            // 
            // clusterComboBox
            // 
            clusterComboBox.FormattingEnabled = true;
            clusterComboBox.Location = new Point(93, 20);
            clusterComboBox.Name = "clusterComboBox";
            clusterComboBox.Size = new Size(441, 23);
            clusterComboBox.TabIndex = 5;
            clusterComboBox.SelectedIndexChanged += clusterComboBox_SelectedIndexChanged;
            // 
            // QuickActionsTargetSelector
            // 
            AcceptButton = selectButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(546, 98);
            Controls.Add(clusterComboBox);
            Controls.Add(databaseTextBox);
            Controls.Add(selectButton);
            Controls.Add(cancelButton);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "QuickActionsTargetSelector";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Select Default Cluster and Database for Quick Actions";
            Load += QuickActionsTargetSelector_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Button cancelButton;
        private Button selectButton;
        private TextBox databaseTextBox;
        private ComboBox clusterComboBox;
    }
}