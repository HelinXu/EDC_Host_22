﻿using OpenCvSharp;
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
    // 定位器：进行图像处理，确定位置并绘图
    public class Localiser
    {
        //依次为车1、车2位置的中心点集
        private List<Point2i> centres1;
        private List<Point2i> centres2;

        public Localiser()
        {
            centres1 = new List<Point2i>();
            centres2 = new List<Point2i>();
        }

        // 根据计算得到的中心点集，返回定位到的小车坐标
        // 若在相机拍摄的图中没有发现某小车，则该车的坐标返回(-1, -1)
        public void GetCarLocations(out Point2i pt1, out Point2i pt2)
        {
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

        // 定位核心代码
        public void Locate(Mat mat, MyFlags localiseFlags)
        {
            // 如果没有传入摄像机拍摄的画面，则返回
            if (mat == null || mat.Empty()) return;
            // 如果没有指定定位小车的标准，则返回
            if (localiseFlags == null) return;

            // 解释：
            // MatType的组成方式：CV_(位数）+（数据类型）+ C（通道数）
            // 位数：1个像素点在内存中占据的大小（bit），有 8, 16, 32, 64；
            // 数据类型：U(unsigned), S(signed), F(float);
            // 通道数：1（灰度图），2（实数+虚数，用于特殊处理，不常见）
            //         3（RGB彩色图像）,4（带Alpha通道的RGB图像）
            // 可见本上位机采用的图像格式为 “8位深度无符号整数灰度图”

            // using表示在代码作用域结束后自动释放资源
            using (Mat hsv = new Mat())
            using (Mat car1 = new Mat())
            using (Mat car2 = new Mat())

            using (Mat black = new Mat(mat.Size(), MatType.CV_8UC1))
            {
                // 颜色空间转化：将图片从RGB格式转化为HSV格式
                // hue色调，sat饱和度，value亮度
                Cv2.CvtColor(mat, hsv, ColorConversionCodes.RGB2HSV);
                // 为了后面Scalar函数中参数写起来方便
                MyFlags.LocConfigs configs = localiseFlags.configs;

                // 由定义：typedef struct Scalar { 
                //            double val[4]; 
                //        } Scalar;
                // 可见Scalar是由1~4个数据组成的结构体，对应的是图像的1~4个通道的值
                // 可以认为 Scalar 是某个像素点（不一定在图像中）
                // 在下面的代码中，Scalar()中的3个参数分别对应 HSV 图像的 H、S、V 通道

                // InRange函数实现了将图像二值化的功能：
                // 即将设定值区间内像素点灰度值设置为 255（白），否则为 0（黑）
                // 从而完成了目标颜色和其他颜色的区分，即生成蒙版(Mask)
                // Lower表示范围的最小值，Upper表示范围的最大值
                // 注意：只有H,S,V全部位于给定区间内的像素点才会被设为全白

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

                // “显示调试蒙版”选项在SetWindow窗口中可以勾选，勾选后 showmask 为 true
                // 若被勾选，则将二值化图像打印到窗口，以便调试
                if (localiseFlags.showMask)
                {
                    Cv2.ImShow("CarA", car1);
                    Cv2.ImShow("CarB", car2);
                }
                else
                {
                    Cv2.DestroyAllWindows();
                }

                // Contour 意即“轮廓”，contours是双重向量，向量内每个元素保存了一组由连续的Point点构成的向量
                // 每一组Point点集就是一个轮廓。有多少轮廓，向量contours就有多少元素。
                // 下面两个双重向量分别保存两个小车的轮廓拐点坐标
                Point2i[][] contours1, contours2;

                // FindContoursAsArray(图像轮廓识别)：根据二值化图象，识别值为255的色块轮廓
                // 参数解释：1、待处理的图像；2、轮廓检索模式；3、轮廓的近似方法
                // RetrieveModes.External 表示只检测最外围轮廓，忽略内轮廓
                // ContourApproximationModes.ApproxSimple 表示仅保存轮廓的拐点信息，不保存所有轮廓点
                // 返回值：轮廓上的拐点集合
                contours1 = Cv2.FindContoursAsArray(car1, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                contours2 = Cv2.FindContoursAsArray(car2, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                //根据拐点的图像矩来计算拐点的中心点坐标
                //小车1
                foreach (Point2i[] c1 in contours1)
                {
                    Point2i centre = new Point2i();
                    // Moments表示矩，这是一个概率与统计学上的概念
                    Moments moments = Cv2.Moments(c1);
                    // Mij = ∑(r * X^i * Y^j)，其中 r = (x, y)为向量
                    // M00 为面积
                    // 质心(X0, Y0) = (M10 / M00, M01 / M00)
                    // 此处计算出轮廓拐点的质心坐标
                    centre.X = (int)(moments.M10 / moments.M00);
                    centre.Y = (int)(moments.M01 / moments.M00);
                    double area = moments.M00;
                    // 如果计算出的面积太小，则认为是噪声点，不计入统计
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


                // 分别在小车1和小车2的位置上绘制圆圈
                foreach (Point2i c1 in centres1) Cv2.Circle(mat, c1, 10, new Scalar(0x3c, 0x14, 0xdc), -1);
                foreach (Point2i c2 in centres2) Cv2.Circle(mat, c2, 10, new Scalar(0xff, 0x00, 0x00), -1);

                // 如果比赛已经开始
                if (localiseFlags.gameState != GameState.UNSTART)
                {
                    //在人员起始位置上绘制矩形
                    int x10 = localiseFlags.posPsgStart.X - 8;
                    int y10 = localiseFlags.posPsgStart.Y - 8;
                    Cv2.Rectangle(mat, new Rect(x10, y10, 16, 16), new Scalar(0x00, 0xff, 0x00), -1);

                }
                //Cv2.Merge(new Mat[] { car1, car2, black }, merged);
                //Cv2.ImShow("binary", merged);
            }
        }
    }
}
