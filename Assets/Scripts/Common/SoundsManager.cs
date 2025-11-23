using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : SingletonDontDestroyOnLoad<SoundsManager>
{
    [SerializeField] private SoundsData[] _soundsDatas;
    private Dictionary<SoundType, AudioClipData> _soundIdPairs = new();

    private void Start()
    {
        foreach (SoundsData soundData in _soundsDatas)
            _soundIdPairs.Add(soundData.SoundType, soundData.AudioClipData);
    }

    public void PlaySound(SoundType soundType)
    {
        if (!SaveLoadSystem.data.SoundsOn)
            return;

        if(!_soundIdPairs.ContainsKey(soundType))
        {
            Debug.LogWarning($"There's no audioclip for sound type {soundType}!");
            return;
        }

        _soundIdPairs[soundType].AudioSource?.PlayOneShot(_soundIdPairs[soundType].AudioClip);
    }

    [Serializable]
    public class AudioClipData
    {
        public AudioClip AudioClip;
        public AudioSource AudioSource;
    }

    [Serializable] 
    public class SoundsData
    {
        public SoundType SoundType;
        public AudioClipData AudioClipData;
    }
}

public enum SoundType
{
    Button,
    Lose,
    Victory,
    TileCollected,
    TilesMatch,
    GameStart,
}