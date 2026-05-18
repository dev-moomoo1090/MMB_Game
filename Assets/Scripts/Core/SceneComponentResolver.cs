using UnityEngine;

namespace MMBGame
{
    public static class SceneComponentResolver
    {
        public static T Resolve<T>(T current) where T : Object
        {
            return current != null ? current : Object.FindFirstObjectByType<T>();
        }

        public static T Resolve<T>() where T : Object
        {
            return Object.FindFirstObjectByType<T>();
        }

        public static T ResolveOrAdd<T>(T current, GameObject target) where T : Component
        {
            T resolved = Resolve(current);
            return resolved != null || target == null ? resolved : target.AddComponent<T>();
        }
    }
}
