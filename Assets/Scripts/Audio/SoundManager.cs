using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
PARA METER SONIDOS HAY QUE METER EL ARCHIVO DE SONIDO EN EL SERIALIZEFIELD, PUEDES PONERLE UN NOMBRE O NO HACERLO, EN CASO DE NO HACERLO
COGE EL NOMBRE DEL ARCHIVO.

LUEGO DESDE LA FUNCION DE CUALQUIER OTRO OBJETO O SCRIPT USAS ESTA LINEA:
SoundManager.Instance.PlaySound("nombre_de_audio", position);

int "nombre_de_audio" -- NOMBRE QUE LE HAYAS PUESTO
Vector3 position -- PASARLE EL transform.position O LA POSICION EN LA QUE SE PRODUZCA EL SONIDO EN CASO DE QUE SEA INGAME Y NO QUIERES QUE SEA UN SONIDO GLOBAL
*/

public class SoundManager : MonoBehaviour
{
    
    public static SoundManager Instance;

    [SerializeField] private List<SoundEffect> soundEffects;
    [SerializeField] private Dictionary<string, AudioClip> soundLibrary;
    private List<AudioSource> activeAudioSources;

    private Player player;
    private Vector3 playerPosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        soundLibrary = new Dictionary<string, AudioClip>();

        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.clip != null) {

                if (string.IsNullOrEmpty(soundEffect.name))
                {
                    soundEffect.name = soundEffect.clip.name;
                    soundLibrary[soundEffect.name] = soundEffect.clip;
                    Debug.Log(soundEffect.name);
                    Debug.Log(soundEffect.clip);
                }
                else 
                {
                    soundLibrary[soundEffect.name] = soundEffect.clip;
                    Debug.Log(soundEffect.name);
                    Debug.Log(soundEffect.clip);
                }
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayer(Player currentPlayer)
    {
        player = currentPlayer;
    }

    public void PlaySound(string soundName)
    {
        
        GameObject soundObject = new GameObject();
        
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();
        audioSource.clip = soundLibrary[soundName];
        audioSource.Play();

        Destroy(soundObject, audioSource.clip.length);
    }

    public void PlaySound(string soundName, Vector3 position)
    {
        if (!soundLibrary.ContainsKey(soundName))
        {
            return;
        }

        if (position != null)
        {
            if (player != null)
            {
                playerPosition = player.cameraPosition;

                if ((Mathf.Abs(position.x - playerPosition.x) > 10) || (Mathf.Abs(position.y - playerPosition.y) > 6))
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        PlaySound(soundName);
    }

}

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
}