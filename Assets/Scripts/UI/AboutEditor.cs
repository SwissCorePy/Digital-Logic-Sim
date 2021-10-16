using System;
using TMPro;
using UnityEngine;

namespace UI {
[ExecuteInEditMode]
public class AboutEditor : MonoBehaviour {
  public TMP_Text target;

  public CustomCols[] cols;
  public CustomSizes[] sizes;

  private TMP_Text _source;

  private void Update() {
    if (!Application.isPlaying) {
      if (_source == null)
        _source = GetComponent<TMP_Text>();
      var formattedText = _source.text;
      if (cols != null)
        for (var i = 0; i < cols.Length; i++) {
          var key = $"<color={cols[i].name}>";
          var replace =
              $"<color=#{ColorUtility.ToHtmlStringRGB(cols[i].colour)}>";
          formattedText = formattedText.Replace(key, replace);
        }

      if (sizes != null)
        for (var i = 0; i < sizes.Length; i++) {
          var key = $"<size={sizes[i].name}>";
          var replace = $"<size={sizes[i].fontSize}>";
          formattedText = formattedText.Replace(key, replace);
        }

      target.text = formattedText;
    }
  }

  [Serializable]
  public struct CustomSizes {
    public string name;
    public int fontSize;
  }

  [Serializable]
  public struct CustomCols {
    public string name;
    public Color colour;
  }
}
}
