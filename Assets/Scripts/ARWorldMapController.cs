using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;

public class ARWorldMapController : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;

    [SerializeField] private GameObject _promptWindow;

    [SerializeField] private TMP_Text _promptText;

    private void Start()
    {
        _promptWindow.SetActive(false);
    }

    public void SaveCurrentWorldMap()
    {
        StartCoroutine(Save());
    }

    private IEnumerator Save()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem == null)
        {
            Debug.Log("No session subsystem available. Could not save.");
            yield break;
        }

        var request = sessionSubsystem.GetARWorldMapAsync();

        while (!request.status.IsDone())
            yield return null;

        if (request.status.IsError())
        {
            Debug.Log($"Session serialization failed with status {request.status}");
            yield break;
        }

        var worldMap = request.GetWorldMap();
        request.Dispose();

        SaveAndDisposeWorldMap(worldMap);
    }

    private void SaveAndDisposeWorldMap(ARWorldMap worldMap)
    {
        Debug.Log("Serializing ARWorldMap to byte array...");
        var data = worldMap.Serialize(Allocator.Temp);
        Debug.Log($"ARWorldMap has {data.Length} bytes.");
        float mapSize = data.Length / 1024.0f / 1024.0f;

        string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        string path = Path.Combine(Application.persistentDataPath, date + ".worldmap");

        var file = File.Open(path, FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(data.ToArray());
        writer.Close();
        data.Dispose();
        worldMap.Dispose();
        Debug.Log($"ARWorldMap written to {path}");

        _promptText.text = $"ARWorldMap saved with size {mapSize:F2} mb";
        _promptWindow.SetActive(true);
        StartCoroutine(PromptWindowFadeOut());
    }

    private IEnumerator PromptWindowFadeOut()
    {
        yield return new WaitForSeconds(3f);
        _promptWindow.SetActive(false);
    }
}
