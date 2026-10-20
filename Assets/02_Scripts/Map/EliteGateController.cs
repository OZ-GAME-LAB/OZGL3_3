using UnityEngine;

public class EliteGateController : MonoBehaviour, IBreakable
{
    private bool _isBroken;

    public void Break()
    {
        if (_isBroken)
        {
            return;
        }

        _isBroken = true;
        gameObject.SetActive(false);
    }
}
