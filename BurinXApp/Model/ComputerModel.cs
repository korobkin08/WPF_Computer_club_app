using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurinXApp.Model
{
    public class ComputerModel
    {
        public int ComputerID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int? CurrentUserID { get; set; }
        public DateTime? ReservationStartTime { get; set; } // Начало брони
        public DateTime? ReservationEndTime { get; set; }   // Окончание брони
        public TimeSpan? StartTime { get; set; }
    }
}
