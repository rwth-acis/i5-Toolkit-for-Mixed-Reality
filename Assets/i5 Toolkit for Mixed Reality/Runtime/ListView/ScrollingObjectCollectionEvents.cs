using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScrollingObjectCollection))]
public class ScrollingObjectCollectionEvents : MonoBehaviour
{
    private ScrollingObjectCollection scroller;

    private int scrollingActiveCounter;

    public UnityEvent OnScrollingUpdate;

    private int ScrollingActiveCounter
    {
        get => scrollingActiveCounter;
        set
        {
            scrollingActiveCounter = value;
            this.enabled = scrollingActiveCounter > 0;
        }
    }

    private void Awake()
    {
        scroller = GetComponent<ScrollingObjectCollection>();
        scroller.OnTouchStarted.AddListener(OnTouchStarted);
        scroller.OnTouchEnded.AddListener(OnTouchEnded);
        scroller.OnMomentumStarted.AddListener(OnMomentumStarted);
        scroller.OnMomentumEnded.AddListener(OnMomentumEnded);
        this.enabled = false;
    }

    private void OnTouchStarted(GameObject sender)
    {
        ScrollingActiveCounter++;
    }

    private void OnTouchEnded(GameObject arg0)
    {
        ScrollingActiveCounter--;
    }

    private void OnMomentumStarted()
    {
        ScrollingActiveCounter++;
    }

    private void OnMomentumEnded()
    {
        ScrollingActiveCounter--;
    }

    private void Update()
    {
        OnScrollingUpdate.Invoke();
    }
}
