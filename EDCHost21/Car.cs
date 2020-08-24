using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    //队名
    public enum Camp
    {
        NONE = 0, CMP_A, CMP_B
    };
    public class Car //选手的车
    {
        public const int PKG_CREDIT = 10;//获取物资可以得到10分;
        public const int RESCUE_CREDIT = 30;//营救人员可以得到30分;
        public const int FLOOD_PENALTY = 15;//经过泄洪口惩罚15分;
        public const int OBST_PENALTY = 10;//经过虚拟障碍物惩罚15分;
        public const int WRONG_DIR_PENALTY = 10;//逆行惩罚10分;
        public const int FOUL_PENALTY = 50; //犯规扣分50分;


        public Dot Pos;
        public Dot LastPos;
        public Camp Cmp { get; set; }//A or B get、set直接两个封装好的函数
        public int Score { get; set; } //得分
        public int PkgCount;//小车成功收集物资个数
        public int TaskState;//小车任务 0为上半场任务，1为下半场任务
        public int IsWithPassenger;//小车上是否载人 0未载人 1载人
        public int RescueCount;//小车成功运送人个数
        public int IsInMaze;//小车所在的区域 0在迷宫外 1在迷宫内
        public int CrossFloodCount;//小车经过泄洪口的次数
        public int CrossWallCount;//小车经过虚拟障碍的次数
        public int WrongDirCount;//小车逆行次数
        public int FoulCount;//犯规摁键次数

        public Car(Camp c, int task)
        {
            Cmp = c;
            Pos = new Dot(0, 0);
            Score = 0;
            PkgCount = 0;
            TaskState = task;
            IsWithPassenger = 0;
            RescueCount = 0;
            IsInMaze = 0;
            CrossFloodCount = 0;
            CrossWallCount = 0;
            WrongDirCount = 0;
            FoulCount = 0;
        }
        public void AddFloodPunish() //犯规
        {
            CrossFloodCount++;
            UpdateScore();
        }
        public void AddWallPunish()
        {
            CrossWallCount++; //前一个版本疑似typo（xhl）
            UpdateScore();
        }
        public void WrongDirectionplus()
        {
            WrongDirCount++;
            UpdateScore();
        }
        public void TransportNumplus()
        {
            RescueCount++;
            UpdateScore();
        }
        public void PickNumplus()
        {
            PkgCount++;
            UpdateScore();
        }
        public void FoulNumplus()
        {
            FoulCount++;
            UpdateScore();
        }
        public void SwitchPickedState()
        {
            if (IsWithPassenger == 0)
            {
                IsWithPassenger = 1;
            }
            else
            {
                IsWithPassenger = 0;
            }
        }

        //8-14 yd将Score后的代码折成多行，便于阅读
        public void UpdateScore()
        {
            Score = PkgCount * PKG_CREDIT
                + RescueCount * RESCUE_CREDIT
                - CrossFloodCount * FLOOD_PENALTY
                - OBST_PENALTY * CrossWallCount
                - WrongDirCount * WRONG_DIR_PENALTY
                - FoulCount * FOUL_PENALTY;
        }
    }
}
