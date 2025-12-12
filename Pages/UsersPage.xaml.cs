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
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as User));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DGridUsers.SelectedItems.Cast<User>().ToList();

            if (usersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {usersForRemoving.Count} пользователей?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = Manufacture_bdEntities1.GetContext();

                    foreach (var user in usersForRemoving)
                    {
                        var userToDelete = context.Users.Find(user.Id);
                        if (userToDelete != null)
                        {
                            context.Users.Remove(userToDelete);
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные удалены!");

                    DGridUsers.ItemsSource = context.Users.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }


        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Manufacture_bdEntities1.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                DGridUsers.ItemsSource = Manufacture_bdEntities1.GetContext().Users.ToList();
            }
        }
    }
}
