using Fusion;
using System.Diagnostics;
using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

// This component will sync parenting of a transform, 
// using either a NetworkObject or NetworkBehaviour reference.
public class WrapUnwrapSample : NetworkBehaviour
{

    // Networked NetworkObject with auto properties:
    // The ILWeaver replaces the Get method with code which writes the NetworkId
    // to the snapshot, and the Set method with handling which finds the Object using the NetworkId.
    // Get will return the Object if found.
    // Get will return null if the property was set to null, and also will return null if the Object could not be found.
    [Networked] public NetworkObject MyNetworkObject { get; set; }

    // Networked NetworkBehaviour with auto properties:
    // The ILWeaver replaces the Get method with an implementation which writes the NetworkBehaviourId
    // to the snapshot, and the Set method implementation which finds the Object using the NetworkBehaviourId.
    // Get will return the Object if found.
    // Get will return null if the property was set to null, and also will return null if the Object could not be found.
    [Networked] public NetworkBehaviour MyNetworkBehaviour { get; set; }

    // NetworkId is backed by one int value.
    // Zero represents null/Invalid.
    [Networked] public NetworkId ParentNetId { get; set; }

    // NetworkBehaviourId is backed by two values:
    // Object = The NetworkId of the NetworkObject
    // Behaviour = NetworkBehaviour index in the hierarchy of the NetworkObject.
    [Networked] public NetworkBehaviourId ParentNBId { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            CaptureParent();
        }
        else
        {
            ApplyParent();
        }
    }

    private void CaptureParent()
    {
        var parent = transform.parent;

        // If the current parent is null, set the Object value to default (0).
        // This represents null.
        if (parent == null)
        {
            ParentNetId = default;
            ParentNBId = default;
            return;
        }

        // Find a NetworkBehaviour on the parent and capture its NetworkId.
        var parentNB = parent.GetComponent<NetworkBehaviour>();
        if (parentNB != null)
        {
            ParentNBId = parentNB.Id;
            ParentNetId = default;
        }
        // if the parent does not have a NetworkBehaviour component,
        // find a NetworkObject instead.  
        else
        {
            var parentNO = parent.GetComponent<NetworkObject>();
            if (parentNO != null)
            {
                ParentNetId = parentNO.Id;
                ParentNBId = default;
            }
            else
            {
                Debug.LogWarning("Parent on StateAuthority does not have a NetworkObject or NetworkBehaviour. Cannot be identified and serialized.");
                ParentNetId = default;
                ParentNBId = default;
            }
        }
    }

    private void ApplyParent()
    {
        // Test for an NetworkId == 0 (same as Invalid),
        // which represents an explicit null value.
        if (ParentNBId.Object.IsValid == false && ParentNetId.IsValid == false)
        {
            transform.SetParent(null);
            return;
        }

        // If the parent was a NetworkObject Id, try to unwrap it.
        if (ParentNetId.IsValid)
        {
            if (Runner.TryFindObject(ParentNetId, out var parentNO))
            {
                transform.SetParent(parentNO.transform);
            }
            else
            {
                Debug.LogWarning($"NetworkObject:{ParentNBId.Object} not found on the local runner. " +
                                 "NetworkObject is possibly out of Interest.");
                transform.SetParent(null);
            }
            return;
        }

        // if the parent was a NetworkBehaviour, try to unwrap it.
        if (ParentNBId.IsValid)
        {
            if (Runner.TryFindBehaviour(ParentNBId, out var parentNB))
            {
                transform.SetParent(parentNB.transform);
            }
            else
            {
                Debug.LogWarning($"NetworkObject:{ParentNBId.Object} NB:{ParentNBId.Behaviour} not found on the local runner. " +
                                 "NetworkObject is possibly out of Interest.");
                transform.SetParent(null);
            }
        }
    }
}