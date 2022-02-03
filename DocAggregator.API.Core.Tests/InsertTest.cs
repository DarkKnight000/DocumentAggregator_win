using Xunit;

namespace DocAggregator.API.Core.Tests
{
    public class InsertTest
    {
        [Theory]
        [InlineData("", InsertKind.PlainText, "", InsertKind.PlainText, true)]
        [InlineData("10", InsertKind.PlainText, "10", InsertKind.PlainText, true)]
        [InlineData("fa", InsertKind.PlainText, "fa", InsertKind.PlainText, true)]
        [InlineData("fa", InsertKind.CheckMark, "fa", InsertKind.CheckMark, true)]
        [InlineData("", InsertKind.PlainText, "10", InsertKind.PlainText, false)]
        [InlineData("10", InsertKind.PlainText, "fa", InsertKind.PlainText, false)]
        [InlineData("flag", InsertKind.PlainText, "flag", InsertKind.CheckMark, false)]
        public void Insert_ObjectEquals(string maskA, InsertKind kindA, string maskB, InsertKind kindB, bool expected)
        {
            Insert one = new Insert(maskA, kindA);
            Insert another = new Insert(maskB, kindB);

            bool actual = one.Equals(another);

            Assert.Equal(expected, actual);

            actual = one.GetHashCode() == another.GetHashCode();

            Assert.Equal(expected, actual);
        }
    }
}
