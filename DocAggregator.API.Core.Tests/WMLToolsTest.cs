﻿using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class WMLToolsTest
    {
        [Fact]
        public void WMLTools_FindInserts_InEmptyDocument()
        {
            var WMLDocument = new XDocument();

            var controls = Wml.WordprocessingMLTools.FindInserts(WMLDocument);

            Assert.Empty(controls);
        }

        [Theory]
        [XmlData("..\\..\\..\\XmlData\\SingleEmptyTextField.xml")]
        public void WMLTools_FindInserts_FindPlainTextField(string input)
        {
            var WMLDocument = XDocument.Parse(input);

            var controls = Wml.WordprocessingMLTools.FindInserts(WMLDocument);

            var expectedCount = 1;
            var actualCount = controls.Count();
            Assert.Equal(expectedCount, actualCount);

            var insert = controls.First();

            var expectedKind = InsertKind.PlainText;
            var actualKind = insert.Kind;
            Assert.Equal(expectedKind, actualKind);

            var sdtText = insert.AssociatedChunk as XElement;
            Assert.NotNull(sdtText);
            Assert.Equal(Wml.W.sdt, sdtText.Name);
            Assert.Contains(sdtText.Descendants(), e => e.Name == Wml.W.text);
        }

        [Theory]
        [XmlData("..\\..\\..\\XmlData\\SingleEmptyCheckBox.xml")]
        public void WMLTools_FindInserts_FindCheckBoxField(string input)
        {
            var WMLDocument = XDocument.Parse(input);

            var controls = Wml.WordprocessingMLTools.FindInserts(WMLDocument);

            var expectedCount = 1;
            var actualCount = controls.Count();
            Assert.Equal(expectedCount, actualCount);

            var insert = controls.First();

            var expectedKind = InsertKind.CheckMark;
            var actualKind = insert.Kind;
            Assert.Equal(expectedKind, actualKind);

            var sdtText = insert.AssociatedChunk as XElement;
            Assert.NotNull(sdtText);
            Assert.Equal(Wml.W.sdt, sdtText.Name);
            Assert.Contains(sdtText.Descendants(), e => e.Name == Wml.W14.checkbox);
        }

        [Theory]
        [XmlData("..\\..\\..\\XmlData\\SingleEmptyTextField.xml")]
        public void WMLTools_SetInserts_SetPlainTextField(string input)
        {
            var expected = "Test";
            var WMLDocument = XDocument.Parse(input);
            var textInsert = Wml.WordprocessingMLTools.FindInserts(WMLDocument).First();
            textInsert.ReplacedText = expected;

            Wml.WordprocessingMLTools.SetInserts(textInsert);

            var inserts = Wml.WordprocessingMLTools.FindInserts(WMLDocument);
            Assert.Empty(inserts);

            var contentRoot = textInsert.AssociatedChunk as XElement;
            Assert.Equal(Wml.W.p, contentRoot.Name);
            Assert.Null(contentRoot.Element(Wml.W.r).Element(Wml.W.rPr).Element(Wml.W.rStyle));

            var exactContent = contentRoot.Element(Wml.W.r).Element(Wml.W.t).Value;
            Assert.Equal(expected, exactContent);
        }

        [Theory]
        [XmlData("..\\..\\..\\XmlData\\SingleEmptyCheckBox.xml")]
        public void WMLTools_SetInserts_SetCheckBoxField(string input)
        {
            var expected = "☐";
            var WMLDocument = XDocument.Parse(input);
            var checkInsert = Wml.WordprocessingMLTools.FindInserts(WMLDocument).First();
            checkInsert.ReplacedCheckmark = false;

            Wml.WordprocessingMLTools.SetInserts(checkInsert);

            var inserts = Wml.WordprocessingMLTools.FindInserts(WMLDocument);
            Assert.Empty(inserts);

            var contentRoot = checkInsert.AssociatedChunk as XElement;
            Assert.Equal(Wml.W.p, contentRoot.Name);

            var exactContent = contentRoot.Element(Wml.W.r).Element(Wml.W.t).Value;
            Assert.Equal(expected, exactContent);
        }

        [Theory]
        [XmlData("..\\..\\..\\XmlData\\TableEmptyTextField.xml")]
        public void WMLTools_SetInserts_SetPlainTextFieldInTable(string input)
        {
            var expected = "Test";
            var WMLDocument = XDocument.Parse(input);
            var textInsert = Wml.WordprocessingMLTools.FindInserts(WMLDocument).First();
            textInsert.ReplacedText = expected;

            Wml.WordprocessingMLTools.SetInserts(textInsert);

            var inserts = Wml.WordprocessingMLTools.FindInserts(WMLDocument);
            Assert.Empty(inserts);

            var contentRoot = textInsert.AssociatedChunk as XElement;
            Assert.Equal(Wml.W.tc, contentRoot.Name);

            var exactContent = contentRoot.Element(Wml.W.p).Element(Wml.W.r).Element(Wml.W.t).Value;
            Assert.Equal(expected, exactContent);
        }
    }
}