using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    public class Stop //泄洪口
    {
        public int num;//泄洪口开启数量
        public Dot dot1;//泄洪口1的位置信息
        public Dot dot2;//泄洪口2的位置信息
    }
    public void ResetIndex() { num=0; }//num复位
    public Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
    {
        Dot temp;
        temp.x = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoX;
        temp.y = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoY;
        return temp;
    }
}
