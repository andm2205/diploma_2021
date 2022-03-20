using System.IO;
using System.Xml.Serialization;


namespace WpfApp1
{
    class XmlSerializeUtils
    {
        public static void WriteConnection(Connection connection)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Connection));
            using (FileStream fileStream = new FileStream("connection.xml", FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, connection);
            }
        }
        public static Connection GetConnection()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Connection));
            using (FileStream fileStream = new FileStream("connection.xml", FileMode.Open))
            {
                return (Connection)xmlSerializer.Deserialize(fileStream);
            }
        }
    }
}
