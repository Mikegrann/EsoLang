using Microsoft.VisualStudio.TestTools.UnitTesting;
using EsoLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsoLang.Tests
{
    [TestClass()]
    public class SExpressionTests
    {
        [TestMethod()]
        public void ToStringTest()
        {
            // Test nesting, one child (linear)
            SExpression root = new SExpression(SExpression.FormatType.Document);
            SExpression statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression str = statement.AddChild(new SString("testString"));

            Assert.AreEqual("\"testString\"",
                str.ToString());
            Assert.AreEqual("statement(\"testString\")",
                statement.ToString());
            Assert.AreEqual("document(statement(\"testString\"))",
                root.ToString());

            // Test nesting, many children (branching)
            statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression under = statement.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("x"));
            SExpression italic = under.AddChild(new SExpression(SExpression.FormatType.Italic));
            italic.AddChild(new SString("y"));
            SExpression strike = italic.AddChild(new SExpression(SExpression.FormatType.Strike));
            strike.AddChild(new SString("z"));

            Assert.AreEqual("statement(under(\"x\", italic(\"y\", strike(\"z\"))))",
                statement.ToString());
            Assert.AreEqual("document(statement(\"testString\"), " + 
                "statement(under(\"x\", italic(\"y\", strike(\"z\")))))",
                root.ToString());

            // Test last few SExpression.FormatTypes
            statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression bold = statement.AddChild(new SExpression(SExpression.FormatType.Bold));
            bold.AddChild(new SString("cond"));
            SExpression size = bold.AddChild(new SExpression(SExpression.FormatType.Size, null, 12));
            under = size.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("a"));
            italic = under.AddChild(new SExpression(SExpression.FormatType.Italic));
            italic.AddChild(new SString("a"));
            italic.AddChild(new SString("one"));
            size.AddChild(new SString(","));
            size.AddChild(new SString("Mult"));
            SExpression super = size.AddChild(new SExpression(SExpression.FormatType.Super));
            super.AddChild(new SString("a"));
            SExpression color = super.AddChild(new SExpression(SExpression.FormatType.Color, null, 10));
            color.AddChild(new SString("tenVal"));
            bold.AddChild(new SString("Arr"));
            SExpression sub = bold.AddChild(new SExpression(SExpression.FormatType.Sub));
            sub.AddChild(new SString("b"));

            Assert.AreEqual("statement(bold(\"cond\", size12(under(\"a\", italic(\"a\", \"one\")), \",\", \"Mult\", super(\"a\", color10(\"tenVal\"))), \"Arr\", sub(\"b\")))",
                statement.ToString());
            Assert.AreEqual("document(statement(\"testString\"), " + 
                "statement(under(\"x\", italic(\"y\", strike(\"z\")))), " + 
                "statement(bold(\"cond\", size12(under(\"a\", italic(\"a\", \"one\")), \",\", \"Mult\", super(\"a\", color10(\"tenVal\"))), \"Arr\", sub(\"b\"))))",
                root.ToString());
        }

        [TestMethod()]
        public void fromFileTest()
        {
            string testFolder = @"D:\My Files\Documents\EsoLang\";

            // Try some simple assignments (multiple statements)
            SExpression test, test2;
            /*test = new SExpWord(testFolder + @"Esolang_Test.docx");
            test2 = new SExpOpenXML(testFolder + @"Esolang_Test.docx");
            
            Assert.AreEqual("document(" + 
                "statement(under(\"one\", color1(\"oneValue\"))), " +
                "statement(under(\"x\", italic(\"y\", strike(\"z\")))), " +
                "statement(bold(\"cond\", size12(under(\"a\", italic(\"a\", \"one\")), \",\", \"Mult\", super(\"a\", color10(\"tenVal\"))), \"Arr\", sub(\"b\")))" +
                ")",
                test.ToString());
            Assert.AreEqual(test.ToString(), test2.ToString());
            */
            // Try the full starting environment (assume noException = success)
            test = new SExpWord(testFolder + @"Esolang_StartEnv.docx");
            //test2 = new SExpOpenXML(testFolder + @"Esolang_StartEnv.docx");
            //Assert.AreEqual(test.ToString(), test2.ToString());

            // Try the full PISpigot program
            test = new SExpWord(testFolder + @"Esolang_PiSpigot.docx");
            //test2 = new SExpOpenXML(testFolder + @"Esolang_PiSpigot.docx");
            //Assert.AreEqual(test.ToString(), test2.ToString());
        }

        [TestMethod()]
        public void cullBachelorsTest()
        {
            // Test removal of bachelors as leaves on different levels
            SExpression root = new SExpression(SExpression.FormatType.Document);
            SExpression statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression under = statement.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("x"));
            SExpression italic = under.AddChild(new SExpression(SExpression.FormatType.Italic));
            italic.AddChild(new SString("y"));
            SExpression strike = italic.AddChild(new SExpression(SExpression.FormatType.Strike));
            strike.AddChild(new SString("z"));
            italic.AddChild(new SExpression(SExpression.FormatType.Color, null, 15));
            under.AddChild(new SExpression(SExpression.FormatType.Size, null, 12));
            
            Assert.AreEqual("document(statement(under(\"x\", italic(\"y\", strike(\"z\"), color15()), size12())))",
                root.ToString());

            root.cullBachelors();
            Assert.AreEqual("document(statement(under(\"x\", italic(\"y\", strike(\"z\")))))",
                root.ToString());

            // Ensure subsequent calls don't remove anything else
            root.cullBachelors();
            Assert.AreEqual("document(statement(under(\"x\", italic(\"y\", strike(\"z\")))))",
                root.ToString());
        }

        [TestMethod()]
        public void AddStringTest()
        {
            SExpression super = new SExpression(SExpression.FormatType.Super);
            Assert.AreEqual("super()",
                super.ToString());

            // Empty string does nothing
            super.AddString("");
            Assert.AreEqual("super()",
                super.ToString());

            // A whitespace-less token is added straight
            super.AddString("word");
            Assert.AreEqual("super(\"word\")",
                super.ToString());

            // Whitespace is stripped from ends of a token
            super.AddString(" \ttest\n");
            Assert.AreEqual("super(\"word\", \"test\")",
                super.ToString());

            // Whitespace is used to split multiple tokens
            // '.' is whitespace, ',' is its own character like any other
            super.AddString(" \vfoo . bar\r,\n");
            Assert.AreEqual("super(\"word\", \"test\", \"foo\", \"bar\", \",\")",
                super.ToString());
        }

        [TestMethod()]
        public void isEmptyTest()
        {
            SExpression root = new SExpression(SExpression.FormatType.Document);
            Assert.IsTrue(root.isEmpty());

            SExpression statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            Assert.IsFalse(root.isEmpty());
            Assert.IsTrue(statement.isEmpty());
        }
    }
}