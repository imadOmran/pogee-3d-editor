using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ApplicationCore
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressWindow : UserControl
    {
        private Action<BackgroundWorker> _workToDo = null;
        private BackgroundWorker _worker = new BackgroundWorker();

        public ProgressWindow(Action<BackgroundWorker> work)
        {
            InitializeComponent();            
            _workToDo = work;            
            
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            _worker.RunWorkerAsync();            
        }

        public ProgressWindow()
        {

        }

        public string Text
        {
            get { return this.InfoText.Content.ToString(); }
            set
            {
                InfoText.Content = value;
            }
        }

        public Action<BackgroundWorker> WorkToDo
        {
            get { return _workToDo; }
            set
            {
                _workToDo = value;
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke( (Action)(() =>
                {
                    ProgressBar.Value = e.ProgressPercentage;
                    PercentComplete.Content = e.ProgressPercentage.ToString();
                    if (e.ProgressPercentage == 100)
                    {
                        InfoText.Content = "Completed";
                    }
                }));
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker != null && _workToDo != null)
            {
                _workToDo.Invoke(worker);
            }
        }
    }
}
