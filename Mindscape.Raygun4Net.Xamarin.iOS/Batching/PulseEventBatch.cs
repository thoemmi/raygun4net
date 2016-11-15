﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Mindscape.Raygun4Net
{
  internal class PulseEventBatch
  {
    private List<PendingEvent> _pendingEvents = new List<PendingEvent> ();
    private DateTime _lastUpdate;
    private readonly RaygunClient _raygunClient;

    private readonly int _lifeSpan = 1;

    private bool _locked;

    public PulseEventBatch (RaygunClient raygunClient, int lifeSpan)
    {
      _raygunClient = raygunClient;
      _lastUpdate = DateTime.UtcNow;

      _lifeSpan = lifeSpan;

      Thread t = new Thread (CheckTime);
      t.Start ();
    }

    private void CheckTime ()
    {
      while (true) {
        Thread.Sleep (1500);

        if ((DateTime.UtcNow - _lastUpdate).TotalSeconds > _lifeSpan && _pendingEvents.Count > 0) {
          Done ();
          break;
        }
      }
    }

    public void Add (PendingEvent pendingEvent)
    {
      _lastUpdate = DateTime.UtcNow;
      _pendingEvents.Add (pendingEvent);
      if (_pendingEvents.Count >= 50) {
        Done ();
      }
    }

    public bool IsLocked {
      get { return _locked; }
    }

    public void Done ()
    {
      if (!_locked) {
        _locked = true;
        _raygunClient.Send (this);
      }
    }

    public void DoneNow ()
    {
      if (!_locked) {
        _locked = true;
        _raygunClient.SendNow (this);
      }
    }

    public int PendingEventCount {
      get { return _pendingEvents.Count; }
    }

    public IEnumerable<PendingEvent> PendingEvents {
      get {
        foreach (PendingEvent pendingEvent in _pendingEvents) {
          yield return pendingEvent;
        }
      }
    }
  }

  internal class PendingEvent
  {
    private readonly RaygunPulseEventType _eventType;
    private readonly string _name;
    private readonly long _milliseconds;
    private readonly DateTime _timestamp;
    private readonly string _sessionId;

    public PendingEvent (RaygunPulseEventType eventType, string name, long milliseconds, string sessionId)
    {
      _eventType = eventType;
      _name = name;
      _milliseconds = milliseconds;
      _timestamp = DateTime.UtcNow - TimeSpan.FromMilliseconds (milliseconds);
      _sessionId = sessionId;
    }

    public RaygunPulseEventType EventType {
      get { return _eventType; }
    }

    public string Name {
      get { return _name; }
    }

    public long Duration {
      get { return _milliseconds; }
    }

    public DateTime Timestamp {
      get { return _timestamp; }
    }

    public string SessionId {
      get { return _sessionId; }
    }
  }
}
