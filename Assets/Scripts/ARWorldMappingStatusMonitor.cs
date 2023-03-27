using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARFoundation;

public class ARWorldMappingStatusMonitor : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;

    private void Update()
    {
        if (_arSession == null)
            return;

        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem == null)
            return;

        ARWorldMappingStatus currentStatus = sessionSubsystem.worldMappingStatus;
        Debug.Log($"Current status: {currentStatus}");
    }
}
