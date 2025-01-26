using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurinXApp.Model
{
    internal class SpecialOfferModel
    {
        public int OfferID { get; set; }
        public string Name { get; set; }
        public decimal WeekdayPrice { get; set; }
        public decimal WeekendPrice { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DurationInMinutes { get; set; } // Новое поле
    }
}
