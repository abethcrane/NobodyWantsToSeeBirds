using System;
using UnityEngine;

// This offscreen system became a little more complex than I would have liked, given we can have multiple children sprites
// So basically when a child sprite becomes invisible invisible we set the checkOffscreen to true
// When a child sprite becomes visible,  wet set the checkOffscreen back to false
// Every update(), if the check is treu then we queue up a more thorough check and indicate that with _checkingoffscren
// The actual check waits 1 second, and then if the check flag is still on it loops through all children
// And only if they're all invisible does it trigger its BecameInvisible event

public class VisibilityEventer : MonoBehaviour
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
