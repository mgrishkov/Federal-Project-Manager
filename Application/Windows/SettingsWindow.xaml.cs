using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FederalProjectManager.Model;
using SmartClasses;
using DevExpress.Xpf.Grid;


namespace FederalProjectManager.Windows
{
    /// <summary>
    /// Логика взаимодействия для UserSettings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
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

        private ORM.FPMDataContext _dc;
        private string _storagePath;
        private SqlConnectionStringBuilder _connectionString;
        private int _profileRole;
        private IEnumerable<ORM.DictionaryView> _dictionaries;
        private ORM.DictionaryView _focusedDictionary;
        private ORM.Dictionary _focusedConstant;
        private IEnumerable<ORM.Parameter> _parameters;
        private ORM.Parameter _focusedParameter;
        private IEnumerable<ORM.ParameterValue> _parameterValues;
        private ORM.ParameterValue _focusedParameterValue;
        private bool _isDBConnected;

        public TimeSpan AnimationDuration { get; private set; }
        public string StoragePath
        {
            get { return _storagePath; }
            set
            {
                if (_storagePath != value)
                {
                    _storagePath = value;
                    OnPropertyChanged("StoragePath");
                };
            }
        }
        public SqlConnectionStringBuilder ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    OnPropertyChanged("ConnectionString");
                };
            }
        }
        public int ProfileRole
        {
            get { return _profileRole; }
            set
            {
                if (_profileRole != value)
                {
                    _profileRole = value;
                    OnPropertyChanged("ProfileRole");
                };
            }
        }
        public IEnumerable<ORM.DictionaryView> Dictionaries
        {
            get { return _dictionaries; }
            set
            {
                if (_dictionaries != value)
                {
                    _dictionaries = value;
                    OnPropertyChanged("Dictionaries");
                }
            }
        }
        public ORM.DictionaryView FocusedDictionary
        {
            get { return _focusedDictionary; }
            set
            {
                if (_focusedDictionary != value)
                {
                    _focusedDictionary = value;
                    OnPropertyChanged("FocusedDictionary");
                }
            }
        }
        public ORM.Dictionary FocusedConstant
        {
            get { return _focusedConstant; }
            set
            {
                if (_focusedConstant != value)
                {
                    _focusedConstant = value;
                    OnPropertyChanged("FocusedConstant");
                }
            }
        }
        public IEnumerable<ORM.Parameter> Parameters
        {
            get { return _parameters; }
            set
            {
                if (_parameters != value)
                {
                    _parameters = value;
                    OnPropertyChanged("Parameters");
                };
            }
        }
        public ORM.Parameter FocusedParameter
        {
            get { return _focusedParameter; }
            set
            {
                if (_focusedParameter != value)
                {
                    _focusedParameter = value;
                    OnPropertyChanged("FocusedParameter");
                    OnPropertyChanged("IsAllowAddParameterValue");
                    OnPropertyChanged("IsAllowDeleteParameterValue");
                };
            }
        }
        public IEnumerable<ORM.ParameterValue> ParameterValues
        {
            get { return _parameterValues; }
            set
            {
                if (_parameterValues != value)
                {
                    _parameterValues = value;
                    OnPropertyChanged("ParameterValues");
                }
            }
        }
        public ORM.ParameterValue FocusedParameterValue
        {
            get { return _focusedParameterValue; }
            set
            {
                if (_focusedParameterValue != value)
                {
                    _focusedParameterValue = value;
                    OnPropertyChanged("FocusedParameterValue");
                    OnPropertyChanged("IsAllowAddParameterValue");
                    OnPropertyChanged("IsAllowDeleteParameterValue");
                };
            }
        }
        public List<KeyValuePair<Int16, string>> ValueTypes 
        {
            get
            {
                return EParameterValueType.Int32.GetInt16KeyDescriptionList();
            }
        }
        public bool IsAllowAddParameterValue
        {
            get { return FocusedParameter != null && FocusedParameter.ValueType == Convert.ToInt16(EParameterValueType.Enum); }
        }
        public bool IsAllowDeleteParameterValue
        {
            get { return IsAllowAddParameterValue && FocusedParameterValue != null ; }
        }
        public List<KeyValuePair<int, string>> Roles
        {
            get
            {
                return ERole.Administrator.GetKeyDescriptionList();
            }
        }
        public bool IsDBConnected
        {
            get { return _isDBConnected; }
            set
            {
                if (_isDBConnected != value)
                {
                    if (!_isDBConnected && value)
                    {
                        _dc = new ORM.FPMDataContext(ConnectionString.ConnectionString);
                        Dictionaries = _dc.DictionaryView;
                        FocusedDictionary = Dictionaries.First();
                        Parameters = _dc.Parameter;
                        FocusedParameter = Parameters.First();
                    };
                    _isDBConnected = value;
                    OnPropertyChanged("IsDBConnected");
                }
            }
        }

        public SettingsWindow()
        {
            IsDBConnected = UserSettings.CheckConnection();
            AnimationDuration = new TimeSpan(0, 0, 0, 0, 500);
            InitializeComponent();
            this.DataContext = this;
            ConnectionString = new SqlConnectionStringBuilder(UserSettings.ConnectionString);
            StoragePath = UserSettings.StoragePath;
            ProfileRole = (int)UserSettings.ProfileRole;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SettingsApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkConnection(true))
            {
                UserSettings.ConnectionString = ConnectionString.ConnectionString;
                UserSettings.StoragePath = StoragePath;
                UserSettings.ProfileRole = (ERole)ProfileRole;
                Properties.Settings.Default.Save();
                if (_dc != null)
                {
                    _dc.SubmitChanges();
                };
                closeWindow();
                ParameterRepository.Reload();
                this.DialogResult = true;
            };
        }

        private void SettingsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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

        void fadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StoragePathEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.SelectedPath = StoragePath;
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StoragePath = dlg.SelectedPath;
                };
            };
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            IsDBConnected = checkConnection(false);
        }
        private bool checkConnection(bool silentOK)
        {
            var msg = String.Empty;
            var result = UserSettings.CheckConnection(ConnectionString.ConnectionString, out msg);
            if (result)
            {
                if (!silentOK)
                {
                    MessageBox.Show("Проверка усппешно выполнена", "Результат проверки", MessageBoxButton.OK, MessageBoxImage.Information);
                };
            }
            else
            {
                MessageBox.Show(String.Format("Ошибка проверки:\n{0}", msg), "Результат проверки", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            return result;
        }

        private void ConstantsAddButton_Click(object sender, RoutedEventArgs e)
        {
            ((TableView)ConstatnsGrid.View).AddNewRow();
        }
        private void ConstantTableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            var row = ConstatnsGrid.GetRow(e.RowHandle) as ORM.Dictionary;
            row.DictionaryView = FocusedDictionary;
            row.IsVisible = true;
            ConstatnsGrid.Focus();
        }
        private void ConstantsDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите исключить выбранную константу из справочника?", "Исключить константу", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var rowHandle = ConstatnsGrid.View.FocusedRowHandle;
                var row = ConstatnsGrid.GetRow(rowHandle) as ORM.Dictionary;
                if (row.DictionaryNumber == 1)
                {
                    if (!_dc.Project.Any(x => x.Type == row.ID))
                    {
                        ((TableView)ConstatnsGrid.View).DeleteRow(rowHandle);
                        _dc.Dictionary.DeleteOnSubmit(row);
                    }
                    else if (MessageBox.Show("Тип проекта не может быть удален, т.к. имеются проекты с выбранным типом.\nИсключить тип для будущих проектов?", "Ошибка исключения", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        row.IsVisible = false;
                    };
                };
            };
        }
        private void ParameterAddButton_Click(object sender, RoutedEventArgs e)
        {
            ((TableView)ParameterGrid.View).AddNewRow();
        }
        private void ParameterTableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            ConstatnsGrid.Focus();
        }
        private void ParameterDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите исключить выбранный параметр?", "Исключить параметр", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var rowHandle = ParameterGrid.View.FocusedRowHandle;
                var row = ParameterGrid.GetRow(rowHandle) as ORM.Parameter;

                if (_dc.StageParameter.Any(x => x.ParameterID == row.ID))
                {
                    MessageBox.Show("Нельзя удалить выбранный параметр, т.к. на него ссылается стадия проекта.", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (row.ParameterValue.Any())
                {
                    MessageBox.Show("Нельзя удалить выбранный параметр, т.к. у него имеются значения.", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ((TableView)ParameterGrid.View).DeleteRow(rowHandle);
                    _dc.Parameter.DeleteOnSubmit(row);
                };
            };
        }
        private void ParameterTableView_ValidateRow(object sender, GridRowValidationEventArgs e)
        {
            var row = e.Row as ORM.Parameter;
            if (row.ID > 0)
            {
                if (_dc.StageParameter.Any(x => x.ParameterID == row.ID))
                {
                    e.IsValid = false;
                    MessageBox.Show("Нельзя изменить тип параметра, т.к. он уже был использован в страдии проекта.", "Ошибка изменения", MessageBoxButton.OK, MessageBoxImage.Error);
                };
            }
        }
        private void ParameterTableView_InvalidRowException(object sender, InvalidRowExceptionEventArgs e)
        {
            e.ExceptionMode = ExceptionMode.NoAction;            
        }

        private void ParameterValueAddButton_Click(object sender, RoutedEventArgs e)
        {
            ((TableView)ParameterValueGrid.View).AddNewRow();
        }
        private void ParameterValueTableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            var row = ParameterValueGrid.GetRow(e.RowHandle) as ORM.ParameterValue;
            row.Parameter = FocusedParameter;
            ParameterValueGrid.Focus();
        }
        private void ParameterValueDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите исключить выбранное значение из спаска доступных значений параметра?", "Исключить значение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var rowHandle = ParameterValueGrid.View.FocusedRowHandle;
                var row = ParameterValueGrid.GetRow(rowHandle) as ORM.ParameterValue;
                if (_dc.StageParameter.Any(x => x.ParameterID == row.ParameterID && x.NumberValue == row.ID))
                {
                    MessageBox.Show("Нельзя удалить значение параметра, т.к. оно используется в стадии проекта.", "Ошибка исключения", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ((TableView)ParameterValueGrid.View).DeleteRow(rowHandle);
                    _dc.ParameterValue.DeleteOnSubmit(row);
                };
            };
        }

        private void TableView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "ValueType")
            {
                OnPropertyChanged("IsAllowAddParameterValue");
            }
        }
        
    }
}
