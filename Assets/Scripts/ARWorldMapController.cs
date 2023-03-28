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

    [SerializeField] private MapSlot _mapSlotPrefab;

    [SerializeField] private RectTransform _mapRoot;

    [SerializeField] private GameObject _mapScroll;

    private void Start()
    {
        _promptWindow.SetActive(false);
        _mapScroll.SetActive(false);
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

    public void OnMapsButtonPressed()
    {
        if (_mapScroll.activeSelf)
        {
            _mapScroll.SetActive(false);
        }
        else
        {
            QueryMaps();
            _mapScroll.SetActive(true);
        }
    }

    private void QueryMaps()
    {
        for (int i = 0; i < _mapRoot.childCount; i++)
        {
            Destroy(_mapRoot.GetChild(i).gameObject);
        }

        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
        {
            Debug.Log(file);
            string[] arr1 = file.Split("/");
            string[] arr2 = arr1[arr1.Length - 1].Split(".");
            string date = arr2[0];

            FileInfo fileInfo = new FileInfo(file);
            string size = (fileInfo.Length / 1024f / 1024f).ToString("F2");

            var mapSlotInstance = Instantiate(_mapSlotPrefab);
            mapSlotInstance.Date.text = "Date: " + date;
            mapSlotInstance.Size.text = "Size: " + size + "mb";
            mapSlotInstance.LoadButton.onClick.AddListener(() =>
            {
                LoadWorldMap(file);
            });
            mapSlotInstance.DeleleButton.onClick.AddListener(() =>
            {
                DeleteWorldMap(file);
            });

            mapSlotInstance.transform.SetParent(_mapRoot);
            mapSlotInstance.transform.localScale = Vector3.one;
        }
    }

    private void LoadWorldMap(string filePath)
    {
        _mapScroll.SetActive(false);

        // Destroy all existing ARAnchors
        var anchors = FindObjectsOfType<ARAnchorPrefab>();
        foreach (var anchor in anchors)
        {
            Destroy(anchor.gameObject);
        }

        StartCoroutine(Load(filePath));
    }

    private void DeleteWorldMap(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("File deleted.");
        }
        else
        {
            Debug.Log("File does not exist.");
        }

        QueryMaps();
    }

    private IEnumerator Load(string path)
    {
        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem == null)
        {
            Debug.Log("No session subsystem available. Could not load.");
            yield break;
        }

        FileStream file;
        try
        {
            file = File.Open(path, FileMode.Open);
        }
        catch (FileNotFoundException)
        {
            Debug.LogError("No ARWorldMap was found. Make sure to save the ARWorldMap before attempting to load it.");
            yield break;
        }

        Debug.Log($"Reading {path}...");

        const int bytesPerFrame = 1024 * 10;
        var bytesRemaining = file.Length;
        var binaryReader = new BinaryReader(file);
        var allBytes = new List<byte>();
        while (bytesRemaining > 0)
        {
            var bytes = binaryReader.ReadBytes(bytesPerFrame);
            allBytes.AddRange(bytes);
            bytesRemaining -= bytesPerFrame;
            yield return null;
        }

        var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
        data.CopyFrom(allBytes.ToArray());

        Debug.Log("Deserializing to ARWorldMap...");
        if (ARWorldMap.TryDeserialize(data, out ARWorldMap worldMap))
            data.Dispose();

        if (worldMap.valid)
        {
            Debug.Log("Deserialized successfully.");
        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");

            _promptText.text = $"Data is not a valid ARWorldMap";
            _promptWindow.SetActive(true);
            StartCoroutine(PromptWindowFadeOut());

            yield break;
        }

        Debug.Log("Apply ARWorldMap to current session.");
        sessionSubsystem.ApplyWorldMap(worldMap);

        _promptText.text = $"ARWorldMap successfully loaded";
        _promptWindow.SetActive(true);
        StartCoroutine(PromptWindowFadeOut());
    }
}
