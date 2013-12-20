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
using ORM = FederalProjectManager.ORM;
using DevExpress.Xpf.Grid;
using System.Windows.Markup;
using System.Globalization;
using FederalProjectManager.Model;
using DevExpress.Xpf.LayoutControl;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace FederalProjectManager.View
{
    public class PriorityConverter : MarkupExtension, IValueConverter
    {
        public PriorityConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            if (value is Int16)
            {
                switch ((EProjectPriority)(Int16)value)
                {
                    case EProjectPriority.VeryLow:
                        brush = new LinearGradientBrush(Color.FromRgb(0x4F, 0x9B, 0x6C), Color.FromRgb(0xAC, 0xC6, 0x7F), 0);
                        break;
                    case EProjectPriority.Low:
                        brush = new LinearGradientBrush(Color.FromRgb(0x0E, 0x9B, 0x43), Color.FromRgb(0x87, 0xBE, 0x29), 0);
                        break;
                    case EProjectPriority.Middle:
                        brush = new LinearGradientBrush(Color.FromRgb(0xFC, 0xCB, 0x01), Color.FromRgb(0xFC, 0xD9, 0x50), 0);
                        break;
                    case EProjectPriority.High:
                        brush = new LinearGradientBrush(Color.FromRgb(0xE8, 0x57, 0x21), Color.FromRgb(0xEC, 0x79, 0x1D), 0);
                        break;
                    case EProjectPriority.VeryHigh:
                        brush = new LinearGradientBrush(Color.FromRgb(0xE3, 0x1E, 0x25), Color.FromRgb(0xE3, 0x1E, 0x25), 0);
                        break;
                }
            };
            return brush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class StringCutConverter : MarkupExtension, IValueConverter
    {
        public StringCutConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var caption = (string)value;
            return (caption.Length > 39) ? String.Format("{0}...", caption.Substring(0, 36)) : caption;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class StringFormatConverter : MarkupExtension, IValueConverter
    {
        public StringFormatConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format(culture, (string)parameter, value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    /// <summary>
    /// Логика взаимодействия для ActiveProjects.xaml
    /// </summary>
    public partial class ActiveProjectsView : UserControl, INotifyPropertyChanged
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

        public class FocusedProjectChangedEventArgs: EventArgs
        {
            public int ProjectID { get; set; }            
        }
        public event EventHandler<FocusedProjectChangedEventArgs> FocusedProjectChanged;
        protected virtual void OnFocusedProjectChanged(FocusedProjectChangedEventArgs e)
        {
            if (FocusedProjectChanged != null)
            {
                FocusedProjectChanged(this, e);
            };
        }

        public event EventHandler ProjectDoubleClick;
        protected virtual void OnProjectDoubleClick()
        {
            if (ProjectDoubleClick != null)
            {
                ProjectDoubleClick(this, new EventArgs());
            }
        }

        private ObservableCollection<ORM.ProjectStatisticView> _projectStatistics;
        private ObservableCollection<ORM.ActualProjectView> _activeProjects;
        private string _focusedTailTag;

        public ObservableCollection<ORM.ProjectStatisticView> ProjectStatistics 
        {
            get { return _projectStatistics; }
            set
            {
                if (_projectStatistics != value)
                {
                    _projectStatistics = value;
                    OnPropertyChanged("ProjectStatistics");
                }
            }
        }
        public ObservableCollection<ORM.ActualProjectView> ActiveProjects
        {
            get { return _activeProjects; }
            set
            {
                if (_activeProjects != value)
                {
                    _activeProjects = value;
                    ((TableView)ActiveProjectsGrid.View).FocusedRowHandle = GridControl.InvalidRowHandle;
                    OnPropertyChanged("ActiveProjects");
                }
            }
        }
        public bool IsShowPrices
        {
            get
            {
                return UserSettings.ProfileRole == ERole.Administrator || UserSettings.ProfileRole == ERole.Accountant; 
            }
        }

        public ActiveProjectsView()
        {
            this.DataContext = this;
            _focusedTailTag = "Current";
            InitializeComponent();
            Refresh();
        }
        public void HighLightTile(int projectID)
        {
            using (var dc = new ORM.FPMDataContext())
            {
                if(dc.ActualProjectView.Any(x => x.ProjectID == projectID))
                {
                    var project = dc.ActualProjectView.Single(x => x.ProjectID == projectID);
                    var inTile = project.GetProjectStatisticsTile();
                    
                    var fadeIn = new DoubleAnimation()
                    {
                        From = 1,
                        To = 0.5,
                        AutoReverse = true,
                        Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                        RepeatBehavior = new RepeatBehavior(4)
                    };
                    var tile = (Tile)this.FindName(String.Format("{0}Tile", inTile.ToString()));
                    tile.BeginAnimation(Window.OpacityProperty, fadeIn);
                   // settings.BeginAnimation(Window.OpacityProperty, fadeIn);
                };
            };
        }
        public void Refresh()
        {
            Refresh(-1);
        }
        public void Refresh(int projectID)
        {
            OnPropertyChanged("IsShowPrices");
            using (var dc = new ORM.FPMDataContext())
            {
                ProjectStatistics = new ObservableCollection<ORM.ProjectStatisticView>(dc.ProjectStatisticView);
            };
            bindActiveProjectsData();
            if (projectID > 0)
            {
                HighLightTile(projectID);
            };
        }
        private void bindActiveProjectsData()
        {
            Func<ORM.ActualProjectView, bool> clause = null;
            var focusedTile = (EStatisticsTile)Enum.Parse(typeof(EStatisticsTile), _focusedTailTag);
            switch (focusedTile)
            {
                case EStatisticsTile.HighPriority:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsHighPriority);
                    break;
                case EStatisticsTile.Today:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsToday);
                    break;
                case EStatisticsTile.Current:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsCurrent);
                    break;
                case EStatisticsTile.Overstay:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsOverstay);
                    break;
                case EStatisticsTile.ProductionCompleted:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsProductionCompleted);
                    break; 
                case EStatisticsTile.Prepare:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsPrepare);
                    break;
                case EStatisticsTile.Archive:
                    clause = new Func<ORM.ActualProjectView, bool>(x => x.IsArchive);
                    break;
            };

            using (var dc = new ORM.FPMDataContext())
            {
                ActiveProjects = new ObservableCollection<ORM.ActualProjectView>(dc.ActualProjectView.Where(clause));
            };
        }       

        private void TableView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var row = e.NewRow as ORM.ActualProjectView;
            OnFocusedProjectChanged(new FocusedProjectChangedEventArgs()
            {
                ProjectID = (row != null) ? row.ProjectID : -1
            });
        }

        private void TileLayoutControl_TileClick(object sender, TileClickEventArgs e)
        {
            var ctrl = (TileLayoutControl)sender;
            foreach (var tile in ctrl.Children.OfType<Tile>())
            {
                if(tile != e.Tile)
                {
                    tile.SetResourceReference(BorderThicknessProperty, "NormalTileBorder");
                }
                else
                {
                    tile.SetResourceReference(BorderThicknessProperty, "FocusedTileBorder");
                };
            };
            _focusedTailTag = (string)e.Tile.Tag;
            bindActiveProjectsData();
        }

        private void TableView_RowDoubleClick(object sender, RowDoubleClickEventArgs e)
        {
            OnProjectDoubleClick();
        }


    }
}
