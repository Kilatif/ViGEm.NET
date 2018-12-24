using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //var val = BitConverter.ToUInt16(new byte[] {0x34, 0xFE}, 0);


            var root = new RootController();
            root.Run();
        }
    }
}
