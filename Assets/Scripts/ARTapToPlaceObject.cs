using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject ObjectToPlace;
    public GameObject PlacementIndicator;

    [SerializeField] private ARRaycastManager _arRaycastManager;

    [SerializeField] private ARAnchorManager _arAnchorManager;

    private Pose _placementPose;
    private bool _placementPoseIsValid = false;

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    public void PlaceObject()
    {
        if (_placementPoseIsValid)
        {
            PlaceObjectInternal();
        }
    }

    private void PlaceObjectInternal()
    {
        //Instantiate(ObjectToPlace, _placementPose.position, _placementPose.rotation);
        _arAnchorManager.AddAnchor(new Pose(_placementPose.position, _placementPose.rotation));
    }

    private void UpdatePlacementIndicator()
    {
        if (_placementPoseIsValid)
        {
            PlacementIndicator.SetActive(true);
            PlacementIndicator.transform.SetPositionAndRotation(_placementPose.position, _placementPose.rotation);
        }
        else
        {
            PlacementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        //_xrOrigin.Raycast(screenCenter, hits, TrackableType.Planes);
        _arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        _placementPoseIsValid = hits.Count > 0;
        if (_placementPoseIsValid)
        {
            _placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            _placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}