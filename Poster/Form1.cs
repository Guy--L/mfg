using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Poster
{
    public partial class Form1 : Form
    {
        private string _indir;
        private string _upto;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            indir.Text = Properties.Settings.Default["uploadfrom"].ToString();
            _upto = Properties.Settings.Default["uploadto"].ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default["uploadfrom"] = indir.Text;
            Properties.Settings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                indir.Text = folderBrowser.SelectedPath;

                bool exists = Directory.Exists(indir.Text);
                if (!exists) Directory.CreateDirectory(indir.Text);
            }
        }

        private void Picker_Click(object sender, EventArgs e)
        {
            openFiler.InitialDirectory = indir.Text;
            DialogResult dr = openFiler.ShowDialog();
            if (dr == DialogResult.OK)
            {
                indir.Text = openFiler.FileName;

                bool exists = Directory.Exists(indir.Text);
                if (!exists) Directory.CreateDirectory(indir.Text);
            }
        }
    }
}
