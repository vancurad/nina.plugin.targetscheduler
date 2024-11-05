using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;
using System;
using System.IO;

namespace NINA.Plugin.TargetScheduler.Test.Database {

    [TestFixture]
    public class SchedulerDatabaseTest {
        private const string profileId = "01234567-abcd-9876-gfed-0123456abcde";
        private static DateTime markDate = DateTime.Now.Date;

        private string testDatabasePath;
        private SchedulerDatabaseInteraction db;

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            testDatabasePath = Path.Combine(Path.GetTempPath(), $"scheduler-unittest.sqlite");
            if (File.Exists(testDatabasePath)) {
                File.Delete(testDatabasePath);
            }

            TestContext.WriteLine($"TEST DB: {testDatabasePath}");
            db = new SchedulerDatabaseInteraction(string.Format(@"Data Source={0};", testDatabasePath));
            Assert.That(db, Is.Not.Null);
            LoadTestDatabase();
        }

        [Test, Order(1)]
        [NonParallelizable]
        public void TestLoad() {
            using (var context = db.GetContext()) {
            }
        }

        private void LoadTestDatabase() {
            using (var context = db.GetContext()) {
                ProfilePreference pp = new ProfilePreference(profileId);
                context.ProfilePreferenceSet.Add(pp);
                context.SaveChanges();
            }
        }
    }
}