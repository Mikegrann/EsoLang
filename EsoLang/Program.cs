using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EsoLang
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool refreshEnv = false;
            bool showSexp = false;
            bool showExpr = false;

            List<string> files = new List<string>();
            foreach (string arg in args)
            {
                if (arg.Substring(0, 1) == "-")
                {
                    switch (arg)
                    {
                        case "-r":
                        case "--refreshEnv":
                            refreshEnv = true;
                            break;

                        case "-v":
                        case "--verbose":
                            showSexp = true;
                            showExpr = true;
                            break;

                        default:
                            Console.Error.WriteLine("Unrecognized commandline option: " + arg);
                            break;
                    }
                }
                else
                {
                    files.Add(arg);
                }
            }

            Environment env = GetStartEnv(refreshEnv);
            Val result = new NumV(-1);

            foreach (string file in files)
            {
                SExpression sexp = new SExpOpenXML(file);
                if (showSexp)
                {
                    Console.WriteLine("Lexical Analyzer Output:");
                    Console.WriteLine(sexp.ToString());
                }

                ExprC prog = Parser.parse(sexp);
                if (showExpr)
                {
                    Console.WriteLine("Parser Output:");
                    Console.WriteLine(prog.ToString());
                }

                result = prog.interp(env);
            }

            Console.WriteLine(result.ToString());
        }

        public static Environment GetStartEnv(bool refresh)
        {
            Environment env;

            if (refresh)
            {
                env = GetStartEnvDoc();
            }
            else
            {
                try // Read from serialized binary file
                {
                    env = GetStartEnvBin();
                }
                catch (Exception) // Rebuild from source
                {
                    env = GetStartEnvDoc();
                }
            }

            return env;
        }

        private static Environment GetStartEnvBin()
        {
            using (Stream stream = File.Open(@"include\Esolang_StartEnv.bin", FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                return (Environment)binaryFormatter.Deserialize(stream);
            }
        }

        private static Environment GetStartEnvDoc()
        {
            Environment env = new Environment();

            try
            {
                Parser.parse(new SExpOpenXML(@"include\Esolang_StartEnv.docx")).interp(env);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error interpretting environment. Continuing with empty environment.");
                env = new Environment();
            }

            try // attempt to save to binary file for future
            {
                using (Stream stream = File.Open(@"include\Esolang_StartEnv.bin", FileMode.Create))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, env);
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Could not save parsed environment to file");
            }

            return env;
        }
    }
}
