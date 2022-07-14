using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;


public class UdpFileServer
{
    // Информация о файле (требуется для получателя)
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

   

    [STAThread]
    static void Main(string[] args)
    {

        FileDetails fileDet = new FileDetails();

        // Поля, связанные с UdpClient
        IPAddress remoteIPAddress;
        int TCPPort;
        int UDPPort;
        int dalay;
        string fileName;
        UdpClient sender = new UdpClient();
        IPEndPoint endPoint;

        // Filestream object
        FileStream fs;

        string s = null;
        Console.WriteLine("---- PLS Write host portTCP portUDP catalog dalay ----");
        s = Console.ReadLine();
        string[] s_arr = s.Split();
        remoteIPAddress = IPAddress.Parse(s_arr[0]);
        TCPPort = int.Parse(s_arr[1]);
        UDPPort = int.Parse(s_arr[2]);
        fileName = s_arr[3];
        dalay = int.Parse(s_arr[4]);
  

        try
        {
            // Получаем удаленный IP-адрес и создаем IPEndPoint
            Console.WriteLine("Введите удаленный IP-адрес");
            endPoint = new IPEndPoint(remoteIPAddress, TCPPort);

            // Получаем путь файла и его размер (должен быть меньше 8kb)
            Console.WriteLine("Введите путь к файлу и его имя");
            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            
            //if (fs.Length > 8192)
            //{
            //    Console.Write("Файл должен весить меньше 8кБ");
            //    sender.Close();
            //    fs.Close();
            //    return;
            //}

            // Отправляем информацию о файле
            // Получаем тип и расширение файла
            fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

            // Получаем длину файла
            fileDet.FILESIZE = fs.Length;

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream = new MemoryStream();

            // Сериализуем объект
            fileSerializer.Serialize(stream, fileDet);

            // Считываем поток в байты
            stream.Position = 0;

            Byte[] bytes = new Byte[1024];
            
            stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

            Console.WriteLine("Отправка деталей файла...");

            // Отправляем информацию о файле
            sender.Send(bytes, bytes.Length, endPoint);
            stream.Close();

            // Ждем 2 секунды
            Thread.Sleep(dalay);

            // Отправляем сам файл
            // Создаем файловый поток и переводим его в байты
            
            fs.Read(bytes, 0, bytes.Length);

            Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
            try
            {
                // Отправляем файл
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }
            finally
            {
                // Закрываем соединение и очищаем поток
                fs.Close();
                sender.Close();
            }
            Console.WriteLine("Файл успешно отправлен.");
            Console.Read();

            Console.ReadLine();

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
}