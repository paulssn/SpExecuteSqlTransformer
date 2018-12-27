using log4net;
using log4net.Config;
using Ninject;
using SpExecuteSqlTransformer.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace SpExecuteSqlTransformer.Runner.WinForms
{
    public partial class MainForm : Form
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //we want no window to be visible
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Format();
        }

        private void Format()
        {
            try
            {
                InitLogging();

                log.Info("SpExecuteSqlTransformer.Runner started");

                if (Clipboard.ContainsText())
                {
                    log.Debug("Clipboard contains text. Trying to transform string.");
                    var inputText = Clipboard.GetText();
                    var result = GetTransformationManager().TransformSqlString(inputText);

                    if (result.ResultString == inputText || result.ResultString == (inputText + Environment.NewLine))
                    {
                        log.Info("No transformation happened. Clipboard is not touched.");
                    }
                    else
                    {
                        log.Debug("String was transformed. Setting clipboard text.");
                        Clipboard.SetText(result.ResultString);
                    }
                }
                else
                {
                    log.Info("Clipboard does not contain text.");
                }

                //TODO show notification
            }
            catch (Exception ex)
            {
                log.Error("Unexpected Error", ex);
                MessageBox.Show($"{ex.GetType()}\r\nMessage: {ex.Message}");
            }

            Application.Exit();
        }

        private static ITransformationManager GetTransformationManager()
        {
            return new StandardKernel(new IoCModule()).Get<ITransformationManager>();
        }

        private static void InitLogging()
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
