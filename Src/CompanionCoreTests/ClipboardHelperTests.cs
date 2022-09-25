using CompanionCore;

namespace CompanionCoreTests
{
    [TestClass]
    public class ClipboardHelperTests
    {
        public static readonly string s_testData =
@"col 1	Col 2	Col 3
a	b	c
1	2	3
x	y	z

";

        [TestMethod]
        public void GivenValidTsv_WhenDetectFormat_ResultIsTab()
        {
            Assert.IsTrue(ClipboardHelper.TryDetectTabularTextFormat(s_testData, out var separator));
            Assert.AreEqual('\t', separator);
        }

        [TestMethod]
        public void GivenInvalidTsv_WhenDetectFormat_ResultIsTab()
        {
            var data = s_testData.Replace("z", "z\t");

            Assert.IsFalse(ClipboardHelper.TryDetectTabularTextFormat(data, out var separator));
        }

        [TestMethod]
        public void GivenValidCsv_WhenDetectFormat_ResultIsComa()
        {
            var data = s_testData.Replace('\t', ',');
            Assert.IsTrue(ClipboardHelper.TryDetectTabularTextFormat(data, out var separator));
            Assert.AreEqual(',', separator);
        }

    }
}
