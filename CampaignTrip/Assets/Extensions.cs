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
}
