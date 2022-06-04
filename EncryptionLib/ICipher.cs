
 
using System;
namespace EncryptionLib
{
    delegate void ProgressEventHandler(object o, ProgressEventArgs args);

    interface ICipher
    {     
        void Decrypt(string SourceFileName, string DestinationFileName, string Key);
        
        byte[] Decrypt(byte[] BytesInput, string Key);
        
        byte[] Encrypt(byte[] BytesInput, string Key);
        
        void Encrypt(string SourceFileName, string DestinationFileName, string Key);
    }

    class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs()
        { }

        public ProgressEventArgs(int i)
        { progress = i; }

        public ProgressEventArgs(int i, bool b)
        { progress = i; stop = b; }

        public int progress;
        public bool stop;
    }
}
