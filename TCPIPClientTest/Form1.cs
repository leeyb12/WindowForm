using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TCPIPClientTest
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool connected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                MessageBox.Show("이미 서버에 연결되어 있습니다.");
                return;
            }

            try
            {
                // 서버 IP와 포트 (서버와 동일해야 함)
                string ip = "192.168.0.135";
                int port = 9000;

                client = new TcpClient();
                client.Connect(ip, port);

                stream = client.GetStream();
                connected = true;

                // 서버 메시지 수신 스레드 실행
                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                AddMsg($"서버 연결됨 ({ip}:{port})");
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버 연결 실패: " + ex.Message);
            }
        }

        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (connected)
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes == 0) break; // 연결 끊김
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                    AddMsg(msg);
                }
            }
            catch
            {
                AddMsg("서버 연결이 끊어졌습니다.");
            }
            finally
            {
                Disconnect();
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                MessageBox.Show("서버에 연결되지 않았습니다.");
                return;
            }

            string msg = txtSend.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                AddMsg("[나] " + msg);
                txtSend.Clear();
            }
            catch (Exception ex)
            {
                AddMsg("전송 오류: " + ex.Message);
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
            listBox1.TopIndex = listBox1.Items.Count - 1; // 스크롤 자동 이동
        }

        private void Disconnect()
        {
            connected = false;
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            connected = false;
            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }
            base.OnFormClosing(e);
        }
    }
}
