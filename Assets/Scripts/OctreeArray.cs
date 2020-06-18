﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeArray : MonoBehaviour
{
    public float size = 10.0f;
    int headindex = 0;

    struct Square
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
    struct Tree
    {
        public Square boundary;
        public float numpoints;
        public int[] child;     //lower sw, se, ne, sw | upper sw, se, ne, sw
        public int level;
        public bool divided;

        public Tree(int level_, Vector3 pos, float w)
        {
            divided = false;
            level = level_;
            boundary = new Square();
            boundary.pos = pos;
            boundary.w = w;
            numpoints = 0;
            child = new int[8];
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
                divided = true;
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
                if (divided == false)
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

    Tree[] AllNodes = new Tree[32768];

    void show(Tree[] AllNodes, int index)
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

        if (AllNodes[index].level > 1 && AllNodes[index].divided == true)
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
        AllNodes[headindex] = (new Tree(5, new Vector3(size / 2, size / 2, size / 2), size / 2));
        headindex++;
        //Tree noctreee = new Tree(1, new Vector3(size / 2, size / 2, size / 2), size / 2);
        //AllNodes.Add(noctreee);
        for (float i = 0; i < 6; i += 0.1f)
        {
            for (float j = 0; j < 6; j += 0.1f)
            {
                for (float k = 0; k < 6; k += 0.1f)
                {
                    Vector3 pt = new Vector3(i, j, k);
                    if (Vector3.Distance(pt, Vector3.zero) < 6)
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
        show(AllNodes, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("headindex: " + headindex);
        Debug.Log("count: " + AllNodes[0].boundary.w);
    }
}