using System.Windows;

namespace GraphEditor.Windows
{
    /// <summary>
    /// Логика взаимодействия для IdWindow.xaml
    /// </summary>
    public partial class IdWindow : Window
    {
        private readonly List<int> existingIds;
        public int NewId { get; private set; }

        public IdWindow(int id, IEnumerable<int> ids)
        {
            InitializeComponent();
            IdTextBox.Text = id.ToString();
            existingIds = ids.ToList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(IdTextBox.Text, out int enteredId))
            {
                // Проверка на уникальность ID
                if (!existingIds.Contains(enteredId))
                {
                    NewId = enteredId;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Вершина с таким ID уже существует. Пожалуйста, введите уникальный ID.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    "Введите корректный числовой ID.", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
        }
    }
}
