namespace Poster
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.inputDir = new System.Windows.Forms.Button();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.indir = new System.Windows.Forms.TextBox();
            this.Picker = new System.Windows.Forms.Button();
            this.openFiler = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.Messages = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.VersionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputDir
            // 
            this.inputDir.Location = new System.Drawing.Point(13, 13);
            this.inputDir.Name = "inputDir";
            this.inputDir.Size = new System.Drawing.Size(85, 23);
            this.inputDir.TabIndex = 0;
            this.inputDir.Text = "Input Directory";
            this.inputDir.UseVisualStyleBackColor = true;
            this.inputDir.Click += new System.EventHandler(this.button1_Click);
            // 
            // indir
            // 
            this.indir.Location = new System.Drawing.Point(126, 16);
            this.indir.Name = "indir";
            this.indir.Size = new System.Drawing.Size(208, 20);
            this.indir.TabIndex = 1;
            // 
            // Picker
            // 
            this.Picker.Location = new System.Drawing.Point(13, 54);
            this.Picker.Name = "Picker";
            this.Picker.Size = new System.Drawing.Size(85, 23);
            this.Picker.TabIndex = 2;
            this.Picker.Text = " File Picker";
            this.Picker.UseVisualStyleBackColor = true;
            this.Picker.Click += new System.EventHandler(this.Picker_Click);
            // 
            // openFiler
            // 
            this.openFiler.FileName = "openFileDialog1";
            this.openFiler.Multiselect = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(126, 53);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Messages
            // 
            this.Messages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.Messages.Location = new System.Drawing.Point(0, 82);
            this.Messages.Multiline = true;
            this.Messages.Name = "Messages";
            this.Messages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Messages.Size = new System.Drawing.Size(346, 134);
            this.Messages.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.VersionLabel,
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 219);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(346, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // VersionLabel
            // 
            this.VersionLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(46, 17);
            this.VersionLabel.Text = "Version";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.VersionLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(39, 17);
            this.StatusLabel.Text = "Status";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 241);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.Messages);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Picker);
            this.Controls.Add(this.indir);
            this.Controls.Add(this.inputDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Data Poster";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button inputDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.TextBox indir;
        private System.Windows.Forms.Button Picker;
        private System.Windows.Forms.OpenFileDialog openFiler;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox Messages;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel VersionLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
    }
}

