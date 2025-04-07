#if !UNITY
namespace UnityEngine
{
    // Mock classes for when building without Unity references
    public class Object
    {
        public static T[] FindObjectsOfType<T>() where T : class => new T[0];
    }

    public class MonoBehaviour
    {
    }

    public class Time
    {
        public static float deltaTime => 0.016f;
    }

    public class GameObject
    {
        public int GetInstanceID() => 0;
    }
}
#endif 