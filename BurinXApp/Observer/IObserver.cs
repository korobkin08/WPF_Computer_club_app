using System;

namespace BurinXApp
{
    // Интерфейс наблюдателя, который будет обновляться при изменении времени
    public interface IObserver
    {
        void Update(string time);
    }

}
