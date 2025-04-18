﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management; // 添加引用以使用WMI
using System.IO; // 添加文件操作引用
using System.Xml.Serialization; // 添加XML序列化引用
using SPClient.Models;


namespace SPClient
{

    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private bool isPortOpen = false;
        private bool isAsciiSend = false; // ASCII发送模式
        private bool isAsciiReceive = false; // ASCII接收模式
        private List<byte> receiveBuffer = new List<byte>(); // 接收缓冲区
        private System.Timers.Timer packetTimer; // 数据包超时定时器
        private const int PACKET_TIMEOUT = 50; // 包超时时间（毫秒）
        
        // 定时发送相关变量
        private System.Windows.Forms.Timer timerSend; // 定时发送定时器
        private bool isTimedSendEnabled = false; // 定时发送状态
        
        // 快捷指令列表
        private List<CommandShortcut> commandShortcuts = new List<CommandShortcut>();
        private string shortcutsFilePath = Path.Combine(Application.StartupPath, "commands.xml");

        public Form1()
        {
            InitializeComponent();
            InitializeSerialPorts();
            InitializeBaudRates();
            InitializeDataBits();
            InitializeParities();
            InitializeStopBits();

            // 默认为十六进制模式
            isAsciiSend = false;
            isAsciiReceive = false;

            // 初始化数据包超时定时器
            packetTimer = new System.Timers.Timer();
            packetTimer.Interval = PACKET_TIMEOUT;
            packetTimer.AutoReset = false;
            packetTimer.Elapsed += PacketTimer_Elapsed;
            
            // 初始化窗体布局
            this.Load += (s, e) => Form1_Resize(this, EventArgs.Empty);
            
            // 初始化定时发送功能
            InitializeTimedSendFeature();
            
            // 调整txtSend的位置，给按钮留出空间
            txtSend.Location = new Point(txtSend.Location.X, btnShortcuts.Bottom + 10);
            
            // 加载快捷指令
            LoadCommandShortcuts();
        }

        // 初始化定时发送功能
        private void InitializeTimedSendFeature()
        {
            // 初始化定时器
            timerSend = new System.Windows.Forms.Timer();
            timerSend.Tick += TimerSend_Tick;
        }
        
        // 定时发送按钮点击事件
        private void btnTimedSend_Click(object sender, EventArgs e)
        {
            if (!isTimedSendEnabled)
            {
                if (!isPortOpen)
                {
                    MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                if (string.IsNullOrEmpty(txtSend.Text.Trim()))
                {
                    MessageBox.Show("请输入要发送的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // 开始定时发送
                timerSend.Interval = (int)numTimerInterval.Value;
                timerSend.Start();
                isTimedSendEnabled = true;
                btnTimedSend.Text = "停止发送";
                numTimerInterval.Enabled = false;
            }
            else
            {
                // 停止定时发送
                timerSend.Stop();
                isTimedSendEnabled = false;
                btnTimedSend.Text = "定时发送";
                numTimerInterval.Enabled = true;
            }
        }
        
        // 定时器触发事件
        private void TimerSend_Tick(object sender, EventArgs e)
        {
            // 检查串口是否仍然打开
            if (!isPortOpen || serialPort == null)
            {
                timerSend.Stop();
                isTimedSendEnabled = false;
                btnTimedSend.Text = "定时发送";
                numTimerInterval.Enabled = true;
                MessageBox.Show("串口已关闭，定时发送已停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // 调用发送方法
            btnSend_Click(sender, e);
        }

        // 快捷指令按钮点击事件
        private void btnShortcuts_Click(object sender, EventArgs e)
        {
            using (ShortcutForm shortcutForm = new ShortcutForm(commandShortcuts))
            {
                if (shortcutForm.ShowDialog() == DialogResult.OK)
                {
                    // 更新快捷指令列表
                    commandShortcuts = shortcutForm.CommandShortcuts;
                    SaveCommandShortcuts();
                    
                    // 如果有选中的快捷指令，则填入发送框
                    if (shortcutForm.SelectedShortcut != null)
                    {
                        CommandShortcut selectedShortcut = shortcutForm.SelectedShortcut;
                        
                        // 应用快捷指令设置
                        txtSend.Text = selectedShortcut.Command;
                        
                        // 设置发送模式和选项
                        isAsciiSend = selectedShortcut.IsAsciiMode;
                        chkAsciiSend.Checked = selectedShortcut.IsAsciiMode;
                        chkAddCRC.Checked = selectedShortcut.AddCRC;
                        chkAddCR.Checked = selectedShortcut.AddCR;
                        chkAddLF.Checked = selectedShortcut.AddLF;
                    }
                }
            }
        }

        // 处理窗体大小变化的事件
        private void Form1_Resize(object sender, EventArgs e)
        {
            // 调整控件尺寸以适应窗体大小
            // 获取窗体的工作区域宽度
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;
            
            // 调整分组框的宽度
            groupBox1.Width = formWidth - 24; // 左右各留12像素边距
            groupBox2.Width = formWidth - 24;
            groupBox3.Width = formWidth - 24;
            
            // 调整txtSend输入框的宽度
            txtSend.Width = groupBox2.Width - 218; // 保留发送按钮、快捷指令按钮和左右边距
            
            // 调整txtReceived的宽度和高度
            txtReceived.Width = groupBox3.Width - 138; // 保留清除按钮和左右边距
            
            // 调整groupBox3(消息面板)的高度
            groupBox3.Height = formHeight - groupBox1.Height - groupBox2.Height - 48; // 减去其他控件高度和间距
            
            // 调整txtReceived的高度
            txtReceived.Height = groupBox3.Height - 41; // 减去groupBox标题和边距
            
            // 调整按钮位置
            btnOpenClose.Left = groupBox1.Width - btnOpenClose.Width - 6;
            btnSend.Left = groupBox2.Width - btnSend.Width - 6;
            btnClearReceived.Left = groupBox3.Width - btnClearReceived.Width - 6;
            
            // 调整定时发送和快捷指令按钮位置
            if (btnTimedSend != null && btnShortcuts != null)
            {
                btnTimedSend.Location = new Point(btnShortcuts.Right + 10, btnShortcuts.Top);
                if (lblInterval != null && numTimerInterval != null)
                {
                    lblInterval.Location = new Point(btnTimedSend.Right + 10, btnTimedSend.Top + 5);
                    numTimerInterval.Location = new Point(lblInterval.Right + 5, btnTimedSend.Top);
                }
            }
        }

        // 串口信息类
        public class COMPortInfo
        {
            public string Name { get; set; }        // COM端口名称
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

        // 获取串口信息列表
        private void InitializeSerialPorts()
        {
            cbPortName.Items.Clear();
            
            // 获取带有友好名称的串口列表
            List<COMPortInfo> comPorts = COMPortInfo.GetCOMPortsInfo();
            
            if (comPorts.Count > 0)
            {
                foreach (COMPortInfo port in comPorts)
                {
                    cbPortName.Items.Add(port);
                }
                cbPortName.SelectedIndex = 0;
            }
            else
            {
                // 如果WMI方法没有获取到串口，使用传统方法
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    cbPortName.Items.AddRange(ports);
                    cbPortName.SelectedIndex = 0;
                }
            }
        }

        private void InitializeBaudRates()
        {
            cbBaudRate.Items.Clear();
            int[] baudRates = new int[] { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
            foreach (int baudRate in baudRates)
            {
                cbBaudRate.Items.Add(baudRate.ToString());
            }
            cbBaudRate.SelectedIndex = 6; // 默认选择9600
        }

        private void InitializeDataBits()
        {
            cbDataBits.Items.Clear();
            int[] dataBits = new int[] { 5, 6, 7, 8 };
            foreach (int bit in dataBits)
            {
                cbDataBits.Items.Add(bit.ToString());
            }
            cbDataBits.SelectedIndex = 3; // 默认选择8位
        }

        private void InitializeParities()
        {
            cbParity.Items.Clear();
            cbParity.Items.Add("None");
            cbParity.Items.Add("Odd");
            cbParity.Items.Add("Even");
            cbParity.Items.Add("Mark");
            cbParity.Items.Add("Space");
            cbParity.SelectedIndex = 0; // 默认选择None
        }

        private void InitializeStopBits()
        {
            cbStopBits.Items.Clear();
            cbStopBits.Items.Add("1");
            cbStopBits.Items.Add("1.5");
            cbStopBits.Items.Add("2");
            cbStopBits.SelectedIndex = 0; // 默认选择1
        }

        private void btnOpenClose_Click(object sender, EventArgs e)
        {
            if (!isPortOpen)
            {
                try
                {
                    // 配置串口
                    serialPort = new SerialPort();
                    // 从选中项获取COM端口名称
                    if (cbPortName.SelectedItem is COMPortInfo)
                    {
                        serialPort.PortName = ((COMPortInfo)cbPortName.SelectedItem).Name;
                    }
                    else
                    {
                        serialPort.PortName = cbPortName.Text;
                    }
                    serialPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                    serialPort.DataBits = Convert.ToInt32(cbDataBits.Text);

                    // 配置校验位
                    switch (cbParity.SelectedIndex)
                    {
                        case 0: serialPort.Parity = Parity.None; break;
                        case 1: serialPort.Parity = Parity.Odd; break;
                        case 2: serialPort.Parity = Parity.Even; break;
                        case 3: serialPort.Parity = Parity.Mark; break;
                        case 4: serialPort.Parity = Parity.Space; break;
                        default: serialPort.Parity = Parity.None; break;
                    }

                    // 配置停止位
                    switch (cbStopBits.SelectedIndex)
                    {
                        case 0: serialPort.StopBits = StopBits.One; break;
                        case 1: serialPort.StopBits = StopBits.OnePointFive; break;
                        case 2: serialPort.StopBits = StopBits.Two; break;
                        default: serialPort.StopBits = StopBits.One; break;
                    }

                    // 清空缓冲区
                    receiveBuffer.Clear();

                    // 注册数据接收事件
                    serialPort.DataReceived += SerialPort_DataReceived;

                    // 打开串口
                    serialPort.Open();
                    isPortOpen = true;
                    btnOpenClose.Text = "关闭串口";

                    // 禁用配置控件
                    cbPortName.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbParity.Enabled = false;
                    cbStopBits.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开串口失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    // 关闭串口
                    packetTimer.Stop();
                    serialPort.Close();
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Dispose();
                    isPortOpen = false;
                    btnOpenClose.Text = "打开串口";

                    // 启用配置控件
                    cbPortName.Enabled = true;
                    cbBaudRate.Enabled = true;
                    cbDataBits.Enabled = true;
                    cbParity.Enabled = true;
                    cbStopBits.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("关闭串口失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    // 重启定时器，等待可能的后续数据
                    packetTimer.Stop();

                    // 读取可用数据
                    int bytesToRead = serialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    serialPort.Read(buffer, 0, bytesToRead);

                    // 将读取的数据添加到接收缓冲区
                    lock (receiveBuffer)
                    {
                        receiveBuffer.AddRange(buffer);
                    }

                    // 启动定时器，如果在超时时间内没有新数据，则认为一个数据包接收完成
                    packetTimer.Start();
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show("接收数据错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            }
        }

        private void PacketTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 数据包接收完成，处理接收到的数据
            byte[] packetData;

            lock (receiveBuffer)
            {
                packetData = receiveBuffer.ToArray();
                receiveBuffer.Clear();
            }

            if (packetData.Length > 0)
            {
                string displayText;
                
                if (isAsciiReceive)
                {
                    // ASCII模式 - 转换为ASCII文本
                    displayText = Encoding.ASCII.GetString(packetData);
                }
                else
                {
                    // 十六进制模式 - 转换为十六进制字符串格式
                    displayText = BitConverter.ToString(packetData).Replace("-", " ");
                }

                // 使用Invoke在UI线程中更新控件
                this.Invoke(new Action(() =>
                {
                    txtReceived.AppendText($"{DateTime.Now:HH:mm:ss} 接收: " + Environment.NewLine + displayText + Environment.NewLine + Environment.NewLine);
                }));
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!isPortOpen || serialPort == null)
            {
                MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string inputText = txtSend.Text.Trim();
                if (string.IsNullOrEmpty(inputText))
                {
                    MessageBox.Show("请输入要发送的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                byte[] data;
                
                if (isAsciiSend)
                {
                    // ASCII模式 - 直接将输入文本转换为ASCII字节
                    data = Encoding.ASCII.GetBytes(inputText);
                }
                else
                {
                    // 十六进制模式 - 解析十六进制字符串
                    string[] hexValues = inputText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    data = new byte[hexValues.Length];

                    for (int i = 0; i < hexValues.Length; i++)
                    {
                        if(hexValues[i].Length>1){
                            //清理空字符
                            hexValues[i] = hexValues[i].Trim();
                            //去掉换行符
                            hexValues[i] = hexValues[i].Replace("\r", "");
                            hexValues[i] = hexValues[i].Replace("\n", "");

                        }
                        data[i] = Convert.ToByte(hexValues[i], 16);
                    }
                }

                // 准备要发送的数据
                byte[] dataToSend;
                byte[] endBytes = GetEndBytes();

                // 检查是否需要添加CRC校验
                if (chkAddCRC.Checked)
                {
                    byte[] dataWithCRC = AddModbusCRC(data);

                    // 添加终止符（如果需要）
                    if (endBytes != null && endBytes.Length > 0)
                    {
                        dataToSend = new byte[dataWithCRC.Length + endBytes.Length];
                        Array.Copy(dataWithCRC, 0, dataToSend, 0, dataWithCRC.Length);
                        Array.Copy(endBytes, 0, dataToSend, dataWithCRC.Length, endBytes.Length);
                    }
                    else
                    {
                        dataToSend = dataWithCRC;
                    }

                    serialPort.Write(dataToSend, 0, dataToSend.Length);

                    // 显示发送的数据
                    string sentData;
                    if (isAsciiReceive) // 使用接收模式的显示格式来显示发送数据
                    {
                        sentData = Encoding.ASCII.GetString(dataToSend);
                    }
                    else
                    {
                        sentData = BitConverter.ToString(dataToSend).Replace("-", " ");
                    }
                    txtReceived.AppendText($"{DateTime.Now:HH:mm:ss} 发送: " + Environment.NewLine + sentData + Environment.NewLine + Environment.NewLine);
                }
                else
                {
                    // 添加终止符（如果需要）
                    if (endBytes != null && endBytes.Length > 0)
                    {
                        dataToSend = new byte[data.Length + endBytes.Length];
                        Array.Copy(data, 0, dataToSend, 0, data.Length);
                        Array.Copy(endBytes, 0, dataToSend, data.Length, endBytes.Length);
                    }
                    else
                    {
                        dataToSend = data;
                    }

                    serialPort.Write(dataToSend, 0, dataToSend.Length);

                    // 显示发送的数据
                    string sentData;
                    if (isAsciiReceive) // 使用接收模式的显示格式来显示发送数据
                    {
                        sentData = Encoding.ASCII.GetString(dataToSend);
                    }
                    else
                    {
                        sentData = BitConverter.ToString(dataToSend).Replace("-", " ");
                    }
                    txtReceived.AppendText($"{DateTime.Now:HH:mm:ss} 发送: " + Environment.NewLine + sentData + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送数据错误: 请检查十六进制数据是否正确，是否存在连续空格" , "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] GetEndBytes()
        {
            if (chkAddLF.Checked && chkAddCR.Checked)
            {
                return new byte[] { 0x0D, 0x0A }; // \r\n
            }
            else if (chkAddLF.Checked)
            {
                return new byte[] { 0x0A }; // \n
            }
            else if (chkAddCR.Checked)
            {
                return new byte[] { 0x0D }; // \r
            }

            return null;
        }

        private byte[] AddModbusCRC(byte[] data)
        {
            // 计算Modbus CRC-16
            ushort crc = 0xFFFF;

            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            // 创建新数组，包含原始数据和CRC
            byte[] result = new byte[data.Length + 2];
            Array.Copy(data, result, data.Length);

            // 添加CRC (低字节在前，高字节在后)
            result[data.Length] = (byte)(crc & 0xFF);
            result[data.Length + 1] = (byte)(crc >> 8);

            return result;
        }

        private void btnClearReceived_Click(object sender, EventArgs e)
        {
            txtReceived.Clear();
        }

        private void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            InitializeSerialPorts();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                packetTimer.Stop();
                
                // 停止定时发送
                if (timerSend != null && timerSend.Enabled)
                {
                    timerSend.Stop();
                }
                
                serialPort.Close();
                serialPort.Dispose();
            }
        }

        // 切换ASCII发送模式
        private void chkAsciiSend_CheckedChanged(object sender, EventArgs e)
        {
            isAsciiSend = chkAsciiSend.Checked;
            
            if (isAsciiSend)
            {
                // ASCII模式提示
                MessageBox.Show("已切换到ASCII发送模式，请直接输入ASCII文本", "模式切换", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // 十六进制模式提示
                MessageBox.Show("已切换到十六进制发送模式，请输入十六进制数据，以空格分隔 (例如: 01 02 03)", "模式切换", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        // 切换ASCII接收模式
        private void chkAsciiReceive_CheckedChanged(object sender, EventArgs e)
        {
            isAsciiReceive = chkAsciiReceive.Checked;
        }

        // 加载快捷指令
        private void LoadCommandShortcuts()
        {
            try
            {
                if (File.Exists(shortcutsFilePath))
                {
                    using (FileStream fs = new FileStream(shortcutsFilePath, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<CommandShortcut>));
                        commandShortcuts = (List<CommandShortcut>)serializer.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载快捷指令失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 保存快捷指令
        private void SaveCommandShortcuts()
        {
            try
            {
                using (FileStream fs = new FileStream(shortcutsFilePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<CommandShortcut>));
                    serializer.Serialize(fs, commandShortcuts);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存快捷指令失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
