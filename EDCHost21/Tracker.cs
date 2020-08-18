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
using Point2i = OpenCvSharp.Point;

namespace EDCHOST21
{
    public partial class Tracker : Form
    {
        public MyFlags flags = null;
        public VideoCapture capture = null;
        //private Thread threadCamera = null;
        //设定的显示画面四角坐标
        private Point2f[] ptsShowCorners = null;
        //当前时间
        private DateTime timeCamNow;
        //上一个记录的时间
        private DateTime timeCamPrev;
        //坐标转换器
        public CoordinateConverter cc;
        //定位器
        private Localiser localiser;
        //小球坐标
        private Point2f[] ball;
        //车1坐标
        private Point2i car1;
        //车2坐标
        private Point2i car2;
        //游戏逻辑
        private Game game;
        //视频输出流
        private VideoWriter vw = null;

        //是否已经进行了参数设置
        private bool alreadySet;
        
        public SerialPort serial1, serial2;
        public string[] validPorts;

        private string[] gametext = { "上半场", "下半场", "加时1", "加时2",
            "加时3", "加时4", "加时5", "加时6", "加时7" , "加时8", "加时9", "加时10", "加时11", "加时12"};
        private Camp[] UI_LastRoundCamp = new Camp[5];

        public Dot CarALocation()
        {
            Dot D = new Dot();
            D.x = car1.X;
            D.y = car1.Y;
            return D;
        }
        public Dot CarBLocation()
        {
            Dot D = new Dot();
            D.x = car2.X;
            D.y = car2.Y;
            return D;
        }

        public Tracker()
        {
            //UI界面初始化
            InitializeComponent();

            //组件设置与初始化
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

            //创建视频流
            capture = new VideoCapture();
            // threadCamera = new Thread(CameraReading);
            capture.Open(0);

            //相机画面大小设为视频帧大小
            flags.cameraSize.Width = capture.FrameWidth;
            flags.cameraSize.Height = capture.FrameHeight;

            //显示大小设为界面组件大小
            flags.showSize.Width = pbCamera.Width;
            flags.showSize.Height = pbCamera.Height;
            ptsShowCorners = new Point2f[4];

            //以既有的flags参数初始化坐标转换器
            cc = new CoordinateConverter(flags);

            localiser = new Localiser();

            //记录时间
            timeCamNow = DateTime.Now;
            timeCamPrev = timeCamNow;

            ball = new Point2f[0];
            car1 = new Point2i();
            car2 = new Point2i();

            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;
            button_AReset.Enabled = false;
            button_BReset.Enabled = false;

            validPorts = SerialPort.GetPortNames();
            alreadySet = false;

            //Game.LoadMap();
            game = new Game();

            //如果视频流开启，开始进行计时器事件
            if (capture.IsOpened())
            {
                //设置帧大小
                capture.FrameWidth = flags.cameraSize.Width;
                capture.FrameHeight = flags.cameraSize.Height;
                capture.ConvertRgb = true;

                //设置计时器timer100ms的触发间隔：75ms
                timer100ms.Interval = 75;
                //计时器事件开始：间隔75ms执行Flush
                timer100ms.Start();
                //Cv2.NamedWindow("binary");
            }

        }

        //进行界面刷新、图像处理、逻辑处理的周期性函数
        private void Flush()
        {
            //如果还未进行参数设置
            if (!alreadySet)
            {
                lock (flags)
                {
                    //创建并打开SetWindow窗口，进行参数设置
                    SetWindow st = new SetWindow(ref flags, ref game, this);
                    st.Show();
                }
                alreadySet = true;
            }
            //从视频帧中读取一帧，进行图像处理、绘图和数值更新
            CameraReading();
            lock (flags)
            {
                game.BallsDot.Clear();
                foreach (Point2i posBall in flags.posBalls)
                    game.BallsDot.Add(new Dot(posBall.X, posBall.Y));
                game.CarA.Pos.x = flags.posCarA.X;
                game.CarA.Pos.y = flags.posCarA.Y;
                game.CarB.Pos.x = flags.posCarB.X;
                game.CarB.Pos.y = flags.posCarB.Y;
            }
            game.Update();
            lock (flags)
            {
                flags.currPersonNum = game.CurrPersonNumber;
                for (int i = 0; i != Game.MaxPersonNum; ++i)
                {
                    flags.posPersonStart[i].X = game.People[i].StartPos.x;
                    flags.posPersonStart[i].Y = game.People[i].StartPos.y;
                    flags.gameState = game.State;
                }
            }
            byte[] Message = game.PackMessage();
            label_CountDown.Text = Convert.ToString(game.Round);
            if (serial1 != null && serial1.IsOpen)
                serial1.Write(Message, 0, 32);
            if (serial2 != null && serial2.IsOpen)
                serial2.Write(Message, 0, 32); ShowMessage(Message);
            validPorts = SerialPort.GetPortNames();
        }

        //从视频帧中读取一帧，进行图像处理、绘图和数值更新
        private void CameraReading()
        {
            bool control = false;
            lock (flags)
            {
                control = flags.running;
            }
            if (control)
            {
                using (Mat videoFrame = new Mat())
                using (Mat showFrame = new Mat())
                {
                    //从视频流中读取一帧相机画面videoFrame
                    if (capture.Read(videoFrame))
                    {
                        lock (flags)
                        {
                            //调用坐标转换器，将flags中设置的人员出发点从逻辑坐标转换为显示坐标
                            cc.PeopleFilter(flags);
                            //调用定位器，进行图像处理，得到小车和小球的位置中心点集
                            localiser.Locate(videoFrame, flags);

                            //绘制边界点
                            foreach (Point2f pt in cc.ShowToCamera(ptsShowCorners))
                            {
                                Cv2.Line(videoFrame, (int)(pt.X - 3), (int)(pt.Y), (int)(pt.X + 3), (int)(pt.Y), new Scalar(0x00, 0xff, 0x98));
                                Cv2.Line(videoFrame, (int)(pt.X), (int)(pt.Y - 3), (int)(pt.X), (int)(pt.Y + 3), new Scalar(0x00, 0xff, 0x98));
                            }
                        }
                        //调用定位器，得到小车和小球的坐标
                        localiser.GetLocations(out ball, out car1, out car2);

                        lock (flags)
                        {
                            Point2f[] posBallsF = new Point2f[0];
                            if (flags.calibrated)
                            {
                                //将小球和小车坐标从摄像头画面坐标转化成逻辑坐标
                                //再将小车坐标存储到flags中
                                if (ball.Any())
                                    posBallsF = cc.CameraToLogic(ball);
                                Point2f[] car12 = { car1, car2 };
                                Point2f[] carAB = cc.CameraToLogic(car12);
                                flags.posCarA = carAB[0];
                                flags.posCarB = carAB[1];
                            }
                            else
                            {
                                //直接将小车坐标存储到flags中
                                posBallsF = ball;
                                flags.posCarA = car1;
                                flags.posCarB = car2;
                            }
                            //将球坐标从float转为int
                            Point2i[] posBallsI = new Point2i[posBallsF.Length];
                            for (int i = 0; i < posBallsF.Length; ++i)
                                posBallsI[i] = posBallsF[i];
                            //检验小球坐标是否符合逻辑规则，若符合才将其转入flags中
                            List<Point2i> posBallsList = new List<Point2i>();
                            foreach (Point2i b in posBallsI)
                            {
                                if (!posBallsList.Any(bb => b.DistanceTo(bb) < Game.MinBallSept))
                                    posBallsList.Add(b);
                            }
                            flags.posBalls = posBallsList.ToArray();
                        }

                        //处理时间参数
                        timeCamNow = DateTime.Now;
                        TimeSpan timeProcess = timeCamNow - timeCamPrev;
                        timeCamPrev = timeCamNow;

                        //将摄像头视频帧缩放成显示帧
                        Cv2.Resize(videoFrame, showFrame, flags.showSize, 0, 0, InterpolationFlags.Cubic);
                        //更新界面组件的画面显示
                        BeginInvoke(new Action<Image>(UpdateCameraPicture), BitmapConverter.ToBitmap(showFrame));
                        //输出视频
                        if (flags.videomode == true)
                            vw.Write(showFrame);
                    }
                    lock (flags)
                    {
                        control = flags.running;
                    }
                }
            }
        }

        private void UpdateCameraPicture(Image img)
        {
            pbCamera.Image = img;
        }

        private void Tracker_FormClosed(object sender, FormClosedEventArgs e)
        {
            lock (flags)
            {
                flags.End();
            }
            timer100ms.Stop();
            //threadCamera.Join();
            capture.Release();
            if (serial1 != null && serial1.IsOpen)
                serial1.Close();
            if (serial2 != null && serial2.IsOpen)
                serial2.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            lock (flags)
            {
                flags.clickCount = 0;
                flags.calibrated = false;
                for (int i = 0; i < 4; ++i)
                    ptsShowCorners[i].X = ptsShowCorners[i].Y = 0;
            }
        }

        private void pbCamera_MouseClick(object sender, MouseEventArgs e)
        {
            int widthView = pbCamera.Width;
            int heightView = pbCamera.Height;

            int xMouse = e.X;
            int yMouse = e.Y;

            int idx = -1;
            lock (flags)
            {
                if (flags.clickCount < 4) idx = flags.clickCount++;
            }

            if (idx == -1) return;

            if (xMouse >= 0 && xMouse < widthView && yMouse >= 0 && yMouse < heightView)
            {
                ptsShowCorners[idx].X = xMouse;
                ptsShowCorners[idx].Y = yMouse;
                if (idx == 3)
                {
                    cc.UpdateCorners(ptsShowCorners, flags);
                    MessageBox.Show($"边界点设置完成\n"
                        + $"0: {ptsShowCorners[0].X,5}, {ptsShowCorners[0].Y,5}\t"
                        + $"1: {ptsShowCorners[1].X,5}, {ptsShowCorners[1].Y,5}\n"
                        + $"2: {ptsShowCorners[2].X,5}, {ptsShowCorners[2].Y,5}\t"
                        + $"3: {ptsShowCorners[3].X,5}, {ptsShowCorners[3].Y,5}");
                }
            }
        }

        //计时器事件：执行Flush
        private void timer100ms_Tick(object sender, EventArgs e)
        {
            Flush();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            game.Start();
            buttonPause.Enabled = true;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = false;
            button_AReset.Enabled = true;
            button_BReset.Enabled = true;
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            game.Pause();
            buttonPause.Enabled = false;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = true;
            button_AReset.Enabled = false;
            button_BReset.Enabled = false;
        }

        private void ShowMessage(byte[] M) //通过Message显示信息到UI上
        {
            label_CountDown.Text = $"{(game.MaxRound - game.Round) / 600}:{((game.MaxRound - game.Round) / 10) % 60 / 10}{((game.MaxRound - game.Round) / 10) % 60 % 10}";

            labelAScore.Text = $"{game.CarA.Score}";
            labelBScore.Text = $"{game.CarB.Score}";

            label_GameCount.Text = gametext[game.GameCount - 1];
            label_APauseNum.Text = $"{game.APauseNum}";
            label_BPauseNum.Text = $"{game.BPauseNum}";
            label_AFoul1Num.Text = $"{game.AFoul1}";
            label_BFoul1Num.Text = $"{game.BFoul1}";
            label_AFoul2Num.Text = $"{game.AFoul2}";
            label_BFoul2Num.Text = $"{game.BFoul2}";

            label_AMessage.Text = $"接到人员数　　{game.CarA.PersonCnt}\n抓取物资数　　{game.CarA.BallGetCnt}\n运回物资数　　{game.CarA.BallOwnCnt}";
            label_BMessage.Text = $"{game.CarB.PersonCnt}　　接到人员数\n{game.CarB.BallGetCnt}　　抓取物资数\n{game.CarB.BallOwnCnt}　　运回物资数";
            label_Debug.Text = $"A车坐标： ({game.CarA.Pos.x}, {game.CarA.Pos.y})\nB车坐标： ({game.CarB.Pos.x}, {game.CarB.Pos.y})";
            //if (game.CarA.HaveBonus)
            //    label_CarA.Text = "+" + Car.BonusRate.ToString("0%") + "  " + label_CarA.Text;
            //if (game.CarB.HaveBonus)
            //    label_CarB.Text = label_CarB.Text + "  +" + Car.BonusRate.ToString("0%");
            //  groupBox_Person.Refresh();
        }

        private void button_restart_Click(object sender, EventArgs e)
        {
            lock (game) { game = new Game(); }
            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;
            button_AReset.Enabled = false;
            button_BReset.Enabled = false;
            label_CarA.Text = "A车";
            label_CarB.Text = "B车";
        }

        //private void buttonChangeScore_Click(object sender, EventArgs e)
        //{
        //    int AScore = (int)numericUpDownScoreA.Value;
        //    int BScore = (int)numericUpDownScoreB.Value;
        //    numericUpDownScoreA.Value = 0;
        //    numericUpDownScoreB.Value = 0;
        //    lock (game)
        //    {
        //        game.CarA.Score += AScore;
        //        game.CarB.Score += BScore;
        //    }
        //}



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

        private void button_AReset_Click(object sender, EventArgs e)
        {
            if (game.APauseNum < 3)
            {
                game.AskPause(Camp.CampA);
                buttonPause.Enabled = false;
                buttonEnd.Enabled = false;
                buttonStart.Enabled = true;
                button_AReset.Enabled = false;
                button_BReset.Enabled = false;
            }
        }

        private void button_BReset_Click(object sender, EventArgs e)
        {
            if (game.BPauseNum < 3)
            {
                game.AskPause(Camp.CampB);
                buttonPause.Enabled = false;
                buttonEnd.Enabled = false;
                buttonStart.Enabled = true;
                button_AReset.Enabled = false;
                button_BReset.Enabled = false;
            }
        }

        private void button_set_Click(object sender, EventArgs e)
        {
            lock (flags)
            {
                SetWindow st = new SetWindow(ref flags, ref game, this);
                st.Show();
            }
        }

        //private void numericUpDownScoreA_ValueChanged(object sender, EventArgs e)
        //{
        //    game.AddScore(Camp.CampA, (int)((NumericUpDown)sender).Value);
        //    ((NumericUpDown)sender).Value = 0;
        //}

        //private void numericUpDownScoreB_ValueChanged(object sender, EventArgs e)
        //{
        //    game.AddScore(Camp.CampB, (int)((NumericUpDown)sender).Value);
        //    ((NumericUpDown)sender).Value = 0;
        //}

        private void Tracker_Load(object sender, EventArgs e)
        {
            if (File.Exists("data.txt"))
            {
                FileStream fsRead = new FileStream("data.txt", FileMode.Open);
                int fsLen = (int)fsRead.Length;
                byte[] heByte = new byte[fsLen];
                int r = fsRead.Read(heByte, 0, heByte.Length);
                string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                string[] str = myStr.Split(' ');
                flags.configs.hue0Lower = Convert.ToInt32(str[0]);
                flags.configs.hue0Upper = Convert.ToInt32(str[1]);
                flags.configs.hue1Lower = Convert.ToInt32(str[2]);
                flags.configs.hue1Upper = Convert.ToInt32(str[3]);
                flags.configs.hue2Lower = Convert.ToInt32(str[4]);
                flags.configs.hue2Upper = Convert.ToInt32(str[5]);
                flags.configs.saturation0Lower = Convert.ToInt32(str[6]);
                flags.configs.saturation1Lower = Convert.ToInt32(str[7]);
                flags.configs.saturation2Lower = Convert.ToInt32(str[8]);
                flags.configs.valueLower = Convert.ToInt32(str[9]);
                flags.configs.areaLower = Convert.ToInt32(str[10]);
                fsRead.Close();
            }
        }

        private void button_Continue_Click(object sender, EventArgs e)
        {
            //if (game.state == GameState.End)
            game.NextStage();
            buttonPause.Enabled = false;
            buttonEnd.Enabled = true;
            buttonStart.Enabled = true;
            button_AReset.Enabled = false;
            button_BReset.Enabled = false;
        }

        private void button_AFoul1_Click(object sender, EventArgs e)
        {
            game.AFoul1++;
            game.AddScore(Camp.CampA, Score.Foul1);
            if (game.FoulTimeFS != null)
            {
                byte[] data = Encoding.Default.GetBytes($"A -10 {game.Round}\r\n");
                game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        private void button_AFoul2_Click(object sender, EventArgs e)
        {
            game.AFoul2++;
            game.AddScore(Camp.CampA, Score.Foul2);
            if (game.FoulTimeFS != null)
            {
                byte[] data = Encoding.Default.GetBytes($"A -50 {game.Round}\r\n");
                game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        private void button_BFoul1_Click(object sender, EventArgs e)
        {
            game.BFoul1++;
            game.AddScore(Camp.CampB, Score.Foul1);
            if (game.FoulTimeFS != null)
            {
                byte[] data = Encoding.Default.GetBytes($"B -10 {game.Round}\r\n");
                game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        private void button_BFoul2_Click(object sender, EventArgs e)
        {
            game.BFoul2++;
            game.AddScore(Camp.CampB, Score.Foul2);
            if (game.FoulTimeFS != null)
            {
                byte[] data = Encoding.Default.GetBytes($"B -50 {game.Round}\r\n");
                game.FoulTimeFS.Write(data, 0, data.Length);
            }
        }

        private void label_AMessage_Click(object sender, EventArgs e)
        {
            if (game.State == GameState.Normal)
            {
                game.AddScore(Camp.CampA, Score.BallGetScore);
                game.CarA.BallGetCnt++;
                game.CarA.HaveBall = true;
            }
            else if (game.State == GameState.End)
            {
                if (game.CarA.HaveBall)
                {
                    game.AddScore(Camp.CampA, Score.BallStoreScore);
                    game.CarA.BallOwnCnt++;
                    game.CarA.HaveBall = false;
                }
            }
        }

        private void label_BMessage_Click(object sender, EventArgs e)
        {
            if (game.State == GameState.Normal)
            {
                game.AddScore(Camp.CampB, Score.BallGetScore);
                game.CarB.BallGetCnt++;
                game.CarB.HaveBall = true;
            }
            else if (game.State == GameState.End)
            {
                if (game.CarB.HaveBall)
                {
                    game.AddScore(Camp.CampB, Score.BallStoreScore);
                    game.CarB.BallOwnCnt++;
                    game.CarB.HaveBall = false;
                }
            }
        }

        private void buttonEnd_Click(object sender, EventArgs e)
        {
            game.End();
            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonEnd.Enabled = false;
            button_AReset.Enabled = false;
            button_BReset.Enabled = false;
        }

        ////绘制人员信息
        //private void groupBox_Person_Paint(object sender, PaintEventArgs e)
        //{
        //    Brush br_No_NV = new SolidBrush(Color.Silver);
        //    Brush br_No_V = new SolidBrush(Color.DimGray);
        //    Brush br_A_NV = new SolidBrush(Color.Pink);
        //    Brush br_A_V = new SolidBrush(Color.Red);
        //    Brush br_B_NV = new SolidBrush(Color.SkyBlue);
        //    Brush br_B_V = new SolidBrush(Color.RoyalBlue);
        //    Graphics gra = e.Graphics;
        //    int vbargin = 100;
        //    for(int i = 0;i!=game.CurrPersonNumber;++i)
        //    {
        //        switch(game.People[i].Owner)
        //        {
        //            case Camp.None:
        //                gra.FillEllipse(br_No_V, 40, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_A_NV, 100, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_B_NV, 160, 100 + i * vbargin, 30, 30);
        //                break;
        //            case Camp.CampA:
        //                gra.FillEllipse(br_No_NV, 40, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_A_V, 100, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_B_NV, 160, 100 + i * vbargin, 30, 30);
        //                break;
        //            case Camp.CampB:
        //                gra.FillEllipse(br_No_NV, 40, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_A_NV, 100, 100 + i * vbargin, 30, 30);
        //                gra.FillEllipse(br_B_V, 160, 100 + i * vbargin, 30, 30);
        //                break;
        //            default:break;
        //        }
        //    }
        //}
    }

    public class MyFlags
    {
        public bool showMask;    //调试颜色识别
        public bool running;     //比赛是否正在进行
        public bool calibrated;  //地图是否被校准
        public bool videomode;
        public int clickCount;

        //人员状况：被困、在小车上还未到指定点、到达运送目标点
        public enum PersonState { TRAPPED, INCAR, RESCUED};

        //图像识别参数
        //HSV颜色模型：Hue为色调，Saturation为饱和度，Value为亮度
        public struct LocConfigs
        {
            public int hue0Lower;
            public int hue0Upper;
            public int hue1Lower;
            public int hue1Upper;
            public int hue2Lower;
            public int hue2Upper;
            public int saturation0Lower;
            public int saturation1Lower;
            public int saturation2Lower;
            public int valueLower;
            public int areaLower;
        }
        public LocConfigs configs;

        //三个画面的大小
        public OpenCvSharp.Size showSize;
        public OpenCvSharp.Size cameraSize;
        public OpenCvSharp.Size logicSize;

        //当前小车的坐标
        public Point2i posCarA;
        public Point2i posCarB;

        //防汛物资的坐标
        public Point2i[] posPackages;
        
        //人员起始坐标和待运输的位置坐标
        public Point2i posPersonStart;
        public Point2i posPersonEnd;

        //目前场上被困人员的状况（同一时间场上最多1个被困人员）
        public PersonState personState;

        //比赛状态：未开始、正常进行、暂停、结束
        public GameState gameState;

        public void Init()
        {
            //初始化比赛状况相关数据
            showMask = false;
            running = false;
            calibrated = false;
            videomode = false;
            configs = new LocConfigs();
            posPackages = new Point2i[0];
            posCarA = new Point2i();
            posCarB = new Point2i();

            //以下数据待定，根据实际设备确定
            showSize = new OpenCvSharp.Size(960, 720);
            cameraSize = new OpenCvSharp.Size(1280, 960);
            logicSize = new OpenCvSharp.Size(Game.MAX_SIZE, Game.MAX_SIZE);

            //点击次数（暂不懂什么意思）
            clickCount = 0;

            //初始化人员位置
            posPersonStart = new Point2i();
            posPersonEnd = new Point2i();
        }

        public void Start()
        {
            running = true;
        }

        public void End()
        {
            running = false;
        }
    }

    //坐标转换器：将三种坐标（摄像头坐标、显示坐标、逻辑坐标）上的点坐标进行相互转换
    //摄像头坐标：摄像头直接捕捉到的视频帧对应的坐标
    //显示坐标：界面上的组件大小所决定的显示画面帧对应的坐标
    //逻辑坐标：规则文档中描述的场地大小对应的坐标
    public class CoordinateConverter : IDisposable
    {
        //投影变换中的变换矩阵
        private Mat cam2logic;
        private Mat logic2cam;
        private Mat show2cam;
        private Mat cam2show;
        private Mat show2logic;
        private Mat logic2show;

        //逻辑画面、摄像头画面、显示画面的四个角坐标（顺序依次为左上、右上、左下、右下）
        private Point2f[] logicCorners;
        private Point2f[] camCorners;
        private Point2f[] showCorners;

        //释放托管资源
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)(cam2logic)).Dispose();
                ((IDisposable)(logic2cam)).Dispose();
                ((IDisposable)(show2cam)).Dispose();
                ((IDisposable)(cam2show)).Dispose();
                ((IDisposable)(show2logic)).Dispose();
                ((IDisposable)(logic2show)).Dispose();
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public CoordinateConverter(MyFlags myFlags)
        {
            //相机拍摄的地图
            camCorners = new Point2f[4];
            //内存中存储的逻辑的地图
            logicCorners = new Point2f[4];
            //在屏幕上显示的地图
            showCorners = new Point2f[4];

            cam2logic = new Mat();
            show2cam = new Mat();
            logic2show = new Mat();
            show2logic = new Mat();
            cam2show = new Mat();
            logic2cam = new Mat();

            //逻辑画面四角坐标设置
            logicCorners[0].X = 0;
            logicCorners[0].Y = 0;
            logicCorners[1].X = myFlags.logicSize.Width;
            logicCorners[1].Y = 0;
            logicCorners[2].X = 0;
            logicCorners[2].Y = myFlags.logicSize.Height;
            logicCorners[3].X = myFlags.logicSize.Width;
            logicCorners[3].Y = myFlags.logicSize.Height;

            //显示画面四角坐标设置
            showCorners[0].X = 0;
            showCorners[0].Y = 0;
            showCorners[1].X = myFlags.showSize.Width;
            showCorners[1].Y = 0;
            showCorners[2].X = 0;
            showCorners[2].Y = myFlags.showSize.Height;
            showCorners[3].X = myFlags.showSize.Width;
            showCorners[3].Y = myFlags.showSize.Height;

            //摄像头画面四角坐标设置
            camCorners[0].X = 0;
            camCorners[0].Y = 0;
            camCorners[1].X = myFlags.cameraSize.Width;
            camCorners[1].Y = 0;
            camCorners[2].X = 0;
            camCorners[2].Y = myFlags.cameraSize.Height;
            camCorners[3].X = myFlags.cameraSize.Width;
            camCorners[3].Y = myFlags.cameraSize.Height;

            //通过投影变换函数计算变换矩阵
            show2cam = Cv2.GetPerspectiveTransform(showCorners, camCorners);
            cam2show = Cv2.GetPerspectiveTransform(camCorners, showCorners);
        }

        public void UpdateCorners(Point2f[] corners, MyFlags myFlags)
        {
            if (corners == null) return;
            if (corners.Length != 4) return;
            else showCorners = corners;

            //计算几个变换矩阵
            logic2show = Cv2.GetPerspectiveTransform(logicCorners, showCorners);
            show2logic = Cv2.GetPerspectiveTransform(showCorners, logicCorners);
            //将显示画面投影变换成摄像头画面，同时更新摄像头画面的四个角标
            //（没看懂为什么）
            camCorners = Cv2.PerspectiveTransform(showCorners, show2cam);
            cam2logic = Cv2.GetPerspectiveTransform(camCorners, logicCorners);
            logic2cam = Cv2.GetPerspectiveTransform(logicCorners, camCorners);
            //标记为已校正
            myFlags.calibrated = true;
        }

        //以下为变换函数：输入某一个画面对应的坐标序列，
        //通过透视（投影）矩阵的作用，输出另一个画面对应的坐标序列。
        public Point2f[] ShowToCamera(Point2f[] ptsShow)
        {
            return Cv2.PerspectiveTransform(ptsShow, show2cam);
        }

        public Point2f[] CameraToShow(Point2f[] ptsCamera)
        {
            return Cv2.PerspectiveTransform(ptsCamera, cam2show);
        }

        public Point2f[] CameraToLogic(Point2f[] ptsCamera)
        {
            return Cv2.PerspectiveTransform(ptsCamera, cam2logic);
        }

        public Point2f[] LogicToCamera(Point2f[] ptsLogic)
        {
            return Cv2.PerspectiveTransform(ptsLogic, logic2cam);
        }

        public Point2f[] LogicToShow(Point2f[] ptsLogic)
        {
            return Cv2.PerspectiveTransform(ptsLogic, logic2show);
        }

        public Point2f[] ShowToLogic(Point2f[] ptsShow)
        {
            return Cv2.PerspectiveTransform(ptsShow, show2logic);
        }

        //将flags中人员的起始位置从逻辑坐标转换为摄像头坐标
        public void PeopleFilter(MyFlags flags)
        {
            //如果图像还未被校正，直接返回
            if (!flags.calibrated) return;
            //因为被困人员同一时间在场上只有1个，其实只要计算1个坐标变换
            //但是还是将这1个坐标构造成了坐标点列方便调用已有函数
            Point2f[] res = LogicToCamera(new Point2f[] { flags.posPersonStart });
            //此句存疑（yd）
            flags.posPersonStart = res[0];
        }
    }

    //定位器：进行图像处理，确定位置并且绘图
    public class Localiser
    {
        //依次为小球、车1、车2位置的中心点集
        private List<Point2i> centres0;
        private List<Point2i> centres1;
        private List<Point2i> centres2;

        public Localiser()
        {
            centres0 = new List<Point2i>();
            centres1 = new List<Point2i>();
            centres2 = new List<Point2i>();

        }

        //根据计算得到的中心点集，返回定位到的小车、小球坐标
        //其中，小球坐标返回点数组，小车坐标返回中心点集中的第0个元素
        public void GetLocations(out Point2f[] pts0, out Point2i pt1, out Point2i pt2)
        {
            List<Point2f> ptsList0 = new List<Point2f>();
            if (centres0.Count != 0)
            {
                foreach (Point2i c0 in centres0)
                    ptsList0.Add(c0);
                centres0.Clear();
            }
            // else ptsList0.Add(new Point2f(-1, -1));
            pts0 = ptsList0.ToArray();

            if (centres1.Count != 0)
            {
                pt1 = centres1[0];
                centres1.Clear();
            }
            else pt1 = new Point2i(-1, -1);
            if (centres2.Count != 0)
            {
                pt2 = centres2[0];
                centres2.Clear();
            }
            else pt2 = new Point2i(-1, -1);
        }

        //定位核心代码
        public void Locate(Mat mat, MyFlags localiseFlags)
        {
            if (mat == null || mat.Empty()) return;
            if (localiseFlags == null) return;
            using (Mat hsv = new Mat())
            using (Mat ball = new Mat())
            using (Mat car1 = new Mat())
            using (Mat car2 = new Mat())
            //using (Mat merged = new Mat())
            using (Mat black = new Mat(mat.Size(), MatType.CV_8UC1))
            {
                //颜色空间转化：从RGB转化为HSV
                //hue色调，sat饱和度，value亮度
                Cv2.CvtColor(mat, hsv, ColorConversionCodes.RGB2HSV);
                MyFlags.LocConfigs configs = localiseFlags.configs;

                //二值化：将位于设定区间内的像素点灰度值设置为255，否则为0
                //实现了目标颜色和其他颜色的区分
                //Lower表示范围的最小值，Upper表示范围的最大值

                //针对小球颜色的二值化
                Cv2.InRange(hsv,
                    new Scalar(configs.hue0Lower, configs.saturation0Lower, configs.valueLower),
                    new Scalar(configs.hue0Upper, 255, 255),
                    ball);
                //针对小车1颜色的二值化
                Cv2.InRange(hsv,
                    new Scalar(configs.hue1Lower, configs.saturation1Lower, configs.valueLower),
                    new Scalar(configs.hue1Upper, 255, 255),
                    car1);
                //针对小车2颜色的二值化
                Cv2.InRange(hsv,
                    new Scalar(configs.hue2Lower, configs.saturation2Lower, configs.valueLower),
                    new Scalar(configs.hue2Upper, 255, 255),
                    car2);

                //将二值化图像打印到窗口（蒙版画面，从setwindow中调出）上，以便后续调试
                if (localiseFlags.showMask)
                {
                    Cv2.ImShow("Ball", ball);
                    Cv2.ImShow("CarA", car1);
                    Cv2.ImShow("CarB", car2);
                }
                else
                {
                    Cv2.DestroyAllWindows();
                }

                Point2i[][] contours0, contours1, contours2;

                //图像轮廓识别：根据二值化图象，识别值为255的色块轮廓
                //参数解释：图像矩阵，扫描外围轮廓，只保留轮廓的拐点
                //其返回值为轮廓上的拐点
                contours0 = Cv2.FindContoursAsArray(ball, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                contours1 = Cv2.FindContoursAsArray(car1, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                contours2 = Cv2.FindContoursAsArray(car2, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                //根据拐点的图像矩来计算拐点的中心点坐标
                //小球
                foreach (Point2i[] c0 in contours0)
                {
                    Point2i centre = new Point2i();
                    Moments moments = Cv2.Moments(c0);
                    centre.X = (int)(moments.M10 / moments.M00);
                    centre.Y = (int)(moments.M01 / moments.M00);
                    double area = moments.M00;
                    if (area <= configs.areaLower / 9) continue;
                    centres0.Add(centre);
                }
                //小车1
                foreach (Point2i[] c1 in contours1)
                {
                    Point2i centre = new Point2i();
                    Moments moments = Cv2.Moments(c1);
                    centre.X = (int)(moments.M10 / moments.M00);
                    centre.Y = (int)(moments.M01 / moments.M00);
                    double area = moments.M00;
                    if (area <= configs.areaLower) continue;
                    centres1.Add(centre);
                }
                //小车2
                foreach (Point2i[] c2 in contours2)
                {
                    Point2i centre = new Point2f();
                    Moments moments = Cv2.Moments(c2);
                    centre.X = (int)(moments.M10 / moments.M00);
                    centre.Y = (int)(moments.M01 / moments.M00);
                    double area = moments.M00;
                    if (area <= configs.areaLower) continue;
                    centres2.Add(centre);
                }

                //foreach (Point2i c0 in centres0) Cv2.Circle(mat, c0, 3, new Scalar(0x1b, 0xa7, 0xff), -1);
                //分别在小车1和小车2的位置上绘制圆圈
                foreach (Point2i c1 in centres1) Cv2.Circle(mat, c1, 10, new Scalar(0x3c, 0x14, 0xdc), -1);
                foreach (Point2i c2 in centres2) Cv2.Circle(mat, c2, 10, new Scalar(0xff, 0x00, 0x00), -1);
                if (localiseFlags.gameState != GameState.Unstart)
                {
                    for (int i = 0; i < localiseFlags.currPersonNum; ++i)
                    {
                        int x10 = localiseFlags.posPersonStart[i].X - 8;
                        int y10 = localiseFlags.posPersonStart[i].Y - 8;
                        //在人员起始位置上绘制矩形
                        Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);
                    }
                }

                //Cv2.Merge(new Mat[] { car1, car2, black }, merged);
                //Cv2.ImShow("binary", merged);
            }
        }
    }
}

