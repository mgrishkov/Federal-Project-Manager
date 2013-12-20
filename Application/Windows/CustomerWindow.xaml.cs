using FederalProjectManager.ORM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ORM = FederalProjectManager.ORM;
using FederalProjectManager.Model;
using DevExpress.Xpf.Grid;
using System.ComponentModel;
using System.Windows.Markup;
using System.Globalization;
using DevExpress.Xpf.Bars;

namespace FederalProjectManager.Windows
{
    
    /// <summary>
    /// Логика взаимодействия для CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window, INotifyPropertyChanged
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
        private IEnumerable<Customer> _customers;
        private Contact _focusedContact;
        private Customer _focusedCustomer;

        public TimeSpan AnimationDuration { get; private set; }
        public IEnumerable<Customer> Customers
        {
            get { return _customers; }
            set
            {
                if (_customers != value)
                {
                    _customers = value;
                    OnPropertyChanged("Customers");
                }
            }
        }
        public Contact FocusedContact
        {
            get { return _focusedContact; }
            set
            {
                if (_focusedContact != value)
                {
                    _focusedContact = value;
                    OnPropertyChanged("FocusedContact");
                    OnPropertyChanged("IsDataModified");
                    OnPropertyChanged("AllowSaveChanges");
                    OnPropertyChanged("AllowDeleteContact");
                }
            }
        }
        public Customer FocusedCustomer
        {
            get { return _focusedCustomer; }
            set
            {
                if (_focusedCustomer != value)
                {
                    _focusedCustomer = value;
                    OnPropertyChanged("FocusedCustomer");
                    OnPropertyChanged("IsDataModified");
                    OnPropertyChanged("AllowDeleteCustomer");
                }
            }
        }
        public bool IsDataModified
        {
            get
            {
                var result = false;
                if (_dc != null)
                {
                    var cs = _dc.GetChangeSet();
                    result = cs.Updates.Count > 0 || cs.Deletes.Count > 0 || cs.Inserts.Count > 0;
                }
                return result;
            }
        }
        public bool AllowSaveChanges
        {
            get { return Project.IsEditable && FocusedContact != null; }
        }
        public bool IsEditable
        {
            get { return Model.UserSettings.ProfileRole == ERole.Administrator; }
        }
        public bool IsShowApplyButton { get; set; }
        public bool AllowDeleteCustomer
        {
            get { return IsEditable && FocusedCustomer != null; }
        }
        public bool AllowDeleteContact
        {
            get { return IsEditable && FocusedContact != null; }
        }

        public CustomerWindow()
        {
            _dc = new ORM.FPMDataContext();
            InitializeComponent();
            AnimationDuration = new TimeSpan(0, 0, 0, 0, 500);
            this.DataContext = this;
            ContactGrid.FilterString = "RowState = 1";
            IsShowApplyButton = true;
        }
        public void Load(int contactID)
        {
            Customers = _dc.Customer.Where(x => x.RowState == Convert.ToInt16(ERowState.Active));
            if (_dc.Contact.Any(x => x.ID == contactID))
            {
                ContactGrid.Focus();
                FocusedContact = _dc.Contact.Single(x => x.ID == contactID);
                FocusedCustomer = FocusedContact.Customer;
            }
            else
            {
                CustomerGrid.Focus();
            };
        }


        private void CloseCustomerWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            closeWindow();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
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

        private void CustumerApply_Click(object sender, RoutedEventArgs e)
        {
            saveChnages();
            this.DialogResult = true;
            closeWindow();
        }

        private void CustomerAddButton_Click(object sender, RoutedEventArgs e)
        {
            ((TableView)CustomerGrid.View).AddNewRow();
            
        }

        private void CustomerDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var rowHandle = CustomerGrid.View.FocusedRowHandle;
            if (rowHandle != GridControl.InvalidRowHandle)
            {
                if (MessageBox.Show("Вы действительно хотите удалить выбранного заказчика?", "Удаление заказчика", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ((TableView)CustomerGrid.View).DeleteRow(rowHandle);
                };
            };
        }
        private void ContactAddButton_Click(object sender, RoutedEventArgs e)
        {
            ContactGrid.FilterString = ""; //если не сбрасвать фильр, то в гриде появляется 2 строки
            ((TableView)ContactGrid.View).AddNewRow();
        }
        private void ContactDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var rowHandle = ContactGrid.View.FocusedRowHandle;
            if (rowHandle != GridControl.InvalidRowHandle)
            {
                if (MessageBox.Show("Вы действительно хотите удалить выбранное контактное лицо?", "Удаление контактное лицо", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ((TableView)ContactGrid.View).DeleteRow(rowHandle);
                };
            };
        }
        private void ContactTableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            var row = ContactGrid.GetRow(e.RowHandle) as Contact;
            row.Customer = FocusedCustomer;
            row.RowState = Convert.ToInt16(ERowState.Active);
            FocusedContact = row;
            ContactGrid.Focus();
            OnPropertyChanged("IsDataModified");
            ContactGrid.FilterString = "RowState = 1";
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            saveChnages();
        }
        private void saveChnages()
        {
            _dc.SubmitChanges();
            OnPropertyChanged("IsDataModified");
        }

        private void CustomerTableView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            var row = CustomerGrid.GetRow(e.RowHandle) as Customer;
            row.RowState = Convert.ToInt16(ERowState.Active);
            FocusedCustomer = row;
            CustomerGrid.Focus();
            OnPropertyChanged("IsDataModified");
        }

        private void ShowFilterPanelBarItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var ctrl = sender as BarButtonItem;
            var view = ((TableView)((GridColumnMenuInfo)ctrl.DataContext).Grid.View);
            view.ShowAutoFilterRow = !view.ShowAutoFilterRow;
        }

        private void TableView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            OnPropertyChanged("IsDataModified");
        }
    }
}
