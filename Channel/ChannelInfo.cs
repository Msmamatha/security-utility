using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WirelessNodeSimulation.Channel
{
    class ChannelInfo
    {
        #region Variables
        string userName;
        string Ip;
        string Ptno;
        string UniqueKey,fname,efile;

        bool estat = false;
        bool sendsatus = false;
        #endregion

        public event EventHandler SendEvent;

        #region Propertise
        public string UserName
        {
            get { return userName; }

            set { userName = value; }
        }


        public string IPaddress
        {
            get { return Ip; }

            set {Ip = value; }
        }

        public string PortNumber
        {
            get { return Ptno; }

            set { Ptno = value; }
        }

        public string AESKey
        {
            get { return UniqueKey; }
            set { UniqueKey = value; }
        }

        public string filename
        {
            get { return fname; }

            set { fname = "C:\\UPRShare\\" + value; }
        }

        public string Encryptedfilename
        {
            get { return efile; }

            set { efile = value; }
        }

        public bool IsEncypted
        {
            get { return estat; }
            set { estat = value;
                if (!this.sendsatus)
                {
                  this.sendsatus = true;
                  if (this.SendEvent != null)
                      this.SendEvent(this, new EventArgs());

                }
            }
        }
        #endregion

    }
}
