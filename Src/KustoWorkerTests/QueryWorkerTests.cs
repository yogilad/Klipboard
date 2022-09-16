using System.Diagnostics;
using CompanionCore;
using KustoWorker;

namespace KustoWorkerTests
{
    [TestClass]
    public class QueryWorkerTests
    {
        static QueryWorker? s_queryWorker;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            var appConfig = AppConfig.TestAppConfig();
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