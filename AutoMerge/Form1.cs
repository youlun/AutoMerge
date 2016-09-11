using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMerge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        internal void StartProcess(string filename, string arguments, bool waitForExit = true, bool hidden = false, bool redirectStdout = false)
        {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = true
            };

            if (hidden) {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }
            if (redirectStdout) {
                psi.RedirectStandardOutput = true;
            }

            Process p = new Process() {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            p.Start();
            if (waitForExit)
                p.WaitForExit();
        }

        internal List<string> EnumerateFiles(string filePattern, string directory = null, int depth = 1)
        {
            var result = new List<string>();

            if (0 == depth && directory != null) {
                var files = Directory.EnumerateFiles(directory, filePattern);
                foreach (var file in files) {
                    result.Add(file);
                }
                return result;
            }

            var dirs = Directory.EnumerateDirectories((null == directory ? Directory.GetCurrentDirectory() : directory));
            foreach (var dir in dirs) {
                var files = Directory.EnumerateFiles(dir, filePattern);
                foreach (var file in files) {
                    result.Add(file);
                    Console.WriteLine(file);
                }
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Episode> episodes = new List<Episode>();

            var m2tsFiles = EnumerateFiles("*.m2ts");
            foreach (var file in m2tsFiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                string mkvFile = string.Format(@"{0}\{1}.mkv", directory, fileNameWithoutExtension);
                if (File.Exists(mkvFile)) continue;

                string videoFile = string.Format(@"{0}\{1}.{2}", directory, fileNameWithoutExtension, (this.rdo264.Checked ? "264" : "hevc"));
                if (!File.Exists(videoFile)) continue;

                List<string> audioFiles = null;
                if (this.rdoFlac.Checked) {
                    audioFiles = EnumerateFiles(string.Format("{0}*.flac", fileNameWithoutExtension), directory, 0);
                } else if (this.rdoM4a.Checked) {
                    audioFiles = EnumerateFiles(string.Format("{0}*.m4a", fileNameWithoutExtension), directory, 0);
                } else if (this.rdoAudioBoth.Checked) {
                    audioFiles = EnumerateFiles(string.Format("{0}*.flac", fileNameWithoutExtension), directory, 0);
                    audioFiles.AddRange(EnumerateFiles(string.Format("{0}*.m4a", fileNameWithoutExtension), directory, 0));
                } else {
                    MessageBox.Show("请选择音频");
                    return;
                }
                if (0 == audioFiles.Count) continue;

                string chapterFile = string.Format(@"{0}\{1}.txt", directory, fileNameWithoutExtension);

                var subtitleFiles = this.EnumerateFiles(string.Format("{0}*.sup", fileNameWithoutExtension), directory, 0);
                

                Episode episode = new Episode() {
                    VideoFile = videoFile,
                    MkvFile = mkvFile,
                    ChapterFile = (File.Exists(chapterFile) ? chapterFile : null)
                };
                foreach (var audioFile in audioFiles) {
                    episode.AudioFiles.Add(audioFile);
                }
                foreach (var subtitleFile in subtitleFiles) {
                    episode.SubtitleFiles.Add(subtitleFile);
                }

                episodes.Add(episode);
            }

            /*
                \"C:/Program Files/MKVToolNix\mkvmerge.exe\"
                --ui-language zh_CN
                --output ^\"{0}^\"
                hevc:       --language 0:und ^\"^(^\" ^\"{0}^\" ^\"^)^\"
                --language 0:jpn ^\"^(^\" ^\"C:\Users\youlu\Documents\vol6-ok\00000.2.m4a^\" ^\"^)^\"
                --language 0:jpn ^\"^(^\" ^\"C:\Users\youlu\Documents\vol6-ok\00014.2.flac^\" ^\"^)^\"
                --language 0:jpn ^\"^(^\" ^\"C:\Users\youlu\Documents\vol6-ok\00000.2.flac^\" ^\"^)^\"
                --language 0:und ^\"^(^\" ^\"E:\old\00000\T5_Subtitle - Japanese.sup^\" ^\"^)^\"
                --chapters ^\"{0}^\"
                --track-order 0:0,1:0,2:0,3:0,4:0
            */
            string trackTemplate = "--language 0:{0} \"(\" \"{1}\" \")\"";
            List<string> mergeParameters = new List<string>();
            foreach (var episode in episodes) {
                List<string> parameters = new List<string>();
                List<string> trackOrder = new List<string>();

                int fileID = 0;

                parameters.Add("--ui-language zh_CN");
                parameters.Add(string.Format("--output \"{0}\"", episode.MkvFile));

                parameters.Add(string.Format(trackTemplate, "und", episode.VideoFile));
                trackOrder.Add(string.Format("{0}:0", fileID++));

                foreach (var audioFile in episode.AudioFiles) {
                    parameters.Add(string.Format(trackTemplate, "jpn", audioFile));
                    trackOrder.Add(string.Format("{0}:0", fileID++));
                }

                foreach (var subtitleFile in episode.SubtitleFiles) {
                    parameters.Add(string.Format(trackTemplate, "jpn", subtitleFile));
                    trackOrder.Add(string.Format("{0}:0", fileID++));
                }

                if (null != episode.ChapterFile) parameters.Add(string.Format("--chapters \"{0}\"", episode.ChapterFile));

                parameters.Add(string.Format("--track-order {0}", string.Join(",", trackOrder)));

                mergeParameters.Add(string.Join(" ", parameters));
            }

            Parallel.ForEach(mergeParameters, new ParallelOptions {
                MaxDegreeOfParallelism = Program.NumaNodes.Count
            }, parameter => {
                Console.WriteLine(@"C:\Users\youlu\AppData\Local\Programs\MKVToolNix\mkvmerge.exe" + "\n" + parameter.Replace(" --", "\n--") + "\n");
                StartProcess(@"C:\Users\youlu\AppData\Local\Programs\MKVToolNix\mkvmerge.exe", parameter);
            });

        }
    }
}
