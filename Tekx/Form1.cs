using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.IO;

using CellsGenerator;
using LUTs;
using ColorOperations;

namespace Tekx
{
    public partial class Form1 : Form
    {
        delegate DistanceFunction CrossOperation(DistanceFunction df1, DistanceFunction df2);

        static Random r = new Random();
        static PictureBox[] PB = new PictureBox[16];
        static PictureBox[] PB2 = new PictureBox[4];
        static int picture_index = 0;
        static int picture_index2 = 0;
        static string prev_filename = "";

        const string version = "0.8 github public 2026";
        
        BackgroundWorker bw;
        public Form1()
        {
            LUT.Init();
            InitializeComponent();
            this.Text += " V" + version;
        }

        private void ReSelectPBox(object sender, MouseEventArgs e)
        {
            Point actual_mouse = MousePosition;
            actual_mouse.X -= f2.Location.X;
            actual_mouse.Y -= f2.Location.Y;
            actual_mouse.X -= 20;
            actual_mouse.Y -= 40;

            for (int i = 0; i < PB.Length; i++)
            {
                if (PB[i].Bounds.Contains(actual_mouse))
                {
                    PB[picture_index].BorderStyle = BorderStyle.None;
                    PB[i].BorderStyle = BorderStyle.Fixed3D;
                    picture_index = i;
                    break;
                }
            }
        }

        private void ReSelectPBox2(object sender, MouseEventArgs e)
        {
            Point actual_mouse = MousePosition;
            actual_mouse.X -= f2.Location.X;
            actual_mouse.Y -= f2.Location.Y;
            actual_mouse.X -= 20;
            actual_mouse.Y -= 40;

            for (int i = 0; i < PB2.Length; i++)
            {
                if (PB2[i].Bounds.Contains(actual_mouse))
                {
                    PB2[picture_index2].BorderStyle = BorderStyle.None;
                    PB2[i].BorderStyle = BorderStyle.Fixed3D;
                    picture_index2 = i;
                    break;
                }
            }
        }

        private object[] Op(object[] i)
        {
            return i;
        }

        static Form2 f2;
        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            ProcessorBlock p = new ProcessorBlock();
            p.In = new NamedParameters[10];
            p.Operation = Op;
            p.Work();
            */
            f2 = new Form2();
            f2.f1 = this;
            f2.Show();

            this.Left = f2.Width+f2.Left;

            for (int i = 0; i < 16; i++)
            {
                PB[i] = new PictureBox();
                PB[i].Image = new Bitmap(128, 128);
                PB[i].Dock = DockStyle.Left;
                PB[i].Size = new Size(128, 128);
                ((System.ComponentModel.ISupportInitialize)(PB[i])).BeginInit();
                PB[i].MouseClick += new MouseEventHandler(ReSelectPBox);
                f2.tableLayoutPanel1.Controls.Add(PB[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                PB2[i] = new PictureBox();
                PB2[i].Image = new Bitmap(256, 256);
                PB2[i].Dock = DockStyle.Left;
                PB2[i].Size = new Size(256, 256);
                ((System.ComponentModel.ISupportInitialize)(PB2[i])).BeginInit();
                PB2[i].MouseClick += new MouseEventHandler(ReSelectPBox2);
                f2.tableLayoutPanel3.Controls.Add(PB2[i]);
            }

            f2.pictureBox1.Image = new Bitmap(512, 512);
            f2.pictureBox2.BackgroundImage = new Bitmap(256, 256);
            b = (Bitmap)PB[0].Image;
            PB[0].BorderStyle = BorderStyle.Fixed3D;
            PB2[0].BorderStyle = BorderStyle.Fixed3D;
            textBox7_TextChanged(null, null);

            b = new Bitmap(128, 128);

            Oscillator o1 = new Oscillator(Oscillator.OscillatorFunctions.Sine);
            Oscillator o2 = new Oscillator(Oscillator.OscillatorFunctions.Cosine);

            int value;
            double xd, yd;
            double rootx, rooty;
            for (int x = 0; x < 128; x++)
                for (int y = 0; y < 128; y++)
                {
                    xd = (double)(x) / 128;
                    yd = (double)(y) / 128;
                    rootx = Math.Sqrt(o1[xd]);
                    rooty = Math.Sqrt(o2[yd]);
                    value = (int)(rootx * rooty * 255);
                    b.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            GetGeneratedImage(null, null);

        }

        CellGenArgs cga;
        bool cga_ok;
        private void PrepareCellGenArgs()
        {
            cga = new CellGenArgs();
            cga_ok = false;

            if (f2.tabControl1.SelectedIndex == 0)
                cga.size = new Size(128, 128);
            else
                if (f2.tabControl1.SelectedIndex == 1)
                    cga.size = new Size(256, 256);
                else
                    if (f2.tabControl1.SelectedIndex == 2)
                        cga.size = f2.pictureBox1.Image.Size;
                    else
                        if (f2.tabControl1.SelectedIndex == 3)
                            cga.size = f2.pictureBox2.BackgroundImage.Size;
                        else
                            if (f2.tabControl1.SelectedIndex == 4)
                            {
                                try
                                {
                                    cga.size = new Size(int.Parse(textBox10.Text), int.Parse(textBox11.Text));
                                }
                                catch { MessageBox.Show("Dimensions?"); return; }
                                if (!Directory.Exists(f2.textBox8.Text))
                                {
                                    MessageBox.Show("Output directory?"); return;
                                }
                            }

            try { double power = double.Parse(textBox2.Text); cga.power = power; }
            catch { MessageBox.Show("Power?"); return; }

            try { double noise = double.Parse(textBox4.Text); cga.noise = noise; }
            catch { MessageBox.Show("Noise?"); return; }

            try { int threads = int.Parse(textBox7.Text); cga.threads = threads; }
            catch { MessageBox.Show("Threads?"); return; }

            cga.threads_preinit = checkBox10.Checked;

            cga.invert_colors = checkBox1.Checked;

            DistanceFunction GetDistance1 = null;
            DistanceFunction GetDistance2 = null;

            if (radioButton5.Checked)
                GetDistance1 = Distance.Pythagorean;
            else
                if (radioButton6.Checked)
                    GetDistance1 = Distance.Manhattan;
                else
                    if (radioButton27.Checked)
                        GetDistance1 = Distance.Chebyshev;
                    else
                        if (radioButton35.Checked)
                        {
                            double norm;
                            if ((!double.TryParse(textBox20.Text, out norm)))// || (norm == 0))
                            {
                                MessageBox.Show("Norm 1?");
                                return;
                            }
                            GetDistance1 = Distance.CreateCustomDistance(norm);
                        }

            if (checkBox9.Checked)
            {
                int leap;
                if ((!int.TryParse(textBox13.Text, out leap)) || (leap < 1))
                {
                    MessageBox.Show("Leap 1?");
                    return;
                }
                DistanceFunction Base = GetDistance1;
                GetDistance1 = new DistanceFunction
                              (
                                  (p1, p2)
                                  =>
                                  { return Base(p1, p2) % leap; }
                    //{ return Math.Sin(((Base(p1, p2) % leap) / (leap - 1)) * Math.PI * 2) * leap; }
                              );
            }

            if (checkBox11.Checked)
            {
                double divisor;
                if ((!double.TryParse(textBox16.Text, out divisor)) || (divisor == 0))
                {
                    MessageBox.Show("Divisor 1?");
                    return;
                }
                DistanceFunction Base = GetDistance1;
                GetDistance1 = new DistanceFunction
                                (
                                    (p1, p2)
                                    =>
                                    { return (Math.Sin((float)Base(p1, p2) / divisor)); }
                                );
            }

            if (radioButton26.Checked)
            {
                DistanceFunction Base = GetDistance1;
                GetDistance1 = new DistanceFunction
                              (
                                  (p1, p2)
                                  =>
                                  { return -Base(p1, p2); }
                              );
            }

            if (radioButton32.Checked)
                GetDistance2 = Distance.Pythagorean;
            else
                if (radioButton31.Checked)
                    GetDistance2 = Distance.Manhattan;
                else
                    if (radioButton30.Checked)
                        GetDistance2 = Distance.Chebyshev;
                    else
                        if (radioButton36.Checked)
                        {
                            double norm;
                            if ((!double.TryParse(textBox21.Text, out norm)))// || (norm == 0))
                            {
                                MessageBox.Show("Norm 2?");
                                return;
                            }
                            GetDistance2 = Distance.CreateCustomDistance(norm);
                        }

            if (checkBox16.Checked)
            {
                int leap;
                if ((!int.TryParse(textBox18.Text, out leap)) || (leap < 1))
                {
                    MessageBox.Show("Leap 2?");
                    return;
                }
                DistanceFunction Base = GetDistance2;
                GetDistance2 = new DistanceFunction
                              (
                                  (p1, p2)
                                  =>
                                  { return (Base(p1, p2) % leap); }
                              );
            }

            if (checkBox15.Checked)
            {
                double divisor;
                if ((!double.TryParse(textBox17.Text, out divisor)) || (divisor == 0))
                {
                    MessageBox.Show("Divisor 2?");
                    return;
                }
                DistanceFunction Base = GetDistance2;
                GetDistance2 = new DistanceFunction
                                (
                                    (p1, p2)
                                    =>
                                    { return (Math.Sin((float)Base(p1, p2) / divisor)); }
                                );
            }

            if (radioButton26.Checked)
            {
                DistanceFunction Base = GetDistance2;
                GetDistance2 = new DistanceFunction
                              (
                                  (p1, p2)
                                  =>
                                  { return -Base(p1, p2); }
                              );
            }

            CrossOperation Op = null;

            if (f2.radioButton9.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return d1(p1, p2); }); });
            if (radioButton34.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return GetDistance2(p1, p2); }); });
            if (radioButton24.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return GetDistance1(p1, p2) + GetDistance2(p1, p2); }); });
            if (radioButton20.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return GetDistance1(p1, p2) - GetDistance2(p1, p2); }); });
            if (radioButton25.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return GetDistance1(p1, p2) * GetDistance2(p1, p2); }); });
            if (radioButton28.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return GetDistance1(p1, p2) / GetDistance2(p1, p2); }); });
            if (radioButton33.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return Math.Sin(GetDistance1(p1, p2)) * GetDistance2(p1, p2); }); });
            if (radioButton29.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return Math.Log(GetDistance1(p1, p2), GetDistance2(p1, p2)); }); });
            if (radioButton37.Checked)
                Op = new CrossOperation((d1, d2) => { return new DistanceFunction((p1, p2) => { return Math.Exp(GetDistance1(p1, p2)) * GetDistance2(p1, p2); }); });

            cga.distance_function = Op(GetDistance1, GetDistance2);


            if (checkBox18.Checked)
            {
                DistanceFunction Base = cga.distance_function;
                cga.distance_function = new DistanceFunction((p1, p2) => { return Base(p1, p2) * AngleDrive.GetForce(p1, p2); });
            }

            double angle;
            if (ValueGetter.TryDoubleFromTextBox(textBox8, out angle))
            {
                if (angle != 0)
                {
                    DistanceFunction Base = cga.distance_function;

                    cga.distance_function = new DistanceFunction((p1, p2) =>
                    {
                        System.Drawing.Drawing2D.Matrix x = new System.Drawing.Drawing2D.Matrix();
                        x.RotateAt((float)angle, new Point(cga.size.Width / 2, cga.size.Height / 2));
                        Point[] pa = new Point[] { p1, p2 };
                        x.TransformPoints(pa);
                        p1 = pa[0];
                        p2 = pa[1];
                        return Base(p1, p2);
                    });
                }
            }

            if (!int.TryParse(textBox1.Text, out cga.points))
            {
                MessageBox.Show("Points?");
                return;
            }

            cga.points_distribution_map = null;
            if (checkBox14.Checked)
            {
                if (Generator.GradientView != null)
                {
                    cga.points_distribution_map = new double[Generator.GradientView.Width, Generator.GradientView.Height];
                    for (x = 0; x < Generator.GradientView.Width; x++)
                        for (y = 0; y < Generator.GradientView.Height; y++)
                            cga.points_distribution_map[x, y] = Generator.GradientView.GetPixel(x, y).R / 255.0;
                }
            }

            if (radioButton1.Checked)
                cga.selection_mode = PointSelectionMode.Closest;

            if (radioButton2.Checked)
                cga.selection_mode = PointSelectionMode.Second;

            if (radioButton3.Checked)
                cga.selection_mode = PointSelectionMode.Mid_1_2;

            if (radioButton4.Checked)
                cga.selection_mode = PointSelectionMode.Third;

            cga.from = panel1.BackColor;
            cga.to = panel2.BackColor;
            cga.hue_from = panel3.BackColor;
            cga.hue_to = panel4.BackColor;
            cga.use_spectre = checkBox4.Checked;

            if (cga.to == cga.from)
            {
                MessageBox.Show("Colors? Using colors from set to inverted.");
                cga.from = Color.FromArgb(cga.to.ToArgb() ^ 0x00FFFFFF);
            }

            if (!checkBox3.Checked)
                cga.merge_mode = MergeMode.NoMerge;
            else
            {
                cga.prev_image = GetPrevBitmap();

                try
                {
                    cga.merge_amount = double.Parse(textBox5.Text) / 100;/*
                    if ((cga.merge_amount <= 0) || (cga.merge_amount > 1))
                        throw new Exception();*/
                }
                catch { MessageBox.Show("Mix %?"); return; }
                
                if (radioButton11.Checked)
                    cga.merge_mode = MergeMode.Mix;
                if (radioButton12.Checked)
                    cga.merge_mode = MergeMode.And;
                if (radioButton13.Checked)
                    cga.merge_mode = MergeMode.Or;
                if (radioButton14.Checked)
                    cga.merge_mode = MergeMode.Xor;
                if (radioButton15.Checked)
                    cga.merge_mode = MergeMode.Add;
                if (radioButton16.Checked)
                    cga.merge_mode = MergeMode.Sub;
                if (radioButton17.Checked)
                    cga.merge_mode = MergeMode.Mul;
                if (radioButton18.Checked)
                    cga.merge_mode = MergeMode.WTF;
                if (radioButton8.Checked)
                    cga.merge_mode = MergeMode.Min;
                if (radioButton23.Checked)
                    cga.merge_mode = MergeMode.Max;

                cga.merge_overflow = checkBox5.Checked;
            }

            cga.blur = checkBox2.Checked;
            cga.prewrap = checkBox8.Checked;
            if (radioButton21.Checked)
                cga.prewrap_mode = PreWrapMode.Side4;
            if (radioButton22.Checked)
                cga.prewrap_mode = PreWrapMode.Side8;

            if (checkBox19.Checked)
            {
                if (ValueGetter.TryDoubleFromTextBox(textBox9, out cga.midwrap_amount))
                {
                    cga.midwrap = true;
                    cga.midwrap_amount /= 100;
                }
            }
            else
                cga.midwrap = false;

            double limit;
            if (!double.TryParse(textBox12.Text, out limit))
            {
                MessageBox.Show("R-limit?"); return;
            }
            cga.points_distribution_limit = limit;

            if (limit < 0)
            {
                MessageBox.Show("R-limit?"); return;
            }

            /*
            if ((limit > b.Width / 2)
            || (limit > b.Height / 2))
            {
                MessageBox.Show("R-limit > Size/2!"); return;
            }
            */

            double facet_thickness;
            if (!double.TryParse(textBox15.Text, out facet_thickness))
            {
                MessageBox.Show("Facet?"); return;
            }
            cga.facet_thickness = facet_thickness;

            cga.flat_cells = checkBox13.Checked;

            cga.tlp = tableLayoutPanel2;

            if (!checkBox12.Checked)
                cga.known_points = Generator.pts;

            cga_ok = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
        	f2.Focus();
            if (bw != null)
                if (bw.IsBusy)
            {/*
                DialogResult dr = MessageBox.Show("Cancel the generation?", "Huh?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                if (dr == DialogResult.Yes)
                {
                    bw.CancelAsync();
                    return;
                }
                if (dr == DialogResult.No)*/
                MessageBox.Show("Processing...");
                    return;
            }

            PrepareCellGenArgs();

            textBox7.Enabled = false;
            tabControl3.Enabled = false;
            //tabControl2.Enabled = false;
            checkBox10.Enabled = false;

            foreach (Panel p in tableLayoutPanel2.Controls)
                p.BackColor = Color.Black;

            if (!cga_ok)
            {
                iterations = 0;
                timer2_Tick(null, null);
                return;
            }
            else
                if (!timer2.Enabled)
                    timer2.Start();

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(CellGeneratorWorker);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetGeneratedImage);
            bw.RunWorkerAsync(cga);

            label14.Text = "0";

            if (f2.tabControl1.SelectedIndex == 4)
                if (iterations == -1)
            try
            {
                iterations = int.Parse(f2.textBox9.Text) - 1;
                timer2.Start();
            }
            catch { MessageBox.Show("Amount?"); return; }
        }

        void CellGeneratorWorker(object sender, DoWorkEventArgs e)
        {
            b = CellsGenerator.Generator.Generate_Cells((CellGenArgs)e.Argument);
        }

        public static Bitmap b;

        public void GetGeneratedImage(object sender, EventArgs e)
        {
            if (f2.tabControl1.SelectedIndex == 0)
            {
                PB[picture_index].Image = b;//(Image)b.Clone();
                PB[picture_index].Refresh();

                if (!checkBox17.Checked)
                {
                    PB[picture_index].BorderStyle = BorderStyle.None;

                    picture_index += 1;
                    if (picture_index == PB.Length)
                        picture_index = 0;

                    PB[picture_index].BorderStyle = BorderStyle.Fixed3D;
                }
            }
            else
                if (f2.tabControl1.SelectedIndex == 1)
                {
                    PB2[picture_index2].Image = b;// (Image)b.Clone();
                    PB2[picture_index2].Refresh();

                    if (!checkBox17.Checked)
                    {
                        PB2[picture_index2].BorderStyle = BorderStyle.None;

                        picture_index2 += 1;
                        if (picture_index2 == PB2.Length)
                            picture_index2 = 0;

                        PB2[picture_index2].BorderStyle = BorderStyle.Fixed3D;
                    }
                }
            else
                    if (f2.tabControl1.SelectedIndex == 2)
                {
                    f2.pictureBox1.Image = b;// (Image)b.Clone();
                    f2.pictureBox1.Refresh();
                }
                else
                        if (f2.tabControl1.SelectedIndex == 3)
                    {
                        f2.pictureBox2.BackgroundImage = b;// (Image)b.Clone();
                        f2.pictureBox2.Refresh();
                    }
                    else
                            if (f2.tabControl1.SelectedIndex == 4)
                        {
                            prev_filename = f2.textBox8.Text + "\\" + DateTime.Now.ToString().Replace(':', '-') + " " + iterations.ToString().PadLeft(4, '0') + ".PNG";
                            b.Save(prev_filename);
                        }

            if (Generator.PointsView != null)
                pictureBox3.Image = Generator.PointsView;

            if (Generator.GradientView != null)
                pictureBox4.Image = Generator.GradientView;

            label14.Text = "Ready";

            if (!timer2.Enabled)
            {
                textBox7.Enabled = true;
                checkBox10.Enabled = true;
                tabControl3.Enabled = true;
                tabControl2.Enabled = true;
            }
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
                panel1.BackColor = colorDialog1.Color;
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
                panel2.BackColor = colorDialog1.Color;
        }

        int x, y;
        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap result = new Bitmap(1, 1);
            Bitmap original;

            original = result = GetPrevBitmap();

            int border;

            if (!int.TryParse(textBox6.Text, out border))
            {
                MessageBox.Show("Border?");
                return;
            }

            if ((border > result.Width) || (border > result.Height))
            {
                MessageBox.Show("Border > Width or Height.");
                return;
            }

            //Horizontal
            if (checkBox6.Checked)
            {
                for (x = 0; x < border; x++)
                    for (y = 0; y < result.Height; y++)
                        result.SetPixel(x, y, ColorOperation.MixColors(original.GetPixel(x, y),
                                                        original.GetPixel(original.Width - 1 - x, y),
                                                        (double)(border - x) / border));

                for (x = result.Width - border; x < result.Width; x++)
                    for (y = 0; y < result.Height; y++)
                        result.SetPixel(x, y, ColorOperation.MixColors(original.GetPixel(x, y),
                                                        original.GetPixel(original.Width - 1 - x, y),
                                                        (double)(x - (result.Width - border)) / border));
            }

            //Vertical
            if (checkBox7.Checked)
            {
                for (x = 0; x < result.Width; x++)
                    for (y = 0; y < border; y++)
                        result.SetPixel(x, y, ColorOperation.MixColors(original.GetPixel(x, y),
                                                        original.GetPixel(x, original.Height - 1 - y),
                                                        (double)(border - y) / border));

                for (x = 0; x < result.Width; x++)
                    for (y = result.Height - border; y < result.Height; y++)
                        result.SetPixel(x, y, ColorOperation.MixColors(original.GetPixel(x, y),
                                                        original.GetPixel(x, original.Height - 1 - y),
                                                        (double)(y - (result.Height - border)) / border));
            }

            b = result;
            GetGeneratedImage(null, null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Bitmap b = GetPrevBitmap();
            textBox6.Text = (b.Width / 4).ToString();
        }

        int iterations = -1;
        private void button6_Click(object sender, EventArgs e)
        {
        	f2.Focus();
        	
            PB[picture_index].BorderStyle = BorderStyle.None;
            PB2[picture_index2].BorderStyle = BorderStyle.None;
            button6.Enabled = false;
            button1.Enabled = false;
            f2.button4.Enabled = false;
            tabControl3.Enabled = false;

            iterations = 16;
            if (!checkBox17.Checked)
            {
                picture_index = 0;
                picture_index2 = 0;
            }

            button1_Click(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap b = GetPrevBitmap();
            textBox6.Text = b.Width.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
        	f2.Focus();
            for (int i = 0; i < 16; i++)
                PB[i].Image = new Bitmap(128, 128);
            for (int i = 0; i < 4; i++)
                PB2[i].Image = new Bitmap(256, 256);
            f2.pictureBox1.Image = new Bitmap(512, 512);
            f2.pictureBox2.BackgroundImage = new Bitmap(256, 256);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label10.Text = CellsGenerator.Generator.time_1;
            label11.Text = CellsGenerator.Generator.time_2;
            label13.Text = CellsGenerator.Generator.time_3;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label28.Text = iterations.ToString();
            if (bw != null)
            if (bw.IsBusy)
                return;

            if (iterations > 0)
            {
                iterations--;
                button1_Click(this, new EventArgs());
            }
            else
            {
                prev_filename = "";
                iterations = -1;
                timer2.Stop();
                button6.Enabled = true;
                button1.Enabled = true;
                f2.button4.Enabled = true;
                checkBox10.Enabled = true;
                tabControl3.Enabled = true;
                tabControl2.Enabled = true;
                textBox7.Enabled = true;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(GetPrevBitmap().Width, GetPrevBitmap().Height);

            int i, j;

            Random r = new Random();
            Pen p = new Pen(panel1.BackColor);
            p.Width = 2;
            Graphics gr = Graphics.FromImage(bmp);
            int lines;

            gr.FillRectangle(new Pen(panel2.BackColor).Brush, 0, 0, b.Width, b.Height);

            if (int.TryParse(textBox3.Text, out lines))
                if (lines > 1)
                {
                Point[] pts = new Point[lines];

                for (i = 0; i < lines; i++)
                    pts[i] = new Point(r.Next(b.Width), r.Next(b.Height));

                for (j = 0; j < 5; j++)
                {
                gr.DrawCurve(p, pts);
                for (i = 0; i < lines; i++)
                    pts[i] = pts[r.Next(lines)];
                p.Width -= 1;
                }
                    /*
                gr.DrawEllipse(Pens.Red, r.Next(256), r.Next(256), r.Next(64), r.Next(64));
                gr.DrawEllipse(Pens.Red, r.Next(256), r.Next(256), r.Next(64), r.Next(64));
                gr.DrawEllipse(Pens.Red, r.Next(256), r.Next(256), r.Next(64), r.Next(64));
                gr.DrawEllipse(Pens.Red, r.Next(256), r.Next(256), r.Next(64), r.Next(64));*/
                }

            b = bmp;
            Blur(b, 1, 1);

            GetGeneratedImage(this, new EventArgs());
        }

        void Blur(Bitmap b, int amount, int radius)
        {
            int i, j;
            Color temp;

            int k, line;
            for (k = 0; k < amount; k++)
            {
                for (i = radius; i < b.Width - radius; i++)
                    for (j = radius; j < b.Height - radius; j++)
                    {
                        temp = b.GetPixel(i, j);
                        for (line = -radius; line < radius; line++)
                            temp = ColorOperation.MixColors(temp, b.GetPixel(i + line, j), 0.1);
                        for (line = -radius; line < radius; line++)
                            temp = ColorOperation.MixColors(temp, b.GetPixel(i, j + line), 0.1);

                        b.SetPixel(i, j, temp);
                    }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Bitmap src = GetPrevBitmap();
            Bitmap result;
            Bitmap mask;

            f2.tabControl1.SelectedIndex = 0;

            int x, y;
            for (picture_index = 0; picture_index < 16; picture_index++)
            {
                result = src;

                mask = new Bitmap(result.Width, result.Height);

                int kx = 0, ky = 0;

                kx += (picture_index == 2) ? 1 : (picture_index == 6) ? 1 : (picture_index == 10) ? 1 : 0;
                kx -= (picture_index == 0) ? 1 : (picture_index == 4) ? 1 : (picture_index == 8) ? 1 : 0;
                ky += (picture_index == 8) ? 1 : (picture_index == 9) ? 1 : (picture_index == 10) ? 1 : 0;
                ky -= (picture_index == 0) ? 1 : (picture_index == 1) ? 1 : (picture_index == 2) ? 1 : 0;

                for (x = 0; x < mask.Width; x++)
                    for (y = 0; y < mask.Height; y++)
                    {
                        int n = x * kx + y * ky;
                        if (n < 0) n = 0;
                        if (n > 255) n = 255;
                        mask.SetPixel(x, y, Color.FromArgb(n, n, n));
                    }

                for (x = 0; x < mask.Width; x++)
                    for (y = 0; y < mask.Height; y++)
                        result.SetPixel(x, y, ColorOperation.MulColors(mask.GetPixel(x, y), result.GetPixel(x, y)));
                
                PB[picture_index].Image = result;
            }

            picture_index = 0;
        }

        static public Bitmap GetPrevBitmap()
        {
            int prev_picture_index = picture_index - 1;
            if (prev_picture_index == -1)
                prev_picture_index = PB.Length - 1;

            int prev_picture_index2 = picture_index2 - 1;
            if (prev_picture_index2 == -1)
                prev_picture_index2 = PB2.Length - 1;

            Bitmap prev_image = null;

            if (f2.tabControl1.SelectedIndex == 0)
                prev_image = ((Bitmap)PB[prev_picture_index].Image);
            else
                if (f2.tabControl1.SelectedIndex == 1)
                    prev_image = ((Bitmap)PB2[prev_picture_index2].Image);
                else
                    if (f2.tabControl1.SelectedIndex == 2)
                        prev_image = ((Bitmap)f2.pictureBox1.Image);
                    else
                        if (f2.tabControl1.SelectedIndex == 3)
                            prev_image = ((Bitmap)f2.pictureBox2.BackgroundImage);
                        else
                            if (f2.tabControl1.SelectedIndex == 4)
                            {
                                if (File.Exists(prev_filename))
                                    prev_image = (Bitmap)Bitmap.FromFile(prev_filename);
                                else
                                    prev_image = null;
                                    //throw new Exception("Previous filename points to non-existing or N/A file.");
                            }

            return prev_image;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Bitmap prev_image = GetPrevBitmap();

            int c = 0;
            int rndy=0;

            y = 0;
            for (x = 0; x < b.Width; x++)
            {
                if (x % 4 == 0)
                {
                    rndy = r.Next(b.Height);
                    c = b.GetPixel(x, rndy).ToArgb();
                }
                if (r.Next(100) < 3)
                    for (y = rndy; y < b.Height; y++)
                    {
                        b.SetPixel(x, y, Color.FromArgb(c ^ r.Next(int.MaxValue >> 8)));

                        if (r.Next(10) < 3)
                            b.SetPixel(x, y, Color.FromArgb(c | r.Next(int.MaxValue >> 8)));
                        if (r.Next(10) < 3)
                            b.SetPixel(x, y, Color.FromArgb(c & r.Next(int.MaxValue >> 8)));
                    }
            }

            GetGeneratedImage(this, null);
        }

        private void panel3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
                panel3.BackColor = colorDialog1.Color;
        }

        private void panel4_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
                panel4.BackColor = colorDialog1.Color;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Bitmap b = GetPrevBitmap();
            textBox6.Text = (b.Width / 2).ToString();
        }

        private void button15_Click(object sender, EventArgs e)
        {
        	f2.Focus();
            iterations = 0;
            timer2_Tick(this, null);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            int neww, newh;

            if (!int.TryParse(textBox10.Text, out neww))
            {
                MessageBox.Show("X?"); return;
            }

            if (!int.TryParse(textBox11.Text, out newh))
            {
                MessageBox.Show("Y?"); return;
            }

            if (f2.tabControl1.SelectedIndex == 2)
                f2.pictureBox1.Image = new Bitmap(neww, newh);
            if (f2.tabControl1.SelectedIndex == 3)
                f2.pictureBox2.BackgroundImage = new Bitmap(neww, newh);
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            textBox13.Enabled = checkBox9.Checked;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            textBox16.Enabled = checkBox11.Checked;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            int rndx1 = r.Next(b.Width);
            int rndy1 = r.Next(b.Height);
            int rndx2 = rndx1 + r.Next(b.Width - rndx1);
            int rndy2 = rndy1 + r.Next(b.Height - rndy1);
            int offsetx = r.Next(b.Width);
            int offsety = r.Next(b.Height);
            int color;

            for (x = rndx1; x < rndx2; x++)
            {
                for (y = rndy1; y < rndy2; y++)
                {
                    color = b.GetPixel((x + offsetx) % b.Width, (y + offsety) % b.Height).ToArgb();
                    b.SetPixel(x, y, Color.FromArgb(color));
                }
            }

            GetGeneratedImage(this, null);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            int k;

            if (int.TryParse(textBox7.Text, out k))
            {
                tableLayoutPanel2.Controls.Clear();

                tableLayoutPanel2.ColumnCount = k;
                tableLayoutPanel2.RowCount = k;

                tableLayoutPanel2.Size = new Size(k * 13, k * 13);

                for (int i = 0; i < k; i++)
                    for (int j = 0; j < k; j++)
                    {
                        Panel p = new Panel();
                        p.BackColor = Color.Black;
                        p.Width = 4;
                        p.Height = 4;
                        tableLayoutPanel2.Controls.Add(p);
                    }

                for (int i = 0; i < tableLayoutPanel2.ColumnStyles.Count; i++)
                {
                    tableLayoutPanel2.ColumnStyles[i].SizeType = SizeType.AutoSize;
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Bitmap bmp = GetPrevBitmap();

            Blur(bmp, 1, int.Parse(textBox14.Text));

            GetGeneratedImage(null, null);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Color tmp;

            tmp = panel1.BackColor;
            panel1.BackColor = panel2.BackColor;
            panel2.BackColor = tmp;

            tmp = panel3.BackColor;
            panel3.BackColor = panel4.BackColor;
            panel4.BackColor = tmp;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("All current settings and bitmaps will be lost. OK?", "Total restart", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //???
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            panel1.BackColor = Color.Black;
            panel2.BackColor = Color.White;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            panel3.BackColor = Color.FromArgb(255, 250, 250);
            panel4.BackColor = Color.FromArgb(255, 250, 255);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(GetPrevBitmap().Width, GetPrevBitmap().Height);

            Random r = new Random();
            Pen p = new Pen(panel1.BackColor);
            p.Width = 1;
            Graphics gr = Graphics.FromImage(bmp);
            p.Color = Color.FromArgb(64, p.Color);

            int iterations;

            if (!ValueGetter.TryIntFromTextBox(textBox22, out iterations))
                return;

            int x, y, dx, dy, cx, cy, step, step_random;

            double tension;

            if (!ValueGetter.TryDoubleFromTextBox(textBox26, out tension))
                return;

            cx = bmp.Width / 2;
            cy = bmp.Height / 2;

            x = cx;
            y = cy;

            if (!ValueGetter.TryIntFromTextBox(textBox24, out step))
                return;

            if (!ValueGetter.TryIntFromTextBox(textBox25, out step_random))
                return;

            //step = r.Next(30) + 2;
            do
            {
                dx = r.Next(70) + 10;
                dy = r.Next(70) + 10;
            }
            while (dx == dy);

            gr.FillRectangle(new Pen(panel2.BackColor).Brush, 0, 0, bmp.Width, bmp.Height);

            for (int i = 0; i < iterations; i++)
            {
                gr.DrawLine(p, cx, cy, x, y);
                //gr.DrawCurve(p, new Point[] { new Point(cx, cy), new Point(x, y), new Point(x + dx, y - dy), new Point(x - dx, y + dy) }, (float)tension);
                gr.DrawRectangle(p, x-4, y-4, x+4, y+4);

                if ((x > bmp.Width) || (x < 0))
                    dx = -dx/2;
                if ((y > bmp.Height) || (y < 0))
                    dy = -dy/2;

                x += dx;
                y += dy;
                /*
                if (i % 10 == 0)
                {
                    dx = r.Next(10)-5;
                    dy = r.Next(10)-5;
                }*/
            }

            b = bmp;
            GetGeneratedImage(null, null);
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            b = (Bitmap)GetPrevBitmap().Clone();
            Bitmap c = (Bitmap)b.Clone();

            Graphics g = Graphics.FromImage(b);

            g.FillRectangle(Brushes.Black, 0, 0, b.Width, b.Height);
            g.TranslateTransform(b.Width / 2, b.Height / 2);
            //g.RotateTransform(45);
            Point[] p = new Point[3];
            p[0] = new Point(-b.Width / 2, 0);
            p[1] = new Point(0, -b.Height / 2);
            p[2] = new Point(0, b.Height / 2);
            g.DrawImage(c, p);

            GetGeneratedImage(null, null);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            PrepareCellGenArgs();

            DateTime d = DateTime.Now;
            for (int i = 0; i < cga.size.Width * cga.size.Height; i++)
                cga.distance_function(new Point(r.Next(cga.size.Width), r.Next(cga.size.Height)),
                                      new Point(r.Next(cga.size.Width), r.Next(cga.size.Height)));

            TimeSpan t = DateTime.Now.Subtract(d);

            label37.Text = t.ToString();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            Bitmap bmp = GetPrevBitmap();
            Color pixel;
            double amount;

            if (!ValueGetter.TryDoubleFromTextBox(textBox23, out amount))
                return;

            if (amount == 0)
                return;

            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    pixel = bmp.GetPixel(i, j);
                    pixel = Color.FromArgb(pixel.A,
                                           (int)((int)(pixel.R / amount) * amount),
                                           (int)((int)(pixel.G / amount) * amount),
                                           (int)((int)(pixel.B / amount) * amount));
                    bmp.SetPixel(i, j, pixel);
                }

            b = bmp;
            GetGeneratedImage(null, null);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            Bitmap bmp = GetPrevBitmap();

            Color max = Color.FromArgb(255, 0, 0, 0);
            Color pixel;

            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    pixel = bmp.GetPixel(i, j);

                    if (pixel.R > max.R)
                        max = Color.FromArgb(pixel.R, max.G, max.B);
                    if (pixel.G > max.G)
                        max = Color.FromArgb(max.R, pixel.G, max.B);
                    if (pixel.B > max.B)
                        max = Color.FromArgb(max.R, max.G, pixel.B);
                }

            double rCoeff;
            double gCoeff;
            double bCoeff;

            if (max.R == 0)
                rCoeff = 1;
            else
                rCoeff = 255.0 / max.R;

            if (max.G == 0)
                gCoeff = 1;
            else
                gCoeff = 255.0 / max.G;

            if (max.B == 0)
                bCoeff = 1;
            else
                bCoeff = 255.0 / max.B;

            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    pixel = bmp.GetPixel(i, j);
                    pixel = Color.FromArgb(pixel.A, (int)(pixel.R * rCoeff),
                                                    (int)(pixel.G * gCoeff),
                                                    (int)(pixel.B * bCoeff));
                    bmp.SetPixel(i, j, pixel);
                }

            b = bmp;
            GetGeneratedImage(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            b = (Bitmap)GetPrevBitmap().Clone();

            Point point = new Point(b.Width / 2, b.Height / 2);

            int i, j;
            double[,] values = new double[b.Width, b.Height];

            Point midpoint;

            for (i = 0; i < b.Width; i++)
                for (j = 0; j < b.Height; j++)
                {
                    Point p = new Point(i, j);

                    midpoint = new Point(Math.Min(point.X, i) + (Math.Max(point.X, i) - Math.Min(point.X, i)) / 5*3,
                                         Math.Min(point.Y, j) + (Math.Max(point.Y, j) - Math.Min(point.Y, j)) / 5*3);

                    values[i, j] = Distance.Pythagorean(point, p) * b.GetPixel(i, j).R;
                    //Distance.Pythagorean(point, midpoint) * 
                    //values[i, j] = b.GetPixel(midpoint.X, midpoint.Y).R;
                    values[i, j] += b.GetPixel(i, j).R;
                }

            Operation.Normalize(ref values, 1.0);

            Color c;
            for (i = 0; i < b.Width; i++)
                for (j = 0; j < b.Height; j++)
                {
                    c = b.GetPixel(i, j);
                    b.SetPixel(i, j, Color.FromArgb((int)(values[i, j] * c.R),
                                                    (int)(values[i, j] * c.G),
                                                    (int)(values[i, j] * c.B)));
                }

            GetGeneratedImage(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int range = ValueGetter.IntFromTextBox(textBox27);

            for (int i = 0; i < Generator.pts.Length; i++)
                Generator.pts[i] = new Point(Generator.pts[i].X + r.Next(range) - range / 2, Generator.pts[i].Y + r.Next(range) - range / 2);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int BaseLength = Generator.pts.Length;

            int range = ValueGetter.IntFromTextBox(textBox27);

            for (int i = 0; i < BaseLength; i++)
            {
                //Generator.pts[i] = new Point(Generator.pts[i].X + r.Next(range) - range / 2, Generator.pts[i].Y + r.Next(range) - range / 2);
                Array.Resize<Point>(ref Generator.pts, Generator.pts.Length + 1);
                Generator.pts[Generator.pts.Length - 1] = new Point(Generator.pts[i].X + range, Generator.pts[i].Y);
                Array.Resize<Point>(ref Generator.pts, Generator.pts.Length + 1);
                Generator.pts[Generator.pts.Length - 1] = new Point(Generator.pts[i].X, Generator.pts[i].Y + range);
            }
        }
    }

    public static class ValueGetter
    {
        public static double DoubleFromTextBox(TextBox tb)
        {
            double value;

            if ((!double.TryParse(tb.Text, out value)))
            {
                MessageBox.Show("Couldn't parse " + tb.Name);
                return double.NaN;
            }

            return value;
        }

        public static bool TryDoubleFromTextBox(TextBox tb, out double value)
        {
            if ((!double.TryParse(tb.Text, out value)))
                return false;

            return true;
        }

        public static int IntFromTextBox(TextBox tb)
        {
            int value;

            if ((!int.TryParse(tb.Text, out value)))
            {
                MessageBox.Show("Couldn't parse " + tb.Name);
                return int.MinValue;
            }

            return value;
        }

        public static bool TryIntFromTextBox(TextBox tb, out int value)
        {
            if ((!int.TryParse(tb.Text, out value)))
                return false;

            return true;
        }
    }
}