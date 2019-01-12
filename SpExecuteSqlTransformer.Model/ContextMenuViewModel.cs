using log4net;
using Ninject;
using SpExecuteSqlTransformer.Core;
using System;
using System.Reflection;

namespace SpExecuteSqlTransformer.Model
{
    public class ContextMenuViewModel
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string execSpExecuteSql = "exec sp_executesql";

        private static bool CurrentlySettingClipboardText { get; set; }

        private IClipboard Clipboard { get; set; }
        private IContextMenu ContextMenu { get; set; }
        public bool AutomaticallyTransform { get; private set; }
        public string LastClipboardTextBeforeTransformation { get; private set; }
        private readonly object _lockObject;

        public ContextMenuViewModel(IClipboard clipboard, IContextMenu contextMenu)
        {
            Clipboard = clipboard;
            ContextMenu = contextMenu;
    
            _lockObject = new object();
            AutomaticallyTransform = true;
        }

        public void OnClipboardChanged()
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
        
        public void RunAutomaticTransformation()
        {
            if (!Clipboard.ContainsText())
            {
                log.Debug("Clipboard does not contain text.");
                return;
            }

            var originalText = Clipboard.GetText();
            LastClipboardTextBeforeTransformation = originalText;
            var trimmedText = originalText.Trim();
            if (!trimmedText.ToLower().StartsWith(execSpExecuteSql))
            {
                log.Debug($"Clipboard text doesn't seem to be a query to transform as it doesn't start with '{execSpExecuteSql}'");
                return;
            }

            log.Info($"Clipboard text starts with '{execSpExecuteSql}'. Trying to transform string.");
            Transform(trimmedText);
        }

        public void RunManualTransformation()
        {
            if (!Clipboard.ContainsText())
            {
                log.Debug("Clipboard does not contain text.");
                return;
            }

            LastClipboardTextBeforeTransformation = Clipboard.GetText();
            Transform(LastClipboardTextBeforeTransformation);
        }

        private void Transform(string textToTransform)
        {
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

        public void RestoreClipboardText()
        {
            if (LastClipboardTextBeforeTransformation == null)
                throw new InvalidOperationException($"{nameof(LastClipboardTextBeforeTransformation)} is null");

            SetClipboardText(LastClipboardTextBeforeTransformation);

            LastClipboardTextBeforeTransformation = null;

            ShowRestoredNotification();
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

        public void Close()
        {
            //nothing to do so far
        }

        public void AutomaticallyTransformClicked()
        {
            AutomaticallyTransform = !AutomaticallyTransform;
        }

        private void ShowTransformedNotification()
        {
            ContextMenu.ShowInfoNotification(5000, "Transformed", "Transformed clipboard text!");
        }

        private void ShowRestoredNotification()
        {
            ContextMenu.ShowInfoNotification(5000, "Restored", "Clipboard text restored!");
        }

        private static ITransformationManager GetTransformationManager()
        {
            return new StandardKernel(new IoCModule()).Get<ITransformationManager>();
        }
    }
}
