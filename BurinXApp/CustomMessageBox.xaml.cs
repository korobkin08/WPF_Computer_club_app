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
using System.Windows.Shapes;

namespace BurinXApp
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message; // Устанавливаем текст сообщения
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Закрываем окно
        }

        // Статический метод для вызова кастомного MessageBox
        public static void Show(string message)
        {
            var messageBox = new CustomMessageBox(message);
            messageBox.ShowDialog();
        }
        public static void Show(string message, string title = "Message")
        {
            var messageBox = new CustomMessageBox(message)
            {
                Title = title // Устанавливаем заголовок окна
            };
            messageBox.ShowDialog();
        }
    }
}

