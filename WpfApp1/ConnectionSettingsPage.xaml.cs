using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class ConnectionSettingsPage : Page
    {
        public ConnectionSettingsPage()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Connection connection = propsPanel.Connection;
            try
            {
                Application.Current.Properties.Remove("CurrentConnection");
                Application.Current.Properties.Add("CurrentConnection", connection);
                RegistryUtils.WriteConnection(connection);
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить изменения в файле");
                return;
            }
            MessageBox.Show("Успешно");
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            new CreateWindow().ShowDialog();
            propsPanel.Connection = (Connection)Application.Current.Properties["CurrentConnection"];
        }
    }
}
