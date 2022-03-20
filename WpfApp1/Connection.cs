using System;

namespace WpfApp1
{
    [Serializable]
    public class Connection
    {
        public string ArchiveServerName { get; set; }
        public string ArchiveDbName { get; set; }
        public string LinkedDbName { get; set; }
        public string LinkedTable1Name { get; set; }
        public string LinkedTable2Name { get; set; }

        public DateTime lastUseDateTime = DateTime.Now;

        private Connection()
        {
            lastUseDateTime = DateTime.Now;
        }
        public Connection(
            string archiveServerName, 
            string archiveDbName,
            string linkedDbName,
            string linkedTable1Name,
            string linkedTable2Name)
            : this()
        {
            this.ArchiveServerName = archiveServerName;
            this.ArchiveDbName = archiveDbName;
            this.LinkedDbName = linkedDbName;
            this.LinkedTable1Name = linkedTable1Name;
            this.LinkedTable2Name = linkedTable2Name;
        }

        public override bool Equals(object obj)
        {
            Connection connection = (Connection)obj;
            return
            this.ArchiveServerName == connection.ArchiveServerName
            && this.ArchiveDbName == connection.ArchiveDbName
            && this.LinkedDbName == connection.LinkedDbName
            && this.LinkedTable1Name == connection.LinkedTable1Name
            && this.LinkedTable2Name == connection.LinkedTable2Name;
        }

        public static string FormatConnectionCode(string code, Connection connection)
        {
            return String.Format(
                code,
                connection.ArchiveDbName,
                connection.LinkedDbName,
                connection.LinkedTable1Name,
                connection.LinkedTable2Name);
        }
    }
}
