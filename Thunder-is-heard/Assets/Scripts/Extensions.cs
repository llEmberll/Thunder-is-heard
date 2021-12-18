using UnityEngine;

public static class Extensions
{

    public static GameObject FindChildByTag(this Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.tag == tag)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
