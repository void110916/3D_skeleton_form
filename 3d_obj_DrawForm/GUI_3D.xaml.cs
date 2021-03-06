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


        }
        Model model = new Model();

        private void m_helix_viewport_Initialized(object sender, EventArgs e)
        {
            var visual = new ModelVisual3D { Content = model.m_group };
            
            m_helix_viewport.Children.Add(visual);
            m_helix_viewport.CameraMode = CameraMode.Inspect;
            var zoomBound = model.m_group.Bounds;
            
            m_helix_viewport.ZoomExtents(zoomBound);
            CompositionTarget.Rendering += viewport_render;
            
            //m_helix_viewport.Camera.Position = new Point3D(6d, 9d, 15d);
        }
        public void viewport_render(object sender,EventArgs e)
        {
            
        }
    }
}
