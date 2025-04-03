using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;

namespace SPClient
{
    public class COMPortInfo
    {
        public string Name { get; set; } // COM端口名称
        public string Description { get; set; } // 友好名称

        public override string ToString()
        {
            return $"{Name} - {Description}";
        }

        // 获取串口信息列表
        public static List<COMPortInfo> GetCOMPortsInfo()
        {
            List<COMPortInfo> comPortInfoList = new List<COMPortInfo>();

            try
            {
                // 使用WMI获取串口信息
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%COM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString() ?? string.Empty;
                        if (caption.Contains("(COM"))
                        {
                            COMPortInfo comPortInfo = new COMPortInfo();

                            // 从描述中提取COM端口名称，如"COM1"
                            int startIndex = caption.IndexOf("(COM") + 1;
                            int endIndex = caption.IndexOf(")", startIndex);
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                comPortInfo.Name = caption.Substring(startIndex, endIndex - startIndex);
                                comPortInfo.Description = caption.Replace("(" + comPortInfo.Name + ")", "").Trim();
                                comPortInfoList.Add(comPortInfo);
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果WMI失败，返回空列表
            }

            return comPortInfoList;
        }
    }
}
