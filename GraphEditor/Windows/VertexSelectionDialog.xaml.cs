using System;
using System.Windows;

namespace GraphEditor
{
    public partial class VertexSelectionDialog : Window
    {
        public int? StartVertexId { get; private set; }
        public int? EndVertexId { get; private set; }

        public VertexSelectionDialog()
        {
            InitializeComponent(); // Убедитесь, что этот метод вызывается
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(StartVertexIdTextBox.Text, out int startId) &&
                int.TryParse(EndVertexIdTextBox.Text, out int endId))
            {
                StartVertexId = startId;
                EndVertexId = endId;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные ID вершин.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}