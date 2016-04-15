using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace Poster
{
    public partial class Form1 : Form
    {
        private string _upto;
        private string _recent;
        private string _next;

        public string next
        {
            get { return _next; }
            set
            {
                button1.Visible = !string.IsNullOrWhiteSpace(value);
                _next = value;
                button1.Text = "Upload " + _next;
                button1.Refresh();
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            indir.Text = Properties.Settings.Default["uploadfrom"].ToString();
            _upto = Properties.Settings.Default["uploadto"].ToString();
            _recent = Properties.Settings.Default["recent"].ToString();
            next = Next(_recent);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default["uploadfrom"] = indir.Text;
            Properties.Settings.Default["recent"] = _recent; 
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

        private string Next(string recent)
        {
            var parts = Path.GetFileNameWithoutExtension(recent).Split('-');
            int julian = 1;
            if (parts.Length < 2)                       return recent;
            if (!int.TryParse(parts[0], out julian))    return recent;
            if (parts[1] == "4")
            {
                julian++;
                return julian.ToString().PadLeft(3, '0') + "-1";
            }
            int shift = 1;
            if (!int.TryParse(parts[1], out shift))     return recent;
            if (shift > 3) return recent;
            shift++;
            return julian.ToString().PadLeft(3, '0') + "-" + shift;
        }

        async void Upload(string[] files)
        {
            string last = "";

            HttpClient client = new HttpClient();

            Messages.AppendLine("File      \tCollected      \tSamples\n");

            foreach (var file in files)
            {
                var request = new MultipartFormDataContent();
                try
                {
                    var path = Path.Combine(indir.Text, file);
                    using (Stream fileStream = File.OpenRead(path))
                    {
                        request.Add(new StreamContent(fileStream), "file", file);
                        last = file;

                        var response = await client.PostAsync(_upto, request);
                        response.EnsureSuccessStatusCode();
                        var msg = await response.Content.ReadAsStringAsync();
                        Messages.AppendLine(file + "\t" + msg);
                    }
                }
                catch (Exception e)
                {
                    Messages.AppendLine(e.Message);
                }
            }
            Messages.AppendLine("Done.");

            _recent = Path.GetFileName(last);
            next = Next(_recent);
        }

        private void Picker_Click(object sender, EventArgs e)
        {
            openFiler.Title = "Pick file to upload and publish";
            openFiler.InitialDirectory = indir.Text;
            DialogResult dr = openFiler.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Upload(openFiler.SafeFileNames);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Upload(new string[] { _next });
        }
    }

    public static class WinFormsExtensions
    {
        public static void AppendLine(this TextBox source, string value)
        {
            var now = DateTime.Now.ToString("MM/dd HH:mm  ");
            if (source.Text.Length == 0)
                source.Text = now + value;
            else
                source.AppendText("\r\n" + now + value);
        }
    }
}
