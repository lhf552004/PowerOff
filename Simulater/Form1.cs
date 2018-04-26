using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simulater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Socket SocketClient;
        IPEndPoint EndPoint;
        public byte[] ArrMsgRec { get; private set; }
        private byte[] messageBytes;
        public int DecoderBufferSize => 0x4000;
        private void start(string ip, int port)
        {
            var Ip = IPAddress.Parse(ip);
            EndPoint = new IPEndPoint(Ip, port);
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           // State = ConnectionState.None;
            //_sendDataMessageCompleteHandler = SendDataMessageCompleteHandler;
            ArrMsgRec = new byte[DecoderBufferSize];
            messageBytes = new byte[DecoderBufferSize];
            //DeviceId = 0;
            Connect();
        }
        public bool Connect()
        {
            bool connected = false;
            //State = ConnectionState.Connecting;
            try
            {
                SocketClient.Connect(EndPoint);
                //State = ConnectionState.Connected;
                var receiveCompleteEvent = new SocketAsyncEventArgs();
                receiveCompleteEvent.SetBuffer(ArrMsgRec, 0, ArrMsgRec.Length);
                receiveCompleteEvent.Completed += SocketReceiveEventCompleted;

                if (!SocketClient.ReceiveAsync(receiveCompleteEvent))
                {

                    SocketReceiveEventCompleted(SocketClient, receiveCompleteEvent);
                }
                //Connected(SocketClient, this);
                connected = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return connected;

        }
        public Task SendDataMessage(string msg)
        {

            try
            {
                byte[] theByte = System.Text.Encoding.Default.GetBytes(msg);
                SocketClient.Send(theByte, SocketFlags.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
               // Disconnected(SocketClient, this);

            }

            //if (!sockClient.SendAsync(eap))
            //    SendDataMessageCompleteHandler(sockClient, eap);
            return null;

        }
        private void SocketReceiveEventCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                var ex = new SocketException((int)e.SocketError);

                return;
            }

            try
            {
                //_timer8.Change(Timeout.Infinite, Timeout.Infinite);
                var receivedCount = e.BytesTransferred;
                if (receivedCount == 0)
                {
                    //StatusTextBox.AppendText("Received 0 byte.\n");
                    //CommunicationStateChanging(ConnectionState.Retry);
                    return;
                }
                //parseMessage(e.Buffer, receivedCount);
                Array.Copy(e.Buffer, messageBytes, receivedCount);
                if (SocketClient is null)
                    return;

                if (!SocketClient.ReceiveAsync(e))
                    SocketReceiveEventCompleted(sender, e);
            }
            catch (Exception ex)
            {
                //StatusTextBox.AppendText($"Unexpected exception: {ex.Message}");
                //CommunicationStateChanging(ConnectionState.Retry);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            int port = 0;
            if (int.TryParse(PortTextBox.Text.Trim(), out port))
            {
                start("127.0.0.1",port);
                
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            SendDataMessage("shutdown");
        }
    }
}
