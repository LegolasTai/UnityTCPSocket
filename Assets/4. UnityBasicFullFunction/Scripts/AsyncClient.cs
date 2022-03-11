using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;

namespace UnityBasicFullFunction
{
    public class AsyncClient : MonoBehaviour
    {
        public string StrMsg = "This is a test";

        private const int port = 11000;

        private static String response = String.Empty;

        [SerializeField]
        SocketData sd;

        float fDisconnectTime;
        float fDisconnectTimeout = 5f;


        enum ConnectState {UnConnected, Connecting, Connected, Disconnected }

        private ConnectState state = ConnectState.UnConnected;
        private ConnectState State
        {
            get { return state; }
            set
            {
                state = value;
                if(value == ConnectState.Disconnected)
                {
                    fDisconnectTime = Time.time;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            StartClient();
        }

        private void Update()
        {

            if (sd.Socket != null  && state== ConnectState.Connected)
            {
                if(!sd.Socket.IsConnected())
                {
                    sd.Ns.Close();
                    sd.Socket.Close();
                    State = ConnectState.Disconnected;
                    return;
                }
                if (sd.Ns.DataAvailable)
                {
                    string strData = sd.Sr.ReadLine();
                    if (strData != null)
                    {
                        Debug.Log("Received: " + strData);
                    }
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    Send();
                }
            }
            else if(State == ConnectState.Disconnected && Time.time > (fDisconnectTime+fDisconnectTimeout))
            {
                StartClient();
            }
        }

        public void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);

                State = ConnectState.Connecting;

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                State = ConnectState.Disconnected;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                sd = new SocketData(client);

                state = ConnectState.Connected;
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                state = ConnectState.Disconnected;
            }
        }

        private void Send()
        {
            sd.Sw.WriteLine(StrMsg);
            sd.Sw.Flush();
            Debug.Log("Sent: " + StrMsg);
        }


        private void ReleaseSocket()
        {
            sd.Socket.Shutdown(SocketShutdown.Both);
            sd.Socket.Close();
        }

        private void OnApplicationQuit()
        {
            ReleaseSocket();
        }
    }
}

