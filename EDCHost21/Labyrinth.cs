using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    public class Labyrinth
    {
        public int mWallNum;
        public Wall[] mpWallList;

        public Labyrinth(int iNum, Wall[] iList)
        {
            mWallNum = iNum;
            mpWallList = new Wall[iNum];
            for (int i = 0; i < iNum; i++)
            {
                mpWallList[i] = new Wall(iList[i]);
            }
        }

    }
}
