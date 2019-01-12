using System;

namespace SpExecuteSqlTransformer.Model
{
    public interface IClipboardMonitor : IDisposable
    {
        event EventHandler ClipboardChanged;
    }
}