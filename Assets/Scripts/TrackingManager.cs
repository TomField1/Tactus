using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System;
using System.IO;
using System.Text;
using System.Net;
using UnityEngine;

public class TrackingManager : MonoBehaviour {

    //tags for types of event and possible sources of damage
    public enum EventTag {None, Start, Tap, TryChange, Damaged, Died, TimeEnd, Powerup, CaughtEnemy, End, Scores };
    public enum DamageSource { Enemy, Spikes}
    string filename; //the filename of the current file
    string sessionID;

    //the list of events
    public List<Event> _eventList = new List<Event>();
    
    void Start()
    {
        sessionID = RequestSessionID();
    }

    string RequestSessionID()
    {
        System.Net.WebClient Client = new System.Net.WebClient();
        string s;

        try { 
            s = "ID" + Client.DownloadString("http://leswordfish.x10host.com/getID.php");
        }catch(WebException)
        {
            s = "NOID";
        }

        print("SESSIONID == "+s);

        return s;
    }

    //clear the event list
    public void ClearList()
    {
        print("clearing list");
        _eventList.Clear();
    }

    //print the event list
    public void PrintList(string s)
    {
        print("========================= PRINTING EVENT LIST @ "+s);
        foreach(Event e in _eventList)
        {
            print("event " + e._tag.ToString() + " at " + e._time.ToString());
        }
    }

    /// <summary>
    /// Write the event list
    /// </summary>
    /// <param name="controls"></param>
    public void XMLWriteEventList(GameManager.ControlScheme controls)
    {

        //create the filename at the current time
        filename = sessionID + controls.ToString()
                                 + "_" + DateTime.Now.Hour.ToString()
                                 + "_" + DateTime.Now.Minute.ToString()
                                 + "_" + DateTime.Now.Second.ToString()
                                 + ".xml";


        //if on android, put it at the Application data
#if UNITY_ANDROID
        filename = Application.persistentDataPath + "/" + filename;
#endif

        //using the XMLWriter on a new file
        using (XmlWriter writer = XmlWriter.Create(filename))
        {
            //start the document
            writer.WriteStartDocument();
            writer.WriteStartElement("Events");

            //for each event, write the event
            foreach (Event e in _eventList)
            {
                e.WriteEvent(writer);

            }

            //finish the document
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        //check the file exists
#if UNITY_ANDROID
           FileInfo f = new FileInfo(filename);
        if (f.Exists)
        {
            print("file exists at " + filename);
        }
#endif
    }

    //add an event to the list
    public void AddEvent(Event e)
    {
        _eventList.Add(e);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////// UPLOADING //////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Upload the file, return an error string. No, i'm not sure how it works
    /// </summary>
    /// <returns></returns>
    public bool Upload()
    {

        System.Net.WebClient Client = new System.Net.WebClient();

        Client.Headers.Add("Content-Type", "binary/octet-stream");

        string s;

        //try and upload
        try {

            byte[] result = Client.UploadFile("http://leswordfish.x10host.com/upload3.php", "POST", filename);
        
            s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);

            //if the sessionID failed before but the upload works, take the opportunity to get a sessionID that will work for the next upload
            if (sessionID == "NOID")
            {
                sessionID = RequestSessionID();
            }
            return true;
        }
        //if that fails, return that we failed
        catch (WebException)
        {
            print("no connection");
            return false;
        }
    }
}

/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// ///////// EVENTS ////////////////////////////////////////////////////////////////////////////////////////////////////
/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

///This is the basic EVENT that every other event inherits from
///I'm going to go over the basic stuff here and only mention what's different for the others
public class Event
{
    public TrackingManager.EventTag _tag; //the tag for the kind of event
    public float _time; //the time at which it happened as a float
    public Vector3 _currentLocation; //the player location at the time

    //default event constructors
    public Event()
    {
        this._tag = TrackingManager.EventTag.None;
        this._time = 0;
        this._currentLocation = new Vector3(0, 0, 0);
    }
    public Event(TrackingManager.EventTag tag, float time, Vector3 loc)
    {
        this._tag = tag;
        this._time = time;
        this._currentLocation = loc;
    }

    public virtual void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing default event");
    }

    //the default part of writing an event. Opens the element, writes the tag, time, and location. called by all the others
    public void WriteEventDefault(XmlWriter w)
    {
        w.WriteStartElement(_tag.ToString());

        w.WriteElementString("time", _time.ToString());
        w.WriteElementString("currentLocation", _currentLocation.ToString());
    }
}
    //EVENTSTART and EVENTEND ARE DIFFERENT!
    //they start and finish the Element for the whole list of events.
public class EventStart : Event
{

    public GameManager.ControlScheme currentControls; //this Element is named for the whole control scheme

    public EventStart(GameManager.ControlScheme scheme)
    {
        this.currentControls = scheme;
        this._tag = TrackingManager.EventTag.Start;
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing start event");
        w.WriteStartElement(currentControls.ToString());
    }
}
public class EventEnd : Event
{

    public GameManager.ControlScheme currentControls;

    public EventEnd()
    {
        this._tag = TrackingManager.EventTag.End;
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing end event");
        w.WriteEndElement();
        
    }
}

public class EventResponseScore : Event
{

    public int _control;
    public int _challenge;
    public int _fun;

    public EventResponseScore(int control, int challenge, int fun)
    {
        this._control = control;
        this._challenge = challenge;
        this._fun = fun;
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing score event");
        w.WriteStartElement("ResponseScore");
        w.WriteElementString("Control", _control.ToString());
        w.WriteElementString("Challenge", _control.ToString());
        w.WriteElementString("Fun", _control.ToString());
        w.WriteEndElement();

    }
}

/// <summary>
/// event for the player tapping the screen
/// </summary>
public class EventTap : Event
{

    public Vector3 _tapLocation; //the location the player tapped

    public EventTap(float time, Vector3 loc, Vector3 tapLoc)
    {
        this._tag = TrackingManager.EventTag.Tap;
        this._time = time;
        this._currentLocation = loc;
        this._tapLocation = tapLoc;
        MonoBehaviour.print("saving tap event");
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing tap event");
        this.WriteEventDefault(w);
        w.WriteElementString("tapLocation", _tapLocation.ToString());

        w.WriteEndElement(); //note how each event does the default, its own special data, then ends the element
    }
}

/// <summary>
/// Trying to change direction. Called by both swipe and tilt
/// </summary>
public class EventTryChangeDir : Event
{
    public Vector3 _currentDirection; //the current direction
    public Vector3 _newDirection; //the direction trying to change do
    public bool _succeedChanging; //was it successful

    public EventTryChangeDir(float time, Vector3 loc, Vector3 currentDir, Vector3 newDir, bool succeeded)
    {
        this._tag = TrackingManager.EventTag.TryChange;
        this._time = time;
        this._currentLocation = loc;
        this._currentDirection = currentDir;
        this._newDirection = newDir;
        this._succeedChanging = succeeded;
        MonoBehaviour.print("saving change direction event");
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing try change direction event");
        this.WriteEventDefault(w);
        w.WriteElementString("currentDirection", _currentDirection.ToString());
        w.WriteElementString("newDirection", _newDirection.ToString());
        w.WriteElementString("successfulChange", _succeedChanging.ToString());

        w.WriteEndElement();
    }
}

/// <summary>
/// event for taking damage
/// </summary>
public class EventDamaged : Event
{
    public int _newHealth; //what's the player's health now
    public TrackingManager.DamageSource _damageSource; //what caused the damage?

    public EventDamaged(float time, Vector3 loc, int newhealth, TrackingManager.DamageSource dam)
    {
        this._tag = TrackingManager.EventTag.Damaged;
        this._time = time;
        this._currentLocation = loc;
        this._newHealth = newhealth;
        this._damageSource = dam;
        MonoBehaviour.print("saving damaged event");
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing damage event");
        this.WriteEventDefault(w);
        w.WriteElementString("newHealth", _newHealth.ToString());
        w.WriteElementString("damageSource", _damageSource.ToString());
        w.WriteEndElement();
    }
}

/// <summary>
/// event for the game ending by time running out
/// </summary>
public class EventTimeEnd : Event
{
    public int _score; //what was the final score
    public int _healthLeft; //how much health remains

    public EventTimeEnd(float time, Vector3 loc, int score, int health)
    {
        this._tag = TrackingManager.EventTag.TimeEnd;
        this._time = time;
        this._currentLocation = loc;
        this._healthLeft = health;
        this._score = score;
        MonoBehaviour.print("saving timeout event");
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing end event");
        this.WriteEventDefault(w);
        w.WriteElementString("endScore", _score.ToString());
        w.WriteElementString("endHealth", _healthLeft.ToString());
        w.WriteEndElement();
    }
}

/// <summary>
/// event for the game ending with your death
/// </summary>
public class EventDied : Event
{
    public int _score; //what was the final score

    public EventDied(float time, Vector3 loc, int score)
    {
        this._tag = TrackingManager.EventTag.Died;
        this._time = time;
        this._currentLocation = loc;
        this._score = score;
        MonoBehaviour.print("saving died event");
    }

    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing died event");
        this.WriteEventDefault(w);
        w.WriteElementString("endScore", _score.ToString());
        w.WriteEndElement();
    }
}

/// <summary>
/// Event for grabbing a powerup
/// </summary>
public class EventPowerup: Event
{
    public EventPowerup(float time, Vector3 loc)
    {
        this._tag = TrackingManager.EventTag.Powerup;
        this._time = time;
        this._currentLocation = loc;
        MonoBehaviour.print("saving powerup event");
    }
    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing powerup event");
        this.WriteEventDefault(w);
        w.WriteEndElement();
    }
}

/// <summary>
/// Event for catching and killing an enemy. With your bare hands.
/// </summary>
public class EventCaughtEnemy : Event
{
    public EventCaughtEnemy(float time, Vector3 loc)
    {
        this._tag = TrackingManager.EventTag.CaughtEnemy;
        this._time = time;
        this._currentLocation = loc;
        MonoBehaviour.print("saving killed enemy event");
    }
    public override void WriteEvent(XmlWriter w)
    {
        MonoBehaviour.print("writing killed enemy event");
        this.WriteEventDefault(w);
        w.WriteEndElement();
    }
}

