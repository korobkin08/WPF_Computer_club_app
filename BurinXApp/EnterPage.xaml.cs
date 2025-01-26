using System.Windows;
using System.Windows.Input;

namespace BurinXApp
{
    public partial class EnterPage : Window
    {
        public EnterPage()
        {
            InitializeComponent();
        }

        // Обработчик нажатия клавиши Enter
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Если нажата клавиша Enter
            {
                // Создаем и открываем окно LoginWindow
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрываем текущее окно EnterPage
                this.Close();
            }
        }
    }
}
