using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace SPClient
{
    public class COMPortInfo
    {
        public string Name { get; set; } // 串口名称，如COM1
        public string Description { get; set; } // 串口描述，如"USB Serial Port (COM1)"

        public override string ToString()
        {
            return Description;
        }
        
        public static List<COMPortInfo> GetCOMPortsInfo()
        {
            List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();

            try
            {
                // 使用WMI查询设备信息
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
                
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj != null)
                    {
                        object captionObj = obj["Caption"];
                        if (captionObj != null)
                        {
                            string caption = captionObj.ToString();
                            // 查找包含COM的设备
                            if (caption.Contains("(COM"))
                            {
                                COMPortInfo comPortInfo = new COMPortInfo();
                                // 提取COM端口名称
                                int startIndex = caption.LastIndexOf("(COM") + 1;
                                int endIndex = caption.LastIndexOf(")");
                                comPortInfo.Name = caption.Substring(startIndex, endIndex - startIndex);
                                comPortInfo.Description = caption;
                                comPortInfoList.Add(comPortInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取串口信息失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return comPortInfoList;
        }

    }
}
