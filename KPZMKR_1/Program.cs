using System;
using System.Collections.Generic;
using System.Text;

namespace task5
{
    interface IVisitor
    {
        void Visit(LightElementNode element);
        void Visit(LightTextNode text);
    }

    class StatisticsVisitor : IVisitor
    {
        public int ElementsCount { get; private set; }
        public void Visit(LightElementNode element) => ElementsCount++;
        public void Visit(LightTextNode text) { }
    }

    abstract class LightNode
    {
        public abstract void Accept(IVisitor visitor);

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

        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        public override string OuterHTML() => _text;
        public override string InnerHTML() => _text;
    }

    class LightElementNode : LightNode
    {
        public string TagName { get; }
        public List<string> CssClasses { get; } = new List<string>();
        private List<LightNode> _children = new List<LightNode>();

        public LightElementNode(string tagName) => TagName = tagName;

        public void AddChild(LightNode node) => _children.Add(node);

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in _children) child.Accept(visitor);
        }

        public IEnumerable<LightNode> GetDepthFirst()
        {
            yield return this;
            foreach (var child in _children)
            {
                if (child is LightElementNode el)
                {
                    foreach (var n in el.GetDepthFirst()) yield return n;
                }
                else
                {
                    yield return child;
                }
            }
        }

        protected override void OnCreated() => Console.WriteLine($"[Log] Початок рендерингу <{TagName}>");
        protected override void OnRendered() => Console.WriteLine($"[Log] Завершено рендеринг <{TagName}>");

        public override string InnerHTML()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in _children) sb.Append(child.Render());
            return sb.ToString();
        }

        public override string OuterHTML()
        {
            string classes = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";
            return $"<{TagName}{classes}>{InnerHTML()}</{TagName}>";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var list = new LightElementNode("ul");
            var item1 = new LightElementNode("li");
            item1.AddChild(new LightTextNode("Елемент 1"));
            var item2 = new LightElementNode("li");
            item2.AddChild(new LightTextNode("Елемент 2"));

            list.AddChild(item1);
            list.AddChild(item2);

            var stats = new StatisticsVisitor();
            list.Accept(stats);

            Console.WriteLine($"Статистика");
            Console.WriteLine($"Кількість HTML-тегів: {stats.ElementsCount}");

            Console.WriteLine($"\n Ітерація (DFS)");
            foreach (var node in list.GetDepthFirst())
            {
                if (node is LightElementNode el) Console.WriteLine($"Знайдено тег: {el.TagName}");
            }

            Console.WriteLine($"\nРендеринг");
            Console.WriteLine(list.Render());
        }
    }
}