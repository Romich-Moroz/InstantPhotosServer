using System.ComponentModel;
using System;

namespace InstantPhotosServer
{
    public class NetworkClient : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public string Name { get; set; }

        public string ConnectionTime { get; set; }

        public int SentFiles { get; set; }

        public int TotalSize { get; set; }

        public NetworkClient(string name)
        {
            this.Name = name;
            this.SentFiles = 0;
            this.TotalSize = 0;
            this.ConnectionTime = DateTime.Now.ToString();
        }    
    }
}
