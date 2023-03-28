using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARAnchorDelegateManager : MonoBehaviour
{
    [SerializeField] private ARAnchorManager _arAnchorManager;

    [SerializeField] private GameObject _anchorPrefab;

    private void OnEnable()
    {
        _arAnchorManager.anchorsChanged += OnAnchorsChanged;
    }

    private void OnDisable()
    {
        _arAnchorManager.anchorsChanged -= OnAnchorsChanged;
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
    {
        foreach (ARAnchor anchor in eventArgs.added)
        {
            Debug.Log("New anchor added: " + anchor.trackableId);
            Instantiate(_anchorPrefab, anchor.transform.position, anchor.transform.rotation);
        }
    }
}
