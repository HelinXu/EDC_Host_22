using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
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
}
