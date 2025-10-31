using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TCPIPServerTest
{
    public partial class Form1 : Form
    {
        private TcpListener server = null;
        private Thread serverThread;
        private bool isRunning = false;

        private List<TcpClient> clients = new List<TcpClient>();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                // txtIp와 txtPort에서 입력한 IP주소와 포트번호로 서버를 연다
                IPAddress ip = IPAddress.Parse(txtIp.Text);
                int port = int.Parse(txtPort.Text);
                server = new TcpListener(ip, port); // 지정된 IP 주소(ip)와 포트 번호(port)로 수신 대기하는 TCP 서버를 생성
                server.Start();

                // 서버를 백그라운드 스레드를 실행해서, 클라이언트 연결 요청을 기다림.
                // AcceptClients() 함수가 별도의 스레드에서 돌아감.
                isRunning = true;
                serverThread = new Thread(AcceptClients);  // 클라이언트 연결을 수락하는 서버 스레드를 생성하는 코드 
                serverThread.IsBackground = true;
                serverThread.Start();

                // listbox에 "서버 시작됨" 표시
                AddMsg($"서버 시작됨 ({ip}:{port})");
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버 시작 오류: " + ex.Message);
            }
        }

        private void AcceptClients()
        {
            try
            {
                // AcceptTcpClient()는 클라이언트가 접속할 때까지 기다리는 블로킹 함수
                // 클라이언트가 연결되면 TcpClient 객체가 만들어지고 clients 리스트에 추가됨
                while (isRunning)
                {
                    TcpClient client = server.AcceptTcpClient();
                    clients.Add(client);
                    AddMsg($"클라이언트 접속: {client.Client.RemoteEndPoint}");

                    // 새 클라이언트마다 ReceiveData() 스레드를 따로 만들어서 통신을 처리함
                    // (즉, 여러 클라이언트가 동시에 메시지를 주고받을 수 있음)
                    Thread t = new Thread(ReceiveData);
                    t.IsBackground = true;
                    t.Start(client);
                }
            }

            catch { }
        }

        private void ReceiveData(object obj)
        {
            TcpClient client = (TcpClient)obj;
            
            // NetworkStream은 TCP 연결의 실제 데이터 송수신을 담당하는 스트림.
            // 데이터를 담을 1KB짜리 버퍼를 준비.
            NetworkStream ns = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (isRunning)
                {
                    int bytes = ns.Read(buffer, 0, buffer.Length); // NetworkStream에서 데이터를 읽어 buffer라는 바이트 배열에 저장하고, 
                    if (bytes == 0) break;
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    AddMsg($"[클라이언트] {msg}");

                    BroadcastMessage(msg, client);
                }
            }
            catch
            {
                AddMsg($"클라이언트 연결 종료: {client.Client.RemoteEndPoint}");
            }
            finally
            {
                clients.Remove(client);
                client.Close();
            }
        }

        private void BroadcastMessage(string msg, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            foreach (var c in clients)
            {
                try
                {
                    if (c != sender)
                    {
                        c.GetStream().Write(data, 0, data.Length);
                    }
                }
                catch { }
            }
        }

        private void AddMsg(string msg)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AddMsg), msg);
                return;
            }
            listBox1.Items.Add(msg);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtSend.Text.Trim();
            if (msg.Length == 0) return;

            AddMsg($"[서버] {msg}");
            BroadcastMessage("[서버] " + msg, null);
            txtSend.Clear();
        }
    }
}
