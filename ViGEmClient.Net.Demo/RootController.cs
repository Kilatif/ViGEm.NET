using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using static Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4Buttons;

namespace ViGEmClient.Net.Demo
{
    class RootController
    {
        public void Run()
        {
            var isTimerEnabled = false;
            var prevResponse = Reports.Packet_0x8104;
            var vClient = new Nefarius.ViGEm.Client.ViGEmClient();
            var dsTarget = new DualShock4Controller(vClient);

            dsTarget.Connect();

            dsTarget.FeedbackReceived += (sender, report) =>
            {
                Console.WriteLine("Got request...");
                WriteData(report, 16);
                Console.WriteLine();

                var timerEnable = false;
                byte timerStatus = 2;
                var response = Reports.GetResponse(report, ref timerEnable);

                if (!isTimerEnabled && timerEnable)
                {
                    timerStatus = 1;
                    isTimerEnabled = true;
                }
                
                if (response != null)
                {
                    dsTarget.SendReport(response, timerStatus);

                    Console.WriteLine("Send response...");
                    WriteData(response, 16);
                    Console.WriteLine("-------------------");
                    Console.WriteLine();

                    if (isTimerEnabled)
                    {
                        dsTarget.SendReport(prevResponse);
                    }
                }
            };

            //dsTarget.SendReport(prevResponse);

            Console.ReadLine();

            //prevResponse
            prevResponse[5] = 0x04;
            dsTarget.SendReport(prevResponse, 1);

            Console.ReadLine();

            prevResponse[5] = 0x00;
            dsTarget.SendReport(prevResponse);

            Console.ReadLine();

            //prevResponse[5] = 0x00;
            dsTarget.SendReport(prevResponse, 0);

            Console.ReadLine();

            dsTarget.Disconnect();
        }

        private void WriteData(byte[] data, int splitCount = -1)
        {
            var splitData = Split(data, splitCount < 0 ? data.Length : splitCount);
            Console.WriteLine(string.Join("\n", splitData.Select(arr => $"{string.Join(" ", arr.Select(v => Convert.ToString(v, 16).ToUpper().PadLeft(2, '0')).ToArray())}").ToArray()));
            Console.WriteLine();
        }

        private T[][] Split<T>(T[] data, int arrayCount)
        {
            var result = new T[(int)Math.Ceiling((decimal)data.Length / arrayCount)][];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new T[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    var index = i * arrayCount + j;
                    result[i][j] = index >= data.Length ? default(T) : data[index];
                }
            }

            return result;
        }
    }
}
