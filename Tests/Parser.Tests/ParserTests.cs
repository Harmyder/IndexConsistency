using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ParserLib;

namespace ParserTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Empty()
        {
            var entries = new Parser().CollectIndices("");
            Assert.AreEqual(0, entries.Length);
        }

        [TestMethod]
        public void CollectTop()
        {
            var entries = new Parser().CollectIndices("\\index{text}");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }

        [TestMethod]
        public void CollectTopWithSee()
        {
            var entries = new Parser().CollectIndices("\\index{text|see{else}}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("else", entries[1]);
        }

        [TestMethod]
        public void CollectSubWithSee()
        {
            var entries = new Parser().CollectIndices("\\index{text!subtext|see{else}}");
            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("subtext", entries[1]);
            Assert.AreEqual("else", entries[2]);
        }

        [TestMethod]
        public void CollectTopWithSeeSpaceFront()
        {
            var entries = new Parser().CollectIndices("\\index{text|see {else}}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("else", entries[1]);
        }

        [TestMethod]
        public void CollectTopWithSeeSpaceBack()
        {
            var entries = new Parser().CollectIndices("\\index{text|see{else} }");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("else", entries[1]);
        }

        [TestMethod]
        public void CollectTopWithSeeMulti()
        {
            var entries = new Parser().CollectIndices("\\index{text|see{else1, else2}}");
            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("else1", entries[1]);
            Assert.AreEqual("else2", entries[2]);
        }

        [TestMethod]
        public void CollectTopWithSeeAlso()
        {
            var entries = new Parser().CollectIndices("\\index{text|seealso{else}}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("else", entries[1]);
        }

        [TestMethod]
        public void CollectTopWithVisual()
        {
            var entries = new Parser().CollectIndices("\\index{text@text.visual}");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }


        [TestMethod]
        public void CollectTopWithPageRange()
        {
            var entries = new Parser().CollectIndices("\\index{text|(}");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }

        public void CollectTopWithPageStyle()
        {
            var entries = new Parser().CollectIndices("\\index{text|textit}");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }

        [TestMethod]
        public void CollectSub()
        {
            var entries = new Parser().CollectIndices("\\index{text!subtext}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("subtext", entries[1]);
        }

        [TestMethod]
        public void CollectSubWithVisual()
        {
            var entries = new Parser().CollectIndices("\\index{text!subtext@sub-text}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("subtext", entries[1]);
        }


        [TestMethod]
        public void CollectSubWithPageRange()
        {
            var entries = new Parser().CollectIndices("\\index{text!subtext|(}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("subtext", entries[1]);
        }

        [TestMethod]
        public void CollectSubSub()
        {
            var entries = new Parser().CollectIndices("\\index{text!subtext!subsubtext}");
            Assert.AreEqual(3, entries.Length);
            Assert.AreEqual("text", entries[0]);
            Assert.AreEqual("subtext", entries[1]);
            Assert.AreEqual("subsubtext", entries[2]);
        }

        [TestMethod]
        public void CollectTwoDifferent()
        {
            var entries = new Parser().CollectIndices("\\index{text1}\\index{text2}");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text1", entries[0]);
            Assert.AreEqual("text2", entries[1]);
        }

        [TestMethod]
        public void CollectTwoSame()
        {
            var entries = new Parser().CollectIndices("\\index{text}\\index{text}");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }

        [TestMethod]
        public void CollectSurrounded()
        {
            var entries = new Parser().CollectIndices("some text \\index{text} more text");
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual("text", entries[0]);
        }

        [TestMethod]
        public void CollectTwoSurrounded()
        {
            var entries = new Parser().CollectIndices("some text \\index{text1} more text \\index{text2} event more text");
            Assert.AreEqual(2, entries.Length);
            Assert.AreEqual("text1", entries[0]);
            Assert.AreEqual("text2", entries[1]);
        }

        [TestMethod]
        public void CollectDuplicate()
        {
            Assert.ThrowsException<ArgumentException>(() => new Parser().CollectIndices("\\index{text!text}"));
        }

        [TestMethod]
        public void Enumerate()
        {
            var textTemplate = "some text \\index{0}{{text1}} more text \\index{1}{{text2}} event more text";
            var text = string.Format(textTemplate, new[] { string.Empty, string.Empty });
            var expected = string.Format(textTemplate, new[] { "{0}", "{1}" });
            var entries = new Parser().CollectIndices(text);
            var actual = new Parser().EnumerateIndices(text, entries);
            Assert.AreEqual(expected, actual);
        }
    }
}
