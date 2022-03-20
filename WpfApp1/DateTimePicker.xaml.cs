using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class DateTimePicker : UserControl
    {
        public DateTime DateTime
        {
            get
            {
                try
                {
                    return datePicker.SelectedDate.Value.Date +
                        new TimeSpan(0, UInt16.Parse(hoursTb.Text), UInt16.Parse(minutesTb.Text), 0, 0);
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                if (value == null) return;
                datePicker.SelectedDate = value;
                hoursTb.Text = string.Format("{0:d2}", value.Hour);
                minutesTb.Text = string.Format("{0:d2}", value.Minute);
            }
        }
        public DateTimePicker()
        {
            InitializeComponent();
            this.DateTime = DateTime.Now;
        }

        private void UpdateTimeFields(object sender, RoutedEventArgs e)
        {
            TextBox tbsender = (TextBox)sender;
            string textValue = tbsender.Text;
            bool valid = true;
            valid &= UInt16.TryParse(textValue, out ushort intValue);
            valid &= intValue < ((tbsender == hoursTb) ? 24 : 60);
            if (valid)
                tbsender.Text = String.Format("{0:d2}", intValue);
            else
                tbsender.Text = "00";
        }
    }
}
