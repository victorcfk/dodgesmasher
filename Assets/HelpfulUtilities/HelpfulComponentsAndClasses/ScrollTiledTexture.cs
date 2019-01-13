using UnityEngine;

public class ScrollTiledTexture : MonoBehaviour {

    [SerializeField]
    bool _autoScrollTexture = true;
    [SerializeField]
    string _materialStringProperty = "_MainTex";
    
    public Material MaterialToScroll;
    public Vector2 ScrollUnitsPerSecond = Vector2.left;

    Vector2 _lastKnownOffset;
    Vector2 LastKnownOffSet
    {
        get { return _lastKnownOffset; }
    }

    int _textureID;

    void Awake()
    {
        if(MaterialToScroll == null)
            MaterialToScroll = GetComponent<Renderer>().material;

        SetTexturePropertyToAnimate(_materialStringProperty);
        _lastKnownOffset = MaterialToScroll.GetTextureOffset(_textureID);
    }

    void Update()
    {
        if(_autoScrollTexture)
            ScrollMaterial(MaterialToScroll, _textureID, ref _lastKnownOffset, ScrollUnitsPerSecond, Time.deltaTime);
    }

    public void SetTexturePropertyToAnimate(string inMaterialStringProperty)
    {
        _textureID = Shader.PropertyToID(inMaterialStringProperty);
    }

    public void ScrollMaterial(Material inTargetMaterial, int inMaterialID, ref Vector2 LastKnownOffSet, Vector2 ScrollUnitsPerSecond, float timeFromLastFrame)
    {
        LastKnownOffSet += ScrollUnitsPerSecond * timeFromLastFrame;

        LastKnownOffSet.x %= 1;
        LastKnownOffSet.y %= 1;

        inTargetMaterial.SetTextureOffset(inMaterialID, LastKnownOffSet);
    }
}
