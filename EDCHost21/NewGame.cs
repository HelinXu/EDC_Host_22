using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;


namespace EDCHOST21
{
    public enum GameState { Unstart = 0, Normal = 1, Pause = 2, End = 3 };

    public class Game
    {
        public bool DebugMode; //调试模式，最大回合数 = 1,000,000
        public const int MAX_SIZE = 280;
        public const int MAZE_CROSS_NUM = 6; //迷宫由几条线交叉而成
        public const int MAZE_CROSS_DIST = 30; //间隔的长度
        public const int MazeBorderPoint1 = 35; //迷宫最短的靠边距离  xhl?
        public const int MazeBorderPoint2 = MazeBorderPoint1 + MAZE_CROSS_DIST * MAZE_CROSS_NUM;//迷宫最长的靠边距离 xhl?
        public const int MaxCarryDistance = 10; //判定是否到达的最大距离
        public const int MAX_PKG_NUM = 6; //场上每次刷新package物资的个数

        public int GameCount; //上下半场 1是上半场 2是下半场
        public int GameStage; //上下阶段 1是上阶段 2是下阶段
        public Camp GameCamp; //当前半场需完成“上半场”任务的一方
        public GameState State { get; set; }//比赛状态
        public Car CarA, CarB;//定义小车
        public Passenger Passenger;
        public PassengerGenerator PsgGenerator { get; set; }
        public PackageGenerator[] PkgGenerator;
        public Package[] PackageDot;
        public Flood Flood;
        public Obstacle Obstacle;
        public int StartTime;//时间均改为以毫秒为单位
        public int GameTime;//时间均改为以毫秒为单位
        public int PackageCount;//用于记录现在的Package是第几波
        public int LastWrongDirectionTime;
        //public static bool[,] GameMap = new bool[MaxSize, MaxSize]; //地图信息
        public FileStream FoulTimeFS;

        //该方法用于返回系统现在的时间。开发者：xhl
        public System.DateTime GetCurrentTime()
        {
            System.DateTime currentTime = new System.DateTime();
            return currentTime;
        }
        public static bool InMaze(Dot dot)//确定点是否在迷宫内
        {
            if (dot.x >= MazeBorderPoint1 && dot.x <= MazeBorderPoint2 && dot.y >= MazeBorderPoint1 && dot.y <= MazeBorderPoint2)
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

        /*0814xhl觉得这个没用，而且也应该写在Dot里。
        public static Dot OppoDots(Dot prevDot)//复制点
        {
            Dot newDots = new Dot();
            newDots.x = prevDot.y;
            newDots.y = prevDot.x;
            return newDots;
        }*/
        public static double GetDistance(Dot A, Dot B)//得到两个点之间的距离
        {
            return Math.Sqrt((A.x - B.x) * (A.x - B.x) + (A.y - B.y) * (A.y - B.y));
        }
        public Game()//构造一个新的Game类 默认为CampA是先上半场上一阶段进行
        {
            GameCount = 1;
            GameStage = 1;
            GameCamp = Camp.CampA;
            CarA = new Car(Camp.CampA, 0);
            CarB = new Car(Camp.CampB, 1);
            State = GameState.Unstart;
            PsgGenerator = new PassengerGenerator(100);//上下半场将都用这一个索引
            PkgGenerator[0] = new PackageGenerator(6);
            PkgGenerator[1] = new PackageGenerator(6);
            PkgGenerator[2] = new PackageGenerator(6);
            PkgGenerator[3] = new PackageGenerator(6);
            Flood = new Flood(0);
            PackageCount = 0;
            LastWrongDirectionTime = -10;
        }
        //点击开始键时调用Start函数 上半场上一阶段、上半场下一阶段、下半场上一阶段、下半场下一阶段开始时需要这一函数都需要调用这一函数来开始
        public void Start() //开始比赛上下半场都用这个
        {
            State = GameState.Normal;
            GameTime = 0;
            StartTime = GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second*1000;//记录比赛开始时候的时间
        }
        //点击暂停比赛键时调用Pause函数
        public void Pause() //暂停比赛
        {
            State = GameState.Pause;
            GameTime = GameTime + GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second*1000 - StartTime;//记录现在比赛已经进行了多少时间了
        }
        //半场交换函数自动调用依照时间控制
        public void NextCount()//从上半场更换到下半场函数
        {
            if (GameCount == 1 && GameStage == 2 && GameTime == 120000)
            {
                State = GameState.Pause;
                GameCamp = Camp.CampB;//上半场转换
                PsgGenerator.ResetIndex();//Passenger的索引复位
                if (FoulTimeFS != null)                                            //这里没有搞懂是干什么的
                {
                    byte[] data = Encoding.Default.GetBytes($"nextStage\r\n");
                    FoulTimeFS.Write(data, 0, data.Length);
                }
                CarA.Task = 1;//交换A和B的任务
                CarB.Task = 0;
            }

        }
        //半阶段交换函数自动调用依照时间控制
        public void NextStage()
        {
            if (GameStage == 1 && GameTime == 60000)
            {
                State = GameState.Pause;
                UpdatePassenger();
            }
        }
        //在暂停后需要摁下继续按钮来继续比赛
        public void Continue()
        {
            State = GameState.Normal;
            StartTime = GetCurrentTime().Hour * 3600000 + GetCurrentTime().Minute * 60000 + GetCurrentTime().Second*1000;
        }
        //重置摁键对应的函数
        public void Reset()
        {
            //Game = new Game();
        }
        //犯规键对应的函数
        public void Foul()
        {
            if (GameCount == 1 && GameStage == 1)
            {
                CarA.FoulNum++;
            }
            if (GameCount == 1 && GameStage == 2)
            {
                CarB.FoulNum++;
            }
            if (GameCount == 2 && GameStage == 1)
            {
                CarB.FoulNum++;
            }
            if (GameCount == 2 && GameStage == 2)
            {
                CarA.FoulNum++;
            }
        }
        //每到半点自动更新Package信息函数
        public void UpdatePackage()//到半点时更换Package函数
        {
            int changenum = GameTime / 300 + 1;
            if(GameStage == 2 && PackageCount < changenum)
            {
                for(int i=0;i<MAX_PKG_NUM;i++)
                {
                    PackageDot[i].Pos = PkgGenerator[changenum].GetPkg_Dot(i); 
                    PackageDot[i].IsPicked = false;                              
                }
                PackageCount++;
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
            if (GetDistance(CarA.Pos, Passenger.Start_Dot) <= MaxCarryDistance && CarA.transport == 0)
            {
                CarA.Picked();
            }

        }
        public void CarBGetPassenger()//小车B接到了乘客
        {
            if (GetDistance(CarB.Pos, Passenger.Start_Dot) <= MaxCarryDistance && CarA.transport == 0)
            {
                CarB.Picked();
            }
        }
        public void CarATransPassenger()//小车A成功运送了乘客
        {

            if (GetDistance(CarA.Pos, Passenger.End_Dot) <= MaxCarryDistance && CarA.transport == 1)
            {
                CarA.Picked();
                CarA.TransportNumplus();
            }
            UpdatePassenger();
        }
        public void CarBTransPassenger()//小车A成功运送了乘客
        {
            if (GetDistance(CarB.Pos, Passenger.End_Dot) <= MaxCarryDistance && CarB.transport == 1)
            {
                CarB.Picked();
                CarB.TransportNumplus();
            }
            UpdatePassenger();
        }
        //下面是两个关于包裹的接口
        public void CarAGetpackage()//小车A得到了包裹
        {

            for (int i=0; i < MAX_PKG_NUM; i++)
            {
                if (GetDistance(CarA.Pos, PackageDot[i].Pos) <= MaxCarryDistance && PackageDot[i].IsPicked == false)
                {
                    CarA.PickNumplus();
                    PackageDot[i].IsPicked = true;
                }
            }

        }
        public void CarBGetpackage()//小车B得到了包裹
        {
            for (int i=0; i < MAX_PKG_NUM; i++)
            {
                if (GetDistance(CarB.Pos, PackageDot[i].Pos) <= MaxCarryDistance && PackageDot[i].IsPicked == false)
                {
                    CarB.PickNumplus();
                    PackageDot[i].IsPicked = true;
                }

            }
        }
        /*public void CarAonObstacle()//小车A到达了障碍上              
        {
            
            if(GetDistance(CarA))                                              //待修改
                CarA.ObastaclePunishplus();
        }
        public void CarBonObstacle()//小车B到达了障碍上               
        {
            CarB.ObastaclePunishplus();
        }*/
        public void CarAonFlood()//A车到大障碍上
        {
            
            if(CarA.Task==1)//在下半场的时候才应该判断小车是否经过Flood
            {
                if (Flood.num == 0)
                { }
                else if (Flood.num == 1)
                {
                    if (GetDistance(CarA.Pos, Flood.dot1) <= MaxCarryDistance)
                    {

                        CarA.StopPunishplus();
                    }
                }
                else if (Flood.num == 2)
                {

                    if (GetDistance(CarA.Pos, Flood.dot1) <= MaxCarryDistance)
                    {
                        CarA.StopPunishplus();
                    }
                    if (GetDistance(CarA.Pos, Flood.dot2) <= MaxCarryDistance)
                    {
                        CarA.StopPunishplus();
                    }
                }

            }
        }
        public void CarBonFlood()
        {
            if (CarB.Task == 1)//在下半场的时候才应该判断小车是否经过Flood
            {
                if (Flood.num == 0)
                { }
                else if (Flood.num == 1)
                {
                    if (GetDistance(CarB.Pos, Flood.dot1) <= MaxCarryDistance)
                    {

                        CarB.StopPunishplus();
                    }
                }
                else if (Flood.num == 2)
                {

                    if (GetDistance(CarB.Pos, Flood.dot1) <= MaxCarryDistance)
                    {
                        CarB.StopPunishplus();
                    }
                    if (GetDistance(CarB.Pos, Flood.dot2) <= MaxCarryDistance)
                    {
                        CarB.StopPunishplus();
                    }
                }

            }
        }
        //逆行自动判断//目前为思路两次逆行之间间隔时间判断为5s，这5s之间的逆行忽略不计
        public void CarAWrongDirection()
        {
            if (CarA.LastPos.x < 30 && CarA.Pos.x < 30 && CarA.LastPos.y > 30 && CarA.LastPos.y < 220 && CarA.Pos.y > 30 && CarA.Pos.y < 220 && CarA.Pos.y > CarA.LastPos.y && GameTime - LastWrongDirectionTime > 50)
            {
                CarA.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarA.LastPos.x > 220 && CarA.Pos.x < 220 && CarA.LastPos.y > 30 && CarA.LastPos.y < 220 && CarA.Pos.y > 30 && CarA.Pos.y < 220 && CarA.Pos.y < CarA.LastPos.y && GameTime - LastWrongDirectionTime > 50)
            {
                CarA.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarA.LastPos.y < 30 && CarA.Pos.y < 30 && CarA.LastPos.x > 30 && CarA.LastPos.x < 220 && CarA.Pos.x > 30 && CarA.Pos.x < 220 && CarA.Pos.x < CarA.LastPos.x && GameTime - LastWrongDirectionTime > 50)
            {
                CarA.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarA.LastPos.y > 220 && CarA.Pos.y > 220 && CarA.LastPos.x > 30 && CarA.LastPos.x < 220 && CarA.Pos.x > 30 && CarA.Pos.x < 220 && CarA.Pos.x > CarA.LastPos.x && GameTime - LastWrongDirectionTime > 50)
            {
                CarA.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
        }
        public void CarBWrongDirection()
        {
            if (CarB.LastPos.x < 30 && CarB.Pos.x < 30 && CarB.LastPos.y > 30 && CarB.LastPos.y < 220 && CarB.Pos.y > 30 && CarB.Pos.y < 220 && CarB.Pos.y > CarB.LastPos.y && GameTime - LastWrongDirectionTime > 50)
            {
                CarB.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarB.LastPos.x > 220 && CarB.Pos.x < 220 && CarB.LastPos.y > 30 && CarB.LastPos.y < 220 && CarB.Pos.y > 30 && CarB.Pos.y < 220 && CarB.Pos.y < CarB.LastPos.y && GameTime - LastWrongDirectionTime > 50)
            {
                CarB.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarB.LastPos.y < 30 && CarB.Pos.y < 30 && CarB.LastPos.x > 30 && CarB.LastPos.x < 220 && CarB.Pos.x > 30 && CarB.Pos.x < 220 && CarB.Pos.x < CarB.LastPos.x && GameTime - LastWrongDirectionTime > 50)
            {
                CarB.FoulNumplus();
                LastWrongDirectionTime = GameTime;
            }
            if (CarB.LastPos.y > 220 && CarB.Pos.y > 220 && CarB.LastPos.x > 30 && CarB.LastPos.x < 220 && CarB.Pos.x > 30 && CarB.Pos.x < 220 && CarB.Pos.x > CarB.LastPos.x && GameTime - LastWrongDirectionTime > 50)
            {
                CarB.FoulNumplus();
                LastWrongDirectionTime = GameTime;
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
        /*public byte[] PackMessage()//已更新到最新通信协议
        {
            byte[] message = new byte[40]; //上位机传递多少信息
            int messageCnt = 0;
            message[messageCnt++] = (byte)(GameTime >> 8);
            message[messageCnt++] = (byte)GameTime;
            message[messageCnt++] = (byte)(((byte)State << 6) | ((byte)CarA.Task << 5) | ((byte)CarB.Task << 4)
                | ((byte)CarA.transport << 3 & 0x08) | ((byte)CarA.transport << 2 & 0x04) | ((byte)Flood.num  & 0x03) );
            message[messageCnt++] = (byte)CarA.X;
            message[messageCnt++] = (byte)CarA.Y;
            message[messageCnt++] = (byte)CarB.X;
            message[messageCnt++] = (byte)CarB.Y;
            message[messageCnt++] = (byte)Flood.dot1.X;
            message[messageCnt++] = (byte)Flood.dot1.Y;
            message[messageCnt++] = (byte)Flood.dot2.X;
            message[messageCnt++] = (byte)Flood.dot2.X;
            message[messageCnt++] = (byte)Passenger.Start_Dot.X;
            message[messageCnt++] = (byte)Passenger.Start_Dot.X;
            message[messageCnt++] = (byte)Passenger.End_Dot.X;
            message[messageCnt++] = (byte)Passenger.End_Dot.X;
            message[messageCnt++] = (byte)(（(byte)PackageDot[0].IsPicked<<7） | (（byte)PackageDot[1].IsPicked<<6)|(（byte)PackageDot[2].IsPicked<<5)
                |(（byte)PackageDot[3].IsPicked<<4)|(（byte)PackageDot[4].IsPicked<<3)|(（byte)PackageDot[5].IsPicked<<2)|((byte)CarA.Area<<1)|((byte)CarB.Area);
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
        }*/
        /*ushort Crc16(byte[] data_p, byte length)
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
        }*/
    }
}