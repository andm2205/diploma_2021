using System;
using System.Windows;

namespace WpfApp1
{
    public partial class FiltersWindow : Window
    {
        public FiltersWindow()
        {
            InitializeComponent();
            ReloadFields();
        }

        private void ReloadFields()
        {
            bool allRead = true;
            try {rowCountTb.Text = Convert.ToString(RegistryUtils.Read("RowCountValue")); } catch { allRead = false; }
            try {dateFilterCb.IsChecked = (bool)RegistryUtils.Read("DateFilterEnable"); } catch {allRead = false; }
            try {startDateTimePicker.DateTime = (DateTime)RegistryUtils.Read("DateFilterStart"); } catch {allRead = false;}
            try {endDateTimePicker.DateTime = (DateTime)RegistryUtils.Read("DateFilterEnd"); } catch {allRead = false; }
            try {userNameTb.Text = (string)RegistryUtils.Read("UserName"); } catch {allRead = false; }
            try {objectNameTb.Text = (string)RegistryUtils.Read("ObjectNames"); } catch {allRead = false; }
            try {commentTb.Text = (string)RegistryUtils.Read("Comment"); } catch { allRead = false;}
            if (!allRead)
                MessageBox.Show("Не все настройки удалось загрузить");
        }

        private void SaveProps()
        {
            try
            {
                bool dateFilterEnable = dateFilterCb.IsChecked ?? false;
                DateTime dateFilterStart = startDateTimePicker.DateTime;
                DateTime dateFilterEnd = endDateTimePicker.DateTime;
                uint rowCountValue = Convert.ToUInt32(rowCountTb.Text);
                string userName = userNameTb.Text;
                string objectName = objectNameTb.Text;
                string comment = commentTb.Text;
                if(rowCountValue < 1 && rowCountValue > 1000000)
                {
                    MessageBox.Show("Количество записей должно быть не меньше 1 и не больше 1000000");
                    return;
                }
                RegistryUtils.Write(dateFilterEnable, "DateFilterEnable");
                RegistryUtils.Write(dateFilterStart, "DateFilterStart");
                RegistryUtils.Write(dateFilterEnd, "DateFilterEnd");
                RegistryUtils.Write(rowCountValue, "RowCountValue");
                RegistryUtils.Write(userName, "UserName");
                RegistryUtils.Write(objectName, "ObjectNames");
                RegistryUtils.Write(comment, "Comment");
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить настройки фильтров");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveProps();
                ReloadFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            MessageBox.Show("Успешно");
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ReloadFields();
        }
    }
}
