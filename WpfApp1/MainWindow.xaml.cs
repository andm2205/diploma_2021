using System;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SearchItem_Click(null, null);
        }

        static public void SettingsInicialization()
        {
            if (!Properties.Settings.Default.firstStart)
                return;
            Properties.Settings.Default.dateFilterStart = DateTime.Now.Date.AddDays(-1);
            Properties.Settings.Default.dateFilterEnd = DateTime.Now.Date;
            Properties.Settings.Default.dateFilterEnable = false;
            Properties.Settings.Default.rowCountValueIndex = 0;
            Properties.Settings.Default.rowCountValue = 100;
            Properties.Settings.Default.firstStart = false;
        }

        private void SearchItem_Click(object sender, RoutedEventArgs e)
        {
            frame.NavigationService.Navigate(new Uri("SearchPage.xaml", UriKind.Relative));
        }

        private void ArchiveItem_Click(object sender, RoutedEventArgs e)
        {
            frame.NavigationService.Navigate(new Uri("ArchivePage.xaml", UriKind.Relative));
        }
        private void ConnectionSettingsItem_Click(object sender, RoutedEventArgs e)
        {
            frame.NavigationService.Navigate(new Uri("ConnectionSettingsPage.xaml", UriKind.Relative));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Properties.Remove("CurrentConnection");
                Application.Current.Properties.Add("CurrentConnection", 
                    RegistryUtils.ReadConnection());
            }
            catch (Exception exc)
            {
                Task.Run(() => MessageBox.Show("Не удалось считать подключение из файла\n" + exc.Message));
                return;
            }
        }
    }
}
