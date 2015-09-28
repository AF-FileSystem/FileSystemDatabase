using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Server_Hub
{
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

            // Буффер входящих сообщений.
            Byte[] bytes = new Byte[1024];
            

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
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Перевод данных в ASCII.
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Декодирование данных.
                        data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Отослать назад уведомление.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

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
