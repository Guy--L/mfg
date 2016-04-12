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
            this.indir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.indir.Location = new System.Drawing.Point(126, 16);
            this.indir.Name = "indir";
            this.indir.Size = new System.Drawing.Size(208, 20);
            this.indir.TabIndex = 1;
            // 
            // Picker
            // 
            this.Picker.Location = new System.Drawing.Point(13, 59);
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
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 241);
            this.Controls.Add(this.Picker);
            this.Controls.Add(this.indir);
            this.Controls.Add(this.inputDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Data Poster";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button inputDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.TextBox indir;
        private System.Windows.Forms.Button Picker;
        private System.Windows.Forms.OpenFileDialog openFiler;
    }
}

