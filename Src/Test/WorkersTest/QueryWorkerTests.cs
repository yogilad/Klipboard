using System.Diagnostics;
using Klipboard.Utils;
using Klipboard.Workers;
using Kusto.Cloud.Platform.Utils;

namespace WorkersTest
{
    [TestClass]
    public class QueryWorkerTests
    {
        static QueryWorker? s_queryWorker;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var appConfig = AppConfigFile.CreateDebugConfig().ConfigureAwait(false).ResultEx();
            var serviceManager = new ServiceManager(appConfig);

            s_queryWorker = new QueryWorker(serviceManager.GetAllServices().First());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            s_queryWorker = null;
        }

        [TestMethod]
        public void WhenGetServiceDatabases_ThenDatabsesListReturns()
        {
            var dbs = s_queryWorker?.ListServiceDatabasesAsync().Result;

            Debug.Assert(dbs.Contains("MyDatabase"));
        }
    }
}