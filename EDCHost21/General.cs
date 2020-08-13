using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    //还没用上：这些全局函数可以用吗？如果能写成全局函数会比较好
    //商榷：这部分内容可能应该写在Game流程类里
    public static class General
    {
        //从格点转化为int，传入坐标，返回Dot
        public static Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        {
            int x = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoX;
            int y = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoY;
            Dot temp = new Dot(x, y);
            return temp;
        }
    }

}
