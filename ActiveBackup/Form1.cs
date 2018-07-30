using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ActiveBackup
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        FolderBrowserDialog fbd = new FolderBrowserDialog();

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Add(fbd.SelectedPath);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<String> paths = new List<String>();
            foreach (String path in listBox1.Items)
            {
                paths.Add(path);
            }
            if (SaveContent("checkedfolders.txt", paths))
            {
                MessageBox.Show("Modifications saved!", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("An error occured. Saving failed!", "Error", MessageBoxButtons.OK);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                listBox2.Items.Add(ofd.FileName);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            List<String> whitelisted = new List<String>();
            foreach (String path in listBox2.Items)
            {
                whitelisted.Add(path);
            }
            if (SaveContent("filestokeep.txt", whitelisted))
            {
                MessageBox.Show("Modifications saved!", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("An error occured. Saving failed!", "Error", MessageBoxButtons.OK);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
            }
            if (Directory.Exists(textBox1.Text))
            {
                List<String> path = new List<string>();
                path.Add(textBox1.Text);
                Environment.SetEnvironmentVariable("backuppath", textBox1.Text);
                SaveContent("backuplocation.txt", path);
            }
        }

        private bool SaveContent(String fileName, List<String> paths)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                System.IO.File.WriteAllLines(fileName, paths);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void LoadSettings()
        {
            if (File.Exists("checkedfolders.txt"))
            {
                foreach (String line in File.ReadAllLines("checkedfolders.txt"))
                {
                    listBox1.Items.Add(line);
                }
            }
            if (File.Exists("filestokeep.txt"))
            {
                foreach (String line in File.ReadAllLines("filestokeep.txt"))
                {
                    listBox2.Items.Add(line);
                }
            }
            if (File.Exists("backuplocation.txt"))
            {
                textBox1.Text = File.ReadAllLines("backuplocation.txt")[0];
                Environment.SetEnvironmentVariable("backuppath", textBox1.Text);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (Environment.GetEnvironmentVariable("backuppath") != null && listBox1.Items.Count > 0)
            {
                Thread watchThread = new Thread(new ThreadStart(this.WatchThread));
                watchThread.Start();
            }
            else
            {
                MessageBox.Show("Backup location or folders to watch not configured!", "Error", MessageBoxButtons.OK);
            }
        }

        private void WatchThread()
        {
            while (true)
            {
                TimeSpan timespan = new TimeSpan(0, 0, 10);
                DriveInfo kDrive = new DriveInfo("K");
                setPercentage(((kDrive.AvailableFreeSpace / (float)kDrive.TotalSize) * 100).ToString());
                if (Convert.ToDouble(textBox2.Text) < 25)
                {
                    StartBackup();
                }
                Thread.Sleep(timespan);
            }
        }

        delegate void setPercentageCallback(string text);

        private void setPercentage(string text)
        {
            if (this.textBox2.InvokeRequired)
            {
                setPercentageCallback x = new setPercentageCallback(setPercentage);
                this.Invoke(x, new object[] { text });
            }
            else
            {
                this.textBox2.Text = text;
            }
        }

        private void StartBackup()
        {
            foreach(String path in listBox1.Items)
            {
                foreach(String file in Directory.GetFiles(path))
                {
                    if (!listBox2.Items.Contains(file))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        String fileName = Path.GetFileName(file);
                        File.Move(file, Environment.GetEnvironmentVariable("backuppath") + @"\" +  fileName);
                    }
                }
            }
        }
    }
}

