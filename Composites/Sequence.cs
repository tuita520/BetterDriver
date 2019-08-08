namespace BetterDriver
{
    public class Sequence : Composite
    {
        public override void OnChildCompleted(ISchedulable sender)
        {
            var s = sender.Status;
            if (s == NodeStatus.SUCCESS)
            {
                if (++CurrentIndex >= Children.Count)
                {
                    Status = NodeStatus.SUCCESS;
                    OnComplete(this);
                }
                else
                {
                    var child = Children[CurrentIndex];
                    child.Enter();
                }
            }
            else
            {
                Status = s;
                OnComplete(this);
            }
        }
    }
}
