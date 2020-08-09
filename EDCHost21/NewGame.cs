using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace EDC21HOST
{
    public enum GameState { Unstart = 0, Normal = 1, Pause = 2, End = 3 };
    public class Game
    {
        public bool DebugMode; //调试模式，最大回合数 = 1,000,000
        public const int MaxSize = 280;
        public const int MazeCrossNum = 6;
        public const int MazeCrossDist = 30;//间隔的长度
        public const int MazeBorderPoint1 = 35;//迷宫最短的靠边距离
        public const int MazeBorderPoint2 = MazeBorderPoint1 + MazeCrossDist * MazeCrossNum;//迷宫最长的靠边距离
        public const int MaxCarryDistance = 10; //判定是否到达的最大距离
        public const int MaxPackageNum = 6;

        public int APauseNum = 0;//A暂停次数
        public int BPauseNum = 0;//B暂停次数
        public int GameCount; //上下半场等
        Camp GameCamp; //当前半场需完成“上半场”任务的一方
        public GameState State { get; set; }
        public Car CarA, CarB;
        public Passenger Passenger;
        public PassengerGenerator Generator1 { get; set; }
        public PackageGenerator[] Generator2;
        public Package[] Package;
        public Obstacle Obstacle;
        //public static bool[,] GameMap = new bool[MaxSize, MaxSize]; //地图信息
        public FileStream FoulTimeFS;
        public static bool InMaze(Dot dot)//确定点是否在迷宫内
        {
            if (dot.x>=MazeBorderPoint1 && dot.x<=MazeBorderPoint2 && dot.y>=MazeBorderPoint1 && dot.y<=MazeBorderPoint2)
                return true;
            else return false;
        }

        //public static void LoadMap()//读取地图文件
        //{
        //    //FileStream MapFile = File.OpenRead("../../map/map.bmp");
        //    //byte[] buffer = new byte[MapFile.Length - 54]; //存储图片文件
        //    //MapFile.Position = 54;
        //    //MapFile.Read(buffer, 0, buffer.Length);
        //    //for (int i = 0; i != MaxSize; ++i)
        //    //    for (int j = 0; j != MaxSize; ++j)
        //    //        if (buffer[(i * MaxSize + j) * 3 + 2 * i] > 128)//白色
        //    //            GameMap[j, i] = true;
        //    //        else
        //    //            GameMap[j, i] = false;

        //    Bitmap mapData = new Bitmap("../../map/map.bmp");
        //    for (int i = 0; i != MaxSize; ++i)
        //        for (int j = 0; j != MaxSize; ++j)
        //            GameMap[j, i] = mapData.GetPixel(j, i).Equals(Color.FromArgb(255, 255, 255));

        //    //using (StreamWriter sw = new StreamWriter("../../map/map.txt"))
        //    //{
        //    //    for (int i = 0; i != MaxSize; ++i)
        //    //    {
        //    //        for (int j = 0; j != MaxSize; ++j)
        //    //            sw.Write((GameMap[j, i] = mapData.GetPixel(j, i).Equals(Color.FromArgb(255, 255, 255))) ? '1' : '0');
        //    //        sw.Write('\n');
        //    //    }
        //    //}
        //}
        public static Dot OppoDots(Dot prevDot)//复制点
        {
            Dot newDots = new Dot();
            newDots.x = prevDot.y;
            newDots.y = prevDot.x;
            return newDots;
        }
        public static double GetDistance(Dot A, Dot B)//得到两个点之间的距离
        {
            return Math.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y));
        }
        public Game()
        {
            GameCount = 1;
            GameCamp = Camp.CampA;
            CarA = new Car(Camp.CampA);
            CarB = new Car(Camp.CampB);
            State = GameState.Unstart;
            Generator1 = new PassengerGenerator(100);
            Generator2[0] = new PackageGenerator(6);
            Generator2[1] = new PackageGenerator(6);
            Generator2[2] = new PackageGenerator(6);
            Generator2[3] = new PackageGenerator(6);
            DebugMode = false;
            FoulTimeFS = null;
        }

        public void NextStage()
        {
            ++GameCount;
            if (GameCount % 2 == 1)
                GameCamp = Camp.CampA;
            else
                GameCamp = Camp.CampB;
            if (GameCount >= 3)
                MaxRound = 600;
            else
                MaxRound = 1200;
            Round = 0;
            State = GameState.Unstart;
            CarA.LastInCollectRound = -MinGetBallRound;
            CarB.LastInCollectRound = -MinGetBallRound;
            Generator.ResetIndex();
            InitialPerson();
            DebugMode = false;
            if (FoulTimeFS != null)
            {
                byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                FoulTimeFS.Write(data, 0, data.Length);
            }
            CarA.SaveCnt();
            CarB.SaveCnt();
        }

        protected void InitialPackage() //初始化物资                                       //完成
        {
            for (int i = 0; i < MaxPackageNum;++i)
                Package[i] = new package();
            for (int i = 0; i < MaxPackageNum; ++i)
                Package[i] = new Package(Generator.Next(People), i);
        }
        public void NewPackage(int num) //刷新这一位置的新物资
        {
            Dot temp = new Dot();
            do
            {
                temp = Generator.Next(People);
            }
            while (temp == Package[0]||temp==Package[1]||temp==Package[2]||temp==Package[3]||temp==Package[4]||temp==Package[5]); //防止与其他相同位置
            People[num] = new Person(temp, num);
        }

        public void Start() //开始比赛
        {
            State = GameState.Normal;
            CarA.LastInCollectRound = -MinGetBallRound;
            CarB.LastInCollectRound = -MinGetBallRound;
        }
        public void Pause() //暂停比赛
        {
            State = GameState.Pause;
            CarA.LastInCollectRound = -MinGetBallRound;
            CarB.LastInCollectRound = -MinGetBallRound;
            //CarA.HaveBall = false;
            //CarB.HaveBall = false;
            CarA.Stop();
            CarB.Stop();
        }
        public void End() //结束比赛
        {
            State = GameState.End;
            //结算当前半场分数
            int currBallCntA = 0;
            foreach (Dot ball in BallsDot)
                if (InStorageA(ball)) currBallCntA++;
            switch (GameCamp)
            {
                case Camp.CampA:
                    //小车成功回家计分
                    if (InStorageA(CarA.Pos))
                        AddScore(Camp.CampA, Score.GetBackScore);
                    if (InStorageB(CarB.Pos))
                        AddScore(Camp.CampB, Score.GetBackScore);
                    //小球运输至存放点计分
                    if (currBallCntA > 0 && CarA.HaveBall)
                    {
                        AddScore(Camp.CampA, Score.BallStoreScore);
                        CarA.BallOwnCnt++;
                        CarA.HaveBall = false;
                    }
                    break;
                case Camp.CampB:
                    //小车成功回家计分
                    if (InStorageB(CarA.Pos))
                        AddScore(Camp.CampA, Score.GetBackScore);
                    if (InStorageA(CarB.Pos))
                        AddScore(Camp.CampB, Score.GetBackScore);
                    //小球运输至存放点计分
                    if (currBallCntA > 0 && CarB.HaveBall)
                    {
                        AddScore(Camp.CampB, Score.BallStoreScore);
                        CarB.BallOwnCnt++;
                        CarB.HaveBall = false;
                    }
                    break;
                default: break;
            }
        }
        //复位
        public void AskPause(Camp c)
        {
            Pause();
            CarA.HaveBall = false;
            CarB.HaveBall = false;
            CarA.LoadCnt();
            CarB.LoadCnt();
            Round = 0;
            switch (c)
            {
                case Camp.CampA:
                    ++APauseNum;
                    AddScore(Camp.CampA, Score.PauseScore);
                    break;
                case Camp.CampB:
                    ++BPauseNum;
                    AddScore(Camp.CampB, Score.PauseScore);
                    break;
            }
        }

        //更新小球相关操作的状态、得分
        public void UpdateBallsState()
        {
            NoBall = true;
            bool noBallInCollect = true;
            Camp currCollectCamp = Camp.None;
            //int currBallCntA = 0;
            //BallAtCollect = new Dot(0, 0);
            foreach (Dot ball in BallsDot)
            {
                if (InCollect(ball))
                {
                    BallAtCollect = ball;
                    noBallInCollect = false;
                }
                //else if (InStorageA(ball)) currBallCntA++;
                //else if (InStorageB(ball)) currBallCntB++;

                if (!InStorageA(ball))
                {
                    BallDot = ball;
                    NoBall = false;
                }
            }

            //更新CollectCamp：物资收集点处是A车还是B车
            switch (GameCamp)
            {
                case Camp.CampA: if (InCollect(CarA.Pos)) currCollectCamp = Camp.CampA; break;
                case Camp.CampB: if (InCollect(CarB.Pos)) currCollectCamp = Camp.CampB; break;
                default: currCollectCamp = Camp.None;  break;
            }

            //进入物资收集点计分
            if (CollectCamp == Camp.None && currCollectCamp != Camp.None)
            {
                switch (currCollectCamp)
                {
                    case Camp.CampA:
                        if (Round - CarA.LastInCollectRound > MinGetBallRound)
                        { //防止重复计分
                            CarA.LastInCollectRound = Round;
                            AddScore(currCollectCamp, Score.BallCollectScore); //小车成功到达物资收集点
                        }
                        break;
                    case Camp.CampB:
                        if (Round - CarB.LastInCollectRound > MinGetBallRound)
                        {
                            CarB.LastInCollectRound = Round;
                            AddScore(currCollectCamp, Score.BallCollectScore); //小车成功到达物资收集点
                        }
                        break;
                    default: break;
                }
            }

            RequestNewBall = noBallInCollect && !InCollect(CarA.Pos) && !InCollect(CarB.Pos); //物资收集点处没有车和球时才可请求新球
            
            //抓取到小球计分
            if (RequestNewBall) 
            {
                switch (CollectCamp)
                {
                    case Camp.CampA:
                        if (!CarA.HaveBall)
                        {
                            AddScore(Camp.CampA, Score.BallGetScore);
                            CarA.BallGetCnt++;
                            CarA.HaveBall = true;
                        }
                        break;
                    case Camp.CampB:
                        if (!CarB.HaveBall)
                        {
                            AddScore(Camp.CampB, Score.BallGetScore);
                            CarB.BallGetCnt++;
                            CarB.HaveBall = true;
                        }
                        break;
                    default: break; 
                }
                CollectCamp = Camp.None;
            }

            CollectCamp = currCollectCamp;

            ////小球运输至存放点计分
            //if (currBallCntA == BallCntA + 1)
            //{
            //    if (GameCamp == Camp.CampA && CarA.HaveBall)
            //    {
            //        AddScore(Camp.CampA, Score.BallStoreScore);
            //        CarA.BallOwnCnt++;
            //        CarA.HaveBall = false;
            //    }
            //    if (GameCamp == Camp.CampB && CarB.HaveBall)
            //    {
            //        AddScore(Camp.CampB, Score.BallStoreScore);
            //        CarB.BallOwnCnt++;
            //        CarB.HaveBall = false;
            //    }
            //}
            //BallCntA = currBallCntA;
        }

        //更新小车走出迷宫得分
        public void UpdateMazeState()
        {
            switch (GameCamp)
            {
                case Camp.CampA:
                    bool currCarAInMaze = InMaze(CarA.Pos);
                    if (currCarAInMaze && !CarA.InMaze)
                    {
                        CarA.MazeEntryPos = CarA.Pos;
                    }
                    else if (!currCarAInMaze && CarA.InMaze && !CarA.FinishedMaze)
                    {
                        if (GetDistance(CarA.MazeEntryPos, CarA.Pos) > MinMazeEntryDistance)
                        {
                            AddScore(Camp.CampA, Score.GoOutOfMaze);
                            CarA.FinishedMaze = true;
                        }
                    }

                    CarA.InMaze = InMaze(CarA.Pos);
                    break;
                case Camp.CampB:
                    bool currCarBInMaze = InMaze(CarB.Pos);
                    if (currCarBInMaze && !CarB.InMaze)
                    {
                        CarB.MazeEntryPos = CarB.Pos;
                    }
                    else if (!currCarBInMaze && CarB.InMaze && !CarB.FinishedMaze)
                    {
                        if (GetDistance(CarB.MazeEntryPos, CarB.Pos) > MinMazeEntryDistance)
                        {
                            AddScore(Camp.CampB, Score.GoOutOfMaze);
                            CarB.FinishedMaze = true;
                        }
                    }

                    CarB.InMaze = InMaze(CarB.Pos);
                    break;
                default: break;
            }
        }

        //更新人员上车
        public void UpdatePerson()
        {
            for (int i = 0; i != CurrPersonNumber; ++i)
            {
                Person p = People[i];
                if (CarA.UnderStop == false && GetDistance(p.StartPos, CarA.Pos) < MaxCarryDistance)
                {
                    AddScore(Camp.CampA, Score.PersonGetScore);
                    CarA.PersonCnt++;
                    NewPerson(p.StartPos, i);
                }

                if (CarB.UnderStop == false && GetDistance(p.StartPos, CarB.Pos) < MaxCarryDistance)
                {
                    AddScore(Camp.CampB, Score.PersonGetScore);
                    CarB.PersonCnt++;
                    NewPerson(p.StartPos, i);
                }
            }
        }

        public void Update()//每回合执行
        {
            if (State == GameState.Normal)
            {
                Round++;
                //GetInfoFromCameraAndUpdate();
                CheckPersonNumber();
                UpdateBallsState();
                UpdateMazeState();
                UpdatePerson();
                #region PunishmentPhase
                //if (!CarDotValid(CarA.Pos)) CarA.Stop();
                //if (!CarDotValid(CarB.Pos)) CarB.Stop();
                #endregion

                if ((Round >= MaxRound && DebugMode == false) || (Round >= 1000000 && DebugMode == true)) //结束比赛
                {
                    End();
                }
            }
        } 
        public byte[] PackMessage()
        {
            byte[] message = new byte[32]; //上位机传递的信息
            int messageCnt = 0;
            message[messageCnt++] = (byte)(Round >> 8);
            message[messageCnt++] = (byte)Round;
            message[messageCnt++] = (byte)(((byte)State << 6) | ((byte)(InMaze(CarA.Pos) ? 1 : 0) << 5) | ((byte)(InMaze(CarB.Pos) ? 1 : 0) << 4)
                | (CarA.Pos.x >> 5 & 0x08) | (CarA.Pos.y >> 6 & 0x04) | (CarB.Pos.x >> 7 & 0x02) | (CarB.Pos.y >> 8 & 0x01));
            message[messageCnt++] = (byte)((People[0].StartPos.x >> 1 & 0x80) | (People[0].StartPos.y >> 2 & 0x40)
                | (People[1].StartPos.x >> 3 & 0x20) | (People[1].StartPos.y >> 4 & 0x10)
                | (BallDot.x >> 5 & 0x08) | (BallDot.y >> 6 & 0x04)
                | ((byte)(NoBall ? 0 : 1) << 1) | ((byte)(GameCamp == Camp.CampA ? 1 : 0)));
            message[messageCnt++] = !InMaze(CarA.Pos) ? (byte)CarA.Pos.x : (byte)0;
            message[messageCnt++] = !InMaze(CarA.Pos) ? (byte)CarA.Pos.y : (byte)0;
            message[messageCnt++] = !InMaze(CarB.Pos) ? (byte)CarB.Pos.x : (byte)0;
            message[messageCnt++] = !InMaze(CarB.Pos) ? (byte)CarB.Pos.y : (byte)0;
            for (int i = 0; i < MaxPersonNum; ++i)
            {
                message[messageCnt++] = (byte)People[i].StartPos.x;
                message[messageCnt++] = (byte)People[i].StartPos.y;
            }
            message[messageCnt++] = (byte)BallDot.x;
            message[messageCnt++] = (byte)BallDot.y;
            message[messageCnt++] = (byte)(CarA.Score >> 8);
            message[messageCnt++] = (byte)CarA.Score;
            message[messageCnt++] = (byte)(CarB.Score >> 8);
            message[messageCnt++] = (byte)CarB.Score;
            message[messageCnt++] = (byte)CarA.PersonCnt;
            message[messageCnt++] = (byte)CarB.PersonCnt;
            message[messageCnt++] = (byte)CarA.BallGetCnt;
            message[messageCnt++] = (byte)CarB.BallGetCnt;
            message[messageCnt++] = (byte)CarA.BallOwnCnt;
            message[messageCnt++] = (byte)CarB.BallOwnCnt;
            ushort crc = Crc16(message, 28);
            message[28] = (byte)(crc >> 8);
            message[29] = (byte)crc;
            message[30] = 0x0D;
            message[31] = 0x0A;
            return message;
        }
        ushort Crc16(byte[] data_p, byte length)
        {
            byte i, j;
            ushort crc = 0xffff;
            const ushort CRC_POLY = 0xa001; //0x8005反序

            for (i = 0; i < length; ++i)
            {
                crc ^= (ushort)(0xff & data_p[i]);
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ CRC_POLY);
                    else
                        crc >>= 1;
                }
            }
            // crc = (crc << 8) | (crc >> 8 & 0xff);
            return crc;
        }
    }
}