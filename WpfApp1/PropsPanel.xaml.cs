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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class PropsPanel : UserControl
    {

        public Connection Connection
        {
            set
            {
                if (value == null) return;
                archiveServerNameTb.Text = value.ArchiveServerName;
                archiveDbNameTb.Text = value.ArchiveDbName;
                linkedDbNameTb.Text = value.LinkedDbName;
                table1NameTb.Text = value.LinkedTable1Name;
                table2NameTb.Text = value.LinkedTable2Name;
            }
            get
            {
                return new Connection(
                archiveServerNameTb.Text,
                archiveDbNameTb.Text,
                linkedDbNameTb.Text,
                table1NameTb.Text,
                table2NameTb.Text);
            }
        }
        public PropsPanel()
        {
            InitializeComponent();
            this.Connection = (Connection)Application.Current.Properties["CurrentConnection"];
        }
        public void ChangeStateOfElements(bool state)
        {
            archiveServerNameTb.IsEnabled = state;
            archiveDbNameTb.IsEnabled = state;
            linkedDbNameTb.IsEnabled = state;
            table1NameTb.IsEnabled = state;
            table2NameTb.IsEnabled = state;
        }
    }
}
