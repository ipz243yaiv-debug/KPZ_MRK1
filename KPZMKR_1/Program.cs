using System;
using System.Collections.Generic;
using System.Text;

namespace task5
{
    abstract class LightNode
    {
        public string Render()
        {
            OnCreated();
            string html = OuterHTML();
            OnRendered();
            return html;
        }

        protected virtual void OnCreated() { }
        protected virtual void OnRendered() { }

        public abstract string OuterHTML();
        public abstract string InnerHTML();
    }

    class LightTextNode : LightNode
    {
        private string _text;
        public LightTextNode(string text) => _text = text;

        public override string OuterHTML() => _text;
        public override string InnerHTML() => _text;
    }

    class LightElementNode : LightNode
    {
        public string TagName { get; }
        public string DisplayType { get; }
        public bool IsSingle { get; }
        public List<string> CssClasses { get; } = new List<string>();
        private List<LightNode> _children = new List<LightNode>();

        public LightElementNode(string tagName, string displayType, bool isSingle)
        {
            TagName = tagName;
            DisplayType = displayType;
            IsSingle = isSingle;
        }

        public void AddChild(LightNode node) => _children.Add(node);

        public IEnumerable<LightNode> GetDepthFirst()
        {
            yield return this;
            foreach (var child in _children)
            {
                if (child is LightElementNode element)
                {
                    foreach (var n in element.GetDepthFirst()) yield return n;
                }
                else
                {
                    yield return child;
                }
            }
        }

        public IEnumerable<LightNode> GetBreadthFirst()
        {
            Queue<LightNode> queue = new Queue<LightNode>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                if (current is LightElementNode element)
                {
                    foreach (var child in element._children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        protected override void OnCreated()
        {
            Console.WriteLine($"[Hook] Створено елемент: {TagName}");
        }

        protected override void OnRendered()
        {
            Console.WriteLine($"[Hook] Зрендерено елемент: {TagName}");
        }

        public override string InnerHTML()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in _children)
            {
                sb.Append(child.Render());
            }
            return sb.ToString();
        }

        public override string OuterHTML()
        {
            StringBuilder sb = new StringBuilder();
            string classes = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";

            if (IsSingle)
            {
                sb.Append($"<{TagName}{classes}/>");
            }
            else
            {
                sb.Append($"<{TagName}{classes}>");
                sb.Append(InnerHTML());
                sb.Append($"</{TagName}>");
            }
            return sb.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            LightElementNode list = new LightElementNode("ul", "block", false);
            list.CssClasses.Add("main-list");

            for (int i = 1; i <= 3; i++)
            {
                LightElementNode item = new LightElementNode("li", "block", false);
                item.CssClasses.Add("list-item");
                item.AddChild(new LightTextNode($"Елемент списку №{i}"));
                list.AddChild(item);
            }

            LightElementNode img = new LightElementNode("img", "inline", true);
            img.CssClasses.Add("avatar");
            list.AddChild(img);

            Console.WriteLine("Ітератор в глибину ");
            foreach (var node in list.GetDepthFirst())
            {
                if (node is LightElementNode el) Console.WriteLine(el.TagName);
                if (node is LightTextNode) Console.WriteLine("text");
            }

            Console.WriteLine("\nІтератор в ширину");
            foreach (var node in list.GetBreadthFirst())
            {
                if (node is LightElementNode el) Console.WriteLine(el.TagName);
                if (node is LightTextNode) Console.WriteLine("text");
            }
        }
    }
}