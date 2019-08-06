﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterDriver
{
    public class BadBuilderUseException : Exception
    {
        private string message;
        public BadBuilderUseException(string m) => message = m;
        public override string Message => message;
    }

    public abstract class BehaviorTreeBuilder<TBuilder> : IBehaviorTreeBuilder
        where TBuilder : BehaviorTreeBuilder<TBuilder>
    {
        protected class BuilderNode
        {
            public BuilderNode(Behavior b) { behavior = b; }
            public Behavior behavior { get; private set; }
            public BuilderNode Parent = null;
            public List<BuilderNode> Children = new List<BuilderNode>();
        }

        protected abstract TBuilder BuilderInstance { get; }
        protected BehaviorTree result = new BehaviorTree();
        protected BuilderNode currentNode;

        public BehaviorTree Build() { return result; }

        public TBuilder Root()
        {
            var root = new InfiniteDecorator();
            currentNode = new BuilderNode(root);
            currentNode.Parent = null;
            result.AddBehavior(root);
            result.root = root;
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
        public TBuilder Action(Action a)
        {
            AddLeaf(a);
            return BuilderInstance;
        }
        public TBuilder Condition(Condition c)
        {
            AddLeaf(c);
            return BuilderInstance;
        }
        public TBuilder End()
        {
            currentNode = currentNode.Parent;
            if (currentNode == null) return BuilderInstance;
            while (currentNode.behavior is Decorator dec)
            {
                if (currentNode.Parent == null) return BuilderInstance;
                currentNode = currentNode.Parent;
            }
            return BuilderInstance;
        }

        protected void AddBranch(Behavior e)
        {
            var newNode = new BuilderNode(e);
            newNode.Parent = currentNode;
            currentNode.Children.Add(newNode);
            result.Subscribe(e, currentNode.behavior);
            if (currentNode.behavior is Filter fil)
            {
                fil.AddAction(e);
            }
            else if (currentNode.behavior is Decorator dec)
            {
                dec.SetChild(e);
            }
            else if (currentNode.behavior is Selector sel)
            {
                sel.AddChild(e);
            }
            else if (currentNode.behavior is Sequence seq)
            {
                seq.AddChild(e);
            }
            else
            {
                throw new BadBuilderUseException($"Cannot add branch to node of type {e.GetType()}.");
            }
            currentNode = newNode;
            result.AddBehavior(e);
        }
        protected void AddLeaf(Behavior e)
        {
            result.Subscribe(e, currentNode.behavior);
            if (currentNode.behavior is Filter fil)
            {
                if (e is Action) fil.AddAction(e);
                else if (e is Condition) fil.AddCondition(e);
                else throw new BadBuilderUseException($"Cannot add behavior of type {e.GetType()} to filter.");
            }
            else if (currentNode.behavior is Decorator dec)
            {
                dec.SetChild(e);
                End();
            }
            else if (currentNode.behavior is Selector sel)
            {
                sel.AddChild(e);
            }
            else if (currentNode.behavior is Sequence seq)
            {
                seq.AddChild(e);
            }
            else
            {
                throw new BadBuilderUseException($"Cannot add leaf to node of type {e.GetType()}.");
            }
            result.AddBehavior(e);
        }
    }
}