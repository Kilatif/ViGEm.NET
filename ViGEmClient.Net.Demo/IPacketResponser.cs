using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    public class PacketException : Exception
    {
        public PacketException(byte wrongType, byte exceptedType) 
            : base($"Invalid packet type : excepted 0x{exceptedType:X} instead of 0x{wrongType:X}") { }
    }

    interface IPacketResponser
    {
        byte[] SourcePacket { set; }
        byte[] ResponsePacket { get; }
    }
}
