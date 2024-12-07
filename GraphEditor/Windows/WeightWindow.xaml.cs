using System.Windows;

namespace GraphEditor.Windows
{
    /// <summary>
    /// Логика взаимодействия для WeightWindow.xaml
    /// </summary>
    public partial class WeightWindow : Window
    {
        public int NewWeight { get; private set; }

        public WeightWindow(int weight)
        {
            InitializeComponent();
            WeightTextBox.Text = weight.ToString();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WeightTextBox.Text, out int enderedWeight) && enderedWeight > 0)
            {
                NewWeight = enderedWeight;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "Введите корректный положительный вес.", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
        }
    }
}
