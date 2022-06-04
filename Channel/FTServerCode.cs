using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

using Crypter;

namespace Channel
{
    class FTServerCode
    {

        #region objectDeclaration
        IPEndPoint ipEnd;
        Socket sock;

        crypter Encryption = new crypter();

        FileEventargs fileargs = new FileEventargs();
        //EVENTS for Receivefile 
        public event EventHandler<FileEventargs> File_StartReceiving;
        public event EventHandler<FileEventargs> File_Receiving;
        public event EventHandler File_ReceveComplete;

        //EVENTS for RedirectReceive
        public event EventHandler<FileEventargs> File_StartRedirect;
        public event EventHandler<FileEventargs> File_Redirecting;
        public event EventHandler File_RedirectComplete;
        
        #endregion 

        #region variables
        string currfile = "";
        int fsize = 0;
        string rPath;
       public static string curMsg = "Not Started";


        #endregion


        #region Propertise
        public string ReceivedFile
        {
            get {
                    if (currfile != "")
                    {
                        return currfile;
                    }
                    else
                    {
                        return "none";
                    }
                }

            
        }

        public int FileSize
        {
            get { return fsize; }
        }

        public string ReceivedPath
        {
            get { return rPath; }
            set { rPath = value; }
        }
        #endregion
        public FTServerCode(int port)
        {
            try
            {
                ipEnd = new IPEndPoint(IPAddress.Any, port);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                sock.Bind(ipEnd);
            }
            catch (SocketException ert)
            {
                MessageBox.Show("Client and Server cannot be in same system");
            }
        }
       

        #region Function
        public void ReceiveFile()
        {
            try
            {
                curMsg = "Starting...";
                sock.Blocking = true;
                sock.Listen(-1);
              
                curMsg = "Running and waiting to receive file.";
                Socket clientSock = sock.Accept();
               
                byte[] clientData = new byte[1024];
                
                    int receivedBytesLen = clientSock.Receive(clientData);
                    curMsg = "Receiving data...";
                    int len = 0;
                    int fileNameLen =Convert.ToInt32( ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                    len = len + 4;
                    string fileName = Encoding.ASCII.GetString(clientData, len, fileNameLen);
                    len = 4 + fileNameLen;
                    int s_len = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                    len = len + 4;

                    int size = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, s_len));
                    currfile = fileName;
                    fsize = size;

                //Starting Event
                    EventHandler<FileEventargs> file_start = File_StartReceiving;
                    if (file_start != null)
                    {
                        fileargs = new FileEventargs(0, fsize, currfile);
                        this.File_StartReceiving(this, fileargs);
                    }

                    FileStream fstream = new FileStream(rPath + "/" + fileName, FileMode.Append, FileAccess.Write);
                    NetworkStream ns = new NetworkStream(clientSock);

                    string privateKey = File.ReadAllText(@"C:\tempKey\PrivateKey.xml");
                for (int i = 0; i < size; )
                    {
                        byte[] fdata = new byte[10*1024];

                        int ll = ns.Read(fdata, 0, fdata.Length);
                        if (ll == 0)
                            break;
                       
                        if (fdata != null)
                        {
                           
                            fstream.Flush();
                            try
                            {
                                string cipher = ASCIIEncoding.ASCII.GetString(fdata);
                                string plain = Encryption.RSADecryptString(cipher, privateKey);
                                byte[] actualdata = Convert.FromBase64String(plain);
                            }
                            catch { }
                            fstream.Write(fdata, 0, fdata.Length);

                            ll = fdata.Length;
                            i = i + ll;
                            curMsg = ll.ToString();

                            EventHandler<FileEventargs> file_Rec = File_Receiving;
                            if (file_Rec != null)
                            {
                                fileargs = new FileEventargs(i, fsize, currfile);
                                this.File_Receiving(this, fileargs);
                            }
                           
                        }
                    }
                fstream.Close();
                curMsg = "Saving file...";
                               
                clientSock.Close();
                if (File_ReceveComplete != null)
                {
                    this.File_ReceveComplete(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                curMsg = ex.Message;
            }
        }
      //  IPEndPoint ipEnd;
      //  Socket sock;
        public static string ss = "";
        public FTServerCode()
        {
            try
            {
                ipEnd = new IPEndPoint(IPAddress.Any, 5656);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                sock.Bind(ipEnd);
            }
            catch (SocketException ert)
            {
                MessageBox.Show("Client and Server cannot be in same system");
            }
        }
        public static string receivedPath;
        //public static string curMsg = "Stopped";
        public void StartServer()
        {
            try
            {
                curMsg = "Starting...";
                sock.Blocking = true;
                sock.Listen(-1);

                curMsg = "Running and waiting to receive file.";
                Socket clientSock = sock.Accept();

                byte[] clientData = new byte[1024];

                int receivedBytesLen = clientSock.Receive(clientData);
                curMsg = "Receiving data...";
                int len = 0;
                int fileNameLen = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                len = len + 4;
                string fileName = Encoding.ASCII.GetString(clientData, len, fileNameLen);
                len = 4 + fileNameLen;
                int s_len = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                len = len + 4;

                int size = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, s_len));
                ss += fileName + "::" + size;


                FileStream fstream = new FileStream(receivedPath + "/" + fileName, FileMode.Append, FileAccess.Write);
                NetworkStream ns = new NetworkStream(clientSock);
                //BinaryWriter bWrite = new BinaryWriter(File.Open(receivedPath + "/" + fileName, FileMode.Append));
                for (int i = 0; i < size; )
                {
                    byte[] fdata = new byte[1024];

                    int ll = ns.Read(fdata, 0, fdata.Length);
                    fstream.Flush();
                    fstream.Write(fdata, 0, ll);
                    i = i + ll;
                    curMsg = ll.ToString();
                    //bWrite.Write(fdata, 0, fdata.Length);

                }
                fstream.Close();
                //bWrite.Close();
                //bWrite.Write(clientData, 4 + fileNameLen, receivedBytesLen - 4 - fileNameLen);

                curMsg = "Saving file...";

                //bWrite.Close();
                clientSock.Close();
                MessageBox.Show("File Received and saved....");
            }
            catch (Exception ex)
            {
                curMsg = ex.Message;
            }
        }
        public void redirectRecive(string ip,int port)
        {
            try
            {

                IPAddress ipadd = IPAddress.Parse(ip);
                IPEndPoint ipEnd = new IPEndPoint(ipadd, port);
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                clientSocket.Blocking = true;

               
                sock.Blocking = true;
                sock.Listen(-1);

                
                Socket clientSock = sock.Accept();

                byte[] clientData = new byte[1024];

                clientSocket.Connect(ipEnd);
                
                int receivedBytesLen = clientSock.Receive(clientData);
                
                int len = 0;
                int fileNameLen = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                len = len + 4;
                string fileName = Encoding.ASCII.GetString(clientData, len, fileNameLen);
                len = 4 + fileNameLen;
                int s_len = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, 4));
                len = len + 4;

                int size = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(clientData, len, s_len));
                currfile = fileName;
                fsize = size;


                clientSocket.Send(clientData);
                //Starting Event
                EventHandler<FileEventargs> file_start = File_StartRedirect;
                if (file_start != null)
                {
                    fileargs = new FileEventargs(0, fsize, currfile);
                    this.File_StartRedirect(this, fileargs);
                }

               
                NetworkStream ns = new NetworkStream(clientSock);
                NetworkStream nsredirect = new NetworkStream(clientSocket);
                //BinaryWriter bWrite = new BinaryWriter(File.Open(receivedPath + "/" + fileName, FileMode.Append));
                for (int i = 0;i<size; )
                {
                    byte[] fdata = new byte[10*1024];

                    int ll = ns.Read(fdata, 0, fdata.Length);
                    if (ll == 0)
                        break;
                    i = i + ll;
                    nsredirect.Write(fdata, 0, fdata.Length);
                    EventHandler<FileEventargs> file_Rec = File_Redirecting;
                    if (file_Rec != null)
                    {
                        fileargs = new FileEventargs(i, fsize, currfile);
                        this.File_Redirecting(this, fileargs);
                    }
                   

                }
               
                nsredirect.Close();
               
                clientSock.Close();
                clientSocket.Close();
                if (File_ReceveComplete != null)
                {
                    this.File_ReceveComplete(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        #endregion
    }

    class FileEventargs : EventArgs
    {
        #region Variables
        int receivedsize = 0,totSize=0;
        string fname = "none";
        #endregion

        #region Propertise
        public int ReceivedBytes
        {
            get { return receivedsize; }
            
        }

        public int TotalSize
        {
            get { return totSize; }
        }

        public string FileName
        {
            get { return fname; }
        }
        #endregion

        public FileEventargs()
        {
            receivedsize = 0;
            totSize = 0;
            fname = "none";
        }
        public FileEventargs(int receivedSize, int fileSize, string Filename)
        {
             receivedsize = receivedSize;
            totSize = fileSize;
            fname = Filename;
        }


    }
}
