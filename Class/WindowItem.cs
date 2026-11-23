using ojo3Utility.Common;
using ojo3Utility.Pictures;
using System;
using System.Drawing;
using System.Xml.Serialization;

namespace dotViewer
{
    [Serializable]
    public class WindowItem
    {
        public string FilePath { get; set; } = string.Empty;
        public ImageOrientation Orientation { get; set; }
        public float ImageRatio { get; set; } = 1.0f;
        public ImageFilterItem ImageFilterItem { get; set; } = new ImageFilterItem();
        public Size Size { get; set; } = new Size(256, 256);
        public Point Location { get; set; } = Point.Empty;
        public bool TopMost { get; set; } = true;
        public bool ShowInTaskbar { get; set; }
        public Rank Rank { get; set; } = Rank.Rank3;
        public bool Bookmark { get; set; }
        public bool KeepRatio { get; set; } = true;

        [XmlIgnore]
        public ImageItem ImageItem { get; set; }

        public WindowItem() { }

        public WindowItem(string filePath,
                          ImageOrientation orientation,
                          float imageRatio,
                          ImageFilterItem imageFilterItem,
                          Size size,
                          Point location,
                          bool topMost) : this()
        {
            FilePath = filePath;
            Orientation = orientation;
            ImageRatio = imageRatio;
            ImageFilterItem = imageFilterItem;
            Size = size;
            Location = location;
            TopMost = topMost;
        }

        public override string ToString() => this.View(true, true);
    }
}
