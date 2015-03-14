/* Copyright (C) 2012, Derp Ltd. All Rights Reserved. */

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace HidenSeq
{
    public partial class MainForm : Form
    {
        #region Win32 API Stuff
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        [DllImport("shell32.dll", CharSet=CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        /// Define our Constants we will use
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        #endregion

        // The constants we'll use to identify our custom system menu items
        public const Int32 _AboutSysMenuID = 1000;

        public MainForm()
        {
            InitializeComponent();
            InitSystemMenu();

            lst_files.MouseDoubleClick += new MouseEventHandler(lst_files_MouseDoubleClick);

            fileSystemWatcher.Path = GetDownloadFolder();
            fileSystemWatcher.Filter = "*.*";
            fileSystemWatcher.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
            fileSystemWatcher.Changed += new FileSystemEventHandler(fileSystemWatcher_Created);
            fileSystemWatcher.Renamed += new RenamedEventHandler(fileSystemWatcher_Renamed);

            bar_progress.Minimum = 0;
            bar_progress.Maximum = 100;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
        }

        void fileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string[] files = {e.FullPath};
            AddFiles(files);
        }

        void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            string[] files = {e.FullPath};
            AddFiles(files);
        }

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        void lst_files_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = lst_files.FocusedItem;
            HidenSeqFile hsfile = item.Tag as HidenSeqFile;

            if (hsfile.matches.Count == 0) return; 

            string r = "";
            r += "Time: " + System.DateTime.Now + "\r\n";
            r += "File: " + hsfile.filename + "\r\n";
            r += "Size: " + hsfile.length + "\r\n";
            r += "\r\n";
            r += hsfile.matches.Count + " Matches\r\n";
            r += "---------------------------------------------------\r\n";
            foreach(HidenMatch match in hsfile.matches.Values)
            {
                r += "(" + match.offset1 + "," + match.hash + "," + match.offset2 + ")\r\n";
            }
            r = "HidenSeq Match Evidence\n" + EncodeTo64(r);

            saveFileDialog.Filter = "HidenSeq Evidence|*.hse";
            saveFileDialog.DefaultExt = "hse";
            saveFileDialog.Title = "Save Evidence...";
            saveFileDialog.ValidateNames = true;
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = Path.GetFileName(hsfile.filename) + ".hse";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog.FileName);
                writer.Write(r);
                writer.Close();
            }
        }

        /// <summary>
        /// When work is completed update button and status messages
        /// </summary>
        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                status_txt.Text = "Canceled";
            }
            else if (e.Error != null)
            {
                status_txt.Text = "Error: " + e.Error.Message;
            }
            else
            {
                status_txt.Text = "Done";
            }
            bar_progress.Value = 0;
            btn_openfiles.Text = "...";
        }

        /// <summary>
        /// Handle progress events from Background Worker, updating list view with completed items.
        /// </summary>
        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            bar_progress.Value = e.ProgressPercentage;

            HidenSeqFile hsfile = e.UserState as HidenSeqFile;
            if (hsfile != null)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = hsfile;
                item.Text = hsfile.filename;
                item.SubItems.Add(string.Format("{0:0.0}mb", hsfile.length / 1024 / 1024));
                item.SubItems.Add(string.Format("{0} matches", hsfile.matches.Count));                
                lst_files.Items.Add(item);
            }
        }

        private void InitSystemMenu()
        {
            /// Get the Handle for the Forms System Menu
            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);

            /// Create our new System Menu items just before the Close menu item
            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, _AboutSysMenuID, "About...");
        }

        protected override void WndProc(ref Message m)
        {
            // Check if a System Command has been executed
            if (m.Msg == WM_SYSCOMMAND)
            {
                // Execute the appropriate code for the System Menu item that was clicked
                switch (m.WParam.ToInt32())
                {
                    case _AboutSysMenuID:
                        new AboutBox().ShowDialog();
                        break;
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Retrieves the Windows Vista+ 'Downloads' directory path
        /// </summary>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/windows/desktop/dd378457(v=vs.85).aspx</remarks>
        /// <returns>Directory path</returns>
        public string GetDownloadFolder()
        {
            string downloads;
            Guid DownloadsGuid = new Guid("374DE290-123F-4565-9164-39C4925E467B");
            SHGetKnownFolderPath(DownloadsGuid, 0, IntPtr.Zero, out downloads);
            return downloads;
        }

        private void AddFiles(string[] filenames)
        {
            List<string> filtered_filenames = new List<string>();
            foreach (string filename in filenames)
            {
                ListViewItem item = lst_files.FindItemWithText(filename);
                if (item != null)
                {
                    // Only ignore if it's the same size as previously scanned one
                    HidenSeqFile hsfile = item.Tag as HidenSeqFile;
                    FileInfo info = new FileInfo(filename);
                    if (hsfile.length == info.Length)
                    {
                        continue;
                    }
                }

                filtered_filenames.Add(filename);
            }
            backgroundWorker.RunWorkerAsync(filtered_filenames.ToArray());
        }

        private void btn_openfiles_Click(object sender, EventArgs e)
        {
            if ( backgroundWorker.IsBusy == false )
            {
                dlgOpenFile.InitialDirectory = GetDownloadFolder();
                dlgOpenFile.Filter = "Movie Files|*.flv;*.f4v;*.wmv;*.mpg;*.mpeg;*.avi;*.mp4;*.m4v|All files (*.*)|*.*";
                dlgOpenFile.RestoreDirectory = false;

                if (dlgOpenFile.ShowDialog() == DialogResult.OK)
                {
                    AddFiles(dlgOpenFile.FileNames);
                }
                btn_openfiles.Text = "Stop";
            }
            else
            {
                backgroundWorker.CancelAsync();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string[] filenames = e.Argument as string[];

            long total_bytes = 0;
            long current_bytes = 0;
            int current_progress = 0;

            foreach (string filename in filenames)
            {                
                FileInfo info = new FileInfo(filename);
                total_bytes += info.Length;
            }

            int i = 0;
            foreach (string filename in filenames)
            {
                i++;
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }

                Dictionary<uint, HidenMatch> matches = new Dictionary<uint, HidenMatch>();
                System.Text.Encoding encoding = new System.Text.ASCIIEncoding();
                int retry_count = 5;
                StreamReader stream = null;
                while (retry_count-- > 0)
                {
                    try
                    {
                        stream = new StreamReader(filename);
                        break;
                    }
                    catch (IOException _ex)
                    {
                        Thread.Sleep(2000);
                        try
                        {
                            stream = new StreamReader(filename);
                            break;
                        }
                        catch (IOException _ex2)
                        {
                            continue;
                        }
                    }
                }
                if (stream == null)
                {
                    continue;
                }

                BinaryReader reader = new BinaryReader(stream.BaseStream);

                status_txt.Text = i + " of " + filenames.Length + " - " + filename;

                // Try to find pattern in 512kb blocks.
                Regex rx = new Regex(@"[\x00]{4}(?<offset1>.{4})(?<hash>.{20})(?<offset2>.{4})[\x00]{4}",
                    RegexOptions.Singleline | RegexOptions.Compiled);
                long idx = 0;
                int readsize = 512 * 1024;
                while (idx < reader.BaseStream.Length)
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }

                    int bytes = Math.Min((int)(stream.BaseStream.Length - stream.BaseStream.Position), readsize);
                    byte[] buffer = reader.ReadBytes(bytes);

                    if (buffer.Length > 0)
                    {
                        MatchCollection rxmatches = rx.Matches(encoding.GetString(buffer));
                        if (rxmatches.Count > 0)
                        {
                            foreach (Match m in rxmatches)
                            {
                                if (worker.CancellationPending == true)
                                {
                                    e.Cancel = true;
                                    break;
                                }

                                GroupCollection group = m.Groups;
                                // Extract the offsets and hash from buffer using Regex Group index
                                uint offset1 = BitConverter.ToUInt32(buffer, group["offset1"].Index);
                                uint offset2 = BitConverter.ToUInt32(buffer, group["offset2"].Index);
                                string hexhash = BitConverter.ToString(buffer, group["hash"].Index, 20).Replace("-", "");

                                // Verify the crc checksum
                                ToolStackCRCLib.CRC32 crc32 = new ToolStackCRCLib.CRC32();
                                uint crccheck = crc32.crc(buffer, group["hash"].Length, (uint)group["hash"].Index);
                                if (offset2 == crccheck)
                                {
                                    matches[offset1] = new HidenMatch(offset1, offset2, hexhash);
                                }
                            }
                        }
                    } /* match buffer */

                    idx += buffer.Length;
                    current_bytes += buffer.Length;

                    current_progress = (int)(current_bytes / (total_bytes / 100));
                    backgroundWorker.ReportProgress(current_progress);
                } /* end I/O loop */

                // Don't add to list if user canceled processing.
                if (e.Cancel == false)
                {
                    HidenSeqFile hsfile = new HidenSeqFile(filename);
                    hsfile.matches = matches;
                    hsfile.length = reader.BaseStream.Length;
                    backgroundWorker.ReportProgress(current_progress, hsfile);
                }

                reader.Close();
                stream.Close();
            } /* end filename loop */
        }
    }
}
