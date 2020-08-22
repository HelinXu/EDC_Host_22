using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST21
{
    //Package,物资。
    //包括：位置Dot Pos，是否已经被获取bool
    public class Package
    {
        public Dot Pos; //物资生成地点
        public int IsPicked { get; set; } //是否已经被获取.//cyy改成int了，因为bool转换不到byte型，0为没有拾取，1为已拾取
        public Package(Dot aPos)
        {
            Pos = aPos;
            IsPicked = 0;
        }
        //本条不知是否有用
        //public Package() : this(new Dot(0, 0), 0) { }
    }
}
