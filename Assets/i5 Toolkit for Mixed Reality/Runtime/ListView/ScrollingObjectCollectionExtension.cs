using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScrollingObjectCollection))]
public class ScrollingObjectCollectionExtension : MonoBehaviour
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
            //if (scrollingActiveCounter > 0)
            //{
            //    this.enabled = true;
            //}
        }
    }

    private IEnumerator DelayedTurnoff()
    {
        yield return null;
        this.enabled = false;
    }

    private void Awake()
    {
        scroller = GetComponent<ScrollingObjectCollection>();
        scroller.OnTouchStarted.AddListener(OnTouchStarted);
        scroller.OnTouchEnded.AddListener(OnTouchEnded);
        scroller.OnMomentumStarted.AddListener(OnMomentumStarted);
        scroller.OnMomentumEnded.AddListener(OnMomentumEnded);
        //this.enabled = false;
    }

    private void OnDestroy()
    {
        scroller.OnTouchStarted.RemoveListener(OnTouchStarted);
        scroller.OnTouchEnded.RemoveListener(OnTouchEnded);
        scroller.OnMomentumStarted.RemoveListener(OnMomentumStarted);
        scroller.OnMomentumEnded.RemoveListener(OnMomentumEnded);
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

        //if (scrollingActiveCounter == 0)
        //{
        //    this.enabled = false;
        //}
    }

    public void ScrollByTier(int shift)
    {
        scroller.MoveByTiers(shift);
    }
}
