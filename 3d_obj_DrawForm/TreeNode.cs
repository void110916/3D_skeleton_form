using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Human
{
    public delegate void TreeVisitor<T>(T nodeData);

     class TreeNode<T> where T:Bone 
    {
        public T data;
        private TreeNode<T> parent;
        private LinkedList<TreeNode<T>> children;
        
        public TreeNode(T data)
        {
            this.data = data;

            children = new LinkedList<TreeNode<T>>();
        }

        public void AddChild(TreeNode<T> ChildNode)
        {
            children.AddLast(ChildNode);
            ChildNode.parent = this;

        }
        public void AddParent(TreeNode<T> ParentNode)
        {
            ParentNode.AddChild(this);
        }
        public TreeNode<T> GetChild(T data)
        {
            foreach (TreeNode<T> n in children)

                if (n.data.Equals(data))
                    return n;
            return null;
        }
        public LinkedList<TreeNode<T>> GetChildren { get { return children; } }
        

        public TreeNode<T> GetParent { get { return parent; } }

        public void Traverse(TreeNode<T> node, TreeVisitor<T> visitor)
        {
            visitor(node.data);
            foreach (TreeNode<T> kid in node.children)
                Traverse(kid, visitor);
        }
    }

}