
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AudioPriority
{
    Medium = 0,
    High = 1
}
/// <summary>
/// 音效源数据（用于追踪每个 AudioSource 的状态）
/// </summary>
public class AudioSourceData
{
    public AudioSource audioSource;
    public float volumeRate = 1f;
    public float baseVolume = 1f;

    public AudioPriority priority;

    public void SetVolume(float volume, float rate)
    {
        baseVolume = volume;
        volumeRate = rate;
        audioSource.volume = baseVolume * volumeRate;
    }


    public void RefreshVolume(float volume)
    {
        baseVolume = volume;
        audioSource.volume = baseVolume * volumeRate;
    }
}


public class AudioManager : Singleton<AudioManager>
{
    private static Dictionary<AudioClip, List<float>> audioPlayTimes = new Dictionary<AudioClip, List<float>>();
    
    public static GameObject gameObject;
    
    private AudioSource bgSound;
    
    // 音效 AudioSource 池
    private List<AudioSourceData> soundPool = new List<AudioSourceData>();
    private int maxSoundPoolSize = 20; // 最多同时播放 20 个音效,意味着有池子的容量为20
    
    // 用于追踪每个 AudioClip 正在播放的 AudioSource
    private Dictionary<AudioClip, List<AudioSourceData>> activeSounds = new Dictionary<AudioClip, List<AudioSourceData>>();
    
    // 中优先级音效在高优先级存在时的音量衰减比例
    private float mediumPriorityAttenuation = 0.3f;
    
    private float musicValue => this.IsMusicMult ? 0 : this.MusicValue;// 这个是专门用来设置这个脚本里面的音量播放的
    private float _MusicValue;
    public float MusicValue
    {
        get
        {
            // float.Epsilon：是 float 类型能表示的最小正数（约为 1.4e-45）
            if (Math.Abs(this._MusicValue - IC.NotFound) < float.Epsilon)
            {
                this._MusicValue = PlayerPrefs.GetFloat("musicValue", 1f);
            }
            return this._MusicValue;
        }
        set
        {
            this._MusicValue = value;
            PlayerPrefs.SetFloat("musicValue", value);
            PlayerPrefs.Save();
            this.refresnMusicVolume();
        }
    }
    
    private int maxSoundCount => 17;
    private float soundValue => this.IsSoundMult ? 0 : this.SoundValue;
    private float _SoundValue;
    public float SoundValue
    {
        get
        {
            if (Math.Abs(this._SoundValue - IC.NotFound) < float.Epsilon)
            {
                this._SoundValue = PlayerPrefs.GetFloat("soundValue", 1);
            }
            return this._SoundValue;
        }
        set
        {
            this._SoundValue = value;
            PlayerPrefs.SetFloat("soundValue", value);
            PlayerPrefs.Save();
            this.refreshAllSound();
        }
    }
    
    public bool IsMusicMult
    {
        get => PlayerPrefs.GetInt("IsMusicMult", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsMusicMult", value ? 1 : 0);
            PlayerPrefs.Save();
            this.refresnMusicVolume();
        }
    }
    public bool IsSoundMult
    {
        get => PlayerPrefs.GetInt("IsSoundMult", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsSoundMult", value ? 1 : 0);
            PlayerPrefs.Save();
            this.refreshAllSound();
        }
    }
    
    /// <summary>
        /// 播放音效（主方法 - 所有音效通过此方法播放）
        /// </summary>
        /// <param name="clipName">音效名称</param>
        /// <param name="volumeRate">音量比例（默认1.0）</param>
        /// <param name="isLoop">是否循环播放</param>
        /// <param name="maxCountInTimeWindow">时间窗口内最大播放次数</param>
        /// <param name="timeWindow">时间窗口（秒）</param>
        /// <returns>返回 AudioSource 以便控制播放</returns>
        public AudioSource PlaySound(string clipName, bool isLoop = false, AudioPriority priority = AudioPriority.Medium,
        float volumeRate = 1, int maxCountInTimeWindow = 1, float timeWindow = 0f)
        {
            var clip = CacheManager.Instance.loadAudioClipByAssetBundle(clipName);

            float duration = clip.length;

            if (timeWindow == 0f)
            {
                timeWindow = duration; // 设置同一个音效同一时间只能播放一个。
            }

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 音效不存在: {clipName}");
                return null;
            }

            // 检查播放频率限制，防止短时间内反复触发同一个音效。
            if (!CanPlaySound(clip, maxCountInTimeWindow, timeWindow))
            {
                return null;
            }

            // 只关心 “当前还在播放的实例数”，你快速连续播放一个 长音效（比如 3 秒的爆炸回响），第 4 次会顶掉最早的。（基于当前活跃列表）
            if (activeSounds.ContainsKey(clip) && activeSounds[clip].Count >= this.maxSoundCount)
            {
                var oldestSound = activeSounds[clip][0];
                // audioSource.isPlaying 会立即变为 false。音频停止播放，不再发出声
                // AudioSource 对象本身不会被销毁
                oldestSound.audioSource.Stop(); 
                activeSounds[clip].RemoveAt(0);
            }

            // 获取可用的 AudioSource
            AudioSourceData sourceData = GetAvailableAudioSource();
            if (sourceData == null)
            {
                Debug.LogWarning("[AudioManager] 无可用的 AudioSource，音效池已满");
                return null;
            }

            sourceData.priority = priority;
            float effectiveVolumeRate = volumeRate;
            if (priority == AudioPriority.Medium && IsAnyHighPrioritySoundPlaying())
            {
                effectiveVolumeRate *= mediumPriorityAttenuation;
            }

            // 配置并播放音效
            sourceData.audioSource.clip = clip;
            sourceData.SetVolume(this.soundValue, effectiveVolumeRate);
            sourceData.audioSource.loop = isLoop;
            sourceData.audioSource.spatialBlend = 0f; // 2D 音效
            sourceData.audioSource.Play();

            // 添加到活跃音效列表
            if (!activeSounds.ContainsKey(clip))
            {
                activeSounds[clip] = new List<AudioSourceData>();
            }
            activeSounds[clip].Add(sourceData);

            // 记录播放时间
            RecordPlayTime(clip);

            return sourceData.audioSource;
        }

    // 检查是否有任何高优先级音效正在播放
    private bool IsAnyHighPrioritySoundPlaying()
    {
        foreach (var list in activeSounds.Values)
        {
            foreach (var data in list)
            {
                if (data.priority == AudioPriority.High && data.audioSource.isPlaying)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    // 获取可用的 AudioSource（自动管理池）
    private AudioSourceData GetAvailableAudioSource()
    {
        // 1. 查找已停止播放的 AudioSource
        foreach (var sourceData in soundPool)
        {
            if (!sourceData.audioSource.isPlaying)
            {
                return sourceData;
            }
        }
        // 2. 如果池未满，创建新的 AudioSource
        if (soundPool.Count < maxSoundPoolSize)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            AudioSourceData newData = new AudioSourceData
            {
                audioSource = newSource
            };
            soundPool.Add(newData);
            return newData;
        }
        // 3. 池已满，查找最早播放的非循环音效并复用
        foreach (var sourceData in soundPool)
        {
            if (!sourceData.audioSource.loop)
            {
                sourceData.audioSource.Stop();
                return sourceData;
            }
        }

        // 4. 所有都是循环音效，返回 null
        return null;
    }
    
    
    //检查音效是否可以播放（基于频率限制）
    private bool CanPlaySound(AudioClip clip, int maxCount, float timeWindow)
    {
        if (clip == null || maxCount <= 0)
        {
            return false;
        }
        float currentTime = Time.time;
        
        // 获取或创建该音效的播放时间列表
        if (!audioPlayTimes.ContainsKey(clip))
        {
            audioPlayTimes[clip] = new List<float>();
        }

        var playTimes = audioPlayTimes[clip];
        
        // 移除超出时间窗口的记录
        playTimes.RemoveAll(time => currentTime - time > timeWindow);

        // 检查是否超过限制
        if (playTimes.Count >= maxCount)
        {
            return false;
        }

        return true;
    }
    
    // 刷新所有音效音量
    public void refreshAllSound()
    {
        // 刷新所有活跃音效的音量
        foreach (var kvp in activeSounds)
        {
            foreach (var sourceData in kvp.Value)
            {
                sourceData.RefreshVolume(this.soundValue);
            }
        }
    }
    
    //刷新音乐音量
    void refresnMusicVolume()
    {
        bgSound.volume = this.musicValue;
    }

    // 记录音效播放时间
    private void RecordPlayTime(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (!audioPlayTimes.ContainsKey(clip))
        {
            audioPlayTimes[clip] = new List<float>();
        }

        audioPlayTimes[clip].Add(Time.time);
    }
    
    // 停止指定的 AudioSource
    public void StopSound(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
    
    // 停止指定音效片段的所有播放实例
    public void StopSound(AudioClip clip)
    {
        if (activeSounds.TryGetValue(clip, out var soundList))
        {
            // 遍历所有该剪辑的播放实例并停止
            foreach (var sourceData in soundList)
            {
                sourceData.audioSource.Stop();
                sourceData.audioSource.clip = null;
            }
            // 清空该剪辑的播放列表
            soundList.Clear();
        }
    }
    
    // 停止指定名称的音效剪辑的所有播放实例
    public void StopSound(string clipName)
    {
        var clip = CacheManager.Instance.loadAudioClipByAssetBundle(clipName);
        if (clip != null)
        {
            StopSound(clip);
        }
    }
    
    // 停止所有正在播放的音效
    public void StopAllSounds()
    {
        foreach (var sourceData in soundPool)
        {
            if (sourceData.audioSource.isPlaying)
            {
                sourceData.audioSource.Stop();
                sourceData.audioSource.clip = null;
            }
        }
        activeSounds.Clear();
    }
}
