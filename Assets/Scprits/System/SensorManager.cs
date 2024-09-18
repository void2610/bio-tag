using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;
using DG.Tweening; // DoTweenの名前空間を追加

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
    private List<VisualEffect> vfxs = new List<VisualEffect>();
    private Tween alphaTween;

    public void AddVFX(VisualEffect vfx)
    {
        vfxs.Add(vfx);
    }

    private void SetAlpha(float a, float d)
    {
        foreach (var vfx in vfxs)
        {
            if (alphaTween != null && alphaTween.IsActive())
            {
                alphaTween.Kill();
            }
            alphaTween = DOTween.To(() => vfx.GetFloat("alpha"), x => vfx.SetFloat("alpha", x), a, d);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            sensorValue = true;
            SetAlpha(1.0f, 2f);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            sensorValue = false;
            SetAlpha(0.0f, 1f);
        }

        value = sensorValue;
    }
}
