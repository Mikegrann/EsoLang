using System;
using System.Collections.Generic;

namespace EsoLang
{
    // The ExprC interface represents an expression to be evaluated
    // NumC, IdC, FuncC, AddC, NegC, BindC, SeqC, and IfC are all subclasses of ExprC
    [Serializable]
    public abstract class ExprC {
        public abstract Val interp(Environment env);

        public static List<Val> interpAll(List<ExprC> exps, Environment env)
        {
            List<Val> result = new List<Val>();

            foreach (ExprC exp in exps)
            {
                result.Add(exp.interp(env));
            }

            return result;
        }
    }

    // Represents a number literal (integers only)
    [Serializable]
    public class NumC : ExprC
    {
        public uint num;

        // num will always be 0 <= num <= 0xFFFFFF
        public NumC(uint num)
        {
            this.num = num;
        }

        public override string ToString()
        {
            return "NumC(" + num + ")";
        }

        public override Val interp(Environment env)
        {
            return new NumV((long)num);
        }
    }

    // Represents a string literal
    [Serializable]
    public class StrC : ExprC
    {
        public string str;

        // num will always be 0 <= num <= 0xFFFFFF
        public StrC(string str)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return "StrC(\"" + str + "\")";
        }

        public override Val interp(Environment env)
        {
            return new StrV(str);
        }
    }

    // Represents a variable reference using a token
    [Serializable]
    public class IdC : ExprC
    {
        public String sym;

        public IdC(String sym)
        {
            this.sym = sym;
        }

        public override string ToString()
        {
            return "IdC(\"" + sym + "\")";
        }

        public override Val interp(Environment env)
        {
            if (env.ContainsKey(sym))
            {
                Val result = env[sym];

                if (result is CloV)
                {
                    result = ((CloV)result).DeepCopy(); // Ensure deep copy on a Closure (immutable environment)
                }

                return result;
            }
            else
            {
                throw new InterpException("Dereferencing variable not in environment");
            }
        }
    }

    // Represents a function with arguments
    // Could be a definition or a call
    [Serializable]
    public class FuncC : ExprC
    {
        public ExprC func;
        public List<ExprC> args;

        public FuncC(ExprC func, List<ExprC> args)
        {
            this.func = func;
            this.args = args;
        }

        public override string ToString()
        {
            string argsList = "";
            foreach (ExprC arg in args)
            {
                argsList += ", " + arg.ToString();
            }
            argsList = argsList.Substring(2);
            return "FuncC(" + func + ", (" + argsList + "))";
        }

        public override Val interp(Environment env)
        {
            if (func is IdC && ((IdC)func).sym == "copyArr")
            {
                Console.WriteLine("HERE");
            }

            // Only should be invoked for function calls
            // BindC handles closure definitions
            CloV called = func.interp(env) as CloV;
            if (called != null) // (is a closure)
            {
                Environment callingContext = env;
                Environment cloEnv = addArgsToEnv(called.parms, args, called.env, env);
                cloEnv.scopeDepth++;
                Val retVal = called.body.interp(cloEnv);
                cloEnv.scopeDepth--;

                // Propogate changes up to caller
                /*foreach (KeyValuePair<string, Val> pair in cloEnv)
                {
                    // Only for vars originally in scope; don't copy parameters
                    if (callingContext.ContainsKey(pair.Key) && !called.parms.Contains(pair.Key))
                    {
                        callingContext[pair.Key] = pair.Value;
                    }
                }*/

                return retVal;
            }
            else
            {
                throw new InterpException("Calling a non-closure");
            }
        }
        
        public static Environment addArgsToEnv(List<String> parms, List<ExprC> args,
            Environment cloEnv, Environment outerEnv)
        {
            if (parms.Count == args.Count)
            {
                Environment newEnv = outerEnv.withLocals(cloEnv); // scope to know all calling vars

                for (int i = 0; i < parms.Count; i++)
                {
                    String param = parms[i];
                    ExprC arg = args[i];
                    Val argVal = arg.interp(outerEnv);
                    newEnv.SetLocal(param, argVal); // overwrite params if var name exists
                }

                return newEnv;
            }
            else
            {
                throw new InterpException("Calling mismatched argument count");
            }
        }
    }

    // Represents an array indexing with indices
    [Serializable]
    public class IndexC : ExprC
    {
        public ExprC arr;
        public List<ExprC> indices;

        public IndexC(ExprC arr, List<ExprC> indices)
        {
            this.arr = arr;
            this.indices = indices;
        }

        public override Val interp(Environment env)
        {
            List<Val> interpIndices = interpAll(indices, env);
            List<long> castIndices = new List<long>();
            
            foreach (Val idx in interpIndices)
            {
                if (idx is NumV)
                {
                    castIndices.Add(((NumV)idx).num);
                }
                else
                {
                    throw new InterpException("Indexing with non-numeric");
                }
            }

            Val interpArr = arr.interp(env);
            if (interpArr is ArrV)
            {
                return ((ArrV)interpArr).GetValue(castIndices);
            }
            else if (interpArr is StrV)
            {
                switch (castIndices.Count)
                {
                    case 1:
                        try
                        {
                            return new StrV(((StrV)interpArr).str.Substring((int)castIndices[0]));
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            return new StrV("");
                        }

                    case 2:
                        try
                        {
                            return new StrV(((StrV)interpArr).str.Substring((int)castIndices[0], (int)castIndices[1] - (int)castIndices[0] + 1));
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            return new StrV("");
                        }

                    default:
                        throw new InterpException("Indexing string with incorrect number of indices");
                }
            }
            else
            {
                throw new InterpException("Indexing into non-array");
            }
        }

        public override string ToString()
        {
            string idxList = "";
            foreach (ExprC idx in indices)
            {
                idxList += "[" + idx.ToString() + "]";
            }
            return "IndexC(" + arr + idxList + ")";
        }
    }

    // Represents an insertion into the environment (binding sym to a value)
    // Can be used similarly to an assignment (lexpr = rexpr)
    [Serializable]
    public class BindC : ExprC
    {
        public ExprC lexpr;
        public ExprC rexpr;

        public BindC(ExprC lexpr, ExprC rexpr)
        {
            this.lexpr = lexpr;
            this.rexpr = rexpr;
        }

        public override Val interp(Environment env)
        {
            Val result;

            if (lexpr is IdC) // binding a variable
            {
                result = rexpr.interp(env);
                env[((IdC)lexpr).sym] = result;
            }
            else if (lexpr is FuncC) // defining a function
            {
                FuncC lexprFunc = (FuncC)lexpr;

                if (lexprFunc.func is IdC)
                {
                    List<string> parms = new List<string>();
                    foreach (ExprC parm in lexprFunc.args)
                    {
                        if (parm is IdC)
                        {
                            parms.Add(((IdC)parm).sym);
                        }
                        else
                        {
                            throw new InterpException("Malformed function definition params");
                        }
                    }
                    result = new CloV((Environment)env.Clone(), rexpr, parms);
                    ((CloV)result).env.SetLocal(((IdC)lexprFunc.func).sym, result); // allow recursion
                    env[((IdC)lexprFunc.func).sym] = result;
                }
                else
                {
                    throw new InterpException("Malformed function definition name");
                }
            }
            else if (lexpr is IndexC) // defining an array or setting an element
            {
                IndexC lexprArray = (IndexC)lexpr;

                if (lexprArray.arr is IdC)
                {
                    List<long> indices = new List<long>();
                    foreach (ExprC idx in lexprArray.indices)
                    {
                        Val idxVal = idx.interp(env);

                        if (idxVal is NumV)
                        {
                            indices.Add(((NumV)idxVal).num);
                        }
                        else
                        {
                            throw new InterpException("Non-numeric array index");
                        }
                    }

                    if (env.ContainsKey(((IdC)lexprArray.arr).sym)) // setting element
                    {
                        ArrV arr = lexprArray.arr.interp(env) as ArrV;
                        if (arr == null)
                        {
                            throw new InterpException("Indexing non-array");
                        }
                        result = rexpr.interp(env);
                        arr.SetValue(indices, result);
                    }
                    else // defining new array
                    { 
                        result = new ArrV(indices, rexpr.interp(env));
                        env[((IdC)lexprArray.arr).sym] = result;
                    }
                }
                else
                {
                    throw new InterpException("Malformed array definition name");
                }
            }
            else
            {
                throw new InterpException("Binding with invalid lvalue");
            }

            return result;
        }

        public override string ToString()
        {
            return "BindC(" + lexpr + ", " + rexpr + ")";
        }
    }

    // Represents a conditional branch
    // Check would always be executed, but only t or f is executed
    //  exclusively based on the result of check
    [Serializable]
    public class IfC : ExprC
    {
        public ExprC check;
        public ExprC t;
        public ExprC f;

        public IfC(ExprC check, ExprC t, ExprC f)
        {
            this.check = check;
            this.t = t;
            this.f = f;
        }

        public override Val interp(Environment env)
        {
            Val checkVal = check.interp(env);
            if (checkVal is NumV)
            {
                return (((NumV)checkVal).num > 0) ? t.interp(env) : f.interp(env);
            }
            else if (checkVal is StrV)
            {
                return (((StrV)checkVal).str.Length > 0) ? t.interp(env) : f.interp(env);
            }
            else
            {
                throw new InterpException("Conditional jump on non-numeric, non-string");
            }
        }

        public override string ToString()
        {
            return "IfC(" + check + ", " + t + ", " + f + ")";
        }
    }

    // Represents a sum of multiple values
    [Serializable]
    public class AddC : ExprC
    {
        public List<ExprC> addends;

        public AddC(List<ExprC> addends)
        {
            this.addends = addends;
        }

        public override Val interp(Environment env)
        {
            object sum = (long)0;
            foreach (ExprC addend in addends)
            {
                Val addVal = addend.interp(env);

                if (sum is string)
                {
                    sum = (string)sum + addVal.ToString();
                }
                else
                {
                    if (addVal is NumV)
                    {
                        sum = (long)sum + ((NumV)addVal).num;
                    }
                    else if (addVal is StrV)
                    {
                        sum = sum.ToString() + ((StrV)addVal).str;
                    }
                    else
                    {
                        throw new InterpException("Adding non-numerics");
                    }
                }
            }

            if (sum is string)
            {
                return new StrV(sum.ToString());
            }
            else { 
                return new NumV((long)sum);
            }
        }

        public override string ToString()
        {
            string addList = "";
            foreach (ExprC addend in addends)
            {
                addList += ", " + addend;
            }
            addList = addList.Substring(2);
            return "AddC(" + addList + ")";
        }
    }

    // Represents a value-negation of a numeric expression
    // (i.e. 2's complement, not bitwise negation)
    [Serializable]
    public class NegC : ExprC
    {
        public ExprC num;

        public NegC(ExprC num)
        {
            this.num = num;
        }

        public override Val interp(Environment env)
        {
            NumV inner = num.interp(env) as NumV;
            if (inner == null)
            {
                throw new InterpException("Negating a non-numeric");
            }

            return new NumV(-inner.num);
        }

        public override string ToString()
        {
            return "NegC(" + num + ")";
        }
    }

    // Represents a collection of sequenced statements
    //  to be executed in order
    // Evaluates to the same value as the final subStatement
    [Serializable]
    public class SeqC : ExprC
    {
        public List<ExprC> subStatements;

        public SeqC(List<ExprC> subStatements)
        {
            this.subStatements = subStatements;
        }

        public override Val interp(Environment env)
        {
            Val mostRecent = null;

            foreach (ExprC exp in subStatements)
            {
                mostRecent = exp.interp(env);
            }

            if (mostRecent == null)
            {
                throw new InterpException("Sequencing zero substatements");
            }

            return mostRecent;
        }

        public override string ToString()
        {
            string statementList = "";
            foreach (ExprC statement in subStatements)
            {
                statementList += ", " + statement;
            }
            statementList = statementList.Substring(2);
            return "SeqC(" + statementList + ")";
        }
    }
}
