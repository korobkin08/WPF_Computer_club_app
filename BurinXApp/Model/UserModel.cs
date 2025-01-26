using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurinXApp.Model
{
    public class UserModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public decimal Balance { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
