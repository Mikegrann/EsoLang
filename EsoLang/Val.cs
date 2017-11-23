using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EsoLang
{
    [Serializable]
    public abstract class Val : ICloneable
    {
        public abstract object Clone();
    }

    [Serializable]
    public class NumV : Val
    {
        public long num;

        public NumV(long num)
        {
            this.num = num;
        }

        public override object Clone()
        {
            return new NumV(num);
        }

        public override string ToString()
        {
            return num.ToString();
        }
    }

    [Serializable]
    public class StrV : Val
    {
        public string str;

        public StrV(string str)
        {
            this.str = str;
        }

        public override object Clone()
        {
            return new StrV(str);
        }

        public override string ToString()
        {
            return str;
        }
    }

    [Serializable]
    public class CloV : Val
    {
        public Environment env;
        public ExprC body;
        public List<string> parms;
        
        public CloV(Environment env, ExprC body, List<string> parms)
        {
            this.env = env;
            this.body = body;
            this.parms = parms;
        }

        public override object Clone()
        {
            // shallow copy env
            return new CloV(env, body, parms);
        }

        public CloV DeepCopy()
        {
            CloV result = new CloV((Environment)env.Clone(), body, parms);

            return result;
        }

        public override string ToString()
        {
            return body.ToString() + " - " + env.ToString();
        }
    }

    /*
    // Store as 1D-array instead of N-dimensional nesting
    [Serializable]
    public class ArrV : Val
    {
        public Val[] arr;
        public List<long> indDimens;

        public ArrV(ArrV other)
        {
            arr = (Val[])other.arr.Clone();
            indDimens = other.indDimens;
        }

        public ArrV(List<long> indices, Val init)
        {
            long entries = 1;
            foreach (long dim in indices)
            {
                entries *= dim;
            }

            arr = new Val[entries];
            for (int i = 0; i < entries; i++)
            {
                arr[i] = (Val)init.Clone();
            }
            this.indDimens = indices;
        }

        public Val GetValue(List<long> indices)
        {
            return arr[GetOneDimIndex(indices)];
        }

        public void SetValue(List<long> indices, Val newVal)
        {
            arr[GetOneDimIndex(indices)] = newVal;
        }

        private long GetOneDimIndex(List<long> indices)
        {
            if (indices.Count != this.indDimens.Count)
            {
                throw new InterpException("Indexing wrong number of dimensions");
            }
            else
            {
                for (int i = 0; i < indDimens.Count; i++)
                {
                    if (indices[i] < 0 || indices[i] >= indDimens[i])
                    {
                        throw new InterpException("Index out of bounds");
                    }
                }
            }

            long idx = 0, cumdim = 1;
            for (int dim = indices.Count - 1; dim >= 0; dim--)
            {
                idx += cumdim * indices[dim];
                cumdim *= indDimens[dim];
            }

            return idx;
        }

        public override object Clone()
        {
            return new ArrV(this);
        }

        public override string ToString()
        {
            string result = "{";

            for (long i = 0; i < arr.LongLength; i++)
            {
                result += arr[i].ToString() + ", ";
            }

            return result.Substring(0, result.Length - 2) + "}";
        }
    }*/

    [Serializable]
    public class ArrV : Val
    {
        public Val[] arr;
        public long dim;

        public ArrV(ArrV other)
        {
            arr = (Val[])other.arr.Clone();
            dim = other.dim;
        }

        public ArrV(List<long> dims, Val init)
        {
            dim = dims[0];
            arr = new Val[dim];

            List<long> dimsCopy = new List<long>();
            for (int i = 1; i < dims.Count; i++) // Skip first dim on copy
            {
                dimsCopy.Add(dims[i]);
            }

            for (long i = 0; i < dim; i++)
            {
                if (dimsCopy.Count > 0) // Still more subdimensions to fill
                {
                    arr[i] = new ArrV(dimsCopy, init);
                }
                else
                {
                    arr[i] = (Val)init.Clone();
                }
            }
        }

        public Val GetValue(List<long> indices)
        {
            long index = indices[0];

            if (index >= dim) {
                throw new InterpException("Index out of bounds");
            }

            List<long> indicesCopy = new List<long>();
            for (int i = 1; i < indices.Count; i++) // Skip first index on copy
            {
                indicesCopy.Add(indices[i]);
            }
            
            if (indicesCopy.Count > 0)
            {
                if (arr[index] is ArrV)
                {
                    return ((ArrV)arr[index]).GetValue(indicesCopy);
                }
                else
                {
                    throw new InterpException("Indexing non-array");
                }
            }
            else
            {
                return arr[index];
            }
        }

        public void SetValue(List<long> indices, Val newVal)
        {
            long index = indices[0];

            if (index >= dim)
            {
                throw new InterpException("Index out of bounds");
            }

            List<long> indicesCopy = new List<long>();
            for (int i = 1; i < indices.Count; i++) // Skip first index on copy
            {
                indicesCopy.Add(indices[i]);
            }

            if (indicesCopy.Count > 0)
            {
                if (arr[index] is ArrV)
                {
                    ((ArrV)arr[index]).SetValue(indicesCopy, newVal);
                }
                else
                {
                    throw new InterpException("Indexing non-array");
                }
            }
            else
            {
                arr[index] = newVal;
            }
        }

        public override object Clone()
        {
            return new ArrV(this);
        }

        public override string ToString()
        {
            string result = "{";

            for (long i = 0; i < arr.LongLength; i++)
            {
                result += arr[i].ToString() + ", ";
            }

            return result.Substring(0, result.Length - 2) + "}";
        }
    }
}
