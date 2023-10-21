namespace Klipboard
{
    partial class TextViewForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox = new TextBox();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // textBox
            // 
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Location = new Point(0, 1);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Size = new Size(800, 425);
            textBox.TabIndex = 0;
            textBox.WordWrap = false;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(800, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 1;
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(81, 17);
            toolStripStatusLabel.Text = "Status Not Set";
            // 
            // TextViewForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(statusStrip);
            Controls.Add(textBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TextViewForm";
            Text = "Title Not Set";
            Load += InspectForm_Load;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel;
    }
}
