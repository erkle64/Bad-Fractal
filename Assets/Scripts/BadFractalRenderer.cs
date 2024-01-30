using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class BadApple : MonoBehaviour
{
    [SerializeField] private Shader _seedShader;
    [SerializeField] private Shader _jfaShader;
    [SerializeField] private Shader _distanceShader;
    [SerializeField] private Shader _outShader;
    [SerializeField] private float _maxDistance = 0.25f;
    [SerializeField] private Texture _sourceTexture;

    Material _seedMaterial;
    Material _jfaMaterial;
    Material _distanceMaterial;
    Material _outMaterial;

    RenderTexture _seeds;
    RenderTexture _jfa0;
    RenderTexture _jfa1;
    RenderTexture _distance;

    void Awake()
    {
        var size = new Vector2Int(_sourceTexture.width, _sourceTexture.height) * 4;
        _seeds = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RGFloat, 0);
        _jfa0 = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RGFloat, 0);
        _jfa1 = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.RGFloat, 0);
        _distance = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.R8, 0);
        _distance.filterMode = FilterMode.Bilinear;

        _seedMaterial = new Material(_seedShader);
        _jfaMaterial = new Material(_jfaShader);
        _distanceMaterial = new Material(_distanceShader);
        _outMaterial = new Material(_outShader);

        UpdatePrefs();
    }

    void OnDestroy()
    {
        _seeds.Release();
        _jfa0.Release();
        _jfa1.Release();
        _distance.Release();
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_sourceTexture, _seeds, _seedMaterial);

        int kernelSize = _sourceTexture.width * 2;
        _jfaMaterial.SetFloat("_KernelSize", kernelSize);
        Graphics.Blit(_seeds, _jfa0, _jfaMaterial);

        while(kernelSize >= 2)
        {
            kernelSize /= 2;
            _jfaMaterial.SetFloat("_KernelSize", kernelSize);
            Graphics.Blit(_jfa0, _jfa1, _jfaMaterial);
            var temp = _jfa0;
            _jfa0 = _jfa1;
            _jfa1 = temp;
        }

        _distanceMaterial.SetFloat("_MaxDistance", _maxDistance);
        Graphics.Blit(_jfa0, _distance, _distanceMaterial);

        _outMaterial.SetFloat("_Aspect", (float)(destination.width / (float)destination.height));
        Graphics.Blit(_distance, destination, _outMaterial);
    }

    public void UpdatePrefs()
    {
        var seed = new Vector4(
            PlayerPrefs.GetFloat("x", -0.5f),
            PlayerPrefs.GetFloat("y", 0.6f),
            PlayerPrefs.GetFloat("xw", 0.16f),
            PlayerPrefs.GetFloat("yw", 0.1f)
            );
        _outMaterial.SetVector("_Seed", seed);

        var trap = new Vector4(
            PlayerPrefs.GetFloat("xs", 1.0f),
            PlayerPrefs.GetFloat("ys", 1.0f),
            PlayerPrefs.GetFloat("xo", -0.471875f),
            PlayerPrefs.GetFloat("yo", -0.4375f)
            );
        _outMaterial.SetVector("_Trap", new Vector4(0.1875f / trap.x, 0.25f / trap.y, -trap.z, -trap.w));

        var speed = new Vector2(
            PlayerPrefs.GetFloat("xf", 0.5f),
            PlayerPrefs.GetFloat("yf", 0.5f)
            );
        _outMaterial.SetVector("_Speed", speed);

        var fade = PlayerPrefs.GetFloat("f", 0.035f);
        _outMaterial.SetFloat("_Fade", 1.0f - fade);

        var background = new Vector3(
            PlayerPrefs.GetFloat("bgf", 4.0f),
            PlayerPrefs.GetFloat("bgs", 0.25f),
            PlayerPrefs.GetFloat("bgv", 0.4f)
            );
        _outMaterial.SetVector("_Background", background);
    }
}
