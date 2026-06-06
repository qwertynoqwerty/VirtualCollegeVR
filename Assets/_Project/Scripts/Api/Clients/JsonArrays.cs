using UnityEngine;

public static class JsonArrays
{
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }

    public static T[] FromJson<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new T[0];
        }

        string wrapped = "{\"items\":" + json + "}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        if (w == null)
        {
            return new T[0];
        }
        else
        {
            if (w.items == null)
            {
                return new T[0];
            }
            else
            {
                return w.items;
            }
        }
    }
}