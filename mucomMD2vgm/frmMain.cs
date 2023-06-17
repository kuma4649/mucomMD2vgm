using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Core;

namespace mucomMD2vgm
{
    public partial class FrmMain : Form
    {
        private string[] args;
        private MucomMD2vgm mv = null;
        private string title = "";
        private FileSystemWatcher watcher = null;
        private long now = 0;
        private bool isSuccess = true;
        private bool isLoopEx = false;

        public FrmMain()
        {
            InitializeComponent();
#if DEBUG
            //Core.log.debug = true;
#endif
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            Core.Common.CheckSoXVersion(System.Windows.Forms.Application.StartupPath, Disp);
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                this.args = args;
                TsbCompile_Click(null, null);
            }
        }

        private void TsbCompile_Click(object sender, EventArgs e)
        {
            if (args == null || args.Length < 2)
            {
                return;
            }

            this.toolStrip1.Enabled = false;
            this.tsslMessage.Text = msg.get("I0100");
            dgvResult.Rows.Clear();

            textBox1.AppendText(msg.get("I0101"));
            textBox1.AppendText(msg.get("I0102"));

            isSuccess = true;
            Thread trdStartCompile = new(new ThreadStart(StartCompile));
            trdStartCompile.Start();

        }

        private void TsbOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Multiselect = true,
                Filter = msg.get("I0103"),
                Title = msg.get("I0104")
            };
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            args = null;
            List<string> a = new()
            {
                "dummy"
            };
            foreach (string fn in ofd.FileNames)
            {
                a.Add(fn);
            }
            args = a.ToArray();
            if (tsbWatcher.Checked)
            {
                StopWatch();
            }

            TsbCompile_Click(null, null);

            if (tsbWatcher.Checked)
            {
                StartWatch();
            }
        }

        private void UpdateTitle()
        {
            if (title == "")
            {
                this.Text = "mucomMD2vgm";
            }
            else
            {
                this.Text = string.Format("mucomMD2vgm - {0}", title);
            }
        }

        private void FinishedCompile()
        {
            if (mv == null)
            {
                textBox1.AppendText(msg.get("I0105"));
                this.toolStrip1.Enabled = true;
                this.tsslMessage.Text = msg.get("I0106");
                return;
            }

            if (isSuccess)
            {
                foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in mv.desVGM.chips)
                {
                    foreach (ClsChip chip in kvp.Value)
                    {
                        if (chip == null) continue;
                        List<partWork> pw = chip.lstPartWork;
                        for (int i = 0; i < pw.Count; i++)
                        {
                            if (pw[i].clockCounter == 0) continue;

                            DataGridViewRow row = new();
                            row.Cells.Add(new DataGridViewTextBoxCell());
                            row.Cells.Add(new DataGridViewTextBoxCell());
                            row.Cells.Add(new DataGridViewTextBoxCell());
                            row.Cells.Add(new DataGridViewTextBoxCell());
                            row.Cells[0].Value = pw[i].PartName;
                            row.Cells[1].Value = pw[i].chip.Name.ToUpper();
                            row.Cells[2].Value = pw[i].loopInfo.use ? pw[i].loopInfo.totalCounter : pw[i].clockCounter;
                            if (mv.desVGM.isLoopEx)
                            {
                                row.Cells[3].Value = pw[i].loopInfo.use ? pw[i].loopInfo.loopCounter.ToString() : "-";
                            }
                            else
                            {
                                row.Cells[3].Value = "not in use(LoopEx mode only.)";
                            }
                            //row.Cells.Add(new DataGridViewTextBoxCell());
                            //row.Cells[4].Value = pw[i].totalSamples;
                            //row.Cells.Add(new DataGridViewTextBoxCell());
                            //row.Cells[5].Value = pw[i].loopInfo.use ? pw[i].loopInfo.length.ToString() : "none";
                            dgvResult.Rows.Add(row);
                        }
                    }
                }
            }

            textBox1.AppendText(msg.get("I0107"));

            foreach (string mes in msgBox.getWrn())
            {
                textBox1.AppendText(string.Format(msg.get("I0108"), mes));
            }

            foreach (string mes in msgBox.getErr())
            {
                textBox1.AppendText(string.Format(msg.get("I0109"), mes));
            }

            textBox1.AppendText("\r\n");
            textBox1.AppendText(string.Format(msg.get("I0110"), msgBox.getErr().Length, msgBox.getWrn().Length));

            if (isSuccess)
            {
                if (mv.desVGM.loopSamples != -1)
                {
                    textBox1.AppendText(string.Format(msg.get("I0111"), mv.desVGM.loopClock));
                    textBox1.AppendText(string.Format(msg.get("I0112")
                        , mv.desVGM.loopSamples
                        , mv.desVGM.loopSamples / 44100L));
                }

                textBox1.AppendText(string.Format(msg.get("I0113"), mv.desVGM.lClock));
                textBox1.AppendText(string.Format(msg.get("I0114")
                    , mv.desVGM.dSample
                    , mv.desVGM.dSample / 44100L));
            }

            textBox1.AppendText(msg.get("I0126"));
            this.toolStrip1.Enabled = true;
            this.tsslMessage.Text = msg.get("I0106");

            if (isSuccess)
            {
                if (args.Length == 2 && tsbOnPlay.Checked)
                {
                    try
                    {
                        if (mv.desVGM.info.format == enmFormat.VGM)
                        {
                            if (!tsbToVGZ.Checked)
                                Process.Start(Path.ChangeExtension(args[1], Properties.Resources.ExtensionVGM));
                            else
                                Process.Start(Path.ChangeExtension(args[1], Properties.Resources.ExtensionVGZ));
                        }
                        else
                        {
                            if (!tsbToVGZ.Checked)
                                Process.Start(Path.ChangeExtension(args[1], Properties.Resources.ExtensionXGM));
                            else
                                Process.Start(Path.ChangeExtension(args[1], Properties.Resources.ExtensionXGZ));
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(msg.get("E0100"), "mucomMD2vgm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void StartCompile()
        {
            //Core.log.debug = true;
            Core.Log.Open();
            Core.Log.Write("start compile thread");

            Action dmy = UpdateTitle;
            string stPath = System.Windows.Forms.Application.StartupPath;
            if (!int.TryParse(tstbMaxRendering.Text, out int rendSecond))
            {
                rendSecond = 600;
            }

            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i];
                if (!File.Exists(arg))
                {
                    continue;
                }


                title = Path.GetFileName(arg);
                this.Invoke(dmy);

                Core.Log.Write(string.Format("  compile at [{0}]", args[i]));

                msgBox.clear();

                string desfn = Path.ChangeExtension(arg, Properties.Resources.ExtensionVGM);
                if (tsbToVGZ.Checked)
                {
                    desfn = Path.ChangeExtension(arg, Properties.Resources.ExtensionVGZ);
                }

                Core.Log.Write("Call mucomMD2vgm core");

                mv = new MucomMD2vgm(arg, desfn, stPath, Disp, isLoopEx, rendSecond);
                if (mv.Start() != 0)
                {
                    isSuccess = false;
                    break;
                }

                Core.Log.Write("Return mucomMD2vgm core");
            }

            Core.Log.Write("Disp Result");

                dmy = FinishedCompile;
                this.Invoke(dmy);

            Core.Log.Write("end compile thread");
            Core.Log.Close();
        }

        private void Disp(string msg)
        {
            Action<string> msgDisp = MsgDisp;
            this.Invoke(msgDisp, msg);
            Core.Log.Write(msg);
        }

        private void MsgDisp(string msg)
        {
            textBox1.AppendText(msg + "\r\n");
        }

        private void StartWatch()
        {
            if (watcher != null) return;

            watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(args[1]),
                NotifyFilter =
                (
                System.IO.NotifyFilters.LastAccess
                | System.IO.NotifyFilters.LastWrite
                | System.IO.NotifyFilters.FileName
                | System.IO.NotifyFilters.DirectoryName
                ),
                Filter = Path.GetFileName(args[1]),
                SynchronizingObject = this
            };

            watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
            watcher.Created += new FileSystemEventHandler(Watcher_Changed);

            watcher.EnableRaisingEvents = true;
        }

        private void StopWatch()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }

        private void Watcher_Changed(System.Object source, System.IO.FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case System.IO.WatcherChangeTypes.Changed:
                case System.IO.WatcherChangeTypes.Created:

                    long n = DateTime.Now.Ticks / 10000000L;
                    if (now == n) return;
                    now = n;
                    TsbCompile_Click(null, null);
                    break;
            }
        }

        private void TsbWatcher_CheckedChanged(object sender, EventArgs e)
        {
            if (args == null || args.Length < 2)
            {
                tsbWatcher.Checked = false;
                return;
            }

            if (tsbWatcher.Checked)
            {
                StartWatch();
                return;
            }

            StopWatch();
        }

        private void TsbLoopEx_Click(object sender, EventArgs e)
        {
            isLoopEx = tsbLoopEx.Checked;
            tslMaxRendering.Enabled = tsbLoopEx.Checked;
            tslSecond.Enabled = tsbLoopEx.Checked;
            tstbMaxRendering.Enabled = tsbLoopEx.Checked;
        }

        private void TstbMaxRendering_TextChanged(object sender, EventArgs e)
        {
            string s= tstbMaxRendering.Text;
            if (!int.TryParse(s, out int i))
            {
                tstbMaxRendering.Text = "600";
                return;
            }
            if (i < 1)
            {
                tstbMaxRendering.Text = "1";
            }
        }

        private void FrmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach(string fn in fileNames)
            {
                string ext = Path.GetExtension(fn).ToLower();
                if (ext == ".muc") continue;
                if (ext == ".mum") continue;
                e.Effect = DragDropEffects.None;

                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        private void FrmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string fn in fileNames)
            {
                string ext = Path.GetExtension(fn).ToLower();
                if (ext == ".muc") continue;
                if (ext == ".mum") continue;

                if (fileNames.Length < 1)
                    MessageBox.Show(msg.get("E0101"));
                else
                    MessageBox.Show(msg.get("E0102"));

                return;
            }

            List<string> fs = new()
            {
                ""//dummy;
            };
            fs.AddRange(fileNames);
            args = fs.ToArray();

            TsbCompile_Click(null, null);
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    TsbOpen_Click(null, null);
                    break;
                //case Keys.F2:
                //break;
                //case Keys.S:
                //break;
                case Keys.F5:
                    TsbCompile_Click(null, null);
                    break;
                default:
                    //↓KeyData確認用
                    //log.Write(string.Format("動作未定義のキー：{0}",e.KeyData));
                    break;
            }
        }
    }

}
