namespace TaleUtil
{
    public class ExecAction : Action
    {
        Delegates.ShallowDelegate action;

        ExecAction() { }

        public ExecAction(Delegates.ShallowDelegate action)
        {
            this.action = action;
        }

        public override Action Clone()
        {
            ExecAction clone = new ExecAction();
            clone.action = action;

            return clone;
        }

        public override bool Run()
        {
            action();
            return true;
        }
    }
}