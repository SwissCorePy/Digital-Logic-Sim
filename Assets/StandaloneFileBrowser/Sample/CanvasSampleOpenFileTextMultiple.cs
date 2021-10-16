using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace StandaloneFileBrowser.Sample
{
[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFileTextMultiple : MonoBehaviour, IPointerDownHandler
{
    public Text output;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".txt", true);
    }

    // Called from browser
    public void OnFileUpload(string urls) {
        StartCoroutine(OutputRoutine(urls.Split(',')));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData)
    {
    }

    private void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "txt", true);
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", true);
        if (paths.Length > 0)
        {
            var urlArr = new List<string>(paths.Length);
            for (var i = 0; i < paths.Length; i++) urlArr.Add(new Uri(paths[i]).AbsoluteUri);
            StartCoroutine(OutputRoutine(urlArr.ToArray()));
        }
    }
#endif


    private IEnumerator OutputRoutine(string[] urlArr)
    {
        var outputText = "";
        for (var i = 0; i < urlArr.Length; i++)
        {
            var loader = new UnityWebRequest(urlArr[i]);
            yield return loader.SendWebRequest();
            outputText += ((DownloadHandlerTexture)loader.downloadHandler).text;
        }

        output.text = outputText;
    }
}
}