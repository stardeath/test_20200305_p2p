using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
    public class Message : BindableBase
    {
        private bool m_IsReceivedMessage;
        public bool IsReceivedMessage
        {
            get
            {
                return m_IsReceivedMessage;
            }
            set
            {
                SetProperty(ref m_IsReceivedMessage, value);
            }
        }

        private string m_Data;
        public string Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                SetProperty(ref m_Data, value);
            }
        }

        public override string ToString()
        {
            if (IsReceivedMessage)
            {
                return "received message : " + Data;
            }
            else
            {
                return "sent message : " + Data;
            }
        }
    }
}
