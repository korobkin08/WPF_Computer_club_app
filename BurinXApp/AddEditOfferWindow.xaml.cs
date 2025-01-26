using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static BurinXApp.AdminWindow;

namespace BurinXApp
{
    /// <summary>
    /// Логика взаимодействия для AddEditOfferWindow.xaml
    /// </summary>
    public partial class AddEditOfferWindow : Window
    {
        public SpecialOffer Offer { get; private set; }

        public AddEditOfferWindow()
        {
            InitializeComponent();
            Offer = new SpecialOffer();
        }

        public AddEditOfferWindow(SpecialOffer offer) : this()
        {
            // Передача существующего предложения для редактирования
            Offer = offer;
            NameTextBox.Text = offer.Name;
            WeekdayPriceTextBox.Text = offer.WeekdayPrice.ToString("0.00");
            WeekendPriceTextBox.Text = offer.WeekendPrice.ToString("0.00");
            DurationTextBox.Text = offer.DurationInMinutes.ToString("0");
            DescriptionTextBox.Text = offer.Description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                !decimal.TryParse(WeekdayPriceTextBox.Text, out decimal weekdayPrice) ||
                !decimal.TryParse(WeekendPriceTextBox.Text, out decimal weekendPrice) ||
                !int.TryParse(DurationTextBox.Text, out int duration))

            {
                MessageBox.Show("Please fill out all fields correctly.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Заполнение объекта предложения
            Offer.Name = NameTextBox.Text.Trim();
            Offer.WeekdayPrice = weekdayPrice;
            Offer.WeekendPrice = weekendPrice;
            Offer.DurationInMinutes = duration;
            Offer.Description = DescriptionTextBox.Text.Trim();

            DialogResult = true; // Закрытие окна с результатом успеха
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Закрытие окна без изменений
            Close();
        }
    }
}