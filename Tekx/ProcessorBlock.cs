using System;
using System.Collections.Generic;
using System.Collections;
//using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Tekx
{
    class ProcessorBlockChain
    {
        struct ProcessorBlockNode
        {
            public ProcessorBlock ProcessorBlock;
            public int InputIndex;
        }

        public ArrayList Chain = new ArrayList();

        ///<summary>Create an empty chain.</summary>
        public ProcessorBlockChain() { }
        
        ///<summary>Form a simple 1-way chain.</summary>
        public ProcessorBlockChain(ProcessorBlock[] ProcessorBlocksArray)
        {
            foreach (ProcessorBlock pb in ProcessorBlocksArray)
                AddToChain(pb);
        }

        ///<summary>Add block to chain, with input from the last block index.</summary>
        static public ProcessorBlockChain operator +(ProcessorBlockChain PBC, ProcessorBlock PB)
        {
            PBC.AddToChain(PB);
            return PBC;
        }

        ///<summary>Add block to chain, with input from the last block index.</summary>
        public void AddToChain(ProcessorBlock ProcessorBlock)
        {
            ProcessorBlockNode pbn = new ProcessorBlockNode();
            pbn.ProcessorBlock = ProcessorBlock;
            pbn.InputIndex = Chain.Count - 1;
            Chain.Add(pbn);
        }

        ///<summary>Add block to chain, with input from the specified block index.</summary>
        public void AddToChain(ProcessorBlock ProcessorBlock, int InputIndex)
        {
            ProcessorBlockNode pbn = new ProcessorBlockNode();
            pbn.ProcessorBlock = ProcessorBlock;
            if ((InputIndex < 0) || (InputIndex > this.Chain.Count))
                throw new Exception("Input Index out of range.");
            pbn.InputIndex = InputIndex;
            Chain.Add(pbn);
        }

        public void Process()
        {
            ProcessorBlockNode prev_pbn;

            //First iteration - blocks with no inputs.
            foreach (ProcessorBlockNode pbn in Chain)
            {
                if (pbn.InputIndex == -1)
                {
                    pbn.ProcessorBlock.Work();
                }
            }

            //Then - with inputs.
            foreach (ProcessorBlockNode pbn in Chain)
            {
                if (pbn.InputIndex != -1)
                {
                    prev_pbn = ((ProcessorBlockNode)Chain[pbn.InputIndex]);
                    prev_pbn.ProcessorBlock.Work();
                    pbn.ProcessorBlock.In = prev_pbn.ProcessorBlock.Out;
                    pbn.ProcessorBlock.Work();
                }
            }
        }
    }

    class ProcessorBlock
    {
        private Type[] InTypes;
        private Type[] OutTypes;

        public object[] In;
        public object[] Out;

        delegate object[] Op(object[] Arguments);

        Op Operation = new Op((A) => { return null; });

        public void Work()
        {
            ParameterInfo[] pi = Operation.Method.GetParameters();

            if (InTypes.Length > pi.Length)
                throw new Exception("Too many parameters in.");
            else
                if (InTypes.Length < pi.Length)
                    throw new Exception("Not enough parameters in.");

            for (int i = 0; i < pi.Length; i++)
                if (InTypes[i] != pi[i].ParameterType)
                    throw new Exception("Input parameter types are incompatible with processor's operation. Parameter #" + i.ToString() + " must be: " + InTypes[i].ToString() + ", is: " + pi[i].ParameterType);

            try
            {
                Out = Operation(In);
            }
            catch { throw new Exception("Operation failed."); }

            try
            {
                for (int i = 0; i < pi.Length; i++)
                    if (OutTypes[i] != Out[i].GetType())
                        throw new Exception("Operation resulted in type controversy. Out value #" + i.ToString() + " is: " + OutTypes[i].ToString() + ", must be: " + Out[i].GetType());
            }
            catch { }
        }

        /*
        //NOT USED AS OF V0.8
        
        static string[] tNames = { "scalar",
                                   "vector",
                                   "matrix",
                                   "2dpoint",
                                   "bitmap" };

        static Type[] Types = { typeof(double),
                                typeof(double[]),
                                typeof(double[,]),
                                typeof(System.Drawing.Point),
                                typeof(System.Drawing.Bitmap) };
        
        static NamedParameters TypeLookup = new NamedParameters(tNames, Types);
        
        static public ProcessorBlock FromFile(string filename)
        {
            //Format is:
            //itype0
            //itype1
            //...
            //v
            //otype0
            //otype1
            //EOF
            
            //Possible types are:
            //{ "scalar",
            //  "vector",
            //  "matrix",
            //  "2dpoint",
            //  "bitmap" };
            
            string[] lines = File.ReadAllLines(filename);

            bool Out = false;

            ArrayList iTypeNames = new ArrayList();
            ArrayList oTypeNames = new ArrayList();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].ToLower() == "v")
                {
                    Out = true;
                    continue;
                }

                if (Out)
                    oTypeNames.Add(lines[i]);
                else
                    iTypeNames.Add(lines[i]);
            }

            Type[] iTypes=new Type[iTypeNames.Count];
            Type[] oTypes=new Type[oTypeNames.Count];

            for (int i = 0; i < iTypeNames.Count; i++)
                iTypes[i] = (Type)TypeLookup[(string)iTypeNames[i]];

            for (int i = 0; i < oTypeNames.Count; i++)
                oTypes[i] = (Type)TypeLookup[(string)oTypeNames[i]];

            return new ProcessorBlock(iTypes, oTypes);
        }
        */

        public ProcessorBlock(Type[] inTypes, Type[] outTypes)
        {
            this.InTypes = inTypes;
            this.OutTypes = outTypes;
        }
    }
}