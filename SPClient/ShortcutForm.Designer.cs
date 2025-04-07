using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SPClient
{
    partial class ShortcutForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // 列表控件
        private ListBox lstShortcuts;
        
        // 编辑区域控件
        private TextBox txtName;
        private TextBox txtCommand;
        private CheckBox chkIsAscii;
        private CheckBox chkAddCRC;
        private CheckBox chkAddCR;
        private CheckBox chkAddLF;
        
        // 按钮
        private Button btnAdd;
        private Button btnDelete;
        private Button btnSelect;
        private Button btnCancel;

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ShortcutForm
            // 
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Name = "ShortcutForm";
            this.Text = "快捷指令管理";
            this.StartPosition = FormStartPosition.CenterParent;
            
            // 初始化控件
            InitializeControls();
            
            this.ResumeLayout(false);
        }

        private void InitializeControls()
        {
            // 创建列表区域
            GroupBox grpList = new GroupBox();
            grpList.Text = "指令列表";
            grpList.Left = 12;
            grpList.Top = 12;
            grpList.Width = 250;
            grpList.Height = 370;
            this.Controls.Add(grpList);

            lstShortcuts = new ListBox();
            lstShortcuts.Left = 10;
            lstShortcuts.Top = 20;
            lstShortcuts.Width = 230;
            lstShortcuts.Height = 340;
            lstShortcuts.SelectedIndexChanged += LstShortcuts_SelectedIndexChanged;
            grpList.Controls.Add(lstShortcuts);

            // 创建编辑区域
            GroupBox grpEdit = new GroupBox();
            grpEdit.Text = "指令详情";
            grpEdit.Left = 274;
            grpEdit.Top = 12;
            grpEdit.Width = 310;
            grpEdit.Height = 370;
            this.Controls.Add(grpEdit);

            // 名称标签和文本框
            Label lblName = new Label();
            lblName.Text = "名称:";
            lblName.Left = 10;
            lblName.Top = 25;
            lblName.Width = 60;
            grpEdit.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Left = 80;
            txtName.Top = 22;
            txtName.Width = 210;
            grpEdit.Controls.Add(txtName);

            // 命令标签和文本框
            Label lblCommand = new Label();
            lblCommand.Text = "命令:";
            lblCommand.Left = 10;
            lblCommand.Top = 55;
            lblCommand.Width = 60;
            grpEdit.Controls.Add(lblCommand);

            txtCommand = new TextBox();
            txtCommand.Left = 80;
            txtCommand.Top = 52;
            txtCommand.Width = 210;
            txtCommand.Multiline = true;
            txtCommand.Height = 150;
            grpEdit.Controls.Add(txtCommand);

            // 选项复选框
            chkIsAscii = new CheckBox();
            chkIsAscii.Text = "ASCII模式";
            chkIsAscii.Left = 10;
            chkIsAscii.Top = 215;
            chkIsAscii.Width = 140;
            grpEdit.Controls.Add(chkIsAscii);

            chkAddCRC = new CheckBox();
            chkAddCRC.Text = "添加CRC校验";
            chkAddCRC.Left = 160;
            chkAddCRC.Top = 215;
            chkAddCRC.Width = 140;
            grpEdit.Controls.Add(chkAddCRC);

            chkAddCR = new CheckBox();
            chkAddCR.Text = "添加回车符(CR)";
            chkAddCR.Left = 10;
            chkAddCR.Top = 245;
            chkAddCR.Width = 140;
            grpEdit.Controls.Add(chkAddCR);

            chkAddLF = new CheckBox();
            chkAddLF.Text = "添加换行符(LF)";
            chkAddLF.Left = 160;
            chkAddLF.Top = 245;
            chkAddLF.Width = 140;
            grpEdit.Controls.Add(chkAddLF);

            // 操作按钮
            btnAdd = new Button();
            btnAdd.Text = "添加/更新";
            btnAdd.Left = 25;
            btnAdd.Top = 290;
            btnAdd.Width = 120;
            btnAdd.Click += BtnAdd_Click;
            grpEdit.Controls.Add(btnAdd);

            btnDelete = new Button();
            btnDelete.Text = "删除";
            btnDelete.Left = 160;
            btnDelete.Top = 290;
            btnDelete.Width = 120;
            btnDelete.Click += BtnDelete_Click;
            grpEdit.Controls.Add(btnDelete);

            // 底部按钮
            btnSelect = new Button();
            btnSelect.Text = "选择并返回";
            btnSelect.Left = 360;
            btnSelect.Top = 400;
            btnSelect.Width = 120;
            btnSelect.Click += BtnSelect_Click;
            this.Controls.Add(btnSelect);

            btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Left = 490;
            btnCancel.Top = 400;
            btnCancel.Width = 80;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        #endregion
    }
} 