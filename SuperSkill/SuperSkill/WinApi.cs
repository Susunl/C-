using ProcessCtr;
using ReadWrite;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SuperSkill;

//命名空间 读写
namespace ReadWrite
{
    /// <summary>
    /// 类:内存读写API
    /// </summary>
    public class ReadWriteAPI
    {
        public const int PROCESS_ALL_ACCESS = 2035711;  //PROCESS_ALL_ACCESS常量

        public struct MemAttribute      //自定义数据类型内存属性
        {
            public int RegBaseAdd;     //区域基地址，与查询的地址值相同，但是四舍五入为页面的边界值
            public int AlloBaseAdd;    //分配基地址指明(用VirtualAlloc函数)分配内存区域的基地址，查询的地址在该区域之内
            public int OriProtect;     //原始保护，指明该地址空间区域被次级保留时赋予该区域的保护属性
            public int Size;           //区域大小，用于指明内存块从基地址开始的所有页面的大小(以字节为计量单位)，这些页面与查询的地址的页面拥有相同的保护属性状态和类型
            public int State;          //状态，用于指明所有相邻页面的状态
            public int Protect;        //保护，用于指明所有相邻页面(内存块)的保护属性，这些页面拥有相同的保护属性状态和类型，意义同原始保护属性
            public int Type;           //类型，用于指明支持所有相邻页面的物理储存器的类型，这些相邻页面拥有相同的保护属性状态和类型
        }

        /// <summary>
        /// 从指定内存中读取字节集数据
        /// </summary>
        /// <param name="handle">进程句柄</param>
        /// <param name="address">内存地址</param>
        /// <param name="data">读取数据</param>
        /// <param name="size">长度</param>
        /// <param name="read">实际读取长度</param>
        [DllImport("kernel32.dll")]
        public static extern int ReadProcessMemory(IntPtr handle, uint address, [Out] byte[] data, int size, int read);

        /// <summary>
        /// 从指定内存中写入字节集数据
        /// </summary>
        /// <param name="handle">进程句柄</param>
        /// <param name="address">内存地址</param>
        /// <param name="data">写入数据</param>
        /// <param name="size">长度</param>
        /// <param name="write">实际写入长度</param>
        /// <returns>成功与否</returns>
        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr handle, uint address, [In] byte[] data, int size, int write);

        /// <summary>
        /// 查询地址空间中内存地址的信息
        /// </summary>
        /// <param name="handle">进程句柄</param>
        /// <param name="address">内存地址</param>
        /// <param name="info">内存属性</param>
        /// <param name="Length">长度</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr handle, uint address, ReadWriteAPI.MemAttribute info, int Length);

    }

    /// <summary>
    /// 类:读写操作
    /// </summary>
    public class ReadWriteCtr
    {
        /// <summary>
        /// 读内存整数型
        /// </summary>
        /// <param name="ProcessID">进程ID，-1为自进程</param>
        /// <param name="Address">地址 无符号整数型</param>
        /// <returns>返回读取值，失败返回-1</returns>
        public static int ReadMemInt(int ProcessID, uint Address)
        {
            //声明变量
            int a = 0;
            byte[] temp = new byte[4];
            IntPtr handle = new IntPtr();

            if (ProcessID == -1)    //-1读取自进程
                handle = ProcessAPI.GetCurrentProcess();
            else
                handle = ProcessAPI.OpenProcess(ReadWriteAPI.PROCESS_ALL_ACCESS, false, ProcessID); //获取句柄
            a = ReadWriteAPI.ReadProcessMemory(handle, Address, temp, 4, 0);
            ProcessAPI.CloseHandle(handle); //关闭对象
            if (a == 0)
                return -1;  //读取失败返回-1
            else
                return BitConverter.ToInt32(temp, 0);
        }


        /// <summary>
        /// 读内存字节集
        /// </summary>
        /// <param name="ProcessID">进程ID，-1为自进程</param>
        /// <param name="Address">地址 无符号整数型</param>
        /// <param name="Size">读取长度 0为智能读取</param>
        /// <returns>返回字节数组，失败返回空字节集</returns>
        public static byte[] ReadMemByteArray(int ProcessID, uint Address, int Size)
        {
            //声明变量
            int a, t_size = 0;
            IntPtr handle = new IntPtr();
            ReadWriteAPI.MemAttribute mematt = new ReadWriteAPI.MemAttribute();

            t_size = Size;
            if (ProcessID == -1)    //-1读取自进程
                handle = ProcessAPI.GetCurrentProcess();
            else
                handle = ProcessAPI.OpenProcess(ReadWriteAPI.PROCESS_ALL_ACCESS, false, ProcessID); //获取句柄
            if (t_size == 0)        //大小为0智能读取
            {
                ReadWriteAPI.VirtualQueryEx(handle, Address, mematt, 28);
                t_size = mematt.Size + mematt.RegBaseAdd - (int)Address;
            }
            byte[] temp = new byte[t_size];
            a = ReadWriteAPI.ReadProcessMemory(handle, Address, temp, t_size, 0);
            ProcessAPI.CloseHandle(handle); //关闭对象
            if (a != 0)
                return temp;
            else
            {
                byte[] falsedata = new byte[1];     //失败返回空字节集
                return falsedata;
            }

        }


        /// <summary>
        /// 读内存整数型2
        /// </summary>
        /// <param name="ProcessID">进程ID</param>
        /// <param name="Address">地址</param>
        /// <param name="Offsets">偏移量 整数数组</param>
        /// <returns>返回读取值，失败返回-1</returns>
        public static int ReadMemInt2(int ProcessID, uint Address, uint[] Offsets)
        {
            //声明变量
            int t_data = 0;
            uint t_add = Address;
            IntPtr handle = new IntPtr();

            if (ProcessID == -1)    //-1读取自进程
                handle = ProcessAPI.GetCurrentProcess();
            else
                handle = ProcessAPI.OpenProcess(ReadWriteAPI.PROCESS_ALL_ACCESS, false, ProcessID); //获取句柄

            for (int i = 0; i < Offsets.Length; i++)
            {
                t_data = ReadMemInt(ProcessID, t_add);
                t_add = (uint)t_data + Offsets[i];
            }
            t_data = ReadMemInt(ProcessID, t_add);
            ProcessAPI.CloseHandle(handle); //关闭对象
            if (t_data == 0)
                return -1;  //失败返回-1
            else
                return t_data;
        }


        /// <summary>
        /// 读内存代码
        /// </summary>
        /// <param name="ProcessID">进程ID</param>
        /// <param name="Address">地址 文本型，用+号连接地址</param>
        /// <returns>返回读取值，失败返回-1</returns>
        public static int ReadMemCode(int ProcessID, string Address)
        {
            //声明变量
            uint baseadd = 0;
            int index1 = -1, index2 = 0, count = 0;
            string t_add = Address;

            if (t_add.Length < 2)   //地址小于2位数即地址错误，返回-1
                return -1;
            count = t_add.Length - t_add.Replace("+", "").Length;   //计算有几个+号，用总长度-去除+号后的长度，即+号数量
            uint[] offsets = new uint[count];                       //声明新数组，储存偏移
            index2 = t_add.IndexOf("+", index1 + 1);                //获得首个+号位置
            baseadd = Convert.ToUInt32(t_add.Substring(index1 + 1, index2 - index1 - 1));   //获得基址
            index1 = index2;
            for (int i = 0; i < (count - 1); i++)                   //循环获得偏移
            {
                index2 = t_add.IndexOf("+", index1 + 1);
                offsets[i] = Convert.ToUInt32(t_add.Substring(index1 + 1, index2 - index1 - 1));
                index1 = index2;
            }
            index1 = t_add.LastIndexOf("+");
            offsets[offsets.Length - 1] = Convert.ToUInt32(t_add.Substring(index1 + 1, t_add.Length - index1 - 1));
            return ReadMemInt2(ProcessID, baseadd, offsets);
        }


        /// <summary>
        /// 写内存整数型
        /// </summary>
        /// <param name="ProcessID">进程ID，-1为自进程</param>
        /// <param name="Address">地址 无符号整数型</param>
        /// <param name="Data">写入数据</param>
        /// <returns>返回是否成功</returns>
        public static bool WriteMemInt(int ProcessID, uint Address, int Data)
        {
            //声明变量
            int a = 0;
            IntPtr handle = new IntPtr();
            byte[] temp = new byte[4];
            temp = BitConverter.GetBytes(Data);

            if (ProcessID == -1)    //-1为自进程
                handle = ProcessAPI.GetCurrentProcess();
            else
                handle = ProcessAPI.OpenProcess(ReadWriteAPI.PROCESS_ALL_ACCESS, false, ProcessID); //获取句柄
            a = ReadWriteAPI.WriteProcessMemory(handle, Address, temp, 4, 0);
            ProcessAPI.CloseHandle(handle); //关闭对象
            if (a == 0)         //返回bool型
                return false;
            else
                return true;
        }


        /// <summary>
        /// 写内存字节集
        /// </summary>
        /// <param name="ProcessID">进程ID，-1为自进程</param>
        /// <param name="Address">地址 无符号整数型</param>
        /// <param name="Data">写入数据 字节数组型</param>
        /// <param name="Size">写入长度 0为完整长度</param>
        /// <returns>返回是否成功</returns>
        public static bool WriteMemByteArray(int ProcessID, uint Address, byte[] Data, int Size)
        {
            //声明变量
            int a = 0;
            IntPtr handle = new IntPtr();

            if (ProcessID == -1)    //-1为自进程
                handle = ProcessAPI.GetCurrentProcess();
            else
                handle = ProcessAPI.OpenProcess(ReadWriteAPI.PROCESS_ALL_ACCESS, false, ProcessID); //获取句柄
            if (Size == 0)
                a = ReadWriteAPI.WriteProcessMemory(handle, Address, Data, Data.Length, 0);
            else
                a = ReadWriteAPI.WriteProcessMemory(handle, Address, Data, Size, 0);
            ProcessAPI.CloseHandle(handle); //关闭对象
            if (a == 0)         //返回bool型
                return false;
            else
                return true;
        }
    }
}

//命名空间 进程操作
namespace ProcessCtr
{
    /// <summary>
    /// 类:与进程操作有关的WindowsAPI
    /// </summary>
    public class ProcessAPI
    {
        /// <summary>
        /// 打开当前进程
        /// </summary>
        /// <returns>返回句柄</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>
        /// 打开指定进程
        /// </summary>
        /// <param name="DesiredAccess">访问级别 PROCESS_ALL_ACCESS为完全访问</param>
        /// <param name="InheritHandle">子进程继承</param>
        /// <param name="ProcessId">进程ID</param>
        /// <returns>返回句柄</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int DesiredAccess, bool InheritHandle, int ProcessId);

        /// <summary>
        /// 关闭指定内核对象
        /// </summary>
        /// <param name="handle">已打开的对象句柄</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
    }

    /// <summary>
    /// 类:进程操作
    /// </summary>
    public class ProCtr
    {
        /// <summary>
        /// 获取进程ID Process类
        /// </summary>
        /// <param name="ProcessName">进程名 不需要.exe</param>
        /// <returns>返回进程ID</returns>
        public static int GetProcessID(string ProcessName)
        {
            //声明Process[]对象
            Process[] localByName = Process.GetProcessesByName(ProcessName);

            if (localByName.Length == 0)
                return -1;                  //如果没找到返回-1
            else
                return localByName[0].Id;   //找到返回数组第一元素的ID属性
        }
    }
}

//命名空间 加密解密
namespace EncryptionDecrypt
{
    //类:加密解密
    public class EncDec
    {
        /// <summary>
        /// 加密，照搬E语言
        /// </summary>
        /// <param name="ProcessID">进程ID</param>
        /// <param name="Address">地址</param>
        /// <param name="Data">加密数据</param>
        /// <param name="DecryptionAdd">解密基址</param>
        public static void Encryption(int ProcessID, uint Address, int Data, uint DecryptionAdd)
        {
            //声明变量
            int encryptionid, offsetpara, offsetadd, data, tailofadd, decryptadd, ax, si = 0;

            encryptionid = ReadWriteCtr.ReadMemInt(ProcessID, Address);
            decryptadd = ReadWriteCtr.ReadMemInt(ProcessID, DecryptionAdd);
            offsetpara = ReadWriteCtr.ReadMemInt(ProcessID, (uint)(decryptadd + (encryptionid >> 16) * 4 + 36));
            offsetadd = offsetpara + (encryptionid & 65535) * 4 + 8468;
            offsetpara = ReadWriteCtr.ReadMemInt(ProcessID, (uint)offsetadd);
            data = offsetpara & 65535;
            data = data + (data << 16);
            ax = offsetpara & 65535;

            tailofadd = (int)Address & 15;
            switch (tailofadd)
            {
                case 0:
                    si = Data >> 16;
                    si = si - ax;
                    si = si + Data;
                    break;
                case 4:
                    si = (Data & 65535) - (Data >> 16);
                    break;
                case 8:
                    si = Data >> 16;
                    si = si * Data;
                    break;
                case 12:
                    si = Data >> 16;
                    si = si + Data;
                    si = si + ax;
                    break;
                default:
                    return;
            }
            ax = si ^ ax;
            byte[] temp = BitConverter.GetBytes(ax);
            ReadWriteCtr.WriteMemByteArray(ProcessID, (uint)(offsetadd + 2), temp, temp.Length);
            ReadWriteCtr.WriteMemInt(ProcessID, Address + 4, data ^ Data);
        }
        public static void 超级加密(int Address, int value, int type = 0)
        {
            int 加密ID = ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)Address);
            int 偏移参数 = ReadWriteCtr.ReadMemInt(全局变量.进程ID, 基址.解密基址);
            偏移参数 = ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)(偏移参数 + (加密ID >> 16) * 4 + 36));
            int 偏移地址 = 偏移参数 + (加密ID & 65535) * 4 + 8468;
            偏移参数 = ReadWriteCtr.ReadMemInt(全局变量.进程ID, (uint)偏移地址);
            int data = 偏移参数 & 0xffff;
            data = data + (data << 16);
            short ax = (short)(偏移参数 & 0xffff);
            short si = 0;
            if (type == 0)
            {
                int 地址尾 = Address & 15;
                switch (地址尾)
                {
                    case 0:
                        si = (short)(value >> 16);
                        si = (short)(si - ax);
                        si = (short)(si + value);
                        break;
                    case 4:
                        si = (short)((value & 0xFFFF) - (value >> 16)); ;
                        break;
                    case 8:
                        si = (short)(value >> 16);
                        si = (short)(si * value);
                        break;
                    case 12:
                        si = (short)(value >> 16);
                        si = (short)(si + value);
                        si = (short)(si + ax);
                        break;
                    default:
                        break;
                }
            }
            else if (type == 1)
                si = (short)(value & 16);
            else if (type == 2)
                si = (short)value;
            else if (type == 3)
            {
                si = (short)(value >> 16);
                si += (short)value;
            }
            else if (type == 4)
            {
                si = (short)(value >> 16);
                si = (short)(si + value & 0xFFFF);
            }
            ax = (short)(si ^ ax);
            byte[] temp = BitConverter.GetBytes(ax);
            ReadWriteCtr.WriteMemByteArray(全局变量.进程ID, (uint)(偏移地址 + 2), temp, temp.Length);
            data = data ^ value;
            int a = Address + (type != 4 ? 4 : 8);
            ReadWriteCtr.WriteMemInt(全局变量.进程ID, (uint)a, data);
        }

        /// <summary>
        /// 解密，照搬E语言
        /// </summary>
        /// <param name="ProcessID">进程ID</param>
        /// <param name="Address">地址</param>
        /// <param name="DecryptionAdd">解密基址</param>
        /// <returns>返回解密值，失败返回0</returns>
        public static int Decrypt(int ProcessID, uint Address, uint DecryptionAdd)
        {
            //声明变量
            int eax, edx, esi = 0;

            eax = ReadWriteCtr.ReadMemInt(ProcessID, Address);
            esi = ReadWriteCtr.ReadMemInt(ProcessID, DecryptionAdd);
            edx = eax >> 16;
            edx = ReadWriteCtr.ReadMemInt(ProcessID, (uint)(esi + edx * 4 + 36));
            eax = eax & 65535;
            eax = ReadWriteCtr.ReadMemInt(ProcessID, (uint)(edx + eax * 4 + 8468));
            edx = eax & 65535;
            esi = edx << 16;
            esi = esi | edx;
            esi = esi ^ ReadWriteCtr.ReadMemInt(ProcessID, Address + 4);
            return (esi);
        }
    }
}

//命名空间 转换
namespace Transform
{
    //类:转换
    public class TransCtr
    {
        /// <summary>
        /// 宽字符到双字节
        /// </summary>
        /// <param name="CodePage"></param>
        /// <param name="Flags"></param>
        /// <param name="WideCharStr"></param>
        /// <param name="WideChar"></param>
        /// <param name="MultiByteStr"></param>
        /// <param name="MultiByte"></param>
        /// <param name="DefaultChar"></param>
        /// <param name="UsedDefaultChar"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern int WideCharToMultiByte(uint CodePage, int Flags, byte[] WideCharStr, int WideChar, byte[] MultiByteStr, int MultiByte, int DefaultChar, bool UsedDefaultChar);

        /// <summary>
        /// Unicode字节集转Ansi，照搬E语言     考虑新方法
        /// </summary>
        /// <param name="UnicodeBytes">Unicode字节集</param>
        /// <returns>返回转换后的文本 string型</returns>
        public static string UnicodeToAnsi(byte[] UnicodeBytes)
        {
            int index = 0;
            string text = "";
            index = UnicodeBytes.Length;
            index = WideCharToMultiByte(936, 512, UnicodeBytes, index, new byte[1], 0, 0, false);
            byte[] data = new byte[index];
            WideCharToMultiByte(936, 512, UnicodeBytes, -1, data, index, 0, false);
            text = Encoding.Default.GetString(data);
            return text;
        }

        /// <summary>
        /// 浮点到整数，照搬E语言
        /// </summary>
        /// <param name="Value">待转换浮点数 整数型</param>
        /// <returns>返回转换后的值 string型</returns>
        public static string FloatToInt(int Value)
        {
            int c = Value;
            byte[] buff = new byte[4];

            unsafe
            {
                int* p = &c;
                if (c > 10000)
                {
                    buff = ReadWriteCtr.ReadMemByteArray(-1, (uint)p, 4);
                    if (BitConverter.ToInt32(buff, 0) == 0)
                        return "-1";
                    else
                        return Convert.ToString(BitConverter.ToSingle(buff, 0));

                }
                else
                    return Convert.ToString(c);
            }
        }

        /// <summary>
        /// 整数到浮点，照搬E语言
        /// </summary>
        /// <param name="Value">待转换整数 字符串</param>
        /// <returns>返回转换后的值 整数型</returns>
        public static int IntToFloat(string Value)
        {
            Single a = Convert.ToSingle(Value);
            byte[] b = BitConverter.GetBytes(a);
            return BitConverter.ToInt32(b, 0);
        }
    }
}

//命名空间 热键
namespace HotKeys
{
    //类:热键
    public class HotKeyCtr
    {

        /// <summary>
        /// 如果函数执行成功，返回值不为0。
        /// 如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, Keys vk);

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hWnd">要取消热键的窗口的句柄</param>
        /// <param name="id">要取消热键的ID</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="keyModifiers">组合键</param>
        /// <param name="key">热键</param>
        public static void RegHotKey(IntPtr hwnd, int hotKeyId, int keyModifiers, Keys key)
        {
            if (!RegisterHotKey(hwnd, hotKeyId, keyModifiers, key))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 1409)
                {
                    MessageBox.Show("热键被占用 ！");
                }
                else
                {
                    MessageBox.Show("注册热键失败！错误代码：" + errorCode);
                }
            }
        }

        /// <summary>
        /// 注销热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        public static void UnRegHotKey(IntPtr hwnd, int hotKeyId)
        {
            //注销指定的热键
            UnregisterHotKey(hwnd, hotKeyId);
        }
        private void calltab()
        {
            SslnEngine.Asm asm = new SslnEngine.Asm();
            asm.Pushad();
            asm.Mov_EAX_DWORD_Ptr(0x916b3c);
            asm.Mov_EAX_DWORD_Ptr_EAX_Add(0x1c);
            asm.Mov_EAX_DWORD_Ptr_EAX_Add(0x28);
            asm.Mov_ECX_EAX();
            asm.Push(0);
            asm.Mov_EBX(0x0045F410);
            asm.Call_EBX();
            asm.Popad();
            asm.Ret();
            //asm.RunAsm(this.);
        }
    }
}