using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace InstantPhotosServer
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public ICommand SetPathCommand { get; set; }

        public ICommand ServerToggleCommand { get; set; }

        private TcpListener dataListener;
        /// <summary>
        /// Listens to data
        /// </summary>
        public TcpListener DataListener
        {
            get
            {
                return this.dataListener;
            }
            set
            {
                if (value == dataListener)
                {
                    return;
                }
                this.dataListener = value;
                ServerStatus = value == null ? false : true;
            }
        }

        /// <summary>
        /// Contains client connection info
        /// </summary>
        public ObservableCollection<NetworkClient> ConnectedClients { get; set; } = new ObservableCollection<NetworkClient>();

        /// <summary>
        /// default server data port
        /// </summary>
        private const int dataPort = 1337;

        /// <summary>
        /// default server data address
        /// </summary>
        private string serverAdress = null;

        /// <summary>
        /// Contains server network node identifier that is used on mobile to connect
        /// </summary>
        public string MagicNumber { get; set; }

        /// <summary>
        /// SavePath for recieved photos
        /// </summary>
        public string SavePath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Stores instance of this viewmodel
        /// </summary>
        static private ViewModel instance = null;

        /// <summary>
        /// Server status
        /// </summary>
        public bool ServerStatus { get; set; }


        private ViewModel()
        {
            this.SetPathCommand = new RelayCommand<object>((dummy)=> 
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = SavePath;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SavePath = dialog.FileName;
                }

            },null);
            this.ServerToggleCommand = new RelayCommand<ToggleButton>((sender) =>
            {
                if ((bool)sender.IsChecked)
                {
                    StartServer();
                }
                else
                {
                    StopServer();
                }
            }, null);
            this.serverAdress = GetLocalIpAddress();
            this.MagicNumber = serverAdress.Substring(serverAdress.LastIndexOf('.')+1);
            this.DataListener = new TcpListener(IPAddress.Parse(serverAdress), dataPort);
            StartServer();
            
        }

        static public ViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new ViewModel();
            }
            return instance;
        }

        /// <summary>
        /// Starts server and runs listener
        /// </summary>
        public void StartServer()
        {    
            DataListener.Start();
            ServerStatus = true;
            Task.Run(Listen);
        }

        public void StopServer()
        {
            DataListener.Stop();
            ServerStatus = false;
            ConnectedClients.Clear();
        }

        /// <summary>
        /// Listens for connections and creates a thread for each client
        /// </summary>
        private void Listen()
        {
            try
            {
                while (ServerStatus)
                {
                    TcpClient clientData = DataListener?.AcceptTcpClient();
                    if (clientData != null)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            ConnectedClients.Add(new NetworkClient(clientData.Client.RemoteEndPoint.ToString()));
                        });
                        Task.Run(() => ClientHandler(clientData));
                    }
                }
            }
            catch // server went offline
            {
                MessageBox.Show("Server is offline, connection listening stopped", "Message");
            }
        }

        /// <summary>
        /// Handles everything from client
        /// </summary>
        /// <param name="commandClient"></param>
        /// <param name="dataClient"></param>
        private void ClientHandler(TcpClient dataClient)
        {
            string clientAddress = dataClient.Client.RemoteEndPoint.ToString();
            try
            {               
                using (NetworkStream dataStream = dataClient.GetStream())
                {
                    while (ServerStatus)
                    {
                        byte[] networkBuffer = new byte[4096]; //buff

                        byte[] package;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int numBytesRead = dataStream.Read(networkBuffer, 0, networkBuffer.Length);
                            if (!ServerStatus)
                            {
                                return;
                            }
                            if (numBytesRead == 0)
                            {
                                break;
                            }
                            int totalSize = BitConverter.ToInt32(networkBuffer, 0);
                            ms.Write(networkBuffer, 0, numBytesRead);
                            int readNow;
                            while ((numBytesRead += (readNow = dataStream.Read(networkBuffer, 0, networkBuffer.Length))) != totalSize)
                            {
                                ms.Write(networkBuffer, 0, readNow);
                            }
                            ms.Write(networkBuffer, 0, readNow);
                            package = ms.ToArray();
                        }


                        int nameLen = 0;
                        for (nameLen = 0; package[4 + nameLen] != '/'; nameLen++);
                        string filename = Encoding.ASCII.GetString(package, 4, nameLen);
                        byte[] imageStream = new byte[package.Length - 5 - nameLen];
                        Array.Copy(package, nameLen + 5, imageStream, 0, imageStream.Length);
                        File.WriteAllBytes(SavePath + '/' + filename, imageStream);
                        NetworkClient n = ConnectedClients.Single(c => c.Name == clientAddress);
                        n.SentFiles++;
                        n.TotalSize += imageStream.Length;
                    }
                }               
            }
            finally
            {
                if (ServerStatus) // if connection is still present
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                    {
                        ConnectedClients.Remove(ConnectedClients.Single(c => c.Name == clientAddress));
                    });
                }        
            }

        }

        /// <summary>
        /// Retrieves local ip address
        /// </summary>
        /// <returns></returns>
        private string GetLocalIpAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
    }
}
