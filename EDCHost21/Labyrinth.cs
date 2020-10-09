using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST22
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

        //Laby. 构造函数 从文本读取
        public Labyrinth(TextReader reader)
        {
            mWallNum = 8;
            mpWallList = new Wall[mWallNum];
            for (int i = 0; i < mWallNum; i++)
            {
                string text = reader.ReadLine();
                string[] bits = text.Split(' ');
                int x1 = int.Parse(bits[0]);
                int y1 = int.Parse(bits[1]);
                int x2 = int.Parse(bits[2]);
                int y2 = int.Parse(bits[3]);
                mpWallList[i] = new Wall(new Dot(x1, y1), new Dot(x2, y2));
            }
            Debug.WriteLine("Labyrinth Created from text.");
        }
    }
}
