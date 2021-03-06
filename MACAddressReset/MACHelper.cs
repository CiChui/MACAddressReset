﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Management;
using System.Threading;
using System.Runtime.InteropServices;
namespace MACAddressReset
{
    public class MACHelper
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);
        /// <summary>
        /// 是否能连接上Internet
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedToInternet()
        {
            int Desc = 0;
            return InternetGetConnectedState(Desc, 0);
        }
        /// <summary>
        /// 获取MAC地址及输出注册表对应的编号
        /// </summary>
        public string GetMACAddress(string name,out string index)
        {
            index = null;
            string MACStr = string.Empty;
            //得到 MAC的注册表键
            RegistryKey macRegistry = Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Control")
            .OpenSubKey("Class").OpenSubKey("{4D36E972-E325-11CE-BFC1-08002bE10318}");
            IList<string> list = macRegistry.GetSubKeyNames().ToList();
            foreach (string str in list)
            {
                RegistryKey tempMacRegistry = macRegistry.OpenSubKey(str, true);
                if (tempMacRegistry.GetValue("DriverDesc").ToString() == name)
                {
                    try
                    {
                        index = str;
                        MACStr = tempMacRegistry.GetValue("NetworkAddress").ToString();
                    }
                    catch
                    {
                        MACStr = string.Empty;
                    }
                    break;
                }
            }
            if (string.IsNullOrEmpty(MACStr))
            {
                IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                var adapter = nics.First(o => o.Description == name);
                if (adapter != null)
                MACStr = adapter.GetPhysicalAddress().ToString();
            }
            return MACStr;
        }
        /// <summary>
        /// 设置指定网卡的MAC地址
        /// </summary>
        /// <param name="newMac"></param>
        public void SetMACAddress(string DriverDesc,string newMac)
        {
            //string index = GetAdapterIndex(out string macAddress);
            GetMACAddress(DriverDesc, out string index);
            if (string.IsNullOrEmpty(index)) return;
            //得到 MAC的注册表键
            RegistryKey macRegistry = Registry.LocalMachine.OpenSubKey("SYSTEM").OpenSubKey("CurrentControlSet").OpenSubKey("Control")
            .OpenSubKey("Class").OpenSubKey("{4D36E972-E325-11CE-BFC1-08002bE10318}").OpenSubKey(index, true);
            if (string.IsNullOrEmpty(newMac))
            {
                if (macRegistry.GetValue("NetworkAddress") != null)
                {
                    macRegistry.DeleteValue("NetworkAddress");
                }
            }
            else
            {
                var temp = macRegistry.GetValue("NetworkAddress");
                macRegistry.SetValue("NetworkAddress", newMac);
            }
        }
        /// <summary>
        /// 重设MAC地址
        /// </summary>
        public void ResetMACAddress(string DriverDesc)
        {
            SetMACAddress(DriverDesc,string.Empty);
        }
        /// <summary>
        /// 生成随机MAC地址
        /// </summary>
        /// <returns></returns>
        public string CreateNewMacAddress()
        {
            //return "0016D3B5C493";
            int min = 0;
            int max = 16;
            Random ro = new Random();
            var sn = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
            ro.Next(min, max).ToString("x"),//0
            ro.Next(min, max).ToString("x"),//
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),//5
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),
            ro.Next(min, max).ToString("x"),//10
            ro.Next(min, max).ToString("x")
            ).ToUpper();
            return sn;
        }
        /// <summary>  
        /// 禁用网卡  
        /// </summary>5  
        /// <param name="netWorkName">网卡名</param>  
        /// <returns></returns>  
        public bool DisableNetWork(ManagementObject network)
        {
            try
            {
                network.InvokeMethod("Disable", null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>  
        /// 得到指定网卡  
        /// </summary>  
        /// <param name="networkname">网卡名字</param>  
        /// <returns></returns>  
        public ManagementObject NetWork(string networkname)
        {
            string netState = "SELECT * From Win32_NetworkAdapter";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(netState);
            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject manage in collection)
            {
                if (manage["Name"].ToString() == networkname)
                {
                    return manage;
                }
            }


            return null;
        }
        /// <summary>  
        /// 启用网卡  
        /// </summary>  
        /// <param name="netWorkName">网卡名</param>  
        /// <returns></returns>  
        public bool EnableNetWork(ManagementObject network)
        {
            try
            {
                network.InvokeMethod("Enable", null);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}