using UnityEngine;

public class SensorManager : MonoBehaviour
{
    public static SensorManager instance;
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

    public bool value { get; protected set; } = false;
    private bool sensorValue = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            sensorValue = true;
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            sensorValue = false;
        }
        //TODO: 実際はキャリブレーション等を行う
        value = sensorValue;
    }
}
