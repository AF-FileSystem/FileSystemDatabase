using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{
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

    class Inform_of_Rec_Message : Message
    {
        public Inform_of_Rec_Message(string s) : base(s)
        {
            text = s;
            type = 1;
        }
    }

    class Inform_of_Down_Message : Message
    {
        public Inform_of_Down_Message(string s) : base(s)
        {
            text = s;
            type = 0;
        }
    }

    class FilePartMessage : Message
    {
        public FilePartMessage(string s, int i) : base(s)
        {
            text = s;
            type = 4;
        }
    }

    class RequestMessage : Message
    {
        public RequestMessage(string s, int i) : base(s)
        {
            text = s;
            type = 3;
        }
    }

    class ListMessage : Message
    {
        public ListMessage(string s, int i) : base(s)
        {
            text = s;
            type = 2;
        }
    }
}
