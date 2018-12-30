using System.Linq;
using ViGEmClient.Net.Demo.Packets;

namespace ViGEmClient.Net.Demo.Responsers
{
    class CommandResponser
    {
        public bool IsTimerEnabled { get; private set; }
        public byte[] DeviceMac { private get; set; } = new byte[1];

        public byte[] BuildResponsePacket(byte[] requestPacket)
        {
            var generalPacket = PacketConstructor.BuildObject<GeneralPacket>(requestPacket);
            switch (generalPacket.CommandId)
            {
                case 0x80: return BuildUsbResponsePacket(generalPacket.SubCommandId);
                case 0x01: return BuildReportPacket(requestPacket);
                default: return null;
            }
        }

        private byte[] BuildUsbResponsePacket(byte subCommandId)
        {
            var responsePacket = new UsbResponsePacket
            {
                CommandId = 0x81,
                SubCommandId = subCommandId
            };

            CmdInputReportPacket response;
            switch (subCommandId)
            {
                case 0x01:
                    responsePacket.GamePadType = UsbControllerType.ProController;
                    responsePacket.GamePadMac = DeviceMac.Reverse().ToArray();
                    break;

                case 0x04:
                    IsTimerEnabled = true;

                    response = PacketConstructor.BuildObject<CmdInputReportPacket>(Reports.Default_0x21);
                    response.ReportId = 0x30;
                    return PacketConstructor.BuildPacket(response);

                case 0x05:
                    IsTimerEnabled = false;
                    response = PacketConstructor.BuildObject<CmdInputReportPacket>(Reports.Default_0x21);
                    response.ReportId = 0x30;
                    
                    return PacketConstructor.BuildPacket(response);
            }

            return PacketConstructor.BuildPacket(responsePacket);
        }

        private byte[] BuildReportPacket(byte[] packet)
        {
            var responsePacket = PacketConstructor.BuildObject<CmdInputReportPacket>(Reports.Default_0x21);
            var requestPacket = PacketConstructor.BuildObject<OutputReportPacket>(packet);

            responsePacket.SubCmdId = requestPacket.SubCommandId;
            responsePacket.ACK = 0x80;

            switch (requestPacket.SubCommandId)
            {
                case 0x10:
                    var subCmdData = PacketConstructor.BuildObject<SubCmdDataFlashRead>(requestPacket.SubCommandData);
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
            }

            return PacketConstructor.BuildPacket(responsePacket);
        }
    }
}
