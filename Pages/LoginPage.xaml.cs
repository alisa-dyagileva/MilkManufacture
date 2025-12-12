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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private static Dictionary<int, int> _failedAttempts = new Dictionary<int, int>();
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TBoxLogin.Text.Trim();
            string password = PBoxPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            try
            {
                var currentUser = App.Context.Users
                    .FirstOrDefault(p => p.Login == login);

                if (currentUser == null)
                {
                    ShowGenericError();
                    return;
                }

                if (currentUser.Blocked == true)
                {
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                bool isPasswordValid = CheckPasswordInDatabase(login, password);

                if (isPasswordValid)
                {
                    if (_failedAttempts.ContainsKey(currentUser.Id))
                    {
                        _failedAttempts.Remove(currentUser.Id);
                    }

                    App.CurrentUser = currentUser;
                    NavigationService.Navigate(new CaptchaPage());
                }
                else
                {
                    HandleFailedLogin(currentUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}");
            }
        }

        private bool CheckPasswordInDatabase(string login, string password)
        {
            try
            {
                var result = App.Context.Database.SqlQuery<int?>(
                    "SELECT CASE WHEN Password = HASHBYTES('SHA2_512', @Password) THEN 1 ELSE 0 END " +
                    "FROM Users WHERE Login = @Login",
                    new System.Data.SqlClient.SqlParameter("@Login", login),
                    new System.Data.SqlClient.SqlParameter("@Password", password))
                    .FirstOrDefault();

                return result == 1;
            }
            catch
            {
                return false;
            }
        }

        private void HandleFailedLogin(User user)
        {
            if (!_failedAttempts.ContainsKey(user.Id))
            {
                _failedAttempts[user.Id] = 0;
            }

            _failedAttempts[user.Id]++;

            if (_failedAttempts[user.Id] >= 3)
            {
                user.Blocked = true;
                App.Context.SaveChanges();

                _failedAttempts.Remove(user.Id);

                MessageBox.Show("Аккаунт заблокирован после 3 неудачных попыток входа. Обратитесь к администратору для разблокировки.",
                    "Блокировка", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                int remainingAttempts = 3 - _failedAttempts[user.Id];

                if (remainingAttempts > 0)
                {
                    MessageBox.Show($"Неверный логин или пароль. Осталось попыток: {remainingAttempts}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    ShowGenericError();
                }
            }
        }

        private void ShowGenericError()
        {
            MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}