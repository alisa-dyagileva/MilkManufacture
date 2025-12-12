using MilkManufacture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json;


namespace MilkManufacture.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private User _currentUser = new User();
        private class ApiResponse
        {
            public string value { get; set; }
        }
        public AddEditPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null) _currentUser = selectedUser;
            else
            {
                _currentUser = new User();
                _currentUser.Blocked = false;
                _currentUser.FIO = "";
                _currentUser.Email = "";
                _currentUser.Phone = "";
                _currentUser.Snils = "";
            }

            DataContext = _currentUser;
            ComboRole.ItemsSource = Manufacture_bdEntities1.GetContext().Roles.ToList();

            FioTextBox.TextChanged += (s, e) => ValidateFio();
            EmailTextBox.TextChanged += (s, e) => ValidateEmail();
            PhoneTextBox.TextChanged += (s, e) => ValidatePhone();
            SnilsTextBox.TextChanged += (s, e) => ValidateSnils();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidateFio(); ValidateEmail(); ValidatePhone(); ValidateSnils();

            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_currentUser.Login)) 
                errors.AppendLine("Укажите логин");
            if (string.IsNullOrWhiteSpace(PasswordBox.Password)) 
                errors.AppendLine("Укажите пароль");
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите фамилию");
            if (FioError.Visibility == Visibility.Visible) 
                errors.AppendLine("ФИО содержит недопустимые символы");
            if (EmailError.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(_currentUser.Email)) 
                errors.AppendLine("Email имеет неверный формат");
            if (PhoneError.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(_currentUser.Phone))
                errors.AppendLine("Телефон имеет неверный формат");
            if (SnilsError.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(_currentUser.Snils)) 
                errors.AppendLine("СНИЛС имеет неверный формат");

            if (errors.Length > 0) { MessageBox.Show(errors.ToString()); return; }

            try
            {
                var context = Manufacture_bdEntities1.GetContext();
                if (_currentUser.Id == 0)
                {
                    if (context.Users.Any(u => u.Login == _currentUser.Login))
                    { MessageBox.Show("Пользователь с таким логином уже существует!"); return; }

                    var newUser = new User
                    {
                        Login = _currentUser.Login,
                        Password = HashPassword(PasswordBox.Password),
                        FIO = _currentUser.FIO,
                        Email = _currentUser.Email,
                        Phone = _currentUser.Phone,
                        Snils = _currentUser.Snils,
                        RoleId = _currentUser.RoleId,
                        Blocked = _currentUser.Blocked ?? false
                    };
                    context.Users.Add(newUser);
                }
                else
                {
                    var existingUser = context.Users.Find(_currentUser.Id);
                    if (existingUser != null)
                    {
                        existingUser.Login = _currentUser.Login;
                        existingUser.FIO = _currentUser.FIO;
                        existingUser.Email = _currentUser.Email;
                        existingUser.Phone = _currentUser.Phone;
                        existingUser.Snils = _currentUser.Snils;
                        existingUser.RoleId = _currentUser.RoleId;
                        existingUser.Blocked = _currentUser.Blocked;
                        if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                            existingUser.Password = HashPassword(PasswordBox.Password);
                    }
                }
                context.SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private byte[] HashPassword(string password)
        {
            var context = Manufacture_bdEntities1.GetContext();
            return context.Database.SqlQuery<byte[]>("SELECT HASHBYTES('SHA2_512', @Password)",
                new System.Data.SqlClient.SqlParameter("@Password", password)).FirstOrDefault();
        }

        private void ValidateFio()
        {
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
            {
                FioError.Visibility = Visibility.Collapsed; 
                FioTextBox.BorderBrush = Brushes.Gray; 
                return;
            }
            bool isValid = _currentUser.FIO.All(c => char.IsLetter(c) || c == ' ' || c == '-');
            FioError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            FioTextBox.BorderBrush = isValid ? Brushes.Gray : Brushes.Red;
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(_currentUser.Email))
            {
                EmailError.Visibility = Visibility.Collapsed; EmailTextBox.BorderBrush = Brushes.Gray; 
                return; 
            }
            if (_currentUser.Email.Contains(";"))
            { EmailError.Visibility = Visibility.Visible;
                EmailTextBox.BorderBrush = Brushes.Red;
                return;
            }
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            bool isValid = Regex.IsMatch(_currentUser.Email, emailPattern);
            int atCount = _currentUser.Email.Count(c => c == '@');
            if (atCount != 1) 
                isValid = false;
            EmailError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            EmailTextBox.BorderBrush = isValid ? Brushes.Gray : Brushes.Red;
        }

        private void ValidatePhone()
        {
            if (string.IsNullOrWhiteSpace(_currentUser.Phone))
            {
                PhoneError.Visibility = Visibility.Collapsed; 
                PhoneTextBox.BorderBrush = Brushes.Gray; 
                return;
            }
            string phonePattern = @"^\+7 \d{3} \d{3}-\d{2}-\d{2}$";
            bool isValid = Regex.IsMatch(_currentUser.Phone, phonePattern);
            if (!_currentUser.Phone.StartsWith("+7 ")) isValid = false;
            string digitsOnly = new string(_currentUser.Phone.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length != 11) isValid = false;
            PhoneError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            PhoneTextBox.BorderBrush = isValid ? Brushes.Gray : Brushes.Red;
        }

        private void ValidateSnils()
        {
            if (string.IsNullOrWhiteSpace(_currentUser.Snils))
            {
                SnilsError.Visibility = Visibility.Collapsed; 
                SnilsTextBox.BorderBrush = Brushes.Gray; return; 
            }
            string snilsPattern = @"^\d{3}-\d{3}-\d{3} \d{2}$";
            bool isValid = Regex.IsMatch(_currentUser.Snils, snilsPattern);
            string digitsOnly = new string(_currentUser.Snils.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length != 11) isValid = false;
            SnilsError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            SnilsTextBox.BorderBrush = isValid ? Brushes.Gray : Brushes.Red;
        }

        private void BtnGetFio_Click(object sender, RoutedEventArgs e)
        { 
            LoadDataForField("fullName", result => { _currentUser.FIO = result;
                DataContext = null;
                DataContext = _currentUser;
                ValidateFio();
            }); 
        }
        private void BtnGetEmail_Click(object sender, RoutedEventArgs e) 
        { 
            LoadDataForField("email", result => { _currentUser.Email = result;
                DataContext = null;
                DataContext = _currentUser; ValidateEmail(); 
            }); 
        }
        private void BtnGetPhone_Click(object sender, RoutedEventArgs e) 
        { 
            LoadDataForField("mobilePhone", result => { _currentUser.Phone = result;
                DataContext = null;
                DataContext = _currentUser;
                ValidatePhone();
            });
        }
        private void BtnGetSnils_Click(object sender, RoutedEventArgs e) 
        {
            LoadDataForField("snils", result => { _currentUser.Snils = result;
                DataContext = null;
                DataContext = _currentUser;
                ValidateSnils();
            });
        }
        private void BtnLoad_Click(object sender, RoutedEventArgs e) 
        { 
            BtnGetFio_Click(sender, e);
            BtnGetEmail_Click(sender, e); 
            BtnGetPhone_Click(sender, e); 
            BtnGetSnils_Click(sender, e); 
        }

        private void LoadDataForField(string endpoint, Action<string> setValueAction)
        {
            try
            {
                var client = new WebClient(); client.Encoding = Encoding.UTF8;
                var response = client.DownloadString($"http://demo.vpmt.ru:4444/TransferSimulator/{endpoint}");
                var result = JsonConvert.DeserializeObject<ApiResponse>(response);
                if (result != null && !string.IsNullOrEmpty(result.value)) setValueAction(result.value);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}"); }
        }
    }
}