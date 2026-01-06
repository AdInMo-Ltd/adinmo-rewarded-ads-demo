using UnityEngine;
/// <summary>
/// An example of a class that can handle the automatic pausing of the game when the Magnifier is showing
/// </summary>
/// 
namespace Adinmo
{
    public class PauseHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            AdinmoManager.SetPauseGameCallback(GamePaused);
            AdinmoManager.SetResumeGameCallback(GameResumed);
        }


        public void GamePaused()
        {
            //Add game specific code for handling pausing the game
            Time.timeScale = 0;
        }

        public void GameResumed()
        {
            //Add game specific code for handling resuming the game
            Time.timeScale = 1;
        }
    }
}