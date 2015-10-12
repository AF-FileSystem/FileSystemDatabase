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
        public Message(string s)
        {
            text = s;
        }
    }

    class InformMessage : Message
    {
        public InformMessage(string s) : base(s)
        {
            text = s;
        }
    }

    class FilePartMessage : Message
    {
        public FilePartMessage(string s) : base(s)
        {
            text = s;
        }
    }

    class RequestMessage : Message
    {
        public RequestMessage(string s) : base(s)
        {
            text = s;
        }
    }

    class ListMessage : Message
    {
        public ListMessage(string s) : base(s)
        {
            text = s;
        }
    }
}
