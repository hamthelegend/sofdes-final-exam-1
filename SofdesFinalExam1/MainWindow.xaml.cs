using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.UI.Controls;


namespace SofdesQuiz3_1
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<User> _users;
        private List<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        private User _selectedUser;

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                if (value == null)
                {
                    removeButton.IsEnabled = false;
                    addButton.Content = "Add";
                }
                else
                {
                    removeButton.IsEnabled = true;
                    addButton.Content = "Update";
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            genderInput.SelectedItem = "Unspecified";
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private async void Remove(object sender, RoutedEventArgs e)
        {
            var response = await new ContentDialog
            {
                Title = "Delete user",
                Content = "Are you sure you want to delete this user?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();

            if (response == ContentDialogResult.Primary)
            {
                var id = SelectedUser.Id;
                if (id != null) UsersDb.Delete((int)id);
                Clear();
                LoadData();
            }
        }

        private async void AddUpdate(object sender, RoutedEventArgs e)
        {
            var user = await ParseUserAsync();
            if (user == null) return;
            UsersDb.InsertUpdate(user);
            Clear();
            LoadData();
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void SelectUser(object sender, DoubleTappedRoutedEventArgs e)
        {
            var user = usersDataGrid.SelectedItem as User;
            if (user != null)
            {
                SelectedUser = user;
                fullNameInput.Text = user.FullName;
                emailInput.Text = user.Email;
                birthdateInput.SelectedDate = new DateTimeOffset(user.Birthdate.Year, user.Birthdate.Month, user.Birthdate.Day, 0, 0, 0, TimeSpan.FromHours(8));
                genderInput.SelectedItem = user.Gender;
                addressInput.Text = user.Address;
            }
        }

        private async Task<User> ParseUserAsync()
        {
            var id = SelectedUser?.Id;

            var fullName = fullNameInput.Text;
            var email = emailInput.Text;
            var birthdateNullable = birthdateInput.SelectedDate;
            var gender = genderInput.SelectedItem as string;
            var address = addressInput.Text;

            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                birthdateNullable == null ||
                gender == null ||
                string.IsNullOrEmpty(address))
            {
                await new ContentDialog
                {
                    Title = "Empty fields",
                    Content = "None of the fields can be empty.",
                    CloseButtonText = "Okay",
                    XamlRoot = Content.XamlRoot,
                }.ShowAsync();
                return null;
            }

            var birthdate = (DateTimeOffset)birthdateNullable;
            return new User(fullName, email, new DateOnly(birthdate.Year, birthdate.Month, birthdate.Day), gender, address, id);
        }

        private void Clear()
        {
            SelectedUser = null;
            fullNameInput.Text = string.Empty;
            emailInput.Text = string.Empty;
            birthdateInput.SelectedDate = null;
            genderInput.SelectedItem = "Unspecified";
            addressInput.Text = string.Empty;
            usersDataGrid.SelectedItem = null;
        }

        private void LoadData()
        {
            var search = searchInput.Text;
            Users = UsersDb.GetAll(search);
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UsersDataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Id") e.Cancel = true;
            else if (e.Column.Header.ToString() == "FullName") e.Column.Header = "Full Name";
        }
    }
}
