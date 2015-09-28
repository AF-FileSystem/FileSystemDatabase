using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Client_Test
{
    class File_Sender
    { }
    class File_Reciever
    { }
    class Program
    {

        public static void StartClient()
        {
            // Буффер входящих данных.
            byte[] bytes = new Byte[1024];
            // Проверка.
            string checker = "";

            IPAddress ServAddr = IPAddress.Parse("192.168.7.101");
            Int32 port = 13000;
            TcpClient clnt = new TcpClient(ServAddr.ToString(), port);

            Byte[] data = Encoding.ASCII.GetBytes("Hello!");

            NetworkStream stream = clnt.GetStream();

            try
            {
                while(checker!="End of list")
                {

                    // Буффер дл получения ответа.
                    data = new Byte[256];

                    // Сткрока для храниения ASCII-варианта ответа.
                    String responseData = String.Empty;

                    // Прочесть ответ сервера.
                    Int32 bytes_1 = stream.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes_1);
                    if (responseData != "End of list")
                    {
                        Console.WriteLine("Received: {0}", responseData);
                    }
                    checker = responseData;


                    // Декодирование данных.
                    responseData = responseData.ToUpper();

                    byte[] msg = Encoding.ASCII.GetBytes(responseData);

                    // Отослать назад уведомление.
                    stream.Write(msg, 0, msg.Length);
                }
                // Закрыть все.
                stream.Close();
                clnt.Close();

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
