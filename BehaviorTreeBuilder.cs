using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;

namespace BetterDriver
{
    public class BadBuilderUseException : Exception
    {

    }

    public abstract class BehaviorTreeBuilder<TContext, TBuilder>
        where TBuilder : BehaviorTreeBuilder<TContext, TBuilder>
    {
        protected abstract TBuilder BuilderInstance { get; }
        protected BehaviorTree<TContext> result = new BehaviorTree<TContext>();
        protected TContext context;
        protected Behavior previousNode;
        public BehaviorTree<TContext> End() { return result; }

        public TBuilder Root()
        {
            var root = new RootDecorator();
            result.nodes.Add(root);
            result.root = root;
            previousNode = root;
            return BuilderInstance;
        }
        public TBuilder Selector()
        {
            AddBranch(new Selector());
            return BuilderInstance;
        }
        public TBuilder Sequence()
        {
            AddBranch(new Sequence());
            return BuilderInstance;
        }
        public TBuilder Filter()
        {
            AddBranch(new Filter());
            return BuilderInstance;
        }
        public TBuilder Repeat(int times)
        {
            AddBranch(new RepeatDecorator(times));
            return BuilderInstance;
        }
        public TBuilder MockAction()
        {
            AddLeaf(new MockAction());
            return BuilderInstance;
        }
        public TBuilder Parent()
        {
            previousNode = previousNode.Parent;
            while (previousNode is Decorator dec)
            {
                if (previousNode is RootDecorator) throw new BadBuilderUseException();
                previousNode = previousNode.Parent;
            }
            return BuilderInstance;
        }

        protected void AddBranch(Behavior e)
        {
            if (previousNode is Filter fil)
            {
                fil.AddAction(e);
                result.nodes.Add(e);
                previousNode = e;
            }
            else if (previousNode is Decorator dec)
            {
                dec.SetChild(e);
                result.nodes.Add(e);
                previousNode = e;
            }
            else if (previousNode is Selector sel)
            {
                sel.AddChild(e);
                result.nodes.Add(e);
                previousNode = e;
            }
            else if (previousNode is Sequence seq)
            {
                seq.AddChild(e);
                result.nodes.Add(e);
                previousNode = e;
            }
            else
            {
                throw new BadBuilderUseException();
            }
        }
        protected void AddLeaf(Behavior e)
        {
            if (previousNode is Filter fil)
            {
                if (e is Action) fil.AddAction(e);
                else if (e is Condition) fil.AddCondition(e);
                else throw new BadBuilderUseException();
                result.nodes.Add(e);
            }
            else if (previousNode is Decorator dec)
            {
                dec.SetChild(e);
                Parent();
                result.nodes.Add(e);
            }
            else if (previousNode is Selector sel)
            {
                sel.AddChild(e);
                result.nodes.Add(e);
            }
            else if (previousNode is Sequence seq)
            {
                seq.AddChild(e);
                result.nodes.Add(e);
            }
            else
            {
                throw new BadBuilderUseException();
            }
        }
    }

    public class NpcBehaviorBuilder : BehaviorTreeBuilder<Npc, NpcBehaviorBuilder>
    {
        protected override NpcBehaviorBuilder BuilderInstance => this;
        public NpcBehaviorBuilder (Npc t) { context = t; }
        public NpcBehaviorBuilder CastSkillAction(string n)
        {
            var act = new NpcCastSkillAction(context, n);
            AddLeaf(act);
            return this;
        }
        public NpcBehaviorBuilder IsInFightCondition()
        {
            var cond = new NpcIsInFightCondition(context);
            AddLeaf(cond);
            return this;
        }
        public NpcBehaviorBuilder CanCastSkillCondition(string n)
        {
            var cond = new NpcCanCastSkillCondition(context, n);
            AddLeaf(cond);
            return this;
        }
        public NpcBehaviorBuilder CastFilters(IEnumerable<string> skills)
        {
            foreach (string skill in skills)
            {
                Filter();
                CanCastSkillCondition(skill);
                CastSkillAction(skill);
                Parent();
            }
            return this;
        }
    }
}
