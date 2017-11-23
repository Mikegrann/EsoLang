using System.Collections.Generic;

namespace EsoLang
{
    public static class Parser
    {
        // Pre-parse on the current level for functions, indexing, sequences
        // Convert from raw style-form to interprettable expression tree
        public static ExprC parse(SExpression rawExpr)
        {
            SExpression expr = preParse(rawExpr);

            ExprC result;

            // Check arg counts and expr object classes for errors
            // Convert to appropriate expression tree (w/ correct subtree)
            switch (expr.type)
            {
                case SExpression.FormatType.Document:
                    if (expr.children.Count < 1)
                    {
                        throw new ParseException("Incorrect Child Number: Document");
                    }
                    foreach (SExpression child in expr.children)
                    {
                        if (child.type != SExpression.FormatType.Statement)
                        {
                            throw new ParseException("Document with non-Statement children");
                        }
                    }
                    result = new SeqC(parseEach(expr.children));
                    break;

                case SExpression.FormatType.Statement:
                    if (expr.children.Count > 1)
                    {
                        throw new ParseException("Incorrect Child Number: Statement");
                    }
                    else if (expr.children.Count == 0)
                    {
                        throw new ParseException("Empty statement");
                    }
                    result = parse(expr.children[0]);
                    break;

                case SExpression.FormatType.Bold:
                    if (expr.children.Count != 3)
                    {
                        throw new ParseException("Incorrect Child Number: Conditional");
                    }
                    result = new IfC(parse(expr.children[0]),
                        parse(expr.children[1]),
                        parse(expr.children[2]));
                    break;

                case SExpression.FormatType.Italic:
                    if (expr.children.Count < 2)
                    {
                        throw new ParseException("Incorrect Child Number: Addition");
                    }
                    result = new AddC(parseEach(expr.children));
                    break;

                case SExpression.FormatType.Under:
                    if (expr.children.Count != 2)
                    {
                        throw new ParseException("Incorrect Child Number: Binding");
                    }
                    result = new BindC(parse(expr.children[0]), parse(expr.children[1]));
                    break;

                case SExpression.FormatType.Strike:
                    if (expr.children.Count != 1)
                    {
                        throw new ParseException("Incorrect Child Number: Negation");
                    }
                    result = new NegC(parse(expr.children[0]));
                    break;

                case SExpression.FormatType.Size:
                    if (expr.children.Count != 1)
                    {
                        throw new ParseException("Incorrect Child Number: Grouping");
                    }
                    result = parse(expr.children[0]);
                    break;

                case SExpression.FormatType.Color:
                    if (expr.children.Count != 1)
                    {
                        throw new ParseException("Incorrect Child Number: Integer Literal");
                    }
                    result = new NumC(expr.value);
                    break;

                case SExpression.FormatType.String:
                    if (expr.children.Count != 0)
                    {
                        throw new ParseException("Incorrect Child Number: Identifier");
                    }
                    if (!(expr is SString))
                    {
                        throw new ParseException("String type on non-SString");
                    }
                    result = new IdC(((SString)expr).str);
                    break;

                case SExpression.FormatType.Function:
                    if (expr.children.Count < 1)
                    {
                        throw new ParseException("Incorrect Child Number: Function");
                    }
                    if (!(expr is SWithArgs))
                    {
                        throw new ParseException("Function type on non-SWithArgs");
                    }
                    result = new FuncC(parse(((SWithArgs)expr).baseExp), parseEach(expr.children));
                    break;

                case SExpression.FormatType.Index:
                    if (expr.children.Count < 1)
                    {
                        throw new ParseException("Incorrect Child Number: Index");
                    }
                    if (!(expr is SWithArgs))
                    {
                        throw new ParseException("Index type on non-SWithArgs");
                    }
                    result = new IndexC(parse(((SWithArgs)expr).baseExp), parseEach(expr.children));
                    break;

                case SExpression.FormatType.Sequence:
                    if (expr.children.Count < 1)
                    {
                        throw new ParseException("Incorrect Child Number: Sequence");
                    }
                    result = new SeqC(parseEach(expr.children));
                    break;

                case SExpression.FormatType.Font:
                    if (expr.children.Count != 1)
                    {
                        throw new ParseException("Incorrect Child Number: String Literal");
                    }
                    if (!(expr.children[0] is SString))
                    {
                        throw new ParseException("String Literal with non-string identifier");
                    }
                    result = new StrC(((SString)expr.children[0]).str);
                    break;

                default:
                case SExpression.FormatType.Sub:
                case SExpression.FormatType.Super:
                    throw new ParseException("Impossible SExpression type");
            }

            return result;
        }

        // Helper to parse through list items in turn
        public static List<ExprC> parseEach(List<SExpression> exprList)
        {
            List<ExprC> result = new List<ExprC>();

            foreach (SExpression expr in exprList)
            {
                result.Add(parse(expr));
            }

            return result;
        }

        // Converts language forms into more parse-friendly structures
        // Only acts on current level (not recursive)
        private static SExpression preParse(SExpression expr)
        {
            SExpression result = expr;

            // Replace token+super with Function, token+sub with Index
            for (int i = expr.children.Count - 1; i >= 0;  i--)
            {
                SExpression tmp = expr.children[i];

                if (tmp.type == SExpression.FormatType.Super ||
                    tmp.type == SExpression.FormatType.Sub) { 
                    // Ensure a token/expression exists as a base for the SWithArgs
                    if (i == 0)
                    {
                        throw new ParseException("Super/Sub first child");
                    }

                    SExpression.FormatType replaceType = (tmp.type == SExpression.FormatType.Super ? 
                        SExpression.FormatType.Function : SExpression.FormatType.Index);
                    SWithArgs newExpr = new SWithArgs(replaceType, expr.children[i - 1], tmp.children);
                    newExpr.parent = expr;

                    // Replace the two children with a single "newExpr"
                    expr.children[i - 1] = newExpr;
                    expr.children.RemoveAt(i);
                    i--;
                }
            }

            // Replace comma-separated statements with Sequence
            bool containsCommas = false;
            foreach (SExpression child in expr.children)
            {
                if (child is SString && ((SString)child).str.Contains(","))
                {
                    containsCommas = true;
                    if (((SString)child).str != ",")
                    {
                        throw new ParseException("Comma in token");
                    }
                    break;
                }
            }

            if (containsCommas)
            {
                result = new SExpression(SExpression.FormatType.Sequence);

                bool expectComma = false;
                foreach (SExpression child in expr.children)
                {
                    // Ensure proper comma-separated list of expressions
                    // E.g. "a,b,c" not ",a,b" or ",a b"
                    if ((expectComma && !(child is SString && ((SString)child).str == ",")) ||
                        (!expectComma && child is SString && ((SString)child).str == ","))
                    {
                        throw new ParseException("Sequencing with improper commas");
                    }

                    if (!expectComma)
                    {
                        result.AddChild(child);
                    }

                    expectComma = !expectComma;
                }

                // Ensure proper comma-separated list of expressions
                // E.g. "a,b,c" not "a,b," or ",a,b,"
                SExpression lastChild = expr.children[expr.children.Count - 1];
                if (lastChild is SString && ((SString)lastChild).str == ",") {
                    throw new ParseException("Sequencing with trailing comma");
                }
            }

            return result;
        }
    }
}
