using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public void setTransform(int frame)
        {
            //Point3D center;
            foreach (var x in skeleton.m_pBoneList)
            {
                if (!x.data.frame_idx.HasValue)
                    return;
                Transform3DGroup translate = new Transform3DGroup();
                Transform3DGroup rotate = new Transform3DGroup();
                //x.data.translate.Children.Clear();
                //x.data.rotate.Children.Clear();
                Point3D center = x.GetParent?.data.coordinate_dir ?? root;


                int target_posture_idx = x.data.frame_idx.Value;

                if (x.data.dofrx)
                {
                    RotateTransform3D r_x = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), motion.m_pPostures[frame].bone_rotation[target_posture_idx].X), center);
                    rotate.Children.Add(r_x);
                }
                if (x.data.dofry)
                {
                    RotateTransform3D r_y = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), motion.m_pPostures[frame].bone_rotation[target_posture_idx].Y), center);
                    rotate.Children.Add(r_y);
                }
                if (x.data.dofrz)
                {
                    RotateTransform3D r_z = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), motion.m_pPostures[frame].bone_rotation[target_posture_idx].Z), center);
                    rotate.Children.Add(r_z);
                }

                if (x.data.doftx || x.data.dofty || x.data.doftz)
                {
                    TranslateTransform3D t = new TranslateTransform3D(motion.m_pPostures[frame].bone_translation[target_posture_idx]);
                    translate.Children.Add(t);
                }
                x.data.translate = translate;
                x.data.rotate = rotate;
            }
            for (int i = 0; i < skeleton.NUM_BONES_IN_FILE; i++)
            {
                var parent = skeleton.m_pBoneList[i];
                parent.data.transform.Children.Add(parent.data.transform);
                parent.data.transform.Children.Add(parent.data.rotate);
                foreach (var child in parent.GetChildren)
                {
                    child.data.transform.Children.Add(parent.data.translate);
                    child.data.transform.Children.Add(parent.data.rotate);
                }
            }
            foreach (var x in skeleton.m_pBoneList)
            {
                x.data.model.Transform = x.data.transform;
            }


        }
        //public void showFrame(int frame_idx)
        //{
        //    Transform3DGroup
        //    Task.Factory.StartNew((i) => { }, mo);
        //}

        public Model()
        {
            skeleton = new Human.Skeleton(@"..\..\..\test data\07-walk.asf", 0.2f);
            motion = new Human.Motion(@"..\..\..\test data\07_05-walk.amc", 1, skeleton);

            m_group.Children.Add(getSphereModel(root));
            setTransform(200);
            for (int i = 1; i < skeleton.m_pBoneList.Count; i++)
            {
                skeleton.compute_coordinate_point(skeleton.m_pBoneList[i]);
                skeleton.m_pBoneList[i].data.model.Children.Add(getSphereModel(skeleton.m_pBoneList[i].data.coordinate_dir, transform: skeleton.m_pBoneList[i].data.transform));
                skeleton.m_pBoneList[i].data.model.Children.Add(getCylinder(skeleton.m_pBoneList[i].data.coordinate_dir, skeleton.m_pBoneList[i].GetParent.data.coordinate_dir, transform: skeleton.m_pBoneList[i].data.transform));
                skeleton.m_pBoneList[i].data.model.Transform = skeleton.m_pBoneList[i].data.transform;
                m_group.Children.Add(skeleton.m_pBoneList[i].data.model);
            }




        }

        public GeometryModel3D getSphereModel(Point3D sphere_center, double diameter = 0.1d, int Div = 32, Transform3D transform = null)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(sphere_center, radius: diameter, thetaDiv: Div, phiDiv: Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh(), Materials.Gold);
            mod.Transform = transform;
            return mod;
        }
        public GeometryModel3D getCylinder(Point3D cylinderP1, Point3D cylinderP2, double diameter = 0.04d, int Div = 32, Transform3D transform = null)
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddCylinder(cylinderP1, cylinderP2, diameter, Div);
            GeometryModel3D mod = new GeometryModel3D(builder.ToMesh(), Materials.Gray);
            //mod.SetName();
            mod.Transform = transform;
            return mod;
        }
    }
}
