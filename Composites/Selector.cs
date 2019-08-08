namespace BetterDriver
{
    public class Selector : Composite
    {
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS)
            {
                Status = s;
                OnComplete(this);
            }
            else
            {
                if (++CurrentIndex >= Children.Count)
                {
                    Status = NodeStatus.FAILURE;
                    OnComplete(this);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    child.Enter();
                }
            }
        }
    }
}
