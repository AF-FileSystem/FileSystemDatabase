using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages
{
    public class Message
    {
        protected string text;
        public Message(string s)
        {
            text = s;
        }
    }

    public class InformMessage : Message
    {
        public InformMessage(string s) : base(s)
        {
            text = s;
        }
    }

    public class FilePartMessage : Message
    {
        public FilePartMessage(string s) : base(s)
        {
            text = s;
        }
    }

    public class RequestMessage : Message
    {
        public RequestMessage(string s) : base(s)
        {
            text = s;
        }
    }

    public class ListMessage : Message
    {
        public ListMessage(string s) : base(s)
        {
            text = s;
        }
    }
}
