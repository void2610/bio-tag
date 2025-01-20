using UnityEngine;

public class GsrMock : MonoBehaviour
{
    private float _current = 0;
    private void Update()
    {
        //ランダムにGSRGraphに値を送る
        GsrGraph.Instance.AddData(_current);
        _current += Random.Range(-0.1f, 0.1f);
    }
}
