using System.Collections.Generic;

using ImGuiNET;

namespace Catan.Event;

/// <summary>
/// Log of all game events
/// </summary>
class Log
{
    private Log()
    {
        m_EventLog = new List<Event>();
    }

    public void PostEvent(in Event e)
    {
        m_EventLog.Add(e);
    }

    public void DebugDrawUI()
    {
        if (!ImGui.BeginListBox("Log List"))
            return;
        
        foreach(Event ev in m_EventLog)
            ImGui.Text(ev.FormatMessage());

        ImGui.EndListBox();
    }

    private readonly List<Event> m_EventLog;

    public static Log Singleton
    {
        get
        {
            s_Singleton ??= new Log();

            return s_Singleton;
        }
    }

    private static Log s_Singleton = null;
}