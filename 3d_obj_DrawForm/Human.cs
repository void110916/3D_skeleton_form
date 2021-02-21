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
    class Posture
    {
        /// <summary>
        /// Root position (x, y, z)
        /// </summary>
        public Vector3D root_pos;
        /// <summary>
        /// Euler angles (thetax, thetay, thetaz) of all bones in their local coordinate system.
        /// If a particular bone does not have a certain degree of freedom, 
        /// the corresponding rotation is set to 0.
        /// The order of the bones in the array corresponds to their ids in .ASf file: root, lhipjoint, lfemur, ...
        /// </summary>
        public Vector3D[] bone_rotation;
        /// <summary>
        /// bones that are translated relative to parents (resulting in gaps) (rarely used)
        /// </summary>
        public Vector3D[] bone_translation;
        /// <summary>
        /// bones that change length during the motion (rarely used)
        /// </summary>
        public Vector3D[] bone_length;
        public Posture(int MAX_BONE_NUM)
        {
            bone_rotation = new Vector3D[MAX_BONE_NUM];
            bone_length = new Vector3D[MAX_BONE_NUM];
            bone_translation = new Vector3D[MAX_BONE_NUM];
            root_pos = new Vector3D(0f, 0f, 0f);

        }
    }
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
        public float rx, ry, rz;
        public string name;
        /// <summary>
        ///rotation matrix from the local coordinate of this bone to the local coordinate system of it's parent
        ///Rotation angles for this bone at a particular time frame (as read from AMC file) in local coordinate system, 
        ///they are set in the setPosture function before display function is called
        /// </summary>
        //public double[,] rot_parent_current;

        /// <summary>
        /// bone's length
        /// </summary>
        //public double tl;
        public int[] dofo;

        public Bone()
        {
            this.dof = 0;
            this.dofrx = this.dofry = this.dofrz = false;
            this.doftx = this.dofty = this.doftz = false;
            this.doftl = false;
            this.dir = new Vector3();
            this.dofo = new int[8];
        }
        public void initialize()
        {
            this.dof = 0;
            this.dofrx = this.dofry = this.dofrz = false;
            this.doftx = this.dofty = this.doftz = false;
            this.doftl = false;
            this.dir = new Vector3();
            this.dofo = new int[8];

        }
    }

    class Skeleton : Exception
    {
        public Bone rootBone { get { return m_pBoneList[0]; } }
        //root position in world coordinate system
        //  start
        //private double[] m_RootPos = new double[3];
        public Vector3D transmit;
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
        public void enableAllRotationalDOFS()
        {
            for (int i = 0; i < m_pBoneList.Count; i++)
            {
                if (m_pBoneList[i].dof == 0)
                    continue;
                if (!m_pBoneList[i].dofrx)
                {
                    m_pBoneList[i].dofrx = true;
                    m_pBoneList[i].rx = 0f;
                    m_pBoneList[i].dof++;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof - 1] = 2;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof] = 0;
                }
                if (!m_pBoneList[i].dofry)
                {
                    m_pBoneList[i].dofry = true;
                    m_pBoneList[i].ry = 0.0f;
                    m_pBoneList[i].dof++;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof - 1] = 2;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof] = 0;
                }

                if (!m_pBoneList[i].dofrz)
                {
                    m_pBoneList[i].dofrz = true;
                    m_pBoneList[i].rz = 0.0f;
                    m_pBoneList[i].dof++;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof - 1] = 3;
                    m_pBoneList[i].dofo[m_pBoneList[i].dof] = 0;
                }
            }
        }
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
                        bone.dof = 0;
                        foreach (string item in block)
                        {
                            if (item == "rx")
                            {
                                bone.dofrx = true;
                                bone.dofo[bone.dof] = 1;
                            }
                            else if (item == "ry")
                            {
                                bone.dofry = true;
                                bone.dofo[bone.dof] = 2;
                            }
                            else if (item == "rz")
                            {
                                bone.dofrz = true;
                                bone.dofo[bone.dof] = 3;
                            }
                            else if (item == "tx")
                            {
                                bone.doftx = true;
                                bone.dofo[bone.dof] = 4;
                            }
                            else if (item == "ty")
                            {
                                bone.dofty = true;
                                bone.dofo[bone.dof] = 5;
                            }
                            else if (item == "tz")
                            {
                                bone.doftz = true;
                                bone.dofo[bone.dof] = 6;
                            }
                            else if (item == "l")
                            {
                                bone.doftl = true;
                                bone.dofo[bone.dof] = 7;
                            }
                            else if (item == "dof")
                            {
                                continue;
                            }
                            bone.dof++;
                            bone.dofo[bone.dof] = 0;
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
        }
        /// <summary>
        /// This function sets rot_parent_current data member.
        /// Rotation from this bone local coordinate system 
        /// to the coordinate system of its parent
        /// </summary>
        /// <param name="child"></param>
        public void compute_coordinate_point(Bone child)
        {
            Vector3 vector;

            var parent_of_child = child.parent;
            vector = Vector3.Add(parent_of_child.dir * parent_of_child.length, child.dir * child.length);
            while (parent_of_child.parent != null)
            {
                parent_of_child = parent_of_child.parent;
                vector = Vector3.Add(parent_of_child.dir * parent_of_child.length, vector);
            }
            child.coordinate_dir = new Point3D(vector.X, vector.Y, vector.Z);
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
        protected int m_NumFrames;
        Skeleton pSkeleton;
        List<Posture> m_pPostures;
        protected void readAMCfile(string AMCname, float scale)
        {
            List<Bone> bone;
            bone = pSkeleton.m_pBoneList;
            int n = 0;
            int numbones = pSkeleton.m_pBoneList.Count;
            int movbones = numbones - 2;
            StreamReader f = new StreamReader(AMCname, Encoding.UTF8);
            while (!f.EndOfStream)
            {
                var line = f.ReadLine();
                //if (f.EndOfStream) break;
                if (!line.Equals(""))
                    n++;

            }
            f.Close();
            n = (n - 3) / (movbones + 1);
            m_NumFrames = n;
            m_pPostures = new List<Posture>(m_NumFrames);
            f = new StreamReader(AMCname, Encoding.UTF8);
            while (true)
            {
                var line = f.ReadLine();
                if (line.Contains(":FORCE-ALL-JOINTS-BE-3DOF"))
                    pSkeleton.enableAllRotationalDOFS();
                if (line.Contains(":DEGREES"))
                    break;
            }
            for (int i = 0; i < m_NumFrames; i++)
            {
                Posture pose = new Posture(pSkeleton.m_pBoneList.Count);
                int frameNum = int.Parse(f.ReadLine());
                for (int j = 0; j < movbones; j++)
                {
                    var line = f.ReadLine().Split();
                    int bone_idx = pSkeleton.m_pBoneList.FindIndex(b => b.name == line[0]);

                    pose.bone_rotation[bone_idx].X = pose.bone_rotation[bone_idx].Y = pose.bone_rotation[bone_idx].Z = 0;
                    for (int x = 0; x < pSkeleton.m_pBoneList[bone_idx].dof; x++)
                    {
                        float tmp = float.Parse(line[x + 1]);
                        switch (bone[bone_idx].dofo[x])
                        {
                            case 0:
                                x = bone[bone_idx].dof;
                                break;
                            case 1:
                                pose.bone_rotation[bone_idx].X = tmp;
                                break;
                            case 2:
                                pose.bone_rotation[bone_idx].Y = tmp;
                                break;
                            case 3:
                                pose.bone_rotation[bone_idx].Z = tmp;
                                break;
                            case 4:
                                pose.bone_translation[bone_idx].X = tmp * scale;
                                break;
                            case 5:
                                pose.bone_translation[bone_idx].Y = tmp * scale;
                                break;
                            case 6:
                                pose.bone_translation[bone_idx].Z = tmp * scale;
                                break;
                            case 7:
                                pose.bone_length[bone_idx].X = tmp;
                                break;
                        }

                    }
                    if (line.Contains("root"))
                    {
                        pose.root_pos.X = pose.bone_rotation[0].X;
                        pose.root_pos.Y = pose.bone_rotation[0].Y;
                        pose.root_pos.Z = pose.bone_rotation[0].Z;
                    }
                }
                m_pPostures.Add(pose);
            }
            f.Close();
        }
        public Motion(int numFrames_, Skeleton pSkeleton_)
        {
            pSkeleton = pSkeleton_;
            m_NumFrames = numFrames_;
            m_pPostures = new List<Posture>(m_NumFrames);
            SetPosturesToDefault();
        }
        public Motion(string amc_fileName, float scale, Skeleton pSkeleton_)
        {
            if (!File.Exists(amc_fileName))
            {
                throw new FileNotFoundException("file no found", amc_fileName);
            }
            pSkeleton = pSkeleton_;
            m_NumFrames = 0;
            m_pPostures = null;
            readAMCfile(amc_fileName, scale);
        }
        void SetPosturesToDefault()
        {
            for (int frame = 0; frame < m_NumFrames; frame++)
            {
                m_pPostures[frame].root_pos.X = 0f;
                m_pPostures[frame].root_pos.Y = 0f;
                m_pPostures[frame].root_pos.Z = 0f;
                for (int j = 0; j < pSkeleton.m_pBoneList.Count; j++)
                {
                    m_pPostures[frame].bone_rotation[j].X = 0f;
                    m_pPostures[frame].bone_rotation[j].Y = 0f;
                    m_pPostures[frame].bone_rotation[j].Z = 0f;
                }

            }
        }
    }
}
