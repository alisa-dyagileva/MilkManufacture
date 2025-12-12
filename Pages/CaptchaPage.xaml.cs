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
    /// Логика взаимодействия для CaptchaPage.xaml
    /// </summary>
    public partial class CaptchaPage : Page
    {
        private List<Captcha> _captchaFragments = new List<Captcha>();
        private Dictionary<Border, (int row, int col)> _borderPositions = new Dictionary<Border, (int, int)>();

        public bool IsCaptchaPassed { get; private set; } = false;
        public CaptchaPage()
        {
            InitializeComponent();
            LoadNewCaptcha();
        }

        private void LoadNewCaptcha()
        {
            try
            {
                var context = Manufacture_bdEntities1.GetContext();
                var fragmentIds = context.Captchas.Select(c => c.Id_Fragment).Distinct().ToList();
                if (fragmentIds.Count == 0) return;

                Random rnd = new Random();
                int selectedFragmentId = fragmentIds[rnd.Next(fragmentIds.Count)].Value;

                _captchaFragments = context.Captchas
                    .Where(c => c.Id_Fragment == selectedFragmentId)
                    .OrderBy(c => c.Id_Image)
                    .Take(4)
                    .ToList();

                if (_captchaFragments.Count != 4) return;

                _captchaFragments = _captchaFragments.OrderBy(x => rnd.Next()).ToList();
                DisplayCaptchaFragments();
            }
            catch { }
        }

        private void DisplayCaptchaFragments()
        {
            GridCaptcha.Children.Clear();
            GridCaptcha.ColumnDefinitions.Clear();
            GridCaptcha.RowDefinitions.Clear();
            _borderPositions.Clear();

            for (int i = 0; i < 2; i++)
            {
                GridCaptcha.ColumnDefinitions.Add(new ColumnDefinition());
                GridCaptcha.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < _captchaFragments.Count; i++)
            {
                var fragment = _captchaFragments[i];
                int row = i / 2;
                int col = i % 2;

                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    Tag = fragment.Id_Image,
                    Cursor = Cursors.Hand
                };

                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
                border.MouseMove += Border_MouseMove;
                border.AllowDrop = true;
                border.DragEnter += Border_DragEnter;
                border.Drop += Border_Drop;

                Image imageControl = new Image
                {
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Width = 180,
                    Height = 180
                };

                if (fragment.Photo != null && fragment.Photo.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream(fragment.Photo))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        imageControl.Source = bitmap;
                    }
                }

                border.Child = imageControl;
                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                GridCaptcha.Children.Add(border);
                _borderPositions[border] = (row, col);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null) DragDrop.DoDragDrop(border, border.Tag, DragDropEffects.Move);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Border border = sender as Border;
                if (border != null) DragDrop.DoDragDrop(border, border.Tag, DragDropEffects.Move);
            }
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(int))) e.Effects = DragDropEffects.None;
            else e.Effects = DragDropEffects.Move;
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            Border targetBorder = sender as Border;
            if (targetBorder != null && e.Data.GetDataPresent(typeof(int)))
            {
                int draggedImageId = (int)e.Data.GetData(typeof(int));
                Border sourceBorder = _borderPositions.Keys.FirstOrDefault(b => b.Tag is int id && id == draggedImageId);
                if (sourceBorder != null && sourceBorder != targetBorder) SwapBorders(sourceBorder, targetBorder);
            }
        }

        private void SwapBorders(Border border1, Border border2)
        {
            var pos1 = _borderPositions[border1];
            var pos2 = _borderPositions[border2];
            Grid.SetRow(border1, pos2.row);
            Grid.SetColumn(border1, pos2.col);
            Grid.SetRow(border2, pos1.row);
            Grid.SetColumn(border2, pos1.col);
            _borderPositions[border1] = pos2;
            _borderPositions[border2] = pos1;
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            bool isCorrect = true;

            foreach (var kvp in _borderPositions)
            {
                Border border = kvp.Key;
                (int row, int col) = kvp.Value;
                int imageId = (int)border.Tag;
                int expectedPosition = row * 2 + col + 1;
                if (imageId != expectedPosition) { isCorrect = false; break; }
            }

            if (isCorrect)
            {
                IsCaptchaPassed = true;
                MessageBox.Show("Вы успешно авторизовались");
                Manager.MainFrame.Navigate(new UsersPage());
            }
            else
            {
                MessageBox.Show("Неправильно!");
                LoadNewCaptcha();
            }
        }
    }
}