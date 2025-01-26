using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NINA.Plugin.TargetScheduler.Database.Schema {

    public class ImageData {
        [Key] public int Id { get; set; }
        public string tag { get; set; }
        public byte[] imagedata { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        [ForeignKey("AcquiredImage")] public int AcquiredImageId { get; set; }
        public virtual AcquiredImage AcquiredImage { get; set; }

        [NotMapped]
        public string Tag {
            get => tag; set { tag = value; }
        }

        [NotMapped]
        public byte[] Data {
            get => imagedata; set { imagedata = value; }
        }

        [NotMapped]
        public int Width { get => width; set { width = value; } }

        [NotMapped]
        public int Height { get => height; set { height = value; } }

        public ImageData() {
        }

        public ImageData(string tag, byte[] data, int acquiredImageId, int width, int height) {
            Tag = tag;
            Data = data;
            AcquiredImageId = acquiredImageId;
            Width = width;
            Height = height;
        }
    }
}