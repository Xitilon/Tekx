using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tekx
{
    public partial class Form2 : Form
    {
        public Form1 f1;

        public Form2()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap what = new Bitmap(1, 1);

            what = Form1.GetPrevBitmap();

            if (what == null)
                MessageBox.Show("Empty!");
            else
                Clipboard.SetImage(what);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.b = (Bitmap)Clipboard.GetImage();

            f1.GetGeneratedImage(null, null);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            f1.folderBrowserDialog1.SelectedPath = Application.StartupPath;
            if (f1.folderBrowserDialog1.ShowDialog() != DialogResult.Cancel)
                textBox8.Text = f1.folderBrowserDialog1.SelectedPath;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            textBox8.Text = Application.StartupPath;
        }
    }
}
