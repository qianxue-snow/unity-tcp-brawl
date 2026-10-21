using System;
using System.Collections.Generic;
using EchoServer;

public class EventHandler
{
    //断开连接
    public static void OnDisconnect(ClientState c)
    {
        string desc = c.socket.RemoteEndPoint.ToString();
        //广播
        string sendStr = "Leave|" + desc + ',';
        foreach(ClientState cs in MainClass.clients.Values)
        {
            MainClass.Send(cs, sendStr);
        }

    }

}


