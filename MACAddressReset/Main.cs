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
            this.cbo_Network.ValueMember = "Description";
            this.cbo_Network.DisplayMember = "Name";
            this.cbo_Network.DataSource = nics.Where(s=>s.NetworkInterfaceType == NetworkInterfaceType.Ethernet || s.NetworkInterfaceType == NetworkInterfaceType.Wireless80211).ToArray();
            this.txt_MacAddress.Text = macHelper.CreateNewMacAddress();
            string path = System.Windows.Forms.Application.StartupPath+"\\" + "lantern.exe";
            this.txt_AppPath.Text = System.IO.File.Exists(path) ? path : String.Empty;
            if (string.IsNullOrEmpty(this.txt_AppPath.Text))
            {
                ShowInfo(txt_log,$"请将本工具放置在【lantern.exe】应用程序所在目录,或着选择应用路径、如果你不需要修改后自动打开应用程序请留空！");
            }
            
            /*加载默认公告*/this.lab_MSG.Text = "SSR翻墙推荐超值！！";this.lab_MSG.Links[0].LinkData = "http://ssr.cichui.tk";
            /*加载默认开源地址*/this.linkLabel1.Links[0].LinkData = this.linkLabel1.Text;
            /*加载默认博客地址*/this.linkLabel2.Links[0].LinkData = this.linkLabel2.Text;
            //ShowInfo(txt_log, $"本来做这个破玩意是为了破解Lantern限流的，结果正逢十九大 刚做好第二天Lantern就不能用了，Be了个Go的！开源出来给你们当玩具吧。。。");
            WebClient webWlient = new WebClient();
            webWlient.Encoding = Encoding.UTF8;
            webWlient.DownloadStringAsync(new Uri("http://dwz.cn/77c5cu"));
            webWlient.DownloadStringCompleted += WebWlient_DownloadStringCompleted;//异步下载完成回掉
            webWlient.Dispose();//释放资源
        }
        //异步回掉
        private void WebWlient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error==null)
            {
                /*字符串分割（公告文字|公告跳转的链接|开源地址|博客地址|重要提示|重要提示后跳转）*/
                string[] publicMsg = e.Result?.Split('|');
                if (publicMsg?.Length >= 2)
                {
                    /*远程加载公告内容*/this.lab_MSG.Text = publicMsg?[0];
                    /*设置公告跳转地址*/this.lab_MSG.Links[0].LinkData = publicMsg?[1];
                }
                if (publicMsg?.Length >= 3)
                {
                    /*远程加载开源地址*/
                    this.linkLabel1.Text = publicMsg?[2];
                    this.linkLabel1.Links[0].LinkData = publicMsg?[2];
                }
                if (publicMsg?.Length >= 4)
                {
                    /*远程加载博客地址*/
                    this.linkLabel2.Text = publicMsg?[3];
                    this.linkLabel2.Links[0].LinkData = publicMsg?[3];
                }
                if (publicMsg?.Length >= 5 && !string.IsNullOrEmpty(publicMsg[4]) && MessageBox.Show(publicMsg[4], "重要提示", MessageBoxButtons.YesNo,MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    if (publicMsg.Length >= 6) System.Diagnostics.Process.Start(publicMsg[5]);
                }
            }
        }

        //网卡选择
        private void cbo_Network_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowInfo(txt_log, $"当前所选网卡的MAC地址：【{macHelper.GetMACAddress(cbo_Network.SelectedValue.ToString(), out string index)}】");

        }

        // 随机MAC地址
        private void btn_random_Click(object sender, EventArgs e)
        {
            this.txt_MacAddress.Text = macHelper.CreateNewMacAddress();
        }
        //修改程序路径
        private void btn_up_Click(object sender, EventArgs e)
        {
            string path = "";
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                path = openFile.FileName;
                this.txt_AppPath.Text = path;
                ShowInfo(this.txt_AppPath, $"已选定程序路径为【{path}】");
            }
        }
        
        // 更新MAC地址
        private void btn_start_Click(object sender, EventArgs e)
        {
            macHelper.SetMACAddress(cbo_Network.SelectedValue.ToString(), this.txt_MacAddress.Text);
            ResetConn();
            ShowInfo(this.txt_log, $"{DateTime.Now}：MAC地址已更新为【{macHelper.GetMACAddress(cbo_Network.SelectedValue.ToString(), out string index)}】！");
            if (!string.IsNullOrEmpty(txt_AppPath.Text))
            {
                Process[] myproc = Process.GetProcesses();
                foreach (Process item in myproc)
                {
                    if (item.ProcessName == txt_AppPath.Text.Substring(txt_AppPath.Text.LastIndexOf("\\") + 1, (txt_AppPath.Text.LastIndexOf(".")) - txt_AppPath.Text.LastIndexOf("\\") - 1))
                    {
                        item.Kill();
                    }
                }

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = txt_AppPath.Text;
                //info.Arguments = "";
                //info.WindowStyle = ProcessWindowStyle.Minimized;
                Process pro = Process.Start(info);
                pro.Close();
                //pro.WaitForExit();
                ShowInfo(this.txt_log, $"应用程序已启动，如无需要可关闭当前界面！");
            }
        }
        
        // 恢复网卡默认
        private void btn_reset_Click(object sender, EventArgs e)
        {
            macHelper.ResetMACAddress(cbo_Network.SelectedValue.ToString());
            ResetConn();
            ShowInfo(this.txt_log,$"MAC地址已重置为【{macHelper.GetMACAddress(cbo_Network.SelectedValue.ToString(),out string index)}】!");
        }
        
        // 重启网卡
        private void btn_restart_Click(object sender, EventArgs e)
        {
            ResetConn();
        }
        
        // 重启网卡
        private void ResetConn()
        {
            if (!macHelper.DisableNetWork(macHelper.NetWork(this.cbo_Network.SelectedValue.ToString())))
            {
                ShowInfo(this.txt_log,$"禁用网卡失败!");
            }
            else
            {
                ShowInfo(this.txt_log,$"禁用网卡成功!【请等待。。。】");
            }
            if (!macHelper.EnableNetWork(macHelper.NetWork(this.cbo_Network.SelectedValue.ToString())))
            {
                ShowInfo(this.txt_log,$"开启网卡失败!");
            }
            else
            {
                ShowInfo(this.txt_log, $"开启网卡成功!【已完成】");
            }
        }

        /// <summary>
        /// 显示日志信息
        /// </summary>
        /// <param name="txtInfo"></param>
        /// <param name="Info"></param>
        public static void ShowInfo(System.Windows.Forms.TextBox txtInfo, string Info)
        {
            txtInfo.AppendText($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}：\r\n{Info}\r\n");
            txtInfo.AppendText(Environment.NewLine);
            txtInfo.ScrollToCaret();
        }
        
        // 显示隐藏公告
        private void label6_Click(object sender, EventArgs e)
        {
            lab_MSG.Visible = !lab_MSG.Visible;
            this.label6.Text = lab_MSG.Visible ? "隐藏公告：" : "显示公告：";
        }

        // 点击打开公告地址
        private void lab_MSG_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
        
        //打开Git地址
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
        
        //打开我的博客
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}
