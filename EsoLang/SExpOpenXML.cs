using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.IO;

namespace EsoLang
{
    public class SExpOpenXML : SExpression
    {
        // Builds a full SExpression tree from a supplied file
        public SExpOpenXML(String fileName) : base(fileName)
        {
            // Singular root node, parent to all "Statement" expressions
            type = FormatType.Document;

            WordprocessingDocument wDoc;
            // Open file via OpenXML
            try
            {
                Stream fStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // Read-only
                wDoc = WordprocessingDocument.Open(fStream, false);
            }
            catch (Exception)
            {
                throw new ParseException("Could not open file");
            } 

            List<Tuple<String, FontStyle>> statement = new List<Tuple<String, FontStyle>>();
            // Analyze each "Statement" (Sentence, as split by '.' chars)
            // Build a child SExpression tree representing the statement
            foreach (Run section in wDoc.MainDocumentPart.Document.Body.Descendants<Run>())
            {
                if (section.InnerText.Trim() == ".")
                {
                    statement.Add(new Tuple<string, FontStyle>(
                        section.InnerText,
                        getFont(section.GetFirstChild<RunProperties>())));
                    AddChild(new SExpOpenXML(statement));
                    statement = new List<Tuple<String, FontStyle>>();
                }
                else if (section.InnerText.Contains("."))
                {
                    FontStyle fs = getFont(section.GetFirstChild<RunProperties>());
                    statement = innerPeriod(statement, section.InnerText, fs);
                }
                else if (section.InnerText != "")
                {
                    statement.Add(new Tuple<string, FontStyle>(
                        section.InnerText,
                        getFont(section.GetFirstChild<RunProperties>())));
                }
            }

            // Cleanup open file
            wDoc.Close();
        }

        // TODO - Test heavily
        private List<Tuple<String, FontStyle>> innerPeriod(List<Tuple<String, FontStyle>> statement, String txt, FontStyle fs) 
        {
            int periodPos = txt.IndexOf(".");
            string before = txt.Substring(0, periodPos + 1);
            string after = txt.Substring(periodPos + 1);

            statement.Add(new Tuple<string, FontStyle>(before, fs));
            statement.Add(new Tuple<string, FontStyle>(".", fs));
            AddChild(new SExpOpenXML(statement));

            // recurse in case of multiple periods
            if (after.Contains("."))
            {
                return innerPeriod(new List<Tuple<string, FontStyle>>(), after, fs);
            }
            else
            {
                return new List<Tuple<string, FontStyle>>();
            }
        }

        // Builds an SExpression subtree representing a single Statement
        // Traverses each character in turn, looking for style differences
        //  and opening or closing sub-branches based on the types of changes
        private SExpOpenXML(List<Tuple<String, FontStyle>> statement) : base(FormatType.Statement)
        {
            // Initialize the read state
            ParseState state = new ParseState(this, new FontStyle(), "");

            // Read each character individually, consecutively
            foreach (Tuple<String, FontStyle> section in statement)
            {
                SExpression.handleCharacter(state, section.Item2, section.Item1);
            }

            // Flush any remaining tokens, appending them to the current node
            if (state.curString.Length > 0)
            {
                state.current.AddString(state.curString);
            }

            // Clear any bad formatting (childless nodes)
            this.cullBachelors();
        }

        private static FontStyle getFont(RunProperties prop)
        {
            FontStyle fs = new FontStyle();

            fs.bold = (prop.Bold != null && prop.Bold.Val != "false");
            fs.italic = (prop.Italic != null && prop.Italic.Val != "false");
            fs.underline = (prop.Underline != null && prop.Underline.Val != "none");
            fs.sub = (prop.VerticalTextAlignment != null &&
                prop.VerticalTextAlignment.Val == VerticalPositionValues.Subscript);
            fs.super = (prop.VerticalTextAlignment != null &&
                prop.VerticalTextAlignment.Val == VerticalPositionValues.Superscript);
            fs.strike = (prop.Strike != null && prop.Strike.Val != "false");
            fs.size = (prop.FontSize != null ? Convert.ToUInt32(prop.FontSize.Val.Value)/2 : FontStyle.defaultSize);
            fs.color = (prop.Color != null ? toRGB(prop.Color.Val) : FontStyle.defaultColor);
            fs.name = (prop.RunFonts != null && prop.RunFonts.HighAnsi != null ? prop.RunFonts.HighAnsi.Value : FontStyle.defaultFont);

            return fs;
        }

        private static uint toRGB(string hex)
        {
            string hexstr = hex.Substring(hex.Length - 6);
            return Convert.ToUInt32(hexstr, 16);
        }
    }
}
