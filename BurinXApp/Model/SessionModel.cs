using System;

namespace BurinXApp.Model
{
    public class SessionModel
    {
        public int SessionID { get; set; }
        public string SessionDate { get; set; } // Добавлено свойство SessionDate
        public string Duration { get; set; }   // Теперь это свойство доступно для записи
        public string ComputerName { get; set; }
        public decimal Cost { get; set; }
        public int? SpecialOfferID { get; set; } // Может быть null, если нет специального предложения
    }

}
