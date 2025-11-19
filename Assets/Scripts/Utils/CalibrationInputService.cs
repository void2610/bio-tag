using UnityEngine;
using VContainer.Unity;
using BioTag.Biometric;

namespace BioTag.Utils
{
    /// <summary>
    /// キャリブレーションのキー入力を管理するサービス
    /// 全シーンで動作し、GsrProcessorServiceのベースライン・閾値調整を担当
    /// </summary>
    public class CalibrationInputService : ITickable
    {
        private readonly GsrProcessorService _gsrProcessor;

        public CalibrationInputService(GsrProcessorService gsrProcessor)
        {
            _gsrProcessor = gsrProcessor;
        }

        public void Tick()
        {
            // ベースライン調整
            if (Input.GetKey(KeyCode.DownArrow))
            {
                _gsrProcessor.AdjustBaseline(-0.1f);
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                _gsrProcessor.AdjustBaseline(0.1f);
            }

            // 自動キャリブレーション
            else if (Input.GetKey(KeyCode.Return))
            {
                _gsrProcessor.AutoCalibrate();
            }

            // 閾値調整
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
              _gsrProcessor.AdjustThreshold(-0.1f);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                _gsrProcessor.AdjustThreshold(0.1f);
            }
        }
    }
}
