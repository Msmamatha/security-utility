using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using Crypter;

namespace Channel
{
    class FTClientCode
    {
        public static string curMsg = "Idle";
        public static string ip = null;
        public int pcktsize = 10*1024;
        public string Publickey = "none";
        crypter Encryption = new crypter();

        public static int pcktsize1 = 1024 * 1024 * 1024;



        public void SendFile(string fileName)
        {
            try
            {
                if (ip == null)
                {
                    ip = "127.0.0.1";
                }

                IPAddress ipadd = IPAddress.Parse(ip);
                IPEndPoint ipEnd = new IPEndPoint(ipadd, 5656);
                Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                clientSock.Blocking = true;
                byte[] clientData = new byte[1024];



                curMsg = "Connection to server ...";
                clientSock.Connect(ipEnd);

                FileInfo fin = new FileInfo(fileName);
                //getting filename attribute
                byte[] dd = ASCIIEncoding.ASCII.GetBytes(fin.Name);
                byte[] ss = ASCIIEncoding.ASCII.GetBytes(dd.Length.ToString());
                ss.CopyTo(clientData, 0);
                dd.CopyTo(clientData, 4);

                //getting size attribute
                byte[] size = ASCIIEncoding.ASCII.GetBytes(fin.Length.ToString());
                byte[] size_length = ASCIIEncoding.ASCII.GetBytes(size.Length.ToString());

                size_length.CopyTo(clientData, dd.Length + 4);
                size.CopyTo(clientData, dd.Length + 4 + 4);

                curMsg = "File attribute sending.....";
                clientSock.Send(clientData);                                                 //send file information that is 1st packet

                FileStream fstrem = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                NetworkStream nfs = new NetworkStream(clientSock);
                curMsg = "File data sending.....";
               
                nfs.Flush();
                Publickey = File.ReadAllText(@"C:\tempKey\PublicKey.xml");
                for (int i = 0; i < fin.Length && nfs.CanWrite; i += pcktsize)
                {
                    byte[] fdata;
                    if ((fin.Length - i) / pcktsize == 0)
                    {
                        fdata = new byte[fin.Length - i];
                    }
                    else
                    {
                        fdata = new byte[pcktsize];
                    }
                    fstrem.Position = long.Parse(i.ToString());
                    fstrem.Read(fdata, 0, fdata.Length);
                    string fdatatoString = Convert.ToBase64String(fdata);
                    string encryptedString = Encryption.RSAEncryptString(fdatatoString, Publickey);

                    byte[] networkData = ASCIIEncoding.ASCII.GetBytes(encryptedString);
                    networkData = fdata;
                    nfs.Write(networkData, 0, networkData.Length);//writing into socket

                    
                    
                }
                nfs.Close();
                curMsg = "Disconnecting...";
                clientSock.Close();
                curMsg = "File transferred.";
                fstrem.Close();

            }
            catch (Exception ex)
            {
                
                if (ex.Message == "No connection could be made because the target machine actively refused it")
                    curMsg = "File Sending fail. Because server not running.";
                else
                    curMsg = "File Sending fail." + ex.Message;

                Thread.CurrentThread.Abort();
            }

        }
        public static void SendFile1(string fileName)
        {
            try
            {
                //IPAddress[] ipAddress = Dns.GetHostAddresses("localhost");

                IPAddress ipadd = IPAddress.Parse(ip);
                IPEndPoint ipEnd = new IPEndPoint(ipadd, 5656);
                Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                clientSock.Blocking = true;
                byte[] clientData = new byte[1024];



                curMsg = "Connection to server ...";
                clientSock.Connect(ipEnd);

                FileInfo fin = new FileInfo(fileName);
                //getting filename attribute
                byte[] dd = ASCIIEncoding.ASCII.GetBytes(fin.Name);
                byte[] ss = ASCIIEncoding.ASCII.GetBytes(dd.Length.ToString());
                ss.CopyTo(clientData, 0);
                dd.CopyTo(clientData, 4);

                //getting size attribute
                byte[] size = ASCIIEncoding.ASCII.GetBytes(fin.Length.ToString());
                byte[] size_length = ASCIIEncoding.ASCII.GetBytes(size.Length.ToString());

                size_length.CopyTo(clientData, dd.Length + 4);
                size.CopyTo(clientData, dd.Length + 4 + 4);

                curMsg = "File attribute sending.....";
                clientSock.Send(clientData);
                FileStream fstrem = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                NetworkStream nfs = new NetworkStream(clientSock);
                curMsg = "File data sending.....";
                int yy = 0;
                for (int i = 0; i < fin.Length && nfs.CanWrite; i += pcktsize1)
                {
                    byte[] fdata;
                    if ((fin.Length - i) / pcktsize1 == 0)
                    {
                        fdata = new byte[fin.Length - i];
                    }
                    else
                    {
                        fdata = new byte[pcktsize1];
                    }
                    fstrem.Position = long.Parse(i.ToString());
                    fstrem.Read(fdata, 0, fdata.Length);
                    nfs.Write(fdata, 0, fdata.Length);

                }

                curMsg = "Disconnecting...";
                clientSock.Close();
                curMsg = "File transferred.";

            }
            catch (Exception ex)
            {
                if (ex.Message == "No connection could be made because the target machine actively refused it")
                    curMsg = "File Sending fail. Because server not running.";
                else
                    curMsg = "File Sending fail." + ex.Message;
            }

        }
    }
}
