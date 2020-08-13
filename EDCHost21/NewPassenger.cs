using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace EDCHOST21
{

    public class PassengerGenerator //存储预备要用的物资信息
    {
        private Dot[] PassengerstartDotArray;
        private Dot[] PassengerfinalDotArray;
        private int Passengernum;
        private int Passenger_idx;
        public PassengerGenerator(int amount) //生成指定数量的人员
        {
            Passenger_idx = 0;
            Passengernum = amount;
            PassengerstartDotArray = new Dot[Passengernum];
            PassengerfinalDotArray = new Dot[Passengernum];
            int nextX1, nextY1;
            int nextX2, nextY2;
            Dot dot1;
            Dot dot2;
            int same;
            Random NRand = new Random();
            for (int i = 0; i < Passengernum; ++i)
            {
                do
                {
                    same = 1;
                    nextX1 = NRand.Next(Game.MAZE_CROSS_NUM);//仍然需要改进Game.MazeCrossNum是上界
                    nextY1 = NRand.Next(Game.MAZE_CROSS_NUM);
                    dot1 = CrossNo2Dot(nextX1, nextY1);
                    PassengerstartDotArray[i] = dot1;
                    nextX2 = NRand.Next(Game.MAZE_CROSS_NUM);//仍然需要改进Game.MazeCrossNum是上界
                    nextY2 = NRand.Next(Game.MAZE_CROSS_NUM);
                    dot2 = CrossNo2Dot(nextX2, nextY2);
                    PassengerfinalDotArray[i] = dot2;
                    if(nextX1==nextX2 && nextY1==nextY2)
                    {
                        same = 0;
                    }
                                                             //需要加上人员与障碍是否重合的判断

                } while (same == 0); 
            }

        }
        //返回下一个人员的坐标
        public Passenger Next()
        {
            Dot startpos = PassengerstartDotArray[Passenger_idx];
            Dot finalpos = PassengerfinalDotArray[Passenger_idx];
            Passenger temp = new Passenger(startpos, finalpos);   //xhl不确定C#语法是不是这样的
            Passenger_idx = Passenger_idx + 1;
            return temp;
        }
        public void ResetIndex() { Passenger_idx = 0; } //package_idx复位
        public Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        {
            Dot temp;
            temp.x = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoX;
            temp.y = Game.MazeBorderPoint1 + Game.MAZE_CROSS_DIST / 2 + Game.MAZE_CROSS_DIST * CrossNoY;
            return temp;
        }
    }
}