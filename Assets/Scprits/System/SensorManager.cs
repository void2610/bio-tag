using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using DG.Tweening;

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
    private List<Tween> tweens = new List<Tween>();
    private Volume volume;

    public void AddVFX(VisualEffect vfx)
    {
        vfxs.Add(vfx);
    }

    private void SetAlpha(float a, float d)
    {
        foreach (var vfx in vfxs)
        {
            var t = DOTween.To(() => vfx.GetFloat("alpha"), x => vfx.SetFloat("alpha", x), a, d);
            tweens.Add(t);
        }
    }

    private void ChangeToExcited()
    {
        foreach (var t in tweens)
        {
            t.Kill();
        }
        tweens.Clear();
        SetAlpha(0.0f, 1f);
        volume.profile.TryGet(out Vignette vignette);
        var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 1f);
        tweens.Add(tw);
    }
    private void ChangeToCalm()
    {
        foreach (var t in tweens)
        {
            t.Kill();
        }
        tweens.Clear();
        SetAlpha(1.0f, 3f);
        volume.profile.TryGet(out Vignette vignette);
        var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, 1f);
        tweens.Add(tw);
    }

    private void Start()
    {
        volume = FindObjectOfType<Volume>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            sensorValue = true;
            ChangeToExcited();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            sensorValue = false;
            ChangeToCalm();
        }

        value = sensorValue;
    }
}
