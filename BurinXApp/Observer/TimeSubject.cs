using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BurinXApp.Observer
{
    public class TimeSubject : ISubject
    {
        private List<IObserver> observers = new List<IObserver>();
        private string currentTime;

        public void RegisterObserver(IObserver observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update(currentTime);
            }
        }

        public void SetTime(string time)
        {
            currentTime = time;
            NotifyObservers();
        }
    }

}
