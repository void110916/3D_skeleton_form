using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Windows.Media.Media3D;
namespace Human
{
    class Bone
    {

        /// <summary>
        /// the sibling (branch bone) in the hierarchy tree
        /// </summary>
        public Bone parent;
        public Bone sibling;
        public Bone child; // Pointer to the child (outboard bone) in the hierarchy tree 
        public int idx; // Bone index
        public Vector3 dir; // Unit vector describes the direction from local origin to 
                            // the origin of the child bone 
                            // Notice: stored in local coordinate system of the bone
        public Point3D coordinate_dir;
        public float length; // bone length
        public double angle_x, angle_y, angle_z; // orientation of each bone's local coordinate 
                                                 // system as specified in ASF file (axis field)
        public double aspx, aspy; // aspect ratio of bone shape
        public int dof; // number of bone's degrees of freedom 
        public bool dofrx, dofry, dofrz; // rotational degree of freedom mask in x, y, z axis 
        public bool doftx, dofty, doftz; // translational degree of freedom mask in x, y, z axis
        public bool doftl;
        // dofrx=1 if this bone has x rotational degree of freedom, otherwise dofrx=0.

        public string name;
        public int[] dofo;

        public void initialize()
        {
            this.dof = 0;
            this.dofrx = this.dofry = this.dofrz = false;
            this.doftx = this.dofty = this.doftz = false;
            this.doftl = false;
            this.dir = new Vector3(0f);
            this.dofo = new int[8];

        }
    }

    class Skeleton : Exception
    {
        public Bone rootBone { get { return m_pBoneList[0]; } }
        //root position in world coordinate system
        //  start
        //private double[] m_RootPos = new double[3];
        public Vector3 transmit;
        //private double rx, ry, rz;
        //  end
        /// <summary>
        /// Array with all skeleton bones
        /// </summary>
        public List<Bone> m_pBoneList;
        /// <summary>
        /// to the root bone
        /// </summary>
        //private Bone m_pRootBone;
        public int NUM_BONES_IN_FILE = 0;
        public static int rootBoneIndex { get { return 0; } }
        /// <summary>
        /// set the skeleton's pose based on the given posture
        /// </summary>
        /// <param name="posture"></param>
        //public void setPosture(Posture posture)
        //{
        //    //m_RootPos[0] = posture.root_pos.X;
        //    //m_RootPos[1] = posture.root_pos.Y;
        //    //m_RootPos[2] = posture.root_pos.Z;
        //    for (int i = 0; i < m_pBoneList.Count; i++)
        //    {
        //        // if the bone has rotational degree of freedom in x direction
        //        if (m_pBoneList[i].dofrx)
        //            m_pBoneList[i].rx = posture.bone_rotation[m_pBoneList[i].idx].X;
        //        if (m_pBoneList[i].doftx)
        //            m_pBoneList[i].tx = posture.bone_translation[m_pBoneList[i].idx].X;

        //        // if the bone has rotational degree of freedom in y direction
        //        if (m_pBoneList[i].dofry)
        //            m_pBoneList[i].ry = posture.bone_rotation[m_pBoneList[i].idx].Y;
        //        if (m_pBoneList[i].dofty)
        //            m_pBoneList[i].ty = posture.bone_translation[m_pBoneList[i].idx].Y;

        //        // if the bone has rotational degree of freedom in z direction
        //        if (m_pBoneList[i].dofrz)
        //            m_pBoneList[i].rz = posture.bone_rotation[m_pBoneList[i].idx].Z;
        //        if (m_pBoneList[i].dofty)
        //            m_pBoneList[i].tz = posture.bone_translation[m_pBoneList[i].idx].Z;

        //        if (m_pBoneList[i].doftl)
        //            m_pBoneList[i].tl = posture.bone_length[m_pBoneList[i].idx].X;
        //    }
        //}
        public void set_bone_shape(Bone[] bones)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].aspx = 0.25;
                bones[i].aspy = 0.25;
            }

            bones[rootBoneIndex].aspx = 1;
            bones[rootBoneIndex].aspy = 1;
        }
        private void readASFFile(string asf_filename, float scale)
        {
            StreamReader f = new StreamReader(asf_filename, Encoding.UTF8);
            string line;
            do
            {
                line = f.ReadLine();
            } while (!line.Equals(":bonedata"));
            bool done = false;
            //List<Bone> temp_bone_list = new List<Bone>(20);
            //temp_bone_list.Add(m_pBoneList[0]);
            for (int i = 1; (!done); i++)
            {
                NUM_BONES_IN_FILE++;

                Bone bone = new Bone();
                bone.initialize();
                while (true)
                {
                    line = f.ReadLine().Trim();
                    string[] block = line.Split(' ');
                    if (block[0] == "end")
                    {
                        m_pBoneList.Add(bone);
                        break;
                    }
                    if (block[0] == ":hierarchy")
                    {
                        NUM_BONES_IN_FILE--;
                        done = true;
                        break;
                    }
                    if (block[0] == "id")
                    {
                        bone.idx = int.Parse(block[1]) - 1;
                    }
                    if (block[0] == "name")
                    {
                        bone.name = block[1];
                    }
                    if (block[0] == "direction")
                    {
                        Vector3 dir = new Vector3(float.Parse(block[1]), float.Parse(block[2]), float.Parse(block[3]));
                        bone.dir = dir;
                    }
                    if (block[0] == "length")
                    {
                        bone.length = float.Parse(block[1]) * scale;
                    }
                    if (block[0] == "axis")
                    {
                        bone.angle_x = double.Parse(block[1]);
                        bone.angle_y = double.Parse(block[2]);
                        bone.angle_z = double.Parse(block[3]);
                    }
                    if (block[0] == "dof")
                    {
                        bone.dof = block.Length - 1;
                        foreach (string item in block)
                        {
                            if (item == "rx")
                                bone.dofrx = true;
                            else if (item == "ry")
                                bone.dofry = true;
                            else if (item == "rz")
                                bone.dofrz = true;
                            else if (item == "tx")
                                bone.doftx = true;
                            else if (item == "ty")
                                bone.dofty = true;
                            else if (item == "tz")
                                bone.doftz = true;
                            else if (item == "l")
                                bone.doftl = true;
                        }
                    }
                }

            }
            //m_pBoneList = temp_bone_list;
            f.ReadLine();//skip "begin" of hierarchy
            while (true)
            {
                line = f.ReadLine().Trim();
                if (line == "end")
                    break;
                else
                {
                    var block = line.Split(' ');
                    Bone parent = m_pBoneList.Find(b => b.name == block[0]);
                    //Bone parent = Array.Find(m_pBoneList, b => b.name == block[0]);
                    for (int k = 1; k < block.Length; k++)
                    {
                        Bone child = m_pBoneList.Find(b => b.name == block[k]);
                        //Bone child = Array.Find(m_pBoneList, b => b.name == block[k]);
                        child.parent = parent;
                        if (parent.child is null)
                            parent.child = child;
                        else
                        {
                            Bone sibling = parent.child;

                            while (sibling.sibling != null)
                            {
                                sibling = sibling.sibling;
                            }
                            sibling.sibling = child;
                            sibling.parent = parent;
                        }

                    }
                }
            }
            f.Close();
            for (int i = 0; i < m_pBoneList.Count; i++)
            {
                compute_coordinate_point(m_pBoneList[i]);
            }
        }
        /// <summary>
        /// This function sets rot_parent_current data member.
        /// Rotation from this bone local coordinate system 
        /// to the coordinate system of its parent
        /// </summary>
        /// <param name="child"></param>
        private void compute_coordinate_point(Bone child)
        {
            Vector3 vector;
            if (child.parent is null)
            {
                vector = child.dir * child.length;
            }
            else
            {
                Bone parent_of_child = child.parent;
                vector = Vector3.Add(parent_of_child.dir * parent_of_child.length, child.dir * child.length);
                while (parent_of_child.parent != null)
                {
                    parent_of_child = parent_of_child.parent;
                    vector = Vector3.Add(parent_of_child.dir * parent_of_child.length, vector);
                }
            }


            child.coordinate_dir.X = vector.X;
            child.coordinate_dir.Y = vector.Y;
            child.coordinate_dir.Z = vector.Z;
        }

        /// <summary>
        /// build a human skeleton using ASF file.
        /// </summary>
        /// <param name="asf_filename">cccc</param>
        /// <param name="scale">default 0.06 make skeleton 1.7m in height</param>
        public Skeleton(string asf_filename, float scale = 0.06f)
        {
            if (!File.Exists(asf_filename))
            {
                throw new FileNotFoundException("file no found", asf_filename);
            }

            m_pBoneList = new List<Bone>(20);
            int[] dofo = new int[8];
            dofo[0] = 4;
            dofo[1] = 5;
            dofo[2] = 6;
            dofo[3] = 1;
            dofo[4] = 2;
            dofo[5] = 3;
            dofo[6] = 0;
            Bone root = new Bone();
            root.initialize();
            root.dofo = dofo;
            //Initialization
            root.idx = rootBoneIndex;
            root.parent = null;
            root.child = null;
            Vector3 dir = new Vector3(0f, 0f, 0f);
            root.dir = dir;
            root.angle_x = 0d; root.angle_y = 0d; root.angle_z = 0d;
            root.length = 0.05f;
            root.dof = 6;
            root.dofrx = root.dofry = root.dofrz = true;
            root.doftx = root.dofty = root.doftz = true;
            root.doftl = false;
            //m_RootPos[0] = m_RootPos[1] = m_RootPos[2] = 0;
            root.name = "root";
            m_pBoneList.Add(root);
            //	m_NumDOFs=6;
            //tx = ty = tz = rx = ry = rz = 0.0;
            // build hierarchy and read in each bone's DOF information
            readASFFile(asf_filename, scale);
        }
        Bone getBone() { return this.rootBone; }
        ~Skeleton()
        {

        }
    }

    class Motion
    {

    }
}
