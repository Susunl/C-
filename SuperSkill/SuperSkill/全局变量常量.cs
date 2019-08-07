using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSkill
{
    public class 全局变量
    {
        public static int 进程ID;
        public static string 技能名;
        public static string 要修改的数据;
        public static int 要修改的列;


    }
    public class 基址
    {
        public const uint 人物基址 = 0x6545268;
        public const uint 解密基址 = 0x658A748;
        public const uint 释放call = 0x235F5A0;
        public const uint 职业基址 = 0x6264BD8;
        public const uint 评分基址 = 0x640F764;
        public const uint 自动捡物 = 0x2868FD6;
        public const uint 游戏状态 = 0x626396C;
        public const uint 类型偏移 = 0xB4;
        public const uint 评分偏移 = 0x110;
        public const uint 超级技能偏移 = 0xC20;
        public const uint 技能等级偏移 = 0x10E0;
        public const uint 技能名称偏移 = 0x74;
        public const uint 技能冷却1偏移 = 0x728;
        public const uint 技能冷却2偏移 = 0x73C;
        public const uint 技能冷却3偏移 = 0x750;
        public const uint 技能冷却4偏移 = 0x764;
        public const uint 技能冷却5偏移 = 0x778;
        public const uint 戒指偏移 = 0x3484;
        public const uint 上衣偏移 = 0x3468;
        public const uint 攻速偏移 = 0xA0C;
        public const uint 移速偏移 = 0xA04;
        public const uint 释放偏移 = 0xA14;
        public const uint 地图偏移 = 0xD8;
    }
}
