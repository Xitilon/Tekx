using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tekx
{
    class NamedParameters
    {
        static public NamedParameters FromFile(string filename) //Format is: name=values (newline)
        {
            string[] lines = File.ReadAllLines(filename);

            string[] names = new string[lines.Length];
            object[] values = new object[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('=');
                names[i] = parts[0];
                values[i] = parts[1];
            }

            return new NamedParameters(names, values);
        }

        string[] Parameters_Names;
        object[] Parameters_Values;

        public NamedParameters(string[] ParametersNames)
        {
            this.Parameters_Names = ParametersNames;
        }

        public NamedParameters(string[] ParametersNames, object[] ParametersValues)
        {
            if (ParametersValues.Length != ParametersNames.Length)
                throw new Exception("Names and values have different lengths.");

            this.Parameters_Names = ParametersNames;
            this.Parameters_Values = ParametersValues;
        }
        
        public object this[string name]
        {
            get { return Parameters_Values[GetIndex(name)]; }
            set { Parameters_Values[GetIndex(name)] = value; }
        }

        private int GetIndex(string s)
        {
            for (int i = 0; i < Parameters_Names.Length; i++)
                if (Parameters_Names[i] == s)
                    return i;

            throw new NotImplementedException("Requested name is not found in parameter names.");
        }
    }
}