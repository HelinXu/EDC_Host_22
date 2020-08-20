using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    public class Flood //泄洪口
    {
        public int num;//泄洪口开启数量
        public Dot dot1;//泄洪口1的位置信息
        public Dot dot2;//泄洪口2的位置信息
        public void ResetIndex() { num=0; }//num复位
        public Flood(int Num) { num=Num; dot1 = new Dot(0, 0); dot2 = new Dot(0, 0); }//构造函数

        //
    }
    
}
