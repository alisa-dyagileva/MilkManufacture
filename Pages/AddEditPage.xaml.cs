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

namespace MilkManufacture.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private User _currentUser = new User();
        public AddEditPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
            {
                _currentUser = selectedUser;
            }
            else
            {
                _currentUser = new User();
                _currentUser.Blocked = false;
            }

            DataContext = _currentUser;
            ComboRole.ItemsSource = Manufacture_bdEntities1.GetContext().Roles.ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин");

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                errors.AppendLine("Укажите пароль");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                var context = Manufacture_bdEntities1.GetContext();

                if (_currentUser.Id == 0) 
                {
                    if (context.Users.Any(u => u.Login == _currentUser.Login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }

                    string plainPassword = PasswordTextBox.Text;
                    var passwordHash = HashPassword(plainPassword);

                    var newUser = new User
                    {
                        Login = _currentUser.Login,
                        Password = passwordHash,
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
                        existingUser.RoleId = _currentUser.RoleId;
                        existingUser.Blocked = _currentUser.Blocked;

                        // Если введен новый пароль - обновляем
                        if (!string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                        {
                            string newPassword = PasswordTextBox.Text;
                            var newHash = HashPassword(newPassword);
                            existingUser.Password = newHash;
                        }
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private byte[] HashPassword(string password)
        {
            var context = Manufacture_bdEntities1.GetContext();
            return context.Database.SqlQuery<byte[]>(
                "SELECT HASHBYTES('SHA2_512', @Password)",
                new System.Data.SqlClient.SqlParameter("@Password", password))
                .FirstOrDefault();
        }
    }
}