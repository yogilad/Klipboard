using Klipboard.Utils;
using System.Text.RegularExpressions;

namespace Klipboard.Utils.Test
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
        [DataRow("file_06-Jan-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 1, 6)]
        [DataRow("file_06-feb-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 2, 6)]
        [DataRow("file_06-MAR-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 3, 6)]
        [DataRow("file_06-aPr-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 4, 6)]
        [DataRow("file_06-may-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 5, 6)]
        [DataRow("file_06-JUN-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 6, 6)]
        [DataRow("file_06-juL-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 7, 6)]
        [DataRow("file_06-aug-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 8, 6)]
        [DataRow("file_06-SEP-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 9, 6)]
        [DataRow("file_06-OCT-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 10, 6)]
        [DataRow("file_06-Nov-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 11, 6)]
        [DataRow("file_06-DEC-85_name.csv", ".*_(?<DD>[0-9]{2})-(?<MON>[A-Za-z]{3})-(?<YY>[0-9]{2})_.*", 1985, 12, 6)]
        [DataRow("file_19851206_name.csv", ".*_(?<YYYY>[0-9]{4})(?<MM>[0-9]{2})(?<DD>[0-9]{2})_.*", 1985, 12, 6)]
        [DataRow("file_851206_name.csv", ".*_(?<YY>[0-9]{2})(?<MM>[0-9]{2})(?<DD>[0-9]{2})_.*", 1985, 12, 6)]
        [DataRow("file_502675222_name.csv", ".*_(?<EPOC_SEC>[0-9]+)_.*", 1985, 12, 6)]
        [DataRow("file_502675200123_name.csv", ".*_(?<EPOC_MSEC>[0-9]+)_.*", 1985, 12, 6)]
        public void GivenFileNameAndMacther_WhenExtractRegexTime_CreationTimeReturns(string filename, string regexStr, int exYear, int exMon, int exDay)
        {
            var regex = new Regex(regexStr);
            var expectedTime = new DateTime(exYear, exMon, exDay, 0, 0, 0, DateTimeKind.Utc);

            Assert.IsTrue(FileHelper.TryGetFileCreationTimeFromName(filename, regex, out var creationTime));
            Assert.AreEqual(expectedTime, creationTime);
        }
    }
}