using NINA.Plugin.TargetScheduler.Database;
using NUnit.Framework;
using System.IO;

namespace NINA.Plugin.TargetScheduler.Test.Database {

    [TestFixture]
    public class GenerateTestDatabase {
        private SchedulerDatabaseInteraction db;

        [SetUp]
        public void SetUp() {
            db = GetDatabase();
        }

        [Test]
        [Ignore("tbd")]
        public void Test1() {
        }

        private SchedulerDatabaseInteraction GetDatabase() {
            var testDbPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"schedulerdb.sqlite");
            //TestContext.WriteLine($"DB PATH: {testDbPath}");
            return new SchedulerDatabaseInteraction(string.Format(@"Data Source={0};", testDbPath));
        }
    }
}