using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data;

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
            var dt = DateTime.Now;
            var upsteramFileName = $"TestFile_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}"; 
            var res = m_kustoHelper.TryUploadFileToEngineStagingArea("C:\\Users\\yocha\\Desktop\\Klipboard Test Data\\snp.csv", upsteramFileName, out var blobUri, out var errorMsg);

            Assert.IsTrue(res);
            Assert.IsFalse(string.IsNullOrWhiteSpace(blobUri));
            Assert.IsTrue(string.IsNullOrWhiteSpace(errorMsg));
        }
    }
}
