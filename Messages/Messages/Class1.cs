using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{

    // Обработчик сообщений.
    public class Message_Handler
    {
        public Message_Handler()
        {

        }

        // Получить пакет.
        public byte[] Encrypt(Message m)
        {
            int type = -1;
            if (m is Inform_of_Down_Message)
            {
                type = 0;
            }
            if (m is Inform_of_Rec_Message)
            {
                type = 1;
            }
            if (m is ListMessage)
            {
                type = 2;
            }
            if (m is RequestMessage)
            {
                type = 3;
            }
            if (m is FilePartMessage)
            {
                type = 4;
            }
            if (m is EndMessage)
            {
                type = 5;
            }
            if (m is ResponseMessage)
            {
                type = 6;
            }
            if (m is ErrorMessage)
            {
                type = 7;
            }

            // Буффер для передачи данных.
            byte[] bytes = new byte[1024];
            // Вспомогательная строка для составления пакета.
            string assis = string.Empty;
            // Формирование пакета.
            assis += type.ToString();
            assis += m.Get_Data();
            // Кодировка пакета.
            bytes = Encoding.ASCII.GetBytes(assis);

            return bytes;
        }

        // Раскрыть пакет.
        public Message Decrypt(byte[] b)
        {
            // Возвращаемое значение.
            Message mes;
            // Строка, полученная из полученного массива байт.
            string str = Encoding.ASCII.GetString(b);
            // Тип сообщений.
            int type = (int)str[0];
            // Строка данных.
            string assis = str.Substring(1, str.Length - 1);
            // Определение типа сообщения.
            switch (type)
            {
                case 0:
                    mes = new Inform_of_Down_Message(assis);
                    break;
                case 1:
                    mes = new Inform_of_Rec_Message(assis);
                    break;
                case 2:
                    mes = new ListMessage(assis);
                    break;
                case 3:
                    mes = new RequestMessage(assis);
                    break;
                case 4:
                    mes = new FilePartMessage(assis);
                    break;
                case 5:
                    mes = new EndMessage(assis);
                    break;
                case 6:
                    mes = new ResponseMessage(assis);
                    break;
                case 7:
                    mes = new ErrorMessage(assis);
                    break;
                default:
                    mes = new Message(assis);
                    break;
            }
            var m = new Inform_of_Rec_Message("asd");
            return m;
        }
    }

    // Стандартное сообщение.
    public class Message
    {
        // Определяет содержимое.
        protected string text;

        public Message(string s)
        {
            text = s;
        }

        // Возвращает содержимое.
        public string Get_Data()
        {
            return text;
        }

        public byte[] Get_Info()
        {
            byte[] b;
            b = Encoding.ASCII.GetBytes(text);
            return b;
        }
    }

    // Сообщение необзодимости загрузки файлов с сервера (1).
    public class Inform_of_Rec_Message : Message
    {
        public Inform_of_Rec_Message(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение необзодимости загрузки файлов на сервер (0).
    public class Inform_of_Down_Message : Message
    {
        public Inform_of_Down_Message(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение содержащее часть передаваемого файла (4).
    public class FilePartMessage : Message
    {
        public FilePartMessage(string s) : base(s)
        {
            text = s;
        }
        
    }

    // Сообщение содержащее имя файла(3).
    public class RequestMessage : Message
    {
        public RequestMessage(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение содержащее элемент списка файлов (2).
    public class ListMessage : Message
    {
        public ListMessage(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение об окончании коммуникационной сессии (5).
    public class EndMessage : Message
    {
        public EndMessage(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение об окончании коммуникационной сессии (6).
    public class ResponseMessage : Message
    {
        public ResponseMessage(string s) : base(s)
        {
            text = s;
        }
    }

    // Сообщение об окончании коммуникационной сессии (7).
    public class ErrorMessage : Message
    {
        public ErrorMessage(string s) : base(s)
        {
            text = s;
        }
    }
}
