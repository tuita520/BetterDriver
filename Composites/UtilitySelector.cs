using System;
using System.Collections.Generic;
using System.Text;

namespace BetterDriver.Composites
{
    class UtilitySelector : Selector
    {
        public override void Enter()
        {
            Clear();
            Children.Sort((x, y) => (int)(x.Utility - y.Utility));
            var child = Children[CurrentIndex];
            child.Enter();
        }
    }
}
