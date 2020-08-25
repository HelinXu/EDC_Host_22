using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;


namespace EDCHOST21
{
    public enum GameState { UNTART = 0, NORMAL = 1, PAUSE = 2, END = 3 };

    public class Game
    {
        public bool DebugMode; //调试模式，最大回合数 = 1,000,000
        public const int MAX_SIZE_CM = 280;
        public const int MAZE_CROSS_NUM = 6; //迷宫由几条线交叉而成
        public const int MAZE_CROSS_DIST_CM = 30; //间隔的长度
        public const int MAZE_SHORT_BORDER_CM = 35; //迷宫最短的靠边距离  xhl?
        public const int MAZE_LONG_BORDER_CM = MAZE_SHORT_BORDER_CM + MAZE_CROSS_DIST_CM * MAZE_CROSS_NUM;//迷宫最长的靠边距离 xhl?
        public const int COINCIDE_ERR_DIST_CM = 10; //判定小车到达某点允许的最大误差距离
        public const int PKG_NUM_perGROUP = 6; //场上每次刷新package物资的个数

        public int mGameCount; //上下半场 1是上半场 2是下半场
        public int mGameStage; //上下阶段 1是上阶段 2是下阶段
        public Camp UpperCamp; //当前半场需完成“上半场”任务的一方
        public GameState State { get; set; }//比赛状态
        public Car CarA, CarB;//定义小车
        public Passenger Passenger;
        public PassengerGenerator PsgGenerator { get; set; } //?
        public PackageGenerator[] PkgGenerator; //?
        public Package[] PkgList; //?
        public Flood mFlood;
        public Labyrinth mLabyrinth;
        public int mStartTime;//时间均改为以毫秒为单位
        public int mGameTime;//时间均改为以毫秒为单位
        public int mPackageGroupCount;//用于记录现在的Package是第几波
        public int mLastWrongDirTime;
        //public static bool[,] GameMap = new bool[MaxSize, MaxSize]; //地图信息
        public FileStream FoulTimeFS;

        //该方法用于返回系统现在的时间。开发者：xhl
        public System.DateTime GetCurrentTime()
        {
            System.DateTime currentTime = new System.DateTime();
            return currentTime;
        }
        //这个函数可能放到dot里面更好
        public static bool JudgeIsInMaze(Dot dot)//确定点是否在迷宫内
        {
            if (dot.x >= MAZE_SHORT_BORDER_CM && dot.x <= MAZE_LONG_BORDER_CM && dot.y >= MAZE_SHORT_BORDER_CM && dot.y <= MAZE_LONG_BORDER_CM)
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

        //总感觉这个放在这里不妥
        public static double GetDistance(Dot A, Dot B)//得到两个点之间的距离
        {
            return Math.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y));
        }
        public Game()//构造一个新的Game类 默认为CampA是先上半场上一阶段进行
        {
            mGameCount = 1;
            mGameStage = 1;
            UpperCamp = Camp.CMP_A;
            CarA = new Car(Camp.CMP_A, 0);
            CarB = new Car(Camp.CMP_B, 1);
            State = GameState.UNTART;
            PsgGenerator = new PassengerGenerator(100);//上下半场将都用这一个索引
            PkgGenerator[0] = new PackageGenerator(6);
            PkgGenerator[1] = new PackageGenerator(6);
            PkgGenerator[2] = new PackageGenerator(6);
            PkgGenerator[3] = new PackageGenerator(6);
            mFlood = new Flood(0);
            mPackageGroupCount = 0;
            mLastWrongDirTime = -10;
        }
        //点击开始键时调用Start函数 上半场上一阶段、上半场下一阶段、下半场上一阶段、下半场下一阶段开始时需要这一函数都需要调用这一函数来开始
        public void Start() //开始比赛上下半场都用这个
        {
            State = GameState.NORMAL;
            mGameTime = 0;
            mStartTime = GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second * 1000;//记录比赛开始时候的时间
        }
        //点击暂停比赛键时调用Pause函数
        public void Pause() //暂停比赛
        {
            State = GameState.PAUSE;
            mGameTime = mGameTime + GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second * 1000 - mStartTime;//记录现在比赛已经进行了多少时间了
        }
        //半场交换函数自动调用依照时间控制
        public void NextCount()//从上半场更换到下半场函数
        {
            if (mGameCount == 1 && mGameStage == 2 && mGameTime == 120000)
            {
                State = GameState.PAUSE;
                UpperCamp = Camp.CMP_B;//上半场转换
                PsgGenerator.ResetIndex();//Passenger的索引复位
                if (FoulTimeFS != null)                                            //这里没有搞懂是干什么的
                {
                    byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                    FoulTimeFS.Write(data, 0, data.Length);
                }
                CarA.mTaskState = 1;//交换A和B的任务
                CarB.mTaskState = 0;
            }

        }
        //半阶段交换函数自动调用依照时间控制
        public void NextStage()
        {
            if (mGameStage == 1 && mGameTime == 60000)
            {
                State = GameState.PAUSE;
                UpdatePassenger();
            }
        }
        //在暂停后需要摁下继续按钮来继续比赛
        public void Continue()
        {
            State = GameState.NORMAL;
            mStartTime = GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second * 1000;
        }
        //重置摁键对应的函数
        public void Reset()
        {
            //Game = new Game();
        }
        //犯规键对应的函数
        public void Foul()
        {
            if (mGameCount == 1 && mGameStage == 1)
            {
                CarA.mFoulCount++;
            }
            if (mGameCount == 1 && mGameStage == 2)
            {
                CarB.mFoulCount++;
            }
            if (mGameCount == 2 && mGameStage == 1)
            {
                CarB.mFoulCount++;
            }
            if (mGameCount == 2 && mGameStage == 2)
            {
                CarA.mFoulCount++;
            }
        }
        //每到半点自动更新Package信息函数
        public void UpdatePackage()//到半点时更换Package函数
        {
            int changenum = mGameTime / 30000 + 1;
            if (mGameStage == 2 && mPackageGroupCount < changenum)
            {
                for (int i = 0; i < PKG_NUM_perGROUP; i++)
                {
                    PkgList[i].mPos = PkgGenerator[changenum].GetPackage(i).GetDot();
                    PkgList[i].IsPicked = 0;
                }
                mPackageGroupCount++;
            }
        }
        //下面为自动更新乘客信息函数
        public void UpdatePassenger()//更新乘客信息
        {
            Passenger = PsgGenerator.Next();
        }
        //下面四个为接口
        public void CarAGetPassenger()//小车A接到了乘客
        {
            if (GetDistance(CarA.mPos, Passenger.Start_Dot) <= COINCIDE_ERR_DIST_CM && CarA.mIsWithPassenger == 0)
            {
                CarA.SwitchPassengerState();
            }

        }
        public void CarBGetPassenger()//小车B接到了乘客
        {
            if (GetDistance(CarB.mPos, Passenger.Start_Dot) <= COINCIDE_ERR_DIST_CM && CarA.mIsWithPassenger == 0)
            {
                CarB.SwitchPassengerState();
            }
        }
        public void CarATransPassenger()//小车A成功运送了乘客
        {

            if (GetDistance(CarA.mPos, Passenger.End_Dot) <= COINCIDE_ERR_DIST_CM && CarA.mIsWithPassenger == 1)
            {
                CarA.SwitchPassengerState();
                CarA.AddRescueCount();
            }
            UpdatePassenger();
        }
        public void CarBTransPassenger()//小车A成功运送了乘客
        {
            if (GetDistance(CarB.mPos, Passenger.End_Dot) <= COINCIDE_ERR_DIST_CM && CarB.mIsWithPassenger == 1)
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
                if (GetDistance(CarA.mPos, PkgList[i].mPos) <= COINCIDE_ERR_DIST_CM && PkgList[i].IsPicked == 0)
                {
                    CarA.AddPickPkgCount();
                    PkgList[i].IsPicked = 1;
                }
            }

        }
        public void CarBGetpackage()//小车B得到了包裹
        {
            for (int i = 0; i < PKG_NUM_perGROUP; i++)
            {
                if (GetDistance(CarB.mPos, PkgList[i].mPos) <= COINCIDE_ERR_DIST_CM && PkgList[i].IsPicked == 0)
                {
                    CarB.AddPickPkgCount();
                    PkgList[i].IsPicked = 1;
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
        /*public void SetStop(Dot stop)//上半场的小车设定障碍        //这里也需要加判断！！！！！！！！！！！！！！！！！！！！
        {
            if(Stop.num==0)
            {
                Stop.dot1 = stop;
                Stop.num++;
            }
            if(Stop.num==1)
            {
                Stop.dot2 = stop;
                Stop.num++;
            }
        }*/
        public byte[] PackMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[102]; //上位机传递多少信息
            int messageCnt = 0;
            message[messageCnt++] = (byte)(mGameTime >> 8);
            message[messageCnt++] = (byte)mGameTime;
            message[messageCnt++] = (byte)(((byte)State << 6) | ((byte)CarA.mTaskState << 5) | ((byte)CarB.mTaskState << 4)
                | ((byte)CarA.mIsWithPassenger << 3 & 0x08) | ((byte)CarA.mIsWithPassenger << 2 & 0x04) | ((byte)mFlood.num & 0x03));
            message[messageCnt++] = (byte)CarA.mPos.x;
            message[messageCnt++] = (byte)CarA.mPos.y;
            message[messageCnt++] = (byte)CarB.mPos.x;
            message[messageCnt++] = (byte)CarB.mPos.y;
            message[messageCnt++] = (byte)mFlood.dot1.x;
            message[messageCnt++] = (byte)mFlood.dot1.y;
            message[messageCnt++] = (byte)mFlood.dot2.x;
            message[messageCnt++] = (byte)mFlood.dot2.y;
            message[messageCnt++] = (byte)Passenger.Start_Dot.x;
            message[messageCnt++] = (byte)Passenger.Start_Dot.x;
            message[messageCnt++] = (byte)Passenger.End_Dot.x;
            message[messageCnt++] = (byte)Passenger.End_Dot.x;
            message[messageCnt++] = (byte)(((byte)PkgList[0].IsPicked << 7) | ((byte)PkgList[1].IsPicked << 6) | ((byte)PkgList[2].IsPicked << 5)
                | ((byte)PkgList[3].IsPicked << 4) | ((byte)PkgList[4].IsPicked << 3) | ((byte)PkgList[5].IsPicked << 2) | ((byte)CarA.mIsInMaze << 1) | ((byte)CarB.mIsInMaze));
            message[messageCnt++] = (byte)PkgList[0].mPos.x;
            message[messageCnt++] = (byte)PkgList[0].mPos.y;
            message[messageCnt++] = (byte)PkgList[1].mPos.x;
            message[messageCnt++] = (byte)PkgList[1].mPos.y;
            message[messageCnt++] = (byte)PkgList[2].mPos.x;
            message[messageCnt++] = (byte)PkgList[2].mPos.y;
            message[messageCnt++] = (byte)PkgList[3].mPos.x;
            message[messageCnt++] = (byte)PkgList[3].mPos.y;
            message[messageCnt++] = (byte)PkgList[4].mPos.x;
            message[messageCnt++] = (byte)PkgList[4].mPos.y;
            message[messageCnt++] = (byte)PkgList[5].mPos.x;
            message[messageCnt++] = (byte)PkgList[5].mPos.y;
            message[messageCnt++] = (byte)(CarA.MyScore >> 8);
            message[messageCnt++] = (byte)CarA.MyScore;
            message[messageCnt++] = (byte)(CarB.MyScore >> 8);
            message[messageCnt++] = (byte)CarB.MyScore;
            message[messageCnt++] = (byte)CarA.mRescueCount;
            message[messageCnt++] = (byte)CarB.mRescueCount;
            message[messageCnt++] = (byte)CarA.mPkgCount;
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