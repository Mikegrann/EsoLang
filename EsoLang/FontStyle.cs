using Microsoft.Office.Interop.Word;
using System;

namespace EsoLang
{
    public class FontStyle
    {
        public bool bold, italic, underline, sub, super, strike;
        public uint size, color;
        public string name;

        public const uint defaultSize = 11;
        public const uint defaultColor = 0; // Black
        public const string defaultFont = "";

        // Document's default starting font
        public FontStyle()
        {
            bold = italic = underline = sub = super = strike = false;
            size = defaultSize;
            color = defaultColor;
            name = defaultFont;
        }

        // Copy constructor
        public FontStyle(FontStyle f)
        {
            bold = f.bold;
            italic = f.italic;
            underline = f.underline;
            sub = f.sub;
            super = f.super;
            strike = f.strike;
            size = f.size;
            color = f.color;
            name = f.name;
        }
    }
}
