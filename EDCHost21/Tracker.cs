using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Point2i = OpenCvSharp.Point;
using Cvt = EDCHOST21.MyConvert;
using System.Runtime.InteropServices;

namespace EDCHOST21
{
    public partial class Tracker : Form
    {
        #region 成员变量的声明

        // 不合理的坐标
        public static Point2i InvalidPos = new Point2i(-1, -1);

        // 比赛所用参数和场上状况
        public MyFlags flags = null;
        public VideoCapture capture = null;

        // 设定的显示画面四角坐标
        private Point2f[] showCornerPts = null;
        // 当前时间
        private DateTime timeCamNow;
        // 上一个记录的时间
        private DateTime timeCamPrev;
        // 坐标转换器
        public CoordinateConverter coordCvt;
        // 定位器
        private Localiser localiser;

        // 以下坐标均为相机坐标
        // 人员坐标
        private Point2i camPsgStart;
        private Point2i camPsgEnd;
        // 车A坐标
        private Point2i camCarA;
        // 车B坐标
        private Point2i camCarB;
        // 物资坐标
        private Point2i[] camPkgs;

        // 以下坐标均为逻辑坐标
        private Point2i logicPsgStart;
        private Point2i logicPsgEnd;
        private Point2i logicCarA;
        private Point2i logicCarB;
        private Point2i[] logicPkgs;

        // 以下均为显示坐标
        private Point2i showCarA;
        private Point2i showCarB;

        // 游戏逻辑
        private Game game;
        // 视频输出流
        private VideoWriter vw = null;
        //是否已经进行了参数设置
        private bool alreadySet;
        // 与小车进行蓝牙通讯的端口
        public SerialPort serial1, serial2;
        // 可用的端口名称
        public string[] validPorts;

        #endregion

        // 主界面初始化
        public Tracker()
        {
            // UI界面初始化
            InitializeComponent();

            // 组件设置与初始化
            label_RedBG.SendToBack();
            label_BlueBG.SendToBack();
            label_RedBG.Controls.Add(label_CarA);
            label_RedBG.Controls.Add(labelAScore);
            label_BlueBG.Controls.Add(label_CarB);
            int newX = label_CarB.Location.X - label_BlueBG.Location.X;
            int newY = label_CarB.Location.Y - label_BlueBG.Location.Y;
            label_CarB.Location = new System.Drawing.Point(newX, newY);
            label_BlueBG.Controls.Add(labelBScore);
            newX = labelBScore.Location.X - label_BlueBG.Location.X;
            newY = labelBScore.Location.Y - label_BlueBG.Location.Y;
            labelBScore.Location = new System.Drawing.Point(newX, newY);
            label_GameCount.Text = "上半场";

            //flags参数类
            flags = new MyFlags();
            flags.Init();
            flags.Start();

            // 创建视频流
            capture = new VideoCapture();
            // threadCamera = new Thread(CameraReading);
            capture.Open(0);

            // 相机画面大小设为视频帧大小
            flags.cameraSize.Width = capture.FrameWidth;
            flags.cameraSize.Height = capture.FrameHeight;

            // 显示大小设为界面组件大小
            flags.showSize.Width = pbCamera.Width;
            flags.showSize.Height = pbCamera.Height;

            // 用于存储鼠标点击的画面中场地的4个角的坐标
            showCornerPts = new Point2f[4];

            // 以既有的flags参数初始化坐标转换器
            coordCvt = new CoordinateConverter(flags);

            // 定位小车位置的类
            localiser = new Localiser();

            // 记录时间
            timeCamNow = DateTime.Now;
            timeCamPrev = timeCamNow;

            // 相机坐标初始化
            camPsgStart = new Point2i();
            camPsgEnd = new Point2i();
            camCarA = new Point2i();
            camCarB = new Point2i();
            camPkgs = new Point2i[0];

            // 逻辑坐标初始化
            logicPsgStart = new Point2i();
            logicPsgEnd = new Point2i();
            logicCarA = new Point2i();
            logicCarB = new Point2i();
            logicPkgs = new Point2i[0];

            // 显示坐标初始化
            showCarA = new Point2i();
            showCarB = new Point2i();

            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;

            validPorts = SerialPort.GetPortNames();
            alreadySet = false;

            //Game.LoadMap();
            game = new Game();

            // 如果视频流开启，开始进行计时器事件
            if (capture.IsOpened())
            {
                // 设置帧大小
                capture.FrameWidth = flags.cameraSize.Width;
                capture.FrameHeight = flags.cameraSize.Height;
                capture.ConvertRgb = true;

                // 设置计时器timer100ms的触发间隔：75ms
                timer.Interval = 100; //xhl改成了100
                // 计时器事件开始：间隔75ms执行Flush
                timer.Start();
                // Cv2.NamedWindow("binary");

                // 设置定时器的触发间隔为 100ms
                timerMsg100ms.Interval = 100;
                // 启动计时器，执行给迷宫外的小车定时发信息的任务
                timerMsg100ms.Start();

                // 设置定时器的触发间隔为 1s
                timerMsg1s.Interval = 1000;
                // 启动计时器，执行给迷宫内的小车定时发送信息的任务
                timerMsg1s.Start();
            }
            Debug.WriteLine("Tracker Initialize Finished\n");
        }

        // 进行界面刷新、读取摄像头图像、与游戏逻辑交互的周期性函数
        private void Flush()
        {
            // 如果还未进行参数设置，则创建并打开SetWindow窗口，进行参数设置
            if (!alreadySet)
            {
                SetWindow st = new SetWindow(ref flags, ref game, this);
                st.Show();
                alreadySet = true;
            }

            // 从视频帧中读取一帧，进行图像处理、绘图和数值更新
            VideoProcess();

            // 保存上一帧的小车位置，以便判断是否逆行
            game.CarA.UpdateLastPos();
            game.CarB.UpdateLastPos();

            // 游戏逻辑端接收图像处理端信息
            game.CarA.SetPos(Cvt.Point2Dot(logicCarA));
            game.CarB.SetPos(Cvt.Point2Dot(logicCarB));  

            // 更新比赛信息
            game.Update();

            // 图像处理端接收游戏逻辑端信息
            logicPsgStart = Cvt.Dot2Point(game.curPsg.Start_Dot);
            logicPsgEnd = Cvt.Dot2Point(game.curPsg.End_Dot); 
        }

        // 当Tracker被加载时调用此函数
        // 读取data.txt文件中存储的hue,saturation,value等的默认值
        private void Tracker_Load(object sender, EventArgs e)
        {
            if (File.Exists("data.txt"))
            {
                FileStream fsRead = new FileStream("data.txt", FileMode.Open);
                int fsLen = (int)fsRead.Length;
                byte[] heByte = new byte[fsLen];
                fsRead.Read(heByte, 0, heByte.Length);
                string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                string[] str = myStr.Split(' ');

                flags.configs.hue1Lower = Convert.ToInt32(str[0]);
                flags.configs.hue1Upper = Convert.ToInt32(str[1]);
                flags.configs.hue2Lower = Convert.ToInt32(str[2]);
                flags.configs.hue2Upper = Convert.ToInt32(str[3]);
                flags.configs.saturation1Lower = Convert.ToInt32(str[4]);
                flags.configs.saturation2Lower = Convert.ToInt32(str[5]);
                flags.configs.valueLower = Convert.ToInt32(str[6]);
                flags.configs.areaLower = Convert.ToInt32(str[7]);

                fsRead.Close();
            }
        }


        #region 向小车传送信息
        // 给小车A发送信息
        private void SendCarAMessage()
        {
            // 打包好要发给A车的信息
            byte[] Message = game.PackCarAMessage();

            // 通过串口1发送给A车
            if (serial1 != null && serial1.IsOpen)
                serial1.Write(Message, 0, 102);
            ShowMessage(Message);
            validPorts = SerialPort.GetPortNames();
        }

        // 给小车B发送信息
        private void SendCarBMessage()
        {
            // 打包好要发给B车的信息
            byte[] Message = game.PackCarBMessage();

            // 通过串口2发送给B车
            if (serial2 != null && serial2.IsOpen)
                serial2.Write(Message, 0, 102);

            ShowMessage(Message);
            validPorts = SerialPort.GetPortNames();
        }
        #endregion


        #region 图像处理与界面显示

        // 从视频帧中读取一帧，进行图像处理、绘图和数值更新
        private void VideoProcess()
        {
            if (flags.running)
            {
                // 多个using连在一起写可能是共用最后一个using的作用域（没查到相关资料）
                using (Mat videoFrame = new Mat())
                using (Mat showFrame = new Mat())
                {
                    // 从视频流中读取一帧相机画面videoFrame
                    if (capture.Read(videoFrame))
                    {
                        // 调用坐标转换器，将flags中设置的人员出发点从逻辑坐标转换为显示坐标
                        // coordCvt.PeopleFilter(flags);

                        // 调用定位器，进行图像处理，得到小车位置中心点集
                        // 第一个形参videoFrame传入的是指针，所以videoFrame已被修改（画上了红蓝圆点）
                        localiser.Locate(videoFrame, flags);
                        
                        // 调用定位器，得到小车的坐标
                        localiser.GetCarLocations(out camCarA, out camCarB);

                        // 小车的相机坐标数组
                        Point2f[] camCars = { camCarA, camCarB };

                        // 转换成显示坐标数组
                        // 此转换与只与showMap和camMap有关，不需要图像被校正
                        Point2f[] showCars = coordCvt.CameraToShow(camCars);

                        showCarA = showCars[0];
                        showCarB = showCars[1];

                        // 如果画面已经被手工校正，则可以获取小车的逻辑坐标
                        if (flags.calibrated)
                        {
                            // 将小车坐标从相机坐标转化成逻辑坐标
                            Point2f[] logicCars = coordCvt.CameraToLogic(camCars);

                            logicCarA = logicCars[0];
                            logicCarB = logicCars[1];
                        }
                        else  // 否则将小车的坐标设为（-1，-1）
                        {
                            logicCarA = InvalidPos;
                            logicCarB = InvalidPos;
                        }





                        // 在显示的画面上绘制小车，乘客，物资等对应的图案
                        PaintPattern(videoFrame, localiser);

                        // 处理时间参数
                        timeCamNow = DateTime.Now;
                        TimeSpan timeProcess = timeCamNow - timeCamPrev;
                        timeCamPrev = timeCamNow;

                        // 将摄像头视频帧缩放成显示帧
                        // Resize函数的最后一个参数是缩放函数的插值算法
                        // InterpolationFlags.Cubic 表示双三次插值法，放大图像时效果较好，但速度较慢
                        Cv2.Resize(videoFrame, showFrame, flags.showSize, 0, 0, InterpolationFlags.Cubic);

                        

                        // 更新界面组件的画面显示
                        BeginInvoke(new Action<Image>(UpdateCameraPicture), BitmapConverter.ToBitmap(showFrame));
                        // 输出视频
                        if (flags.videomode == true)
                        {
                            vw.Write(showFrame);
                        }
                    }
                }
            }
        }

        // 在图像上绘制图案便于观察
        public void PaintPattern(Mat mat, Localiser loc)
        {
            // 绘制边界点，在鼠标点击的场地的四个边界点上画上绿色小十字
            foreach (Point2f pt in coordCvt.ShowToCamera(showCornerPts))
            {
                Cv2.Line(mat, (int)(pt.X - 3), (int)(pt.Y),
                    (int)(pt.X + 3), (int)(pt.Y), new Scalar(0x00, 0xff, 0x98));
                Cv2.Line(mat, (int)(pt.X), (int)(pt.Y - 3),
                    (int)(pt.X), (int)(pt.Y + 3), new Scalar(0x00, 0xff, 0x98));
            }

            // Cv2.Circle(mat, 50, 50, 15, new Scalar(0x3c, 0x14, 0xdc), -1);

            //Debug.WriteLine("{0}\n", loc.GetCentres(Camp.A).Count());

            // 在小车1的位置上绘制红色实心圆
            foreach (Point2i c1 in loc.GetCentres(Camp.A))
                Cv2.Circle(mat, c1, 10, new Scalar(0x3c, 0x14, 0xdc), -1);

            //Point2f[] camCentCarB = loc.GetCentres(Camp.B).ToArray();
            // 在小车2的位置上绘制蓝色实心圆
            foreach (Point2i c2 in loc.GetCentres(Camp.B))
                Cv2.Circle(mat, c2, 10, new Scalar(0xff, 0x00, 0x00), -1);
/*
            // 在人员起始位置上绘制矩形
            // 如果人员存在
            if (logicPsgStart != InvalidPos)
            {
                int x10 = logicPsgStart.X - 8;
                int y10 = logicPsgStart.Y - 8;
                Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
            }*/
            //绘制小车
            //car1 深蓝色 圆  半径10
            //car2 纯红色 圆  半径10
            //Cv2.Circle(videoFrame, camCarA, 10, new Scalar(0x3c, 0x14, 0xdc), -1);
            //Cv2.Circle(videoFrame, camCarB, 10, new Scalar(0xff, 0x00, 0x00), -1);

            //绘制人员起始或终点位置， 并在当前位置和目标位置连线
            //目标点 绿色 正方形  边长16
            //连线 浅绿 线宽 3
            if (game.gameState != GameState.UNSTART)
            {
                if (game.UpperCamp == Camp.A)
                {
                    if (game.CarB.mIsWithPassenger == 0)
                    {
                        int x10 = logicPsgStart.X - 8;
                        int y10 = logicPsgStart.Y - 8;
                        Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
                        Cv2.Line(mat, camCarB.X, camCarB.Y, x10, y10, new Scalar(0x00, 0xff, 0x98), 3);
                    }
                    else
                    {
                        int x10 = logicPsgEnd.X - 8;
                        int y10 = logicPsgEnd.Y - 8;
                        Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
                        Cv2.Line(mat, camCarB.X, camCarB.Y, x10, y10, new Scalar(0x00, 0xff, 0x98), 3);
                    }
                }
                else if (game.UpperCamp == Camp.B)
                {
                    if (game.CarA.mIsWithPassenger == 0)
                    {
                        int x10 = logicPsgStart.X - 8;
                        int y10 = logicPsgStart.Y - 8;
                        Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
                        Cv2.Line(mat, camCarA.X, camCarA.Y, x10, y10, new Scalar(0x00, 0xff, 0x98), 3);
                    }
                    else
                    {
                        int x10 = logicPsgEnd.X - 8;
                        int y10 = logicPsgEnd.Y - 8;
                        Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
                        Cv2.Line(mat, camCarA.X, camCarA.Y, x10, y10, new Scalar(0x00, 0xff, 0x98), 3);
                    }
                }

            }

            //绘制迷宫障碍物
            for(int i = 0; i < game.mLabyrinth.mWallNum; i++)
            {
                Dot StartDot = game.mLabyrinth.mpWallList[i].w1;
                Dot EndDot = game.mLabyrinth.mpWallList[i].w2;
                Cv2.Line(mat, StartDot.x, StartDot.y, EndDot.x, EndDot.y, new Scalar(0x00, 0x00, 0x00), 5);
                
            }
            Debug.WriteLine("Has created Laby.");
            //Cv2.Merge(new Mat[] { car1, car2, black }, merged);
            //Cv2.ImShow("binary", merged);
        }

        // 更新UI界面上的显示图像
        private void UpdateCameraPicture(Image img)
        {
            pbCamera.Image = img;
        }

        // 显示信息到UI界面上
        // 参数 M 接收的是发送给小车的编码过的信息，但并未使用，猜测可能仅于调试时使用
        private void ShowMessage(byte[] M)
        {
            //label_CountDown.Text = $"{(game.MaxRound - game.Round) / 600}:{((game.MaxRound - game.Round) / 10) % 60 / 10}{((game.MaxRound - game.Round) / 10) % 60 % 10}";

            // A,B车的总分数
            labelAScore.Text = $"{game.CarA.MyScore}";
            labelBScore.Text = $"{game.CarB.MyScore}";

            // 上半场或下半场
            label_GameCount.Text = (game.gameStage == GameStage.FIRST_1 || game.gameStage == GameStage.LATTER_1) ? "上半场" : "下半场";

            // 阶段一或阶段二
            label_GameStage.Text = (game.gameStage == GameStage.FIRST_2 || game.gameStage == GameStage.LATTER_2) ? "阶段一" : "阶段二";

            // A,B车犯规的次数
            label_AFoulNum.Text = $"{game.CarA.mFoulCount}";
            label_BFoulNum.Text = $"{game.CarB.mFoulCount}";

            // A,B车的得分明细
            label_AMessage.Text =
                $"转移被困人员数　　{game.CarA.mRescueCount}\n" +
                $"获得防汛物资数　　{game.CarA.mPkgCount}\n";
            label_BMessage.Text =
                $"{game.CarB.mRescueCount}　　转移被困人员数\n" +
                $"{game.CarB.mPkgCount}　　获得防汛物资数\n";

            // A,B车的坐标信息
            label_Debug.Text =
                $"A车坐标： ({game.CarA.mPos.x}, {game.CarA.mPos.y})\n" +
                $"B车坐标： ({game.CarB.mPos.x}, {game.CarB.mPos.y})";
        }

        #endregion


        #region 与界面控件有关的函数

        // 当Tracker界面被关闭时，处理一些接口的关闭
        private void Tracker_FormClosed(object sender, FormClosedEventArgs e)
        {
            lock (flags)
            {
                flags.End();
            }
            timer.Stop();
            //threadCamera.Join();
            capture.Release();
            if (serial1 != null && serial1.IsOpen)
                serial1.Close();
            if (serial2 != null && serial2.IsOpen)
                serial2.Close();
        }

        // 重置画面
        private void btnReset_Click(object sender, EventArgs e)
        {
            lock (flags)
            {
                flags.clickCount = 0;
                flags.calibrated = false;
                for (int i = 0; i < 4; ++i)
                    showCornerPts[i].X = showCornerPts[i].Y = 0;
            }
        }

        // 通过鼠标点击屏幕上的地图的4个角以校正画面
        // 当显示画面被点击时触发
        // C#中，X轴从左向右，Y轴从上向下
        // 左上角（0,0）；     右上角（width，0）
        // 左下角（0,height）；右下角（width，height）
        private void pbCamera_MouseClick(object sender, MouseEventArgs e)
        {
            int widthView = pbCamera.Width;
            int heightView = pbCamera.Height;

            int xMouse = e.X;
            int yMouse = e.Y;

            int idx = -1;

            lock (flags)
            {
                if (flags.clickCount < 4)
                {
                    flags.clickCount++;
                    idx = flags.clickCount - 1;
                }
            }

            // 如果画面已经被点击了4次，则不再重复校正
            if (idx == -1) return;

            if (xMouse >= 0 && xMouse < widthView && yMouse >= 0 && yMouse < heightView)
            {
                showCornerPts[idx].X = xMouse;
                showCornerPts[idx].Y = yMouse;
                if (idx == 3)
                {
                    coordCvt.UpdateCorners(showCornerPts, flags);
                    MessageBox.Show(
                          $"边界点设置完成\n"
                        + $"0: {showCornerPts[0].X, 5}, {showCornerPts[0].Y, 5}\t"
                        + $"1: {showCornerPts[1].X, 5}, {showCornerPts[1].Y, 5}\n"
                        + $"2: {showCornerPts[2].X, 5}, {showCornerPts[2].Y, 5}\t"
                        + $"3: {showCornerPts[3].X, 5}, {showCornerPts[3].Y, 5}");
                }
            }
        }

        // 比赛开始
        private void buttonStart_Click(object sender, EventArgs e)
        {
            game.Start();
            buttonPause.Enabled = true;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = false;
        }

        // 比赛暂停（待完善）
        private void buttonPause_Click(object sender, EventArgs e)
        {
            // to add something...
            game.Pause();
            buttonPause.Enabled = false;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = true;
        }

        // 比赛重新开始
        private void button_restart_Click(object sender, EventArgs e)
        {
            lock (game) { game = new Game(); }
            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;
            label_CarA.Text = "A车";
            label_CarB.Text = "B车";
        }

        // 开始录像
        private void button_video_Click(object sender, EventArgs e)
        {
            lock (flags)
            {
                if (flags.videomode == false)
                {
                    string time = DateTime.Now.ToString("MMdd_HH_mm_ss");
                    vw = new VideoWriter("../../video/" + time + ".avi",
                        FourCC.XVID, 10.0, flags.showSize);
                    flags.videomode = true;
                    ((Button)sender).Text = "停止录像";
                    game.FoulTimeFS = new FileStream("../../video/" + time + ".txt", FileMode.CreateNew);
                }
                else
                {
                    vw.Release();
                    vw = null;
                    flags.videomode = false;
                    ((Button)sender).Text = "开始录像";
                    game.FoulTimeFS = null;
                }
            }
        }

        // 打开设置调试窗口
        private void button_set_Click(object sender, EventArgs e)
        {
            lock (flags)
            {
                SetWindow st = new SetWindow(ref flags, ref game, this);
                st.Show();
            }
        }

        // 开始下半场比赛
        private void button_Continue_Click(object sender, EventArgs e)
        {
            //if (game.state == GameState.End)
            game.Continue();
            buttonPause.Enabled = false;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = true;
        }

        // A车记1次犯规
        private void button_AFoul_Click(object sender, EventArgs e)
        {
            game.CarA.mFoulCount++;
            game.CarA.AddFoulCount();

            if (game.FoulTimeFS != null)
            {
                //byte[] data = Encoding.Default.GetBytes($"A -50 {game.Round}\r\n");
                //game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        // B车记1次犯规
        private void button_BFoul_Click(object sender, EventArgs e)
        {
            game.CarB.mFoulCount++;
            game.CarB.AddFoulCount();

            if (game.FoulTimeFS != null)
            {
                //byte[] data = Encoding.Default.GetBytes($"B -50 {game.Round}\r\n");
                //game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        // 比赛结束（待完善）
        private void buttonEnd_Click(object sender, EventArgs e)
        {
            //game.
            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;
        }

        #endregion


        #region 由定时器控制的函数
        //计时器事件：执行Flush
        private void timer_Tick(object sender, EventArgs e)
        {
            Flush();
        }

        //计时器事件，每100ms触发一次，向在迷宫外的小车发送信息
        private void timerMsg100ms_Tick(object sender, EventArgs e)
        {
            // 如果A车在场地内且在迷宫外
            if (game.CarA.mIsInField == 1 && game.CarA.mIsInMaze == 0)
            {
                SendCarAMessage();
            }
            // 如果B车在场地内且在迷宫外
            if (game.CarB.mIsInField == 1 && game.CarB.mIsInMaze == 0)
            {
                SendCarBMessage();
            }
        }

        //计时器事件，每1s触发一次，向在迷宫内的小车发送信息
        private void timerMsg1s_Tick(object sender, EventArgs e)
        {
            game.UpdateCarLastOneSecondPos();
            game.SetFlood();
            // 如果A车在场地内且在迷宫内
            if (game.CarA.mIsInField == 1 && game.CarA.mIsInMaze == 1)
            {
                SendCarAMessage();
            }
            // 如果B车在场地内且在迷宫内
            if (game.CarB.mIsInField == 1 && game.CarB.mIsInMaze == 1)
            {
                SendCarBMessage();
            }
        }
        #endregion


        #region 被注释的代码 暂先保留
        /*
        private void buttonchangescore_click(object sender, eventargs e)
        {
            int ascore = (int)numericupdownscorea.value;
            int bscore = (int)numericupdownscoreb.value;
            numericupdownscorea.value = 0;
            numericupdownscoreb.value = 0;
            lock (game)
            {
                game.cara.score += ascore;
                game.carb.score += bscore;
            }
        }

        private void numericUpDownScoreA_ValueChanged(object sender, EventArgs e)
        {
            game.AddScore(Camp.CampA, (int)((NumericUpDown)sender).Value);
            ((NumericUpDown)sender).Value = 0;
        }

        private void numericUpDownScoreB_ValueChanged(object sender, EventArgs e)
        {
            game.AddScore(Camp.CampB, (int)((NumericUpDown)sender).Value);
            ((NumericUpDown)sender).Value = 0;
        }

        //绘制人员信息
        private void groupBox_Person_Paint(object sender, PaintEventArgs e)
        {
            Brush br_No_NV = new SolidBrush(Color.Silver);
            Brush br_No_V = new SolidBrush(Color.DimGray);
            Brush br_A_NV = new SolidBrush(Color.Pink);
            Brush br_A_V = new SolidBrush(Color.Red);
            Brush br_B_NV = new SolidBrush(Color.SkyBlue);
            Brush br_B_V = new SolidBrush(Color.RoyalBlue);
            Graphics gra = e.Graphics;
            int vbargin = 100;
            for (int i = 0; i != game.CurrPersonNumber; ++i)
            {
                switch (game.People[i].Owner)
                {
                    case Camp.None:
                        gra.FillEllipse(br_No_V, 40, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_A_NV, 100, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_B_NV, 160, 100 + i * vbargin, 30, 30);
                        break;
                    case Camp.CampA:
                        gra.FillEllipse(br_No_NV, 40, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_A_V, 100, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_B_NV, 160, 100 + i * vbargin, 30, 30);
                        break;
                    case Camp.CampB:
                        gra.FillEllipse(br_No_NV, 40, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_A_NV, 100, 100 + i * vbargin, 30, 30);
                        gra.FillEllipse(br_B_V, 160, 100 + i * vbargin, 30, 30);
                        break;
                    default: break;
                }
            }
        }
        */
        #endregion
    }

}

