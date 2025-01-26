using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Npgsql;
using System.Windows.Controls;
using BurinXApp.Model;
using static BurinXApp.AdminWindow;

namespace BurinXApp
{
    public partial class BookingWindow : Window
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";
        private decimal userBalance;
        private int currentUserID;
        private int selectedComputerID;
        private decimal totalCost;
        private int totalMinutes; // Общее время бронирования в минутах

        public BookingWindow(int userID, int computerID)
        {
            InitializeComponent();
            currentUserID = userID;
            selectedComputerID = computerID;
            LoadUserBalance();
            LoadPackages();
        }

        private void LoadUserBalance()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Balance FROM Users WHERE UserID = @userID";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("userID", currentUserID);
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        userBalance = (decimal)result;
                    }
                }
            }
            BalanceTextBlock.Text = $"${userBalance:F2}";
        }

        private void LoadPackages()
        {
            List<SpecialOffer> specialOffers = new List<SpecialOffer>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT OfferID, Name, WeekdayPrice, WeekendPrice, DurationInMinutes, IsFixedDuration FROM SpecialOffers";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            specialOffers.Add(new SpecialOffer
                            {
                                OfferID = (int)reader["OfferID"],
                                Name = reader["Name"].ToString(),
                                WeekdayPrice = (decimal)reader["WeekdayPrice"],
                                WeekendPrice = (decimal)reader["WeekendPrice"],
                                DurationInMinutes = (int)reader["DurationInMinutes"],
                                IsFixedDuration = (bool)reader["IsFixedDuration"]
                            });
                        }
                    }
                }
            }

            foreach (var offer in specialOffers)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = offer.Name,
                    Tag = offer
                };
                PackageComboBox.Items.Add(item);
            }
        }

        private void UpdateTotalCost()
        {
            totalCost = 0;
            totalMinutes = 0;

            if (PackageComboBox.SelectedItem != null)
            {
                var selectedOffer = (SpecialOffer)((ComboBoxItem)PackageComboBox.SelectedItem).Tag;
                decimal hourlyPrice = GetHourlyPrice(selectedOffer);

                if (selectedOffer.IsFixedDuration) // Для фиксированного пакета
                {
                    totalCost = hourlyPrice;
                    totalMinutes = selectedOffer.DurationInMinutes;

                    PackageInfoTextBlock.Text = $"Selected Package: {selectedOffer.Name} (Fixed: {selectedOffer.DurationInMinutes / 60} hrs)";
                    HoursTextBox.Text = " ";
                    HoursTextBox.IsEnabled = false; // Отключаем ввод часов
                }
                else // Для пакетов с пользовательским временем
                {
                    HoursTextBox.IsEnabled = true;

                    if (int.TryParse(HoursTextBox.Text, out int additionalHours) && additionalHours > 0)
                    {
                        totalCost = hourlyPrice * additionalHours;
                        totalMinutes = additionalHours * 60;

                        PackageInfoTextBlock.Text = $"Selected Package: {selectedOffer.Name} (Custom Duration)";
                    }
                    else
                    {
                        PackageInfoTextBlock.Text = "Enter a valid number of hours.";
                    }
                }
            }

            TotalCostTextBlock.Text = $"Total Cost: ${totalCost:F2}";
        }


        private decimal GetHourlyPrice(SpecialOffer offer)
        {
            var currentDay = DateTime.Now.DayOfWeek;

            // Определяем цену в зависимости от дня недели
            return currentDay == DayOfWeek.Saturday || currentDay == DayOfWeek.Sunday
                ? offer.WeekendPrice
                : offer.WeekdayPrice;
        }

        private void PackageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void HoursTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка наличия выбранного пользователя и компьютера
                if (currentUserID <= 0)
                {
                    CustomMessageBox.Show("Выберите пользователя для бронирования.");
                    return;
                }

                if (selectedComputerID <= 0)
                {
                    CustomMessageBox.Show("Выберите компьютер для бронирования.");
                    return;
                }

                // Проверка на положительное значение стоимости
                if (totalCost <= 0)
                {
                    CustomMessageBox.Show("Невозможно забронировать с нулевой или отрицательной стоимостью.");
                    return;
                }

                // Проверка баланса
                UpdateTotalCost();
                if (totalCost > userBalance)
                {
                    CustomMessageBox.Show("Недостаточно средств на балансе.");
                    return;
                }

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Проверка на доступность компьютера
                            string checkComputerQuery = "SELECT Status FROM Computers WHERE ComputerID = @computerID";
                            using (var checkCmd = new NpgsqlCommand(checkComputerQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("computerID", selectedComputerID);
                                string computerStatus = checkCmd.ExecuteScalar()?.ToString();

                                if (computerStatus == "Occupied" || computerStatus == "Reserved")
                                {
                                    MessageBox.Show("Выбранный компьютер уже занят или забронирован.");
                                    transaction.Rollback();
                                    return;
                                }
                            }

                            // Уменьшение баланса пользователя
                            string updateBalanceQuery = "UPDATE Users SET Balance = Balance - @totalCost WHERE UserID = @userID";
                            using (var cmd = new NpgsqlCommand(updateBalanceQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("totalCost", totalCost);
                                cmd.Parameters.AddWithValue("userID", currentUserID);
                                cmd.ExecuteNonQuery();
                            }

                            // Создание записи в таблице сессий
                            DateTime sessionStart = DateTime.Now;
                            DateTime sessionEnd = sessionStart.AddMinutes(totalMinutes);

                            string sessionQuery = "INSERT INTO Sessions (UserID, ComputerID, StartTime, EndTime, Cost) VALUES (@userID, @computerID, @startTime, @endTime, @cost)";
                            using (var cmd = new NpgsqlCommand(sessionQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("userID", currentUserID);
                                cmd.Parameters.AddWithValue("computerID", selectedComputerID);
                                cmd.Parameters.AddWithValue("startTime", sessionStart);
                                cmd.Parameters.AddWithValue("endTime", sessionEnd);
                                cmd.Parameters.AddWithValue("cost", totalCost);
                                cmd.ExecuteNonQuery();
                            }

                            // Обновление статуса компьютера
                            string updateComputerQuery = "UPDATE Computers SET Status = 'Reserved', CurrentUserID = @userID, ReservationEndTime = @endTime WHERE ComputerID = @computerID";
                            using (var cmd = new NpgsqlCommand(updateComputerQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("endTime", sessionEnd);
                                cmd.Parameters.AddWithValue("userID", currentUserID);
                                cmd.Parameters.AddWithValue("computerID", selectedComputerID);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            CustomMessageBox.Show($"Бронирование успешно завершено! Ваш новый баланс: ${userBalance - totalCost:F2}");

                            // Обновление ListView с компьютерами
                            UpdateComputerListView();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            CustomMessageBox.Show($"Ошибка бронирования: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private void UpdateComputerListView()
        {
            // Пример обновления ListView, в зависимости от вашей реализации
            if (Application.Current.Windows.OfType<ClientWindow>().FirstOrDefault() is ClientWindow clientWindow)
            {
                clientWindow.LoadAvailableComputers(); // Перезагрузка данных о компьютерах
                clientWindow.LoadUserBalance();
                clientWindow.LoadUserStatistics();
            }
        }
    }
}
