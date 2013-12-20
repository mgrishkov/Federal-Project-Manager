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
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using System.Windows.Media.Animation;
using System.ComponentModel;
using DevExpress.Xpf.Core.ServerMode;
using Microsoft.Win32;
using DevExpress.Xpf.Grid;
using SmartClasses;


namespace FederalProjectManager.Windows
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window, INotifyPropertyChanged
    {
        #region implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private DateTime _dateFrom;
        private DateTime _dateTo;
        private LinqInstantFeedbackDataSource _ds;
        private List<ORM.Dictionary> _projectTypes;
        private List<KeyValuePair<short, string>> _projectPriorities;
        private List<KeyValuePair<int, string>> _projectStates;

        public DateTime DateFrom
        {
            get { return _dateFrom;}
            set
            {
                if (_dateFrom != value)
                {
                    _dateFrom = value;
                    OnPropertyChanged("DateFrom");
                    refresh();
                }
            }
        }
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                if (_dateTo != value)
                {
                    _dateTo = value;
                    OnPropertyChanged("DateTo");
                    refresh();
                }
            }
        }
        public LinqInstantFeedbackDataSource DataSource
        {
            get { return _ds; }
        }
        public List<ORM.Dictionary> ProjectTypes
        {
            get
            {
                if (_projectTypes == null)
                {
                    using (var dc = new ORM.FPMDataContext())
                    {
                        _projectTypes = dc.Dictionary.Where(x => x.DictionaryNumber == 1).ToList();
                    };
                }
                return _projectTypes;
            }
        }
        public List<KeyValuePair<short, string>> ProjectPriorities
        {
            get
            {
                if (_projectPriorities == null)
                {
                    _projectPriorities = Model.EProjectPriority.High.GetInt16KeyDescriptionList();
                }
                return _projectPriorities;
            }
        }
        public List<KeyValuePair<int, string>> ProjectStates
        {
            get
            {
                if (_projectStates == null)
                {
                    _projectStates = Model.EProjectState.Completed.GetKeyDescriptionList();
                }
                return _projectStates;
            }
        }

        public TimeSpan AnimationDuration { get; private set; }

        public StatisticsWindow()
        {
            var dt = DateTime.Now.Date;
            AnimationDuration = new TimeSpan(0, 0, 0, 0, 500);
            DateFrom = new DateTime(dt.Year, dt.Month, 1);
            DateTo = dt;
            InitializeComponent();
            this.DataContext = this;
            initDataSource();
        }
        private void initDataSource()
        {
            _ds = new LinqInstantFeedbackDataSource();
            var dc = new ORM.FPMDataContext();
            dc.ObjectTrackingEnabled = false;
            _ds.QueryableSource = dc.ProjectPivotView.Where(x => x.DeadLine >= DateFrom && x.DeadLine <= DateTo.AddDays(1).AddMinutes(-1));
            _ds.KeyExpression = "ProjectID";
            _ds.DefaultSorting = "DeadLine DESC";
            OnPropertyChanged("DataSource");
        }
        
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            closeWindow();
        }

        private void closeWindow()
        {
            var fadeOut = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = new Duration(AnimationDuration),
                AutoReverse = false
            };
            fadeOut.Completed += fadeOut_Completed;
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
        private void fadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExportButton_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "xlsx",
                Filter = "Файлы MS Excel 2007-2013|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                var table = mainGrid.View as TableView;
                table.ExportToXlsx(dlg.FileName);
            };
        }

        private void RefreshButton_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            refresh();
        }
        private void refresh()
        {
            if (_ds != null)
            {
                _ds.Refresh();
            };
        }

        private void mainGrid_EndGrouping(object sender, RoutedEventArgs e)
        {
            
        }

    }
}
