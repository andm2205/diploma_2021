using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class SearchPage : Page
    {
        private ObservableCollection<object> list = new ObservableCollection<object>();
        private DispatcherTimer timer;
        private DateTime executeTime = DateTime.Now;
        private SqlCommand currentCommand;
        private bool working = false;
        private bool Canceling { get; set; } = false;
        private bool Working
        {
            get
            {
                return working;
            }
            set
            {
                ChangeStateOfElements(!value);
                working = value;
                if (value)
                {
                    executeTime = DateTime.Now;
                    timer.Start();
                }
                else
                {
                    currentCommand = null;
                    timer.Stop();
                }
            }
        }

        public SearchPage()
        {
            InitializeComponent();
            if (Properties.Settings.Default.firstStart)
                SettingsInicialization();
            Properties.Settings.Default.firstStart = false;
            dataGrid.ItemsSource = list;
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            Working = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            executeTimeL.Content = (executeTime - DateTime.Now).ToString(@"hh\:mm\:ss");
        }

        void ChangeStateOfElements(bool state)
        {
            printButton.IsEnabled = state;
            filtersButton.IsEnabled = state;
            clearButton.IsEnabled = state;
            stopButton.IsEnabled = !state;
            isExecutePb.IsIndeterminate = !state;
        }

        static public void SettingsInicialization()
        {
            Properties.Settings.Default.dateFilterStart = DateTime.Now.Date.AddDays(-1);
            Properties.Settings.Default.dateFilterEnd = DateTime.Now.Date;
            Properties.Settings.Default.dateFilterEnable = false;
            Properties.Settings.Default.rowCountValueIndex = 0;
            Properties.Settings.Default.rowCountValue = 100;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FiltersButton_Click(object sender, RoutedEventArgs e)
        {
            new FiltersWindow().ShowDialog();
        }

        private async void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            uint rowCount;
            bool dateFilter;
            DateTime startDateTime;
            DateTime endDateTime;
            string userName;
            string objectNames;
            string comment;
            try
            {
                rowCount = (uint)RegistryUtils.Read("RowCountValue");
                dateFilter = (bool)RegistryUtils.Read("DateFilterEnable");
                startDateTime = (DateTime)RegistryUtils.Read("DateFilterStart");
                endDateTime = (DateTime)RegistryUtils.Read("DateFilterEnd");
                userName = (string)RegistryUtils.Read("UserName");
                objectNames = (string)RegistryUtils.Read("ObjectNames");
                comment = (string)RegistryUtils.Read("Comment");
            }
            catch(Exception exc)
            {
                MessageBox.Show("Ошибка при загрузке фильтров\n" + exc.Message);
                return;
            }
            Working = true;
            Connection connection = (Connection)Application.Current.Properties["CurrentConnection"];
            if (connection == null)
            {
                Working = false;
                MessageBox.Show("Текущее подключение не найдено");
                return;
            }

            using (var sqlConnection = DBUtils.GetSqlConnection(
                    connection.ArchiveDbName,
                    connection.ArchiveServerName))
            {
                try
                {
                    await sqlConnection.OpenAsync();
                    var command = currentCommand = new SqlCommand(Connection.FormatConnectionCode(Properties.Resources.SearchFunc, connection), sqlConnection);
                    Clipboard.SetText(command.CommandText);
                    await command.ExecuteNonQueryAsync();

                    command = currentCommand = new SqlCommand("SearchFunc", sqlConnection)
                    {
                        CommandTimeout = 1000 * 60 * 20,
                        CommandType = System.Data.CommandType.StoredProcedure
                    };
                    command.Parameters.Add(new SqlParameter("@rowCount", System.Data.SqlDbType.BigInt) { Value = rowCount });
                    if (dateFilter)
                    {
                        command.Parameters.AddWithValue("@startDate", startDateTime);
                        command.Parameters.AddWithValue("@endDate", endDateTime);
                    }
                    if (!string.IsNullOrWhiteSpace(userName))
                        command.Parameters.AddWithValue("@userName", userName);
                    if (!string.IsNullOrWhiteSpace(objectNames))
                    {
                        string[] objectNamesArray = objectNames.Split('\n');
                        var dataTable = new DataTable();
                        var dataColumn = new DataColumn();
                        dataTable.Columns.Add(new DataColumn() { Unique = true });
                        for (int a = 0; a < objectNamesArray.Length; ++a)
                            dataTable.Rows.Add(new object[] { objectNamesArray[a] });
                        command.Parameters.AddWithValue("@objNames", dataTable);
                    }
                    if (!string.IsNullOrWhiteSpace(comment))
                        command.Parameters.AddWithValue("@comment", comment);

                    ClearButton_Click(sender, e);

                    Clipboard.SetText(command.CommandText);
                    
                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        for (int a = 0; a < dataReader.FieldCount; a++)
                        {
                            var column = new DataGridTextColumn
                            {
                                Header = dataReader.GetName(a),
                                Binding = new Binding(string.Format("[{0}]", a))
                            };
                            dataGrid.Columns.Add(column);
                        }
                        while (await dataReader.ReadAsync())
                        {
                            object[] buf = new object[dataReader.FieldCount];
                            dataReader.GetValues(buf);
                            list.Add(buf);
                        }
                    }
                    GC.Collect(5, GCCollectionMode.Forced);
                    command.Parameters.Clear();
                }
                catch (SqlException e1) when ((string)e1.Data["HelpLink.EvtID"] == "0")
                {
                    
                }
                catch (Exception e0)
                {
                    Working = false;
                    MessageBox.Show(e0.Message);
                    return;
                }
            }
            Working = false;
            MessageBox.Show("Успешно");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            list.Clear();
            dataGrid.ItemsSource = list;
            dataGrid.Columns.Clear();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if(Working)
                try
                {
                    Canceling = true;
                    currentCommand?.Cancel();
                }
                catch { }
                finally
                {
                    Canceling = false;
                }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            new CreateWindow().ShowDialog();
        }
    }
}
