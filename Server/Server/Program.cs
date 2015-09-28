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
            // Буффер входящих данных. 
            byte[] bytes = new Byte[1024];

            // Установление удаленной конечной точки для сокета.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Создание TCP\IP сокета.
            Socket listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            // Привязка сокета к конечной точке и ожидание коннектов.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Начало прослушки.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Программа приостанавливается, ожидая входящее соединение.
                    Socket handler = listener.Accept();
                    data = null;
                    // Входящее соединение должно быть обработано.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    // Показать данные на консоли.
                    Console.WriteLine("Text received : {0}", data);

                    // Эхо назад (клиенту).
                    byte[] msg = Encoding.ASCII.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();                    
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
