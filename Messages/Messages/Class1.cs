using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{

    // Обработчик сообщений.
    public class Message_Handler <T>
    {
        public Message_Handler()
        {

        }

        // Получить пакет.
        public byte[] Encrypt(Message m)
        {
            // Буффер для передачи данных.
            byte[] bytes = new byte[1024];
            // Вспомогательная строка для составления пакета.
            string assis = string.Empty;
            // Формирование пакета.
            assis += m.Get_Type().ToString();
            assis += m.Get_Data();
            // Кодировка пакета.
            bytes = Encoding.ASCII.GetBytes(assis);

            return bytes;
        }

        // Раскрыть пакет.
        public T Decrypt<T>(byte[] bytes) where T: Message
        {
            return m;
        }
    }

    // Стандартное сообщение.
    public class Message
    {
        // Определяет содержимое.
        protected string text;
        // Определяет тип сообщения.
        protected int type;

        public Message()
        {
            text = string.Empty;
            type = -1;
        }

        public Message(string s)
        {
            text = s;
        }

        // Возвращает содержимое.
        public string Get_Data()
        {
            return text;
        }

        //Возвращает тип.
        public int Get_Type()
        {
            return type;
        }
    }

    // Сообщение необзодимости загрузки файлов с сервера (1).
    public class Inform_of_Rec_Message : Message
    {
        public Inform_of_Rec_Message(string s) : base(s)
        {
            text = s;
            type = 1;
        }
    }

    // Сообщение необзодимости загрузки файлов на сервер (0).
    public class Inform_of_Down_Message : Message
    {
        public Inform_of_Down_Message(string s) : base(s)
        {
            text = s;
            type = 0;
        }
    }

    // Сообщение содержащее часть передаваемого файла (4).
    public class FilePartMessage : Message
    {
        public FilePartMessage(string s) : base(s)
        {
            text = s;
            type = 4;
        }
    }

    // Сообщение содержащее имя файла(3).
    public class RequestMessage : Message
    {
        public RequestMessage(string s) : base(s)
        {
            text = s;
            type = 3;
        }
    }

    // Сообщение содержащее элемент списка файлов (2).
    public class ListMessage : Message
    {
        public ListMessage(string s) : base(s)
        {
            text = s;
            type = 2;
        }
    }

    // Сообщение об окончании коммуникационной сессии (5).
    public class EndMessage : Message
    {
        public EndMessage(string s) : base(s)
        {
            text = s;
            type = 5;
        }
    }

    // Сообщение об окончании коммуникационной сессии (6).
    public class ResponseMessage : Message
    {
        public ResponseMessage(string s) : base(s)
        {
            text = s;
            type = 6;
        }
    }

    // Сообщение об окончании коммуникационной сессии (7).
    public class ErrorMessage : Message
    {
        public ErrorMessage(string s) : base(s)
        {
            text = s;
            type = 7;
        }
    }
}
