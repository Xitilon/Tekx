using System;
using System.Drawing;

namespace ColorOperations
{
    class ColorOperation
    {
        static public Color MixColors(Color c1, Color c2, double coeff)
        {
            double anti = 1.0 - coeff;
            return Color.FromArgb((int)(c1.R * anti + c2.R * coeff),
                                  (int)(c1.G * anti + c2.G * coeff),
                                  (int)(c1.B * anti + c2.B * coeff));
        }

        static public Color MulColors(Color c1, Color c2)
        {
            return Color.FromArgb((int)(c1.R * c2.R),
                                  (int)(c1.G * c2.G),
                                  (int)(c1.B * c2.B));
        }

        static public Color ColorFromHSV(double h, double s, double v)
        {
            double r = 0, g = 0, b = 0;

            int hi = (int)Math.Floor(h / 60);
            double vmin = (1.0 - s) * v;
            double a = (v - vmin) * ((h % 60) / 60);
            double vinc = vmin + a;
            double vdec = v - a;

            if (hi == 0)
            {
                r = v;
                g = vinc;
                b = vmin;
            }
            else
                if (hi == 1)
                {
                    r = vdec;
                    g = v;
                    b = vmin;
                }
                else
                    if (hi == 2)
                    {
                        r = vmin;
                        g = v;
                        b = vinc;
                    }
                    else
                        if (hi == 3)
                        {
                            r = vmin;
                            g = vdec;
                            b = v;
                        }
                        else
                            if (hi == 4)
                            {
                                r = vinc;
                                g = vmin;
                                b = v;
                            }
                            else
                            {
                                r = v;
                                g = vmin;
                                b = vdec;
                            }

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        static public Color HueColorize(Color c1, Color c2, double value)
        {
            double anti = 1.0 - value;

            double hue = c1.GetHue() * value + c2.GetHue() * anti;
            double sat = (c1.GetSaturation() + c2.GetSaturation()) / 2;
            double val = c1.GetBrightness() * value + c2.GetBrightness() * anti;

            return ColorFromHSV(hue, sat, val);
        }
    }
}