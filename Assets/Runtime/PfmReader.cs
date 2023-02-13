using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Runtime
{
    public static class PfmReader
    {
        // See F:\ML Data\MiDaS
        public static float[,] Read(string fileName)
        {
            using (var memoryReader = new MemoryStream(File.ReadAllBytes(fileName)))
            using (var reader = new BinaryReader(memoryReader))
            {
                // https://www.pauldebevec.com/Research/HDR/PFM/
                // Pf
                // 512 512
                // -1.000000
                var pf = ReadString(reader);
                if (pf != "Pf")
                {
                    throw new InvalidOperationException("Expected Pf!");
                }

                var res = ReadString(reader).Split(' ');
                var width = int.Parse(res[0]);
                var height = int.Parse(res[1]);
                var depths = new float[width, height];
                var byteOrder = float.Parse(ReadString(reader), CultureInfo.InvariantCulture);

                if (byteOrder >= 0)
                {
                    throw new InvalidOperationException("Expected -1");
                }

                var min = float.MaxValue;
                var max = float.MinValue;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var val = reader.ReadSingle();
                        depths[x, y] = val;// height - y - 1
                        min = Math.Min(min, val);
                        max = Math.Max(max, val);
                    }
                }

                Normalize(max, min, width, height, depths);

                return depths;
            }
        }

        private static void Normalize(float max, float min, int width, int height, float[,] depths)
        {
            var range = max - min;
            var invRange = range > 0 ? 1 / range : 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    depths[x, y] = (depths[x, y] - min) * invRange;
                }
            }
        }

        private static string ReadString(BinaryReader reader)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var letter = reader.ReadChar();
                if (letter != '\n')
                {
                    sb.Append(letter);
                }
                else
                {
                    return sb.ToString();
                }
            }
        }
    }
}