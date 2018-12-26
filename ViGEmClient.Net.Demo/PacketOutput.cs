using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViGEmClient.Net.Demo
{
    static class PacketOutput
    {
        public static void WriteData(byte[] data, int splitCount = -1)
        {
            var splitData = Split(data, splitCount < 0 ? data.Length : splitCount);
            Console.WriteLine(string.Join("\n", splitData.Select(arr => $"{string.Join(" ", arr.Select(v => Convert.ToString(v, 16).ToUpper().PadLeft(2, '0')).ToArray())}").ToArray()));
            Console.WriteLine();
        }

        public static T[][] Split<T>(T[] data, int arrayCount)
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
