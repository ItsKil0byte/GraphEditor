using System.Windows;

namespace GraphEditor.Windows
{
    /// <summary>
    /// Логика взаимодействия для WeightWindow.xaml
    /// </summary>
    public partial class WeightWindow : Window
    {
        public int NewWeight { get; private set; }
        public int NewCapacity { get; private set; }

        public bool NewDirection { get; set; }

        public bool isDirectionShowed { get; set; }

        public WeightWindow(int weight, int capacity, bool showed, bool direction)
        {
            InitializeComponent();
            WeightTextBox.Text = weight.ToString();
            CapacityTextBox.Text = capacity.ToString();
            InvertCheckBox.IsChecked = direction;
            ShowDirectionCheckBox.IsChecked = showed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WeightTextBox.Text, out int enderedWeight) && enderedWeight > 0 && int.TryParse(CapacityTextBox.Text, out int enderedCapacity) && enderedCapacity >= 0)
            {
                NewWeight = enderedWeight;
                NewCapacity = enderedCapacity;
                NewDirection = (bool)InvertCheckBox.IsChecked;
                isDirectionShowed = (bool)ShowDirectionCheckBox.IsChecked;

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
