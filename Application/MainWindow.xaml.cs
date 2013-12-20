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
using FederalProjectManager.View;
using DevExpress.Xpf.Grid;
using FederalProjectManager.Model;

namespace FederalProjectManager
{
    public partial class MainWindow : DXWindow
    {
        public string ApplicationVersion
        {
            get 
            {
                return String.Format("Текущая версия: {0}", UserSettings.ApplicationVersion);
            }
        }
        public string ApplicationBuildDate
        {
            get
            {
                return String.Format("Дата сборки: {0}", UserSettings.ApplicationBuildDate.ToString("dd.MM.yyyy HH:mm"));
            }
        }

        public MainWindow()
        {
            var isContinue = UserSettings.CheckConnection();
            if (!isContinue)
            {
                MessageBox.Show("Неудалось подключиться к базе данных. Проврьте настройки подключения.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                var dlg = new Windows.SettingsWindow();
                if (dlg.ShowDialog() == true)
                {
                    isContinue = true;
                }
                else
                {
                    MessageBox.Show("Неудалось подключиться к базе данных. Приложение будет закрыто", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                };
            };
            if (isContinue)
            {
                this.DataContext = this;
                InitializeComponent();
            }
            else
            {
                Environment.Exit(0);
            };   
        }
        protected void ActiveProjectView_FocusedProjectChanged(object sender, ActiveProjectsView.FocusedProjectChangedEventArgs e)
        {
            HeaderView.ProjectID = e.ProjectID;
        }

        private void HeaderView_RefreshActiveProjectRequest(object sender, Header.RefreshRequestEventArgs e)
        {
            ActiveProjectView.Refresh(e.ProjectID);
        }

        private void ActiveProjectView_ProjectDoubleClick(object sender, EventArgs e)
        {
            if (HeaderView.ProjectID > 0)
            {
                HeaderView.EditProject();
            };
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
