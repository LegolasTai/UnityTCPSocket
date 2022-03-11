using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;

public class SyncClient : MonoBehaviour
{
    public string StrMsg = "This is a test";
    // Start is called before the first frame update
    void Start()
    {
        Thread t = new Thread(SynchronousSocketClient.StartClient);

        t.Start(this);

    }
}

public class SynchronousSocketClient
{

    public static void StartClient(object obj)
    {
        SyncClient sc = (SyncClient)obj;

        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Debug.Log(string.Format("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString()));

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes( sc.StrMsg + "<EOF>" );

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);
                Debug.Log(string.Format("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec)));

                // Release the socket.  
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Debug.Log(string.Format("ArgumentNullException : {0}", ane.ToString()));
            }
            catch (SocketException se)
            {
                Debug.Log(string.Format("SocketException : {0}", se.ToString()));
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Unexpected exception : {0}", e.ToString()));
            }

        }
        catch (Exception e)
        {
            Debug.Log("e: " + e.ToString());
        }
    }

}