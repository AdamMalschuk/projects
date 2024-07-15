using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class vectors : MonoBehaviour
{
    // Start is called before the first frame update
    /*void Start()
    {
        int theta = 0;
        int rho = 0;
        positions[] pos = null;
        Vector3 coords;
        int counter = 0;
        Debug.Log(counter);
        string path = "Assets/test.txt";
        for (int i = 0; i < 360; i++)
        {
            
            for (int j = 0; i < 360; i++)
            {
                theta = i;
                rho = j;
                coords.x = Mathf.Sin(theta) * Mathf.Cos(rho);
                coords.y = Mathf.Sin(theta) * Mathf.Sin(rho);
                coords.z = Mathf.Cos(theta);
                counter ++;
                positions temp = null;
                temp.theta = theta;
                temp.rho = rho;
                temp.vector = coords;
                pos[i] = temp;
                StreamWriter writer = new StreamWriter(path, true);
                writer.WriteLine(temp);
                writer.Close();
                Debug.Log(counter);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class positions
    {
        public int theta;
        public int rho;
        public Vector3 vector;
    }*/
}
