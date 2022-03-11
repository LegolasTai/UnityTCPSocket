using UnityEngine;

using System.Net.Sockets;
using System;

public static class SocketExtensions
{
    static int iReceiveCount;
    public static bool IsConnected(this Socket socket)
    {
        try
        {
            if (socket != null && socket.Connected)
            {
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    iReceiveCount = socket.Receive(new byte[1], SocketFlags.Peek);

                    return !(socket.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                //send empty will block the client
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Disconnected: " + e);
            return false;
        }
    }
}