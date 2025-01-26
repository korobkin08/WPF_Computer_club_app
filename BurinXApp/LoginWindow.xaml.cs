using BurinXApp.Observer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BurinXApp.Observer;

namespace BurinXApp
{
    public partial class LoginWindow : Window
    {
        private TimeSubject timeSubject;
        private TimeObserver timeObserver;

        public LoginWindow()
        {
            InitializeComponent();

            // Инициализация TimeSubject и TimeObserver
            timeSubject = new TimeSubject();
            timeObserver = new TimeObserver(TimeTextBlock);

            // Регистрация наблюдателя
            timeSubject.RegisterObserver(timeObserver);

            // Установка таймера для обновления времени каждую секунду
            var timer = new DispatcherTimer();
            timer.Tick += (s, e) => UpdateTime();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void UpdateTime()
        {
            string currentTime = DateTime.Now.ToString("HH:mm");
            timeSubject.SetTime(currentTime); // Обновление времени и уведомление наблюдателей
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var user = AuthService.Instance.AuthenticateUser(username, password);

            if (user != null)
            {
                if (user.Role == "Admin")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                }
                else
                {
                    ClientWindow clientWindow = new ClientWindow(user);
                    clientWindow.Show();
                }

                this.Close();
            }
            else
            {
                CustomMessageBox.Show("Введите корректные значения для входа :(");
                return;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
        }

        private void RegisterTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
        }
    }
}
