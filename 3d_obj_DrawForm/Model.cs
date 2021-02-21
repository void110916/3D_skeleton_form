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
        private static Point3D root { get { return new Point3D(0, 0, 0); } }
        Human.Skeleton skeleton;

        public Model()
        {
            skeleton = new Human.Skeleton(@"C:\Users\void\Downloads\mocapPlayer\07-walk.asf", 0.2f);
            group.Children.Add(getSphereModel(root));
            for (int i = 1; i < skeleton.m_pBoneList.Count; i++)
            {
                skeleton.compute_coordinate_point(skeleton.m_pBoneList[i]);
                group.Children.Add(getCylinder(skeleton.m_pBoneList[i].coordinate_dir, skeleton.m_pBoneList[i].parent.coordinate_dir));
                group.Children.Add(getSphereModel(skeleton.m_pBoneList[i].coordinate_dir));
            }


        }
        public Model3DGroup group = new Model3DGroup();
        public GeometryModel3D getSphereModel(Point3D sphere_center, double diameter = 0.1d, int Div = 32)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(sphere_center, radius: diameter, thetaDiv: Div, phiDiv: Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh(), Materials.Gold);
            return mod;
        }
        public GeometryModel3D getCylinder(Point3D cylinderP1, Point3D cylinderP2, double diameter = 0.04d, int Div = 32)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddCylinder(cylinderP1, cylinderP2, diameter, Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh(), Materials.Gray);
            //mod.SetName();
            return mod;
        }
    }
}
