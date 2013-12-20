using DevExpress.Xpf.LayoutControl;
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
using FederalProjectManager.Model;
using ORM = FederalProjectManager.ORM;
using FederalProjectManager.View;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Windows.Markup;
using System.Globalization;


namespace FederalProjectManager.Windows
{
    public class ProjectArchiveCommandNameConverter : MarkupExtension, IValueConverter
    {
        public ProjectArchiveCommandNameConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "Архивировать";
            int rowState = -1;
            if (value != null && Int32.TryParse(value.ToString(), out rowState) && rowState > 0)
            {
                switch((ERowState)rowState)
                {
                    case ERowState.Active:
                        name = "Архивировать";
                        break;
                    case ERowState.Archived:
                        name = "Восстановить";
                        break;
                };
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

    /// <summary>
    /// Логика взаимодействия для ProjectWizardWindow.xaml
    /// </summary>
    public partial class ProjectWizardWindow : Window, INotifyPropertyChanged
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

        public TimeSpan AnimationDuration { get; private set; }

        private bool _loaded;
        private ORM.ProjectInfoView _project;

        private StageInfoView _stageInfoView;
        private bool _projectWasChanged;
        private bool _changesWasSaved;

        public bool ProjectWasChanged
        {
            get { return _projectWasChanged; }
            set
            {
                if (_loaded && _project.RowState == Convert.ToInt16(ERowState.Active) && _projectWasChanged != value)
                {
                    _projectWasChanged = value;
                    OnPropertyChanged("ProjectWasChanged");
                };
            }
        }
        public ORM.ProjectInfoView Project
        {
            get { return _project; }
            set
            {
                if (_project != value)
                {
                    _project = value;
                    OnPropertyChanged("Project");
                }
            }
        }

        public bool AllowDeleteAndArchieveProject
        {
            get { return Project.ID > 0 && ORM.Project.IsEditable; }
        }
        
        public ProjectWizardWindow()
        {
            _projectWasChanged = false;
            _changesWasSaved = false;
            AnimationDuration = TimeSpan.FromMilliseconds(500);
            InitializeComponent();
            this.DataContext = this;
            _stageInfoView = new View.StageInfoView()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Top
            };
            _stageInfoView.StageParameterWasChanged += _stageInfoView_StageParameterWasChanged;
            StageInfoPanel.Children.Add(_stageInfoView);
            _stageInfoView.Visibility = System.Windows.Visibility.Collapsed;
            StageTileView.StageChanged += StageTileView_StageChanged;
        }

        private void _stageInfoView_StageParameterWasChanged(object sender, EventArgs e)
        {
            ProjectWasChanged = true;
        }
        public void Load(int projectID)
        {
            _loaded = false;
            using (var dc = new ORM.FPMDataContext())
            {
                if (dc.ProjectInfoView.Any(x => x.ID == projectID))
                {
                    Project = dc.ProjectInfoView.Single(x => x.ID == projectID);
                    ProjectWasChanged = false;
                }
                else
                {
                    Project = new ORM.ProjectInfoView()
                    {
                        Caption = "Новый проект",
                        CrtDate = DateTime.Now,
                        IsInWork = false,
                        Priority = 3,
                        RowState = Convert.ToInt16(ERowState.Active)
                    };

                    ProjectWasChanged = true;
                };
            };
            Project.PropertyChanged += _project_PropertyChanged;
            ProjectCaption.Text = (_project.Caption.Length > 26) ? String.Format("{0}...", _project.Caption.Substring(0, 23)) : _project.Caption;
            ProjectCaption.ToolTip = _project.Caption;
            StageTileView.Load(_project);
            GeneralProjectView.Project = _project;
            _loaded = true;
        }

        private void _project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsValid")
            {
                ProjectWasChanged = true;
            };
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void CloseWizardButton_Click(object sender, RoutedEventArgs e)
        {
            closeWizard();
        }
        private void StageTileView_StageChanged(object sender, EventArgs e)
        {
            var ctrl = sender as WizardStageView;
            if (ctrl.FocusedStage != null)
            {
                if (ctrl.FocusedStage.ID >= 0)
                {
                    GeneralProjectView.Visibility = System.Windows.Visibility.Collapsed;
                    _stageInfoView.Visibility = System.Windows.Visibility.Visible;
                    _stageInfoView.Load((ORM.ProjectStage)ctrl.FocusedStage);
                }
                else
                {
                    GeneralProjectView.Project = _project;
                    GeneralProjectView.Visibility = System.Windows.Visibility.Visible;
                    _stageInfoView.Visibility = System.Windows.Visibility.Collapsed;
                };
            };
        }

        private void closeWizard()
        {
            this.DialogResult = _changesWasSaved;
            var fadeOut = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = new Duration(AnimationDuration), 
                AutoReverse= false
            };
            fadeOut.Completed += fadeOut_Completed;
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
        private void fadeOut_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void WizardApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateWizard())
            {
                var saveData = true;
                var newProjectID = -1;
                using (var dc = new ORM.FPMDataContext())
                {
                    #region ProjectInfo changes
                    var findProject = new Func<ORM.Project, bool>(x => x.ID == _project.ID);
                    ORM.Project project = null;
                    if (dc.Project.Any(findProject))
                    {
                        project = dc.Project.Single(findProject);
                        newProjectID = project.ID;
                    }
                    else
                    {
                        project = new ORM.Project();
                    };
                    project.Type = _project.Type;
                    project.ResponsiblePerson = _project.ResponsiblePerson;
                    project.Progress = getProjectProgress();
                    project.Priority = _project.Priority;
                    project.Note = _project.Note;
                    project.IsInWork = _project.IsInWork;
                    project.ContactID = _project.ContactID;
                    project.Caption = _project.Caption;
                    project.CrtDate = _project.CrtDate;
                    project.RowState = _project.RowState;
                    #endregion
                    if (_project.ID <= 0)
                    {
                        dc.Project.InsertOnSubmit(project);
                    }
                    else
                    {
                        #region ProjectStage changes
                        var stages = dc.ProjectStage.Where(x => x.ProjectID == _project.ID);
                        foreach (var itm in stages)
                        {
                            if (!saveData)
                            {
                                break;
                            };
                            if (StageTileView.Stages.Any(x => x.ID == itm.ID))
                            {
                                /*измененные стадии*/
                                var stg = StageTileView.Stages.Single(x => x.ID == itm.ID);
                                itm.IsPublic = stg.IsPublic;
                                itm.SortIndex = stg.SortIndex;
                                itm.Name = stg.Name;
                                itm.Note = stg.Note;
                                itm.ResponsibleRole = stg.ResponsibleRole;
                                itm.StageState = stg.StageState;

                                /*Изменение параметров стадии*/
                                var parameters = dc.StageParameter.Where(x => x.ProjectStageID == itm.ID);
                                foreach (var par in parameters)
                                {
                                    if (stg.StageParameter.Any(x => x.ID == par.ID))
                                    {
                                        var srcPar = stg.StageParameter.Single(x => x.ID == par.ID);
                                        par.NumberValue = srcPar.NumberValue;
                                        par.StringValue = srcPar.StringValue;
                                        par.DateTimeValue = srcPar.DateTimeValue;
                                    }
                                    else
                                    {
                                        dc.StageParameter.DeleteOnSubmit(par);
                                        if(ParameterRepository.GetParameterValueType(par.ParameterID) == EParameterValueType.Path)
                                        {
                                            if(!stg.StageParameter.Any(x => x.StringValue == par.StringValue))
                                            {
                                                saveData = deleteFile(par);
                                            };
                                        };
                                    };
                                };
                                if (saveData)
                                {
                                    /*Новые параметры*/
                                    foreach (var nStg in stg.StageParameter.Where(x => x.ID < 0))
                                    {
                                        var newParameter = new ORM.StageParameter()
                                        {
                                            CrtDate = nStg.CrtDate,
                                            DateTimeValue = nStg.DateTimeValue,
                                            NumberValue = nStg.NumberValue,
                                            StringValue = nStg.StringValue,
                                            ParameterID = nStg.ParameterID,
                                            ProjectStageID = nStg.ProjectStageID
                                        };
                                        dc.StageParameter.InsertOnSubmit(newParameter);
                                    };
                                };
                            }
                            else
                            {
                                /*удаленные стадии*/
                                itm.RowState = Convert.ToInt16(ERowState.Deleted);
                            };
                        };
                        #endregion
                    };
                    if (saveData)
                    {
                        if (StageTileView.Stages.Any(x => x.ID == 0))
                        {
                            /*новые стадии*/
                            foreach (var itm in StageTileView.Stages.Where(x => x.ID == 0))
                            {
                                var stage = new ORM.ProjectStage()
                                {
                                    CrtDate = itm.CrtDate,
                                    IsPublic = itm.IsPublic,
                                    Name = itm.Name,
                                    Note = itm.Note,
                                    Project = project,
                                    ResponsibleRole = itm.ResponsibleRole,
                                    RowState = 1,
                                    SortIndex = itm.SortIndex,
                                    StageState = itm.StageState,
                                    StageParameter = itm.StageParameter
                                };
                                dc.ProjectStage.InsertOnSubmit(stage);
                            };
                        };
                    };
                    #region Upload Files
                    if (project.ID > 0)
                    {
                        saveData = UploadFiles();
                        if (saveData)
                        {
                            dc.SubmitChanges();
                        };
                    }
                    else
                    {
                        dc.SubmitChanges();
                        newProjectID = project.ID;
                        UploadFiles();
                    };
                    #endregion
                };
                if (saveData)
                {
                    var focusedStage = StageTileView.Stages.Single(x => x.IsFocused);
                    Load(newProjectID);
                    StageTileView.FocusedStage = focusedStage;
                };
                ProjectWasChanged = !saveData;
                _changesWasSaved = saveData;

                OnPropertyChanged("AllowDeleteAndArchieveProject");
            };
        }

        private bool deleteFile(ORM.StageParameter par)
        {
            var result = true;
            var filePath = System.IO.Path.Combine(par.ProjectStage.GetStagePath(), par.StringValue);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch(Exception ex)
                {
                    result = MessageBox.Show(String.Format("Файл {0} не может быть удален.Вы хотите продолжить сохранение проекта?\nОшибка:{1}", par.StringValue, ex.Message), "Ошибка удаления файла", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                };
            };
            return result;
        }
        private bool UploadFiles()
        {
            bool result = true;
            foreach (var stg in StageTileView.Stages)
            {
                foreach (var par in stg.StageParameter.Where(x => !String.IsNullOrEmpty(x.OriginalFilePath)))
                {
                    if (ParameterRepository.GetParameterValueType(par.ParameterID) == EParameterValueType.Path)
                    {
                        var stageFolder = String.Empty;
                        if (checkDirectory(stg, out stageFolder))
                        {
                            var filePath = System.IO.Path.Combine(stageFolder, par.StringValue);
                            try
                            {
                                if (!System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Copy(par.OriginalFilePath, filePath);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(String.Format("При копировании файла {0} в файловое хранилище возникла ошибка:\n{1}", par.StringValue, ex.Message), "Ошибка копированияы", MessageBoxButton.OK, MessageBoxImage.Error);
                                result = false;
                            };
                        }
                        else
                        {
                            result = false;
                        }
                    };
                };
            };
            return result;
        }
        private void WizardCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ProjectWasChanged)
            {
                closeWizard();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("В проект были внесены изменения. Вы действительно хотите отменить изменения и закрыть мастер?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    closeWizard();
                };
            };
        }


        private bool checkDirectory(ORM.ProjectStage stage, out string stageFolder)
        {
            bool result = false;
            stageFolder = stage.GetStagePath();
            var projectFolder = stage.Project.GetProjectPath();
            try
            {
                if (!System.IO.Directory.Exists(projectFolder))
                {
                    System.IO.Directory.CreateDirectory(projectFolder);
                };
                if (!System.IO.Directory.Exists(stageFolder))
                {
                    System.IO.Directory.CreateDirectory(stageFolder);
                };
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Ошибка доступа к папке проекта:\n{0}", ex.Message), "Ошибка доступа к папке проекта", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            return result;
        }
        private float getProjectProgress()
        {
            float result = 0f;
            if (_project.IsInWork)
            {
                int stagesNumber = StageTileView.Stages.Count(x => x.RowState == 1);
                if (stagesNumber > 0)
                {
                    int processedStages = StageTileView.Stages.Count(x => x.RowState == 1 && (x.StageState == Convert.ToInt16(EStageState.Completed)
                                                                                              || x.StageState == Convert.ToInt16(EStageState.Skipped)
                                                                                              || x.StageState == Convert.ToInt16(EStageState.Canceled)));
                    result = 100 * processedStages / stagesNumber;
                };
            };
            return result;
        }
        private void StageTileView_StagePropertyWasChanged(object sender, EventArgs e)
        {
            ProjectWasChanged = true;
        }

        private void WizardArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            var message = (_project.RowState == Convert.ToInt16(ERowState.Active)) ? "Вы действительно хотите отправить выбранный проект в архив?" : "Вы действительно хотите восстановить выбранный проект?";

            if (MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var dc = new ORM.FPMDataContext())
                {
                    if (dc.Project.Any(x => x.ID == _project.ID))
                    {
                        var project = dc.Project.Single(x => x.ID == _project.ID);
                        project.RowState = (project.RowState == Convert.ToInt16(ERowState.Active)) ? Convert.ToInt16(ERowState.Archived) : Convert.ToInt16(ERowState.Active);
                        dc.SubmitChanges();
                    };
                };
                ProjectWasChanged = false;
                _changesWasSaved = true;
                closeWizard();
            }
        }

        private void WizardDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить выбранный проект?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (var dc = new ORM.FPMDataContext())
                {
                    if(dc.Project.Any(x => x.ID == _project.ID))
                    {
                        var project = dc.Project.Single(x => x.ID == _project.ID);
                        project.RowState = Convert.ToInt16(ERowState.Deleted);
                        dc.SubmitChanges();
                    };
                };
                ProjectWasChanged = false;
                _changesWasSaved = true;
                closeWizard();
            }
        }

        private bool ValidateWizard()
        {
            var result = true;
            if (StageTileView.FocusedStage != null 
                && (StageTileView.FocusedStage.StageState == Convert.ToInt16(EStageState.Completed)
                    || StageTileView.FocusedStage.StageState == Convert.ToInt16(EStageState.Immutable)))
            {
                if (StageTileView.FocusedStage.ID >= 0)
                {
                    if (_stageInfoView.Stage.Validate().Length > 0)
                    {
                        result = false;
                    }
                    else
                    {
                        foreach (var itm in _stageInfoView.Stage.StageParameter)
                        {
                            if (itm.Validate().Length > 0)
                            {
                                result = false;
                                break;
                            };
                        };
                    };
                }
                else
                {
                    if (GeneralProjectView.Project.Validate().Length > 0)
                    {
                        result = false;
                    };
                };
                if (!result)
                {
                    MessageBox.Show("Форма редактирования проекта имеет ошибки.\nИсправьте ошибки и повторите попытку.", "Ошибки на форме", MessageBoxButton.OK, MessageBoxImage.Error);
                };
            };
            return result;
        }
        private void StageTileView_StageChanging(object sender, CancelEventArgs e)
        {

            e.Cancel = !ValidateWizard();
        }
    }
}
