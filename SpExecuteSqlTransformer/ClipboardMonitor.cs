using log4net;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpExecuteSqlTransformer
{
    /// <summary>
    /// Basically taken from https://stackoverflow.com/questions/621577/clipboard-event-c-sharp and slightly modified 
    /// </summary>
    [DefaultEvent("ClipboardChanged")]
    public class ClipboardMonitor : Control
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IntPtr nextClipboardViewer;

        public ClipboardMonitor()
        {
            BackColor = Color.Red;
            Visible = false;

            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)Handle);
        }

        /// <summary>
        /// Clipboard contents changed.
        /// </summary>
        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        protected override void Dispose(bool disposing)
        {
            if (nextClipboardViewer != IntPtr.Zero)
            {
                ChangeClipboardChain(Handle, nextClipboardViewer);
                nextClipboardViewer = IntPtr.Zero;
            }
            base.Dispose(disposing);
        }

        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref Message m)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    OnClipboardChanged();
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void OnClipboardChanged()
        {
            try
            {
                IDataObject iData = Clipboard.GetDataObject();
                ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(iData));
            }
            catch (Exception e)
            {
                log.Error("Unexpected error", e);
            }
        }
    }

    public class ClipboardChangedEventArgs : EventArgs
    {
        public readonly IDataObject DataObject;

        public ClipboardChangedEventArgs(IDataObject dataObject)
        {
            DataObject = dataObject;
        }
    }
}
