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
using FederalProjectManager.Windows;
using System.Timers;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace FederalProjectManager
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class Header : UserControl, INotifyPropertyChanged
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

        public class RefreshRequestEventArgs : EventArgs
        {
            public int ProjectID { get; set; }
        }

        public event EventHandler<RefreshRequestEventArgs> RefreshActiveProjectRequest;
        protected virtual void OnRefreshActiveProjectRequest(int projectID)
        {
            OnPropertyChanged("AllowCreateNewProject");
            if (RefreshActiveProjectRequest != null)
            {
                RefreshActiveProjectRequest(this, new RefreshRequestEventArgs() { ProjectID = projectID });
            };
        }
        private int _projectID;
        private Timer _timer;

        public int ProjectID 
        {
            get { return _projectID; }
            set
            {
                if (_projectID != value)
                {
                    _projectID = value;
                    OnPropertyChanged("ProjectID");
                }
            }
        }
        public Timer Timer { get { return _timer; } }
        

        private DateTime _timerDate = DateTime.MinValue;

        public bool AllowCreateNewProject
        {
            get { return Model.UserSettings.ProfileRole == Model.ERole.Administrator; }
        }

        public Header()
        {
            this.DataContext = this;
            InitializeComponent();
            var secondsFadeOut = new DoubleAnimation()
            {
                From = 1,
                To = 0.2,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AutoReverse = true,
                RepeatBehavior =  new RepeatBehavior(TimeSpan.FromDays(7))
            };
            HourMinuteDelimeterText.BeginAnimation(TextBlock.OpacityProperty, secondsFadeOut);
            
            _timer = new Timer(1000);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = false;
        }

        public void StartTimer()
        {
            _timer.Enabled = true;
            timer_Elapsed(_timer, null);

        }
        

        public void EditProject()
        {
            showWizard(ProjectID);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var minutes = now.Subtract(_timerDate).TotalMinutes;
            var hours = _timerDate.Hour - now.Hour;
            var days =  _timerDate.Day - now.Day;

            if (minutes >= 1)
            {
                _timerDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    if (minutes >= 1)
                    {
                        OnRefreshActiveProjectRequest(-1);
                        this.BeginStoryboard((Storyboard)this.FindResource("MinutesChangedAnimation"));
                    };
                    if (hours != 0)
                    {
                        this.BeginStoryboard((Storyboard)this.FindResource("HoursChangedAnimation"));
                    };
                    if (days != 0 || (_timerDate.Day == 1 && now.Day == 1))
                    {
                        DateText.Text = _timerDate.ToString("dd.MM.yyyy");
                    };
                });
            };
        }

        private void MinutesChangedAnimation_Completed(object sender, EventArgs e)
        {
            MinuteText.Text = _timerDate.ToString("mm");
        }
        private void HoursChangedAnimation_Completed(object sender, EventArgs e)
        {
            HourText.Text = _timerDate.ToString("HH");
        }

        private void showWizard(int projectID)
        {
            var master = new ProjectWizardWindow();
            var fadeIn = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(master.AnimationDuration)
            };
            master.BeginAnimation(Window.OpacityProperty, fadeIn);
            master.Load(projectID);
            if (master.ShowDialog() == true)
            {
                OnRefreshActiveProjectRequest(master.Project.ID);
            };
        }

        private void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            showWizard(ProjectID);
        }
        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            showWizard(-1);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var allowEditSettings = Model.UserSettings.ProfileRole == Model.ERole.Administrator;
            if (!allowEditSettings)
            {
                var authDlg = new AdminAutorizationWindow();
                allowEditSettings = authDlg.ShowDialog() ?? false;
            };
            if (allowEditSettings)
            {
                var settings = new SettingsWindow();
                var fadeIn = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(settings.AnimationDuration)
                };
                settings.BeginAnimation(Window.OpacityProperty, fadeIn);
                if (settings.ShowDialog() == true)
                {
                    OnRefreshActiveProjectRequest(-1);
                };
            };
        }

        private void CustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var customers = new CustomerWindow();
            var fadeIn = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(customers.AnimationDuration)
            };
            customers.BeginAnimation(Window.OpacityProperty, fadeIn);
            customers.IsShowApplyButton = false;
            customers.Load(-1);
            customers.ShowDialog();
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var allowViewStatistics = Model.UserSettings.ProfileRole == Model.ERole.Administrator
                || Model.UserSettings.ProfileRole == Model.ERole.Accountant
                || Model.UserSettings.ProfileRole == Model.ERole.Designer;
            if (allowViewStatistics)
            {
                var statistics = new StatisticsWindow();
                var fadeIn = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(statistics.AnimationDuration)
                };
                statistics.BeginAnimation(Window.OpacityProperty, fadeIn);
                statistics.ShowDialog();
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }
    }
}
