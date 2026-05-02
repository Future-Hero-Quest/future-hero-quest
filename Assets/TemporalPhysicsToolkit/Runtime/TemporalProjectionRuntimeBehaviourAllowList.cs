using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITemporalProjectionRuntimeBehaviour
{
    bool RunInTemporalProjection { get; }
}

public interface ITemporalProjectionStepBehaviour : ITemporalProjectionRuntimeBehaviour
{
    void SimulateProjectionStep(PhysicsScene physicsScene, float stepDuration, int simulatedStep);
}

[DisallowMultipleComponent]
public sealed class TemporalProjectionRuntimeBehaviourAllowList : MonoBehaviour
{
    [SerializeField] private bool includeChildren = true;
    [SerializeField] private bool forceEnableAllowedBehaviours = true;
    [SerializeField] private List<string> allowedBehaviourTypeNames = new List<string>();

    public bool IncludeChildren => includeChildren;
    public bool ForceEnableAllowedBehaviours => forceEnableAllowedBehaviours;
    public IList<string> AllowedBehaviourTypeNames => allowedBehaviourTypeNames;

    public void AllowBehaviourType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return;
        }

        if (!allowedBehaviourTypeNames.Contains(typeName))
        {
            allowedBehaviourTypeNames.Add(typeName);
        }
    }

    public bool Allows(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return false;
        }

        if (!includeChildren && behaviour.gameObject != gameObject)
        {
            return false;
        }

        Type behaviourType = behaviour.GetType();
        foreach (string allowedTypeName in allowedBehaviourTypeNames)
        {
            if (string.IsNullOrWhiteSpace(allowedTypeName))
            {
                continue;
            }

            if (string.Equals(behaviourType.Name, allowedTypeName, StringComparison.Ordinal) ||
                string.Equals(behaviourType.FullName, allowedTypeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
