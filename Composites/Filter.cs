namespace BetterDriver
{
    public class Filter : Sequence
    {
        public void AddCondition(Behavior condition) { Children.Insert(0, condition); }
        public void AddAction(Behavior action) { Children.Add(action); }
    }
}
