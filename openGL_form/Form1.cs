using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Windows.Forms.Integration;





namespace openGL_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _3d_obj_DrawForm.UserControl1 uc =
               new _3d_obj_DrawForm.UserControl1();


            //uc.UserControl1_resize(this.Size.Height, this.Size.Height);
        }
        ElementHost host = new ElementHost();
        public void opengl_ini()
        {
            
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            ModelVisual3D model = new ModelVisual3D();
            MeshGeometry3D mesh = new MeshGeometry3D();
            mesh.Clone();

        }
        ElementHost h = new ElementHost();
        private void Form1_Load(object sender, EventArgs e)
        {
            //ElementHost host = new ElementHost();
            //_3d_obj_DrawForm.UserControl1 uc =
            //    new _3d_obj_DrawForm.UserControl1();

            //host.Child = uc;


            //this.Controls.Add(host);
            _3d_obj_DrawForm.UserControl1 uc =
               new _3d_obj_DrawForm.UserControl1();
           

            //uc.UserControl1_resize(this.Size.Height, this.Size.Height);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            _3d_obj_DrawForm.UserControl1 uc =
                new _3d_obj_DrawForm.UserControl1();
            //uc.Width = host.Size.Height;
            //uc.Height = host.Size.Height;
            
            
        }
    }
}
