using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

public class CameraTrackingStateMonitor : MonoBehaviour
{
    [SerializeField] private ARSession _arSession;

    [SerializeField] private GameObject _promptWindow2;

    private bool _isRelocalizaing = false;

    private void Start()
    {
        _promptWindow2.SetActive(false);
    }

    private void Update()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)_arSession.subsystem;
        if (sessionSubsystem != null)
        {
            //Debug.Log($"Tracking state: {sessionSubsystem.trackingState} and reason: {sessionSubsystem.notTrackingReason}");
            if (sessionSubsystem.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited
                && sessionSubsystem.notTrackingReason == UnityEngine.XR.ARSubsystems.NotTrackingReason.Relocalizing)
            {
                _isRelocalizaing = true;
                return;
            }

            if (_isRelocalizaing && sessionSubsystem.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                _promptWindow2.SetActive(true);
                StartCoroutine(PromptWindowFadeOut());
            }
        }
    }

    private IEnumerator PromptWindowFadeOut()
    {
        yield return new WaitForSeconds(3f);
        _promptWindow2.SetActive(false);
    }
}
