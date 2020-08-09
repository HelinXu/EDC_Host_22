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

        public int GameCount; //上下半场等
        public Camp GameCamp; //当前半场需完成“上半场”任务的一方
        public GameState State { get; set; }//比赛状态
        public Car CarA, CarB;//定义小车
        public Passenger Passenger;
        public PassengerGenerator PsgGenerator { get; set; }
        public PackageGenerator[] PkgGenerator;
        public Dot[] PackageDot;
        public Stop Stop;
        public Obstacle Obstacle;
        public int StartTime;
        public int GameTime;
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
        public Game()//构造一个新的Game类 默认为CampA是先上半场
        {
            GameCount = 1;
            GameCamp = Camp.CampA;
            CarA = new Car(Camp.CampA,0);
            CarB = new Car(Camp.CampB,1);
            State = GameState.Unstart;
            PsgGenerator = new PassengerGenerator(100);//上下半场将都用这一个索引
            PkgGenerator[0] = new PackageGenerator(6);
            PkgGenerator[1] = new PackageGenerator(6);
            PkgGenerator[2] = new PackageGenerator(6);
            PkgGenerator[3] = new PackageGenerator(6);
            FoulTimeFS = null;
        }
        
        public void NextStage()//从上半场更换到下半场函数
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
        public void Start() //开始比赛上下半场都用这个
        {
            State = GameState.Normal;

            StartTime = GetCurrentTime().Hour * 3600 + GetCurrentTime().Minute * 60 + GetCurrentTime().Second;//记录比赛开始时候的时间
        }
        public void Pause() //暂停比赛
        {
            State = GameState.Pause;
            GameTime = GameTime + GetCurrentTime().Hour * 3600 + GetCurrentTime().Minute * 60 + GetCurrentTime().Second - StartTime;//记录现在比赛已经进行了多少时间了
        }
        public void Continue()//继续比赛
        {
            State = GameState.Normal;
            StartTime = GetCurrentTime().Hour * 3600 + GetCurrentTime().Minute * 60 + GetCurrentTime().Second;
        }
        public void UpdatePackage(int changenum)//到半点时更换Package函数changenum是第几次更换
        {
            for(int i=0;i<MaxPackageNum;i++)
            {
                PackageDot[i] = PkgGenerator[changenum].GetPackageDot(i);   //xhl把Package改成了Dot类型。
            }
        }
        public void UpdatePassenger()//更新乘客信息
        {
            Passenger = Generator.Next();
        }
        public void CarAGetPassenger()//小车A接到了乘客
        {
            CarA.Picked();
        }
        public void CarBGetPassenger()//小车B接到了乘客
        {
            CarB.Picked();
        }
        public void CarATransPassenger()//小车A成功运送了乘客
        {
            CarA.Picked();
            CarA.TransportNumplus();
            UpdatePassenger();
        }
        public void CarBTransPassenger()//小车A成功运送了乘客
        {
            CarB.Picked();
            CarB.TransportNumplus();
            UpdatePassenger();
        }
        public void CarAGetpackage()//小车A得到了包裹
        {
            CarA.PickNumplus();
        }
        public void CarBGetpackage()//小车B得到了包裹
        {
            CarB.PickNumplus();
        }
        public void CarAonObstacle()//小车A到达了障碍上
        {
            CarA.ObastaclePunishplus();
        }
        public void CarBonObstacle()//小车B到达了障碍上
        {
            CarB.ObastaclePunishplus();
        }
        public void CarAonStop()
        {
            CarA.StopPunishplus();
        }
        public void CarBonStop()
        {
            CarB.StopPunishplus();
        }
        public void CarAWrongDirection()
        {
            CarA.WrongDirectionplus();
        }
        public void CarBWrongDirection()
        {
            CarB.WrongDirectionplus();
        }
        public void SetStop(Dot stop)//上半场的小车设定障碍
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
        }
        public byte[] PackMessage()
        {
            byte[] message = new byte[40]; //上位机传递多少信息
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