using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    //说明（xhl）：1，目前的规则下，生成package应该是读取地图即可生成。同时还要保证上下半场是不变的。
    //2，最终Game需要读取的是PkgList（其中应该包含24个Package），所以Game啥的可以先写起来。（外界要用的是GetPkgDot）
    //3，具体如何生成，可以等地图写出来再说。
    public class PackageGenerator //存储预备要用的物资信息
    {
        private Dot[] PkgList_Dot;
        private int Package_idx;
        private int PKG_NUM;
        public PackageGenerator(int AMOUNT) //生成指定数量的物资
        {
            Package_idx = 0;
            PKG_NUM = AMOUNT;
            PkgList_Dot = new Dot[PKG_NUM];
            int nextX, nextY;
            Dot dots;
            Random NRand = new Random();
            for (int i = 0; i < PKG_NUM; ++i)
            {

                nextX = NRand.Next(Game.MAZE_CROSS_NUM);//仍然需要改进
                nextY = NRand.Next(Game.MAZE_CROSS_NUM);
                dots = CrossNo2Dot(nextX, nextY);
                PkgList_Dot[i] = dots;
                //需要加上位置是否重合的判断
            }
        }
        //从格点转化为int，传入坐标，返回Dot
        public static Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        {
            int x = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoX;
            int y = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoY;
            Dot temp = new Dot(x, y);
            return temp;
        }
        //返回下标为i的PackageDotArray中的点。开发者：xhl
        public Dot GetPkg_Dot(int i)
        {
            return PkgList_Dot[i];
        }
    }
}
