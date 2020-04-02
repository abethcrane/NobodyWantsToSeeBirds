using System;
using UnityEngine;

public class OnScreenVisibilityEventer : MonoBehaviour
{
    public event Action BecameInvisible;
    public event Action BecameVisible;
	
	private void OnBecameInvisible()
	{
        BecameInvisible?.Invoke();
	}
	
	private void OnBecameVisible()
	{
        BecameVisible?.Invoke();
	}
}
