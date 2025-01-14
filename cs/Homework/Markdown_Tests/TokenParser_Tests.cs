﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Markdown;
using Markdown.Parser;
using Markdown.Tokens;
using NUnit.Framework;

namespace Markdown_Tests
{
    [TestFixture]
    public class TokenParser_Tests
    {
        private readonly MdTokenParser sut = new();

        [Test]
        public void Parse_Throw_WhenNullArgument()
        {
            Action act = () => sut.Parse(null).First();
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Parse_ReturnsNothing_WhenEmptyString()
        {
            sut.Parse("")
                .Count()
                .Should()
                .Be(0);
        }

        [TestCase("abc", TestName = "one word without tags")]
        [TestCase("a b c", TestName = "string with spaces")]
        [TestCase("abc\n", TestName = "string with newline symbol")]
        public void Parse_ReturnsSingleTokenWithText_WhenNoTags(string text)
        {
            var expected = new PlainTextToken(text, null, 0, 0);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_ReturnsSeveralPlainTextTokens_WhenSeparatedByLines()
        {
            var text = "abc\ndef";
            var expected = new List<Token>()
            {
                new PlainTextToken("abc\n", null, 0, 0),
                new PlainTextToken("def", null, 1, 0)
            };
            sut.Parse(text).Should().BeEquivalentTo(expected);
        }

        [TestCase("_abc_", TestName = "one word with italic tags")]
        [TestCase("_a b c_", TestName = "string with spaces (with tags)")]
        public void Parse_ReturnsItalicToken(string text)
        {
            var expected = new ItalicToken(text, "_", 0, 0);
            AssertSingleToken(text, expected);
        }

        [TestCase("_abc", TestName = "no closing tag")]
        [TestCase("_abc _", TestName = "closing tag after space symbol")]
        [TestCase("_ abc_", TestName = "opening tag before space symbol")]
        [TestCase("_12_3", TestName = "tags inside number")]
        [TestCase("__", TestName = "empty string between tags")]
        [TestCase("a_bc cd_e", TestName = "selection in part of different words")]
        [TestCase("_abc __cd de_ fe__", TestName = "double tags intersection")]
        [TestCase("_ab__cd__e_", TestName = "bold inside italic tags")]
        public void Parse_ReturnSingleUnformattedToken_WhenTagUsingIsIncorrect(string text)
        {
            var expected = new PlainTextToken(text, null, 0, 0);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_ReturnsCorrectTokens_WhenItalicAndPlainText()
        {
            var text = "_abc_ de";
            var expected = new List<Token>()
            {
                new ItalicToken("_abc_", "_", 0, 0),
                new PlainTextToken(" de", null, 0, 5),
            };
            sut.Parse(text).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Parse_ReturnsItalicToken_WhenSelectionInsideWord()
        {
            var text = "_abc_de";
            var expected = new List<Token>()
            {
                new ItalicToken("_abc_", "_", 0, 0),
                new PlainTextToken("de", null, 0, 5),
            };
            sut.Parse(text).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Parse_ReturnsStrongToken()
        {
            var text = "__abc__";
            var expected = new StrongToken("__abc__", "__", 0, 0);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_ReturnsStrong_WhenItalicTagsInsideBold()
        {
            var text = "__ab_cd_e__";
            var expected = new List<Token>()
            {
                new StrongToken("__ab_cd_e__","__", 0, 0)
            };
            var result = sut.Parse(text);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Parse_ReturnsHeaderTag()
        {
            var text = "### abc";
            var expected = new HeaderToken("### abc", "###", 0, 0);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_ReturnsSeveralHeaderTag()
        {
            var text = "# abc\n# cde";
            var expected = new List<Token>()
            {
                new HeaderToken("# abc\n", "#", 0, 0),
                new HeaderToken("# cde", "#", 1, 0),
            };
            sut.Parse(text).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Parse_IdentifyNestedTags_WhenTagsInsideHeader()
        {
            var text = "# abc _cde_ __fg__";
            var expected = new List<Token>()
            {
                new HeaderToken("# abc _cde_ __fg__", "#",0, 0)
                {
                    SubTokens = new List<MarkdownToken>()
                    {
                        new PlainTextToken("abc ", null, 0, 0),
                        new ItalicToken("_cde_", "_", 0, 6),
                        new StrongToken("__fg__", "__", 0, 12)
                    }
                },
            };
            sut.Parse(text).Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Parse_ReturnsPlainText_WhenSelectorUnderScreening()
        {
            var text = @"\_abc_";
            var expected = new PlainTextToken(@"_abc_", null, 0, 1);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_ReturnsScreeningSymbol_WhenUnderScreening()
        {
            var text = @"\\";
            var expected = new PlainTextToken(@"\", null, 0, 1);
            AssertSingleToken(text, expected);
        }

        [Test]
        public void Parse_IdentifyScreeningAsPlainText_WhenNothingToScreen()
        {
            var text = @"ab\c";
            var expected = new PlainTextToken(@"ab\c", null, 0, 0);
            AssertSingleToken(text, expected);
        }

        private void AssertSingleToken(string text, Token expectedToken)
        {
            var actual = sut.Parse(text);
            actual.Single()
                .Should()
                .BeEquivalentTo(expectedToken);
        }
    }
}