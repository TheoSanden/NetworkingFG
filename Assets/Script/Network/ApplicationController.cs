using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        bool hasGPU = SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;
        StartInMode(!hasGPU);
    }

    async void StartInMode(bool isDedicatedServer) 
    {
        if (isDedicatedServer) {
            
        }
        else {
            await HostSingleton.GetInstance().InitServerAsync();
            await ClientSingleton.GetInstance().InitClientAsync();
            //if everything init then go to main menu

            if (ClientSingleton.GetInstance().authenticated) {
                SceneManager.LoadScene("S_MainMenu");
            }
        }    
        
      
    }
}
