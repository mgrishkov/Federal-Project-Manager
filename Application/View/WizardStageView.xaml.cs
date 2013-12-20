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
using FederalProjectManager.ORM;
using FederalProjectManager.Model;

using DevExpress.Xpf.LayoutControl;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Globalization;
using System.ComponentModel;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.Native;
using SmartClasses;
using DevExpress.Xpf.Editors;

namespace FederalProjectManager.View
{
    
    public class IsEditabeToIconConverter : MarkupExtension, IValueConverter
    {
        public IsEditabeToIconConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = new BitmapImage();
            var state = (bool)value;
            if (value is bool && !(bool)value)
            {
                img = Application.Current.FindResource("LockIcon") as BitmapImage;
            }
            return img;
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
    public class StageStateToIconConverter : MarkupExtension, IValueConverter
    {
        public StageStateToIconConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = new BitmapImage();
            var state = (EStageState)System.Convert.ToInt32(value);
            switch (state)
            {
                case EStageState.Completed:
                    img = Application.Current.FindResource("CheckIcon") as BitmapImage;
                    break;
                case EStageState.Skipped:
                    img = Application.Current.FindResource("ArrowRightIcon") as BitmapImage;
                    break;
                case EStageState.Canceled:
                    img = Application.Current.FindResource("CancelIcon") as BitmapImage;
                    break;
                case EStageState.Procssing:
                    img = Application.Current.FindResource("GearIcon") as BitmapImage;
                    break;
                case EStageState.Configurating:
                    img = Application.Current.FindResource("ToolIcon") as BitmapImage;
                    break;
            };

            return img;
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
    public class AllowRemoveStageConverter : MarkupExtension, IValueConverter
    {
        public AllowRemoveStageConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            var stage = value as ProjectStageTile;
            if (stage != null)
            {
                result = stage.ID >= 0;
            };
            return result;
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
    public class TileBackgroundConverter : MarkupExtension, IValueConverter
    {
        public TileBackgroundConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string styleName = ((bool)value) ? "FocusedTile" : "NormalTile";
            var style = Application.Current.FindResource(styleName);
            return style;
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
    public class StageCnangingEventArgs : EventArgs
    {
        public bool Cancel = false;
        //public StageTile OldStage;
        //public StageTile NewStage;
    }
    public partial class WizardStageView : UserControl, INotifyPropertyChanged
    {
        public event EventHandler StageChanged;
        protected virtual void OnStageChanged()
        {
            if (StageChanged != null)
            {
                StageChanged(this, new EventArgs());
            }
        }
        public event CancelEventHandler StageChanging;
        protected virtual void OnStageChanging(CancelEventArgs e)
        {
            if (StageChanging != null)
            {
                StageChanging(this, e);
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler StagePropertyWasChanged;
        protected virtual void OnStagePropertyWasChanged()
        {
            if (StagePropertyWasChanged != null)
            {
                StagePropertyWasChanged(this, new EventArgs());
            };
        }

        private ORM.ProjectInfoView _project;
        private ProjectStageTile _focusedStage;
        private bool isLoaded = false;
        private FPMDataContext _dc;
        private ORM.Project _originalProject;

        private ObservableCollection<ProjectStageTile> _stages;
        public ProjectStageTile FocusedStage 
        { 
            get { return _focusedStage; }
            set
            {
                if (_focusedStage != value)
                {
                    Stages.Where(x => x.IsFocused).ToList().ForEach(x => x.IsFocused = false);
                    if (Stages.Any(x => x.SortIndex == value.SortIndex))
                    {
                        _focusedStage = Stages.Single(x => x.SortIndex == value.SortIndex);
                    }
                    else
                    {
                        _focusedStage = Stages.OrderByDescending(x => x.SortIndex).First();
                    };
                    _focusedStage.IsFocused = true;
                    OnPropertyChanged("FocusedStage");
                    OnPropertyChanged("IsRemoveStageEnabled");
                    OnStageChanged();
                }
            }
        }
        public bool IsAddStageEnabled
        {
            get { return _project != null && _project.IsEditable; }
        }
        public bool IsRemoveStageEnabled
        {
            get { return IsAddStageEnabled && FocusedStage != null && FocusedStage.ID >= 0 && FocusedStage.IsEditable; }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<KeyValuePair<int, string>> StageTemplates
        {
            get
            {
                return EStageTemplate.Shipment.GetKeyDescriptionList();
            }
        }
        public ObservableCollection<ProjectStageTile> Stages
        {
            get { return _stages; }
            set
            {
                if (value != _stages)
                {
                    _stages = value;
                    OnPropertyChanged("Stages");
                };
            }
        }

        void itm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (String.Compare(e.PropertyName, "IsFocused") != 0
                && String.Compare(e.PropertyName, "IsValid") != 0)
            {
                OnStagePropertyWasChanged();
            };
        }
        public bool IsStageOrderChanged { get; set; }

        public WizardStageView()
        {
            
            InitializeComponent();
            this.DataContext = this;
            IsStageOrderChanged = false;
            _project = null;
            _stages = new ObservableCollection<ProjectStageTile>();
            _stages.CollectionChanged += _stages_CollectionChanged;
            
        }

        void _stages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (isLoaded)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                    || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    OnStagePropertyWasChanged();
                    int sortIndex = 0;
                    foreach (var itm in _stages)
                    {
                        if (itm.SortIndex != sortIndex)
                        {
                            itm.SortIndex = sortIndex;
                            if (!IsStageOrderChanged)
                            {
                                IsStageOrderChanged = true;
                            }
                        };
                        sortIndex++;
                    };
                };
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var itm in e.NewItems.Cast<ProjectStageTile>())
                {
                    itm.PropertyChanged +=itm_PropertyChanged;
                }
            };
        }
        public void Load(ORM.ProjectInfoView project)
        {
            if (_dc != null)
            {
                _dc.Dispose();
            };
            _dc = new FPMDataContext();
            Stages.Clear();
            var mainStage = new ProjectStageTile()
            {
                ID = -1,
                Name = "Основные данные",
                SortIndex = 0,
                IsPublic = true,
                RowState = 0,
            };
            Stages.Add(mainStage);
            FocusedStage = mainStage;

            _project = project;
            _originalProject = project.ToProject(); 

            if (_project.ID > 0)
            {
                var stages = _dc.ProjectStage
                    .Where(x => x.RowState == Convert.ToInt16(ERowState.Active)
                                && x.ProjectID == _project.ID
                                && (x.IsPublic || (UserSettings.ProfileRole == ERole.Administrator) || x.ResponsibleRole == Convert.ToInt16(UserSettings.ProfileRole)))
                    .OrderBy(x => x.SortIndex);

                foreach (var stage in stages)
                {
                    Stages.Add(new ProjectStageTile(stage));
                };
            }
            else
            {
                Stages.Add(new ProjectStageTile(EStageTemplate.Sketch, 1));
                Stages.Add(new ProjectStageTile(EStageTemplate.Contract, 2));
                Stages.Add(new ProjectStageTile(EStageTemplate.Payment, 3));
                Stages.Add(new ProjectStageTile(EStageTemplate.Shipment, 4));
            };
            
            Stages.Where(x => x.Project == null).ToList().ForEach(x => x.Project = _originalProject);
            
            OnStageChanged();
            isLoaded = true;
        }

        private void Tile_Click(object sender, TileClickEventArgs e)
        {
            var eventResult = new CancelEventArgs();
            OnStageChanging(eventResult);
            if (!eventResult.Cancel)
            {
                var data = e.Tile.DataContext as ProjectStageTile;
                if (data != null)
                {
                    FocusedStage = data;
                };
            };
        }

        private void AddStage_ItemClick(object sender, ItemClickEventArgs e)
        {
            var eventResult = new CancelEventArgs();
            OnStageChanging(eventResult);
            if (!eventResult.Cancel)
            {
                int nextSortIndex = _stages.Select(x => x.SortIndex).Max() ?? 0;
                var newStage = new ProjectStageTile(_originalProject, ++nextSortIndex);
                _stages.Add(newStage);
                FocusedStage = newStage;
            };
        }

        private void TemplatesListBox_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (Stages != null && e.NewValue != null)
            {
                var sortIndex = Stages.Max(x => x.SortIndex) ?? 1;
                var template = (KeyValuePair<int,string>)e.NewValue;
                var newStage = new ProjectStageTile((EStageTemplate)template.Key, ++sortIndex);
                newStage.Project = _originalProject;
                Stages.Add(newStage);
                FocusedStage = newStage;
                StageTemplatesPopup.ClosePopup();
            }
        }

        private void StageTemplatesPopup_Closed(object sender, EventArgs e)
        {
            StageTemplatesListBox.SelectedItem = null;
        }

        private void DeleteStage_ItemClick(object sender, ItemClickEventArgs e)
        {
            var eventResult = new CancelEventArgs();
            OnStageChanging(eventResult);
            if (!eventResult.Cancel)
            {
                var stage = _stages.Where(x => x.IsFocused).First();
                if (stage.ID >= 0)
                {
                    var previousStage = _stages.Where(x => x.SortIndex < stage.SortIndex).OrderByDescending(x => x.SortIndex).First();
                    _stages.Remove(stage);
                    FocusedStage = previousStage;
                };
            };
        }

    }
}
