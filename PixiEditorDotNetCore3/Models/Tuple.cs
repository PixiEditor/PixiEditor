using System;
using System.Collections.Generic;
using System.Text;

namespace PixiEditorDotNetCore3.Models
{
    public class Tuple<T1, T2, T3> : IEquatable<Object>
    {
        public T1 Item1
        {
            get;
            set;
        }

        public T2 Item2
        {
            get;
            set;
        }

        public T3 Item3
        {
            get;
            set;
        }

        public Tuple(T1 Item1, T2 Item2, T3 Item3)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
            this.Item3 = Item3;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || (obj as Tuple<T1, T2, T3>) == null) //if the object is null or the cast fails
                return false;
            else
            {
                Tuple<T1, T2, T3> tuple = (Tuple<T1, T2, T3>)obj;
                return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2) && Item3.Equals(tuple.Item3);
            }
        }

        public override int GetHashCode()
        {
            return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
        }

        public static bool operator ==(Tuple<T1, T2, T3> tuple1, Tuple<T1, T2, T3> tuple2)
        {
            return tuple1.Equals(tuple2);
        }

        public static bool operator !=(Tuple<T1, T2, T3> tuple1, Tuple<T1, T2, T3> tuple2)
        {
            return !tuple1.Equals(tuple2);
        }
    }
}
