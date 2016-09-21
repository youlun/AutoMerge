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
using System.Configuration;

namespace AutoMerge
{
    public partial class Form1 : Form
    {
        private Dictionary<OutputType, string> OutputFileExtension = new Dictionary<OutputType, string>()
        {
            { OutputType.MKV, ".mkv" },
            { OutputType.MP4, ".mp4" }
        };
        private Dictionary<VideoSourceType, string> VideoSourceFileExtension = new Dictionary<VideoSourceType, string>()
        {
            { VideoSourceType.AVC, ".264" },
            { VideoSourceType.HEVC, ".hevc" }
        };
        private Dictionary<AudioSourceType, string> AudioSourceFileExtension = new Dictionary<AudioSourceType, string>()
        {
            { AudioSourceType.FLAC, ".flac" },
            { AudioSourceType.M4A, ".m4a" }
        };
        private string MkvMergeFilePath = null;
        private string Mp4MuxerFilePath = null;


        public Form1()
        {
            InitializeComponent();

            this.MkvMergeFilePath = ConfigurationManager.AppSettings["MkvMergeFilePath"];
            this.Mp4MuxerFilePath = ConfigurationManager.AppSettings["Mp4MuxerFilePath"];

            Func<string, string> selectFile = (title) => {
                using (OpenFileDialog dlg = new OpenFileDialog()) {
                    dlg.Filter = "exe(*.exe)|*.exe";
                    dlg.Title = title;
                    dlg.Multiselect = false;
                    if (DialogResult.OK == dlg.ShowDialog()) {
                        return dlg.FileName;
                    }
                }
                return null;
            };

            while (null == this.MkvMergeFilePath || !File.Exists(this.MkvMergeFilePath)) {
                this.MkvMergeFilePath = selectFile("选择 MKVToolNix mkvmerge.exe");
                ConfigurationManager.AppSettings["MkvMergeFilePath"] = this.MkvMergeFilePath;
            }
            while (null == this.Mp4MuxerFilePath || !File.Exists(this.Mp4MuxerFilePath)) {
                this.Mp4MuxerFilePath = selectFile("选择 L-SMASH muxer.exe");
                ConfigurationManager.AppSettings["Mp4MuxerFilePath"] = this.Mp4MuxerFilePath;
            }
        }

        private void StartProcess(string filename, string arguments, bool waitForExit = true, bool hidden = false, bool redirectStdout = false)
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
                psi.UseShellExecute = false;
            }

            Process p = new Process() {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            p.Start();
            if (waitForExit)
                p.WaitForExit();
        }

        private List<string> EnumerateFiles(string filePattern, string directory = null, int depth = 1)
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

        private List<Episode> MakeEpisodes(
            List<string> m2tsfiles,
            AudioSourceType audioType,
            VideoSourceType videoType,
            OutputType outputType,
            string videoFps,
            string audioLanguage,
            string subtitleLanguage,
            bool mergeChapter,
            bool mergeSubtitle
        )
        {

            List<Episode> episodes = new List<Episode>();

            foreach (var file in m2tsfiles) {
                string directory = Path.GetDirectoryName(file);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string baseName = $@"{directory}\{fileNameWithoutExtension}";

                string outputFile = baseName + this.OutputFileExtension[outputType];
                string videoFile = baseName + this.VideoSourceFileExtension[videoType];
                string chapterFile = baseName + ".txt";
                var audioFiles = new List<string>();
                var subtitleFiels = new List<string>();

                if (File.Exists(outputFile)) continue;
                if (!File.Exists(videoFile)) continue;

                if (AudioSourceType.BOTH == audioType) {
                    foreach (var extension in this.AudioSourceFileExtension) {
                        audioFiles.AddRange(this.EnumerateFiles($"{fileNameWithoutExtension}*{extension.Value}", directory, 0));
                    }
                } else {
                    audioFiles.AddRange(this.EnumerateFiles($"{fileNameWithoutExtension}*{this.AudioSourceFileExtension[audioType]}", directory, 0));
                }
                if (0 == audioFiles.Count) continue;

                subtitleFiels.AddRange(this.EnumerateFiles($"{fileNameWithoutExtension}*.sup", directory, 0));

                Episode ep = new Episode() {
                    VideoFile = videoFile,
                    VideoFps = videoFps,
                    OutputFile = outputFile,
                    OutputFileType = outputType,
                    ChapterFile = File.Exists(chapterFile) && mergeChapter ? chapterFile : null,
                    AudioFiles = audioFiles,
                    AudioLanguage = audioLanguage,
                    SubtitleFiles = mergeSubtitle ? subtitleFiels : null,
                    SubtitleLanguage = subtitleLanguage
                };
                episodes.Add(ep);
            }

            return episodes;
        }

        private string MkvMergeParameter(Episode episode)
        {
            string trackTemplate = "--language 0:{0} \"(\" \"{1}\" \")\"";

            var parameters = new List<string>();
            var trackOrder = new List<string>();

            int fileID = 0;

            parameters.Add("--ui-language zh_CN");
            parameters.Add($"--output \"{episode.OutputFile}\"");

            parameters.Add(string.Format(trackTemplate, "und", episode.VideoFile));
            trackOrder.Add($"{fileID++}:0");

            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add(string.Format(trackTemplate, episode.AudioLanguage, audioFile));
                trackOrder.Add($"{fileID++}:0");
            }

            if (episode.SubtitleFiles != null) {
                foreach (var subtitleFile in episode.SubtitleFiles) {
                    parameters.Add(string.Format(trackTemplate, episode.SubtitleLanguage, subtitleFile));
                    trackOrder.Add($"{fileID++}:0");
                }
            }

            if (episode.ChapterFile != null) parameters.Add($"--chapters \"{episode.ChapterFile}\"");

            parameters.Add(string.Format("--track-order {0}", string.Join(",", trackOrder)));

            return string.Join(" ", parameters);
        }

        private string Mp4MergeParameter(Episode episode)
        {
            var parameters = new List<string>();
            //mp4muxer --interleave 500 --file-format mp4 --chapter {0} -i{1}?fps=24000/1001 -i{2}?language=jpn -o {3}
            parameters.Add("--file-format mp4");

            if (episode.ChapterFile != null) parameters.Add($"--chapter \"{episode.ChapterFile}\"");

            parameters.Add($"-i {episode.VideoFile}?fps={episode.VideoFps}");

            foreach (var audioFile in episode.AudioFiles) {
                parameters.Add($"-i {audioFile}?language={episode.AudioLanguage}");
            }

            parameters.Add($"-o {episode.OutputFile}");

            return string.Join(" ", parameters);
        }

        private List<Tuple<OutputType, string>> MakeMergeParameters(List<Episode> episodes)
        {
            var mergeParameters = new List<Tuple<OutputType, string>>();

            foreach (var ep in episodes) {
                string parameter = null;
                switch (ep.OutputFileType) {
                    case OutputType.MKV:
                        parameter = this.MkvMergeParameter(ep);
                        break;
                    case OutputType.MP4:
                        parameter = this.Mp4MergeParameter(ep);
                        break;
                    default:
                        continue;
                }
                mergeParameters.Add(Tuple.Create(ep.OutputFileType, parameter));
            }

            return mergeParameters;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AudioSourceType audioType;
            VideoSourceType videoType;
            OutputType outputType;

            #region UI
            if (this.rdoFlac.Checked) {
                audioType = AudioSourceType.FLAC;
            } else if (this.rdoM4a.Checked) {
                audioType = AudioSourceType.M4A;
            } else if (this.rdoAudioBoth.Checked) {
                audioType = AudioSourceType.BOTH;
            } else {
                MessageBox.Show("需要选择输入音频格式");
                return;
            }

            if (this.rdo264.Checked) {
                videoType = VideoSourceType.AVC;
            } else if (this.rdoHevc.Checked) {
                videoType = VideoSourceType.HEVC;
            } else {
                MessageBox.Show("需要选择输入视频格式");
                return;
            }

            if (this.rdoMkv.Checked) {
                outputType = OutputType.MKV;
            } else if (this.rdoMp4.Checked) {
                outputType = OutputType.MP4;
            } else {
                MessageBox.Show("需要选择输出视频格式");
                return;
            }
            #endregion

            var m2tsFiles = EnumerateFiles("*.m2ts");
            var episodes = this.MakeEpisodes(m2tsFiles, audioType, videoType, outputType,
                this.txtFps.Text, this.txtAudioLanguage.Text, this.txtSubtitleLanguage.Text, this.chkChapter.Checked, this.chkSubtitle.Checked);
            var mergeParameters = this.MakeMergeParameters(episodes);

            Parallel.ForEach(mergeParameters, new ParallelOptions {
                MaxDegreeOfParallelism = Program.NumaNodes.Count
            }, parameter => {
                string mainProcess = null;
                switch (parameter.Item1) {
                    case OutputType.MKV:
                        mainProcess = this.MkvMergeFilePath;
                        break;
                    case OutputType.MP4:
                        mainProcess = this.Mp4MuxerFilePath;
                        break;
                    default:
                        return;
                }

                Console.WriteLine($"{mainProcess} {parameter.Item2}");
                StartProcess(mainProcess, parameter.Item2);
            });

        }
    }
}
