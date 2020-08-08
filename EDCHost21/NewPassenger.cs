using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace EDC21HOST
{
    public class Passenger//救援人员位置信息
    {
        public Dot startpos;
        public Dot finalpos;
        public Passenger(Dot startDot, Dot finalDot)
        {
            startpos = startDot;
            finalpos = finalDot;
        }
        public Passenger() : this(new Dot(0,0), new Dot(0,0) { }

        public void ResetInfo(Dot startDot, Dot finalDot)//重新生成位置
        {
            startpos = startDot;
            finalpos = finalDot;
        }
    }
    public class PassengerGenerator //存储预备要用的物资信息
    {
        private Dot[] PassengerstartDotArray;
        private Dot[] PassengerfinalDotArray;
        private int Passengernum;
        private int Passenger_idx;
        public PersonGenerator(int amount) //生成指定数量的人员
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
                    nextX1 = NRand.Next(Game.MazeCrossNum);//仍然需要改进Game.MazeCrossNum是上界
                    nextY1 = NRand.Next(Game.MazeCrossNum);
                    dot1 = CrossNo2Dot(nextX1, nextY1);
                    PackagestartDotArray[i] = dot1;
                    nextX2 = NRand.Next(Game.MazeCrossNum);//仍然需要改进Game.MazeCrossNum是上界
                    nextY2 = NRand.Next(Game.MazeCrossNum);
                    dot2 = CrossNo2Dot(nextX2, nextY2);
                    PackagefinalDotArray[i] = dot2;
                    if(nextX1==nextX2 && nextY1==nextY2)
                    {
                        same = 0;
                    }
                } while (same == 0); 
            }
        }
        //返回下一个人员的坐标
        public Dot Next()
        {
            Dot temp;
            temp = PackageDotArray[Passenger_idx++];
            return temp;
        }
        public void ResetIndex() { Passenger_idx = 0; } //package_idx复位
        public Dot CrossNo2Dot(int CrossNoX, int CrossNoY)
        {
            Dot temp;
            temp.x = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoX;
            temp.y = Game.MazeBorderPoint1 + Game.MazeCrossDist / 2 + Game.MazeCrossDist * CrossNoY;
            return temp;
        }
    }
}