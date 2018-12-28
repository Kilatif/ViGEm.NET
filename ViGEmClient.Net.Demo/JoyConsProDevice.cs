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
    enum MainJoyCon
    {
        Left, Right
    }

    enum ErrorCode
    {
        NoError,
        LeftJoyConInitializeError,
        RightJoyConInitializeError,
    }

    class JoyConsProDevice : IDisposable
    {
        private const int LeftJoyConProductId = 0x2006;
        private const int RightJoyConProductId = 0x2007;

        private JoyConDevice _leftJoyCon;
        private JoyConDevice _rightJoyCon;

        private MainJoyCon _mainJoyCon;

        public ErrorCode Initialize()
        {
            _leftJoyCon = new JoyConDevice();
            _rightJoyCon = new JoyConDevice();

            if (!_leftJoyCon.InitializeDevice(LeftJoyConProductId))
            {
                return ErrorCode.LeftJoyConInitializeError;
            }

            if (!_rightJoyCon.InitializeDevice(LeftJoyConProductId))
            {
                return ErrorCode.RightJoyConInitializeError;
            }

            _leftJoyCon.InputReportRecived += OnLeftJoyConInputReceived;
            _rightJoyCon.InputReportRecived += OnRightJoyConInputReceived;

            return ErrorCode.NoError;
        }

        private void OnLeftJoyConInputReceived(byte[] report)
        {
            
        }

        private void OnRightJoyConInputReceived(byte[] report)
        {
            
        }

        public void Dispose()
        {
            _leftJoyCon.Dispose();
            _rightJoyCon.Dispose();
        }
    }
}
