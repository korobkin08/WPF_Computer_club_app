using Npgsql;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LiveCharts;
using LiveCharts.Wpf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using BurinXApp.Model;



namespace BurinXApp
{
    public partial class AdminWindow : Window
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadComputers();
            this.WindowState = WindowState.Maximized;
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(ManageUsersPage);
        }

        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(SettingsPage);
            RefreshSpecialOffersTable();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(HomePage);
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }


        private void ShowPage(UIElement page)
        {
            // Скрываем все страницы
            HomePage.Visibility = Visibility.Collapsed;
            ManageUsersPage.Visibility = Visibility.Collapsed;
            StatisticsPage.Visibility = Visibility.Collapsed;
            SettingsPage.Visibility = Visibility.Collapsed;

            // Показываем текущую страницу
            page.Visibility = Visibility.Visible;
        }

        private void LoadUsers()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var users = new List<UserModel>();

                    string query = "SELECT * FROM Users";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new UserModel
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Balance = (decimal)reader["Balance"],
                                    Role = reader["Role"].ToString()
                                });
                            }
                        }
                    }

                    UsersTable.ItemsSource = users; // Обновляем данные в таблице
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        private void UsersTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, выбран ли элемент
            if (UsersTable.SelectedItem is UserModel selectedUser)
            {
                // Заполняем TextBox-ы и ComboBox
                UsernameTextBox.Text = selectedUser.Username;
                BalanceTextBox.Text = selectedUser.Balance.ToString();

                // Устанавливаем выбранное значение ComboBox на основании роли
                foreach (ComboBoxItem item in RoleComboBox.Items)
                {
                    if (item.Content.ToString() == selectedUser.Role)
                    {
                        RoleComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // Добавление нового пользователя
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    // Проверка существования пользователя
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                    using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("username", UsernameTextBox.Text);
                        int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким именем уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Добавление нового пользователя
                    string query = "INSERT INTO Users (Username, Passwordhash, Balance, Role) VALUES (@username, @password, @balance, @role)";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", UsernameTextBox.Text);
                        cmd.Parameters.AddWithValue("password", PasswordTextBox.Text);
                        cmd.Parameters.AddWithValue("balance", decimal.Parse(BalanceTextBox.Text));
                        cmd.Parameters.AddWithValue("role", (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                        cmd.ExecuteNonQuery();
                    }
                }

                CustomMessageBox.Show("Пользователь успешно добавлен!");
                ClearInputFields();
                LoadUsers(); // Обновляем таблицу
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактирование выбранного пользователя
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersTable.SelectedItem is UserModel selectedUser)
            {
                try
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        // Правильный синтаксис для обновления данных
                        string query = "UPDATE Users SET Username = @username, Passwordhash = @password, Balance = @balance, Role = @role WHERE UserID = @userid";
                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            // Добавляем параметры запроса
                            cmd.Parameters.AddWithValue("username", UsernameTextBox.Text);
                            cmd.Parameters.AddWithValue("password", PasswordTextBox.Text);
                            cmd.Parameters.AddWithValue("balance", decimal.Parse(BalanceTextBox.Text));
                            cmd.Parameters.AddWithValue("role", (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                            cmd.Parameters.AddWithValue("userid", selectedUser.UserID);  // Обновление по ID пользователя

                            // Выполняем запрос
                            cmd.ExecuteNonQuery();
                        }
                    }

                    CustomMessageBox.Show("Пользователь успешно обновлён!");
                    ClearInputFields();
                    LoadUsers(); // Обновляем таблицу
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                }
            }
            else
            {
                CustomMessageBox.Show("Выберите пользователя!");
            }
        }

        // Удаление выбранного пользователя
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersTable.SelectedItem is UserModel selectedUser)
            {
                try
                {
                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();

                        string query = "DELETE FROM Users WHERE UserID = @userid";
                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("userid", selectedUser.UserID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    CustomMessageBox.Show("Пользователь успешно удалён!");
                    LoadUsers(); // Обновляем таблицу
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show($"Ошибка удаления пользователя: {ex.Message}");
                }
            }
            else
            {
                CustomMessageBox.Show("Выберите пользователя для удаления.");
            }
        }

        // Поиск пользователей
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    var users = new List<UserModel>();

                    string query = "SELECT * FROM Users WHERE LOWER(Username) LIKE @searchTerm";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("searchTerm", $"%{SearchTextBox.Text.ToLower()}%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new UserModel
                                {
                                    UserID = (int)reader["UserID"],
                                    Username = reader["Username"].ToString(),
                                    Balance = (decimal)reader["Balance"],
                                    Role = reader["Role"].ToString()
                                });
                            }
                        }
                    }

                    UsersTable.ItemsSource = users; // Обновляем данные в таблице
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error searching users: {ex.Message}");
            }
        }
        private void ShowUserStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersTable.SelectedItem as UserModel;

            if (selectedUser != null)
            {
                DisplayUserStatistics(selectedUser.UserID);
            }
            else
            {
                MessageBox.Show("Please select a user first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DisplayUserStatistics(int userId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    StringBuilder statistics = new StringBuilder("User Statistics:\n\n");

                    // Общее количество сеансов
                    string sessionQuery = "SELECT COUNT(*) FROM sessions WHERE userid = @userId";
                    using (var cmd = new NpgsqlCommand(sessionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        var result = cmd.ExecuteScalar();
                        var sessionCount = result != DBNull.Value ? (long)result : 0;
                        statistics.AppendLine($"- Total Sessions: {sessionCount}");
                    }

                    // Общее время за компьютером
                    string timeQuery = "SELECT SUM(EXTRACT(EPOCH FROM (endtime - starttime))) / 3600 FROM sessions WHERE userid = @userId";
                    using (var cmd = new NpgsqlCommand(timeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        var result = cmd.ExecuteScalar();
                        var totalHours = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
                        statistics.AppendLine($"- Total Hours Spent: {totalHours:F2} hours");
                    }

                    // История бронирований
                    string sessionsQuery = @"SELECT s.SessionID,s.ComputerID,s.StartTime,s.EndTime,s.Cost
                    FROM Sessions s
                   
                    WHERE s.UserID = @userId";

                    using (var cmd = new NpgsqlCommand(sessionsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            statistics.AppendLine("\nSession History:");
                            while (reader.Read())
                            {
                                int sessionId = reader.GetInt32(0);
                                int computerId = reader.GetInt32(1);
                                DateTime startTime = reader.GetDateTime(2);
                                DateTime endTime = reader.GetDateTime(3);
                                decimal cost = reader.GetDecimal(4);
                                

                                statistics.AppendLine($"  - Session ID: {sessionId}, Computer ID: {computerId}, Start: {startTime}, End: {endTime}, Cost: {cost:C}");
                            }
                        }
                    }

                    // Показываем статистику в MessageBox
                    MessageBox.Show(statistics.ToString(), "User Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Ошибка получения статистики: {ex.Message}");
            }
        }

        // Очистка полей ввода
        private void ClearInputFields()
        {
            UsernameTextBox.Text = "";
            BalanceTextBox.Text = "";
            PasswordTextBox.Text = "";
            RoleComboBox.SelectedItem = null;
        }

        private void ViewStatistics_ClickStat(object sender, RoutedEventArgs e)
        {
            // Переход на страницу статистики
            HomePage.Visibility = Visibility.Collapsed;
            ManageUsersPage.Visibility = Visibility.Collapsed;
            StatisticsPage.Visibility = Visibility.Visible;
            SettingsPage.Visibility = Visibility.Collapsed;

            // Загрузка статистики за сегодняшний день
            LoadStatistics("today");
        }

        private void Back_ClickStat(object sender, RoutedEventArgs e)
        {
            // Возврат к управлению пользователями
            StatisticsPage.Visibility = Visibility.Collapsed;
            HomePage.Visibility = Visibility.Visible;
        }

        private void TimePeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimePeriodComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string period = selectedItem.Content.ToString().ToLower();
                LoadStatistics(period);
            }
        }
        private void LoadStatistics(string period)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    string query = period switch
                    {
                        "today" => "SELECT * FROM get_statistics('today')",
                        "this month" => "SELECT * FROM get_statistics('month')",
                        "this year" => "SELECT * FROM get_statistics('year')",
                        _ => throw new ArgumentException("Invalid period")
                    };

                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                TotalSessionsTextBlock.Text = reader["total_sessions"].ToString();
                                TotalIncomeTextBlock.Text = $"{reader["total_income"]:C}";
                                var statistics = new List<object>();

                                do
                                {
                                    statistics.Add(new
                                    {
                                        Date = reader["date"],
                                        Sessions = reader["sessions"],
                                        Income = reader["income"]
                                    });
                                } while (reader.Read());

                                StatisticsDataGrid.ItemsSource = statistics;
                            }
                        }
                        else
                        {

                            TotalIncomeTextBlock.Text = "";
                            TotalSessionsTextBlock.Text = "";
                        }
                    }
                    GenerateChart(period);
                }
            }
            catch (Exception ex)
            {
            }
        }



        private void GenerateChart(string period)
        {
            try
            {
                // Определяем начальную и конечную дату в зависимости от выбранного периода
                DateTime startDate;
                DateTime endDate;

                switch (period)
                {
                    case "today":
                        startDate = DateTime.Today;
                        endDate = DateTime.Today.AddDays(1).AddTicks(-1);  // Конец дня
                        break;
                    case "this month":
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endDate = startDate.AddMonths(1).AddTicks(-1);  // Конец месяца
                        break;
                    case "this year":
                        startDate = new DateTime(DateTime.Now.Year, 1, 1);
                        endDate = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);  // Конец года
                        break;
                    default:
                        throw new ArgumentException("Invalid period");
                }

                string query = @"
            SELECT 
                s.SessionID, 
                s.StartTime, 
                s.EndTime, 
                s.Cost,
                DATE_TRUNC('hour', s.StartTime) AS period
            FROM Sessions s
            LEFT JOIN Users u ON u.UserID = s.UserID
            LEFT JOIN Payments p ON p.UserID = u.UserID
            WHERE s.StartTime >= @startDate AND s.StartTime <= @endDate
            ORDER BY s.StartTime";

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (var reader = cmd.ExecuteReader())
                        {
                            var sessionData = new Dictionary<string, ChartPoint>();

                            while (reader.Read())
                            {
                                var periodKey = reader["period"].ToString();
                                var cost = Convert.ToDecimal(reader["cost"]);

                                if (!sessionData.ContainsKey(periodKey))
                                {
                                    sessionData[periodKey] = new ChartPoint
                                    {
                                        Period = periodKey,
                                        TotalSessions = 0,
                                        TotalIncome = 0
                                    };
                                }

                                sessionData[periodKey].TotalSessions += 1;
                                sessionData[periodKey].TotalIncome += cost;
                            }

                            // Переносим данные в список для графика
                            var chartPoints = sessionData.Values.ToList();

                            // Создаем график
                            var incomeChart = new ColumnSeries
                            {
                                Title = "Доход",
                                Values = new ChartValues<double>(chartPoints.Select(p => (double)p.TotalIncome)),
                                DataLabels = true,
                                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.CornflowerBlue),
                                LabelPoint = point => point.Y.ToString("C")
                            };

                            var sessionChart = new LineSeries
                            {
                                Title = "Сессии",
                                Values = new ChartValues<int>(chartPoints.Select(p => p.TotalSessions)),
                                PointGeometry = null,
                                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange),
                                LineSmoothness = 0.2,
                                DataLabels = true,
                                LabelPoint = point => point.Y.ToString()
                            };

                            // Обновляем диаграмму
                            var chart = new CartesianChart
                            {
                                Series = new SeriesCollection
                        {
                            incomeChart,
                            sessionChart
                        },
                                AxisX = new AxesCollection
                        {
                            new Axis
                            {
                                Title = "Период",
                                Labels = chartPoints.Select(p => p.Period).ToArray(),
                            }
                        },
                                AxisY = new AxesCollection
                        {
                            new Axis
                            {
                                Title = "Сумма / Сессии",
                                LabelFormatter = value => value.ToString("C"),
                                MinValue = 0,
                                MaxValue = (double)(chartPoints.Max(p => p.TotalIncome) + 10)
                            }
                        },
                                LegendLocation = LegendLocation.Right,
                                Hoverable = true
                            };

                            // Очищаем контейнер и добавляем график
                            ChartContainer.Children.Clear();
                            ChartContainer.Children.Add(chart);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка при генерации графика: {ex.Message}");
                ChartContainer.Children.Clear();
            }
        }

        public class ChartPoint
        {
            public string Period { get; set; }
            public int TotalSessions { get; set; }
            public decimal TotalIncome { get; set; }
        }

        private void AddOffer_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно добавления нового предложения
            var addOfferWindow = new AddEditOfferWindow();
            if (addOfferWindow.ShowDialog() == true)
            {
                // Получаем новое предложение и добавляем его в базу данных
                var newOffer = addOfferWindow.Offer;
                AddOfferToDatabase(newOffer);
                RefreshSpecialOffersTable();
            }
        }

        private void EditOffer_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранное предложение
            var selectedOffer = SpecialOffersTable.SelectedItem as SpecialOffer;
            if (selectedOffer == null)
            {
                MessageBox.Show("Please select an offer to edit.");
                return;
            }

            // Открываем окно редактирования предложения
            var editOfferWindow = new AddEditOfferWindow(selectedOffer);
            if (editOfferWindow.ShowDialog() == true)
            {
                // Обновляем предложение в базе данных
                var updatedOffer = editOfferWindow.Offer;
                UpdateOfferInDatabase(updatedOffer);
                RefreshSpecialOffersTable();
            }
        }

        private void DeleteOffer_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранное предложение
            var selectedOffer = SpecialOffersTable.SelectedItem as SpecialOffer;
            if (selectedOffer == null)
            {
                MessageBox.Show("Please select an offer to delete.");
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show($"Are you sure you want to delete the offer '{selectedOffer.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteOfferFromDatabase(selectedOffer.OfferID);
                RefreshSpecialOffersTable();
            }
        }
        private void AddOfferToDatabase(SpecialOffer offer)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(
                    "INSERT INTO SpecialOffers (Name, WeekdayPrice, WeekendPrice, DurationInMinutes, Description) " +
                    "VALUES (@Name, @WeekdayPrice, @WeekendPrice, @DurationInMinutes, @Description)", connection);

                command.Parameters.AddWithValue("@Name", offer.Name);
                command.Parameters.AddWithValue("@WeekdayPrice", offer.WeekdayPrice);
                command.Parameters.AddWithValue("@WeekendPrice", offer.WeekendPrice);
                command.Parameters.AddWithValue("@DurationInMinutes", offer.DurationInMinutes);
                command.Parameters.AddWithValue("@Description", (object)offer.Description ?? DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        private void UpdateOfferInDatabase(SpecialOffer offer)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand(
                    "UPDATE SpecialOffers SET Name = @Name, WeekdayPrice = @WeekdayPrice, " +
                    "WeekendPrice = @WeekendPrice, DurationInMinutes = @DurationInMinutes, Description = @Description " +
                    "WHERE OfferID = @OfferID", connection);

                command.Parameters.AddWithValue("@OfferID", offer.OfferID);
                command.Parameters.AddWithValue("@Name", offer.Name);
                command.Parameters.AddWithValue("@WeekdayPrice", offer.WeekdayPrice);
                command.Parameters.AddWithValue("@WeekendPrice", offer.WeekendPrice);
                command.Parameters.AddWithValue("@DurationInMinutes", offer.DurationInMinutes);
                command.Parameters.AddWithValue("@Description", (object)offer.Description ?? DBNull.Value);

                command.ExecuteNonQuery();
            }
        }


        private void DeleteOfferFromDatabase(int offerID)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("DELETE FROM SpecialOffers WHERE OfferID = @OfferID", connection);
                command.Parameters.AddWithValue("@OfferID", offerID);
                command.ExecuteNonQuery();
            }
        }

        private void RefreshSpecialOffersTable()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT * FROM SpecialOffers", connection);
                using (var reader = command.ExecuteReader())
                {
                    var offers = new List<SpecialOffer>();
                    while (reader.Read())
                    {
                        offers.Add(new SpecialOffer
                        {
                            OfferID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            WeekdayPrice = reader.GetDecimal(2),
                            WeekendPrice = reader.GetDecimal(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            DurationInMinutes = reader.GetInt32(6) 
                        });
                    }
                    SpecialOffersTable.ItemsSource = offers;
                }
            }
        }


        public class SpecialOffer
        {
            public int OfferID { get; set; }
            public string Name { get; set; }
            public decimal WeekdayPrice { get; set; }
            public decimal WeekendPrice { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public int DurationInMinutes { get; set; } // Новое поле
            public bool IsFixedDuration { get; set; }
        }

        public List<ComputerModel> GetComputers()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ComputerID, Name, Status, CurrentUserID, ReservationEndTime , ReservationStartTime FROM Computers";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        var computers = new List<ComputerModel>();
                        while (reader.Read())
                        {
                            computers.Add(new ComputerModel
                            {
                                ComputerID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Status = reader.GetString(2),
                                CurrentUserID = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                ReservationEndTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                ReservationStartTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                            });
                        }
                        return computers;
                    }
                }
            }
        }

        private void LoadComputers()
        {
            var computers = GetComputers();
            ComputersTable.ItemsSource = computers;
        }

        private void AddComputer_Click(object sender, RoutedEventArgs e)
        {
            string name = ComputerNameTextBox.Text;
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(status))
            {
                MessageBox.Show("Введите имя ПК и выберите статус.");
                return;
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Computers (Name, Status) VALUES (@name, @status)";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("status", status);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Компьютер успешно добавлен!");
            LoadComputers(); // Обновляем список
        }

        private void DeleteComputer_Click(object sender, RoutedEventArgs e)
        {
            if (ComputersTable.SelectedItem is ComputerModel selectedComputer)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Computers WHERE ComputerID = @id";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("id", selectedComputer.ComputerID);
                        cmd.ExecuteNonQuery();
                    }
                }

                CustomMessageBox.Show("Компьютер успешно удален!");
                LoadComputers(); // Обновляем список
            }
            else
            {
                CustomMessageBox.Show("Выберите компьютер для удаления.");
            }
        }
        private void ManageComputers_Click(object sender, RoutedEventArgs e)
        {
            HomePage.Visibility = Visibility.Collapsed;
            ManageUsersPage.Visibility = Visibility.Collapsed;
            StatisticsPage.Visibility = Visibility.Collapsed;
            SettingsPage.Visibility = Visibility.Collapsed;
            ManageComputersPage.Visibility = Visibility.Visible;
        }

        private void Back_ClickComputers(object sender, RoutedEventArgs e)
        {
            ManageComputersPage.Visibility = Visibility.Collapsed;
            HomePage.Visibility = Visibility.Visible;
        }

        private void SetFreeStatus_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный ПК из таблицы
            var selectedComputer = ComputersTable.SelectedItem as ComputerModel;
            if (selectedComputer != null)
            {
                // Обновляем статус на "Free" и очищаем время начала и конца бронирования
                selectedComputer.Status = "Free";
                selectedComputer.CurrentUserID = null;
                selectedComputer.ReservationStartTime = null;
                selectedComputer.ReservationEndTime = null;

                // Обновляем информацию в базе данных
                UpdateComputerStatus(selectedComputer.ComputerID, selectedComputer.Status);
                UpdateReservationTimes(selectedComputer.ComputerID, null, null);

                // Перезагружаем таблицу
                LoadComputers();
            }
        }

        private void SetOccupiedStatus_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный ПК из таблицы
            var selectedComputer = ComputersTable.SelectedItem as ComputerModel;
            if (selectedComputer != null)
            {
                // Проверяем, выбрано ли время окончания бронирования
                

                // Обновляем статус на "Occupied" и устанавливаем время начала и конца бронирования
                selectedComputer.Status = "Occupied";
                selectedComputer.ReservationStartTime = DateTime.Now;
            
                // Обновляем информацию в базе данных
                UpdateComputerStatus(selectedComputer.ComputerID, selectedComputer.Status);
                UpdateReservationTimes(selectedComputer.ComputerID, selectedComputer.ReservationStartTime, selectedComputer.ReservationEndTime);

                // Перезагружаем таблицу
                LoadComputers();
            }
        }

        private void UpdateComputerStatus(int computerID, string status)
        {
            // Проверяем данные, которые передаются в запрос
            MessageBox.Show($"Updating status for ComputerID: {computerID} to {status}");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Computers SET Status = @Status WHERE ComputerID = @ComputerID";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateReservationTimes(int computerID, DateTime? reservationStartTime, DateTime? reservationEndTime)
        {
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Computers SET ReservationStartTime = @ReservationStartTime, ReservationEndTime = @ReservationEndTime WHERE ComputerID = @ComputerID";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ReservationStartTime", reservationStartTime.HasValue ? (object)reservationStartTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReservationEndTime", reservationEndTime.HasValue ? (object)reservationEndTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);

                    cmd.ExecuteNonQuery();
                }
            }
        }


    }

}

