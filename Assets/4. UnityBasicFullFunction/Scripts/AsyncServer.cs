using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

namespace UnityBasicFullFunction
{
    public class AsyncServer : MonoBehaviour
    {

        Socket listener;
        static System.Diagnostics.Stopwatch sw;

        [SerializeField]
        List<SocketData> lClientSocketData = new List<SocketData>();

        [SerializeField]
        List<SocketData> lDisconnectedSocketData = new List<SocketData>();

        // Start is called before the first frame update
        void Start()
        {
            sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            StartListening();
        }

        private void Update()
        {
            foreach (SocketData sd in lClientSocketData)
            {
                if (!sd.Socket.IsConnected())
                {
                    lDisconnectedSocketData.Add(sd);
                    continue;
                }

                if (sd.Ns.DataAvailable)
                {
                    string strData = sd.Sr.ReadLine();

                    if (strData != null)
                    {
                        Debug.Log("Received: " + strData);
                    }
                }
            }

            for(int i = lDisconnectedSocketData.Count -1; i>=0; i--)
            {
                lClientSocketData.Remove(lDisconnectedSocketData[i]);

                lDisconnectedSocketData[i].Ns.Close();
                lDisconnectedSocketData[i].Socket.Close();

                lDisconnectedSocketData.RemoveAt(i);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                BroadCast();
            }
        }

        public void BroadCast()
        {
            foreach (SocketData sd in lClientSocketData)
            {
                try
                {
                    sd.Sw.WriteLine("hihi");
                    sd.Sw.Flush();
                    Debug.Log("Sent: hihi");
                }
                catch(Exception e)
                {
                    Debug.Log("Write fail: "+e);
                }
            }
        }


        public void StartListening()
        {
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                StartAccept();
                Debug.Log("Waiting for connections");

            }
            catch (Exception e)
            {
                Debug.Log("Listen fail: "+e.ToString());
            }
        }

        public void StartAccept()
        {
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }


        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            lClientSocketData.Add(new SocketData(handler));

            Debug.Log(lClientSocketData.Count + " servers is connected");

            StartAccept();
        }

        private void ReleaseSocket()
        {
            foreach (SocketData sd in lClientSocketData)
            {
                sd.Ns.Close();
                sd.Socket.Shutdown(SocketShutdown.Both);
                sd.Socket.Close();
            }
            listener.Close();
        }

        private void OnApplicationQuit()
        {
            ReleaseSocket();
        }

    }

}