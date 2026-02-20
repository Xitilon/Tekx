using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LUTs
{
    static class LUT
    {
        static public float[] Sqrt = new float[2048 * 2048 * 2];
        static public int[,] SubAndSquare = new int[2048, 2048];

        static public void Init()
        {
            int i;

            for (i = 0; i < Sqrt.Length; i++)
                Sqrt[i] = (float)Math.Sqrt(i);
                    
            int j;

            for (i = 0; i < 2048; i++)
                for (j = 0; j < 2048; j++)
                    SubAndSquare[i, j] = (int)Math.Pow(i - j, 2);
        }
    }
}