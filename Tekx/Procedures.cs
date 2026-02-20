using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;

namespace Procedures
{
    static class ProcedureReflector
    {
    }

    static class Generic
    {
        private static Random r = new Random();

        static public void Noise(ref double[,] data, double amount)
        {
            int width = data.GetLength(0);
            int height = data.GetLength(1);
            int x, y;

            for (x = 0; x < width; x++)
                for (y = 0; y < height; y++)
                    data[x, y] += r.NextDouble() * amount;
        }

        static public void Carve(Bitmap b)
        {

        }
    }
}