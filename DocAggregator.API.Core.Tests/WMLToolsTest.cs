using Moq;
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
    }
}
