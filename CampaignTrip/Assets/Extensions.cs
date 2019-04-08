using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void Initialize<T>(this T[] collection, Func<T> value)
    {
        for (int i = 0; i < collection.Length; i++)
        {
            collection[i] = value();
        }
    }

    public static void TryPlay(this AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null)
            audioSource.PlayOneShot(audioClip);
    }

    public static T Random<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default;

        int i = UnityEngine.Random.Range(0, list.Count);
        return list[i];
    }

    public static T Random<T>(this T[] list)
    {
        if (list == null || list.Length == 0)
            return default;

        int i = UnityEngine.Random.Range(0, list.Length);
        return list[i];
    }
}
