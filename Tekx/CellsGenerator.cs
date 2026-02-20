using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using LUTs;
using ColorOperations;
using Procedures;

namespace CellsGenerator
{
    public struct Point3D
    {
        public int X;
        public int Y;
        public int Z;

        public Point3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    enum PreWrapMode
    {
        Side4, Side8
    }
    enum MergeMode
    {
        NoMerge, Mix, Add, Sub, Mul, Or, And, Xor, WTF, Min, Max
    }
    enum PointSelectionMode
    {
        Closest, Second, Mid_1_2, Third
    }

    delegate double DistanceFunction(Point p1, Point p2);

    struct CellGenArgs
    {
        public Size size;
        public int points;
        public double points_distribution_limit;
        public double[,] points_distribution_map;
        public double facet_thickness;
        public double noise;
        public double power;
        public bool invert_colors;
        public DistanceFunction distance_function;
        public PointSelectionMode selection_mode;
        public Color from;
        public Color to;
        public Color hue_from;
        public Color hue_to;
        public bool use_spectre;
        public MergeMode merge_mode;
        public double merge_amount;
        public bool merge_overflow;
        public bool blur;
        public Bitmap prev_image;
        public bool prewrap;
        public PreWrapMode prewrap_mode;
        public bool midwrap;
        public double midwrap_amount;
        public int threads;
        public bool threads_preinit;
        public TableLayoutPanel tlp;
        public Point[] known_points;
        public bool flat_cells;

        public override string ToString()
		{
			return string.Format("[CellGenArgs Size={0}, Points={1}, Points_distribution_limit={2}, Points_distribution_map={3}, Facet_thickness={4}, Noise={5}, Power={6}, Invert_colors={7}, Distance_function={8}, Selection_mode={9}, From={10}, To={11}, Hue_from={12}, Hue_to={13}, Use_spectre={14}, Merge_mode={15}, Merge_amount={16}, Merge_overflow={17}, Blur={18}, Prev_image={19}, Prewrap={20}, Prewrap_mode={21}, Midwrap={22}, Midwrap_amount={23}, Threads={24}, Threads_preinit={25}, Tlp={26}, Known_points={27}, Flat_cells={28}]", size, points, points_distribution_limit, points_distribution_map, facet_thickness, noise, power, invert_colors, distance_function,
selection_mode, @from, to, hue_from, hue_to, use_spectre, merge_mode, merge_amount, merge_overflow, blur,
prev_image, prewrap, prewrap_mode, midwrap, midwrap_amount, threads, threads_preinit, tlp, known_points, flat_cells);
		}

        /*string ToString()
        {
            StringBuilder sb = new StringBuilder("Cell Generation Arguments: ");
            CellGenArgs c = this;

            sb.Append("Size ");
            sb.Append(c.size.Width.ToString());
            sb.Append(" x ");
            sb.Append(c.size.Height.ToString());
            sb.Append(" Points: ");
            sb.Append(c.points.ToString());
            //sb.Append(c.points_distribution_limit);
            /*
            points_distribution_map;
            facet_thickness;
            noise;
            power;
            invert_colors;
            distance_function;
            selection_mode;
            from;
            to;
            hue_from;
            hue_to;
            use_spectre;
            merge_mode;
            merge_amount;
            */
            /*
            sb.Append(c.merge_overflow.ToString());
            sb.Append(c.blur.ToString());
            sb.Append(c.prev_image.ToString());
            sb.Append(c.prewrap.ToString());
            sb.Append(c.prewrap_mode.ToString());
            sb.Append(c.known_points.ToString());
            sb.Append(c.flat_cells.ToString());
            sb.Append(c.blur.ToString());
            
            return sb.ToString();
        }*/
    }

    static class AngleDrive
    {
        public static double Angle(Point p1, Point p2)
        {
            return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }

        public static double GetForce(Point p1, Point p2)
        {
            double d; //-pi/2 < b < pi/2

            /*
            d = Math.Abs(p1.X * p2.X + p1.Y * p2.Y)
                /
                (Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y) + Math.Sqrt(p2.X * p2.X + p2.Y * p2.Y));
            */

            d = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            return d / (Math.PI * 2);
        }
    }

    class Distance
    {
        static public double Pythagorean(Point p1, Point p2)
        {
            /*
            Point tp1 = p1, tp2 = p2;
            
            if (p1.X < 0)
            {
                tp2.Offset(-p1.X, 0);
                tp1.Offset(-p1.X, 0);
            }

            if (p1.Y < 0)
            {
                tp2.Offset(0, -p1.Y);
                tp1.Offset(0, -p1.Y);
            }

            if (p2.X < 0)
            {
                tp1.Offset(-p2.X, 0);
                tp2.Offset(-p2.X, 0);
            }

            if (p2.Y < 0)
            {
                tp1.Offset(0, -p2.Y);
                tp2.Offset(0, -p2.Y);
            }
            */

            //LUT.SubAndSquare[tp2.X, tp1.X] + LUT.SubAndSquare[tp2.Y, tp1.Y];
            //if (LUT.Sqrt.Length<i)
            
            /*
            double coeff = 1;
            double angle = AngleDrive.Angle(p1, p2);
            double modifier = 10;

            modifier *= angle / (Math.PI / 2);
            coeff = 1 + (1 + Math.Cos(angle*modifier));
            */

            return Math.Sqrt((Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));// *coeff;
            /*else
                return LUT.Sqrt[i];
        */}
        static public double Pythagorean3D(Point3D p1, Point3D p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2));
        }
        static public double Manhattan(Point p1, Point p2)
        {
            return /*Math.Abs((float)Math.Abs(p2.X - p1.X) * (Math.Abs(p2.X) + Math.Abs(p1.X))) //Canberra distance
                 + Math.Abs((float)Math.Abs(p2.Y - p1.Y) * (Math.Abs(p2.Y) + Math.Abs(p1.Y)));*/
            Math.Abs(p2.X - p1.X) + Math.Abs(p2.Y - p1.Y);
        }
        static public double Manhattan3D(Point3D p1, Point3D p2)
        {
            return Math.Abs(p2.X - p1.X) + Math.Abs(p2.Y - p1.Y) + Math.Abs(p2.Z - p1.Z);
        }
        static public double Chebyshev(Point p1, Point p2)
        {
            return Math.Max(Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
        }

        static public DistanceFunction CreateCustomDistance(double norm)
        {
            return new DistanceFunction((p1, p2) => { return Math.Pow(Math.Pow(Math.Abs(p2.X - p1.X), norm) + Math.Pow(Math.Abs(p2.Y - p1.Y), norm), 1 / norm); });
        }
    }

    class Operation
    {
        static public void Normalize(ref double[] value, double norm)
        {
            if (norm == 0)
                return;

            int length = value.GetUpperBound(0) + 1;
            double max = 0;

            for (int i = 0; i < length; i++)
                {
                    if (max < Math.Abs(value[i]))
                        max = value[i];
                }

            if (max == 0)
                return;

            double ratio = norm / max;

            for (int i = 0; i < length; i++)
                {
                    value[i] = Math.Abs(value[i] * ratio);
                    value[i] = (value[i] > norm) ? norm : value[i];
                    if (double.IsNaN(value[i]))
                        value[i] = 0;
                }
        }
        static public void Normalize(ref double[,] value, double norm)
        {
            if (norm == 0)
                return;

            int width = value.GetUpperBound(0) + 1;
            int height = value.GetUpperBound(1) + 1;
            double max = 0;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (max < Math.Abs(value[i, j]))
                        max = value[i, j];
                }

            if (max == 0)
                return;

            double ratio = norm / max;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    value[i, j] = Math.Abs(value[i, j] * ratio);
                    value[i, j] = (value[i, j] > norm) ? norm : value[i, j];
                    if (double.IsNaN(value[i, j]))
                        value[i, j] = 0;
                }
        }
        static public double Mix(double a, double b, double coeff)
        {
            double anti = 1.0 - coeff;
            return a * anti + b * coeff;
        }
    }

    class Generator
    {
        static Random r = new Random();
        public static string time_1 = "???";
        public static string time_2 = "???";
        public static string time_3 = "???";

        public static Bitmap PointsView;
        public static Bitmap GradientView;

        public static int[,] indexes;

        public static Point[] pts = null;
        public static double[,] grad_values = null;

        static public Point[] Generate_Points(Size size, int amount, bool prewrap, PreWrapMode prewrap_mode, double max_radius, double[,] distribution_map)
        {
            Point[] pts;

            int i;

            try
            {
                if (amount < 0)
                    throw new Exception();
                pts = new Point[amount];
            }

            catch { return null; }

            if (distribution_map == null)
            {
                if (max_radius > 0)
                {
                    for (i = 0; i < pts.Length; i++)
                    {
                        pts[i].X = size.Height / 2 + (int)(Math.Sin(r.Next(360)) * max_radius);
                        pts[i].Y = size.Width / 2 + (int)(Math.Cos(r.Next(360)) * max_radius);
                    }
                }
                else
                    for (i = 0; i < pts.Length; i++)
                    {
                        pts[i].X = r.Next(size.Width);
                        pts[i].Y = r.Next(size.Height);
                    }
            }
            else
            {
                if ((distribution_map.GetUpperBound(0)+1 != size.Width)
                    || (distribution_map.GetUpperBound(1)+1 != size.Height))
                {
                    for (i = 0; i < pts.Length; i++)
                    {
                        pts[i].X = r.Next(size.Width);
                        pts[i].Y = r.Next(size.Height);
                    }
                }
                else
                {
                    int x, y;
                    double sum;

                    for (x = 0; x < distribution_map.GetUpperBound(0) + 1; x++)
                    {
                        sum = 0;
                        for (y = 0; y < distribution_map.GetUpperBound(1) + 1; y++)
                            sum += distribution_map[x, y];

                        for (y = 0; y < distribution_map.GetUpperBound(1) + 1; y++)
                        {
                            distribution_map[x, y] /= sum;
                        }
                    }

                    double[][] cumulative_map_x = new double[distribution_map.GetUpperBound(0) + 1][];
                    double[][] cumulative_map_y = new double[distribution_map.GetUpperBound(1) + 1][];
                    double temp_sum;

                    for (x = 0; x < distribution_map.GetUpperBound(0) + 1; x++)
                    {
                        temp_sum = 0;
                        cumulative_map_x[x] = new double[distribution_map.GetUpperBound(1) + 1];
                        for (y = 0; y < distribution_map.GetUpperBound(1) + 1; y++)
                        {
                            temp_sum += distribution_map[x, y];
                            cumulative_map_x[x][y] = temp_sum;
                        }
                    }

                    for (x = 0; x < cumulative_map_x.Length; x++)
                        Operation.Normalize(ref cumulative_map_x[x], 1.0);

                    for (y = 0; y < distribution_map.GetUpperBound(1) + 1; y++)
                        cumulative_map_y[y] = new double[distribution_map.GetUpperBound(1) + 1];

                    for (y = 0; y < distribution_map.GetUpperBound(1) + 1; y++)
                    {
                        temp_sum = 0;

                        for (x = 0; x < distribution_map.GetUpperBound(0) + 1; x++)
                        {
                            temp_sum += distribution_map[x, y];
                            cumulative_map_y[y][x] = temp_sum;
                        }
                    }

                    for (x = 0; x < cumulative_map_x.Length; x++)
                        Operation.Normalize(ref cumulative_map_y[x], 1.0);

                    for (i = 0; i < pts.Length; i++)
                    {
                        double px = r.NextDouble();
                        double py = r.NextDouble();

                        int index_x = 0;
                        int index_y = 0;

                        double fraction;
                        int j;

                        int[] possible_indexes_x = new int[cumulative_map_x.Length];
                        int[] possible_indexes_y = new int[cumulative_map_y.Length];

                        for (x = 0; x < cumulative_map_x.Length; x++)
                        {
                            index_y = (cumulative_map_x[x].Length - 1) / 2;
                            fraction = 0.25 * (cumulative_map_x.Length);

                            for (j = 0; j < Math.Log(cumulative_map_x[x].Length, 2); j++)
                            {
                                if (cumulative_map_x[x][index_y] > py)
                                    index_y -= (int)(fraction);
                                else
                                    index_y += (int)(fraction);

                                fraction /= 2;
                            }

                            possible_indexes_y[x] = index_y;
                        }

                        for (x = 0; x < possible_indexes_y.Length; x++)
                        {
                            index_x = (cumulative_map_x[x].Length - 1) / 2;
                            fraction = 0.25 * (cumulative_map_x.Length);

                            for (j = 0; j < Math.Log(cumulative_map_x.Length, 2); j++)
                            {
                                if (cumulative_map_y[possible_indexes_y[x]][index_x] > px)
                                    index_x -= (int)(fraction);
                                else
                                    index_x += (int)(fraction);

                                fraction /= 2;
                            }

                            possible_indexes_x[x] = index_x;
                        }

                        pts[i].X = possible_indexes_x[r.Next(size.Width)];//(int)(px*cumulative_map[index_x][index_y] * size.Width);
                        pts[i].Y = possible_indexes_y[r.Next(size.Height)];//(int)(py*cumulative_map[index_y][index_x] * size.Height);
                    }
                }
            }

            if (prewrap)
            {
                int j;
                int total_points = pts.Length;

                if (prewrap_mode == PreWrapMode.Side8)
                {
                    Array.Resize<Point>(ref pts, pts.Length * 9);

                    /*
                    /-\
                    |X| X=Original
                    \-/
                    */

                    for (j = 0; j < total_points; j++)
                    {
                        pts[1 * total_points + j] = new Point(pts[j].X - size.Width, pts[j].Y);
                        pts[2 * total_points + j] = new Point(pts[j].X, pts[j].Y - size.Height);
                        pts[3 * total_points + j] = new Point(pts[j].X - size.Width, pts[j].Y - size.Height);
                        pts[4 * total_points + j] = new Point(pts[j].X + size.Width, pts[j].Y);
                        pts[5 * total_points + j] = new Point(pts[j].X, pts[j].Y + size.Height);
                        pts[6 * total_points + j] = new Point(pts[j].X + size.Width, pts[j].Y + size.Height);
                        pts[7 * total_points + j] = new Point(pts[j].X + size.Width, pts[j].Y - size.Height);
                        pts[8 * total_points + j] = new Point(pts[j].X - size.Width, pts[j].Y + size.Height);
                    }
                }
                else
                    if (prewrap_mode == PreWrapMode.Side4)
                    {
                        Array.Resize<Point>(ref pts, pts.Length * 5);

                        /*
                         - 
                        |X| X=Original
                         - 
                        */

                        for (j = 0; j < total_points; j++)
                        {
                            pts[1 * total_points + j] = new Point(pts[j].X - size.Width, pts[j].Y);
                            pts[2 * total_points + j] = new Point(pts[j].X, pts[j].Y - size.Height);
                            pts[3 * total_points + j] = new Point(pts[j].X + size.Width, pts[j].Y);
                            pts[4 * total_points + j] = new Point(pts[j].X, pts[j].Y + size.Height);
                        }
                    }
            }

            return pts;
        }

        static public double[,] Compute_Gradients(Point StartCoordinates, Size size, Point[] pts, DistanceFunction GetDistance, PointSelectionMode selection_mode, double power, double facet_thickness, bool flat_cells, int threads, bool threads_preinit, TableLayoutPanel tlp)
        {
            indexes = new int[Math.Abs(size.Width), Math.Abs(size.Height)];
            int i;
            double[,] grad_values = new double[Math.Abs(size.Width), Math.Abs(size.Height)];

            if ((pts != null) && (pts.Length > 0))
            {
                if (threads <= 0)
                {
                    throw new Exception("Threads < 1?");
                }

                int step_x = (size.Width + threads) / threads;
                int step_y = (size.Height + threads) / threads;

                Thread[,] works = new Thread[threads, threads];

                ParallelGradiatorArgs[] pga_m = null;
                if (threads_preinit)
                    pga_m = new ParallelGradiatorArgs[threads * threads];

                for (i = 0; i < threads; i++)
                    for (int j = 0; j < threads; j++)
                    {
                        ParallelGradiatorArgs pga = new ParallelGradiatorArgs();
                        pga.x1 = StartCoordinates.X + step_x * i;
                        pga.x2 = StartCoordinates.X + step_x * i + step_x;
                        
                        if (pga.x2 > StartCoordinates.X + size.Width)
                            pga.x2 = StartCoordinates.X + size.Width - 1;

                        pga.y1 = StartCoordinates.Y + step_y * j;
                        pga.y2 = StartCoordinates.Y + step_y * j + step_y;

                        if (pga.y2 > StartCoordinates.Y + size.Height)
                            pga.y2 = StartCoordinates.Y + size.Height - 1;

                        pga.GetDistance = GetDistance;
                        pga.psm = selection_mode;
                        pga.power = power;
                        pga.grad_values = grad_values;
                        pga.pts = pts;
                        pga.facet_thickness = facet_thickness;
                        pga.flat_cells = flat_cells;
                        pga.indexes = indexes;

                        pga.StartCoordinates = StartCoordinates;

                        pga.tlp = tlp;
                        pga.id = i + j * threads;

                        works[i, j] = new Thread(new ParameterizedThreadStart(ParallelGradiator));
                        works[i, j].IsBackground = true;
                        if (!threads_preinit)
                            works[i, j].Start(pga);
                        else
                        {
                            pga_m[i + j * threads] = new ParallelGradiatorArgs();
                            pga_m[i + j * threads] = pga;
                        }
                    }

                if (threads_preinit)
                {
                    for (i = 0; i < threads; i++)
                        for (int j = 0; j < threads; j++)
                            works[i, j].Start(pga_m[i + j * threads]);
                }

                for (i = 0; i < threads; i++)
                    for (int j = 0; j < threads; j++)
                        while (works[i, j].IsAlive)
                            Thread.Sleep(100);
            }

            return grad_values;
        }

        static public Bitmap Generate_Cells(CellGenArgs Parameters)
        {
            DateTime d = DateTime.Now;
            TimeSpan nt;

            Bitmap b = new Bitmap(Parameters.size.Width, Parameters.size.Height);

            double power = Parameters.power;

            int x, y;

            DistanceFunction GetDistance = Parameters.distance_function;

            if (Parameters.known_points == null)
                pts = Generate_Points(Parameters.size, Parameters.points, Parameters.prewrap, Parameters.prewrap_mode, Parameters.points_distribution_limit, Parameters.points_distribution_map);
            else
                pts = Parameters.known_points;

            PointsViewUpdate(Parameters.size.Width, Parameters.size.Height);
            
            grad_values = Compute_Gradients(new Point(0,0), Parameters.size, pts, GetDistance, Parameters.selection_mode, power, Parameters.facet_thickness, Parameters.flat_cells, Parameters.threads, Parameters.threads_preinit, Parameters.tlp);
            /*
             * mid-distance percented overflow
            double[] maxdist = new double[pts.Length];
            double tdist;

            for (int i = 0; i < pts.Length; i++)
                maxdist[i] = double.MinValue;

            for (x = 0; x < Parameters.size.Width; x++)
                for (y = 0; y < Parameters.size.Height; y++)
                {
                    for (int i = 0; i < pts.Length; i++)
                    {
                        if (indexes[x, y] == i)
                        {
                            tdist = GetDistance(pts[indexes[x, y]], new Point(x, y));
                            if (tdist > maxdist[i])
                                maxdist[i] = tdist;
                        }
                    }
                }

            for (x = 0; x < Parameters.size.Width; x++)
                for (y = 0; y < Parameters.size.Height; y++)
                {
                    for (int i = 0; i < pts.Length; i++)
                    {
                        if (GetDistance(pts[indexes[x, y]], new Point(x, y)) > maxdist[i] / 2)
                            grad_values[x, y] += maxdist[i];
                    }
                }
            */
            if (Parameters.midwrap)
            {
                int x_border = (int)(Parameters.midwrap_amount * Parameters.size.Width);
                int y_border = (int)(Parameters.midwrap_amount * Parameters.size.Height);
                double[,] extra_grad_values;
                Size BorderSize;

                BorderSize = new Size(x_border, Parameters.size.Height);
                extra_grad_values = Compute_Gradients(new Point(Parameters.size.Width, 0), BorderSize, pts, GetDistance, Parameters.selection_mode, power, Parameters.facet_thickness, Parameters.flat_cells, Parameters.threads, Parameters.threads_preinit, Parameters.tlp);
                for (x = 0; x < x_border; x++)
                    for (y = 0; y < Parameters.size.Height; y++)
                        grad_values[x, y] = Operation.Mix(grad_values[x, y], extra_grad_values[x, y], (double)(x_border - x) / x_border);
                /*
                extra_grad_values = Compute_Gradients(new Point(Parameters.size.Width - x_border, 0), BorderSize, pts, GetDistance, Parameters.selection_mode, power, Parameters.facet_thickness, Parameters.flat_cells, Parameters.threads, Parameters.threads_preinit, Parameters.tlp);
                for (x = Parameters.size.Width - x_border; x < Parameters.size.Width; x++)
                    for (y = 0; y < Parameters.size.Height; y++)
                        grad_values[x, y] = Operation.Mix(base_grad_values[x, y], extra_grad_values[Parameters.size.Width - 1 - x, y], (double)(x - (Parameters.size.Width - x_border)) / x_border);
                /*
                original.GetPixel(original.Width - 1 - x, y),
                (double)(x - (result.Width - border)) / border));
                */
                BorderSize = new Size(Parameters.size.Width, y_border);
                extra_grad_values = Compute_Gradients(new Point(0, Parameters.size.Height), BorderSize, pts, GetDistance, Parameters.selection_mode, power, Parameters.facet_thickness, Parameters.flat_cells, Parameters.threads, Parameters.threads_preinit, Parameters.tlp);
                for (x = 0; x < Parameters.size.Width; x++)
                    for (y = 0; y < y_border; y++)
                        grad_values[x, y] = Operation.Mix(grad_values[x, y], extra_grad_values[x, y], (double)(y_border - y) / y_border);
            }

            nt = DateTime.Now.Subtract(d);
            time_1 = nt.ToString();
            d = DateTime.Now;

            if (Parameters.noise > 0)
            {
                Generic.Noise(ref grad_values, Parameters.noise);
            }

            Operation.Normalize(ref grad_values, 255.0);

            if (Parameters.invert_colors)
                for (x = 0; x < b.Width; x++)
                    for (y = 0; y < b.Height; y++)
                        grad_values[x, y] = 255.0 - grad_values[x, y];

            if (Parameters.blur)
            {
                for (x = 1; x < b.Width - 1; x++)
                    for (y = 1; y < b.Height - 1; y++)
                    {
                        grad_values[x, y] = (int)
                                            (
                                            (double)grad_values[x - 1, y] / 8
                                          + (double)grad_values[x, y - 1] / 8
                                          + (double)grad_values[x, y] / 2
                                          + (double)grad_values[x, y + 1] / 8
                                          + (double)grad_values[x + 1, y] / 8
                                            );
                        grad_values[x, y] = (grad_values[x, y] > 255) ? 255 : grad_values[x, y];
                    }
            }

            for (x = 0; x < b.Width; x++)
                for (y = 0; y < b.Height; y++)
                    if (double.IsNaN(grad_values[x, y]))
                        grad_values[x, y] = x;

            GradientView = new Bitmap(grad_values.GetUpperBound(0)+1, grad_values.GetUpperBound(1)+1);
            for (x = 0; x < grad_values.GetUpperBound(0)+1; x++)
                for (y = 0; y < grad_values.GetUpperBound(1)+1; y++)
                    GradientView.SetPixel(x, y, Color.FromArgb((int)grad_values[x, y],
                                                               (int)grad_values[x, y],
                                                               (int)grad_values[x, y]));

            Color from = Parameters.from;
            Color to = Parameters.to;

            int r_dif = to.R - from.R;
            int g_dif = to.G - from.G;
            int b_dif = to.B - from.B;

            if (Parameters.use_spectre)
            {
                for (x = 0; x < b.Width; x++)
                    for (y = 0; y < b.Height; y++)
                    {
                        try
                        {
                            Color HueColor = ColorOperation.HueColorize(Parameters.hue_from, Parameters.hue_to, grad_values[x, y] / 255.0);
                            b.SetPixel(x, y, HueColor//MixColors(from, HueColor, grad_values[x, y] / 255
                                /*
                                        Color.FromArgb(
                                                        (byte)((from.R + grad_values[x, y] / 255 * (r_dif)) * ((float)HueColor.R) / 255),
                                                        (byte)((from.G + grad_values[x, y] / 255 * (g_dif)) * ((float)HueColor.G) / 255),
                                                        (byte)((from.B + grad_values[x, y] / 255 * (b_dif)) * ((float)HueColor.B) / 255)

                                                        /*
                                                        (byte)((from.R + grad_values[x, y] / 255 * (r_dif)) * HueColor.R),
                                                        (byte)((from.G + grad_values[x, y] / 255 * (g_dif)) * HueColor.G),
                                                        (byte)((from.B + grad_values[x, y] / 255 * (b_dif)) * HueColor.B)
                                        /*
                                                        LUTs.ColorMix[from.R, (byte)(grad_values[x, y]), r_dif],
                                                        LUTs.ColorMix[from.G, (byte)(grad_values[x, y]), g_dif],
                                                        LUTs.ColorMix[from.B, (byte)(grad_values[x, y]), b_dif]*/
                                //   )
                                        );
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Some weird thing happened while coloring:\r\n" + ex.Message + ex.Data);
                            for (; x < b.Width; x++)
                                for (; y < b.Height; y++)
                                    b.SetPixel(x, y, Color.Black);
                            return b;
                        }
                    }
            }
            else
            {
                for (x = 0; x < b.Width; x++)
                    for (y = 0; y < b.Height; y++)
                    {
                        try
                        {
                            Color HueColor = ColorOperation.HueColorize(Parameters.hue_from, Parameters.hue_to, grad_values[x, y] / 255.0);
                            b.SetPixel(x, y, Color.FromArgb(
                                                        (byte)((from.R + grad_values[x, y] / 255 * (r_dif))),
                                                        (byte)((from.G + grad_values[x, y] / 255 * (g_dif))),
                                                        (byte)((from.B + grad_values[x, y] / 255 * (b_dif))))

                                      );
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Some weird thing happened while coloring:\r\n" + ex.Message);
                            for (; x < b.Width; x++)
                                for (; y < b.Height; y++)
                                    b.SetPixel(x, y, Color.Black);
                            return b;
                        }
                    }
            }

            nt = DateTime.Now.Subtract(d);
            time_2 = nt.ToString();
            d = DateTime.Now;

            if (Parameters.prev_image != null)
                if (Parameters.merge_mode != MergeMode.NoMerge)
                {
                    Color c; Color c2; Color result;

                    Bitmap original = Parameters.prev_image;
                    /*
                     * Move previous bitmap to the center of increased bitmap
                    if (original.Width < b.Width)
                    {
                        Bitmap tmp = new Bitmap(b.Width, b.Height);
                        int cx = b.Width / 2 - original.Width / 2;
                        int cy = b.Height / 2 - original.Height / 2;

                        for (x = 0; x < tmp.Width; x++)
                            for (y = 0; y < tmp.Height; y++)
                                tmp.SetPixel(x, y, from);

                        for (x = cx; x < cx + original.Width; x++)
                            for (y = cx; y < cx + original.Height; y++)
                                tmp.SetPixel(x, y, original.GetPixel(x - cx, y - cy));

                        original = (Bitmap)tmp.Clone();
                        tmp.Dispose();
                    }
                    */

                    double mix_amount = Parameters.merge_amount;

                    double anti_mix_amount = 1 - mix_amount;
                    if (anti_mix_amount < 0)
                        anti_mix_amount = 0;

                    for (x = 0; x < b.Width; x++)
                        for (y = 0; y < b.Height; y++)
                        {
                            c = b.GetPixel(x, y);
                            c = Color.FromArgb((int)(c.R * mix_amount), (int)(c.G * mix_amount), (int)(c.B * mix_amount));
                            c2 = original.GetPixel(x, y);
                            c2 = Color.FromArgb((int)(c2.R * anti_mix_amount), (int)(c2.G * anti_mix_amount), (int)(c2.B * anti_mix_amount));

                            result = Color.White;

                            if (Parameters.merge_mode == MergeMode.Mix)
                                result = Color.FromArgb(c.R + c2.R,
                                                      c.G + c2.G,
                                                      c.B + c2.B);
                            else
                                if (Parameters.merge_mode == MergeMode.And)
                                    result = Color.FromArgb((c.R & c2.R), (c.G & c2.G), (c.B & c2.B));
                                else
                                    if (Parameters.merge_mode == MergeMode.Or)
                                        result = Color.FromArgb((c.R | c2.R), (c.G | c2.G), (c.B | c2.B));
                                    else
                                        if (Parameters.merge_mode == MergeMode.Xor)
                                            result = Color.FromArgb((c.R ^ c2.R), (c.G ^ c2.G), (c.B ^ c2.B));
                                        else
                                            if (Parameters.merge_mode == MergeMode.Add)
                                                result = Color.FromArgb((c.R + c2.R) % 256, (c.G + c2.G) % 256, (c.B + c2.B) % 256);
                                            else
                                                if (Parameters.merge_mode == MergeMode.Sub)
                                                    result = Color.FromArgb(Math.Abs(c.R - c2.R), Math.Abs(c.G - c2.G), Math.Abs(c.B - c2.B));
                                                else
                                                    if (Parameters.merge_mode == MergeMode.Mul)
                                                        result = Color.FromArgb((int)(c.R * ((double)c2.R) / 255) % 256,
                                                                                (int)(c.G * ((double)c2.G) / 255) % 256,
                                                                                (int)(c.B * ((double)c2.B) / 255) % 256);
                                                    else
                                                        if (Parameters.merge_mode == MergeMode.WTF)
                                                            result = Color.FromArgb(Math.Abs((int)(128 + (Math.Sin(c.R) + Math.Cos(c2.R)) * 63) % 256),
                                                                                    Math.Abs((int)(128 + (Math.Sin(c.G) + Math.Cos(c2.G)) * 63) % 256),
                                                                                    Math.Abs((int)(128 + (Math.Sin(c.B) + Math.Cos(c2.B)) * 63) % 256));
                                                        else
                                                            if (Parameters.merge_mode == MergeMode.Min)
                                                                result = Color.FromArgb(Math.Min(c.R, c2.R),
                                                                                        Math.Min(c.G, c2.G),
                                                                                        Math.Min(c.B, c2.B));
                                                            else
                                                                if (Parameters.merge_mode == MergeMode.Max)
                                                                    result = Color.FromArgb(Math.Max(c.R, c2.R),
                                                                                            Math.Max(c.G, c2.G),
                                                                                            Math.Max(c.B, c2.B));

                            if (Parameters.merge_overflow)
                            {
                                result = Color.FromArgb(result.R * 2 % 256,
                                                        result.G * 2 % 256,
                                                        result.B * 2 % 256);
                            }

                            b.SetPixel(x, y, result);
                        }
                }

            nt = DateTime.Now.Subtract(d);
            time_3 = nt.ToString();

            return b;
        }

        struct ParallelGradiatorArgs
        {
            public int x1;
            public int x2;
            public int y1;
            public int y2;
            public Point StartCoordinates;
            public DistanceFunction GetDistance;
            public Point[] pts;
            public PointSelectionMode psm;
            public double[,] grad_values;
            public int[,] indexes;
            public double power;
            public double facet_thickness;
            public TableLayoutPanel tlp;
            public int id;
            public bool flat_cells;
        }

        static void ParallelGradiator(object o)
        {
            ParallelGradiatorArgs pga = (ParallelGradiatorArgs)o;

            pga.tlp.Controls[pga.id].BackColor = Color.Red;

            /*
            int x1 = Math.Min(pga.x1, pga.x2);
            int x2 = Math.Max(pga.x1, pga.x2);
            int y1 = Math.Min(pga.y1, pga.y2);
            int y2 = Math.Max(pga.y1, pga.y2);
            */
            int x1 = pga.x1;
            int x2 = pga.x2;
            int y1 = pga.y1;
            int y2 = pga.y2;
            int width = x2 - x1;
            int ok25 = x1 + width / 4;
            int ok50 = x1 + width / 2;
            int ok75 = x1 + width / 4 * 3;

            DistanceFunction GetDistance = pga.GetDistance;
            Point[] pts = pga.pts;
            PointSelectionMode psm = pga.psm;
            double power = pga.power;
            double[,] grad_values = pga.grad_values;

            Point pt = new Point(); double t;
            int closest_index, prev_index, third_index;

            int x, y;
            int ix = x1 - pga.StartCoordinates.X, iy;
            
            for (x = x1; x <= x2; x++)
            {
                iy = y1 - pga.StartCoordinates.Y;

                for (y = y1; y <= y2; y++)
                {
                    pt.X = x; pt.Y = y;

                    t = double.MaxValue;

                    closest_index = 0;
                    prev_index = 0;
                    third_index = 0;

                    for (int i = 0; i < pts.Length; i++)
                        if (t > GetDistance(pt, pts[i]))
                        {
                            t = GetDistance(pt, pts[i]);

                            third_index = prev_index;
                            prev_index = closest_index;
                            closest_index = i;
                        }

                    indexes[ix, iy] = closest_index;

                    if (pga.flat_cells)
                        switch (psm)
                        {
                            case PointSelectionMode.Closest:
                                grad_values[ix, iy] = closest_index;// % (pts.Length/9);//for PreWrap-8
                                break;

                            case PointSelectionMode.Second:
                                grad_values[ix, iy] = prev_index;
                                break;

                            case PointSelectionMode.Mid_1_2:
                                grad_values[ix, iy] = (prev_index + closest_index) / 2;
                                break;

                            case PointSelectionMode.Third:
                                grad_values[ix, iy] = third_index;
                                break;
                        }
                    else
                    {
                        switch (psm)
                        {
                            case PointSelectionMode.Closest:
                                grad_values[ix, iy] = GetDistance(pt, pts[closest_index]);
                                break;

                            case PointSelectionMode.Second:
                                grad_values[ix, iy] = GetDistance(pt, pts[prev_index]);
                                break;

                            case PointSelectionMode.Mid_1_2:
                                grad_values[ix, iy] = GetDistance(pt,
                                                                  new Point((pts[prev_index].X + pts[closest_index].X) / 2,
                                                                            (pts[prev_index].Y + pts[closest_index].Y) / 2));
                                break;

                            case PointSelectionMode.Third:
                                grad_values[ix, iy] = GetDistance(pt, pts[third_index]);
                                break;
                        }
                    }

                    if (pga.facet_thickness > 0)
                        if (Math.Abs(GetDistance(pt, pts[closest_index]) - GetDistance(pt, pts[prev_index])) < pga.facet_thickness)
                            grad_values[ix, iy] *= 2;

                    if (!pga.flat_cells)
                        grad_values[ix, iy] = Math.Pow(grad_values[ix, iy], power);

                    if (iy < grad_values.GetUpperBound(1))
                        iy++;
                }

                if (x == ok25)
                    pga.tlp.Controls[pga.id].BackColor = Color.Yellow;
                else
                if (x == ok50)
                    pga.tlp.Controls[pga.id].BackColor = Color.Green;
                else
                if (x == ok75)
                    pga.tlp.Controls[pga.id].BackColor = Color.Blue;

                if (ix < grad_values.GetUpperBound(0))
                    ix++;
            }
            pga.tlp.Controls[pga.id].BackColor = Color.White;
        }

        public static void PointsViewUpdate(int width, int height)
        {
            PointsView = new Bitmap(width, height);

            int coeff_x = 1 + width / 100;
            int coeff_y = 1 + height / 100;

            int x, y, i;

            for (x = 0; x < width; x++)
                for (y = 0; y < height; y++)
                {
                    PointsView.SetPixel(x, y, Color.Black);
                    for (i = 0; i < pts.Length; i++)
                        if ((Math.Abs(pts[i].X - x) < coeff_x) && (Math.Abs(pts[i].Y - y) < coeff_y))
                        {
                            PointsView.SetPixel(x, y, Color.White);
                            break;
                        }
                }
        }
    }
}