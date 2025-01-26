
using Npgsql;
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
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string role = "User"; // Получаем выбранную роль

            if (password.Length < 6 || password.Length > 12 || !password.Any(char.IsLetter))
            {
                CustomMessageBox.Show("Пароль должен содержать от 6 до 12 символов и хотя бы одну букву.");
                return;
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Проверяем существование пользователя с таким же логином
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                    using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("username", username);
                        int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            CustomMessageBox.Show("Такой пользователь уже существует. Пожалуйста, выберите другой никнейм.");
                            return;
                        }
                    }

                    // Если пользователя с таким логином нет, выполняем регистрацию
                    string query = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@username, @passwordHash, @role)";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("passwordHash", HashPassword(password)); // Здесь тоже нужно хешировать пароль
                        cmd.Parameters.AddWithValue("role", role); // Добавляем роль
                        cmd.ExecuteNonQuery();
                    }
                }

                CustomMessageBox.Show("Вы успешно зарегестрировались!");
                this.Close(); 
            }
            else
            {
                CustomMessageBox.Show("Пожалуйста, заполните все поля дял ввода.");
            }
        }



        private string HashPassword(string password)
        {
            // Добавь здесь логику хеширования пароля
            return password; // Временно без хеширования
        }
    }

}