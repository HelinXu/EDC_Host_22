﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;

namespace EDCHOST21
{
    // 比赛状况：未开始、正常进行中、暂停、结束
    public enum GameState { UNSTART = 0, NORMAL = 1, PAUSE = 2, END = 3 };

    // 人员状况：被困、在小车上且还未到指定点、到达目标点
    public enum PassengerState { TRAPPED, INCAR, RESCUED };

    public enum GameStage { FIRST_1 = 0, FIRST_2, LATTER_1, LATTER_2 };
    public class Game
    {
        public bool DebugMode;                       //调试模式，最大回合数 = 1,000,000
        public const double MAX_SIZE_CM = 280;          //地图大小
        public const double MAZE_CROSS_NUM = 6;         //迷宫由几条线交叉而成
        public const double MAZE_CROSS_DIST_CM = 30;    //间隔的长度
        public const double MAZE_SHORT_BORDER_CM = 35;  //迷宫最短的靠边距离
        public const double MAZE_LONG_BORDER_CM = MAZE_SHORT_BORDER_CM
                                             + MAZE_CROSS_DIST_CM * MAZE_CROSS_NUM;
        //迷宫最长的靠边距离
        public const double COINCIDE_ERR_DIST_CM = 10;  //判定小车到达某点允许的最大误差距离
        public const int PKG_NUM_perGROUP = 6;       //场上每次刷新package物资的个数
        public GameStage gameStage;//比赛阶段
        public Camp UpperCamp; //当前半场需完成“上半场”任务的一方
        public GameState gameState;//比赛状态
        public Car CarA, CarB;//定义小车
        public Passenger currentPassenger;//当前被运载的乘客
        public Package[] currentPackageList;//当前场上的物资列表
        public PassengerGenerator psgGenerator;//仅用来生成乘客序列
        public PackageGenerator pkgGenerator; //仅用来生成物资序列
        public int mPackageGroupCount;//用于记录现在的Package是第几波
        public Flood mFlood;
        public Labyrinth mLabyrinth;
        public int mPrevTime;//时间均改为以毫秒为单位,记录上一次的时间，精确到秒，实时更新
        public int mGameTime;//时间均改为以毫秒为单位
        public int mLastWrongDirTime;
        public FileStream FoulTimeFS;

        public Game(GameStage gameStage = GameStage.FIRST_1)//构造一个新的Game类 默认为CampA是先上半场上一阶段进行
        {
            if (gameStage == GameStage.FIRST_1)
            {

                UpperCamp = Camp.CMP_A;
                CarA = new Car(Camp.CMP_A, 0);
                CarB = new Car(Camp.CMP_B, 1);
                gameState = GameState.UNSTART;
                psgGenerator = new PassengerGenerator(100);//上下半场将都用这一个索引
                pkgGenerator = new PackageGenerator(PKG_NUM_perGROUP * 4);
                currentPackageList = new Package[PKG_NUM_perGROUP];
                currentPassenger = new Passenger(new Dot(), new Dot()); //?
                mFlood = new Flood(0);
                mPackageGroupCount = 0;
                mLastWrongDirTime = -10;
            }
            else
            {
                //@TODO
                UpperCamp = Camp.CMP_A;
                CarA = new Car(Camp.CMP_A, 0);
                CarB = new Car(Camp.CMP_B, 1);
                gameState = GameState.UNSTART;
                psgGenerator = new PassengerGenerator(100);//上下半场将都用这一个索引
                pkgGenerator = new PackageGenerator(PKG_NUM_perGROUP * 4);
                currentPackageList = new Package[PKG_NUM_perGROUP];
                currentPassenger = new Passenger(new Dot(), new Dot()); //?
                mFlood = new Flood(0);
                mPackageGroupCount = 0;
                mLastWrongDirTime = -10;
            }
        }
        //每到半点自动更新Package信息函数,8.29已更新
        public void UpdatePackage()//更换Package函数,每次都更新，而只在半分钟的时候起作用
        {
            int changenum = mGameTime / 30000 + 1;
            if ((gameStage == GameStage.FIRST_2
                || gameStage == GameStage.LATTER_2)
                && mPackageGroupCount < changenum)
            {
                for (int i = 0; i < PKG_NUM_perGROUP; i++)
                {
                    currentPackageList[i].mPos
                        = pkgGenerator.
                        GetPackage(i + PKG_NUM_perGROUP * mPackageGroupCount).
                        GetDot();
                    currentPackageList[i].IsPicked = 0;
                }
                mPackageGroupCount++;
            }
        }

        //该方法用于返回系统现在的时间。开发者：xhl
        public int GetCurrentTime()
        {
            System.DateTime currentTime = new System.DateTime();
            return currentTime.Hour * 3600000 + currentTime.Minute * 60000 + currentTime.Second * 1000;
        }

        public static double GetDistance(Dot A, Dot B)//得到两个点之间的距离
        {
            return Math.Sqrt((A.x - B.x) * (A.x - B.x)
                + (A.y - B.y) * (A.y - B.y));
        }


        //这个函数可能放到dot里面更好
        public void JudgeAIsInMaze()//确定点是否在迷宫内
        {
            if (CarA.mPos.x >= MAZE_SHORT_BORDER_CM 
                && CarA.mPos.x <= MAZE_LONG_BORDER_CM 
                && CarA.mPos.y >= MAZE_SHORT_BORDER_CM 
                && CarA.mPos.y <= MAZE_LONG_BORDER_CM)
            {
                CarA.mIsInMaze = 1;
            }
            else
            {
                CarA.mIsInMaze = 0;
            }
        }
        public void JudgeBIsInMaze()//确定点是否在迷宫内
        {
            if (CarB.mPos.x >= MAZE_SHORT_BORDER_CM 
                && CarB.mPos.x <= MAZE_LONG_BORDER_CM 
                && CarB.mPos.y >= MAZE_SHORT_BORDER_CM 
                && CarB.mPos.y <= MAZE_LONG_BORDER_CM)
            {
                CarB.mIsInMaze = 1;
            }
            else
            {
                CarB.mIsInMaze = 0;
            }
        }

        //点击开始键时调用Start函数 
        //上半场上一阶段、上半场下一阶段、下半场上一阶段、
        //下半场下一阶段开始时需要这一函数都需要调用这一函数来开始
        //暂停不用这个函数开始
        public void Start() //开始比赛上下半场都用这个
        {
            if (gameState == GameState.UNSTART)
            {
                gameState = GameState.NORMAL;
                mGameTime = 0;
                mPrevTime = GetCurrentTime();
            }
        }

        //点击暂停比赛键时调用Pause函数
        public void Pause() //暂停比赛
        {
            gameState = GameState.PAUSE;
        }

        //半场交换函数自动调用依照时间控制
        public void CheckNextStage()//从上半场更换到下半场函数
        {
            //判断是否结束
            if (gameStage == GameStage.FIRST_1 
                || gameStage == GameStage.LATTER_1)
            {
                if (mGameTime >= 60000)
                {
                    gameState = GameState.UNSTART;
                    UpdatePassenger();
                }
            }
            else
            {
                if (mGameTime >= 120000)
                {
                    gameState = GameState.UNSTART;
                    if (gameStage == GameStage.FIRST_2)
                    {
                        UpperCamp = Camp.CMP_B;//上半场转换
                        psgGenerator.ResetIndex();//Passenger的索引复位
                        if (FoulTimeFS != null)                                            //这里没有搞懂是干什么的
                        {
                            byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                            FoulTimeFS.Write(data, 0, data.Length);
                        }
                        CarA.mTaskState = 1;//交换A和B的任务
                        CarB.mTaskState = 0;
                    }
                }
            }
        }
        
        //在暂停后需要摁下继续按钮来继续比赛
        public void Continue()
        {
            gameState = GameState.NORMAL;
            mPrevTime = GetCurrentTime();
        }
        //重置摁键对应的函数
        //@TODO
        public void Reset()
        {
            //Game = new Game();
        }
        //犯规键对应的函数
        public void AddFoul()
        {
            if (gameStage == GameStage.FIRST_2 || gameStage == GameStage.LATTER_1)
            {
                CarB.mFoulCount++;
            }
            else
            {
                CarA.mFoulCount++;
            }
        }

        //下面为更新乘客信息函数
        public void UpdatePassenger()//更新乘客信息
        {
            currentPassenger = psgGenerator.Next();
        }

        //下面四个为接口
        public void CarAGetPassenger()//小车A接到了乘客
        {
            if (GetDistance(CarA.mPos, currentPassenger.Start_Dot) 
                <= COINCIDE_ERR_DIST_CM 
                && CarA.mIsWithPassenger == 0)
            {
                CarA.SwitchPassengerState();
            }

        }
        public void CarBGetPassenger()//小车B接到了乘客
        {
            if (GetDistance(CarB.mPos, currentPassenger.Start_Dot) 
                <= COINCIDE_ERR_DIST_CM 
                && CarB.mIsWithPassenger == 0)
            {
                CarB.SwitchPassengerState();
            }
        }
        public void CarATransPassenger()//小车A成功运送了乘客
        {

            if (GetDistance(CarA.mPos, currentPassenger.End_Dot) 
                <= COINCIDE_ERR_DIST_CM 
                && CarA.mIsWithPassenger == 1)
            {
                CarA.SwitchPassengerState();
                CarA.AddRescueCount();
            }
            UpdatePassenger();
        }
        public void CarBTransPassenger()//小车A成功运送了乘客
        {
            if (GetDistance(CarB.mPos, currentPassenger.End_Dot) 
                <= COINCIDE_ERR_DIST_CM 
                && CarB.mIsWithPassenger == 1)
            {
                CarB.SwitchPassengerState();
                CarB.AddRescueCount();
            }
            UpdatePassenger();
        }

        //下面是两个关于包裹的接口
        public void CarAGetpackage()//小车A得到了包裹
        {

            for (int i = 0; i < PKG_NUM_perGROUP; i++)
            {
                if (GetDistance(CarA.mPos, currentPackageList[i].mPos)
                    <= COINCIDE_ERR_DIST_CM 
                    && currentPackageList[i].IsPicked == 0)
                {
                    CarA.AddPickPkgCount();
                    currentPackageList[i].IsPicked = 1;
                }
            }

        }
        public void CarBGetpackage()//小车B得到了包裹
        {
            for (int i = 0; i < PKG_NUM_perGROUP; i++)
            {
                if (GetDistance(CarB.mPos, currentPackageList[i].mPos) 
                    <= COINCIDE_ERR_DIST_CM
                    && currentPackageList[i].IsPicked == 0)
                {
                    CarB.AddPickPkgCount();
                    currentPackageList[i].IsPicked = 1;
                }

            }
        }

        public void CarAonObstacle()//小车A到达了障碍上              
        {
            for (int i = 0; i <= 15; i++)
            {
                if (mLabyrinth.mpWallList[i].w1.x == mLabyrinth.mpWallList[i].w2.x)//障碍的两个点的横坐标相同
                {
                    if (mLabyrinth.mpWallList[i].w1.y < mLabyrinth.mpWallList[i].w2.y)//障碍1在障碍2的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.x == CarA.mPos.x && CarA.mPos.y <= mLabyrinth.mpWallList[i].w2.y && mLabyrinth.mpWallList[i].w1.y <= CarA.mPos.y)
                        {
                            CarA.AddWallPunish();
                        }

                    }
                    if (mLabyrinth.mpWallList[i].w2.y < mLabyrinth.mpWallList[i].w1.y)//障碍2在障碍1的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.x == CarA.mPos.x && CarA.mPos.y <= mLabyrinth.mpWallList[i].w1.y && mLabyrinth.mpWallList[i].w2.y <= CarA.mPos.y)
                        {
                            CarA.AddWallPunish();
                        }

                    }
                }
                if (mLabyrinth.mpWallList[i].w1.y == mLabyrinth.mpWallList[i].w2.y)//障碍的两个点的纵坐标相同
                {
                    if (mLabyrinth.mpWallList[i].w1.x < mLabyrinth.mpWallList[i].w2.x)//障碍1在障碍2的左面
                    {
                        if (mLabyrinth.mpWallList[i].w1.y == CarA.mPos.y && CarA.mPos.x <= mLabyrinth.mpWallList[i].w2.x && mLabyrinth.mpWallList[i].w1.x <= CarA.mPos.x)
                        {
                            CarA.AddWallPunish();
                        }

                    }
                    if (mLabyrinth.mpWallList[i].w2.x < mLabyrinth.mpWallList[i].w1.x)//障碍2在障碍1的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.y == CarA.mPos.y && CarA.mPos.x <= mLabyrinth.mpWallList[i].w1.x && mLabyrinth.mpWallList[i].w2.x <= CarA.mPos.x)
                        {
                            CarA.AddWallPunish();
                        }

                    }
                }
            }
        }
        public void CarBonObstacle()//小车B到达了障碍上               
        {
            for (int i = 0; i <= 15; i++)
            {
                if (mLabyrinth.mpWallList[i].w1.x == mLabyrinth.mpWallList[i].w2.x)//障碍的两个点的横坐标相同
                {
                    if (mLabyrinth.mpWallList[i].w1.y < mLabyrinth.mpWallList[i].w2.y)//障碍1在障碍2的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.x == CarB.mPos.x && CarB.mPos.y <= mLabyrinth.mpWallList[i].w2.y && mLabyrinth.mpWallList[i].w1.y <= CarB.mPos.y)
                        {
                            CarB.AddWallPunish();
                        }

                    }
                    if (mLabyrinth.mpWallList[i].w2.y < mLabyrinth.mpWallList[i].w1.y)//障碍2在障碍1的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.x == CarB.mPos.x && CarB.mPos.y <= mLabyrinth.mpWallList[i].w1.y && mLabyrinth.mpWallList[i].w2.y <= CarB.mPos.y)
                        {
                            CarB.AddWallPunish();
                        }

                    }
                }
                if (mLabyrinth.mpWallList[i].w1.y == mLabyrinth.mpWallList[i].w2.y)//障碍的两个点的纵坐标相同
                {
                    if (mLabyrinth.mpWallList[i].w1.x < mLabyrinth.mpWallList[i].w2.x)//障碍1在障碍2的左面
                    {
                        if (mLabyrinth.mpWallList[i].w1.y == CarB.mPos.y && CarB.mPos.x <= mLabyrinth.mpWallList[i].w2.x && mLabyrinth.mpWallList[i].w1.x <= CarB.mPos.x)
                        {
                            CarB.AddWallPunish();
                        }

                    }
                    if (mLabyrinth.mpWallList[i].w2.x < mLabyrinth.mpWallList[i].w1.x)//障碍2在障碍1的下面
                    {
                        if (mLabyrinth.mpWallList[i].w1.y == CarB.mPos.y && CarB.mPos.x <= mLabyrinth.mpWallList[i].w1.x && mLabyrinth.mpWallList[i].w2.x <= CarB.mPos.x)
                        {
                            CarB.AddWallPunish();
                        }

                    }
                }
            }
        }
        public void CarAonFlood()//A车到大障碍上
        {

            if (CarA.mTaskState == 1)//在下半场的时候才应该判断小车是否经过Flood
            {
                if (mFlood.num == 0)
                { }
                else if (mFlood.num == 1)
                {
                    if (GetDistance(CarA.mPos, mFlood.dot1) <= COINCIDE_ERR_DIST_CM)
                    {

                        CarA.AddFloodPunish();
                    }
                }
                else if (mFlood.num == 2)
                {

                    if (GetDistance(CarA.mPos, mFlood.dot1) <= COINCIDE_ERR_DIST_CM)
                    {
                        CarA.AddFloodPunish();
                    }
                    if (GetDistance(CarA.mPos, mFlood.dot2) <= COINCIDE_ERR_DIST_CM)
                    {
                        CarA.AddFloodPunish();
                    }
                }

            }
        }
        public void CarBonFlood()
        {
            if (CarB.mTaskState == 1)//在下半场的时候才应该判断小车是否经过Flood
            {
                if (mFlood.num == 0)
                { }
                else if (mFlood.num == 1)
                {
                    if (GetDistance(CarB.mPos, mFlood.dot1) <= COINCIDE_ERR_DIST_CM)
                    {

                        CarB.AddFloodPunish();
                    }
                }
                else if (mFlood.num == 2)
                {

                    if (GetDistance(CarB.mPos, mFlood.dot1) <= COINCIDE_ERR_DIST_CM)
                    {
                        CarB.AddFloodPunish();
                    }
                    if (GetDistance(CarB.mPos, mFlood.dot2) <= COINCIDE_ERR_DIST_CM)
                    {
                        CarB.AddFloodPunish();
                    }
                }

            }
        }
        //逆行自动判断//目前为思路两次逆行之间间隔时间判断为5s，这5s之间的逆行忽略不计
        public void CarAWrongDirection()
        {
            if (CarA.mLastPos.x < 30 && CarA.mPos.x < 30 && CarA.mLastPos.y > 30 && CarA.mLastPos.y < 220 && CarA.mPos.y > 30 && CarA.mPos.y < 220 && CarA.mPos.y > CarA.mLastPos.y && mGameTime - mLastWrongDirTime > 50)
            {
                CarA.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarA.mLastPos.x > 220 && CarA.mPos.x < 220 && CarA.mLastPos.y > 30 && CarA.mLastPos.y < 220 && CarA.mPos.y > 30 && CarA.mPos.y < 220 && CarA.mPos.y < CarA.mLastPos.y && mGameTime - mLastWrongDirTime > 50)
            {
                CarA.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarA.mLastPos.y < 30 && CarA.mPos.y < 30 && CarA.mLastPos.x > 30 && CarA.mLastPos.x < 220 && CarA.mPos.x > 30 && CarA.mPos.x < 220 && CarA.mPos.x < CarA.mLastPos.x && mGameTime - mLastWrongDirTime > 50)
            {
                CarA.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarA.mLastPos.y > 220 && CarA.mPos.y > 220 && CarA.mLastPos.x > 30 && CarA.mLastPos.x < 220 && CarA.mPos.x > 30 && CarA.mPos.x < 220 && CarA.mPos.x > CarA.mLastPos.x && mGameTime - mLastWrongDirTime > 50)
            {
                CarA.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
        }
        public void CarBWrongDirection()
        {
            if (CarB.mLastPos.x < 30 && CarB.mPos.x < 30 && CarB.mLastPos.y > 30 && CarB.mLastPos.y < 220 && CarB.mPos.y > 30 && CarB.mPos.y < 220 && CarB.mPos.y > CarB.mLastPos.y && mGameTime - mLastWrongDirTime > 50)
            {
                CarB.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarB.mLastPos.x > 220 && CarB.mPos.x < 220 && CarB.mLastPos.y > 30 && CarB.mLastPos.y < 220 && CarB.mPos.y > 30 && CarB.mPos.y < 220 && CarB.mPos.y < CarB.mLastPos.y && mGameTime - mLastWrongDirTime > 50)
            {
                CarB.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarB.mLastPos.y < 30 && CarB.mPos.y < 30 && CarB.mLastPos.x > 30 && CarB.mLastPos.x < 220 && CarB.mPos.x > 30 && CarB.mPos.x < 220 && CarB.mPos.x < CarB.mLastPos.x && mGameTime - mLastWrongDirTime > 50)
            {
                CarB.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
            if (CarB.mLastPos.y > 220 && CarB.mPos.y > 220 && CarB.mLastPos.x > 30 && CarB.mLastPos.x < 220 && CarB.mPos.x > 30 && CarB.mPos.x < 220 && CarB.mPos.x > CarB.mLastPos.x && mGameTime - mLastWrongDirTime > 50)
            {
                CarB.AddFoulCount();
                mLastWrongDirTime = mGameTime;
            }
        }
        public void SetFlood()
        {
            if (gameStage == GameStage.FIRST_1)
            {
                for (int i = 1; i <= 6; i++)
                {

                    for (int j = 1; j <= 6; j++)
                    {
                        if (GetDistance(CarA.mLastOneSecondPos,
                            new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15)) < COINCIDE_ERR_DIST_CM &&
                            GetDistance(CarA.mPos,
                            new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15)) < COINCIDE_ERR_DIST_CM)
                        {
                            if (mFlood.num == 0)
                            {
                                mFlood.dot1 = new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15);
                                mFlood.num = 1;
                            }
                            if (mFlood.num == 1)
                            {
                                mFlood.dot2 = new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                                                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15);
                                mFlood.num = 2;
                            }
                        }
                       

                    }
                }

            }
            if (gameStage == GameStage.LATTER_1)
            {
                for (int i = 1; i <= 6; i++)
                {

                    for (int j = 1; j <= 6; j++)
                    {
                        if (GetDistance(CarB.mLastOneSecondPos,
                            new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15)) < COINCIDE_ERR_DIST_CM &&
                            GetDistance(CarB.mPos,
                            new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15)) < COINCIDE_ERR_DIST_CM)
                        {
                            if (mFlood.num == 0)
                            {
                                mFlood.dot1 = new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15);
                                mFlood.num = 1;
                            }
                            if (mFlood.num == 1)
                            {
                                mFlood.dot2 = new Dot(MAZE_SHORT_BORDER_CM + i * MAZE_CROSS_DIST_CM - 15,
                                                            MAZE_CROSS_DIST_CM + j * MAZE_CROSS_DIST_CM - 15);
                                mFlood.num = 2;
                            }
                        }
                        
                    }
                }

            }
        }

        public void UpdateGameTime()
        {
            if (gameState == GameState.NORMAL)
            {
                mGameTime = GetCurrentTime() - mPrevTime + mGameTime;
            }
            mPrevTime = GetCurrentTime();
        }
        //0.1s
        public void Update()
        {
            if (gameState == GameState.NORMAL)
            {
                UpdateGameTime();
                UpdatePackage();
                CheckNextStage();
                if (gameStage == GameStage.FIRST_1 || gameStage == GameStage.LATTER_2)
                {
                    JudgeAIsInMaze();
                    CarAGetpackage();
                    CarAGetPassenger();
                    CarAonFlood();
                    CarAonObstacle();
                    CarATransPassenger();
                    CarAWrongDirection();
                }
                else
                {
                    JudgeBIsInMaze();
                    CarBGetpackage();
                    CarBGetPassenger();
                    CarBonFlood();
                    CarBonObstacle();
                    CarBTransPassenger();
                    CarBWrongDirection();
                }
            }
        }

        public void UpdateCarLastOneSecondPos()
        {
            if (gameState == GameState.NORMAL)
            {
                CarA.mLastOneSecondPos = CarA.mPos;
                CarB.mLastOneSecondPos = CarB.mPos;
            }
        }
        public byte[] PackCarAMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[102]; //上位机传递多少信息
            int messageCnt = 0;
            message[messageCnt++] = (byte)(mGameTime >> 8);
            message[messageCnt++] = (byte)mGameTime;
            message[messageCnt++] = (byte)(((byte)gameState << 6) | ((byte)CarA.mTaskState << 5 | ((byte)CarA.mIsWithPassenger << 3 & 0x08) | ((byte)mFlood.num & 0x03)));
            message[messageCnt++] = (byte)CarA.mPos.x;
            message[messageCnt++] = (byte)CarA.mPos.y;
            message[messageCnt++] = (byte)mFlood.dot1.x;
            message[messageCnt++] = (byte)mFlood.dot1.y;
            message[messageCnt++] = (byte)mFlood.dot2.x;
            message[messageCnt++] = (byte)mFlood.dot2.y;
            message[messageCnt++] = (byte)currentPassenger.Start_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.Start_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.End_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.End_Dot.x;
            message[messageCnt++] = (byte)(((byte)currentPackageList[0].IsPicked << 7) | ((byte)currentPackageList[1].IsPicked << 6) | ((byte)currentPackageList[2].IsPicked << 5)
                | ((byte)currentPackageList[3].IsPicked << 4) | ((byte)currentPackageList[4].IsPicked << 3) | ((byte)currentPackageList[5].IsPicked << 2) | ((byte)CarA.mIsInMaze << 1));
            message[messageCnt++] = (byte)currentPackageList[0].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[0].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[1].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[1].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[2].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[2].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[3].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[3].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[4].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[4].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[5].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[5].mPos.y;
            message[messageCnt++] = (byte)(CarA.MyScore >> 8);
            message[messageCnt++] = (byte)CarA.MyScore;
            message[messageCnt++] = (byte)CarA.mRescueCount;
            message[messageCnt++] = (byte)CarA.mPkgCount;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w2.y;
            message[messageCnt++] = 0x0D;
            message[messageCnt++] = 0x0A;
            return message;
        }
        public byte[] PackCarBMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[102]; //上位机传递多少信息
            int messageCnt = 0;
            message[messageCnt++] = (byte)(mGameTime >> 8);
            message[messageCnt++] = (byte)mGameTime;
            message[messageCnt++] = (byte)(((byte)gameState << 6) | ((byte)CarB.mTaskState << 5) | ((byte)CarB.mIsWithPassenger << 3 & 0x08) | ((byte)mFlood.num & 0x03));
            message[messageCnt++] = (byte)CarB.mPos.x;
            message[messageCnt++] = (byte)CarB.mPos.y;
            message[messageCnt++] = (byte)mFlood.dot1.x;
            message[messageCnt++] = (byte)mFlood.dot1.y;
            message[messageCnt++] = (byte)mFlood.dot2.x;
            message[messageCnt++] = (byte)mFlood.dot2.y;
            message[messageCnt++] = (byte)currentPassenger.Start_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.Start_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.End_Dot.x;
            message[messageCnt++] = (byte)currentPassenger.End_Dot.x;
            message[messageCnt++] = (byte)(((byte)currentPackageList[0].IsPicked << 7) | ((byte)currentPackageList[1].IsPicked << 6) | ((byte)currentPackageList[2].IsPicked << 5) | ((byte)currentPackageList[3].IsPicked << 4) | ((byte)currentPackageList[4].IsPicked << 3) | ((byte)currentPackageList[5].IsPicked << 2) | ((byte)CarB.mIsInMaze << 1));
            message[messageCnt++] = (byte)currentPackageList[0].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[0].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[1].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[1].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[2].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[2].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[3].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[3].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[4].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[4].mPos.y;
            message[messageCnt++] = (byte)currentPackageList[5].mPos.x;
            message[messageCnt++] = (byte)currentPackageList[5].mPos.y;
            message[messageCnt++] = (byte)(CarB.MyScore >> 8);
            message[messageCnt++] = (byte)CarB.MyScore;
            message[messageCnt++] = (byte)CarB.mRescueCount;
            message[messageCnt++] = (byte)CarB.mPkgCount;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[0].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[1].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[2].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[3].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[4].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[5].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[6].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[7].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[8].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[9].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[10].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[11].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[12].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[13].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[14].w2.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w1.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w1.y;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w2.x;
            message[messageCnt++] = (byte)mLabyrinth.mpWallList[15].w2.y;
            message[messageCnt++] = 0x0D;
            message[messageCnt++] = 0x0A;
            return message;
        }
    }
}