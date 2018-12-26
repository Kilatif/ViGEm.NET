using System.Linq;

namespace ViGEmClient.Net.Demo
{
    enum UsbControllerType : byte
    {
        LeftJoyCon = 1,
        RightJoyCon = 2,
        ProController = 3
    }

    [PacketSize(64)]
    class UsbRequestPacket
    {
        [PacketInfo(0)]
        public byte CommandId { get; set; }

        [PacketInfo(1)]
        public byte SubCommandId { get; set; }
    }

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

    class UsbResponser : IPacketResponser
    {
        private const byte SrcPckTypeVal = 0x80;
        private const byte RespPckTypeVal = 0x81;

        private readonly byte[] _deviceMac;

        private UsbRequestPacket _requestPacket;

        public bool TimerEnabled { get; private set; }

        public byte[] SourcePacket { set => SetSourcePacket(value); }
        public byte[] ResponsePacket => BuildResponsePacket();

        public UsbResponser(byte[] deviceMac)
        {
            _deviceMac = deviceMac;
        }

        public void SetSourcePacket(byte[] packet)
        {
            _requestPacket = PacketConstructor.BuildObject<UsbRequestPacket>(packet);
            if (_requestPacket.CommandId != SrcPckTypeVal)
            {
                throw new PacketException(packet[0], SrcPckTypeVal);
            }
        }

        public byte[] BuildResponsePacket()
        {
            var responsePacket = new UsbResponsePacket
            {
                CommandId = RespPckTypeVal,
                SubCommandId = _requestPacket.SubCommandId
            };

            if (_requestPacket.SubCommandId == 0x01)
            {
                responsePacket.GamePadType = UsbControllerType.ProController;
                responsePacket.GamePadMac = _deviceMac.Reverse().ToArray();
            }
            else if (_requestPacket.SubCommandId == 0x04)
            {
                TimerEnabled = true;
            }
            else if (_requestPacket.SubCommandId == 0x05)
            {
                TimerEnabled = false;
            }

            return PacketConstructor.BuildPacket(responsePacket);
        }
    }
}
