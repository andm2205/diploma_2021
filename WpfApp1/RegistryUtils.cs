using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WpfApp1
{
    class RegistryUtils
    {
        static public void WriteConnection(Connection connection)
        {
            Write(connection, "Connection");
        }
        static public Connection ReadConnection()
        {
            return (Connection)Read("Connection");
        }

        static public void WriteFilters(
            bool dateFilterEnable, 
            DateTime dateFilterStart,
            DateTime dateFilterEnd, 
            int rowCountValueIndex,
            uint rowCountValue,
            string userName,
            string objectName)
        {
            Write(dateFilterEnable, "DateFilterEnable");
            Write(dateFilterStart, "DateFilterStart");
            Write(dateFilterEnd, "DateFilterEnd");
            Write(rowCountValueIndex, "RowCountValueIndex");
            Write(rowCountValue, "RowCountValue");
            Write(userName, "UserName");
            Write(objectName, "ObjectName");

        }

        static public void ReadFilters(
            out bool dateFilterEnable,
            out DateTime dateFilterStart,
            out DateTime dateFilterEnd,
            out int rowCountValueIndex,
            out uint rowCountValue,
            out string userName,
            out string objectName)
        {
            dateFilterEnable = (bool)Read("DateFilterEnable");
            dateFilterStart = (DateTime)Read("DateFilterStart");
            dateFilterEnd = (DateTime)Read("DateFilterEnd");
            rowCountValueIndex = (int)Read("RowCountValueIndex");
            rowCountValue = (uint)Read("RowCountValue");
            userName = (string)Read("UserName");
            objectName = (string)Read("ObjectName");
        }

        static public void Write(object obj, string name)
        {
            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\LoodsmanArchive");
                registryKey.SetValue(name, memoryStream.ToArray(), RegistryValueKind.Binary);
            }
        }

        static public object Read(string name)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\LoodsmanArchive");
            if (registryKey != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Byte[] bytes = (Byte[])registryKey.GetValue(name, null);
                    if (bytes == null)
                        throw new Exception("object is null");
                    memoryStream.Write(bytes, 0, bytes.Length);
                    memoryStream.Position = 0;
                    return formatter.Deserialize(memoryStream);
                }
            }
            else
                throw new Exception("registryKey is null");
        }
    }
}
