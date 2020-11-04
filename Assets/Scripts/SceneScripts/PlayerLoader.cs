using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class PlayerLoader : MonoBehaviour {
    public SceneReference playerScene;

    IEnumerator Start() {
        yield return CustomSceneManager.WaitForSceneLoadedOrUnloaded(playerScene);

        if (CustomSceneManager.IsSceneUnloaded(playerScene)) {
            yield return CustomSceneManager.LoadSceneAsync(playerScene, LoadSceneMode.Additive);
            GameObject player = GameObject.FindWithTag("Player");
            player.transform.position = transform.position;
        }
    }
}
