using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree : MonoBehaviour
{
    public float size = 10.0f;
    class Square
    {
        public Vector3 pos = new Vector3();
        public float w = 0f;

        public bool isInside(Vector3 point)
        {
            return
                (   point.x >= pos.x - w &&
                    point.x < pos.x + w &&
                    point.y >= pos.y - w &&
                    point.y < pos.y + w &&
                    point.z >= pos.z - w &&
                    point.z < pos.z + w
                );
        }
    }
    class Tree
    {
        public Square boundary = new Square();
        public float numpoints;
        public Tree[] child = new Tree[8];     //lower sw, se, ne, sw | upper sw, se, ne, sw
        public int level;
        public bool divided;

        public Tree(int level_, Vector3 pos, float w)
        {
            divided = false;
            level = level_;
            boundary.pos = pos;
            boundary.w = w;
            numpoints = 0;
        }

        public void subdivide()
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
                    child[i] = new Tree(level - 1, centers[i], boundary.w / 2.0f);
                }
                divided = true;
            }
        }

        public void insert(Vector3 point)
        {
            if (!boundary.isInside(point))
            {
                return;
            }

            if (level > 1)
            {
                if (divided == false)
                {
                    subdivide();
                }

                for (int i = 0; i < 8; i++)
                {
                    child[i].insert(point);
                }
            }
            numpoints++;
        }
    }

    private Tree ot;

    void show(Tree ot)
    {
        if (ot.numpoints > 0)
        {
            float S = 2 * ot.boundary.w;
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = ot.boundary.pos;
            cube.transform.localScale = new Vector3(S, S, S);
            cube.GetComponent<MeshRenderer>().enabled = false;
        }

        if (ot.level > 1 && ot.divided == true)
        {
            for (int i = 0; i < 8; i++)
            {
                show(ot.child[i]);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ot = new Tree(5, new Vector3(size / 2, size / 2, size / 2), size / 2);
        ot.insert(new Vector3(4.1f, 5.1f, 2.1f));
        //for (int i = 0; i < 8; i++)
        //{
        //    for (int j = 0; j < 8; j++)
        //    {
        //        Debug.Log(((ot.child[i]).child[j]).numpoints);
        //    }
        //}
        show(ot);
    }

    // Update is called once per frame
    void Update()
    {
        //Color color = new Color(0.5f, 1.0f, 1.0f);
        //Gizmos.DrawWireCube(Vector3.zero, new Vector3(10, 10, 10));
    }
}
