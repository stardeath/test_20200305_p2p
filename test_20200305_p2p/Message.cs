using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_20200305_p2p
{
    public class Message : BindableBase
    {
        public enum StatusDesc
        {
            None = 0,
            Sending,
            Sent,
            Received,
            Technical
        }

        private StatusDesc m_Status;
        public StatusDesc Status
        {
            get
            {
                return m_Status;
            }
            set
            {
                SetProperty(ref m_Status, value);
                RaisePropertyChanged( "Value" );
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

        private int m_Id = -1;
        public int Id
        {
            get
            {
                return m_Id;
            }
            set
            {
                SetProperty( ref m_Id, value );
            }
        }

        public string Value
        {
            get
            {
                switch( Status )
                {
                    case StatusDesc.Sending:
                        return "sending : " + Data;
                    case StatusDesc.Sent:
                        return "sent : " + Data;
                    case StatusDesc.Received:
                        return "received : " + Data;
                    case StatusDesc.Technical:
                        return "technical : " + Data;
                }
                return Data;
            }
        }
    }
}
