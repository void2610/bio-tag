using UnityEngine;

public class ItMarker : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.9f, 0);

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        if (target == null) this.transform.position = new Vector3(0, -100, 0);
        else
            this.transform.position = target.position + offset;
    }
}
