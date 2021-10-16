using System;
using System.Linq;
using Chip;
using UnityEngine;

namespace Graphics
{
public class DisplayScreen : BuiltinChip
{
    private const int Size = 8;
    public Renderer textureRender;
    private string _editCoords;
    private int[] _texCoords;
    private Texture2D _texture;

    protected override void Awake()
    {
        _texture = CreateSolidTexture2D(new Color(0, 0, 0), Size);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        textureRender.sharedMaterial.mainTexture = _texture;
        base.Awake();
    }

    private static Texture2D CreateSolidTexture2D(Color color, int width, int height = -1)
    {
        if (height == -1) height = width;
        var texture = new Texture2D(width, height);
        var pixels = Enumerable.Repeat(color, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private int[] map2d(int index, int size)
    {
        var coords = new int[2];
        coords[0] = index % size;
        coords[1] = index / size;
        return coords;
    }

    //update display here
    protected override void ProcessOutput()
    {
        _editCoords = "";
        for (var i = 6; i < 12; i++) _editCoords += inputPins[i].State.ToString();
        _texCoords = map2d(Convert.ToInt32(_editCoords, 2), Size);
        _texture.SetPixel(_texCoords[0], _texCoords[1],
                          new Color(Convert.ToInt32(inputPins[0].State + inputPins[1].State.ToString(), 2) / 2f,
                                    Convert.ToInt32(inputPins[2].State + inputPins[3].State.ToString(), 2) / 2f,
                                    Convert.ToInt32(inputPins[4].State + inputPins[5].State.ToString(), 2)) / 2f);
        _texture.Apply();
    }
}
}