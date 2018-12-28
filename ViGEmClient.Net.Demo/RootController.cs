using System;
using Microsoft.Win32;
using Nefarius.ViGEm.Client.Targets;
using ViGEmClient.Net.Demo.Responsers;

namespace ViGEmClient.Net.Demo
{
    enum TimerStatus : byte
    {
        Disable = 0,
        EnableAndSavePacket = 1,
        Ignore = 2
    }

    class RootController : IDisposable
    {
        private const string MacRegistryPath = "SYSTEM\\ControlSet001\\Services\\ViGEmBus\\Parameters\\Targets\\NintendoSwitchPro\\";
        private const string MacValueName = "TargetMacAddress";

        private NintendoSwitchController _deviceTarget;
        private CommandResponser _responser;

        public void Run()
        {
            var joyConsProDev = new JoyConsProDevice();

            joyConsProDev.AddSyncResponseContidion(report => report[0] == 0x21);
            joyConsProDev.InputRecieved += JoyConsProDev_InputRecieved;

            var error = joyConsProDev.Initialize();

            Console.ReadKey();

            joyConsProDev.Dispose();
        }

        private byte[] _curMessage = new byte[64];
        private void JoyConsProDev_InputRecieved(byte[] inputPacket)
        {
            lock (_curMessage)
            {
                Array.Copy(inputPacket, _curMessage, _curMessage.Length);
                Console.SetCursorPosition(0, 0);
                PacketOutput.WriteData(_curMessage, 16);
            }
        }

        /*public void Run()
        {
            var macAddress = InitializeAndConnectDevice();
            var control = PacketConstructor.BuildObject<ReportPacket>(Reports.Default_0x21);

            _responser = new CommandResponser {DeviceMac = macAddress};
            _deviceTarget.FeedbackReceived += DeviceFeedbackReciver;

            Console.ReadLine();

            control.ReportId = 0x30;
            _deviceTarget.SendReport(PacketConstructor.BuildPacket(control), (byte)TimerStatus.EnableAndSavePacket);

            Console.ReadLine();

            control.LeftButtons = LeftButtons.Right;
            _deviceTarget.SendReport(PacketConstructor.BuildPacket(control));

            Console.ReadLine();
        }

        private void DeviceFeedbackReciver(object sender, byte[] packet)
        {
            Console.WriteLine("Got request...");
            PacketOutput.WriteData(packet, 16);
            Console.WriteLine();

            var prevTimerEnable = _responser.IsTimerEnabled;
            var response = _responser.BuildResponsePacket(packet);
            var curTimerEnable = _responser.IsTimerEnabled;

            byte timerStatus = (byte)TimerStatus.Ignore;
            if (!prevTimerEnable && curTimerEnable)
            {
                timerStatus = (byte)TimerStatus.EnableAndSavePacket;
            }

            if (response != null)
            {
                _deviceTarget.SendReport(response, timerStatus);

                Console.WriteLine("Send response...");
                PacketOutput.WriteData(response, 16);
                Console.WriteLine("-------------------");
                Console.WriteLine();
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
        }*/

        public void Dispose()
        {
            _deviceTarget?.Disconnect();
            _deviceTarget?.Dispose();
        }
    }
}
