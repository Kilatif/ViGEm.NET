using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo.Packets
{
    enum UsbControllerType : byte
    {
        LeftJoyCon = 1,
        RightJoyCon = 2,
        ProController = 3
    }

    [PacketSize(64)]
    class UsbResponsePacket
    {
        [PacketInfo(0)]
        public byte CommandId { get; set; }

        [PacketInfo(1)]
        public byte SubCommandId { get; set; }

        [PacketInfo(3)]
        public UsbControllerType GamePadType { get; set; }

        [PacketInfo(4, MaxSize = 6)]
        public byte[] GamePadMac { get; set; }
    }
}
