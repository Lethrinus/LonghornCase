// ClickFxRegistry.cs  (tamamı)
using System.Collections.Generic;
using UnityEngine;
using Clickables;

public static class ClickFxRegistry
{
    static readonly HashSet<Collider> knownGood = new();
    static readonly HashSet<Collider> knownBad  = new();

    /// Tek satır API
    public static bool IsClickableCollider(Collider col)
    {
        if (knownGood.Contains(col)) return true;
        if (knownBad .Contains(col)) return false;

        // İlk defa görüyoruz – hiyerarşide IClickable var mı?
        bool ok = col.GetComponentInParent<IClickable>() != null;

        (ok ? knownGood : knownBad).Add(col);
        return ok;
    }
}