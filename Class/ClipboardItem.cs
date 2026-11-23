using ojo3Utility.Common;
using System;

namespace dotViewer
{
    public class ClipboardItem<T>
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        public T Data { get; set; }
        public ClipboardItem(T data)
        {
            Data = data;
        }
        public override string ToString() => this.View();
    }
}
