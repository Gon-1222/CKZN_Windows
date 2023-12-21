using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
namespace Chikuzenni
{
    public partial class Form1 : Form
    {
        int timeout=5;
        string rcvdata = "";
        //コンストラクタ
        public Form1()
        {
            this.FormClosing += Form1_FormClosing;
            //Form2を表示
            Form2 form2 = new Form2(this);
            form2.Show();
            string[] PortList = SerialPort.GetPortNames();
            form2.cmbPortName.Items.Clear();
            //! シリアルポート名をコンボボックスにセットする.
            foreach (string PortName in PortList)
            {
                form2.cmbPortName.Items.Add(PortName);
            }
            if (form2.cmbPortName.Items.Count > 0)
            {
                form2.cmbPortName.SelectedIndex = 0;
            }
            this.Visible = false;
            InitializeComponent();
            serialPort1.DataReceived += serialPort1_DataReceived;
        }

        //メインフォームが読み込まれたとき
        private void Form1_Load(object sender, EventArgs e)
        {
            this.NetworkInput.Enabled = false;
            this.UserInput.Enabled = false;
            this.PassWordInput.Enabled = false;
            this.DomainInput.Enabled = false;
            this.PortInput.Enabled = false;
            this.SettingButton.Enabled = false;
        }

        //メインフォームが閉じられたとき
        private void Form1_FormClosing(object sender,FormClosingEventArgs e) 
        {
            Application.Exit();
        }

        //初期動作
        public void PortButtton_Click(string portname) 
        {
            this.Show();
            /*---変数の宣言---*/
            string[] rcvDataWifi = new string[4];
            string[] rcvDataHttp = new string[3];

            
            //シリアル通信の開始
            serialOpen(portname);

            //現在の情報の取得
            //WiFi情報
            Request("req 1, 1;", rcvDataWifi);
            if (rcvDataWifi.Length < 4)
            {
                return;
            }
            if (rcvDataWifi[0] == "0")
            {
                return;
            }
            this.NetworkInput.Text = rcvDataWifi[1];   this.NetworkInput.Enabled = true;
            this.UserInput.Text = rcvDataWifi[2];         this.UserInput.Enabled = true;
            this.PassWordInput.Text = rcvDataWifi[3];  this.PassWordInput.Enabled = true;
            Info.Text = "WiFiの設定を読み込みました。";

            //プロキシ情報
            Request( "req 1, 2;",  rcvDataHttp);
            if (rcvDataHttp.Length < 3)
            {
                return;
            }
            if (rcvDataHttp[0] == "0")
            {
                return;
            }
            this.DomainInput.Text = rcvDataHttp[1];     this.DomainInput.Enabled = true;
            this.PortInput.Text = rcvDataHttp[2];          this.PortInput.Enabled = true;
            this.SettingButton.Enabled = true;
            Info.Text = "プロキシの設定を読み込みました。";
            //メインウィンドウを表示
            this.Show();
        }

        //設定ボタンが押されたとき
        private void SettingButton_Click(object sender, EventArgs e)
        {
            string[] rcvDataWifi = new string[4];
            string[] rcvDataHttp = new string[3];

            //WiFi情報を設定
            string sendData = "req 2,1";
            sendData += (this.NetworkInput.Text + ",");
            sendData += (this.UserInput.Text + ",");
            sendData += (this.PassWordInput.Text);
            sendData += ";";

            Request(sendData, rcvDataWifi);
            if (rcvDataWifi[0] == "0")
            {
                return;
            }
            this.NetworkInput.Text = rcvDataWifi[1];
            this.UserInput.Text = rcvDataWifi[2];
            this.PassWordInput.Text = rcvDataWifi[3];

            //プロキシ情報の設定
            sendData = "req 2,2,";
            sendData += (this.DomainInput.Text + ",");
            sendData += (this.PortInput.Text);
            sendData += ";";
            Request(sendData, rcvDataHttp);
            if (rcvDataHttp[0] == "0")
            {
                Info.Text = "エラー:" + "設定に失敗しました。";
                return;
            }
            this.DomainInput.Text = rcvDataHttp[1];
            this.PortInput.Text = rcvDataHttp[2];
            Info.Text = "情報:" + "設定を更新しました。";
        }

        //終了ボタンが押されたとき
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //シリアル通信の開始
        private int serialOpen(String portname)
        {
            serialPort1.PortName = portname;
            serialPort1.BaudRate = 9600;
            serialPort1.DataBits = 8;
            serialPort1.Parity = Parity.Odd;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Handshake = Handshake.XOnXOff;
            serialPort1.Encoding = Encoding.UTF8;
            serialPort1.ReadTimeout = 500;

            try
            {
                serialPort1.Open();
            }
            catch (Exception)
            {
                Info.Text = "情報: シリアルポートを開けませんでした。";
                return 1;
            }
            return 0;
        }

        //リクエスト
        private int Request(string data, string[] splitData)
        {
            //シリアル送受信（リトライ）
            int i;
            while(true){
                if (Stream(data))   break;
                for(i = 5; i > 0; i--) { 
                    Info.Text = "タイムアウトしました：" + i.ToString() + "秒後に再試行します。";
                    Thread.Sleep(100); 
                }
                
            }
            //受信データのパース
            string rcvdata = this.rcvdata.Substring(4,this.rcvdata.IndexOf(";")-4);
            this.rcvdata = "";
            rcvdata = Regex.Replace(rcvdata, @"\s", "");
             string[] buf = rcvdata.Split(',');
            for (i=0; i<buf.Length; i++)
            {
                splitData[i] = buf[i];
            }
            return 0;
        }

        //シリアル送受信
        private bool Stream(String data)
        {
            //送信
            try
            {
                serialPort1.Write(data);
            }
            catch (Exception ex)
            {
                Info.Text = "エラー: " + ex.Message;
                return false;
            }
            //受信待ち
            int i = 0;
            while (true)
            {
                Thread.Sleep(1000);
                i++;
                if (i == timeout)
                {
                    Info.Text = "エラー: データの受信に失敗しました。(タイムアウト)";
                    return false;
                }
                if (this.rcvdata != "")
                {
                    break;
                }
            }
            return true;
        }

        //受信ハンドラ
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //! シリアルポートをオープンしていない場合、処理を行わない.
            if (serialPort1.IsOpen == false)
            {
                return;
            }

            try
            {
                //! 受信データを読み込む.
                string data = serialPort1.ReadExisting();
                rcvdata = data;
                //! 受信したデータをテキストボックスに書き込む.
            }
            catch (Exception ex)
            {
                Info.Text = "エラー: "+ex.Message;
            }
        }
    }
}
