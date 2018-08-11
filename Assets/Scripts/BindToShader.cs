using Intel.RealSense;
using System.Linq;
using System.Threading;
using UnityEngine;

public class BindToShader : MonoBehaviour
{
    public Stream sourceStreamType;
    public TextureFormat textureFormat;
    public Material materialToBind;
    public string texToBind;
    public bool bindPrev;

    private Texture2D texture;
    private Texture2D prevTexture;
    
    [System.NonSerialized]
    byte[] data;
    byte[] pData;

    AutoResetEvent f = new AutoResetEvent(false);
    int threadId;

    void Awake()
    {
        threadId = Thread.CurrentThread.ManagedThreadId;
    }
    /// <summary>
    /// Called per frame before publishing it
    /// </summary>
    /// <param name="f">The frame to process</param>
    /// <returns>The processed frame</returns>
    virtual protected Frame ProcessFrame(Frame f)
    {
        return f;
    }

    public bool fetchFramesFromDevice = true;

    void Start()
    {
        texture = Texture2D.blackTexture;
        prevTexture = Texture2D.blackTexture;
        if (RealSenseDevice.Instance.ActiveProfile != null)
            OnStartStreaming(RealSenseDevice.Instance.ActiveProfile);
        else
            RealSenseDevice.Instance.OnStart += OnStartStreaming;
    }

    private void OnStartStreaming(PipelineProfile activeProfile)
    {
        var profile = RealSenseDevice.Instance.ActiveProfile.Streams.First(p => p.Stream == sourceStreamType);
        if (profile == null)
            return;
        var videoProfile = profile as VideoStreamProfile;
        //set prev texture


        texture = new Texture2D(videoProfile.Width, videoProfile.Height, textureFormat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        texture.Apply();
        //textureBinding.Invoke(texture);
        
        if (bindPrev){
            prevTexture = new Texture2D(videoProfile.Width, videoProfile.Height, textureFormat, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
           
            prevTexture.Apply();
            materialToBind.SetTexture("_PrevDepthTex", prevTexture);
        }

        materialToBind.SetTexture(texToBind,texture);
        if (fetchFramesFromDevice)
        {
            if (RealSenseDevice.Instance.processMode == RealSenseDevice.ProcessMode.UnityThread)
                RealSenseDevice.Instance.onNewSample += OnNewSampleUnityThread;
            else
                RealSenseDevice.Instance.onNewSample += OnNewSampleThreading;
        }
    }

    public void OnFrame(Frame f)
    {
        if (RealSenseDevice.Instance.processMode == RealSenseDevice.ProcessMode.UnityThread)
        {
            OnNewSampleUnityThread(f);
        }
        else
        {
            OnNewSampleThreading(f);
        }
    }

    private void UpdateData(Frame frame)
    {
        if (frame.Profile.Stream != sourceStreamType)
            return;

        var vidFrame = ProcessFrame(frame) as VideoFrame;
        
        if (data != null){
            pData = data;
        }

        data = data ?? new byte[vidFrame.Stride * vidFrame.Height];
        vidFrame.CopyTo(data);
    }
    private void UploadPrevTexture()
    {
        prevTexture.LoadRawTextureData(pData);
        prevTexture.Apply();
    }
    private void UploadTexture()
    {
        UploadPrevTexture();
        texture.LoadRawTextureData(data);
        texture.Apply();
    }
    
    private void OnNewSampleThreading(Frame frame)
    {
        UpdateData(frame);
        f.Set();
    }

    private void OnNewSampleUnityThread(Frame frame)
    {
        UnityEngine.Assertions.Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
        UpdateData(frame);
        UploadTexture();
    }

    // Update is called once per frame
    void Update()
    {
        if (f.WaitOne(0))
        {
            UploadTexture();
        }
    }
}
