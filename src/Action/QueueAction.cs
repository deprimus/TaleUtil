using System.Collections.Generic;

namespace TaleUtil
{
    public class QueueAction : Action
    {
        LinkedList<Action> actions;

        QueueAction() { }

        public QueueAction(Action[] actions)
        {
            this.actions = new LinkedList<Action>();

            for(int i = actions.Length - 1; i >= 0; --i)
            {
                this.actions.AddFirst(actions[i]);

                // Remove the action from the big Tale queue, because it will be added to this internal queue.
                Queue.RemoveLast(actions[i]);
            }
        }

        public override Action Clone()
        {
            QueueAction clone = new QueueAction();
            clone.actions = new LinkedList<Action>();

            LinkedListNode<Action> node = actions.First;

            while (node != null)
            {
                clone.actions.AddLast(node.Value.Clone());
                node = node.Next;
            }

            return clone;
        }

        public override bool Run()
        {
            LinkedListNode<Action> node = actions.First;

            if(node == null)
                return true;

            if(node.Value.Run())
                actions.RemoveFirst();

            return actions.Count == 0;
        }
    }
}