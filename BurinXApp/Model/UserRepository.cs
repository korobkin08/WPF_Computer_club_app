using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace BurinXApp.Model
{
    public class UserRepository
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=0806AAA2005;Database=BurinX";

        public void RegisterUser(UserModel user)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@username, @passwordHash, @role)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("passwordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("role", user.Role);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
