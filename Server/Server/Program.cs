using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Server_Hub
{   class Hasher
    {
        public Hasher()
        { }
        public Int32 HashMe(byte[] a)
        {
            Int32 ans=0;
            return ans;
        }
    } 
    class Request_Handler
    {
        int port;
        public Request_Handler(int p)
        {
            this.port = p;
        }

        public void Handling()
        {
            TcpListener lstnr = null;
            IPAddress localAddr = IPAddress.Parse("172.16.16.95");
            Int32 port = this.port;
            lstnr = new TcpListener(localAddr, port);
            string flnme;

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];

            // Привязка сокета к конечной точке и ожидание коннектов.
            try
            {

                // Начало прослушки.
                lstnr.Start();

                Console.Write("Start handling... ");

                // Потверждение соединения.
                TcpClient client = lstnr.AcceptTcpClient();
                // Выделяем поток
                NetworkStream stream = client.GetStream();

                // Буффер дл получения ответа.
                Byte[] data = new Byte[256];

                // Сткрока для храниения ASCII-варианта ответа.
                String responseData = String.Empty;

                // Прочесть ответ сервера.
                Int32 bytes_1 = stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes_1);

                Console.WriteLine("Request for " + responseData);
                /*
                //Отправка файлов.
                File_Translator FT = new File_Translator(15000);
                FT.Send(responseData);
                */

                Console.WriteLine("Handling completed!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        
    }
    }
    class File_Translator
    {
        private int port;
        public File_Translator(int p)
        {
            this.port = p;
        }
        public void Send(string s)
        {

        }
    }
    class Server
    {
        // Входящие данные от клиента.
        public static string data = null;

        public static void StartListening()
        {
            TcpListener lstnr = null;       
            IPAddress localAddr = IPAddress.Parse("172.16.16.95");
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
                    // Начало работы с файлами.
                    Request_Handler RH = new Request_Handler(15000);
                    RH.Handling();
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
