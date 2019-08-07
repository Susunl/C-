using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperSkill
{
    public partial class Form2 : Form
    {
        public Form1 f1;
        public Form2(Form1 f1)
        {
            this.f1 = f1;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //设置textBox1文本为要修改的文本
            this.textBox1.Text = 全局变量.要修改的数据;
            //设置窗口2位置
            this.Top = f1.Top + 330;
            this.Left = f1.Left + 64;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            全局变量.要修改的数据 = this.textBox1.Text;
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //如果列索引为1
            if (全局变量.要修改的列 == 1)
            {
                //只允许输入数字
                if (e.KeyChar == 0x20)
                    e.KeyChar = (char)0;  //禁止空格键
                if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0))
                    return;   //处理负数
                if (e.KeyChar > 0x20)
                {
                    try
                    {
                        double.Parse(((TextBox)sender).Text + e.KeyChar.ToString());
                    }
                    catch
                    {
                        e.KeyChar = (char)0;   //处理非法字符
                    }
                }
            }
            if (e.KeyChar == 0xD)   //按下enter
            {
                全局变量.要修改的数据 = this.textBox1.Text;
                this.Close();
            }
        }
    }
}
