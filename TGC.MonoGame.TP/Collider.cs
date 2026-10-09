using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP
{
    internal class Collider
    {
        public ICollisionShape Shape { get; }
        public bool IsTrigger { get; }

        public Collider(ICollisionShape shape, bool isTrigger = false)
        {
            Shape = shape;
            IsTrigger = isTrigger;
        }

        public static readonly Collider None = new Collider(new NullShape(), false);
    }
    
}
