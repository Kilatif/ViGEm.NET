using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    class CommandResponser : IPacketResponser
    {
        private const byte SrcPckTypeVal = 0x01;
        private const byte RespPckTypeVal = 0x21;
        private const byte DataPckTypeVal = 0x30;

        private CommandRequestPacket _requestPacket;

        public byte[] SourcePacket { set => SetSourcePacket(value); }
        public byte[] ResponsePacket => BuildResponsePacket();

        public void SetSourcePacket(byte[] packet)
        {
            _requestPacket = PacketConstructor.BuildObject<CommandRequestPacket>(packet);
        }

        public byte[] BuildResponsePacket()
        {
            var responsePacket = PacketConstructor.BuildObject<ReportPacket>(Reports.Default_0x21);
            responsePacket.SubCmdId = _requestPacket.SubCommandId;
            responsePacket.ACK = 0x80;

            switch (_requestPacket.SubCommandId)
            {
                case 0x10 :
                    var subCmdData = PacketConstructor.BuildObject<SubCmdDataFlashRead>(_requestPacket.SubCommandData);
                    if (Reports.FlashDataMap.TryGetValue(subCmdData.Address, out byte[] data))
                    {
                        responsePacket.ACK = 0x90;
                        subCmdData.FlashData = data;
                        responsePacket.SubCmdData = PacketConstructor.BuildPacket(subCmdData);
                    }
                    else
                    {
                        return null;
                    }
                    break;
                case 0x40:

                    break;
            }

            return PacketConstructor.BuildPacket(responsePacket);
        }
    }
}
