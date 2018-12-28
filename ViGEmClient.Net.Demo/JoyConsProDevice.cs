using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports.Input;

namespace ViGEmClient.Net.Demo
{
    enum JoyConType
    {
        Left, Right
    }

    enum ErrorCode
    {
        NoError,
        LeftJoyConInitializeError,
        RightJoyConInitializeError,
    }

    class SyncResponse
    {
        public byte[] LeftReport;
        public byte[] RightReport;
        public Func<byte[], bool> Condition;
    }

    class JoyConsProDevice : IDisposable
    {
        private const int LeftJoyConProductId = 0x2006;
        private const int RightJoyConProductId = 0x2007;

        private JoyConDevice _leftJoyCon;
        private JoyConDevice _rightJoyCon;

        private byte[] _leftJoyConReport;
        private byte[] _rightJoyConReport;

        private readonly List<SyncResponse> _syncResponses;
        private JoyConType _mainJoyCon = JoyConType.Left;

        public event Action<byte[]> InputRecieved;

        public JoyConsProDevice()
        {
            _syncResponses = new List<SyncResponse>();
        }

        public ErrorCode Initialize()
        {
            _leftJoyCon = new JoyConDevice();
            _rightJoyCon = new JoyConDevice();

            if (!_leftJoyCon.InitializeDevice(LeftJoyConProductId))
            {
                return ErrorCode.LeftJoyConInitializeError;
            }

            if (!_rightJoyCon.InitializeDevice(RightJoyConProductId))
            {
                return ErrorCode.RightJoyConInitializeError;
            }

            SetupFullInputMode();

            _leftJoyCon.InputReportRecived += report => JoyConInputReceived(report, JoyConType.Left);
            _rightJoyCon.InputReportRecived += report => JoyConInputReceived(report, JoyConType.Right);

            return ErrorCode.NoError;
        }

        private void SetupFullInputMode()
        {
            var requestPacket = PacketConstructor.BuildPacket(new OutputReportPacket
            {
                CommandId = 0x01,
                SubCommandId = 0x03,
                SubCommandData = new byte[] {0x30}
            });

            SendOutputReport(requestPacket);
        }

        public void SendOutputReport(byte[] report)
        {
            _leftJoyCon.SendOutputReport(report);
            _rightJoyCon.SendOutputReport(report);
        }

        public void AddSyncResponseContidion(Func<byte[], bool> condition)
        {
            _syncResponses.Add(new SyncResponse{Condition = condition});
        }

        private void JoyConInputReceived(byte[] report, JoyConType joyConType)
        {
            var wasSyncResponse = false;
            foreach (var syncResponse in _syncResponses)
            {
                if (syncResponse.Condition(report))
                {
                    wasSyncResponse = true;
                    if (joyConType == JoyConType.Left)
                    {
                        syncResponse.LeftReport = report;
                    }
                    else if (joyConType == JoyConType.Right)
                    {
                        syncResponse.RightReport = report;
                    }

                    if (syncResponse.LeftReport != null && syncResponse.RightReport != null)
                    {
                        InputRecieved?.Invoke(BuildUnionReport(syncResponse.LeftReport, syncResponse.RightReport));
                        syncResponse.LeftReport = null;
                        syncResponse.RightReport = null;
                        break;
                    }
                }
            }

            if (!wasSyncResponse)
            {
                if (joyConType == JoyConType.Left)
                {
                    _leftJoyConReport = report;
                }
                else if (joyConType == JoyConType.Right)
                {
                    _rightJoyConReport = report;
                }

                if (_leftJoyConReport != null && _rightJoyConReport != null)
                {
                    InputRecieved?.Invoke(BuildUnionReport(_leftJoyConReport, _rightJoyConReport));
                }
            }
        }

        private byte[] BuildUnionReport(byte[] leftReport, byte[] rightReport)
        {
            var cmd = leftReport[0];
            switch (cmd)
            {
                case 0x30: return Reports.BuildUnionReport_0x30(leftReport, rightReport, _mainJoyCon);
                case 0x21: return Reports.BuildUnionReport_0x21(leftReport, rightReport, _mainJoyCon);
                default: return null;
            }
        }

        public void Dispose()
        {
            _leftJoyCon.Dispose();
            _rightJoyCon.Dispose();
        }
    }
}
