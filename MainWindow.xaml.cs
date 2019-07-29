
using ExportWadLol.Helpers.Utilities;
using ExportWadLol.IO.WAD;
using ExportWadLol.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExportWadLol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string[] inputfiles;
        int fileIndex = 0;
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                fileIndex = 0;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                inputfiles = files;
                ExtractFile(inputfiles[fileIndex++]);

            }
        }

        private void ExtractFile(string path)
        {
            try
            {
                var info = new FileInfo(path);
                var Wad = new WADFile(path);
                var stringDictionary  = new Dictionary<ulong, string>();
                WADHashGenerator.GenerateWADStrings( Wad, stringDictionary);
                var worker = ExtractWADEntries(info.Directory.FullName+"/"+ System.IO.Path.GetFileNameWithoutExtension(path), Wad.Entries, stringDictionary);
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            }
            catch (Exception excp)
            {
                return;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            if (fileIndex >= inputfiles.Length)
            {
                MessageBox.Show(this,"Complete!!");
                return;
            }
            ExtractFile(inputfiles[fileIndex++]);
        }

        private BackgroundWorker ExtractWADEntries(string selectedPath, IReadOnlyCollection<WADEntry> entries, Dictionary<ulong, string> stringDictionary )
        {
            Directory.CreateDirectory(selectedPath);
           this.progress.Maximum = entries.Count;
            this.IsEnabled = false;

            BackgroundWorker wadExtractor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            wadExtractor.ProgressChanged += (sender, args) =>
            {
                this.progress.Value = args.ProgressPercentage;
                this.TractName.Content = args.UserState;
            };

            wadExtractor.DoWork += (sender, e) =>
            {
                Dictionary<string, byte[]> fileEntries = new Dictionary<string, byte[]>();
                double progress = 0;

                foreach (WADEntry entry in entries)
                {
                    byte[] entryData = entry.GetContent(true);
                    string entryName;
                    if (stringDictionary.ContainsKey(entry.XXHash))
                    {
                        entryName = stringDictionary[entry.XXHash];
                        Directory.CreateDirectory(string.Format("{0}//{1}", selectedPath, System.IO.Path.GetDirectoryName(entryName)));
                    }
                    else
                    {
                        entryName = BitConverter.ToString(BitConverter.GetBytes(entry.XXHash)).Replace("-", "");
                        entryName += "." + Utilities.GetEntryExtension(Utilities.GetLeagueFileExtensionType(entryData));
                    }
                    e.Result = entryName;
                   
                    fileEntries.Add(entryName, entryData);
                    progress += 0.5;
                    wadExtractor.ReportProgress((int)progress, "Decompress " + new FileInfo(entryName).Name);
                }
                foreach (KeyValuePair<string, byte[]> entry in fileEntries)
                {
                    File.WriteAllBytes(string.Format("{0}//{1}", selectedPath, entry.Key), entry.Value);
                    progress += 0.5;
                    wadExtractor.ReportProgress((int)progress, "Extract: " + new FileInfo(entry.Key).Name);
                }
            };

            wadExtractor.RunWorkerCompleted += (sender, args) =>
            {
                this.IsEnabled = true;
                this.progress.Maximum = 100;
                this.progress.Value = 100;
              
            };

            wadExtractor.RunWorkerAsync();
            return wadExtractor;
        } 
    }
}
