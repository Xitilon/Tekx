using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tekx
{
    class Oscillator
    {
        /*
        public static OscillatorFunctionDelegate operator *(OscillatorFunctionDelegate ofd1, OscillatorFunctionDelegate ofd2)
        {
            return new OscillatorFunctionDelegate((p) => { return ofd1(p) * ofd2(p); });
        }
         */

        public class OscillatorFunctions
        {
            
            static public void PreparePhase(ref double phase)
            {
                if (phase > 0)
                    phase = phase - Math.Truncate(phase);
                else
                    phase = 1 + (phase - Math.Truncate(phase));
            }

            //0-_1-_2-
            static public double Square(double phase)
            {
                PreparePhase(ref phase);

                if (phase < 0.5)
                    return 1;
                else
                    return 0;
            }

            //0/\1/\2/
            static public double Triangle(double phase)
            {
                PreparePhase(ref phase);

                if (phase < 0.5)
                    return phase * 2;
                else
                    return 2 - (phase * 2);
            }

            //0/|1/|2/
            static public double Sawtooth(double phase)
            {
                PreparePhase(ref phase);
                
                return phase;
            }

            //0^v1^v2^
            static public double Sine(double phase)
            {
                return (1 + Math.Sin(phase * 2 * Math.PI)) / 2;
            }

            //0v^1v^2v
            static public double Cosine(double phase)
            {
                return (1 + Math.Cos(phase * 2 * Math.PI)) / 2;
            }
        }

        public delegate double OscillatorFunctionDelegate(double phase);

        OscillatorFunctionDelegate OscillatorFunction;

        public double this[double phase]
        {
            get { return this.OscillatorFunction(phase); }
        }

        public Oscillator()
        {
            OscillatorFunction = (p) => { return 0.5; };
        }

        public Oscillator(OscillatorFunctionDelegate OscillatorFunction)
        {
            this.OscillatorFunction = OscillatorFunction;
        }
    }
}