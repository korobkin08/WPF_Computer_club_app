using BurinXApp.Model;
using Npgsql;

public class AuthService
{
    // Статический экземпляр класса
    private static AuthService _instance;

    // Объект для блокировки
    private static readonly object LockObj = new();

    // Приватный конструктор
    private AuthService() { }

    // Глобальная точка доступа к экземпляру
    public static AuthService Instance
    {
        get
        {
            lock (LockObj)
            {
                if (_instance == null)
                {
                    _instance = new AuthService();
                }
                return _instance;
            }
        }
    }

    // Строка подключения
    private readonly string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";

    public UserModel AuthenticateUser(string username, string password)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            string query = "SELECT * FROM Users WHERE Username = @username AND PasswordHash = @passwordHash";
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("passwordHash", HashPassword(password));

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserModel
                        {
                            UserID = (int)reader["UserID"],
                            Username = reader["Username"].ToString(),
                            PasswordHash = reader["PasswordHash"].ToString(),
                            Balance = (decimal)reader["Balance"],
                            Role = reader["Role"].ToString(),
                            CreatedAt = (DateTime)reader["CreatedAt"]
                        };
                    }
                }
            }
        }

        return null; // Если пользователь не найден
    }

    private string HashPassword(string password)
    {
        // Здесь будет логика хеширования
        return password;
    }
}
