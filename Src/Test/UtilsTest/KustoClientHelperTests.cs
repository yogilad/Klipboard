using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data;
using NuGet.Frameworks;

namespace Klipboard.Utils.Test
{
    [TestClass]
    public class KustoClientHelperTests
    {
        private KustoClientHelper m_kustoHelper;

        public KustoClientHelperTests()
        {
            var appConfig = AppConfigFile.CreateDebugConfig().ConfigureAwait(false).ResultEx();
            var kcsb = new KustoConnectionStringBuilder(appConfig.DefaultClusterConnectionString).WithAadUserPromptAuthentication();
            m_kustoHelper = new KustoClientHelper(kcsb, appConfig.DefaultClusterDatabaseName);
        }

        [TestMethod]
        public void GivenFile_WhenUploadToEngineStore_BlobIsCreated()
        {
            var res = TryUploadFileToBlob(out var blobUri, out var errorMsg);

            Assert.IsTrue(res);
            Assert.IsFalse(string.IsNullOrWhiteSpace(blobUri));
            Assert.IsTrue(string.IsNullOrWhiteSpace(errorMsg));
        }

        [TestMethod]
        public void GivenBlob_WhenDetectScheme_SchemeIsCorrect()
        {
            var res = TryUploadFileToBlob(out var blobUri, out var errorMsg);

            Assert.IsTrue(res);

            var schemeRes = m_kustoHelper.TryGetBlobSchemeAsync(blobUri, "csv", firstRowIsHeader: true).ConfigureAwait(false).ResultEx();
            Assert.IsTrue(schemeRes.Success);
            Assert.IsNotNull(schemeRes.TableScheme);
            Assert.IsNull(schemeRes.Error);
            Assert.IsTrue(schemeRes.TableScheme.Columns.Count > 0);
        }

        private bool TryUploadFileToBlob(out string? blobUri, out string? errorMsg)
        {
            var dt = DateTime.Now;
            var upsteramFileName = $"TestFile_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}.csv";
            var dataStream = new FileStream("C:\\Users\\yocha\\Desktop\\Klipboard Test Data\\snp.csv", FileMode.Open, FileAccess.Read);
            var res = m_kustoHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upsteramFileName).ConfigureAwait(false).ResultEx();

            blobUri = res.BlobUri;
            errorMsg = res.Error;
            return res.Success;
        }
    }
}
