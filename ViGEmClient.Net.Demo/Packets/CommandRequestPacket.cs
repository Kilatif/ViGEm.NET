using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    [PacketSize(64)]
    class CommandRequestPacket
    {
        [PacketInfo(0)]
        public byte CommandId { get; set; }
        [PacketInfo(1)]
        public byte PacketNumber { get; set; }
        [PacketInfo(10)]
        public byte SubCommandId { get; set; }
        [PacketInfo(11)]
        public byte[] SubCommandData { get; set; }
    }

    [PacketSize(35)]
    class SubCmdDataFlashRead
    {
        [PacketInfo(0)]
        public ulong Address { get; set; }
        [PacketInfo(4)]
        public byte Length { get; set; }
        [PacketInfo(5, MaxSize = 30)]
        public byte[] FlashData { get; set; }
    }
}
