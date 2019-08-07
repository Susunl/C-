using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSkill
{
    class 功能call
    {
        public static void 释放call(int 人物基质,int X坐标,int Y坐标,int Z坐标,int 代码,int 伤害)
        {
            int 释放地址 = (int)基址.释放call;
            SslnEngine.Asm asm = new SslnEngine.Asm();
            asm.Push(Z坐标);
            asm.Push(Y坐标);
            asm.Push(X坐标);
            asm.Push(伤害);
            asm.Push(代码);
            asm.Push(106189416);
            asm.Mov_EDI(37090720);
            asm.Mov_EAX_EDI();
            asm.Call_EAX();
            asm.Add_ESP(24);
            asm.RunAsm(全局变量.进程ID);
        }





    }
}
