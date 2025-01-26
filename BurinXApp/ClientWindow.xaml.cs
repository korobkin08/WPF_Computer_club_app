using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Windows.Media;
using BurinXApp.Model;
using System.Globalization;
using System.Windows.Data;

namespace BurinXApp
{
    public partial class ClientWindow : Window
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";
        private decimal userBalance = 0; 
        private int currentUserID;
        private string UserNickname;

        public ClientWindow(UserModel user)
        {
            InitializeComponent();
            currentUserID = user.UserID; // Сохраняем ID текущего пользователя
            UserNickname = user.Username; // Получаем никнейм пользователя
            WelcomeTextBlock.Text = $"Добро пожаловать, {UserNickname}"; // Устанавливаем текст
            LoadUserBalance();
            LoadUserStatistics();
            LoadAvailableComputers();
            LoadSessionHistory();
            this.WindowState = WindowState.Maximized;
        }

        public void LoadUserBalance()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT Balance FROM Users WHERE UserID = @userID";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("userID", currentUserID); // Используем ID текущего пользователя
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        userBalance = (decimal)result;
                    }
                }
            }

            // Обновление метки с балансом
            BalanceLabel.Text = $"${userBalance.ToString("F2")}";
        }

        // Загрузка доступных компьютеров
        public void LoadAvailableComputers()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                var computers = new List<ComputerModel>();
                string query = "SELECT * FROM Computers";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            computers.Add(new ComputerModel
                            {
                                ComputerID = (int)reader["ComputerID"],
                                Name = reader["Name"].ToString(),
                                Status = reader["Status"].ToString(),
                                ReservationEndTime = reader.IsDBNull(reader.GetOrdinal("ReservationEndTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ReservationEndTime"))
                            });
                        }
                    }
                }

                // Отображение компьютеров в ListView
                ComputersListView.ItemsSource = computers;
            }
        }

        // Логика для бронирования компьютера


        private void OnBookComputerClick(object sender, RoutedEventArgs e)
        {
            var selectedComputer = (ComputerModel)((Button)sender).Tag;

            // Логика бронирования компьютера
            if (selectedComputer.Status == "Free")
            {
                // Открытие окна для выбора пакета игрового времени, передаем ID текущего пользователя
                BookingWindow bookingWindow = new BookingWindow(currentUserID, selectedComputer.ComputerID);
                bookingWindow.ShowDialog();
            }
            else
            {
                CustomMessageBox.Show("К сожалению этот ПК уже забронирован.");
                return;
            }
        }

        private void OnTopUpClick(object sender, RoutedEventArgs e)
        {
            // Получаем введенную сумму
            if (decimal.TryParse(TopUpAmountTextBox.Text, out decimal topUpAmount) && topUpAmount > 0)
            {
                // Обновляем баланс в базе данных
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Users SET Balance = Balance + @topUpAmount WHERE UserID = @userID";
                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("topUpAmount", topUpAmount);
                        cmd.Parameters.AddWithValue("userID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Загружаем обновленный баланс
                LoadUserBalance();

                // Очищаем поле ввода
                TopUpAmountTextBox.Clear();
                MessageBox.Show("Balance updated successfully!");
            }
            else
            {
                MessageBox.Show("Invalid amount. Please enter a valid number.");
            }
        }


        public class UserStatisticsModel
        {
            public int TotalSessions { get; set; }
            public double TotalHoursSpent { get; set; }
            public decimal TotalPayments { get; set; }
   
        }
        public void LoadUserStatistics()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    // Получаем количество сессий
                    string sessionQuery = "SELECT COUNT(*) FROM Sessions WHERE UserID = @userID";
                    using (var cmd = new NpgsqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", currentUserID);
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            int totalSessions = Convert.ToInt32(result);
                            TotalSessionsLabel.Text = $"Общее количество сессий: {totalSessions}";
                        }
                    }

                    // Получаем общее количество часов, играя между временем начала и окончания сессий
                    string hoursQuery = @"
                SELECT SUM(EXTRACT(EPOCH FROM (EndTime - StartTime)) / 3600) 
                FROM Sessions 
                WHERE UserID = @userID AND EndTime IS NOT NULL AND StartTime IS NOT NULL";
                    using (var cmd = new NpgsqlCommand(hoursQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", currentUserID);
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            decimal totalHours = Convert.ToDecimal(result);
                            TotalHoursLabel.Text = $"Часов в ПК проведено: {totalHours:F2} hours";
                        }
                    }

                    // Получаем количество платежей
                    string paymentsQuery = "SELECT SUM(Cost) FROM Sessions WHERE UserID = @userID";
                    using (var cmd = new NpgsqlCommand(paymentsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", currentUserID);
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            int totalPayments = Convert.ToInt32(result);
                            TotalPaymentsLabel.Text = $"Общая сумма депозита: {totalPayments}";
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading user statistics: {ex.Message}");
                }
            }
        }

        public void LoadSessionHistory()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    // Получаем сессии текущего пользователя
                    string query = @"
                SELECT SessionID, ComputerID, StartTime, EndTime, Cost 
                FROM Sessions 
                WHERE UserID = @userID
                ORDER BY StartTime DESC";

                    var sessions = new List<Session>();

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("userID", currentUserID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sessions.Add(new Session
                                {
                                    SessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                                    ComputerID = reader.GetInt32(reader.GetOrdinal("ComputerID")),
                                    StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("EndTime")),
                                    Cost = reader.GetDecimal(reader.GetOrdinal("Cost"))
                                });
                            }
                        }
                    }

                    // Привязка к ListView
                    SessionHistoryListView.ItemsSource = sessions;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading session history: {ex.Message}");
                }
            }
        }

        // Класс модели для отображения сессий
        public class Session
        {
            public int SessionID { get; set; }
            public int ComputerID { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal Cost { get; set; }
        }



        private void OnEndSessionClick(object sender, RoutedEventArgs e)
        {
            // Окно подтверждения завершения сессии
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите закончить все ваши сессии?",
                "Подтверждение завершения сессии",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Обновление статуса всех компьютеров для текущего пользователя на "Free" и очистка времени бронирования
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        // Запрос для обновления статуса и очистки времени бронирования
                        string updateQuery = @"
                    UPDATE Computers 
                    SET Status = 'Free', ReservationStartTime = NULL, currentuserid = NULL,  ReservationEndTime = NULL 
                    WHERE currentUserID = @currentUserID";

                        using (var cmd = new NpgsqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("currentUserID", currentUserID);
                            cmd.ExecuteNonQuery(); // Выполнение запроса
                        }
                    }
                    // Переход на окно LoginWindow
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    // Закрытие текущего окна (например, клиентского окна)
                    this.Close();

                    
                }
                catch (Exception ex)
                {
                    // Обработка ошибок
                    MessageBox.Show($"Ошибка при завершении сессии: {ex.Message}");
                }
            }
        }

    }
}