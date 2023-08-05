using System.Collections.Generic;

namespace Catan;

class Log
{
    public Log()
    {
        m_EventLog = new List<Event>();
    }

    public void PostEvent(Event e)
    {
        m_EventLog.Add(e);
    }

    private List<Event> m_EventLog;
}