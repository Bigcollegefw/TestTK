using System;
using System.Collections.Generic;
using UnityEngine;

namespace TKFramework
{
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

        // 是否需要音效播放完之后才能停止
        public bool pendingStop = false;

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
        // 音效播放时间记录，用于限制同一音效的播放频率
        private static Dictionary<AudioClip, List<float>> audioPlayTimes = new Dictionary<AudioClip, List<float>>();
        public static GameObject gameObject;
        private AudioSource bgSound;

        // 音效 AudioSource 池,音效停止播放的时候，会直接从activeSounds移除这个键，但是再Unity组件上这个AudioClip还是存在的留在这个池子里面
        private List<AudioSourceData> soundPool = new List<AudioSourceData>();
        private int maxSoundPoolSize = 20; // 最多同时播放 20 个音效

        // 用于追踪每个 AudioClip 正在播放的 AudioSource,这个用于实时维护哪些个音效在播放。不播放即刻移除。
        private Dictionary<AudioClip, List<AudioSourceData>> activeSounds = new Dictionary<AudioClip, List<AudioSourceData>>();

        // 中优先级音效在高优先级存在时的音量衰减比例
        private float mediumPriorityAttenuation = 0.5f;

        private float musicValue => this.IsMusicMult ? 0 : this.MusicValue;
        private float _MusicValue;
        public float MusicValue
        {
            get
            {
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

        public override void Init()
        {
            this._SoundValue = IC.NotFound;
            this._MusicValue = IC.NotFound;
            if (gameObject == null)
            {
                gameObject = new GameObject("AudioManager");
                GameObject.DontDestroyOnLoad(gameObject);
            }

            GameObject.DontDestroyOnLoad(gameObject);
            bgSound = gameObject.AddComponent<AudioSource>();
            bgSound.playOnAwake = true;
            bgSound.loop = true;
        }

        public void update(float dt)
        {
            // 清理已停止播放的音效
            foreach (var kvp in activeSounds)
            {
                kvp.Value.RemoveAll(data => !data.audioSource.isPlaying);
            }

            foreach (var sourceData in soundPool)
            {
                // 检查是否标记了待停止且当前正在播放
                if (sourceData.pendingStop && sourceData.audioSource.isPlaying)
                {
                    // 检查是否接近当前循环结束（播放时间大于等于这个音效的时候）
                    if (sourceData.audioSource.time >= sourceData.audioSource.clip.length-0.05f)
                    {
                        StopAudioSource(sourceData.audioSource);
                        sourceData.pendingStop = false;
                    }
                }
            }
        }

        //播放背景音乐
        public void Play(string clipName)
        {
            if (bgSound.clip == null || bgSound.clip.name != clipName)
            {
                var clip = CacheManager.Instance.loadAudioClipByAssetBundle(clipName);
                bgSound.clip = clip;
                bgSound.playOnAwake = true;
                bgSound.loop = true;
                bgSound.volume = this.musicValue;
            }
            bgSound.Play();
        }

        //停止背景音乐
        public void Stop()
        {
            bgSound.Stop();
        }

        //刷新音乐音量
        void refresnMusicVolume()
        {
            bgSound.volume = this.musicValue;
        }

        /// <summary>
        /// 播放音效（从列表中随机选择）
        /// </summary>
        public AudioSource PlaySound(List<string> clips, float volumeRate = 1, bool isLoop = false, int maxCountInTimeWindow = 1, float timeWindow = 0f)
        {
            if (clips.Count == 0)
            {
                return null;
            }
            return this.PlaySound(clips.getRandomOne(), isLoop, AudioPriority.Medium, volumeRate, maxCountInTimeWindow, timeWindow);
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
                timeWindow = duration;
            }

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 音效不存在: {clipName}");
                return null;
            }

            // 检查播放频率限制
            if (!CanPlaySound(clip, maxCountInTimeWindow, timeWindow))
            {
                return null;
            }

            // 检查同一音效播放数量限制
            if (activeSounds.ContainsKey(clip) && activeSounds[clip].Count >= this.maxSoundCount)
            {
                var oldestSound = activeSounds[clip][0];
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

        /// <summary>
        /// 检查是否有任何高优先级音效正在播放
        /// </summary>
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


        /// <summary>
        /// 获取可用的 AudioSource（自动管理池）
        /// </summary>
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

        /// <summary>
        /// 检查音效是否可以播放（基于频率限制）
        /// </summary>
        /// <param name="clip">音效剪辑</param>
        /// <param name="maxCount">时间窗口内最大播放次数</param>
        /// <param name="timeWindow">时间窗口（秒）</param>
        /// <returns>是否可以播放</returns>
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

        /// <summary>
        /// 记录音效播放时间
        /// </summary>
        /// <param name="clip">音效剪辑</param>
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

        /// <summary>
        /// 停止指定的 AudioSource
        /// </summary>
        public void StopAudioSource(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                var sourceData = soundPool.Find(data => data.audioSource == audioSource);
                if (sourceData != null)
                {
                    sourceData.pendingStop = false; // 清除标记
                }
                
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                }
            }
        }
        
        /// <summary>
        /// 停止指定音效剪辑的所有播放实例
        /// </summary>
        /// <param name="clip">要停止的音效剪辑</param>
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

        /// <summary>
        /// 停止指定名称的音效剪辑的所有播放实例
        /// </summary>
        /// <param name="clipName">要停止的音效名称</param>
        public void StopSound(string clipName)
        {
            var clip = CacheManager.Instance.loadAudioClipByAssetBundle(clipName);
            if (clip != null)
            {
                StopSound(clip);
            }
        }

        // 延迟停止循环音效的方法
        public void StopLoopSoundAfterCurrentCycle(string clipName)
        {
            var clip = CacheManager.Instance.loadAudioClipByAssetBundle(clipName);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] 找不到音效: {clipName}");
                return;
            }

            // 查找该音效所有正在播放的循环实例
            if (activeSounds.TryGetValue(clip, out var soundList))
            {
                foreach (var sourceData in soundList)
                {
                    if (sourceData.audioSource.isPlaying && sourceData.audioSource.loop)
                    {
                        sourceData.pendingStop = true;
                    }
                }
            }
        }

        /// <summary>
        /// 停止所有正在播放的音效
        /// </summary>
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

        /// <summary>
        /// 刷新所有音效音量
        /// </summary>
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

    }

    public class AudiosType
    {
        #region BGM 类型
        /// <summary>主页BGM 当玩家进入游戏主页时播放</summary>
        public static string Sound_Bgm_MainPage = "Sound_Bgm_MainPage";
        /// <summary>游戏页BGM 当玩家进入游戏页面时播放</summary>
        public static string Sound_Bgm_GamePage = "Sound_Bgm_GamePage";
        /// <summary>Loading页BGM 当玩家处于Loading页面时播放</summary>
        public static string Sound_Bgm_LoadingPage = "Sound_Bgm_LoadingPage";
        /// <summary>园圃页面BGM 当玩家处于园圃页面时播放</summary>
        public static string Sound_Bgm_GardenPage = "Sound_Bgm_GardenPage"; // 修正拼写：Graden → Garden
        #endregion

        #region UI交互音效
        /// <summary>按钮点击 点击任何普通按钮时播放</summary>
        public static string Sound_UIInteraction_Button = "Sound_UIInteraction_Button";
        /// <summary>弹窗打开 任意弹窗出现时播放</summary>
        public static string Sound_UIInteraction_PopupOpen = "Sound_UIInteraction_PopupOpen";
        /// <summary>弹窗关闭 任意弹窗关闭时播放</summary>
        public static string Sound_UIInteraction_PopupClose = "Sound_UIInteraction_PopupClose";
        /// <summary>页面切换 切换页面时播放</summary>
        public static string Sound_UIInteraction_PageSwitch = "Sound_UIInteraction_PageSwitch";
        /// <summary>进度条填充 进度条增长动画时播放</summary>
        public static string Sound_UIInteraction_ProgressFill = "Sound_UIInteraction_ProgressFill";
        /// <summary>开关切换 设置中切换开关时播放</summary>
        public static string Sound_UIInteraction_ToggleSwitch = "Sound_UIInteraction_ToggleSwitch";
        #endregion

        #region 核心玩法音效
        /// <summary>角色移动 当角色移动时播放</summary>
        public static string Sound_CoreGame_Move = "Sound_CoreGame_Move";
        /// <summary>收集收集物 当角色收集到收集物时播放</summary>
        public static string Sound_CoreGame_Collect = "Sound_CoreGame_Collect"; // 合并为单一字段，按清单第12项
        /// <summary>解锁目标点 当收集到梦境钥匙且解锁下一层目标点时播放</summary>
        public static string Sound_CoreGame_Unlock = "Sound_CoreGame_Unlock";
        /// <summary>死亡-掉落 当角色与空白地块交互且死亡时播放</summary>
        public static string Sound_CoreGame_Death_Drop = "Sound_CoreGame_Death_Drop";
        /// <summary>传送门进入 当角色与传送门地块交互且进入时播放</summary>
        public static string Sound_CoreGame_Interaction_Portal = "Sound_CoreGame_Interaction_Portal";
        /// <summary>传送门出现 当角色与传送门地块交互且出现时播放</summary>
        public static string Sound_CoreGame_Portal_Out = "Sound_CoreGame_Portal_Out";
        /// <summary>地刺冒出 当地刺地块激活时播放</summary>
        public static string Sound_CoreGame_landmine_Appear = "Sound_CoreGame_landmine_Appear";
        /// <summary>地刺收回 当地刺地块休眠时播放</summary>
        public static string Sound_CoreGame_landmine_Disappear = "Sound_CoreGame_landmine_Disappear";
        /// <summary>地块坍塌 当坍塌地块消失时播放</summary>
        public static string Sound_CoreGame_Collapse = "Sound_CoreGame_Collapse";
        /// <summary>方向转变 当角色与方向转变地块交互时播放</summary>
        public static string Sound_CoreGame_ChangeDirection = "Sound_CoreGame_ChangeDirection";
        /// <summary>旋转门激活 当角色与摇杆交互，摇杆与机关联动时播放</summary>
        public static string Sound_CoreGame_Interaction_RevolvingDoor = "Sound_CoreGame_Interaction_RevolvingDoor";
        #endregion

        #region 道具技能音效
        /// <summary>提示灯使用 使用道具“提示灯”时播放</summary>
        public static string Sound_PropSkills_HintLight = "Sound_PropSkills_HintLight";
        /// <summary>回溯使用 使用道具“回溯”时播放</summary>
        public static string Sound_PropSkills_Traceback = "Sound_PropSkills_Traceback";
        /// <summary>时间沙漏使用 使用道具“时间沙漏”时播放</summary>
        public static string Sound_PropSkills_Hourglass = "Sound_PropSkills_Hourglass";
        /// <summary>魔法帽子使用 使用道具“魔法帽子”时播放</summary>
        public static string Sound_PropSkills_MagicHat = "Sound_PropSkills_MagicHat";
        #endregion

        #region 系统反馈音效
        /// <summary>关卡胜利 通关结算页面弹出时播放</summary>
        public static string Sound_SystemFeedback_LevelSuccess = "Sound_SystemFeedback_LevelSuccess";
        /// <summary>关卡失败 失败结算页面弹出时播放</summary>
        public static string Sound_SystemFeedback_LevelFail = "Sound_SystemFeedback_LevelFail";
        /// <summary>宝箱开启 章节/故事宝箱开启时播放</summary>
        public static string Sound_SystemFeedback_TreasureOpen = "Sound_SystemFeedback_TreasureOpen";
        /// <summary>金币获得 获得金币时播放</summary>
        public static string Sound_SystemFeedback_CoinReward = "Sound_SystemFeedback_CoinReward";
        /// <summary>碎片获得 获得梦境碎片时播放</summary>
        public static string Sound_SystemFeedback_FragmentReward = "Sound_SystemFeedback_FragmentReward";
        /// <summary>奖励获得 任意形式的获得任意奖励时播放包括皮肤和情绪宝石</summary>
        public static string Sound_SystemFeedback_Reward = "Sound_SystemFeedback_Reward"; // 补充第34项
        /// <summary>交互成功 购买成功/更新成功/资源加载成功等成功交互场景播放</summary>
        public static string Sound_SystemFeedback_InteractionSuccess = "Sound_SystemFeedback_InteractionSuccess";
        /// <summary>交互错误 网络连接失败/金币不足/购买失败等失败交互场景播放</summary>
        public static string Sound_SystemFeedback_InteractionFail = "Sound_SystemFeedback_InteractionFail";
        #endregion

        #region 外循环音效
        /// <summary>物件复原 当玩家复原场景内物件时播放（不同物件有差异）</summary>
        public static string Sound_OuterLoop_Restoration = "Sound_OuterLoop_Restoration";
        /// <summary>场景完全复原 当玩家完成场景内所有内容的复原时播放</summary>
        public static string Sound_OuterLoop_RestorationComplete = "Sound_OuterLoop_RestorationComplete";
        /// <summary>植物合成 当玩家点击合成植物交互按钮时播放</summary>
        public static string Sound_OuterLoop_PlantSynthesis = "Sound_OuterLoop_PlantSynthesis";
        /// <summary>添加土壤 当玩家点击添加土壤交互按钮时播放</summary>
        public static string Sound_OuterLoop_AddSoil = "Sound_OuterLoop_AddSoil";
        /// <summary>添加水源 当玩家点击添加水源交互按钮时播放</summary>
        public static string Sound_OuterLoop_AddWater = "Sound_OuterLoop_AddWater";
        /// <summary>植物成熟 当盆栽成熟时，玩家点击收获交互按钮时播放</summary>
        public static string Sound_OuterLoop_PlantMaturity = "Sound_OuterLoop_PlantMaturity";
        /// <summary>园圃开垦 当玩家点击开垦地块交互按钮时播放</summary>
        public static string Sound_OuterLoop_GardenCultivation = "Sound_OuterLoop_GardenCultivation";
        #endregion

        #region 梦境复原音效
        /// <summary>悬浮图书馆完整复原 当玩家复原完最后一件物品后，重新完整播放复原动画时播放</summary>
        public static string Sound_OuterLoop_FloatingLibrary = "Audio_FloatingLibrary";
        /// <summary>鸟笼 当玩家鸟笼动画开始复原时播放</summary>
        public static string Sound_OuterLoop_Birdcage = "Audio_Birdcage";
        /// <summary>梯子 当玩家梯子动画开始复原时播放</summary>
        public static string Sound_OuterLoop_Scaffold = "Audio_Scaffold";
        /// <summary>顶部蜡烛 当玩家顶部蜡烛动画开始复原时播放</summary>
        public static string Sound_OuterLoop_SkyCandle = "Audio_SkyCandle";
        /// <summary>底部蜡烛 当玩家底部蜡烛动画开始复原时播放</summary>
        public static string Sound_OuterLoop_FrontCandle = "Audio_FrontCandle";
        /// <summary>底部书籍 当玩家底部书籍动画开始复原时播放</summary>
        public static string Sound_OuterLoop_GroundBooksCats_1 = "Audio_GroundBooksCats_1";
        /// <summary>中央书架 当玩家中央书架动画开始复原时播放</summary>
        public static string Sound_OuterLoop_Bookshelf_1 = "Audio_Bookshelf_1";
        /// <summary>中部书籍 当玩家中部书籍动画开始复原时播放</summary>
        public static string Sound_OuterLoop_Bookshelf_2 = "Audio_Bookshelf_2";
        /// <summary>漂浮纸张 当玩家漂浮纸张动画开始复原时播放</summary>
        public static string Sound_OuterLoop_SkyBooks = "Audio_SkyBooks";
        /// <summary>魔法猫咪 当玩家魔法猫咪动画开始复原时播放</summary>
        public static string Sound_OuterLoop_GroundBooksCats_2 = "Audio_GroundBooksCats_2";
        #endregion
    }

}



