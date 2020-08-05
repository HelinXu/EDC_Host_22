﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC21HOST
{
    public class Car //选手的车
    {
        public const int PunishScore = 100; //惩罚分数
        public const double BonusRate = 0.5; //增加总分比例
        public Dot Pos;
        public Camp Who { get; set; } //A or B
        public int Score { get; set; } //得分
        public int PersonCnt; //人员数
        public int BallGetCnt; //抓取小球数
        public int BallOwnCnt; //己方小球数
        public int LastScore { get; set; } //之前回合得分
        public int LastPersonCnt; //之前回合人员数
        public int LastBallGetCnt; //之前回合抓取小球数
        public int LastBallOwnCnt; //之前回合己方小球数
        public bool HaveBonus; //是否增加总分
        public bool UnderStop; //是否正在强制停车
        public int LastInCollectRound; //上一次进入物资收集点时间
        public Dot MazeEntryPos; //进入迷宫的位置
        public bool InMaze; //是否进入迷宫
        public bool HaveBall; //是否正在推球
        public bool FinishedMaze; //是否已经完成穿过迷宫任务
        public int FoulCnt; //惩罚次数
        public void Stop() { UnderStop = true; } //车辆强制停止
        public void Start() { UnderStop = false; } //从强制停止中恢复
        public void Out() //出界处理
        {
            Stop();
            Foul();
        }
        public void Foul() //犯规
        {
            FoulCnt++;
        }
        public void SaveCnt() //保存当前回合得分状态
        {
            LastBallGetCnt = BallGetCnt;
            LastBallOwnCnt = BallOwnCnt;
            LastPersonCnt = PersonCnt;
            LastScore = Score;
        }
        public void LoadCnt() //恢复上一回合得分状态
        {
            BallGetCnt = LastBallGetCnt;
            BallOwnCnt = LastBallOwnCnt;
            PersonCnt = LastPersonCnt;
            Score = LastScore;
        }
        public Car(Camp c)
        {
            Who = c;
            Pos = new Dot();
            Score = 0;
            PersonCnt = 0;
            BallGetCnt = 0;
            BallOwnCnt = 0;
            LastScore = 0;
            LastPersonCnt = 0;
            LastBallGetCnt = 0;
            LastBallOwnCnt = 0;
            UnderStop = false;
            HaveBonus = false;
            HaveBall = false;
            FinishedMaze = false;
            MazeEntryPos = new Dot(0, 0);
            InMaze = false;
            FoulCnt = 0;
            LastInCollectRound = -Game.MinGetBallRound;
        }
    }
}
