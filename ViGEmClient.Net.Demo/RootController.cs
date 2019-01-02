using System;
using Microsoft.Win32;
using Nefarius.ViGEm.Client.Targets;
using ViGEmClient.Net.Demo.Responsers;

namespace ViGEmClient.Net.Demo
{
    class RootController : IDisposable
    {
        private const string MacRegistryPath = "SYSTEM\\ControlSet001\\Services\\ViGEmBus\\Parameters\\Targets\\NintendoSwitchPro\\";
        private const string MacValueName = "TargetMacAddress";

        private JoyConsProDevice _joyConsProDevice;
        private NintendoSwitchController _deviceTarget;
        private CommandResponser _responser;

        /*public void Run()
        {
            _joyConsProDevice = new JoyConsProDevice
            {
                MainJoyCon = JoyConType.Right
            };

            _joyConsProDevice.AddSyncResponseContidion(report => report[0] == 0x21);
            _joyConsProDevice.AddSyncResponseContidion(report => report[0] == 0x30);
            _joyConsProDevice.InputRecieved += JoyConsProDev_InputRecieved;

            var error = _joyConsProDevice.Initialize();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine("Error!!");
                Console.ReadLine();
                return;
            }

            var macAddress = InitializeAndConnectDevice();
            

            _responser = new CommandResponser {DeviceMac = macAddress};
            _deviceTarget.FeedbackReceived += DeviceFeedbackReciver;

            Console.ReadLine();
        }*/

        public void Run()
        {
            var device = new Nefarius.ViGEm.Client.ViGEmClient();

            var deviceTarget = new NintendoSwitchController(device);

            deviceTarget.Connect();
            deviceTarget.FeedbackReceived += (sender, outputReport) =>
            {
                outputReport[0] = 0x30;
                deviceTarget.SendReport(outputReport);
            };

            Console.ReadLine();

            var packet = Reports.Default_0x21;
            deviceTarget.SendReport(packet);

            Console.ReadLine();

            packet[1] = 0xAA;
            deviceTarget.SendReport(packet);

            Console.ReadLine();

            deviceTarget.Disconnect();
        }

        private void JoyConsProDev_InputRecieved(byte[] inputPacket)
        {
            if (inputPacket != null)
            {
                _deviceTarget.SendReport(inputPacket);

                if (inputPacket[0] == 0x21)
                {
                    Console.WriteLine("Send response...");
                    PacketOutput.WriteData(inputPacket, 16);
                    Console.WriteLine("-------------------");
                    Console.WriteLine();
                }
            }
        }

        private void DeviceFeedbackReciver(object sender, byte[] packet)
        {
            Console.WriteLine("Got request...");
            PacketOutput.WriteData(packet, 16);
            Console.WriteLine();

            if (packet[0] == 0x80)
            {
                var response = _responser.BuildResponsePacket(packet);

                if (response != null)
                {
                    _deviceTarget.SendReport(response);

                    Console.WriteLine("Send response...");
                    PacketOutput.WriteData(response, 16);
                    Console.WriteLine("-------------------");
                    Console.WriteLine();
                }
            }
            else
            {
                var joyConPacket = new byte[49];
                Array.Copy(packet, joyConPacket, joyConPacket.Length);
                _joyConsProDevice.SendOutputReport(joyConPacket);
            }
        }

        private byte[] InitializeAndConnectDevice()
        {
            var vClient = new Nefarius.ViGEm.Client.ViGEmClient();
            _deviceTarget = new NintendoSwitchController(vClient);
            _deviceTarget.Connect();

            return ReadDeviceMac(1);
        }

        private byte[] ReadDeviceMac(uint serialNumber)
        {
            byte[] mac;
            using (var key = Registry.LocalMachine.OpenSubKey(MacRegistryPath + serialNumber.ToString().PadLeft(4, '0')))
            {
                mac = (byte[])key?.GetValue("TargetMacAddress") ?? new byte[0];
            }

            return mac;
        }

        public void Dispose()
        {
            _deviceTarget?.Disconnect();
            _deviceTarget?.Dispose();
        }
    }
}
