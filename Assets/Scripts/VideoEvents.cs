using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoEvents : MonoBehaviour
{
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private string _returnScene;

    void Start()
    {
        _videoPlayer.loopPointReached += OnVideoFinished;

        var volume = PlayerPrefs.GetFloat("volume", 0.5f);
        for (ushort trackIndex = 0; trackIndex < _videoPlayer.audioTrackCount; trackIndex++)
        {
            _videoPlayer.SetDirectAudioVolume(trackIndex, volume);
        }

#if UNITY_WEBGL
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = "https://erkle64.github.io/BadFractal/badapple.ogv";
#endif
        _videoPlayer.Prepare();
    }

    void Update()
    {
        if (!_videoPlayer.isPlaying && _videoPlayer.isPrepared) _videoPlayer.Play();
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            SceneManager.LoadScene(_returnScene);
        }
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        SceneManager.LoadScene(_returnScene);
    }
}
