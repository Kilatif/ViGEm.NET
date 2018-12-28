using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports.Input;

namespace ViGEmClient.Net.Demo
{
    class JoyConDevice : IDisposable
    {
        private HidDevice _hidDevice;
        private HidStream _hidStream;
        private HidDeviceInputReceiver _inputReceiver;
        private byte[] _reportBuffer;

        public event Action<byte[]> InputReportRecived;

        public bool InitializeDevice(int productId)
        {
            var list = DeviceList.Local;
            var hidDeviceList = list.GetHidDevices().ToArray();
            _hidDevice = hidDeviceList.FirstOrDefault(dev => dev.ProductID == productId);

            if (_hidDevice == null)
            {
                return false;
            }

            var isSuccess = _hidDevice.TryOpen(out _hidStream);
            if (!isSuccess)
            {
                return false;
            }

            _hidStream.ReadTimeout = Timeout.Infinite;

            _reportBuffer = new byte[_hidDevice.GetMaxInputReportLength()];
            _inputReceiver = _hidDevice.GetReportDescriptor().CreateHidDeviceInputReceiver();
            _inputReceiver.Received += OnReportReceived;
            _inputReceiver.Start(_hidStream);

            return true;
        }

        public void SendOutputReport(byte[] report)
        {
            _hidStream.Write(report);
        }

        private void OnReportReceived(object sender, EventArgs args)
        {
            while (_inputReceiver.TryRead(_reportBuffer, 0, out _))
            {
             
            }

            InputReportRecived?.Invoke(_reportBuffer);
        }

        public void Dispose()
        {
            if (_inputReceiver != null)
            {
                _inputReceiver.Received -= OnReportReceived;
            }

            _hidStream?.Dispose();
        }
    }
}
