using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Client_Test
{
    class Program
    {

        public static void StartClient()
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];

            //Установление удаленной конечной точки для сокета.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            //Создание TCP/IP сокета
            Socket sendr = new Socket(AddressFamily.InterNetworkV6,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {

                sendr.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sendr.RemoteEndPoint.ToString());

                // Кодировка сообщения.
                byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                // Отправка сообщения через сокет.
                int bytesSent = sendr.Send(msg);

                // Получение ответа от сервера.
                int bytesRec = sendr.Receive(bytes);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.
                sendr.Shutdown(SocketShutdown.Both);
                sendr.Close();

            }
            catch (ArgumentNullException anex)
            {
                Console.WriteLine("ArgumentNullException : {0}", anex.ToString());
            }
            catch (SocketException sex)
            {
                Console.WriteLine("SocketException : {0}", sex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception : {0}", ex.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartClient();
        }
    }
}
