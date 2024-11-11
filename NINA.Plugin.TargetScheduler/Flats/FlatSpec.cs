using NINA.Core.Model.Equipment;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;

namespace NINA.Plugin.TargetScheduler.Flats {

    public class FlatSpec : IEquatable<FlatSpec> {
        public int TargetId { get; private set; }
        public string FilterName { get; private set; }
        public int Gain { get; private set; }
        public int Offset { get; private set; }
        public BinningMode BinningMode { get; private set; }
        public int ReadoutMode { get; private set; }
        public double Rotation { get; private set; }
        public double ROI { get; private set; }
        public string Key { get; private set; }

        public FlatSpec(int targetId, string filterName, int gain, int offset, BinningMode binning, int readoutMode, double rotation, double roi) {
            TargetId = targetId;
            FilterName = filterName;
            Gain = gain;
            Offset = offset;
            BinningMode = binning;
            ReadoutMode = readoutMode;
            Rotation = RotationZero(rotation);
            ROI = roi;
            Key = GetKey();
        }

        public FlatSpec(int targetId, AcquiredImage exposure) {
            TargetId = targetId;
            FilterName = exposure.FilterName;
            Gain = exposure.Metadata.Gain;
            Offset = exposure.Metadata.Offset;
            BinningMode bin;
            BinningMode.TryParse(exposure.Metadata.Binning, out bin);
            BinningMode = bin;
            ReadoutMode = exposure.Metadata.ReadoutMode;
            Rotation = RotationZero(exposure.Metadata.RotatorMechanicalPosition);
            ROI = exposure.Metadata.ROI;
            Key = GetKey();
        }

        public FlatSpec(FlatHistory flatHistory) {
            TargetId = flatHistory.TargetId;
            FilterName = flatHistory.FilterName;
            Gain = flatHistory.Gain;
            Offset = flatHistory.Offset;
            BinningMode = flatHistory.BinningMode;
            ReadoutMode = flatHistory.ReadoutMode;
            Rotation = RotationZero(flatHistory.Rotation);
            ROI = flatHistory.ROI;
            Key = GetKey();
        }

        private string GetKey() {
            return $"{FilterName}_{Gain}_{Offset}_{BinningMode}_{ReadoutMode}_{ROI}";
        }

        private double RotationZero(double rotation) {
            if (rotation == ImageMetadata.NO_ROTATOR_ANGLE || Double.IsNaN(rotation)) {
                return 0.0;
            }

            return rotation;
        }

        public bool Equals(FlatSpec other) {
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            if (GetType() != other.GetType()) { return false; }

            // If rotation isn't a factor for these targets OR it's the same target, then just compare on key
            if (Rotation == 0.0 && other.Rotation == 0.0) { return Key == other.Key; }
            if (TargetId == other.TargetId) { return Key == other.Key; }

            // Targets are different - if rotation is different then we're not the same
            if (Rotation != other.Rotation) { return false; }

            return Key == other.Key;
        }

        public override string ToString() {
            string rot = Rotation != ImageMetadata.NO_ROTATOR_ANGLE ? Rotation.ToString() : "n/a";
            return $"tid:{TargetId} filter:{FilterName} gain:{Gain} offset:{Offset} bin:{BinningMode} readout:{ReadoutMode} rot:{rot} roi: {ROI}";
        }
    }
}