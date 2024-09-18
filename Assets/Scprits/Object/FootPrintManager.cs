using UnityEngine;
using System.Collections.Generic;

public class FootPrintManager : MonoBehaviour
{
    public static FootPrintManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [SerializeField]
    private GameObject footPrintPrefab;
    private List<GameObject> footPrints = new List<GameObject>();

    public void CreateFootPrint(Vector3 position, Quaternion rotation, int index)
    {
        Debug.Log("CreateFootPrint");
        var pos = position + new Vector3(0, 0.1f, 0);
        var footPrint = Instantiate(footPrintPrefab, pos, rotation, this.transform);
        footPrints.Add(footPrint);
    }
}
