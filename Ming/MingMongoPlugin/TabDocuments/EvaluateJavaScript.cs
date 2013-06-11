using MingMongoPlugin.TabDocuments.UserControls;
using MingPluginInterfaces;
using MongoDB.Driver;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace MingMongoPlugin.TabDocuments
{
    internal class EvaluateJavaScript : ITabDocument
    {
        public BusyStateChangedEventHandler BusyStateChanged;

        private readonly EvalJSControl _control;
        private readonly ConnectionInfo _cnn;
        private readonly string _databaseName;

        private int _instanceId;

        public int InstanceId
        {
            set
            {
                _instanceId = value;
            }
        }

        public EvaluateJavaScript(ConnectionInfo cnn, string database)
        {
            _cnn = cnn;
            _databaseName = database;

            _control = new EvalJSControl();
            _control.EvaluateClicked += EvaluateClicked;
            _control.PreviewKeyUp += PreviewKeyUp;
        }

        void PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                EvaluateClicked();
            }
        }

        public void EvaluateClicked()
        {
            var script = _control.JavaScript.Text;
            if (BusyStateChanged != null) BusyStateChanged(true, Properties.Resources.EvaluateJS_Evaluating);
            var task = new CancelableTask(() => DoEvaluate(script), null);
            task.Execute();
        }

        private void DoEvaluate(string script)
        {
            string output;
            try
            {
                var result = MongoUtilities.Create(_cnn).GetDatabase(_databaseName).Eval(script);
                output = MongoUtilities.PrettyPrintJson(result.ToString());
            }
            catch (MongoCommandException ex)
            {
                output = MongoUtilities.PrettyPrintJson(ex.CommandResult.Response.ToString());
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }
            _control.Dispatcher.Invoke(new Action(
                () => _control.Output.Text = output));
            if (BusyStateChanged != null) BusyStateChanged(false, null);
        }

        public UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return string.Format(Properties.Resources.EvaluateJS_Title, _databaseName); }
        }

        public string Description
        {
            get { return string.Format(Properties.Resources.EvaluateJS_Description, _cnn.Host, _cnn.Port, _databaseName); }
        }

        public bool CloseRequested()
        {
            return true;
        }

        public void MenuItemClicked(string menuKey)
        {
            throw new NotImplementedException();
        }
    }
}
