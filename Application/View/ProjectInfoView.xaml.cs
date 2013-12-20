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
using FederalProjectManager.Model;
using SmartClasses;
using System.Windows.Markup;
using System.Globalization;
using DevExpress.Xpf.Grid.LookUp;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace FederalProjectManager.View
{
    /// <summary>
    /// Логика взаимодействия для ProjectInfoView.xaml
    /// </summary>
    public partial class ProjectInfoView : UserControl, INotifyPropertyChanged
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

        private ORM.ProjectInfoView _project;

        public ORM.ProjectInfoView Project
        {
            get { return _project; }
            set 
            {
                if (_project != value)
                {
                    _project = value;
                    OnPropertyChanged("Project");
                };
            }
        }

        public ProjectInfoView()
        {
            InitializeComponent();
            this.DataContext = this;

            using (var dc = new ORM.FPMDataContext())
            {
                TypeLookup.ItemsSource = dc.ProjectKindsView.ToList();
            };
            PriorityLookup.ItemsSource = EProjectPriority.High.GetKeyDescriptionList();
        }

        private void CustomerEdit_DefaultButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FederalProjectManager.Windows.CustomerWindow();
            var fadeIn = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(dlg.AnimationDuration)
            };
            dlg.BeginAnimation(Window.OpacityProperty, fadeIn);
            dlg.Load(_project.ContactID);
            if (dlg.ShowDialog() == true)
            {
                Project.ContactID = dlg.FocusedContact.ID;
                Project.CustomerFullName = String.Format(@"{0} \ {1} {2}", dlg.FocusedCustomer.Name, dlg.FocusedContact.Name, 
                    !String.IsNullOrEmpty(dlg.FocusedContact.Position) ? String.Format("({0})", dlg.FocusedContact.Position) : String.Empty) ;
                
            }
        }

        
        
    }
}
