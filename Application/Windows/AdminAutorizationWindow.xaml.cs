using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FederalProjectManager.Windows
{
    /// <summary>
    /// Логика взаимодействия для AdminAutorizationWindow.xaml
    /// </summary>
    public partial class AdminAutorizationWindow : Window, INotifyPropertyChanged
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

        private string _password;

        public string Password 
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged("Password");
                };
            }
        }

        public AdminAutorizationWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            AdminPassowrdEdit.Focus();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void AuthirizateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Password == Model.UserSettings.AdminPassword)
            {
                this.DialogResult = true;
                this.Close();
            }
            else if (MessageBox.Show("Неверный пароль администратора.\nВы хотите повторить ввод пароля?", "Неверный пароль", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                Password = String.Empty;
                AdminPassowrdEdit.Focus();
                this.DialogResult = false;
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
