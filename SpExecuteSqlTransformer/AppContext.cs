using log4net;
using log4net.Config;
using Ninject;
using SpExecuteSqlTransformer.Core;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SpExecuteSqlTransformer
{
    public class AppContext : ApplicationContext
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool CurrentlySettingClipboardText { get; set; }

        private NotifyIcon NotifyIcon { get; set; }
        private MenuItem AutomaticallyTransformMenuItem { get; set; }
        private MenuItem CloseMenuItem { get; set; }
        private MenuItem RestoreClipboardTextMenuItem { get; set; }

        private ClipboardMonitor ClipboardMonitor { get; set; }
        private bool AutomaticallyTransform { get; set; }
        private string LastClipboardTextBeforeTransformation { get; set; }
        private readonly object _lockObject;

        public AppContext()
        {
            InitLogging();

            _lockObject = new object();

            AutomaticallyTransform = true;
            SetupNotifyIconAndContextMenu();
            SetupClipboardMonitor();

            UpdateContextMenu();
        }

        private void SetupClipboardMonitor()
        {
            ClipboardMonitor = new ClipboardMonitor();
            ClipboardMonitor.ClipboardChanged += OnClipboardChanged;
        }

        private void SetupNotifyIconAndContextMenu()
        {
            AutomaticallyTransformMenuItem = new MenuItem("Transform automatically", AutomaticallyTransformClicked);
            CloseMenuItem = new MenuItem("Close", CloseMenuItemClicked);
            RestoreClipboardTextMenuItem = new MenuItem("Restore clipboard text", RestoreClipboardTextClicked);

            NotifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.T,
                ContextMenu = new ContextMenu(new[] { AutomaticallyTransformMenuItem, RestoreClipboardTextMenuItem, CloseMenuItem }),
                Visible = true
            };
        }

        private void RunAutomaticTransformation()
        {
            const string execSpExecuteSql = "exec sp_executesql";
            try
            {
                if (!Clipboard.ContainsText())
                {
                    log.Debug("Clipboard does not contain text.");
                    return;
                }

                var originalText = Clipboard.GetText();
                LastClipboardTextBeforeTransformation = originalText;
                var trimmedText = originalText.Trim();
                if (!trimmedText.StartsWith(execSpExecuteSql))
                {
                    log.Debug($"Clipboard text doesn't seem to be a query to transform as it doesn't start with '{execSpExecuteSql}'");
                    return;
                }

                Transform(execSpExecuteSql, trimmedText);
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

        private void Transform(string execSpExecuteSql, string textToTransform)
        {
            log.Info($"Clipboard text starts with '{execSpExecuteSql}'. Trying to transform string.");
            var result = GetTransformationManager().TransformSqlString(textToTransform);

            if (result.ResultString == textToTransform || result.ResultString == (textToTransform + Environment.NewLine))
            {
                log.Debug("No transformation happened. Clipboard is not touched.");
            }
            else
            {
                log.Info("String was transformed. Setting clipboard text.");

                SetClipboardText(result.ResultString);
                ShowTransformedNotification();
            }
        }

        private void UpdateContextMenu()
        {
            RestoreClipboardTextMenuItem.Enabled = LastClipboardTextBeforeTransformation != null;
            AutomaticallyTransformMenuItem.Checked = AutomaticallyTransform;
        }

        private void ShowTransformedNotification()
        {
            NotifyIcon.ShowBalloonTip(5000, "Transformed", "Transformed clipboard text!", ToolTipIcon.Info);
        }

        private void ShowRestoredNotification()
        {
            NotifyIcon.ShowBalloonTip(5000, "Restored", "Clipboard text restored!", ToolTipIcon.Info);
        }

        private void RestoreClipboardText()
        {
            if (LastClipboardTextBeforeTransformation == null)
                throw new InvalidOperationException($"{nameof(LastClipboardTextBeforeTransformation)} is null");

            SetClipboardText(LastClipboardTextBeforeTransformation);

            LastClipboardTextBeforeTransformation = null;

            ShowRestoredNotification();

            UpdateContextMenu();
        }

        private void SetClipboardText(string text)
        {
            lock (_lockObject)
            {
                CurrentlySettingClipboardText = true;
                Clipboard.SetText(text);
                CurrentlySettingClipboardText = false;
            }
        }

        private void OnClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            try
            {
                log.Debug("Clipboard contents changed");

                if (CurrentlySettingClipboardText)
                {
                    log.Debug("Currently setting clipboard text. No transformation.");
                    return;
                }

                if (!AutomaticallyTransform)
                {
                    log.Debug("Automatic transformation is off. No transformation.");
                    return;
                }

                RunAutomaticTransformation();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
        }

        private void CloseMenuItemClicked(object sender, EventArgs e)
        {
            try
            {
                NotifyIcon.Visible = false;
                ClipboardMonitor.Dispose();
                Application.Exit();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
        }

        private void RestoreClipboardTextClicked(object sender, EventArgs e)
        {
            try
            {
                RestoreClipboardText();
            }
            catch (Exception ex)
            {
                HandleUnexpectedException(ex);
            }
        }

        private void AutomaticallyTransformClicked(object sender, EventArgs e)
        {
            AutomaticallyTransform = !AutomaticallyTransform;
            UpdateContextMenu();
        }

        private static void HandleUnexpectedException(Exception ex)
        {
            log.Error("Unexpected Error", ex);
            MessageBox.Show($"{ex.GetType()}\r\nMessage: {ex.Message}");
        }

        private static ITransformationManager GetTransformationManager()
        {
            return new StandardKernel(new IoCModule()).Get<ITransformationManager>();
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
    }
}

