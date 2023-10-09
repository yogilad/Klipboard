namespace Klipboard
{
    partial class InspectForm
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
            toolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripLabel2 = new ToolStripLabel();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // textBox
            // 
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Location = new Point(0, 28);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.ScrollBars = ScrollBars.Both;
            textBox.Size = new Size(800, 425);
            textBox.TabIndex = 0;
            textBox.WordWrap = false;
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripLabel1, toolStripSeparator3, toolStripLabel2, toolStripSeparator2 });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(800, 25);
            toolStrip.TabIndex = 1;
            toolStrip.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(86, 22);
            toolStripLabel1.Text = "toolStripLabel1";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(86, 22);
            toolStripLabel2.Text = "toolStripLabel2";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // InspectForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(toolStrip);
            Controls.Add(textBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InspectForm";
            Text = "Clipboard Content";
            Load += InspectForm_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox;
        private ToolStrip toolStrip;
        private ToolStripLabel toolStripLabel1;
        private ToolStripLabel toolStripLabel2;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
    }
}
