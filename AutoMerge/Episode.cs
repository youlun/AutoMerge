using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMerge
{
    enum OutputType
    {
        MKV,
        MP4
    }

    enum AudioSourceType
    {
        FLAC,
        M4A,
        BOTH
    }

    enum VideoSourceType
    {
        AVC,
        HEVC
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
