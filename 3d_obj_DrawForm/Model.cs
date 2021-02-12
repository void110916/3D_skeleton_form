using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace _3d_obj_DrawForm
{
    class Model
    {
        private static Point3D root{ get { return new Point3D(0, 0, 0); } }

        public Model()
        {
            
            group.Children.Add(getSphereModel(root));
            group.Children.Add(getCylinder(root, new Point3D(0.692024, -0.648617, 0.316857)));
            group.Children.Add(getSphereModel(new Point3D(0.692024, -0.648617, 0.316857)));
            
        }
        public Model3DGroup group = new Model3DGroup();
        public GeometryModel3D getSphereModel(Point3D sphere_center,double diameter=0.1d,int Div=32)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(sphere_center,radius:diameter,thetaDiv:Div,phiDiv: Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh() ,Materials.Gold);
            return mod;
        }
        public GeometryModel3D getCylinder(Point3D cylinderP1,Point3D cylinderP2,double diameter=0.04d,int Div=32)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddCylinder(cylinderP1, cylinderP2, diameter, Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh(), Materials.Gray);
            return mod;
        }
    }
}
