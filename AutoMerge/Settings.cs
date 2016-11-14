using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace AutoMerge
{
    internal static class Settings
    {
        private static string _mkvMergeFilePath = Path.Combine(Environment.CurrentDirectory, @"tools\MKVToolNix\mkvmerge.exe");
        private static string _mp4MuxerFilePath = Path.Combine(Environment.CurrentDirectory, @"tools\l-smash\mp4muxer.exe");

        internal static Dictionary<VideoSourceType, FileType> VideoSourceFileTypes { get; } = new Dictionary<VideoSourceType, FileType>
        {
            { VideoSourceType.Avc, new FileType { FileExtension = ".264", UIBaseCheckBox = "videoSourceSelector" } },
            { VideoSourceType.Hevc, new FileType { FileExtension = ".hevc", UIBaseCheckBox = "videoSourceSelector" } }
        };
        internal static Dictionary<AudioSourceType, FileType> AudioSourceFileTypes { get; } = new Dictionary<AudioSourceType, FileType>
        {
            { AudioSourceType.Flac, new FileType { FileExtension = ".flac", UIButtonType = ButtonType.Check, UIBaseCheckBox = "audioSourceSelector" } },
            { AudioSourceType.M4a, new FileType { FileExtension = ".m4a", UIButtonType = ButtonType.Check, UIBaseCheckBox = "audioSourceSelector" } },
            { AudioSourceType.Aac, new FileType { FileExtension = ".aac", UIButtonType = ButtonType.Check, UIBaseCheckBox = "audioSourceSelector" } },
            { AudioSourceType.Ac3, new FileType { FileExtension = ".ac3", UIButtonType = ButtonType.Check, UIBaseCheckBox = "audioSourceSelector" } }
        };
        internal static Dictionary<OutputType, FileType> OutputFileTypes { get; set; } = new Dictionary<OutputType, FileType>
        {
            { OutputType.Mkv, new FileType { FileExtension = ".mkv", NeedEnterFps = true } },
            { OutputType.Mp4, new FileType { FileExtension = ".mp4", NeedEnterFps = true } }
        };

        internal static string ChapterFileExtension { get; set; } = ".txt";
        internal static string SubtitleFileExtension { get; set; } = ".sup";

        private static string RegistryPath { get; } = @"SOFTWARE\AutoMerge";

        public static string MkvMergeFilePath
        {
            get
            {
                return _mkvMergeFilePath;
            }
            set
            {
                _mkvMergeFilePath = value;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true)) {
                    if (null == key) {
                        return;
                    }
                    key.SetValue("MkvMergeFilePath", value);
                }
            }
        }

        public static string Mp4MuxerFilePath
        {
            get
            {
                return _mp4MuxerFilePath;
            }
            set
            {
                _mp4MuxerFilePath = value;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true)) {
                    if (null == key) {
                        return;
                    }
                    key.SetValue("Mp4MuxerFilePath", value);
                }
            }
        }

        static Settings()
        {
            RegistryKey key = null;

            try {
                key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (null == key) {
                    key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                }

                if (!File.Exists(_mkvMergeFilePath)) {
                    _mkvMergeFilePath = key.GetValue("MkvMergeFilePath", string.Empty).ToString();
                }
                if (!File.Exists(_mp4MuxerFilePath)) {
                    _mp4MuxerFilePath = key.GetValue("Mp4MuxerFilePath", string.Empty).ToString();
                }
            } catch {
                Console.WriteLine("注册表打开失败");
            } finally {
                if (key != null) {
                    key.Close();
                    key.Dispose();
                }
            }

            SelectBinariesPath();
        }

        private static string ShowSelectFileDialog(string title)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "exe(*.exe)|*.exe";
            dlg.Title = title;
            dlg.Multiselect = false;
            if (true == dlg.ShowDialog()) {
                return dlg.FileName;
            }
            return null;
        }

        private static void SelectBinariesPath()
        {
            var tasks = new List<Tuple<string, string, string, OutputType>> {
                Tuple.Create("MkvMergeFilePath", "MKVToolNix mkvmerge.exe", "mkv", OutputType.Mkv),
                Tuple.Create("Mp4MuxerFilePath", "L-SMASH muxer.exe", "mp4", OutputType.Mp4)
            };
            foreach (var task in tasks) {
                var filePathProperty = typeof(Settings).GetProperty(task.Item1, BindingFlags.Static | BindingFlags.Public);
                var newFilePath = filePathProperty.GetValue(null).ToString();

                while (!File.Exists(newFilePath)) {
                    newFilePath = ShowSelectFileDialog($"选择 {task.Item2}");
                    filePathProperty.SetValue(null, newFilePath);

                    if (string.IsNullOrEmpty(newFilePath)) {
                        if (Utility.ShowQuestionMessageBox($"不使用 {task.Item3} 封装功能？", "确认")) {
                            filePathProperty.SetValue(null, string.Empty);
                            OutputFileTypes.Remove(task.Item4);
                            break;
                        }
                        continue;
                    }

                    if (!Utility.ShowQuestionMessageBox($"确定选对了？选错了可不好改哟\r\n{newFilePath}", "确认")) {
                        filePathProperty.SetValue(null, string.Empty);
                        continue;
                    }
                }
            }
        }
    }
}
