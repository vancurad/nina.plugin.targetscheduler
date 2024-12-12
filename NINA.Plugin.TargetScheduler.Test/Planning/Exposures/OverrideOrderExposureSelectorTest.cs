using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Exposures {

    [TestFixture]
    public class OverrideOrderExposureSelectorTest {

        [Test]
        public void testOverrideOrderExposureSelector() {
            // At least for now, OverrideOrderExposureSelector leverages BasicExposureSelector since both
            // are based on a generated filter cadence - so no need for another test.
        }
    }
}