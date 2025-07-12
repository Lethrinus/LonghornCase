using UnityEngine;
using DG.Tweening;

namespace Core{
public static class ClickableLock
{
    public static void Lock(this Collider col, float seconds)
    {
        col.enabled = false;
        DOVirtual.DelayedCall(seconds, () => col.enabled = true)
            .SetLink(col.gameObject, LinkBehaviour.KillOnDestroy);
    }
}

}
