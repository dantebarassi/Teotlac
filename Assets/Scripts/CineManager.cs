using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CineManager : MonoBehaviour
{
    public static CineManager instance;
    public Dictionary<cineEnum, PlayableDirector> DplayableDirector = new Dictionary<cineEnum, PlayableDirector>();
    [SerializeField] PlayableDirector PobsidianDead;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DplayableDirector.Add(cineEnum.obsidianDead, PobsidianDead);
    }
    public void PlayAnimation(cineEnum cinePasa)
    {
        if(DplayableDirector.ContainsKey(cinePasa))
        {
            DplayableDirector[cinePasa].Play();
        }
    }
    public enum cineEnum{
        obsidianDead
    }
}
