using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
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
using System.Windows.Markup;
using System.Globalization;
using FederalProjectManager.Model;
using SmartClasses;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace FederalProjectManager.View
{

    public class ParameterIDtoNameConverter : MarkupExtension, IValueConverter
    {
        public ParameterIDtoNameConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = String.Empty;
            int parameterID = -1;
            if (Int32.TryParse(value.ToString(), out parameterID) && parameterID > 0)
            {
                name = ParameterRepository.GetParameterName(parameterID);
            };
            return name;
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

    public class NumerToEnumConverter : MarkupExtension, IMultiValueConverter
    {
        public NumerToEnumConverter()
        {
        }
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = String.Empty;
            try
            {
                int valueID = -1;
                int parameterID = -1;
                if (value[0] != null && value[1] != null)
                {
                    if (Int32.TryParse(value[0].ToString(), out valueID) && Int32.TryParse(value[1].ToString(), out parameterID) && valueID > 0 && parameterID > 0)
                    {
                        name = ParameterRepository.GetParameterValue(parameterID, valueID);
                    };
                };
            }
            catch { };
            return name;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    public class ParameterTypeConverter : MarkupExtension, IValueConverter
    {
        public ParameterTypeConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterID = -1;
            var visible = Visibility.Collapsed;
            if(value != null && Int32.TryParse(value.ToString(), out parameterID) && parameterID > 0)
            {
                var valueType = ParameterRepository.GetParameterValueType(parameterID);
                switch((string)parameter)
                {
                    case "Number":
                        visible = (valueType == EParameterValueType.Int32 || valueType == EParameterValueType.Decimal) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "String":
                        visible = (valueType == EParameterValueType.String) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "DateTime":
                        visible = (valueType == EParameterValueType.DateTIme) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "Path":
                        visible = (valueType == EParameterValueType.Path) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "Enum":
                        visible = (valueType == EParameterValueType.Enum) ? Visibility.Visible : Visibility.Collapsed;
                        break;
                };
            };
            return visible;
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
    public class NumericMaskConverter : MarkupExtension, IValueConverter
    {
        public NumericMaskConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mask = String.Empty;
            var parameterID = -1;
            if(value != null && Int32.TryParse(value.ToString(), out parameterID) && parameterID > 0)
            {
                var valueType = ParameterRepository.GetParameterValueType(parameterID);
                switch (valueType)
                {
                    case EParameterValueType.Int32:
                        mask = @"n0";
                        break;
                    case EParameterValueType.Decimal:
                        mask = @"n";
                        break;
                };
            }
            return mask;
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
    public class ParameterValuesConverter : MarkupExtension, IValueConverter
    {
        public ParameterValuesConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = new List<ParameterValue>();
            var parameterID = -1;
            if (value != null && Int32.TryParse(value.ToString(), out parameterID) && parameterID > 0)
            {
                list = ParameterRepository.GetParameterValues(parameterID).ToList();
            };
            return list;
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
    /// Логика взаимодействия для StageView.xaml
    /// </summary>
    public partial class StageInfoView : UserControl, INotifyPropertyChanged
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

        public event EventHandler StageParameterWasChanged;
        protected virtual void OnStageParameterWasChanged()
        {
            if (StageParameterWasChanged != null)
            {
                StageParameterWasChanged(this, new EventArgs());
            };
        }

        private ProjectStage _stage;
        private StageParameter _focusedParameter;


        public ProjectStage Stage
        {
            get { return _stage; }
            set
            {
                if (_stage != value)
                {
                    _stage = value;
                    _stage.StageParameter.ListChanged += StageParameter_ListChanged;
                    foreach (var itm in _stage.StageParameter)
                    {
                        itm.PropertyChanged += itm_PropertyChanged;
                    };
                    OnPropertyChanged("Stage");
                };
            }
        }

        private void itm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnStageParameterWasChanged();
        }
        public bool AllowDeleteParameter
        {
            get { return FocusedParameter != null && Stage.IsEditable; }
        }

        void StageParameter_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded
                || e.ListChangedType == ListChangedType.ItemDeleted)
            {
                var list = (EntitySet<StageParameter>)sender;
                switch(e.ListChangedType)
                {
                    case ListChangedType.ItemAdded:                
                        list[e.NewIndex].PropertyChanged += itm_PropertyChanged;
                        break;
                };
            };
        }
        public StageParameter FocusedParameter
        {
            get { return _focusedParameter; }
            set
            {
                if(_focusedParameter !=  value)
                {
                    _focusedParameter = value;
                    OnPropertyChanged("FocusedParameter");
                    OnPropertyChanged("AllowDeleteParameter");
                };
            }
        }
        
        public StageInfoView()
        {
            this.DataContext = this;
            InitializeComponent();
            StageLookup.ItemsSource = EStageState.Canceled.GetKeyDescriptionList().Where(x => x.Key > 0);
            RoleLookup.ItemsSource = ERole.Accountant.GetKeyDescriptionList();
        }

        private void Stage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsFocused")
            {
                OnStageParameterWasChanged();
            };
        }

        public void Load(ProjectStage stage)
        {
            Stage = stage;
            Stage.PropertyChanged += Stage_PropertyChanged;
        }
        protected void BrowsePathButton_Click(object sender, RoutedEventArgs e)
        {
           var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                AddExtension = true,
                DefaultExt = "png",
                Filter = "Image Files|*.bmp;*.jpg;*.png;*.cdr;*.psd;*.tiff",
                CheckFileExists = true,
                CheckPathExists = true
            };
            dlg.ShowDialog();
            if (!String.IsNullOrWhiteSpace(dlg.FileName))
            {
                var fileName = System.IO.Path.GetFileName(dlg.FileName);
                if (Stage.StageParameter.Any(x => x.StringValue == fileName))
                {
                    MessageBox.Show(String.Format("Выбранная стадия проекта уже имеет прикрепленный файл с именем {0}", fileName), "Файл уже существует", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    FocusedParameter.StringValue = fileName;
                    FocusedParameter.OriginalFilePath = dlg.FileName;
                };
            };
        }

        private void AddParameterButon_Click(object sender, RoutedEventArgs e)
        {
            ((TableView)StageParameterGrid.View).AddNewRow();
            StageParameterGrid.Focus();
            StageParameterGrid.View.ShowEditor();
        }

        private void RemoveParameterButon_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить параметр?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var rowHandle = StageParameterGrid.View.FocusedRowHandle;
                if (rowHandle != GridControl.InvalidRowHandle)
                {
                    ((TableView)StageParameterGrid.View).DeleteRow(rowHandle);
                };
            };
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedParameter != null && !String.IsNullOrEmpty(FocusedParameter.StringValue))
            {
                string path = String.Empty;
                if (FocusedParameter.ID > -0)
                {
                    path = System.IO.Path.Combine(Stage.GetStagePath(), FocusedParameter.StringValue);
                }
                else
                {
                    path = FocusedParameter.OriginalFilePath;
                }
                if (System.IO.File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    MessageBox.Show("Ошибка доступа к файлу. Файл не существует или у вас недостаточно прав для открытия файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                };
            };
        }

        private void TableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            var row = StageParameterGrid.GetRow(e.RowHandle) as StageParameter;
            row.ID = -1;
            row.CrtDate = DateTime.Now;
            row.ParameterID = 0;
            row.ProjectStageID = Stage.ID;
        }
        private void TableView_ValidateRow(object sender, GridRowValidationEventArgs e)
        {
        }
        private void TableView_InvalidRowException(object sender, InvalidRowExceptionEventArgs e)
        {
        }
    }
}

