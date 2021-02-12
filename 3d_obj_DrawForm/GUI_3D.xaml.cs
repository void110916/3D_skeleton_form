using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace _3d_obj_DrawForm
{
    /// <summary>
    /// UserControl1.xaml 的互動邏輯
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
            
            //our_Model = model.group;
        }
Model model = new Model();
        public Model3D our_Model { get; set; }
        private void myViewport_Initialized(object sender, EventArgs e)
        {
            //Model model = new Model();
            //our_Model = model.group;
            
            
        }

        private void m_helix_viewport_Initialized(object sender, EventArgs e)
        {
            m_helix_viewport.Children.Add(new ModelVisual3D { Content = model.group });
        }
    }
}
