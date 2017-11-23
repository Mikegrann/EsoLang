using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace EsoLang.Tests
{
    [TestClass()]
    public class ParserTests
    {
        [TestMethod()]
        public void parseTest()
        {
            // Test nesting, one child (linear)
            SExpression root = new SExpression(SExpression.FormatType.Document);
            SExpression statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression str = statement.AddChild(new SString("testString"));

            Assert.AreEqual("IdC(\"testString\")", Parser.parse(str).ToString());
            Assert.AreEqual("IdC(\"testString\")", Parser.parse(statement).ToString());
            Assert.AreEqual("SeqC(IdC(\"testString\"))", Parser.parse(root).ToString());

            // Test nesting, many children (branching)
            statement = root.AddChild(new SExpression(SExpression.FormatType.Statement));
            SExpression under = statement.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("x"));
            SExpression italic = under.AddChild(new SExpression(SExpression.FormatType.Italic));
            italic.AddChild(new SString("y"));
            SExpression strike = italic.AddChild(new SExpression(SExpression.FormatType.Strike));
            strike.AddChild(new SString("z"));

            Assert.AreEqual("BindC(IdC(\"x\"), AddC(IdC(\"y\"), NegC(IdC(\"z\"))))", Parser.parse(under).ToString());
            Assert.AreEqual("BindC(IdC(\"x\"), AddC(IdC(\"y\"), NegC(IdC(\"z\"))))", Parser.parse(statement).ToString());
            Assert.AreEqual("SeqC(IdC(\"testString\"), BindC(IdC(\"x\"), AddC(IdC(\"y\"), NegC(IdC(\"z\")))))", Parser.parse(root).ToString());

            // Test last few types
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

            Assert.AreEqual("IfC(IdC(\"cond\"), SeqC(BindC(IdC(\"a\"), AddC(IdC(\"a\"), IdC(\"one\"))), FuncC(IdC(\"Mult\"), (IdC(\"a\"), NumC(10)))), IndexC(IdC(\"Arr\")[IdC(\"b\")]))", Parser.parse(statement).ToString());
            Assert.AreEqual("SeqC(IdC(\"testString\"), " +
                "BindC(IdC(\"x\"), AddC(IdC(\"y\"), NegC(IdC(\"z\")))), " +
                "IfC(IdC(\"cond\"), SeqC(BindC(IdC(\"a\"), AddC(IdC(\"a\"), IdC(\"one\"))), FuncC(IdC(\"Mult\"), (IdC(\"a\"), NumC(10)))), IndexC(IdC(\"Arr\")[IdC(\"b\")])))", Parser.parse(root).ToString());

            // Test size for call instead of sequencing
            size = new SExpression(SExpression.FormatType.Size);
            size.AddChild(new SString("Div"));
            super = size.AddChild(new SExpression(SExpression.FormatType.Super));
            super.AddChild(new SString("x"));
            SExpression num = super.AddChild(new SExpression(SExpression.FormatType.Color, null, 5));
            num.AddChild(new SString("fiveVal"));

            Assert.AreEqual("FuncC(IdC(\"Div\"), (IdC(\"x\"), NumC(5)))", Parser.parse(size).ToString());
        }

        // Check that the new preparsing-exclusive SExpression forms work
        [TestMethod()]
        public void PreParseSExpressions()
        {
            SExpression seq = new SExpression(SExpression.FormatType.Sequence);
            SExpression under = seq.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("result"));

            List<SExpression> funcArgs = new List<SExpression>();
            funcArgs.Add(new SString("x"));
            funcArgs.Add(new SString("y"));
            under.AddChild(new SWithArgs(SExpression.FormatType.Function, new SString("func"), funcArgs));

            SExpression idx = seq.AddChild(new SWithArgs(SExpression.FormatType.Index, new SString("array")));
            idx.AddChild(new SString("result"));

            Assert.AreEqual("seq(under(\"result\", function(\"func\", (\"x\", \"y\"))), index(\"array\", (\"result\")))", seq.ToString());

            Assert.AreEqual("SeqC(BindC(IdC(\"result\"), FuncC(IdC(\"func\"), (IdC(\"x\"), IdC(\"y\")))), IndexC(IdC(\"array\")[IdC(\"result\")]))", Parser.parse(seq).ToString());
        }

        /* Test incorrect argument counts (exceptions thrown) */
        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void DocumentEmpty()
        {
            Parser.parse(new SExpression(SExpression.FormatType.Document));
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void DocumentBadChildren()
        {
            SExpression root = new SExpression(SExpression.FormatType.Document);
            root.AddChild(new SString("test"));
            Parser.parse(root);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void StatementEmpty()
        {
            Parser.parse(new SExpression(SExpression.FormatType.Statement));
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void StatementBadChildren()
        {
            SExpression statement = new SExpression(SExpression.FormatType.Statement);
            statement.AddChild(new SString("test1"));
            statement.AddChild(new SString("test2"));
            Parser.parse(statement);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void BoldBadChildren()
        {
            SExpression bold = new SExpression(SExpression.FormatType.Bold);
            bold.AddChild(new SString("test"));
            Parser.parse(bold);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void ItalicBadChildren()
        {
            SExpression italic = new SExpression(SExpression.FormatType.Italic);
            italic.AddChild(new SString("test"));
            Parser.parse(italic);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void UnderBadChildren()
        {
            SExpression under = new SExpression(SExpression.FormatType.Under);
            under.AddChild(new SString("test1"));
            under.AddChild(new SString("test2"));
            under.AddChild(new SString("test3"));
            Parser.parse(under);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void StrikeBadChildren()
        {
            SExpression strike = new SExpression(SExpression.FormatType.Strike);
            Parser.parse(strike);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SizeBadChildren()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            size.AddChild(new SString("test1"));
            size.AddChild(new SString("test2"));
            Parser.parse(size);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void ColorBadChildren()
        {
            SExpression color = new SExpression(SExpression.FormatType.Color);
            Parser.parse(color);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void StringBadChildren()
        {
            SExpression str = new SString("test");
            str.AddChild(new SString("testSub"));
            Parser.parse(str);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void FunctionBadChildren()
        {
            SExpression func = new SWithArgs(SExpression.FormatType.Function, new SString("test"));
            Parser.parse(func);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void IndexBadChildren()
        {
            SExpression func = new SWithArgs(SExpression.FormatType.Index, new SString("test"));
            Parser.parse(func);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SequenceBadChildren()
        {
            SExpression seq = new SExpression(SExpression.FormatType.Sequence);
            Parser.parse(seq);
        }

        /* Test incorrect types (exceptions thrown) */
        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void StringNotSString()
        {
            SExpression str = new SExpression(SExpression.FormatType.String);
            Parser.parse(str);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void FunctionNotSWithArgs()
        {
            SExpression func = new SExpression(SExpression.FormatType.Function);
            func.AddChild(new SString("test"));
            Parser.parse(func);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void IndexNotSWithArgs()
        {
            SExpression idx = new SExpression(SExpression.FormatType.Index);
            idx.AddChild(new SString("test"));
            Parser.parse(idx);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SuperType()
        {
            SExpression super = new SExpression(SExpression.FormatType.Super);
            Parser.parse(super);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SubType()
        {
            SExpression sub = new SExpression(SExpression.FormatType.Sub);
            Parser.parse(sub);
        }

        /* Test preparse issues */
        [TestMethod()]
        [ExpectedException(typeof(EsoException))]
        public void SWithArgsBadConstruct()
        {
            SExpression func = new SWithArgs(SExpression.FormatType.Super, new SString("base"));
        }

        [TestMethod()]
        [ExpectedException(typeof(EsoException))]
        public void SWithArgsBadConstructArgs()
        {
            List<SExpression> argList = new List<SExpression>();
            argList.Add(new SString("test"));
            SExpression func = new SWithArgs(SExpression.FormatType.Sub, new SString("base"), argList);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SuperFirstChild()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            SExpression super = size.AddChild(new SExpression(SExpression.FormatType.Super));
            super.AddChild(new SString("arg1"));
            super.AddChild(new SString("arg2"));
            Parser.parse(size);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void SubFirstChild()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            SExpression sub = size.AddChild(new SExpression(SExpression.FormatType.Sub));
            sub.AddChild(new SString("arg1"));
            sub.AddChild(new SString("arg2"));
            Parser.parse(size);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void CommaToken()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            SExpression under = size.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("bad,arg"));
            under.AddChild(new SString("test"));
            size.AddChild(new SString(","));
            size.AddChild(new SString("test2"));
            Parser.parse(size);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void CommaFirst()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            size.AddChild(new SString(","));
            SExpression under = size.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("test1"));
            under.AddChild(new SString("test2"));
            size.AddChild(new SString(","));
            size.AddChild(new SString("test3"));
            Parser.parse(size);
        }

        [TestMethod()]
        [ExpectedException(typeof(ParseException))]
        public void CommaLast()
        {
            SExpression size = new SExpression(SExpression.FormatType.Size);
            SExpression under = size.AddChild(new SExpression(SExpression.FormatType.Under));
            under.AddChild(new SString("test1"));
            under.AddChild(new SString("test2"));
            size.AddChild(new SString(","));
            size.AddChild(new SString("test3"));
            size.AddChild(new SString(","));
            Parser.parse(size);
        }
    }
}