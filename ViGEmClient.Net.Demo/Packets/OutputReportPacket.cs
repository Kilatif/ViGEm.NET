﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    [PacketSize(49)]
    class OutputReportPacket
    {
        [PacketInfo(0)]
        public byte CommandId { get; set; }
        [PacketInfo(1)]
        public byte PacketNumber { get; set; }
        [PacketInfo(2, MaxSize = 8)]
        public byte[] RumbleData { get; set; } = {0x00, 0x01, 0x40, 0x40, 0x00, 0x01, 0x40, 0x40};
        [PacketInfo(10)]
        public byte SubCommandId { get; set; }
        [PacketInfo(11, MaxSize = 35)]
        public byte[] SubCommandData { get; set; }
    }

    [PacketSize(35)]
    class SubCmdDataFlashRead
    {
        [PacketInfo(0)]
        public uint Address { get; set; }
        [PacketInfo(4)]
        public byte Length { get; set; }
        [PacketInfo(5, MaxSize = 30)]
        public byte[] FlashData { get; set; }
    }
}
