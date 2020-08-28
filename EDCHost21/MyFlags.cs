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
    public class MyFlags
    {
        public bool showMask;    // 调试颜色识别
        public bool running;     // 比赛是否正在进行
        public bool calibrated;  // 地图是否被校准
        public bool videomode;
        public int clickCount;   // 画面被点击的次数

        // 图像识别参数
        // HSV颜色模型：Hue为色调，Saturation为饱和度，Value为亮度
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
            // 小车面积的最小值，低于这个值的检测对象会被过滤掉
            public int areaLower;
        }
        public LocConfigs configs;

        // 三个画面的大小
        public OpenCvSharp.Size showSize;
        public OpenCvSharp.Size cameraSize;
        public OpenCvSharp.Size logicSize;

        // 小车是否在场上
        public bool CarAInField;
        public bool CarBInField;

        // 在场上的小车是否在迷宫中
        public bool IsInMaze;

        // 当前小车的坐标
        public Point2i posCarA;
        public Point2i posCarB;

        // 防汛物资的坐标
        public Point2i[] posPackages;

        // 人员起始坐标和待运输的位置坐标
        public Point2i posPsgStart;
        public Point2i posPsgEnd;

        // 目前场上被困人员的状况（同一时间场上最多1个被困人员）
        public PassengerState psgState;

        // 比赛状态：未开始、正常进行、暂停、结束
        public GameState gameState;

        public void Init()
        {
            // 初始化比赛状况相关数据
            showMask = false;
            running = false;
            calibrated = false;
            videomode = false;
            configs = new LocConfigs();
            posPackages = new Point2i[0];
            posCarA = new Point2i();
            posCarB = new Point2i();


            // 设置3张地图的大小
            // 以下数据待定，根据实际设备确定
            showSize = new OpenCvSharp.Size(960, 720);
            cameraSize = new OpenCvSharp.Size(1280, 960);
            logicSize = new OpenCvSharp.Size(Game.MAX_SIZE_CM, Game.MAX_SIZE_CM);

            // 点击次数（暂不懂什么意思）
            clickCount = 0;

            // 初始化人员位置
            posPsgStart = new Point2i();
            posPsgEnd = new Point2i();
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

}
