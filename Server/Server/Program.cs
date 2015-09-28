using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Server_Hub
{
    class Request_Handler
    { }
    class File_Translator
    { }
    class Server
    {
        // Входящие данные от клиента.
        public static string data = null;

        public static void StartListening()
        {
            TcpListener lstnr = null;       
            IPAddress localAddr = IPAddress.Parse("192.168.7.101");
            Int32 port = 13000;
            lstnr = new TcpListener(localAddr, port);
            string flnme;

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            Byte[] filenames;
            string checkr;
            Int32 ch;

            // Привязка сокета к конечной точке и ожидание коннектов.
            try
            {

                // Начало прослушки.
                lstnr.Start();

                // Начало прослушки.
                while (true)
                {

                    Console.Write("Waiting for a connection... ");

                    // Потверждение соединения.
                    TcpClient client = lstnr.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    String data = null;

                    // Выделяем поток
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Зацикливаем получение данных от клиента.                 
                    string[] dirs = Directory.GetFiles(@"D:\DFS");
                    for (int q = 0;q<dirs.Length;q++)
                    {
                        FileInfo inf = new FileInfo(dirs[q]);
                        flnme = (Path.GetFileName(dirs[q]));
                        filenames = Encoding.ASCII.GetBytes(flnme);
                        stream.Write(filenames, 0, filenames.Length);
                        Console.WriteLine("Sent: {0}", flnme);
                        while (true)
                        {
                            // Прочесть ответ сервера.
                            Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                            checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                            if (checkr!="")
                            {
                                checkr = "";
                                break;
                            }
                        }
                    }
                    flnme = "End of list";
                    filenames = Encoding.ASCII.GetBytes(flnme);
                    stream.Write(filenames, 0, filenames.Length);
                    Console.WriteLine("Sent: {0}", flnme);

                    // Закрытие соединения.
                    client.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartListening();
        }
    }
}
