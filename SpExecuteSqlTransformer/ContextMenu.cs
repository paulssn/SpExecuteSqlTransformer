using log4net;
using log4net.Config;
using SpExecuteSqlTransformer.Model;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SpExecuteSqlTransformer
{
    public class ContextMenu : IContextMenu
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private NotifyIcon NotifyIcon { get; set; }
        private MenuItem AutomaticallyTransformMenuItem { get; set; }
        private MenuItem RunTransformationMenuItem { get; set; }
        private MenuItem CloseMenuItem { get; set; }
        private MenuItem RestoreClipboardTextMenuItem { get; set; }
        private IClipboardMonitor ClipboardMonitor { get; set; }

        private ContextMenuViewModel ViewModel { get; set; }
        private Action CloseAction { get; set; }

        public ContextMenu(Action closeAction)
        {
            InitLogging();
            CloseAction = closeAction;

            ViewModel = new ContextMenuViewModel(new ClipboardWrapper(), this);
            SetupClipboardMonitor();
            SetupNotifyIconAndContextMenu();
            UpdateContextMenu();
        }

        private void SetupClipboardMonitor()
        {
            ClipboardMonitor = new ClipboardMonitor();
            ClipboardMonitor.ClipboardChanged += OnClipboardChanged;
        }

        public void ShowInfoNotification(int duration, string title, string text)
        {
            NotifyIcon.ShowBalloonTip(duration, title, text, ToolTipIcon.Info);
        }

        public void UpdateContextMenu()
        {
            RestoreClipboardTextMenuItem.Enabled = ViewModel.LastClipboardTextBeforeTransformation != null;
            AutomaticallyTransformMenuItem.Checked = ViewModel.AutomaticallyTransform;
        }

        private void SetupNotifyIconAndContextMenu()
        {
            AutomaticallyTransformMenuItem = new MenuItem("Transform automatically", AutomaticallyTransformClicked);
            CloseMenuItem = new MenuItem("Close", CloseMenuItemClicked);
            RestoreClipboardTextMenuItem = new MenuItem("Restore clipboard text", RestoreClipboardTextClicked);
            RunTransformationMenuItem = new MenuItem("Run transformation", RunTransformationClicked);

            NotifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.T,
                ContextMenu = new System.Windows.Forms.ContextMenu(
                    new[] { RunTransformationMenuItem, AutomaticallyTransformMenuItem, RestoreClipboardTextMenuItem, CloseMenuItem }),
                Visible = true
            };
        }

        private void CloseMenuItemClicked(object sender, EventArgs e)
        {
            try
            {
                NotifyIcon.Visible = false;
                ClipboardMonitor.Dispose();
                ViewModel.Close();
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
            finally
            {
                UpdateContextMenu();
            }
        }

        private void RestoreClipboardTextClicked(object sender, EventArgs e)
        {
            try
            {
                ViewModel.RestoreClipboardText();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
            finally
            {
                UpdateContextMenu();
            }
        }

        private void AutomaticallyTransformClicked(object sender, EventArgs e)
        {
            try
            {
                ViewModel.AutomaticallyTransformClicked();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
            finally
            {
                UpdateContextMenu();
            }
        }

        private void OnClipboardChanged(object sender, EventArgs eventArgs)
        {
            try
            {
                ViewModel.OnClipboardChanged();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
            finally
            {
                UpdateContextMenu();
            }
        }

        private void RunTransformationClicked(object sender, EventArgs e)
        {
            try
            {
                RunTransformation();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
            finally
            {
                UpdateContextMenu();
            }
        }

        public void RunTransformation()
        {
            ViewModel.RunManualTransformation();
        }

        public static void HandleUnexpectedException(Exception ex)
        {
            log.Error("Unexpected Error", ex);
            MessageBox.Show($"{ex.GetType()}\r\nMessage: {ex.Message}",
                "Error",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error);
        }

        private void InitLogging()
        {
            var location = AppDomain.CurrentDomain.BaseDirectory;
            var log4netConfigFilePath = Path.Combine(location, "log4net.config");
            var log4netConfigFile = new FileInfo(log4netConfigFilePath);
            if (!log4netConfigFile.Exists)
                throw new InvalidOperationException($"{log4netConfigFile.FullName} does not exist");
            XmlConfigurator.ConfigureAndWatch(log4netConfigFile);
        }

        private class ClipboardWrapper : IClipboard
        {
            public bool ContainsText() => Clipboard.ContainsText();

            public string GetText() => Clipboard.GetText();

            public void SetText(string text) => Clipboard.SetText(text);
        }
    }
}
