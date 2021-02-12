using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace Human
{
    struct Posture
    {
        /// <summary>
        /// Root position (x, y, z)
        /// </summary>
        public Vector3 root_pos;
        /// <summary>
        /// Euler angles (thetax, thetay, thetaz) of all bones in their local coordinate system.
        /// If a particular bone does not have a certain degree of freedom, 
        /// the corresponding rotation is set to 0.
        /// The order of the bones in the array corresponds to their ids in .ASf file: root, lhipjoint, lfemur, ...
        /// </summary>
        public Vector3[] bone_rotation;
        /// <summary>
        /// bones that are translated relative to parents (resulting in gaps) (rarely used)
        /// </summary>
        public Vector3[] bone_translation;
        /// <summary>
        /// bones that change length during the motion (rarely used)
        /// </summary>
        public Vector3[] bone_length;
    }

    struct Bone
    {
        private object _sibling;
        /// <summary>
        /// the sibling (branch bone) in the hierarchy tree
        /// </summary>
        public object parent { get { return (Bone)this._sibling; } set { this._sibling = value; } }
        public List<Bone> children; // Pointer to the child (outboard bone) in the hierarchy tree 
        public int idx; // Bone index
        public double[] dir; // Unit vector describes the direction from local origin to 
                             // the origin of the child bone 
                             // Notice: stored in local coordinate system of the bone

        public double length; // bone length
        public double axis_x, axis_y, axis_z; // orientation of each bone's local coordinate 
                                              // system as specified in ASF file (axis field)
        public double aspx, aspy; // aspect ratio of bone shape
        public int dof; // number of bone's degrees of freedom 
        public bool dofrx, dofry, dofrz; // rotational degree of freedom mask in x, y, z axis 
        public bool doftx, dofty, doftz; // translational degree of freedom mask in x, y, z axis
        public bool doftl;
        // dofrx=1 if this bone has x rotational degree of freedom, otherwise dofrx=0.

        public string name;
        /// <summary>
        ///rotation matrix from the local coordinate of this bone to the local coordinate system of it's parent
        ///Rotation angles for this bone at a particular time frame (as read from AMC file) in local coordinate system, 
        ///they are set in the setPosture function before display function is called
        /// </summary>
        public double[,] rot_parent_current;
        public double rx, ry, rz;
        public double tx, ty, tz;
        /// <summary>
        /// bone's length
        /// </summary>
        public double tl;
        public int[] dofo;
        public Bone(Bone Sibling)
        {
            this._sibling = Sibling;
            this.children = new List<Bone>();
            this.idx = -1;
            this.dir = new double[3];
            this.length = -1;
            this.axis_x = this.axis_y = this.axis_z = 0;
            this.aspx = this.aspy = 0;
            this.dof = 0;
            this.dofrx = this.dofry = this.dofrz = false;
            this.doftx = this.dofty = this.doftz = false;
            this.doftl = false;
            this.name = "";
            this.rot_parent_current = new double[4, 4];
            this.rx = this.ry = this.rz = 0;
            this.tx = this.ty = this.tz = 0;
            this.tl = 0;
            this.dofo = new int[8];
        }
        public void initialize()
        {
            this.dof = 0;
            this.dofrx = this.dofry = this.dofrz = false;
            this.doftx = this.dofty = this.doftz = false;
            this.doftl = false;
            this.dir = new double[3];
            this.dofo = new int[8];
            this.children = new List<Bone>();
        }
        //public Bone shallowCopy()
        //{
        //    return MemberwiseClone() as Bone;
        //}
    }

    class Skeleton : Exception
    {
        private Bone rootBone;
        //root position in world coordinate system
        //  start
        private double[] m_RootPos = new double[3];
        private double tx, ty, tz;
        private double rx, ry, rz;
        //  end
        /// <summary>
        /// Array with all skeleton bones
        /// </summary>
        public Bone[] m_pBoneList = new Bone[256];
        /// <summary>
        /// to the root bone
        /// </summary>
        private Bone m_pRootBone;
        private int NUM_BONES_IN_FILE = 0;
        private int MOV_BONES_IN_FILE = 0;
        public static int rootBoneIndex { get { return 0; } }
        /// <summary>
        /// set the skeleton's pose based on the given posture
        /// </summary>
        /// <param name="posture"></param>
        public void setPosture(Posture posture)
        {
            m_RootPos[0] = posture.root_pos.X;
            m_RootPos[1] = posture.root_pos.Y;
            m_RootPos[2] = posture.root_pos.Z;
            for (int i = 0; i < m_pBoneList.Length; i++)
            {
                // if the bone has rotational degree of freedom in x direction
                if (m_pBoneList[i].dofrx)
                    m_pBoneList[i].rx = posture.bone_rotation[m_pBoneList[i].idx].X;
                if (m_pBoneList[i].doftx)
                    m_pBoneList[i].tx = posture.bone_translation[m_pBoneList[i].idx].X;

                // if the bone has rotational degree of freedom in y direction
                if (m_pBoneList[i].dofry)
                    m_pBoneList[i].ry = posture.bone_rotation[m_pBoneList[i].idx].Y;
                if (m_pBoneList[i].dofty)
                    m_pBoneList[i].ty = posture.bone_translation[m_pBoneList[i].idx].Y;

                // if the bone has rotational degree of freedom in z direction
                if (m_pBoneList[i].dofrz)
                    m_pBoneList[i].rz = posture.bone_rotation[m_pBoneList[i].idx].Z;
                if (m_pBoneList[i].dofty)
                    m_pBoneList[i].tz = posture.bone_translation[m_pBoneList[i].idx].Z;

                if (m_pBoneList[i].doftl)
                    m_pBoneList[i].tl = posture.bone_length[m_pBoneList[i].idx].X;
            }
        }
        public void set_bone_shape(Bone[] bones)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].aspx = 0.25;
                bones[i].aspx = 0.25;
                bones[i].aspy = 0.25;
            }
            bones[rootBoneIndex].aspx = 1;
            bones[rootBoneIndex].aspy = 1;


        }
        private void readASFFile(string asf_filename, double scale)
        {
            StreamReader f = new StreamReader(asf_filename, Encoding.UTF8);
            string line;
            do
            {
                line = f.ReadLine();
            } while (!line.Equals(":bonedata"));
            bool done = false;
            for (int i = 1; (!done) && (i < m_pBoneList.Length); i++)
            {
                NUM_BONES_IN_FILE++;
                MOV_BONES_IN_FILE++;
                m_pBoneList[i].initialize();
                while (true)
                {
                    line = f.ReadLine();
                    string[] block = line.Trim().Split(' ');
                    if ((block[0] == "end") || (block[0] == "begin"))
                        break;
                    if (block[0] == ":hierarchy")
                    {
                        MOV_BONES_IN_FILE--;
                        NUM_BONES_IN_FILE--;
                        done = true;
                        break;
                    }
                    if (block[0] == "id")
                    {
                        m_pBoneList[i].idx = int.Parse(block[1]) - 1;
                    }
                    if (block[0] == "name")
                    {
                        m_pBoneList[i].name = block[1];
                    }
                    if (block[0] == "direction")
                    {
                        double[] dir = new double[3];
                        dir[0] = double.Parse(block[1]);
                        dir[1] = double.Parse(block[2]);
                        dir[2] = double.Parse(block[3]);
                        m_pBoneList[i].dir = dir;
                    }
                    if (block[0] == "length")
                    {
                        m_pBoneList[i].length = double.Parse(block[1]) * scale;
                    }
                    if (block[0] == "axis")
                    {
                        m_pBoneList[i].axis_x = double.Parse(block[1]);
                        m_pBoneList[i].axis_y = double.Parse(block[2]);
                        m_pBoneList[i].axis_z = double.Parse(block[3]);
                    }
                    if (block[0] == "dof")
                    {
                        m_pBoneList[i].dof = block.Length - 1;
                        foreach (string item in block)
                        {
                            if (item == "rx")
                                m_pBoneList[i].dofrx = true;
                            else if (item == "ry")
                                m_pBoneList[i].dofry = true;
                            else if (item == "rz")
                                m_pBoneList[i].dofrz = true;
                            else if (item == "tx")
                                m_pBoneList[i].doftx = true;
                            else if (item == "ty")
                                m_pBoneList[i].dofty = true;
                            else if (item == "tz")
                                m_pBoneList[i].doftz = true;
                            else if (item == "l")
                                m_pBoneList[i].doftl = true;
                        }
                    }
                }
            }
            f.ReadLine();//skip "begin" of hierarchy
            while (true)
            {
                line = f.ReadLine().Trim();
                if (line == "end")
                    break;
                else
                {
                    var block = line.Split(' ');
                    Bone parent = Array.Find(m_pBoneList, b => b.name == block[0]);
                    for (int k = 1; k < block.Length; k++)
                    {

                        Bone child = Array.Find(m_pBoneList, b => b.name == block[k]);
                        child.parent = parent;
                        parent.children.Add(child);
                    }
                }
            }
            f.Close();
        }
        /// <summary>
        /// This function sets rot_parent_current data member.
        /// Rotation from this bone local coordinate system 
        /// to the coordinate system of its parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void compute_rotation_parent_child(Bone parent, Bone child)
        {

        }

        /// <summary>
        /// build a human skeleton using ASF file.
        /// </summary>
        /// <param name="asf_filename">cccc</param>
        /// <param name="scale">default 0.06 make skeleton 1.7m in height</param>
        public Skeleton(string asf_filename, double scale = 0.06)
        {
            if (!File.Exists(asf_filename))
            {
                throw new FileNotFoundException("file no found", asf_filename);
            }

            m_pBoneList = new Bone[256];
            int[] dofo = new int[8];
            dofo[0] = 4;
            dofo[1] = 5;
            dofo[2] = 6;
            dofo[3] = 1;
            dofo[4] = 2;
            dofo[5] = 3;
            dofo[6] = 0;
            m_pBoneList[0].dofo = dofo;
            //Initialization
            m_pBoneList[0].idx = rootBoneIndex;
            m_pRootBone = m_pBoneList[0];
            m_pBoneList[0].parent = null;
            m_pBoneList[0].children = new List<Bone>();
            double[] dir = new double[3] { 0d, 0d, 0d };
            m_pBoneList[0].dir = dir;
            m_pBoneList[0].axis_x = 0d; m_pBoneList[0].axis_y = 0d; m_pBoneList[0].axis_z = 0d;
            m_pBoneList[0].length = 0.05;
            m_pBoneList[0].dof = 6;
            m_pBoneList[0].dofrx = m_pBoneList[0].dofry = m_pBoneList[0].dofrz = true;
            m_pBoneList[0].doftx = m_pBoneList[0].dofty = m_pBoneList[0].doftz = true;
            m_pBoneList[0].doftl = false;
            m_RootPos[0] = m_RootPos[1] = m_RootPos[2] = 0;
            m_pBoneList[0].name = "root";
            //	m_NumDOFs=6;
            tx = ty = tz = rx = ry = rz = 0.0;
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
