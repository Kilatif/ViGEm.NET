using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    public class FlashDataMapDto
    {
        public byte[] DefaultValue;
        public bool IsUseMain;
    }

    static class Reports
    {
        public static readonly byte[] Default_0x21 = 
        {
            0x21, 0x5E, 0x91, 0x00, 0x80, 0x00, 0x6E, 0xE7, 0x76, 0x6D, 0x88, 0x71, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static readonly Dictionary<ulong, byte[]> FlashDataMap = new Dictionary<ulong, byte[]>
        {
            [0xE0600000] = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            [0x20600000] = new byte[] 
            {
                0x5E, 0x00, 0xB2, 0xFF, 0x28, 0x01, 0x00, 0x40, 0x00, 0x40, 0x00, 0x40,
                0xB6, 0xFF, 0xD2, 0xFF, 0xBD, 0xFF, 0x3B, 0x34, 0x3B, 0x34, 0x3B, 0x34
            },
            [0x80600000] = new byte[] { 0x50, 0xFD, 0x00, 0x00, 0xC6, 0x0F },
            [0x3D600000] = new byte[] 
            {
                0xF2, 0x45, 0x66, 0x6F, 0xA7, 0x76, 0xFA, 0xB5, 0x5D,
                0x3A, 0x68, 0x74, 0x54, 0x06, 0x5F, 0xC2, 0xC5, 0x62
            }
        };

        public static readonly Dictionary<ulong, FlashDataMapDto> FlashDataMapper = new Dictionary<ulong, FlashDataMapDto>
        {
            [0xE0600000] = new FlashDataMapDto { DefaultValue = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF } },
            [0x20600000] = new FlashDataMapDto { IsUseMain = true },
            [0x80600000] = new FlashDataMapDto { DefaultValue = new byte[] { 0x50, 0xFD, 0x00, 0x00, 0xC6, 0x0F } },
            [0x3D600000] = new FlashDataMapDto (),
        };

        public static byte[] BuildUnionReport_0x30(byte[] leftReport, byte[] rightReport, JoyConType mainJoyCon)
        {
            var leftReportObj = PacketConstructor.BuildObject<InputReportPacket>(leftReport);
            var rightReportObj = PacketConstructor.BuildObject<InputReportPacket>(rightReport);
            var mainReportObj = (mainJoyCon == JoyConType.Left) ? leftReportObj : rightReportObj;

            var inputReportObj = new InputReportPacket
            {
                ReportId = 0x30,
                Timer = mainReportObj.Timer,
                Battery = BatteryStatus.Charging | BatteryStatus.Full,
                ConnectionType = ConnectionType.Usb,
                ControllerType = ControllerType.Pro,
                LeftButtons = leftReportObj.LeftButtons,
                ShareButtons = leftReportObj.ShareButtons | rightReportObj.ShareButtons | SharedButtons.ChargingGrip,
                RightButtons = rightReportObj.RightButtons,
                LeftStick = leftReportObj.LeftStick,
                RightStick = rightReportObj.RightStick,
                Vibration = 0x00,
            };

            var customData = new byte[mainReportObj.CustomData.Length];
            Array.Copy(mainReportObj.CustomData, customData, customData.Length);
            if (mainJoyCon == JoyConType.Right)
            {
                for (var i = 0; i < customData.Length; i++)
                {
                    customData[i] ^= 0xFF;
                }
            }

            inputReportObj.CustomData = customData;
            
            return PacketConstructor.BuildPacket(inputReportObj);
        }

        public static byte[] BuildUnionReport_0x21(byte[] leftReport, byte[] rightReport, JoyConType mainJoyCon)
        {
            var leftReportObj = PacketConstructor.BuildObject<CmdInputReportPacket>(leftReport);
            var rightReportObj = PacketConstructor.BuildObject<CmdInputReportPacket>(rightReport);
            var mainReportObj = (mainJoyCon == JoyConType.Left) ? leftReportObj : rightReportObj;

            var inputReportObj = new CmdInputReportPacket()
            {
                ReportId = 0x21,
                Timer = mainReportObj.Timer,
                Battery = BatteryStatus.Charging | BatteryStatus.Full,
                ConnectionType = ConnectionType.Usb,
                ControllerType = ControllerType.Pro,
                LeftButtons = leftReportObj.LeftButtons,
                ShareButtons = leftReportObj.ShareButtons | rightReportObj.ShareButtons | SharedButtons.ChargingGrip,
                RightButtons = rightReportObj.RightButtons,
                LeftStick = leftReportObj.LeftStick,
                RightStick = rightReportObj.RightStick,
                Vibration = 0x00,
                ACK = mainReportObj.ACK,
                SubCmdId = mainReportObj.SubCmdId,
            };

            if (mainReportObj.SubCmdId == 0x10)
            {
                var subCmdFlash = PacketConstructor.BuildObject<SubCmdDataFlashRead>(mainReportObj.SubCmdData);
                if (FlashDataMapper.TryGetValue(subCmdFlash.Address, out var flashMapDto))
                {
                    if (flashMapDto.DefaultValue != null)
                    {
                        subCmdFlash.FlashData = flashMapDto.DefaultValue;
                        inputReportObj.SubCmdData = PacketConstructor.BuildPacket(subCmdFlash);
                    }
                    else if (flashMapDto.IsUseMain)
                    {
                        inputReportObj.SubCmdData = mainReportObj.SubCmdData;
                    }
                    else if (leftReportObj.SubCmdData.Length == rightReportObj.SubCmdData.Length)
                    {
                        var subData = new byte[leftReportObj.SubCmdData.Length];
                        for (var i = 0; i < leftReportObj.SubCmdData.Length; i++)
                        {
                            subData[i] = (byte)(leftReportObj.SubCmdData[i] & rightReportObj.SubCmdData[i]);
                        }

                        inputReportObj.SubCmdData = subData;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return PacketConstructor.BuildPacket(inputReportObj);
        }
    }
}
