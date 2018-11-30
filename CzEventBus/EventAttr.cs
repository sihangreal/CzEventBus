using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CzEventBus
{
    public class EventAttr : Attribute
    {
        private string eventType;
        public string EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }
        public EventAttr(string eventType)
        {
            this.eventType = eventType;
        }
        public EventAttr()
        {

        }
    }
}

