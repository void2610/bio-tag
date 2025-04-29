using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using DG.Tweening;

public class SensorManager : MonoBehaviour
{
    public static SensorManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private bool _sensorValue = false;
    private readonly List<VisualEffect> _vfxList = new ();
    private readonly List<Tween> _tweenList = new ();
    private Volume _volume;

    public void AddVFX(VisualEffect vfx)
    {
        _vfxList.Add(vfx);
    }

    private void SetAlpha(float a, float d)
    {
        foreach (var vfx in _vfxList)
        {
            var t = DOTween.To(() => vfx.GetFloat("alpha"), x => vfx.SetFloat("alpha", x), a, d);
            _tweenList.Add(t);
        }
    }

    private void ChangeToExcited()
    {
        Debug.Log("Excited");
        foreach (var t in _tweenList)
        {
            t.Kill();
        }
        _tweenList.Clear();
        SetAlpha(0.0f, 1f);
        _volume.profile.TryGet(out Vignette vignette);
        var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 1f);
        _tweenList.Add(tw);
        
        // プレイヤー速度の変更
        GameManagerBase.Instance.GetMainPlayer().SetWalkSpeed(3f);
    }
    
    private void ChangeToCalm()
    {
        Debug.Log("Calm");
        foreach (var t in _tweenList)
        {
            t.Kill();
        }
        _tweenList.Clear();
        SetAlpha(1.0f, 3f);
        _volume.profile.TryGet(out Vignette vignette);
        var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, 1f);
        _tweenList.Add(tw);
        
        // プレイヤー速度の変更
        GameManagerBase.Instance.GetMainPlayer().SetWalkSpeed(6.5f);
    }

    [Obsolete("Obsolete")]
    private void Start()
    {
        _volume = FindObjectOfType<Volume>();
    }

    private void Update()
    {
        // TODO: yaru
        // _sensorValue = GsrGraph.Instance.IsExcited;

        if (Input.GetKeyDown(KeyCode.J))
        {
            _sensorValue = false;
            ChangeToExcited();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            _sensorValue = true;
            ChangeToCalm();
        }
    }
}
