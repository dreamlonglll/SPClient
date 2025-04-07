using System;

namespace SPClient.Models
{

    // 快捷指令类
    [Serializable]
    public class CommandShortcut
    {
        public string Name { get; set; }       // 命令别名
        public string Command { get; set; }    // 命令内容
        public bool IsAsciiMode { get; set; }  // 是否为ASCII模式
        public bool AddCRC { get; set; }       // 是否添加CRC校验
        public bool AddCR { get; set; }        // 是否添加回车符
        public bool AddLF { get; set; }        // 是否添加换行符

        public CommandShortcut()
        {
            // 无参构造函数，用于序列化
        }

        public CommandShortcut(string name, string command, bool isAsciiMode, bool addCRC, bool addCR, bool addLF)
        {
            Name = name;
            Command = command;
            IsAsciiMode = isAsciiMode;
            AddCRC = addCRC;
            AddCR = addCR;
            AddLF = addLF;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

