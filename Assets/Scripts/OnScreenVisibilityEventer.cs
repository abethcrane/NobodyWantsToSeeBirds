using System;
using UnityEngine;

public class OnScreenVisibilityEventer : MonoBehaviour
{
    public event Action BecameInvisible;
	
	private void OnBecameInvisible()
	{
        BecameInvisible?.Invoke();
	}
}
