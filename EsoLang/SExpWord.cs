using System;
using Microsoft.Office.Interop.Word;

namespace EsoLang
{
    public class SExpWord : SExpression
    {
        // Builds a full SExpression tree from a supplied file
        public SExpWord(String fileName) : base(fileName)
        {
            // Singular root node, parent to all "Statement" expressions
            type = FormatType.Document;

            Document wDoc;
            Application wApp;
            // Open file via Word interop
            try
            {
                wApp = new Application();
                wDoc = wApp.Documents.Open(fileName, false, true);
            }
            catch (Exception)
            {
                throw new ParseException("Could not open file");
            }

            // Analyze each "Statement" (Sentence, as split by '.' chars)
            // Build a child SExpression tree representing the statement
            foreach (Range s in wDoc.Sentences)
            {
                AddChild(new SExpWord(s));
            }

            // Cleanup interop
            wDoc.Close(false);
            if (wApp != null) { wApp.Quit(false); }
        }

        // Builds an SExpression subtree representing a single Statement
        // Traverses each character in turn, looking for style differences
        //  and opening or closing sub-branches based on the types of changes
        private SExpWord(Range s) : base(FormatType.Statement)
        {
            // Initialize the read state
            ParseState state = new ParseState(this, new FontStyle(), "");

            // Read each character individually, consecutively
            foreach (Range character in s.Characters)
            {
                SExpression.handleCharacter(state, getFont(character.Font), character.Text);
            }

            // Flush any remaining tokens, appending them to the current node
            if (state.curString.Length > 0)
            {
                state.current.AddString(state.curString);
            }

            // Clear any bad formatting (childless nodes)
            this.cullBachelors();
        }

        private static FontStyle getFont(Font f)
        {
            FontStyle fs = new FontStyle();

            fs.bold = (f.Bold != 0);
            fs.italic = (f.Italic != 0);
            fs.underline = (f.Underline != 0);
            fs.sub = (f.Subscript != 0);
            fs.super = (f.Superscript != 0);
            fs.strike = (f.StrikeThrough != 0);
            fs.size = (uint)f.Size;
            fs.color = toRGB((uint)f.TextColor.RGB); // Endianness swap
            fs.name = f.Name;

            return fs;
        }

        // Reverses endianness and discards "alpha" value
        // Takes in ABGR format (as supplied by Word interop)
        private static uint toRGB(uint ABGRval)
        {
            return ((ABGRval & 0x000000FF) << 16) |   // R
                   ((ABGRval & 0x0000FF00)) |   // G
                   ((ABGRval & 0x00FF0000) >> 16);    // B
        }
    }
}
