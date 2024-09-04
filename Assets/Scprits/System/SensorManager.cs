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

    public float value { get; protected set; } = 0f;
    private float sensorValue = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            sensorValue++;
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            sensorValue--;
        }
        //TODO: 実際はキャリブレーション等を行う
        value = sensorValue;
    }
}
