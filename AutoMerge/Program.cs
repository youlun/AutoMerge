using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoMerge
{

    struct NumaNode
    {
        public uint NodeNumber { get; set; }
        public ulong Mask { get; set; }
    }

    class Program
    {
        internal static List<NumaNode> NumaNodes = new List<NumaNode>();

        [STAThread]
        static void Main(string[] args)
        {
            var cpus = NativeMethods.GetLogicalProcessorInformation();
            foreach (var cpu in cpus) {
                if (cpu.Relationship == NativeMethods.LOGICAL_PROCESSOR_RELATIONSHIP.RelationNumaNode) {
                    ulong mask = cpu.ProcessorMask.ToUInt64();
                    uint nodeNumber = cpu.ProcessorInformation.NumaNode.NodeNumber;
                    NumaNodes.Add(new NumaNode() { NodeNumber = nodeNumber, Mask = mask });
                    NumaNodes.Sort((x, y) => x.NodeNumber.CompareTo(y.NodeNumber));
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
