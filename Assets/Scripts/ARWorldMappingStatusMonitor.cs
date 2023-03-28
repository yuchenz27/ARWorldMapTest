using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ARWorldMappingStatusMonitor : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;

    [SerializeField] private TMP_Text _text;

    private void Update()
    {
        if (_arSession == null)
            return;

        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem == null)
            return;

        ARWorldMappingStatus currentStatus = sessionSubsystem.worldMappingStatus;
        _text.text = $"Mapping Status: {currentStatus}";
    }
}
