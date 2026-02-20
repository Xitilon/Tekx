using System;
using System.Drawing;

namespace MiniTekx
{
	class Program
	{
		private static Color MixColors(Color c1, Color c2, double ratio)
		{
			if (ratio>1)
				ratio=1;
			
			return Color.FromArgb((int)(c1.R * ratio + c2.R * (1-ratio)),
			                      (int)(c1.G * ratio + c2.G * (1-ratio)),
			                      (int)(c1.B * ratio + c2.B * (1-ratio)));
		}
		
		private static double Distance(Point point1, Point point2)
		{
		    int a = point2.X - point1.X;
		    int b = point2.Y - point1.Y;
		
		    return Math.Sqrt(a * a + b * b);
		}
		
		private static void Normalize(ref double[,] matrix)
		{
			double maximum=double.NegativeInfinity;
			
			for (int x=0; x<matrix.GetLength(0); x++)
				for (int y=0; y<matrix.GetLength(1); y++)
					if (matrix[x,y]>maximum)
						maximum=matrix[x,y];
			
			for (int x=0; x<matrix.GetLength(0); x++)
				for (int y=0; y<matrix.GetLength(1); y++)
					matrix[x,y]/=maximum;
		}
		
		private static void ExpandPoints8(ref Point[] array, Size size)
		{
			int num8 = array.Length;
			Array.Resize<Point>(ref array, array.Length * 9);
			for (int l = 0; l < num8; l++)
			{
				array[num8 + l] = new Point(array[l].X - size.Width, array[l].Y);
				array[2 * num8 + l] = new Point(array[l].X, array[l].Y - size.Height);
				array[3 * num8 + l] = new Point(array[l].X - size.Width, array[l].Y - size.Height);
				array[4 * num8 + l] = new Point(array[l].X + size.Width, array[l].Y);
				array[5 * num8 + l] = new Point(array[l].X, array[l].Y + size.Height);
				array[6 * num8 + l] = new Point(array[l].X + size.Width, array[l].Y + size.Height);
				array[7 * num8 + l] = new Point(array[l].X + size.Width, array[l].Y - size.Height);
				array[8 * num8 + l] = new Point(array[l].X - size.Width, array[l].Y + size.Height);
			}
		}
		
		public static void Main(string[] args)
		{
			if (args.Length!=9)
			{
				Console.WriteLine("Need arguments: width, height, color1, color2, points, iterations, bitcrush, wrap (true or false), facet power");
				return;
			}
			
			try
			{
				Size size=new Size(int.Parse(args[0]), int.Parse(args[1]));
				Bitmap b=new Bitmap(size.Width, size.Height);
				double[,] matrix=new double[b.Width,b.Height];
				
				Color c1=Color.FromArgb(int.Parse(args[2]));
				Color c2=Color.FromArgb(int.Parse(args[3]));
				
				Random r=new Random();
				
				double[,] matrix2=new double[b.Width,b.Height];
				for (int iteration=0; iteration<int.Parse(args[5]); iteration++)
				{
				Console.WriteLine("Points...");
				
				Point[] pts=new Point[int.Parse(args[4])];
				for (int p=0; p<pts.Length; p++)
				{
					pts[p]=new Point(r.Next(b.Width),r.Next(b.Height));
					Console.WriteLine(pts[p]);
				}
				
				if (args[7]=="true")
					ExpandPoints8(ref pts, size);
				
				Console.WriteLine("Matrix...");
				
				double mindist, newdist;
				double facet_power=double.Parse(args[8]);
				for (int x=0; x<b.Width; x++)
					for (int y=0; y<b.Height; y++)
					{
						Point p1=new Point(x,y);
						mindist=double.MaxValue;
						newdist=-1;
						for (int p=0; p<pts.Length; p++)
						{
							newdist=Distance(p1,pts[p]);
							if (newdist<mindist)
							{
								if (Math.Abs(mindist-newdist)<3)
									mindist=newdist*facet_power;
								else
									mindist=newdist;
							}
						}
						matrix2[x,y]=mindist;
					}
				
				if (iteration==0)
				{
					for (int x=0; x<b.Width; x++)
						for (int y=0; y<b.Height; y++)
							matrix[x,y]=matrix2[x,y];
				}
				else
				{
					for (int x=0; x<b.Width; x++)
						for (int y=0; y<b.Height; y++)
							matrix[x,y]=Math.Abs(matrix2[x,y]-matrix[x,y]);
				}
				}
				
				for (int x=0; x<b.Width; x+=b.Width / 14)
				{
					for (int y=0; y<b.Height; y+=b.Height / 14)
						Console.Write(matrix[x,y].ToString("0000")+" ");
					Console.WriteLine();
				}
				
				long bitcrush=long.Parse(args[6]);
				if (bitcrush==0)
					Console.WriteLine("Bitcrush 0 - skipping.");
				else
				for (int x=0; x<b.Width; x++)
					for (int y=0; y<b.Height; y++)
							matrix[x,y]=(((long)matrix[x,y]) / bitcrush)*bitcrush;
				
				Normalize(ref matrix);
				
				Console.WriteLine("Bitmap...");
				for (int x=0; x<b.Width; x++)
					for (int y=0; y<b.Height; y++)
						b.SetPixel(x,y,MixColors(c1,c2,matrix[x,y]));
				
				Console.WriteLine("Output...");
				b.Save("Out.png");
				Console.Write("Finished. Press any key to exit.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.Write("Got exception.");
			}
		}
	}
}