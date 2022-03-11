using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System;
using System.Net;
using System.Net.Sockets;

using System.Text;

namespace AsynchronousVersionInCallback
{
    public class AsyncClient : MonoBehaviour
    {
        public string StrMsg = "This is a test";

        private const int port = 11000;

        private static String response = String.Empty;

        Socket client;

        // Start is called before the first frame update
        void Start()
        {
            StartClient();
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

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.Log(string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));

                Send(client, StrMsg + "<EOF>");

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                        Debug.Log(string.Format("Response received : {0}", response));

                        ReleaseSocket();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.Log(string.Format("Sent {0} bytes to server.", bytesSent));


                Receive(client);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ReleaseSocket()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}