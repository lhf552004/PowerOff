using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Console;
namespace Shutdown
{
    public partial class PowerOff : Form
    {
        public PowerOff()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        private Socket _socket;

        //private void startListen(int port)
        //{
        //    var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    server.Bind(new IPEndPoint(IPAddress.Any, port));
        //    server.Listen(0);
        //    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    _socket.Accept();
        //}

        public void Listener(int port)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.ReceiveTimeout = 5000; // receive timout 5 seconds
            listener.SendTimeout = 5000; // send timeout 5 seconds 

            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(backlog: 15);

            textBox1.AppendText($"listener started on port {port}\n");

            var cts = new CancellationTokenSource();


            var tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
            tf.StartNew(() =>  // listener task
            {
                textBox1.Text = ("listener task started\n");
                while (true)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        break;
                    }
                    textBox1.AppendText("waiting for accept\n");
                    Socket client = listener.Accept();
                    if (!client.Connected)
                    {
                        textBox1.AppendText("not connected\n");
                        continue;
                    }
                    textBox1.AppendText($"client connected local address {((IPEndPoint)client.LocalEndPoint).Address} and port {((IPEndPoint)client.LocalEndPoint).Port}, remote address {((IPEndPoint)client.RemoteEndPoint).Address} and port {((IPEndPoint)client.RemoteEndPoint).Port}\n");

                    Task t = CommunicateWithClientUsingSocketAsync(client);

                }
                listener.Dispose();
                textBox1.AppendText("Listener task closing\n");

            }, cts.Token);

            textBox1.AppendText("Press return to exit\n");
            //ReadLine();
            //cts.Cancel();
        }

        private Task CommunicateWithClientUsingSocketAsync(Socket socket)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (socket)
                    {
                        bool completed = false;
                        do
                        {
                            byte[] readBuffer = new byte[1024];
                            int read = socket.Receive(readBuffer, 0, 1024, SocketFlags.None);
                            string fromClient = Encoding.UTF8.GetString(readBuffer, 0, read);
                            textBox1.AppendText($"read {read} bytes: {fromClient}\n");
                            if (string.Compare(fromClient, "shutdown", ignoreCase: true) == 0)
                            {
                                completed = true;
                                ExecuteShutDownOperation();
                                textBox1.AppendText("received message\n");
                            }

                            byte[] writeBuffer = Encoding.UTF8.GetBytes($"echo {fromClient}");

                            int send = socket.Send(writeBuffer);
                            textBox1.AppendText($"sent {send} bytes");

                        } while (!completed);
                    }
                    textBox1.AppendText("closed stream and client socket");
                }
                catch (SocketException ex)
                {
                    textBox1.AppendText(ex.Message);
                }
                catch (Exception ex)
                {
                    textBox1.AppendText(ex.Message);
                }
            });
        }

        private static async Task CommunicateWithClientUsingNetworkStreamAsync(Socket socket)
        {
            try
            {
                using (var stream = new NetworkStream(socket, ownsSocket: true))
                {

                    bool completed = false;
                    do
                    {
                        byte[] readBuffer = new byte[1024];
                        int read = await stream.ReadAsync(readBuffer, 0, 1024);
                        string fromClient = Encoding.UTF8.GetString(readBuffer, 0, read);
                        WriteLine($"read {read} bytes: {fromClient}");
                        if (string.Compare(fromClient, "shutdown", ignoreCase: true) == 0)
                        {
                            completed = true;
                        }

                        byte[] writeBuffer = Encoding.UTF8.GetBytes($"echo {fromClient}");

                        await stream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                    } while (!completed);
                }
                WriteLine("closed stream and client socket");
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
            }
        }

        private async Task CommunicateWithClientUsingReadersAndWritersAsync(Socket socket)
        {
            try
            {
                using (var stream = new NetworkStream(socket, ownsSocket: true))
                using (var reader = new StreamReader(stream, Encoding.UTF8, false, 8192, leaveOpen: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 8192, leaveOpen: true))
                {
                    writer.AutoFlush = true;

                    bool completed = false;
                    do
                    {
                        string fromClient = await reader.ReadLineAsync();
                        WriteLine($"read {fromClient}");
                        if (string.Compare(fromClient, "shutdown", ignoreCase: true) == 0)
                        {
                            completed = true;
                            textBox1.AppendText($"echo {fromClient}");
                            //ExecuteShutDownOperation();
                        }
                    } while (!completed);
                }
                WriteLine("closed stream and client socket");
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            int port = 0;
            if(int.TryParse(PortTextBox.Text.Trim(), out port))
            {
                Listener(port);
                textBox1.AppendText("server started");
            }
            
        }

        public static void ExecuteShutDownOperation()
        {
            try
            {
                Process p = new Process();//实例化一个独立进程
                p.StartInfo.FileName = "cmd.exe";//进程打开的文件为Cmd
                p.StartInfo.UseShellExecute = false;//是否启动系统外壳选否
                p.StartInfo.RedirectStandardInput = true;//这是是否从StandardInput输入
                p.StartInfo.CreateNoWindow = true;//这里是启动程序是否显示窗体
                p.Start();//启动
                p.StandardInput.WriteLine("shutdown -s -t 10");
                p.StandardInput.WriteLine("exit");
                //string cmd = @"shutdown";
                //string args = @"true";

                //var start = new ProcessStartInfo(cmd, args);
                //start.ErrorDialog = true;
                //Debug.WriteLine("cmd: " + (cmd ?? "null"));
                //Debug.WriteLine("args: " + (args ?? "null"));
                //start.UseShellExecute = false;
                //start.CreateNoWindow = true;
                //using (Process proc = Process.Start(start))
                //{
                //    proc.WaitForExit();
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
              
            }
        }
    }
}
