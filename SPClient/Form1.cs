using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPClient
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private bool isPortOpen = false;

        public Form1()
        {
            InitializeComponent();
            InitializeSerialPorts();
            InitializeBaudRates();
            InitializeDataBits();
            InitializeParities();
            InitializeStopBits();
        }

        private void InitializeSerialPorts()
        {
            cbPortName.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                cbPortName.Items.AddRange(ports);
                cbPortName.SelectedIndex = 0;
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
                    serialPort.PortName = cbPortName.Text;
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
                    int dataLength = serialPort.BytesToRead;
                    byte[] data = new byte[dataLength];
                    serialPort.Read(data, 0, dataLength);

                    // 转换为十六进制字符串格式
                    string hexString = BitConverter.ToString(data).Replace("-", " ");
                    
                    // 使用Invoke在UI线程中更新控件
                    this.Invoke(new Action(() =>
                    {
                        txtReceived.AppendText(hexString + Environment.NewLine);
                    }));
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

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!isPortOpen || serialPort == null)
            {
                MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string hexText = txtSend.Text.Trim();
                if (string.IsNullOrEmpty(hexText))
                {
                    MessageBox.Show("请输入要发送的数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 解析十六进制字符串
                string[] hexValues = hexText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] data = new byte[hexValues.Length];

                for (int i = 0; i < hexValues.Length; i++)
                {
                    data[i] = Convert.ToByte(hexValues[i], 16);
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
                    
                    // 显示带CRC的完整数据
                    string sentData = BitConverter.ToString(dataToSend).Replace("-", " ");
                    txtReceived.AppendText("发送: " + sentData + Environment.NewLine);
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
                    string sentData = BitConverter.ToString(dataToSend).Replace("-", " ");
                    txtReceived.AppendText("发送: " + sentData + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送数据错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                serialPort.Close();
                serialPort.Dispose();
            }
        }
    }
}
