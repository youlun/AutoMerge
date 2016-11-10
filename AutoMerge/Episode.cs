using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AutoMerge
{
    enum OutputType
    {
        Mkv,
        Mp4
    }

    enum AudioSourceType
    {
        Flac,
        M4a,
        Aac,
        Ac3
    }

    enum VideoSourceType
    {
        Avc,
        Hevc
    }

    enum ButtonType
    {
        Check,
        Radio
    }

    class FileType
    {
        private string _uiDisplayName = string.Empty;

        public string UIDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(this._uiDisplayName)) return this._uiDisplayName;
                else if (!string.IsNullOrEmpty(this.FileExtension)) return this.FileExtension.Substring(1);
                else throw new NullReferenceException("必须设置 FileExtension");
            }
            set
            {
                this._uiDisplayName = value;
            }
        }
        public string FileExtension { get; set; }
        public ButtonType UIButtonType { get; set; } = ButtonType.Radio;
        public bool NeedEnterFps { get; set; } = false;
        public string UIBaseCheckBox { get; set; } = null;
    }

    class Episode
    {
        public string VideoFile { get; set; }
        public string VideoFps { get; set; }
        public List<string> AudioFiles { get; set; }
        public string AudioLanguage { get; set; }
        public string ChapterFile { get; set; }
        public List<string> SubtitleFiles { get; set; }
        public string SubtitleLanguage { get; set; }

        public OutputType OutputFileType { get; set; }
        public string OutputFile { get; set; }
    }
}
