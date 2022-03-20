using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class ArchivePage : Page
    {
        private DateTime executeTime;
        private DispatcherTimer timer;
        private SqlCommand command;
        ObservableCollection<string> logList;
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
                    logList?.Clear();
                    logList = new ObservableCollection<string>();
                    logListBox.ItemsSource = logList;
                    executeTime = DateTime.Now;
                    timer.Start();
                    AddLogRecord("Подключение");
                    ChangeStateOfElements(false);
                }
                else
                {
                    command = null;
                    ChangeStateOfElements(true);
                    timer.Stop();
                }
            }
        }
        public ArchivePage()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            Working = false;
            Canceling = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            executeTimeL.Content = (executeTime - DateTime.Now).ToString(@"hh\:mm\:ss");
        }

        void ChangeStateOfElements(bool state)
        {
            executeButton.IsEnabled = state;
            stopButton.IsEnabled = !state;
            isExecutePb.IsIndeterminate = !state;
        }

        void AddLogRecord(string text)
        {
            if (logListBox.Items.Count == 50)
                logList.RemoveAt(0);
            logList.Add($"{DateTime.Now} {text}");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            uint rowCount;
            try
            {
                rowCount = Convert.ToUInt32(rowCountTb.Text);
                if (rowCount < 100)
                {
                    MessageBox.Show("Количество записей должно быть не меньше 100");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Неверно введено количество записей");
                return;
            }

            Connection connection = (Connection)Application.Current.Properties["CurrentConnection"];
            if (connection == null)
            {
                MessageBox.Show("Текущее подключение не найдено");
                return;
            }
            string archiveServerName = connection.ArchiveServerName,
            archiveDbName = connection.ArchiveDbName;

            Working = true;
            try
            {
                using (SqlConnection sqlConnection = DBUtils.GetSqlConnection(archiveDbName, archiveServerName))
                {
                    AddLogRecord("Подключение к базе данных");

                    await sqlConnection.OpenAsync();

                    AddLogRecord("Создание функций");

                    var transaction = sqlConnection.BeginTransaction();

                    command = new SqlCommand(Connection.FormatConnectionCode(Properties.Resources.MergeOperationsFunc, connection), sqlConnection) { Transaction = transaction } ;
                    await command.ExecuteNonQueryAsync();
                    command = new SqlCommand(Connection.FormatConnectionCode(Properties.Resources.ChangeStateConstraintsFunc, connection), sqlConnection) { Transaction = transaction };
                    await command.ExecuteNonQueryAsync();
                    command = new SqlCommand(Connection.FormatConnectionCode(Properties.Resources.CopyJournalFunc, connection), sqlConnection) { Transaction = transaction };
                    await command.ExecuteNonQueryAsync();

                    AddLogRecord("Соединение таблиц операций");

                    command = new SqlCommand("MergeOperations", sqlConnection)
                    { CommandType = CommandType.StoredProcedure, Transaction = transaction };
                    await command.ExecuteScalarAsync();

                    AddLogRecord("Отключение проверки внешних ключей");

                    command = new SqlCommand("ChangeStateConstraints", sqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 0,
                        Transaction = transaction
                    };

                    command.Parameters.Add(new SqlParameter("@state", SqlDbType.Bit)
                    { Value = 0 });
                    await command.ExecuteNonQueryAsync();

                    int rowsCopiedCount = 0;
                    int totalRowsCopiedCount = 0;

                    AddLogRecord("Копирование журнала");
                    try
                    {
                        do
                        {
                            if (rowsCopiedCount != 0)
                                transaction = sqlConnection.BeginTransaction();
                            command = new SqlCommand("CopyJournal", sqlConnection)
                            {
                                CommandType = CommandType.StoredProcedure,
                                CommandTimeout = 0,
                                Transaction = transaction
                            };
                            command.Parameters.Add(new SqlParameter("@cRow", SqlDbType.BigInt)
                            { Value = rowCount });
                            rowsCopiedCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                            totalRowsCopiedCount += rowsCopiedCount;
                            transaction.Commit();

                            AddLogRecord($" Скопировано строк: {Convert.ToString(totalRowsCopiedCount)}");
                        } while (rowsCopiedCount == rowCount && !Canceling);

                        AddLogRecord("Завершено");
                    }
                    finally
                    {
                        AddLogRecord("Включение проверки внешних ключей");

                        command = new SqlCommand("ChangeStateConstraints", sqlConnection)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 0,
                            Transaction = transaction
                        };
                        command.Parameters.Add(new SqlParameter("@state", SqlDbType.Bit)
                        { Value = 1});
                        await command.ExecuteNonQueryAsync();
                    }


                }
            }
            catch (SqlException se) when ((string)se.Data["HelpLink.EvtID"] == "0")
            {
                AddLogRecord("Остановлено пользователем");
            }
            catch (Exception a)
            {
                Working = false;
                AddLogRecord("Завершено с ошибкой");
                MessageBox.Show(a.Message);
                return;
            }
            finally
            {
                Working = false;
            }
            MessageBox.Show(
$@"Успешно");
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Working)
            {
                await Task.Run(() =>
                {
                    MessageBox.Show("Архивация пытается завершиться");
                });
                StopButton_Click(sender, e);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if(Working)
                {
                    AddLogRecord("Задание отменяется");
                    Canceling = true;
                    command?.Cancel();
                }
            }
            catch
            {
                MessageBox.Show("Ошибка при останвке программы");
                return;
            }
            finally
            {
                Canceling = false;
            }
        }
    }
}
