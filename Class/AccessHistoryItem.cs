using System;
using System.Drawing;
using System.IO;

namespace dotViewer
{
    public partial class MultiWindow
    {
        private class AccessHistoryItem : IDisposable
        {
            public string FilePath { get; set; }
            public string Text { get; set; }
            public DateTime DateTime { get; set; } = DateTime.Now;
            public Bitmap Image { get; set; }

            public AccessHistoryItem(string path, Bitmap image)
            {
                FilePath = path;
                Text = Path.GetFileName(path);
                Image = image;
            }

            public void Dispose()
            {
                Image?.Dispose();
                Image = null;
            }

            public override string ToString() => $"{DateTime}) {FilePath}";
        }
    }
}
