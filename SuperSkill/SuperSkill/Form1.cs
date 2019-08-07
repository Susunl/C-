using EncryptionDecrypt;
using HotKeys;
using ProcessCtr;
using ReadWrite;
using System;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using Transform;
using KoalaStudio.BookshopManager;

namespace SuperSkill
{
    public partial class Form1 : Form
    {
        public Form1()                                                                          //固定不用修改
        {                                                                                       //固定不用修改
            InitializeComponent();                                                              //固定不用修改
        }                                                                                       //固定不用修改
        /// 时钟时间
        private void timer1_Tick(object sender, EventArgs e)
        {
            SysTime.Text
                    = "当前时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// 窗口1创建完毕过后干的事情
        private void Form1_Load(object sender, EventArgs e)
        {
               SysTimeTimer.Start();       ///启动时钟时间
            //全局变量.进程ID = ProCtr.GetProcessID("DNF");
            //string message = string.Format("进程id是：{0}", 全局变量.进程ID);
            //MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //初始化被单击
        private void button1_Click(object sender, EventArgs e)                                  //初始化被单击
        {
            //string i;
            //int j;
            全局变量.进程ID = ProCtr.GetProcessID("DNF");
            if (全局变量.进程ID == -1)
            {
                MessageBox.Show("未获取到游戏ID\n请进入游戏到仓库重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //读取职业技能被单击
        private void button3_Click(object sender, EventArgs e)
        {
            uint i = 0x4300,技能地址;
            string 技能名称 = "";
            int 技能等级;
            string 总技能="";
            ListView_Skill.Items.Clear();
            while (i <= 0x5000)
            {
                技能地址 = (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID,(uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID,基址.人物基址)+i);
                技能等级 = EncDec.Decrypt(全局变量.进程ID,技能地址+基址.技能等级偏移,基址.解密基址);
                if (技能等级 >= 0 && 技能等级 < 100)
                {
                    技能名称 = TransCtr.UnicodeToAnsi(ReadWriteCtr.ReadMemByteArray(全局变量.进程ID, (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID, 技能地址 + 基址.技能名称偏移), 50));
                    if (技能名称.Length > 1 && 技能名称.IndexOf("?") == -1&&技能名称.IndexOf("不使用") == -1&&总技能.IndexOf(技能名称)==-1&&技能等级>0)
                    {
                        this.ListView_Skill.Update();
                        ListViewItem lvi = this.ListView_Skill.Items.Add(Convert.ToString(i));
                        lvi.SubItems.Add(技能名称);
                        lvi.SubItems.Add(Convert.ToString(技能等级));
                        this.ListView_Skill.EndUpdate();
                        总技能 += 技能名称;
                    }
                }
                i +=4;
            }
        }
        //listview_skill事件被双击
        private void ListView_Skill_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int 技能地址1,技能等级,技能CD,技能数据;
            uint i1, i2, i3, i4, i5,技能地址2;
            string 总技能公式 = "",技能数据2 ="";
            //int 技能等级代码;
            全局变量.技能名 = this.ListView_Skill.SelectedItems[0].SubItems[1].Text;
            技能地址1 = ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID, 基址.人物基址) + Convert.ToUInt32(this.ListView_Skill.SelectedItems[0].Text));
            技能等级 = EncDec.Decrypt(全局变量.进程ID, (uint)技能地址1 + 基址.技能等级偏移, 基址.解密基址);
            this.ListView_SkillProperties.Items.Clear(); //清空ListView_SkillProperties内容
            技能CD = EncDec.Decrypt(全局变量.进程ID, (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.技能冷却1偏移) + 8*(uint)(技能等级-1), 基址.解密基址) / 1000;
            label1.Text = "当前技能cd为:" + 技能CD + "秒";
            //第一层遍历
            i1 = 0;
            while (i1 < 13)
            {
                技能地址2 = (uint)Math.Abs(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.超级技能偏移)) + i1)) + 20));

                if (总技能公式.IndexOf(Convert.ToString(技能地址2)) == -1 && 技能地址2 > 1000000000)
                {
                    技能数据 = EncDec.Decrypt(全局变量.进程ID, (uint)(技能地址2 + 8 * (技能等级 - 1)), 基址.解密基址);
                    if (技能数据 > 1)
                    {
                        技能数据2 = TransCtr.FloatToInt(技能数据);
                        this.ListView_SkillProperties.Update();
                        ListViewItem lvi = this.ListView_SkillProperties.Items.Add(技能数据2);       //liseview添加项
                        lvi.SubItems.Add(this.ListView_Skill.SelectedItems[0].SubItems[0].Text + "+" + Convert.ToString(基址.超级技能偏移) + "+" + Convert.ToString(i1) + "+20");      //添加次级项
                        lvi.SubItems.Add(Convert.ToString(技能地址2));
                        this.ListView_SkillProperties.EndUpdate();
                        总技能公式 = 总技能公式 + " " + Convert.ToString(技能地址2);

                    }
                } 

                i1 += 4;
            }
            //第二层遍历
            i1 = 0;
            while (i1 < 13)
            {
                i2 = 0;
                while (i2 < 13)
                {
                    技能地址2 = (uint)Math.Abs(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.超级技能偏移)) + i1)) + i2)) + 20));

                    if (总技能公式.IndexOf(Convert.ToString(技能地址2)) == -1 && 技能地址2 > 10000000)
                    {
                        技能数据 = EncDec.Decrypt(全局变量.进程ID, (uint)(技能地址2 + 8 * (技能等级 - 1)), 基址.解密基址);
                        if (技能数据 > 1)
                        {
                            技能数据2 = TransCtr.FloatToInt(技能数据);
                            this.ListView_SkillProperties.Update();
                            ListViewItem lvi = this.ListView_SkillProperties.Items.Add(技能数据2);       //liseview添加项
                            lvi.SubItems.Add(this.ListView_Skill.SelectedItems[0].SubItems[0].Text + "+" + Convert.ToString(基址.超级技能偏移) + "+" + Convert.ToString(i1) + "+" + Convert.ToString(i2) + "+20");      //添加次级项
                            lvi.SubItems.Add(Convert.ToString(技能地址2));
                            this.ListView_SkillProperties.EndUpdate();
                            总技能公式 = 总技能公式 + " " + Convert.ToString(技能地址2);
                        }
                    }
                    i2 += 4;
                }
                i1 += 4;
            }
            //第三层遍历
            i1 = 0;
            while (i1 < 13)
            {
                i2 = 0;
                while (i2 < 13)
                {
                    i3 = 0;
                    while (i3 < 13)
                    {
                        技能地址2 = (uint)Math.Abs(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.超级技能偏移)) + i1)) + i2)) + i3)) + 20));

                        if (总技能公式.IndexOf(Convert.ToString(技能地址2)) == -1 && 技能地址2 > 10000000)
                        {
                            技能数据 = EncDec.Decrypt(全局变量.进程ID, (uint)(技能地址2 + 8 * (技能等级 - 1)), 基址.解密基址);
                            if (技能数据 > 1)
                            {
                                技能数据2 = TransCtr.FloatToInt(技能数据);
                                this.ListView_SkillProperties.Update();
                                ListViewItem lvi = this.ListView_SkillProperties.Items.Add(技能数据2);       //liseview添加项
                                lvi.SubItems.Add(this.ListView_Skill.SelectedItems[0].SubItems[0].Text + "+" + Convert.ToString(基址.超级技能偏移) + "+" + Convert.ToString(i1) + "+" + Convert.ToString(i2) + "+" + Convert.ToString(i3) + "+20");      //添加次级项
                                lvi.SubItems.Add(Convert.ToString(技能地址2));
                                this.ListView_SkillProperties.EndUpdate();
                                总技能公式 = 总技能公式 + " " + Convert.ToString(技能地址2);
                            }
                        }
                        i3 += 4;
                    }
                    i2 += 4;
                }
                i1 += 4;
            }
            //第四层遍历
            i1 = 0;
            while (i1 < 13)
            {
                i2 = 0;
                while (i2 < 13)
                {
                    i3 = 0;
                    while (i3 < 13)
                    {
                        i4 = 0;
                        while (i4 < 13)
                        {
                            技能地址2 = (uint)Math.Abs(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.超级技能偏移)) + i1)) + i2)) + i3)) + i4)) + 20));

                            if (总技能公式.IndexOf(Convert.ToString(技能地址2)) == -1 && 技能地址2 > 10000000)
                            {
                                技能数据 = EncDec.Decrypt(全局变量.进程ID, (uint)(技能地址2 + 8 * (技能等级 - 1)), 基址.解密基址);
                                if (技能数据 > 1)
                                {
                                    技能数据2 = TransCtr.FloatToInt(技能数据);
                                    this.ListView_SkillProperties.Update();
                                    ListViewItem lvi = this.ListView_SkillProperties.Items.Add(技能数据2);       //liseview添加项
                                    lvi.SubItems.Add(this.ListView_Skill.SelectedItems[0].SubItems[0].Text + "+" + Convert.ToString(基址.超级技能偏移) + "+" + Convert.ToString(i1) + "+" + Convert.ToString(i2) + "+" + Convert.ToString(i3) + "+" + Convert.ToString(i4) + "+20");      //添加次级项
                                    lvi.SubItems.Add(Convert.ToString(技能地址2));
                                    this.ListView_SkillProperties.EndUpdate();
                                    总技能公式 = 总技能公式 + " " + Convert.ToString(技能地址2);
                                }
                            }
                            i4 += 4;
                        }
                        i3 += 4;
                    }
                    i2 += 4;
                }
                i1 += 4;
            }
            //第五层遍历
            i1 = 0;
            while (i1 < 13)
            {
                i2 = 0;
                while (i2 < 13)
                {
                    i3 = 0;
                    while (i3 < 13)
                    {
                        i4 = 0;
                        while (i4 < 13)
                        {
                            i5 = 0;
                            while (i5 < 13)
                            {
                                技能地址2 = (uint)Math.Abs(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)技能地址1 + 基址.超级技能偏移)) + i1)) + i2)) + i3)) + i4)) + i5)) + 20));

                                if (总技能公式.IndexOf(Convert.ToString(技能地址2)) == -1 && 技能地址2 > 10000000)
                                {
                                    技能数据 = EncDec.Decrypt(全局变量.进程ID, (uint)(技能地址2 + 8 * (技能等级 - 1)), 基址.解密基址);
                                    if (技能数据 > 1)
                                    {
                                        技能数据2 = TransCtr.FloatToInt(技能数据);
                                        this.ListView_SkillProperties.Update();
                                        ListViewItem lvi = this.ListView_SkillProperties.Items.Add(技能数据2);       //liseview添加项
                                        lvi.SubItems.Add(this.ListView_Skill.SelectedItems[0].SubItems[0].Text + "+" + Convert.ToString(基址.超级技能偏移) + "+" + Convert.ToString(i1) + "+" + Convert.ToString(i2) + "+" + Convert.ToString(i3) + "+" + Convert.ToString(i4) + "+" + Convert.ToString(i5) + "+20");      //添加次级项
                                        lvi.SubItems.Add(Convert.ToString(技能地址2));
                                        this.ListView_SkillProperties.EndUpdate();
                                        总技能公式 = 总技能公式 + " " + Convert.ToString(技能地址2);
                                    }
                                }
                                i5 += 4;
                            }
                            i4 += 4;
                        }
                        i3 += 4;
                    }
                    i2 += 4;
                }
                i1 += 4;
            }
        }
    
    private void 鸣谢_Click(object sender, EventArgs e)
        {

            功能.超级三速();
            //功能call.释放call((int)基址.人物基址, 800, 255, 0, 54106, 999999);
            //MessageBox.Show("释放成功", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //ListView_SkillProperties双击事件 添加属性到ListView_SkillProperties_Edit
        private void ListView_SkillProperties_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ListView_SkillProperties_Edit.Update();
            ListViewItem lvi = this.ListView_SkillProperties_Edit.Items.Add(this.ListView_SkillProperties.SelectedItems[0].SubItems[1].Text);
            lvi.SubItems.Add(this.ListView_SkillProperties.SelectedItems[0].SubItems[0].Text);
            lvi.SubItems.Add(全局变量.技能名);                                                                                          
            lvi.SubItems.Add(ListView_SkillProperties.SelectedItems[0].SubItems[2].Text);                                                                                                             
            this.ListView_SkillProperties_Edit.EndUpdate();
        }
        // ListView_SkillProperties_Edit双击事件 修改属性或备注
        private void ListView_SkillProperties_Edit_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = this.ListView_SkillProperties_Edit.GetItemAt(e.X, e.Y);
            int 列 = item.SubItems.IndexOf(item.GetSubItemAt(e.X, e.Y));   //列索引
            if (列 == 1 || 列 == 2)
            {
                Form2 form2 = new Form2(this);
                全局变量.要修改的数据 = this.ListView_SkillProperties_Edit.SelectedItems[0].SubItems[列].Text;
                全局变量.要修改的列 = 列 ;
                form2.ShowDialog();
                this.ListView_SkillProperties_Edit.SelectedItems[0].SubItems[全局变量.要修改的列].Text = 全局变量.要修改的数据;
            }


        }
        private void 修改属性()
        {
            //声明变量
            int index = -1, index2, t_level, i = 0;
            uint t_add, t_pet, t_skilladd = 0;

            //ListView_SkillProperties_Edit中有项目才进行修改
            if (this.ListView_SkillProperties_Edit.Items.Count > 0)
            {
                //循环项目数次
                for (i = 0; i < this.ListView_SkillProperties_Edit.Items.Count; i++)
                {
                    index = -1;
                    index2 = 0;
                    index2 = this.ListView_SkillProperties_Edit.Items[i].SubItems[0].Text.IndexOf("+", index + 1);   //公式第一个+号位置
                    t_pet = Convert.ToUInt32(this.ListView_SkillProperties_Edit.Items[i].SubItems[0].Text.Substring(index + 1, index2 - index - 1));  //技能偏移
                    t_skilladd = (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)ReadWriteCtr.ReadMemInt(全局变量.进程ID, 基址.人物基址) + t_pet);   //技能地址
                    t_level = EncDec.Decrypt(全局变量.进程ID, (uint)t_skilladd + 基址.技能等级偏移, 基址.解密基址);   //技能等级
                    Thread.Sleep(30);   //延迟30毫秒
                    t_add = (uint)ReadWriteCtr.ReadMemCode(全局变量.进程ID, Convert.ToString(基址.人物基址) + "+" + this.ListView_SkillProperties_Edit.Items[i].SubItems[0].Text);
                    //加密
                    EncDec.Encryption(全局变量.进程ID, (uint)(t_add + 8 * (t_level - 1)), TransCtr.IntToFloat(this.ListView_SkillProperties_Edit.Items[i].SubItems[1].Text), 基址.解密基址);
                    Thread.Sleep(30);   //延迟30毫秒进入下一循环
                }
            }
            else
            {
                MessageBox.Show("List中没有数据", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //搜索技能 然后焦点于指定技能
        private void Button_SearchSkill_Click(object sender, EventArgs e)
        {
            SearchSkill();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            修改属性();
        }

        private void TextBox_SearchSkill_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0xD)   //如果按下的是Enter
            {
                SearchSkill();
            }
        }
        private void SearchSkill()
        {
            int t_index = 0;
            if (this.ListView_Skill.SelectedItems.Count > 0)
                t_index = this.ListView_Skill.SelectedItems[0].Index + 1;
            for (int i = t_index; i < this.ListView_Skill.Items.Count; i++) //第一次循环，从选中位置开始
            {
                if (this.ListView_Skill.Items[i].SubItems[1].Text.IndexOf(this.TextBox_SearchSkill.Text) != -1)
                {
                    this.ListView_Skill.Items[i].Selected = true;       //选中行
                    this.ListView_Skill.EnsureVisible(i);               //滚动到指定的行位置
                    this.ListView_Skill.Focus();                        //ListView_Skill获得焦点
                    return;
                }
            }
            for (int j = 0; j < this.ListView_Skill.Items.Count; j++)   //第二次循环，从头开始，以搜索选中项之前的内容
            {
                if (this.ListView_Skill.Items[j].SubItems[1].Text.IndexOf(this.TextBox_SearchSkill.Text) != -1)
                {
                    this.ListView_Skill.Items[j].Selected = true;   //选中行
                    this.ListView_Skill.EnsureVisible(j);           //滚动到指定的行位置
                    this.ListView_Skill.Focus();                    //ListView_Skill获得焦点
                    return;
                }
            }
        }

        private void 删除选中项toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.ListView_SkillProperties_Edit.SelectedItems.Count > 0)
                this.ListView_SkillProperties_Edit.SelectedItems[0].Remove();
        }

        private void 清空所有项toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.ListView_SkillProperties_Edit.Items.Clear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            功能.技能无CD();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            功能.超级三速();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            功能.一键评分();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            功能.全屏吸物();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            HotKey.RegisterHotKey(Handle, 100, 0, Keys.F3);
            HotKey.RegisterHotKey(Handle, 101, 0, Keys.V);
            HotKey.RegisterHotKey(Handle, 102, 0, Keys.Oemtilde);
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
            HotKey.UnregisterHotKey(Handle, 101);
            HotKey.UnregisterHotKey(Handle, 102);
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;//如果m.Msg的值为0x0312那么表示用户按下了热键
                                         //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:    //按下的是Shift+S
                            修改属性();        //此处填写快捷键响应代码         
                            break;
                        case 101:    //按下的是Ctrl+B
                            功能.全屏吸物();                 //此处填写快捷键响应代码
                            break;
                        case 102:    //按下的是Alt+D
                            功能.一键评分();         //此处填写快捷键响应代码
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
