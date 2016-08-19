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
        public static string _version = "";

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
            VersionLabel.Text = _version;
            StatusLabel.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            indir.Text = Properties.Settings.Default["uploadfrom"].ToString();
            _upto = Properties.Settings.Default["uploadto"].ToString();
            _recent = Properties.Settings.Default["recent"].ToString();
            next = Next(_recent);
            Messages.AppendLine("File      \tCollected      \tSamples");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Properties.Settings.Default["uploadfrom"] = indir.Text;
            Properties.Settings.Default["recent"] = _recent;
            Properties.Settings.Default.Save();

            base.OnFormClosing(e);
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
            var ext = Path.GetExtension(recent);
            int julian = 1;
            if (parts.Length < 2)                       return recent;
            if (!int.TryParse(parts[0], out julian))    return recent;
            if (parts[1] == "4")
            {
                julian++;
                return julian.ToString().PadLeft(3, '0') + "-1" + ext;
            }
            int shift = 1;
            if (!int.TryParse(parts[1], out shift))     return recent;
            if (shift > 3) return recent;
            shift++;
            return julian.ToString().PadLeft(3, '0') + "-" + shift + ext;
        }

        private static StringComparer str = StringComparer.OrdinalIgnoreCase;

        async void Upload(string[] files)
        {
            string last = "";

            HttpClient client = new HttpClient(new HttpClientHandler() {
                UseDefaultCredentials = true
            });

            StatusLabel.Text = "uploading " + files.Length + " file" + (files.Length != 1 ? "s": "");

            var oldback = BackColor;
            BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.Enable(false);
            Messages.Enable(true);
            statusStrip1.Enable(true);

            var count = 0;
            foreach (var file in files)
            {
                var request = new MultipartFormDataContent();
                try
                {
                    var path = Path.Combine(indir.Text, file);
                    using (Stream fileStream = File.OpenRead(path))
                    {
                        request.Add(new StreamContent(fileStream), "file", file);
                        if (str.Compare(file, last) > 0) last = file;

                        var response = await client.PostAsync(_upto, request);
                        response.EnsureSuccessStatusCode();
                        count++;
                        StatusLabel.Text = "awaiting "+_upto+" for " + file + " (" + count + "/" + files.Length + " file" + (files.Length != 1 ? "s)" : ")");
                        statusStrip1.Refresh();
                        var msg = await response.Content.ReadAsStringAsync();
                        Messages.AppendLine(file + "\t" + msg);
                    }
                }
                catch (Exception e)
                {
                    Messages.AppendLine(file +": "+ e.Message + ", while uploading to " + _upto);
                }
            }
            this.Enable(true);
            BackColor = oldback;
            StatusLabel.Text = "uploaded " + count + " file" + (count != 1 ? "s" : "");

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
        public static void Enable(this Control con, bool enable)
        {
            if (con == null)
                return;

            foreach (Control c in con.Controls)
                c.Enable(enable);

            try
            {
                con.Invoke((MethodInvoker)(() => con.Enabled = enable));
            }
            catch
            {
            }
        }

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
