using System;
using System.Collections.Generic;

namespace EsoLang
{
    public class SExpression
    {
        public enum FormatType
        {
            Document,
            Statement,
            Bold,
            Italic,
            Under,
            Sub,
            Super,
            Strike,
            Size,
            Color,
            Font,
            
            String,
            Function,
            Index,
            Sequence
        };

        public FormatType type;

        // for Size/Color types
        public uint value;
        // for Font types
        public string name;

        // "Expected" style (including parent formats)
        // used to check for style changes on new text
        public FontStyle style;

        // Maintain links both up and down expression tree
        // to allow closing branches and walking back up the tree
        public List<SExpression> children;
        public SExpression parent;

        // Builds a full SExpression tree from a supplied file
        public SExpression(String fileName) {
            type = FormatType.Document;
            style = null;
            value = 0;
            name = "";
            children = new List<SExpression>();
            parent = null;
        }

        public SExpression(FormatType t, FontStyle s = null, object innerVal = null)
        {
            type = t;
            style = s;
            
            if (innerVal is string)
            {
                value = 0;
                name = (string)innerVal;
            }
            else if (innerVal is uint) {
                value = (uint)innerVal;
                name = "";
            }
            else
            {
                value = 0;
                name = "";
            }

            children = new List<SExpression>();
            parent = null;
        }

        /*
        public SExpression(List<SExpression> children, FormatType t, FontStyle s = null, uint v = 0, SExpression parent = null)
        {
            type = t;
            style = s;
            value = v;
            children = new List<SExpression>();
            foreach (SExpression child in children)
            {
                AddChild(child);
            }
            this.parent = parent;
        }
        */

        // Builds an SExpression subtree representing a single Statement
        // Traverses each character in turn, looking for style differences
        //  and opening or closing sub-branches based on the types of changes
        protected static void handleCharacter(ParseState state, FontStyle f, String chr) {
            // Save details from the first encountered character
            if (state.initialName == null)
            {
                state.current.style.name = state.curStyle.name = state.initialName = f.name;
                state.current.style.color = state.curStyle.color = state.initialColor = f.color;
            }

            // Save the style of the current character
            state.newStyle = f;

            // Check each format change in order of precedence
            //  and define how to handle the change if any is detected
            handleStyleDifference(state.newStyle.size != state.curStyle.size,
                state.newStyle.size > state.curStyle.size,
                FormatType.Size, state, state.newStyle.size);
            handleStyleDifference(state.newStyle.bold != state.curStyle.bold,
                state.newStyle.bold & !state.curStyle.bold,
                FormatType.Bold, state);
            handleStyleDifference(state.newStyle.sub != state.curStyle.sub,
                state.newStyle.sub & !state.curStyle.sub,
                FormatType.Sub, state);
            handleStyleDifference(state.newStyle.super != state.curStyle.super,
                state.newStyle.super & !state.curStyle.super,
                FormatType.Super, state);
            handleStyleDifference(state.newStyle.strike != state.curStyle.strike,
                state.newStyle.strike & !state.curStyle.strike,
                FormatType.Strike, state);
            handleStyleDifference(state.newStyle.italic != state.curStyle.italic,
                state.newStyle.italic & !state.curStyle.italic,
                FormatType.Italic, state);
            handleStyleDifference(state.newStyle.underline != state.curStyle.underline,
                state.newStyle.underline & !state.curStyle.underline,
                FormatType.Under, state);
            handleStyleDifference(state.newStyle.color != state.curStyle.color,
                state.newStyle.color != state.initialColor,
                FormatType.Color, state, state.newStyle.color);
            handleStyleDifference(state.newStyle.name != state.curStyle.name,
                state.newStyle.name != state.initialName,
                FormatType.Font, state, state.newStyle.name);

            // Accumulate this character onto the string for this token
            state.curString = state.curString + chr;

            // Update the "expected style" to the saved style of
            //  the currently opened node
            state.curStyle = new FontStyle(state.current.style);
        }

        // Used to build a text representation of the
        // expression tree as nested lists formatted like
        // sexpType[val](child1.ToString(), child2.ToString(), ...)
        public override string ToString()
        {
            string result = "";

            // Denotes current sexp Type (and value, if relevant)
            switch (type)
            {
                case FormatType.Document:
                    result = "document(";
                    break;
                case FormatType.Statement:
                    result = "statement(";
                    break;
                case FormatType.Bold:
                    result = "bold(";
                    break;
                case FormatType.Italic:
                    result = "italic(";
                    break;
                case FormatType.Under:
                    result = "under(";
                    break;
                case FormatType.Sub:
                    result = "sub(";
                    break;
                case FormatType.Super:
                    result = "super(";
                    break;
                case FormatType.Strike:
                    result = "strike(";
                    break;
                case FormatType.Size:
                    result = "size" + value + "(";
                    break;
                case FormatType.Color:
                    result = "color" + value + "(";
                    break;
                case FormatType.Font:
                    result = "font" + name.Replace(" ", "") + "(";
                    break;
                case FormatType.Sequence:
                    result = "seq(";
                    break;
            }

            // Recursively add children onto the list
            foreach (SExpression child in children)
            {
                result = result + child.ToString() + ", ";
            }

            // Remove final ", " and close the list
            if (children.Count > 0)
            {
                result = result.Substring(0, result.Length - 2);
            }
            result = result + ")";

            return result;
        }

        // Check if the style changed on the current character
        // If so, check whether the style was added or removed
        // Change state accordingly, either by adding a new child
        //  or by bubbling up the tree to the appropriate ancestor
        private static void handleStyleDifference(bool styleChanged, bool addNewChild, 
            FormatType newChildType, ParseState state, object childVal = null)
        {
            if (styleChanged)
            {
                // Flush any existing tokens onto the old formatting
                if (state.curString.Length > 0)
                {
                    if (state.current.type == FormatType.Font)
                    {
                        state.current.AddStringNoSplit(state.curString);
                    }
                    else
                    {
                        state.current.AddString(state.curString);
                    }

                    state.curString = "";
                }
                
                if (addNewChild)
                {
                    // Updated the expected style as needed and only
                    //  incrementally to ensure all changes get captured
                    switch (newChildType)
                    {
                        case FormatType.Bold:
                            state.curStyle.bold = state.newStyle.bold;
                            break;
                        case FormatType.Italic:
                            state.curStyle.italic = state.newStyle.italic;
                            break;
                        case FormatType.Under:
                            state.curStyle.underline = state.newStyle.underline;
                            break;
                        case FormatType.Sub:
                            state.curStyle.sub = state.newStyle.sub;
                            break;
                        case FormatType.Super:
                            state.curStyle.super = state.newStyle.super;
                            break;
                        case FormatType.Strike:
                            state.curStyle.strike = state.newStyle.strike;
                            break;
                        case FormatType.Size:
                            // New sizes flush all expected formatting
                            //  back to the defaults to allow nesting
                            state.curStyle = new FontStyle();
                            state.curStyle.color = state.initialColor;
                            state.curStyle.name = state.initialName;
                            state.curStyle.size = state.newStyle.size;
                            break;
                        case FormatType.Color:
                            state.curStyle.color = state.newStyle.color;
                            break;
                        case FormatType.Font:
                            state.curStyle.name = state.newStyle.name;
                            break;
                    }

                    // Add a child with the appropriate expected style
                    state.current = state.current.AddChild(
                        new SExpression(newChildType, new FontStyle(state.curStyle), childVal));
                }
                else
                {
                    // Pull back up the tree until an ancestor of the changed type is found
                    // If the type is Size, keep closing until the sizes match
                    //  to allow multiple nested group closures from a single size change
                    while (!(state.current.type == newChildType && 
                        (newChildType != FormatType.Size ||
                            state.current.parent.style.size == state.newStyle.size)))
                    {
                        state.current = state.current.parent;
                    }

                    // Update state to the desired ancestor's state
                    state.current = state.current.parent;
                    state.curStyle = state.current.style;
                }
            }
        }

        // Represents the machine's state while building an SExpression
        protected class ParseState
        {
            // Current location in the SExpression tree
            // Reacts to the formatting of the currently read character
            // A new added format "opens" a new child of that format
            // A newly missing format "closes" the current and 
            //  returns back up the tree to the appropriate ancestor
            public SExpression current;

            // curStyle is the "expected" or "previous" style based
            //  on earlier characters (used to detect changes)
            // newStyle is the style of this current character
            public FontStyle curStyle, newStyle;

            // Collects multiple consecutive characters of an unbroken style
            //  representing one or more tokens
            public String curString;

            // Captures initial formatting of this parse to set as "default"
            public string initialName;
            public uint initialColor;

            public ParseState(SExpression cur, FontStyle curS, String curStr)
            {
                current = cur;
                curStyle = curS;
                current.style = new FontStyle(curStyle); // Copy, not the same object
                curString = curStr;

                initialName = null;
                initialColor = 0;
            }
        }

        // Recursively removes "childless" nodes
        // All branches should terminate with SString tokens,
        //  not with formatting notes
        // Only likely to occur when whitespace is erroneously formatted
        public void cullBachelors()
        {
            // Loop backwards to allow removal of loop items
            for (int i = children.Count - 1; i >= 0; i--)
            {
                SExpression child = children[i];

                child.cullBachelors(); // recurse

                // Only remove childless non-SString
                if (!(child is SString) && child.isEmpty())
                {
                    children.Remove(child);
                }
            }
        }

        // Add the child and ensure it links back up to its parent
        // Returns the passed child object
        public SExpression AddChild(SExpression child)
        {
            children.Add(child);
            child.parent = this;
            return child;
        }

        // Add an accumulated, single-format string to the current node
        // Splits string on non-text (whitespace) characters into
        //  multiple tokens (ensuring all tokens have >0 length)
        // Adds the resulting tokens as children directly to this node
        // Returns a list of all added children
        public List<SString> AddString(String str)
        {
            List<SString> results = new List<SString>();
            SString child;

            String[] substrs = str.Split(new Char[] { ' ', '\t', '\v', '\r', '\n', '.' });
            foreach (string substr in substrs)
            {
                if (substr.Length > 0)
                {
                    child = (SString)AddChild(new SString(substr));
                    results.Add(child);
                }
            }

            return results;
        }

        // Add a single-format string to the current node
        // Allow whitespace (for string literals only)
        public SString AddStringNoSplit(String str)
        {
            return (SString)AddChild(new SString(str));
        }

        // Returns whether the node has any children
        public bool isEmpty()
        {
            return children.Count == 0;
        }
    }

    // Special type of SExpression with an added String-type member
    // Used to represent tokens instead of formatting
    public class SString : SExpression
    {
        // Represents the token
        public String str;

        public SString(String str) : base(SExpression.FormatType.String)
        {
            this.str = str;
        }

        // Format as a "quoted string" instead of a list
        public override string ToString()
        {
            return "\"" + str + "\"";
        }
    }

    // Special type of SExpression for preparsing ONLY
    // 'Type: Function - Represents a function call/definition structure (token/expression, then super)
    //   "Children" list is args/params
    // 'Type: Index - Represents indexing into an array (token/expression, then sub)
    //   "Children" list is indices in each dimension
    public class SWithArgs : SExpression
    {
        // Represents the token/expression
        public SExpression baseExp;

        public SWithArgs(FormatType type, SExpression baseExp) : base(type)
        {
            if (type != FormatType.Function && type != FormatType.Index)
            {
                throw new EsoException("Malformed SWithArgs type");
            }

            this.baseExp = baseExp;
        }


        public SWithArgs(FormatType type, SExpression baseExp, List<SExpression> args) : base(type)
        {
            if (type != FormatType.Function && type != FormatType.Index)
            {
                throw new EsoException("Malformed SWithArgs type");
            }

            this.baseExp = baseExp;

            foreach (SExpression arg in args)
            {
                AddChild(arg);
            }
        }

        // 'Type: Function - Format like "func([function], ([arg1], [arg2...]))
        // 'Type: Index - Format like "index([array], ([index1], [index2...]))
        public override string ToString()
        {
            string result = "";

            switch (type) {
                case FormatType.Function:
                    result = "function";
                    break;

                case FormatType.Index:
                    result = "index";
                    break;
            }
            
            result = result + "(" + baseExp.ToString() + ", (";

            // Recursively add children onto the list
            foreach (SExpression child in children)
            {
                result = result + child.ToString() + ", ";
            }

            // Remove final ", " and close the list
            if (children.Count > 0)
            {
                result = result.Substring(0, result.Length - 2);
            }
            result = result + "))";

            return result;
        }
    }
}
