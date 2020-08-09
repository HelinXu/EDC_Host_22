using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EDC21HOST
{
    public struct Dot //点
    {
        public int x;
        public int y;
        public Dot(int _x, int _y) { x = _x; y = _y; }
        public static bool operator == (Dot a, Dot b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator != (Dot a, Dot b)
        {
            return !(a == b);
        }
    }
    public class Package
    {
        public Dot Pos; //物资生成地点
        public int Number { get; set; } //编号
        public Package(Dot startDot, int number)
        {
            Pos = startDot;
            Number = number;
        }
        public Package() : this(new Dot(0, 0), 0) { }

        /*public void ResetInfo(Dot startDot, int number)//重新生成位置
        {
            Pos = startDot;
            Number = number;
        }*/
    }
    public class PackageGenerator //存储预备要用的物资信息
    {
        private Dot[] PackageDotArray;
        private int Package_idx;
        private int Packagenum;
        public void PackageGenerator(int amount) //生成指定数量的人员
        {
            Package_idx = 0;
            Packagenum = amount;
            PackageDotArray = new Dot[Packagenum];
            int nextX, nextY;
            Dot dots;
            Random NRand = new Random();
            for (int i = 0; i < Packagenum; ++i)
            {

                nextX = NRand.Next(Game.MazeCrossNum);//仍然需要改进
                nextY = NRand.Next(Game.MazeCrossNum);
                dots = CrossNo2Dot(nextX, nextY);
                PackageDotArray[i] = dots;
                                                       //需要加上位置是否重合的判断
            }
        }
        //返回下一个人员的坐标
        /*public Dot Next(Package [] currentPackage)
        {
            Dot temp;
            bool exist;
            do
            {
                temp = PackageDotArray[Package_idx++];
                exist = false;
                for (int i = 0; i < Game.MaxPackageNum; ++i)
                    if (temp == currentPackage[i].Pos)
                        exist = true;
            }
            while (exist && Package_idx < Packagenum);
            return temp;
        }*/
        //public void ResetIndex() { Package_idx = 0; } //package_idx复位
        public Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        {
            Dot temp;
            temp.x = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoX;
            temp.y = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoY;
            return temp;
        }
    }
}