using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC21HOST
{
    public enum Camp
    {
        none=0,CampA,CampB
    };
    public class Car //选手的车
    {
        public int PackageScore = 10;//获取物资可以得到10分;
        public int RescueScore = 30;//营救人员可以得到30分;
        public int StopPunish = 15;//经过泄洪口惩罚15分;
        public int ObstaclePunish = 10;//经过虚拟障碍物惩罚15分;
        public int WrongDirectionPunish = 10;//逆行惩罚10分;
        public int FoulPunish = 50;
        public Dot Pos;
        public Dot LastPos;
        public Camp Who { get; set; }//A or B get、set直接两个封装好的函数
        public int Score { get; set; } //得分
        public int Picknum;//小车成功收集物资个数
        public int Task;//小车任务 0为上半场任务，1为下半场任务
        public int transport;//小车上是否载人 0未载人 1载人
        public int transportnum;//小车成功运送人个数
        public int Area;//小车所在的区域 0在迷宫外 1在迷宫内
        public int StopPunishNum;//小车经过泄洪口的次数
        public int ObstaclePunishNum;//小车经过虚拟障碍的次数
        public int WrongDirectionNum;//小车逆行次数
        public int FoulNum;//犯规摁键次数
        public void StopPunishplus() //犯规
        {
            StopPunishNum++;
            UpdateScore();
        }
        public void  ObastaclePunishplus()
        {
            ObstaclePunishNum++; //前一个版本疑似typo（xhl）
            UpdateScore();
        }
        public void WrongDirectionplus()
        {
            WrongDirectionNum++;
            UpdateScore();
        }
        public void TransportNumplus()
        {
            transportnum++;
            UpdateScore();
        }
        public void PickNumplus()
        {
            Picknum++;
            UpdateScore();
        }
        public void FoulNumplus()
        {
            FoulNum++;
            UpdateScore();
        }
        public void Picked()
        {
            if(transport==0)
            {
                transport = 1;
            }
            else
            {
                transport = 0;
            }
        }
        public Car(Camp c,int task)
        {
            Who = c;
            Pos = new Dot();
            Score = 0;
            Picknum = 0;
            Task = task;
            transport = 0;
            transportnum = 0;
            Area = 0;
            StopPunishNum = 0;
            //ObastaclePunishNum = 0;
            ObstaclePunishNum = 0; //前一个版本疑似typo
            WrongDirectionNum = 0;
        }
        public void UpdateScore()
        {
            Score = Picknum * PackageScore + transportnum * RescueScore - StopPunishNum * StopPunish - ObstaclePunish * ObstaclePunishNum - WrongDirectionNum * WrongDirectionPunish - FoulNum * FoulPunish;
        }
    }
}
