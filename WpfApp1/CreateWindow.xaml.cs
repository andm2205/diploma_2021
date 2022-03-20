using System;
using System.Data.SqlClient;
using System.Windows;

namespace WpfApp1
{
    public partial class CreateWindow : Window
    {
        public CreateWindow()
        {
            InitializeComponent();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Connection connection = propsPanel.Connection;
            string sep = @"
GO";
            using (SqlConnection sqlConnection = DBUtils.GetSqlConnection("master", connection.ArchiveServerName))
            {
                String[] sCommands = codeTb.Text.Split(new String[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    await sqlConnection.OpenAsync();
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                    return;
                }

                foreach (var sCommand in sCommands)
                {
                    sCommand.Replace(sep, "");

                    var command = new SqlCommand(sCommand, sqlConnection);
                    command.CommandText = sCommand;
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.Message);
                        return;
                    }
                }
            }
            try
            {
                Application.Current.Properties.Remove("CurrentConnection");
                Application.Current.Properties.Add("CurrentConnection", connection);
                RegistryUtils.WriteConnection(connection);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Не удалось сохранить настройки\n" + exc.Message);
                return;
            }
            MessageBox.Show("Успешно");
        }

        private void CreateScriptButton_Click(object sender, RoutedEventArgs e)
        {
            codeTb.Text = Connection.FormatConnectionCode(
                Properties.Resources.DatabaseStruct,
                propsPanel.Connection);
        }
    }
}
