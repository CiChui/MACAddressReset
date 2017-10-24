using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MACAddressReset
{
    public partial class Main : Form
    {
            MACHelper macHelper = new MACHelper();
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (!macHelper.IsConnectedToInternet())
            {
                MessageBox.Show("未连接到网络！");
                Application.Exit();
                return;
            }
            //获取所有网卡信息
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            this.comboBox1.ValueMember = "Description";
            this.comboBox1.DisplayMember = "Name";
            this.comboBox1.DataSource = nics.Where(s=>s.NetworkInterfaceType == NetworkInterfaceType.Ethernet || s.NetworkInterfaceType == NetworkInterfaceType.Wireless80211).ToArray();
            this.textBox1.Text = macHelper.CreateNewMacAddress();
            //this.textBox1.ReadOnly = true;
            this.textBox2.Text = $"{DateTime.Now}：当前MAC地址【{macHelper.GetMACAddress(comboBox1.SelectedValue.ToString(),out string index)}】\r\n";
            string path = System.Windows.Forms.Application.StartupPath+"\\" + "lantern.exe";
            this.textBox3.Text = System.IO.File.Exists(path) ? path : String.Empty;
            if (string.IsNullOrEmpty(this.textBox3.Text))
            {
                ShowInfo(textBox2,$"请将本工具放置在【lantern.exe】应用程序所在目录,或着选择应用路径、如果你不需要修改后自动打开应用程序请留空！");
            }
            ShowInfo(textBox2, $"本来做这个破玩意是为了破解Lantern限流的，结果刚做好第二天Lantern就不能用了，Be了个Go的！开源出来给你们当玩具吧。。。");
        }
        /// <summary>
        /// 更新MAC地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            macHelper.SetMACAddress(comboBox1.SelectedValue.ToString(),this.textBox1.Text);
            resetConn();
            ShowInfo(this.textBox2,$"{DateTime.Now}：MAC地址已更新为【{macHelper.GetMACAddress(comboBox1.SelectedValue.ToString(),out string index)}】！");
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                Process[] myproc = Process.GetProcesses();
                foreach (Process item in myproc)
                {
                    if (item.ProcessName == textBox3.Text.Substring(textBox3.Text.LastIndexOf("\\")+1,(textBox3.Text.LastIndexOf("."))- textBox3.Text.LastIndexOf("\\") - 1))
                    {
                        item.Kill();
                    }
                }
                
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = textBox3.Text;
                //info.Arguments = "";
                //info.WindowStyle = ProcessWindowStyle.Minimized;
                Process pro = Process.Start(info);
                pro.Close();
                //pro.WaitForExit();
                ShowInfo(this.textBox2,$"应用程序已启动，如无需要可关闭当前界面！");
            }
        }
        /// <summary>
        /// 重启网卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            macHelper.ResetMACAddress(comboBox1.SelectedValue.ToString());
            resetConn();
            ShowInfo(this.textBox2,$"MAC地址已重置为【{macHelper.GetMACAddress(comboBox1.SelectedValue.ToString(),out string index)}】!");
        }
        /// <summary>
        /// 随机MAC地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = macHelper.CreateNewMacAddress();
        }
        /// <summary>
        /// 重新连接网络
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //Thread oThread = new Thread(new ThreadStart(macHelper.ReConnect));//new Thread to ReConnect
            //oThread.Start();
            resetConn();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string path = "";
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                path = openFile.FileName;
                this.textBox3.Text = path;
                ShowInfo(this.textBox2,$"已选定程序路径为【{path}】");
            }
        }

        private void resetConn()
        {
            if (!macHelper.DisableNetWork(macHelper.NetWork(this.comboBox1.SelectedValue.ToString())))
            {
                ShowInfo(this.textBox2,$"禁用网卡失败!");
            }
            else
            {
                ShowInfo(this.textBox2,$"禁用网卡成功!【请等待。。。】");
            }
            if (!macHelper.EnableNetWork(macHelper.NetWork(this.comboBox1.SelectedValue.ToString())))
            {
                ShowInfo(this.textBox2,$"开启网卡失败!");
            }
            else
            {
                ShowInfo(this.textBox2, $"开启网卡成功!【已完成】");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.Links[0].LinkData = "https://gitee.com/fawei/";
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel2.Links[0].LinkData = "http://cichui.top/";
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="txtInfo"></param>
        /// <param name="Info"></param>
        public static void ShowInfo(System.Windows.Forms.TextBox txtInfo, string Info)
        {
            txtInfo.AppendText($"\r\n{DateTime.Now}：{Info}");
            txtInfo.AppendText(Environment.NewLine);
            txtInfo.ScrollToCaret();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowInfo(textBox2,$"当前MAC地址【{macHelper.GetMACAddress(comboBox1.SelectedValue.ToString(), out string index)}】\r\n");
        }
    }
}
