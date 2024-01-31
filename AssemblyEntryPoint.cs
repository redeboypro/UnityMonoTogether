using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityMonoTogether
{
    public static class AssemblyEntryPoint
    {
        private static void Initialize()
        {
            var managerObject = new GameObject(nameof(NetworkManager));
            Object.DontDestroyOnLoad(managerObject);
            var networkManager = managerObject.AddComponent<NetworkManager>();
        }
    }
}
