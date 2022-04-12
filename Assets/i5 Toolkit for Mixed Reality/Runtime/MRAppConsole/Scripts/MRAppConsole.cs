using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.UI;

[RequireComponent(typeof(SolverHandler))]
[RequireComponent(typeof(ObjectManipulator))]
public class MRAppConsole : MonoBehaviour
{
    [SerializeField]
    private SolverHandler solver;
    [SerializeField]
    private ObjectManipulator manipulator;
    public void ActivateHeadTracking()
    {
        solver.UpdateSolvers = true;
        manipulator.enabled = false;
    }

    public void ActivateFreePlacement()
    {
        solver.UpdateSolvers = false;
        manipulator.enabled = true;
    }
}
