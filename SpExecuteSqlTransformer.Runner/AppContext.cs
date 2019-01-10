using log4net;
using log4net.Config;
using Ninject;
using SpExecuteSqlTransformer.Core;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SpExecuteSqlTransformer.Runner
{
    public class AppContext : ApplicationContext
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private NotifyIcon NotifyIcon { get; set; }
        private MenuItem AutomaticallyTransformMenuItem { get; set; }
        private MenuItem CloseMenuItem { get; set; }
        private ClipboardMonitor ClipboardMonitor { get; set; }
        private bool AutomaticallyTransform { get; set; }
        private static bool CurrentlySettingClipboardText { get; set; }
        private string LastClipboardTextBeforeTransformation { get; set; }

        public AppContext()
        {
            InitLogging();

            AutomaticallyTransform = true;
            SetupNotifyIcon();
            SetupClipboardMonitor();
        }

        private void SetupClipboardMonitor()
        {
            ClipboardMonitor = new ClipboardMonitor();
            ClipboardMonitor.ClipboardChanged += OnClipboardChanged;
        }

        private void SetupNotifyIcon()
        {
            AutomaticallyTransformMenuItem = new MenuItem("Transform automatically", DoAutomaticallyTransformClicked) { Checked = true };
            CloseMenuItem = new MenuItem("Close", CloseMenuItemClicked);

            NotifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.T,
                ContextMenu = new ContextMenu(new[] { AutomaticallyTransformMenuItem, CloseMenuItem }),
                Visible = true
            };
        }

        private void OnClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            log.Debug("Clipboard contents changed");
            
            if (!AutomaticallyTransform)
            {
                log.Debug("Automatic transformation is off");
                return;
            }

            Transform();
        }

        private void Transform()
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

                log.Info($"Clipboard text starts with '{execSpExecuteSql}'. Trying to transform string.");
                var result = GetTransformationManager().TransformSqlString(trimmedText);

                if (result.ResultString == trimmedText || result.ResultString == (trimmedText + Environment.NewLine))
                {
                    log.Debug("No transformation happened. Clipboard is not touched.");
                }
                else
                {
                    log.Info("String was transformed. Setting clipboard text.");
                    CurrentlySettingClipboardText = true;
                    Clipboard.SetText(result.ResultString);
                    ShowTransformedNotification();
                }              
            }
            catch (Exception ex)
            {
                log.Error("Unexpected Error", ex);
                MessageBox.Show($"{ex.GetType()}\r\nMessage: {ex.Message}");
            }
            finally
            {
                CurrentlySettingClipboardText = false;
            }
        }

        private void UpdateContextMenu()
        {
            //TODO
        }

        private void ShowTransformedNotification()
        {
            NotifyIcon.ShowBalloonTip(5000, "Transformed", "Transformed clipboard text!", ToolTipIcon.Info);            
        }

        private void CloseMenuItemClicked(object sender, EventArgs e)
        {
            NotifyIcon.Visible = false;
            ClipboardMonitor.Dispose();
            Application.Exit();
        }

        private void DoAutomaticallyTransformClicked(object sender, EventArgs e)
        {
            AutomaticallyTransformMenuItem.Checked = !AutomaticallyTransformMenuItem.Checked;
            AutomaticallyTransform = AutomaticallyTransformMenuItem.Checked;
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

