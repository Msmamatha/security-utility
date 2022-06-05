using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace WirelessNodeSimulation
{
    class Brodcast_Sender
    {
        Boolean done = false;
        Boolean exception_thrown = false;
        Socket sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
              
        //Brod cast setting
        IPEndPoint send_to_address=null;
        IPEndPoint sending_end_point=null;
      


        public Brodcast_Sender(string gatewayip, int port)
        {

            send_to_address = new IPEndPoint(IPAddress.Broadcast,port);
   sending_end_point = new IPEndPoint(IPAddress.Parse(gatewayip.Substring(0,8)+"255.255"),port);
        }

        public void Brodcast(string msg)
        {          
                
                if (msg.Length == 0)
                {
                    done = true;
                }
                else
                {
                    sending_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    byte[] send_buffer = Encoding.ASCII.GetBytes(msg);
                    try
                    {
                        sending_socket.SendTo(send_buffer, send_to_address);
                        sending_socket.SendTo(send_buffer, sending_end_point);

                    }
                    catch (Exception send_exception)
                    {
                        exception_thrown = true;
                        MessageBox.Show(send_exception.Message,"ERROR",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                    if (exception_thrown == false)
                    {
                        Trace.WriteLine("Message broadcast successfull");
                        
                    }
                    
                }
            } 
        }


        
        
    }

