using CompanionCore;
using System.Text.RegularExpressions;

namespace CompanionCoreTests
{
    [TestClass]
    public class FileHelperTests
    {
        [DataTestMethod]
        [DataRow("x.json", "json", false)]
        [DataRow("x.json.zip", "json", true)]
        [DataRow("x.json.gz", "json", true)]
        [DataRow("x.y.json.zip", "json", true)]
        [DataRow("file://a/b/c/x.json", "json", false)]
        [DataRow("C:\\\\dir\\drive\\x.json.zip", "json", true)]
        [DataRow("https://host/resource/x.json.gz", "json", true)]
        [DataRow("/mnt/home/b/c/x.y.json.zip", "json", true)]
        public void GivenValidFileNamesOrPaths_WhenGetFileType_DetectionSucceeds(string fileNameOrPath, string expectedType, bool expectedCompressed)
        {
            Assert.IsTrue(FileHelper.TryGetFileExtensionFromPath(fileNameOrPath, out var type, out var compressed));
            Assert.IsTrue(type.Equals(expectedType, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(compressed == expectedCompressed);
        }

        [DataTestMethod]
        [DataRow("x")]
        [DataRow("x.zip")]
        [DataRow("file://a/b/c/x")]
        [DataRow("C:\\\\dir\\drive\\json.zip")]
        [DataRow("https://host/resource/x.gz")]
        [DataRow("/mnt/home/b/c/x.zip")]
        public void GivenInvalidFileNamesOrPaths_WhenGetFileType_DetectionFails(string fileNameOrPath)
        {
            Assert.IsFalse(FileHelper.TryGetFileExtensionFromPath(fileNameOrPath, out var type, out var compressed));
        }

        [DataTestMethod]
        [DataRow("file_19851206_name.csv", ".*_(?<YYYY>[0-9]{4})(?<MM>[0-9]{2})(?<DD>[0-9]{2})_.*", 1985, 12, 6)]
        public void GivenFileNameAndMacther_WhenExtractRegexTime_CreationTimeReturns(string filename, string regexStr, int exYear, int exMon, int exDay)
        {
            var regex = new Regex(regexStr);
            var expectedTime = new DateTime(exYear, exMon, exDay);

            Assert.IsTrue(FileHelper.TryGetFileCreationTimeFromName(filename, regex, out var creationTime));
            Assert.AreEqual(expectedTime, creationTime);
        }
    }
}