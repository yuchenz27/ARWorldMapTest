using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARFoundation;
using TMPro;
using UnityEngine.UI;

public class ARWorldMappingStatusMonitor : MonoBehaviour
{
    public bool MapAvailable => _mapAvailable;

    [SerializeField] private ARSession _arSession;

    [SerializeField] private TMP_Text _text;

    [SerializeField] private Button _saveButton;

    /// <summary>
    /// Whether the current world map can be saved.
    /// </summary>
    private bool _mapAvailable;

    private void Start()
    {
        _saveButton.interactable = false;
    }

    private void Update()
    {
        if (_arSession == null)
            return;

        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem == null)
            return;

        ARWorldMappingStatus currentStatus = sessionSubsystem.worldMappingStatus;

        if (currentStatus == ARWorldMappingStatus.Mapped)
        {
            _mapAvailable = true;
            _saveButton.interactable = true;
        }

        _text.text = $"Mapping Status: {currentStatus}";
    }
}
