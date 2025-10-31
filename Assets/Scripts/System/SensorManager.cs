using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using VContainer;
using BioTag.Biometric;

public class SensorManager : MonoBehaviour
{
    public static SensorManager Instance;

    private BiometricService _biometricService;

    [Inject]
    public void Construct(BiometricService biometricService)
    {
        _biometricService = biometricService;
    }
    
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

    /// <summary>
    /// VFXをBiometricServiceに登録
    /// </summary>
    public void AddVFX(VisualEffect vfx)
    {
        _biometricService?.AddVFX(vfx);
    }

    [Obsolete("Obsolete")]
    private void Start()
    {
        var volume = FindObjectOfType<Volume>();
        _biometricService?.SetVolume(volume);
    }
}
