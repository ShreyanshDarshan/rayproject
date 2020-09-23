using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class OctreeArray : MonoBehaviour
{
    public float size = 10.0f;
    public int headindex = 0;

    public struct Square
    {
        public Vector3 pos;
        public float w;

        public bool isInside(Vector3 point)
        {
            return
                (point.x >= pos.x - w &&
                    point.x < pos.x + w &&
                    point.y >= pos.y - w &&
                    point.y < pos.y + w &&
                    point.z >= pos.z - w &&
                    point.z < pos.z + w
                );
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct tempstruct
    {
        Vector3 pos;    // 3 * 4
        float w;    // 4
        float numpoints;    // 4
        unsafe fixed int child[8];  // 8 * 4
        int level;  // 4
        bool divided;   // 1
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe public struct Tree
    {
        public Square boundary;
        public int numpoints;
        unsafe public fixed int child[8];     //lower sw, se, ne, sw | upper sw, se, ne, sw
        public int level;
        public int divided;

        public Tree(int level_, Vector3 pos, float w)
        {
            divided = 0;
            level = level_;
            boundary = new Square();
            boundary.pos = pos;
            boundary.w = w;
            numpoints = 0;
            //child = new int[8];
            for (int i = 0; i < 8; i++)
            {
                child[i] = -1;
            }
        }

        public void subdivide(Tree[] AllNodes, ref int headind)
        {
            if (level > 1)
            {
                Vector3[] centers = new Vector3[8];
                int index = 0;
                for (int k = -1; k <= 1; k += 2)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        for (int i = -1; i <= 1; i += 2)
                        {
                            centers[index] = new Vector3(boundary.pos.x + i * boundary.w / 2.0f, boundary.pos.y + j * boundary.w / 2.0f, boundary.pos.z + k * boundary.w / 2.0f);
                            index++;
                        }
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    AllNodes[headind] = ( new Tree(level - 1, centers[i], boundary.w / 2.0f) );
                    child[i] = headind;
                    headind++;
                    //Debug.Log("headindex in function: " + headind);
                }
                divided = 1;
            }
        }

        public void insert(Vector3 point, Tree[] AllNodes, ref int headind)
        {
            if (!boundary.isInside(point))
            {
                return;
            }


            if (level > 1)
            {
                if (divided == 0)
                {
                    subdivide(AllNodes, ref headind);
                }

                for (int i = 0; i < 8; i++)
                {
                    AllNodes[child[i]].insert(point, AllNodes, ref headind);
                }
            }
            this.numpoints++;
            //Debug.Log("inserting" + numpoints);
        }
    }

    public Tree[] AllNodes = new Tree[1623385];

    unsafe void show(Tree[] AllNodes, int index)
    {
        if (AllNodes[index].numpoints > 0)
        {
            //Debug.Log("entered");
            float S = 2 * AllNodes[index].boundary.w;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = AllNodes[index].boundary.pos;
            cube.transform.localScale = new Vector3(S, S, S);
            cube.GetComponent<MeshRenderer>().enabled = false;
            cube.GetComponent<BoxCollider>().enabled = false;
        }

        if (AllNodes[index].level > 1 && AllNodes[index].divided == 1)
        {
            for (int i = 0; i < 8; i++)
            {
                show(AllNodes, AllNodes[index].child[i]);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AllNodes[headindex] = (new Tree(9, new Vector3(size / 2, size / 2, size / 2), size / 2));
        headindex++;
        //Tree noctreee = new Tree(1, new Vector3(size / 2, size / 2, size / 2), size / 2);
        //AllNodes.Add(noctreee);
        for (float i = 0; i < 6; i += 0.05f)
        {
            for (float j = 0; j < 6; j += 0.05f)
            {
                for (float k = 0; k < 6; k += 0.05f)
                {
                    Vector3 pt = new Vector3(i, j, k);
                    if (Vector3.Distance(pt, new Vector3(3, 3, 3)) < 3 && Vector3.Distance(pt, new Vector3(3, 3, 3)) > 2)
                    {
                        AllNodes[0].insert(pt, AllNodes, ref headindex);
                    }
                }
            }
        }
        Debug.Log("allnodes before " + AllNodes[0].numpoints);
        
        //AllNodes[0].insert(new Vector3(4.1f, 5.1f, 2.1f), AllNodes, ref headindex);
        //AllNodes[0].insert(new Vector3(4.1f, 2.1f, 2.1f), AllNodes, ref headindex);
        //for (int i = 0; i < 8; i++)
        //{
        //    for (int j = 0; j < 8; j++)
        //    {
        //        Debug.Log(((ot.child[i]).child[j]).numpoints);
        //    }
        //}
        Debug.Log("allnodes after " + AllNodes[0].numpoints);
        //show(AllNodes, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("headindex: " + headindex);
        Debug.Log("sizeof: " + 214985*Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf(typeof(Tree)));
        //Debug.Log("Sizeoftemp " + Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf(typeof(tempstruct)));
    }

    //byte[] getBytes(Tree[] Allnodes, int array_size)
    //{
    //    int size = Marshal.SizeOf(Allnodes[0]);
    //    byte[] arr = new byte[size*array_size];

    //    IntPtr ptr = Marshal.AllocHGlobal(size);
    //    for (int i=0; i<100)
    //    Marshal.StructureToPtr(str, ptr, true);
    //    Marshal.Copy(ptr, arr, 0, size);
    //    Marshal.FreeHGlobal(ptr);
    //    return arr;
    //}
}
