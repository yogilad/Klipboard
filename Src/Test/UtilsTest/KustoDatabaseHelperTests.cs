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
    public class KustoDatabaseHelperTests
    {
        private KustoDatabaseHelper m_kustoHelper;

        public KustoDatabaseHelperTests()
        {
            var appConfig = AppConfigFile.CreateDebugConfig().ConfigureAwait(false).ResultEx();
            var kcsb = new KustoConnectionStringBuilder(appConfig.DefaultClusterConnectionString).WithAadUserPromptAuthentication();
            m_kustoHelper = new KustoDatabaseHelper(kcsb, appConfig.DefaultClusterDatabaseName);
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

        [TestMethod]
        public void CreateTable()
        {
            var dt = DateTime.Now;
            var tableName = $"RegularTable_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}";
            var res = m_kustoHelper.TryCreateTableAync(tableName, "(Line:string)").ConfigureAwait(false).ResultEx();

            Assert.IsTrue(res.Success);
            Assert.IsTrue(string.IsNullOrWhiteSpace(res.Error));
        }

        [TestMethod]
        public void CreateTableWithShortBatching()
        {
            var dt = DateTime.Now;
            var tableName = $"BatchingTable_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}";
            var res = m_kustoHelper.TryCreateTableAync(tableName, "(Line:string)", ingestionBatchingTimeSeconds: 30).ConfigureAwait(false).ResultEx();

            Assert.IsTrue(res.Success);
            Assert.IsTrue(string.IsNullOrWhiteSpace(res.Error));
        }

        [TestMethod]
        public void CreateTableWithShortBatchingAndExpiray()
        {
            var dt = DateTime.Now;
            var tableName = $"TempTable_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}";
            var res = m_kustoHelper.TryCreateTableAync(tableName, "(Line:string)", ingestionBatchingTimeSeconds: 30, tableLifetimeDays:2).ConfigureAwait(false).ResultEx();

            Assert.IsTrue(res.Success);
            Assert.IsTrue(string.IsNullOrWhiteSpace(res.Error));
        }
    }
}
