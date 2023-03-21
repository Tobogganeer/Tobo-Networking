using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public event Action<State> Completed = delegate { };
    public event Action<float> Tick = delegate { };
    public readonly int ID;
    public float Time { get; private set; }
    public bool Running { get; private set; }
    public bool DestroyOnCompletion { get; set; } = true;
    private bool destroyed = false;

    #region Constructors
    /// <summary>
    /// Alternate to 'new Timer(time, callback);' 
    /// </summary>
    public static Timer Create(float time, Action<State> callback, int id = -1) => new Timer(time, callback, id);
    /// <summary>
    /// Alternate to 'new Timer(time, callback);' (Only calls on completion) 
    /// </summary>
    public static Timer Create(float time, Action callback, int id = -1) => new Timer(time, callback, id);
    /// <summary>
    /// Alternate to 'new Timer(time, callback);' 
    /// </summary>
    public static Timer New(float time, Action<State> callback, int id = -1) => new Timer(time, callback, id);
    /// <summary>
    /// Alternate to 'new Timer(time, callback);' (Only calls on completion)
    /// </summary>
    public static Timer New(float time, Action callback, int id = -1) => new Timer(time, callback, id);

    public Timer(float time, Action<State> callback, int id = -1)
    {
        Time = time;
        Completed += callback;

        Init();
    }

    public Timer(float time, Action callback, int id = -1)
    {
        Time = time;
        Completed += (state) => { if (state == State.Finished) callback(); };

        Init();
    }

    #endregion

    private void Init()
    {
        TimerManager.Add(this);
        Running = true;
    }

    private void Update(float dt)
    {
        if (!Running || destroyed) return;

        Time -= dt;
        Tick(Time);

        if (Time <= 0)
        {
            Running = false;
            Completed(State.Finished);
            if (DestroyOnCompletion)
                Destroy();
        }
    }


    public void Start()
    {
        if (destroyed) return;

        if (!Running)
        {
            Running = true;
        }
    }

    public void Restart(float time)
    {
        if (destroyed) return;

        if (!Running)
        {
            Time = time;
            Running = true;
        }
    }

    public void Stop()
    {
        if (destroyed) return;

        if (Running)
        {
            Running = false;
            Completed(State.Stopped);
        }
    }

    public void Destroy()
    {
        if (destroyed) return;

        Stop();
        TimerManager.Remove(this);
        destroyed = true;
    }


    public Timer OnTick(Action<float> tick)
    {
        Tick += tick;
        return this;
    }

    public Timer DestroyOnFinish(bool destroy)
    {
        DestroyOnCompletion = destroy;
        return this;
    }

    public static void DestroyAll(int id) => ClearID(id);

    public static void ClearID(int id)
    {
        TimerManager.ClearID(id);
    }

    public enum State
    {
        None,
        Finished,
        Stopped
    }

    #region Manager

    public class TimerManager : MonoBehaviour
    {
        private static TimerManager instance;
        private static List<Timer> timers = new List<Timer>();

        public static void Add(Timer timer)
        {
            Create();

            timers.Add(timer);
        }

        public static void Remove(Timer timer)
        {
            Create();

            timers.Remove(timer);
        }

        public static void ClearID(int id)
        {
            Create();

            if (id == -1)
            {
                timers.Clear();
                return;
            }

            for (int i = timers.Count; i >= 0; i--)
            {
                if (timers[i].ID == id)
                    timers.RemoveAt(i);
            }
        }

        private static void Create()
        {
            if (instance == null)
                instance = new GameObject("TimerManager").AddComponent<TimerManager>();
        }

        private void Update()
        {
            float dt = UnityEngine.Time.deltaTime;

            for (int i = timers.Count; i > 0;)
            {
                i--;
                timers[i].Update(dt);
            }
        }
    }

    #endregion
}
