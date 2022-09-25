using CompanionCore;

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
            Assert.IsTrue(FileHelper.TryGetFileTypeFromPath(fileNameOrPath, out var type, out var compressed));
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
            Assert.IsFalse(FileHelper.TryGetFileTypeFromPath(fileNameOrPath, out var type, out var compressed));
        }
    }
}