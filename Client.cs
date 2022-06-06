using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
 

using System.Net.Sockets;
using System.Net;

namespace WirelessNodeSimulation
{
    public class Client
    {

        protected Socket _clientSocket;
        //buffer d'envoi
        protected byte[] _sendBuffer;
        // buffer de reception
        protected byte[] _receiveBuffer;
        //savoir si le client est connectÃ©
        protected bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
        }
        // gestionnaire de paquet
        
        // constructeur
        public Client()
        {
            _sendBuffer = new byte[1024];
            _receiveBuffer = new byte[1024];
           
        }

        public delegate void Error(string label, string error);
        public Error NetworkError;

        // connectÃ© le client
        public void Connect()
        {
            try
            {
                //si il n'est pas deja connectÃ©
                if (!_isConnected)
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.102"), 4444);
                    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.UserToken = _clientSocket;
                    args.RemoteEndPoint = endPoint;
                    args.Completed += new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(OnConnect);
                    //connection
                    _clientSocket.ConnectAsync(args);
                }
            }
            catch (Exception)
            {
                // Do What you want in case of error Error("Error Connect", e.Message);
            }
        }

        //callback une fois connectÃ©
        protected void OnConnect(object sender, System.Net.Sockets.SocketAsyncEventArgs args)
        {
            try
            {
                // test de connection
                _isConnected = (args.SocketError == SocketError.Success && _clientSocket.Connected);
                //si on est connectÃ©
                if (_isConnected)
                { //on indique quoi faire pour recevoir
                    args.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
                    args.Completed -= new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(OnConnect);
                    args.Completed += new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(this.OnReceive);
                    //reception asynchrone
                    _clientSocket.ReceiveAsync(args);
                    // Do What you want in case of error Error("okay", "connected");

                }
                else
                {
                    if (NetworkError != null) NetworkError("You Cannot connect to host", args.SocketError.ToString());
                }
            }
            catch (Exception e)
            {
                // Do What you want in case of error Error("Error OnConnect", e.Message);
            }
        }
        //en case de reception

        protected void OnReceive(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError == SocketError.Success && args.BytesTransferred != 0)
                {
                    //on ajoute ce qui est recu dans le
                    //gestionnaire de paquet
                   // PacketManager.AddPacketData(args.Buffer, args.BytesTransferred);
                    //on en veut encore
                    _clientSocket.ReceiveAsync(args);
                }
                else
                {
                    //deconnexion 
                    Disconnect();
                }
            }
            catch (Exception)
            {
                // Do What you want in case of error Error("Error OnReceive", e.Message);

            }
        }
        //envoi de paquet

        public void Send(string data)
        {
            if (data.Length != 0 && _isConnected)
            {
                SendPacket(_clientSocket, data);
            }
        }
        public static void SendPacket(Socket client, byte[] data)
        {
            try
            {
                //envoi asynchrone 
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(data, 0, data.Length);
                client.SendAsync(e);
            }
            catch (Exception)
            {
                // Do What you want in case of error Error("Error Sending data", ex.Message);
            }
        }

        //envoi de string
        public static void SendPacket(Socket client, string data)
        {
            SendPacket(client, Encoding.UTF8.GetBytes(data));
        }

        //deconnexion
        public void Disconnect()
        {
            try
            {
                if (_clientSocket != null)
                {
                    lock (_clientSocket)
                    {
                        _clientSocket.Shutdown(SocketShutdown.Both);
                        _clientSocket.Close();
                        _clientSocket = null;
                    }
                    // Do What you want in case of error Error("Error", "You have been disconnected");

                }
            }
            catch (Exception)
            {
                // Do What you want in case of error Error("Error", e.Message);
            }
            _isConnected = false;
        }
        public void Send_Message(int SourceNodeId, int DestinationNodeId, int MessageId, string NodePath,string Type)
        {
            //   message.Replace('$', '?');
            Send(String.Format("{0}${1}${2}${3}${4}#", Convert.ToString(SourceNodeId), Convert.ToString(DestinationNodeId), Convert.ToString(MessageId), NodePath, Type));
        }

    }
}