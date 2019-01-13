///Taken From http://wiki.unity3d.com/index.php?title=Animating_Tiled_texture
///Original Author: Joachim Ante 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTiledTexture : MonoBehaviour {

    [SerializeField]
    int _uvAnimationTileX = 4;
    [SerializeField]
    int _uvAnimationTileY = 16;
    [SerializeField]
    float _framesPerSecond = 30.0f;
    [SerializeField]
    string materialStringProperty = "_MainTex";

    Material _material;
    int _textureID;

    void Start()
    {
        _material = GetComponent<Renderer>().material;
        _textureID = Shader.PropertyToID(materialStringProperty);
    }

    void Update()
    {
        TextureAnimate(_material, _textureID, _framesPerSecond);
    }

    void TextureAnimate(Material inTargetMaterial, int inMaterialID, float inFramesPerSecond)
    {
        // Calculate index
        int index = (int)(Time.time * inFramesPerSecond);

        // repeat when exhausting all frames
        index = index % (_uvAnimationTileX * _uvAnimationTileY);

        // Size of every tile
        Vector2 size = new Vector2(1.0f / _uvAnimationTileX, 1.0f / _uvAnimationTileY);

        // split into horizontal and vertical index
        int uIndex = index % _uvAnimationTileX;
        int vIndex = index / _uvAnimationTileX;

        // build offset
        // v coordinate is the bottom of the image in opengl so we need to invert.
        Vector2 offset = new Vector2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);

        inTargetMaterial.SetTextureOffset(inMaterialID, offset);
        inTargetMaterial.SetTextureScale(inMaterialID, size);
    }
}
