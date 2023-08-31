using Klipboard.Utils;
using Test.Utils;

namespace Klipboard.Utils.Test
{
    [TestClass]
    public class TabularDataHelperTests
    {
        [TestMethod]
        public void AnalyzeTsvStringWithHeader()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme();
            var tableData = generator.GenerateTableString(lines: 10, addHeader: true, addNullRows: true, addEmptyRows: true);
            var res = TabularDataHelper.TryAnalyzeTabularData(tableData, "\t", out var scheme, out var firstRowIsHeader);
            
            Assert.IsTrue(res);
            Assert.IsTrue(firstRowIsHeader);
            Assert.AreEqual(tableScheme, scheme.ToString());
        }

        [TestMethod]
        public void AnalyzeTsvStringWithoutHeader()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme(firstRowIsHeader: false);
            var tableData = generator.GenerateTableString(lines: 10, addHeader: false);
            var res = TabularDataHelper.TryAnalyzeTabularData(tableData, "\t", out var scheme, out var firstRowIsHeader);

            Assert.IsTrue(res);
            Assert.IsFalse(firstRowIsHeader);
            Assert.AreEqual(tableScheme, scheme.ToString());
        }

        [TestMethod]
        public void AnalyzeTsvStringWithHeaderOnly()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme(firstRowIsHeader: true);
            var tableData = generator.GenerateTableString(lines: 0, addHeader: true);
            var res = TabularDataHelper.TryAnalyzeTabularData(tableData, "\t", out var scheme, out var firstRowIsHeader);

            Assert.IsTrue(res);
            Assert.IsFalse(firstRowIsHeader);
            Assert.AreNotEqual(tableScheme, scheme.ToString());
        }

        [TestMethod]
        public void AnalyzeTsvStringWithSingleLineNoHeader()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme(firstRowIsHeader: false);
            var tableData = generator.GenerateTableString(lines: 1, addHeader: false);
            var res = TabularDataHelper.TryAnalyzeTabularData(tableData, "\t", out var scheme, out var firstRowIsHeader);

            Assert.IsTrue(res);
            Assert.IsFalse(firstRowIsHeader);
            Assert.AreEqual(tableScheme, scheme.ToString());
        }

        [TestMethod]
        public void AnalyzeCsvStream()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme();
            var tableData = generator.GenerateTableStream(lines: 10, addHeader: true);
            var res = TabularDataHelper.TryAnalyzeTabularData(tableData, ",", out var scheme, out var firstRowIsHeader);

            Assert.IsTrue(res);
            Assert.IsTrue(firstRowIsHeader);
            Assert.AreEqual(tableScheme, scheme.ToString());
        }

        [TestMethod]
        public void ConvertTsvStringToInlineQuery()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme();
            var tableData = generator.GenerateTableString(lines: 100, addHeader: true, addNullRows: false, addEmptyRows: false);
            var res = TabularDataHelper.TryConvertTableToInlineQueryText(tableData, "\t", out string inlineQuery);

            Assert.IsTrue(res);
            Assert.IsNotNull(inlineQuery);
        }

        [TestMethod]
        public void ConvertTsvStringToInlineQueryLink()
        {
            var generator = new TableGenerator(autoGenerateScheme: true);
            var tableScheme = generator.GenerateTableScheme();
            var tableData = generator.GenerateTableString(lines: 200, addHeader: true, addNullRows: false, addEmptyRows: false);
            var res = TabularDataHelper.TryConvertTableToInlineQueryGzipBase64(tableData, "\t", out string? inlineQueryLink);

            Assert.IsTrue(res);
            Assert.IsNotNull(inlineQueryLink);

            var inputLength = tableData.Length;
            var outputLength = inlineQueryLink.Length;
            Assert.IsTrue(inputLength > outputLength);
        }

        #region Consider if this is needed
        public static readonly string s_testData =
@"col 1	Col 2	Col 3
a	b	c
1	2	3
x	y	z

";

        [TestMethod]
        public void GivenValidTsv_WhenDetectFormat_ResultIsTab()
        {
            Assert.IsTrue(TabularDataHelper.TryDetectTabularTextFormat(s_testData, out var separator));
            Assert.AreEqual('\t', separator);
        }

        [TestMethod]
        public void GivenInvalidTsv_WhenDetectFormat_ResultIsTab()
        {
            var data = s_testData.Replace("z", "z\t");

            Assert.IsFalse(TabularDataHelper.TryDetectTabularTextFormat(data, out var separator));
        }

        [TestMethod]
        public void GivenValidCsv_WhenDetectFormat_ResultIsComa()
        {
            var data = s_testData.Replace('\t', ',');
            Assert.IsTrue(TabularDataHelper.TryDetectTabularTextFormat(data, out var separator));
            Assert.AreEqual(',', separator);
        }
        #endregion
    }
}
