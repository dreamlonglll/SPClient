using SPClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPClient
{
    public partial class ShortcutForm : Form
    {
        // 快捷指令列表
        public List<CommandShortcut> CommandShortcuts { get; private set; }
        
        // 选中的快捷指令
        public CommandShortcut SelectedShortcut { get; private set; }
        
        public ShortcutForm(List<CommandShortcut> shortcuts)
        {
            InitializeComponent();
            
            // 复制快捷指令列表，避免直接修改原列表
            CommandShortcuts = new List<CommandShortcut>(shortcuts);
            SelectedShortcut = null;
            
            // 更新列表显示
            UpdateShortcutsList();
        }

        private void UpdateShortcutsList()
        {
            lstShortcuts.Items.Clear();
            foreach (CommandShortcut shortcut in CommandShortcuts)
            {
                lstShortcuts.Items.Add(shortcut);
            }
        }

        private void LstShortcuts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstShortcuts.SelectedItem != null)
            {
                CommandShortcut selectedShortcut = (CommandShortcut)lstShortcuts.SelectedItem;
                
                // 显示选中项详情
                txtName.Text = selectedShortcut.Name;
                txtCommand.Text = selectedShortcut.Command;
                chkIsAscii.Checked = selectedShortcut.IsAsciiMode;
                chkAddCRC.Checked = selectedShortcut.AddCRC;
                chkAddCR.Checked = selectedShortcut.AddCR;
                chkAddLF.Checked = selectedShortcut.AddLF;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string shortcutName = txtName.Text.Trim();
            string commandText = txtCommand.Text.Trim();
            
            if (string.IsNullOrEmpty(shortcutName))
            {
                MessageBox.Show("请输入指令名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (string.IsNullOrEmpty(commandText))
            {
                MessageBox.Show("请输入指令内容", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // 检查是否是编辑模式
            bool isEditMode = false;
            int editIndex = -1;
            
            for (int i = 0; i < CommandShortcuts.Count; i++)
            {
                if (CommandShortcuts[i].Name == shortcutName)
                {
                    isEditMode = true;
                    editIndex = i;
                    break;
                }
            }
            
            if (isEditMode)
            {
                // 更新现有指令
                DialogResult result = MessageBox.Show(
                    $"是否更新已存在的指令 \"{shortcutName}\"？",
                    "确认更新",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                
                if (result == DialogResult.Yes)
                {
                    // 更新指令
                    CommandShortcut updatedShortcut = new CommandShortcut(
                        shortcutName,
                        commandText,
                        chkIsAscii.Checked,
                        chkAddCRC.Checked,
                        chkAddCR.Checked,
                        chkAddLF.Checked
                    );
                    
                    CommandShortcuts[editIndex] = updatedShortcut;
                    UpdateShortcutsList();
                    
                    // 选中更新后的项
                    for (int i = 0; i < lstShortcuts.Items.Count; i++)
                    {
                        CommandShortcut item = (CommandShortcut)lstShortcuts.Items[i];
                        if (item.Name == shortcutName)
                        {
                            lstShortcuts.SelectedIndex = i;
                            break;
                        }
                    }
                    
                    MessageBox.Show("指令更新成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // 创建新指令
                CommandShortcut newShortcut = new CommandShortcut(
                    shortcutName,
                    commandText,
                    chkIsAscii.Checked,
                    chkAddCRC.Checked,
                    chkAddCR.Checked,
                    chkAddLF.Checked
                );
                
                CommandShortcuts.Add(newShortcut);
                UpdateShortcutsList();
                
                // 选中新添加的项
                lstShortcuts.SelectedIndex = lstShortcuts.Items.Count - 1;
                
                MessageBox.Show("指令添加成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (lstShortcuts.SelectedItem == null)
            {
                MessageBox.Show("请先选择要删除的指令", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            CommandShortcut selectedShortcut = (CommandShortcut)lstShortcuts.SelectedItem;
            
            // 确认删除
            DialogResult result = MessageBox.Show(
                $"确定要删除指令 \"{selectedShortcut.Name}\" 吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                CommandShortcuts.Remove(selectedShortcut);
                UpdateShortcutsList();
                
                // 清空编辑区域
                txtName.Text = "";
                txtCommand.Text = "";
                chkIsAscii.Checked = false;
                chkAddCRC.Checked = false;
                chkAddCR.Checked = false;
                chkAddLF.Checked = false;
                
                MessageBox.Show("指令已删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            if (lstShortcuts.SelectedItem == null)
            {
                MessageBox.Show("请先选择一个指令", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // 设置选中的快捷指令并返回
            SelectedShortcut = (CommandShortcut)lstShortcuts.SelectedItem;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 