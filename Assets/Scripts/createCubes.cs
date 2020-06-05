using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createCubes : MonoBehaviour
{
    public int LatticeSide = 10;
    public float Spacing = 1.1f;
    public GameObject voxel;
    // Start is called before the first frame update
    void Start()
    {
        for (int i=0; i<LatticeSide; i++)
        {
            for (int j=0; j<LatticeSide; j++)
            {
                for (int k=0; k<LatticeSide; k++)
                {
                    Vector3 pos = new Vector3(transform.position.x + i * Spacing, transform.position.y + j * Spacing, transform.position.z + k * Spacing);
                    Instantiate(voxel, pos, Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
