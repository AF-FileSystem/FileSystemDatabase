using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{
    // Стандартное сообщение.
    class Message
    {
        protected string text;
        protected int type;
        public Message(string s)
        {
            text = s;
        }
        public byte[] Encode()
        {
            byte[] b = new byte[1024];
            string s = string.Empty;
            s += type.ToString();
            s += text;
            b = Encoding.ASCII.GetBytes(s);
            return b;
        }
    }

    // Сообщение необзодимости загрузки файлов с сервера.
    class Inform_of_Rec_Message : Message
    {
        public Inform_of_Rec_Message(string s) : base(s)
        {
            text = s;
            type = 1;
        }
    }

    // Сообщение необзодимости загрузки файлов на сервер.
    class Inform_of_Down_Message : Message
    {
        public Inform_of_Down_Message(string s) : base(s)
        {
            text = s;
            type = 0;
        }
    }

    // Сообщение содержащее часть передаваемого файла.
    class FilePartMessage : Message
    {
        public FilePartMessage(string s, int i) : base(s)
        {
            text = s;
            type = 4;
        }
    }

    // Сообщение содержащее имя файла.
    class RequestMessage : Message
    {
        public RequestMessage(string s, int i) : base(s)
        {
            text = s;
            type = 3;
        }
    }

    // Сообщение содержащее элемент списка файлов.
    class ListMessage : Message
    {
        public ListMessage(string s, int i) : base(s)
        {
            text = s;
            type = 2;
        }
    }

    // Обработчик сообщений.
    class Message_Handler
    {
        public Message_Handler()
        {

        }

        // Получить пакет.
        public byte[] Encrypt(Message m)
        {
            byte[] bytes = new byte[1024];
            return bytes;
        }

        // Раскрыть пакет.
        public Message Decrypt(byte[] bytes)
        {
            Message m = null;
            return m;
        }
    }
}
