using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EDC21HOST
{
    public class Wall
    {
        public Dot w1;
        public Dot w2;
        public Wall(Dot iw1, Dot iw2)
        {
            w1 = iw1;
            w2 = iw2;
        }
    }

    public class Obstacle
    {
        private int BarNum;
        private Wall[] Bars;

        public void AddBar(Wall[] NewBar)
        {
            int Nums = NewBar.Length;
            Bars = new Wall[Nums];
            Bars = NewBar;
        }
    }
}