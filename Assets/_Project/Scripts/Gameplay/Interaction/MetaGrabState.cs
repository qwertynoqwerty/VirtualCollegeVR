using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class MetaGrabState
{
    private static readonly string[] BoolGrabProperties =
    {
        "IsGrabbed",
        "IsBeingGrabbed",
        "IsSelected",
        "Selected",
        "HasSelectedInteractor",
        "HasInteractors",
        "IsInteracting"
    };

    private static readonly string[] CountGrabProperties =
    {
        "InteractorsCount",
        "SelectingInteractorsCount",
        "SelectingPointsCount",
        "GrabPointsCount"
    };

    private static readonly string[] CollectionGrabProperties =
    {
        "Interactors",
        "SelectingInteractors",
        "GrabPoints"
    };

    private static readonly string[] ReleaseMethods =
    {
        "ForceRelease",
        "Release",
        "EndGrab",
        "Unselect",
        "RemoveInteractor",
        "ForceSelectExit",
        "EndInteraction",
        "Drop"
    };

    public static bool IsGrabbed(Component component)
    {
        return component != null && IsGrabbed(component.gameObject);
    }

    public static bool IsGrabbed(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        Component[] components = GetGrabComponents(gameObject);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null || IsGrabComponent(component) == false)
            {
                continue;
            }

            if (ReadGrabState(component))
            {
                return true;
            }
        }

        return false;
    }

    public static void Release(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        Component[] components = GetGrabComponents(gameObject);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null || IsGrabComponent(component) == false)
            {
                continue;
            }

            InvokeRelease(component);
        }
    }

    private static bool IsGrabComponent(Component component)
    {
        Type type = component.GetType();
        string typeName = type.Name;
        string fullName = type.FullName ?? string.Empty;

        return typeName.IndexOf("Grab", StringComparison.OrdinalIgnoreCase) >= 0 ||
               fullName.IndexOf("Oculus.Interaction", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static Component[] GetGrabComponents(GameObject gameObject)
    {
        Component[] ownAndChildren = gameObject.GetComponentsInChildren<Component>(true);
        Component[] parents = gameObject.GetComponentsInParent<Component>(true);

        Component[] result = new Component[ownAndChildren.Length + parents.Length];
        ownAndChildren.CopyTo(result, 0);
        parents.CopyTo(result, ownAndChildren.Length);
        return result;
    }

    private static bool ReadGrabState(Component component)
    {
        Type type = component.GetType();

        for (int i = 0; i < BoolGrabProperties.Length; i++)
        {
            PropertyInfo property = type.GetProperty(BoolGrabProperties[i], BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(bool))
            {
                object value = TryGetPropertyValue(property, component);
                if (value is bool grabbed && grabbed)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < CountGrabProperties.Length; i++)
        {
            PropertyInfo property = type.GetProperty(CountGrabProperties[i], BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(int))
            {
                object value = TryGetPropertyValue(property, component);
                if (value is int count && count > 0)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < CollectionGrabProperties.Length; i++)
        {
            PropertyInfo property = type.GetProperty(CollectionGrabProperties[i], BindingFlags.Instance | BindingFlags.Public);
            if (property != null && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                object value = TryGetPropertyValue(property, component);
                if (HasAnyItem(value as IEnumerable))
                {
                    return true;
                }
            }
        }

        PropertyInfo stateProperty = type.GetProperty("State", BindingFlags.Instance | BindingFlags.Public);
        if (stateProperty != null)
        {
            object value = TryGetPropertyValue(stateProperty, component);
            if (value != null && value.ToString().IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyItem(IEnumerable enumerable)
    {
        if (enumerable == null)
        {
            return false;
        }

        IEnumerator enumerator = enumerable.GetEnumerator();
        return enumerator != null && enumerator.MoveNext();
    }

    private static void InvokeRelease(Component component)
    {
        Type type = component.GetType();
        for (int i = 0; i < ReleaseMethods.Length; i++)
        {
            MethodInfo[] methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => string.Equals(m.Name, ReleaseMethods[i], StringComparison.Ordinal))
                .ToArray();

            for (int j = 0; j < methods.Length; j++)
            {
                TryInvokeReleaseMethod(methods[j], component);
            }
        }
    }

    private static object TryGetPropertyValue(PropertyInfo property, object target)
    {
        if (property == null || target == null)
        {
            return null;
        }

        try
        {
            return property.GetValue(target);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void TryInvokeMethod(MethodInfo method, object target)
    {
        if (method == null || target == null)
        {
            return;
        }

        try
        {
            method.Invoke(target, null);
        }
        catch (Exception)
        {
        }
    }

    private static void TryInvokeReleaseMethod(MethodInfo method, object target)
    {
        if (method == null || target == null)
        {
            return;
        }

        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            TryInvokeMethod(method, target);
            return;
        }

        if (parameters.Length == 1)
        {
            TryInvokeMethodWithNull(method, target);
        }
    }

    private static void TryInvokeMethodWithNull(MethodInfo method, object target)
    {
        if (method == null || target == null)
        {
            return;
        }

        try
        {
            method.Invoke(target, new object[] { null });
        }
        catch (Exception)
        {
        }
    }
}
