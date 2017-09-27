using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
        public bool DefaultSelected { get; set; } = false;
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
        public string ChapterLanguage { get; set; }

        public OutputType OutputFileType { get; set; }
        public string OutputFile { get; set; }
        public Guid TaskId { get; set; }
        public long TotalFileSize { get; set; }
    }

    public class MuxingTask : INotifyPropertyChanged
    {
        private Guid _taskId;
        private string _outputFileName;
        private string _statusText;
        private int _percent;

        public Guid TaskId { get { return _taskId; } set { SetField(ref _taskId, value, "TaskId"); } }
        public string OutputFileName { get { return _outputFileName; } set { SetField(ref _outputFileName, value, "OutputFileName"); } }
        public string StatusText { get { return _statusText; } set { SetField(ref _statusText, value, "StatusText"); } }
        public int Percent { get { return _percent; } set { SetField(ref _percent, value, "Percent"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
