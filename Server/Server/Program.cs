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

        public void Handling(Socket s)
        {
            byte[] filenames;
            string flnme;
            string[] dirs = Directory.GetFiles(@"D:\DFS");
            for (int q = 0; q < dirs.Length; q++) {
                FileInfo inf = new FileInfo(dirs[q]);
                flnme = (Path.GetFileName(dirs[q]));
                filenames = Encoding.ASCII.GetBytes(flnme);
                s.Send(filenames);
                Console.WriteLine("Sent: {0}", flnme);
                while (true) {
                    // Прочесть ответ сервера.
                    //Int32 bytes_1 = stream.Read(bytes, 0, bytes.Length);
                    checkr = Encoding.ASCII.GetString(bytes, 0, bytes_1);
                    if (checkr != "") {
                        checkr = "";
                        break;
                    }
                }
            }
            flnme = "End of list";
            filenames = Encoding.ASCII.GetBytes(flnme);
            stream.Write(filenames, 0, filenames.Length);
            Console.WriteLine("Sent: {0}", flnme);

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

        public void StartListening()
        {
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 11000);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                listener.Bind(EndPoint);
                listener.Listen(100);

                // Начало прослушки.
                while (true)
                {

                    Console.Write("Waiting for a connection... ");

                    // Потверждение соединения.
                    Socket handler = listener.Accept();
                    Console.WriteLine("Connected!");

                    String data = null;

                    // Выделяем поток

                    int i;

                    // Зацикливаем получение данных от клиента.                 
                    
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
