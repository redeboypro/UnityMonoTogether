using UnityEngine;

namespace UnityMonoTogether
{
    public static class MonoTogether
    {
        public static void Initialize()
        {
            var managerObject = new GameObject(nameof(NetworkManager));
            UnityEngine.Object.DontDestroyOnLoad(managerObject);
            var networkManager = managerObject.AddComponent<NetworkManager>();
            networkManager.OnLoad();
        }
    }
}
