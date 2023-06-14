using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<Transform> listStatus = new List<Transform>();
    // Start is called before the first frame update
    void OnEnable()
    {
        for(int i = 0; i < 3; i++)
        {
            listStatus.Add(transform.GetChild(i));
        }
    }
}
