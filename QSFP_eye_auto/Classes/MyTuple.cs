using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSFP_eye_auto.Classes
{
    public class MyTuple
    {
        public int x { get; set; }
        public int y { get; set; }


        public static MyTuple operator +(MyTuple t1, MyTuple t2) =>
            new MyTuple { x = (t1.x + t2.x), y = (t1.y + t2.y) };

        public static MyTuple operator -(MyTuple t1, MyTuple t2) =>
            new MyTuple { x = (t1.x - t2.x), y = (t1.y - t2.y) };

        public static MyTuple operator *(MyTuple t, int mul) =>
            new MyTuple { x = (t.x * mul), y = (t.y * mul) };

        public static MyTuple operator *(int mul, MyTuple t) =>
            new MyTuple { x = (t.x * mul), y = (t.y * mul) };

        public static MyTuple operator *(double mul, MyTuple t)=>
            new MyTuple { x = (int)(t.x * mul), y = (int)(t.y * mul) };

        public static MyTuple operator /(MyTuple t, int div)=>
            new MyTuple { x = (t.x / div), y = (t.y / div) };

        public static MyTuple operator /(MyTuple t, double div) =>
            new MyTuple { x = (int)(t.x / div), y = (int)(t.y / div) };

    }
}
