using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMerge
{
    struct Episode
    {
        public string VideoFile { get; set; }
        public List<string> AudioFiles { get; set; }
        public string ChapterFile { get; set; }
        public string MkvFile { get; set; }
    }

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
