using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo.Packets
{
    [PacketSize(64)]
    class GeneralPacket
    {
        [PacketInfo(0)]
        public byte CommandId { get; set; }

        [PacketInfo(1)]
        public byte SubCommandId { get; set; }
    }
}
