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
        Human.Motion motion;
        int pFrame = 0;

        public Model3DGroup m_group = new Model3DGroup();
        Model3DCollection m_collect = new Model3DCollection();
        public Transform3DCollection t_collect;
        public void setTransform(Human.Bone target,int frame)
        {
            target.rotate.Children.Clear();
            target.translate.Children.Clear() ;
            Point3D center;
            int target_posture_idx = Human.Posture.m_pBone.IndexOf(target);
            if (target.parent is null)
                center = root;
            else
                center = target.parent.coordinate_dir;
            if (target.dofrx)
            {
                RotateTransform3D r_x = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), motion.m_pPostures[frame].bone_rotation[target_posture_idx].X), center);
                target.rotate.Children.Add(r_x);
            }
            if (target.dofry)
            {
                RotateTransform3D r_y = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), motion.m_pPostures[frame].bone_rotation[target_posture_idx].Y), center);
                target.rotate.Children.Add(r_y);
            }
            if (target.dofrz)
            {
                RotateTransform3D r_z = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), motion.m_pPostures[frame].bone_rotation[target_posture_idx].Z), center);
                target.rotate.Children.Add(r_z);
            }

            if (target.doftx || target.dofty || target.doftz)
            {
                TranslateTransform3D t = new TranslateTransform3D(motion.m_pPostures[frame].bone_translation[target_posture_idx]);
                target.translate.Children.Add(t);
            }
        }
        public void showFrame(int frame_idx)
        {
Transform3DGroup 
            Task.Factory.StartNew((i)=> { },mo);
        }

        public Model()
        {
            skeleton = new Human.Skeleton(@"C:\Users\void\Downloads\mocapPlayer\07-walk.asf", 0.2f);
            motion = new Human.Motion(@"", 1, skeleton);
            t_collect = new Transform3DCollection(motion.MOV_BONES);
            m_group.Children.Add(getSphereModel(root));

            for (int i = 1; i < skeleton.m_pBoneList.Count; i++)
            {
                skeleton.compute_coordinate_point(skeleton.m_pBoneList[i]);
                m_group.Children.Add(getCylinder(skeleton.m_pBoneList[i].coordinate_dir, skeleton.m_pBoneList[i].parent.coordinate_dir));
                m_group.Children.Add(getSphereModel(skeleton.m_pBoneList[i].coordinate_dir));


            }

            m_collect.Add(m_group);


        }

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
