using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SpExecuteSqlTransformer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ContextMenu TransformerContextMenu;

        public MainWindow()
        {
            InitializeComponent();
            TransformerContextMenu = new ContextMenu(Close);            
        }

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey([In] IntPtr hWnd,[In] int id,[In] uint fsModifiers,[In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey([In] IntPtr hWnd,[In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private const int VK_CONTROL = 0x11;        

        protected override void OnSourceInitialized(EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
            base.OnSourceInitialized(e);

            //setting to hidden must not happen in the ctor, but later (for example here), otherwise OnSourceInitialized is never called!
            Visibility = Visibility.Hidden; 
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            const uint MOD_CTRL = 0x0002;
            const uint MOD_ALT = 0x0001;
            const uint F_Key = 0x0046;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL | MOD_ALT, F_Key))
            {
                MessageBox.Show("Failed to register hotkey!");
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                const int WM_HOTKEY = 0x0312;
                switch (msg)
                {
                    case WM_HOTKEY:
                        var id = wParam.ToInt32();
                        switch (id)
                        {
                            case HOTKEY_ID:
                                OnHotKeyPressed();
                                handled = true;
                                break;
                        }
                        break;
                }
                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
                throw;
            }            
        }

        private void OnHotKeyPressed()
        {
            TransformerContextMenu.RunTransformation();
        }

        private void HandleUnexpectedException(Exception ex)
        {
            SpExecuteSqlTransformer.ContextMenu.HandleUnexpectedException(ex);
        }
    }
}
