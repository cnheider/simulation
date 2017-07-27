using Neodroid.Messaging;
using Neodroid.Messaging.Messages;
using Neodroid.Models.Observers;
using Neodroid.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neodroid.Models {
  public class NeodroidAgent : MonoBehaviour, HasRegister<Actor>, HasRegister<Observer> {
    private Dictionary<string, Actor> _actors = new Dictionary<string, Actor>();
    private Dictionary<string, Observer> _observers = new Dictionary<string, Observer>();

    // Public unity parameters
    public string _ip_address;
    public int _port;
    public bool _continue_lastest_reaction_on_disconnect = false;

    public bool _debug = false;

    // Private
    MessageServer _message_server;
    bool _waiting_for_action = true;
    bool _actor_updated = false;
    bool _reload_scene = false;
    Vector3 new_position;
    Quaternion new_rotation;

    private Reaction _lastest_reaction = null;

    private void Start() {

      string[] arguments = Environment.GetCommandLineArgs();
      if (arguments[1] != null && arguments[1] != "")
        _ip_address = arguments[1];
      if (arguments[1] != null && arguments[1] != "")
        _port = int.Parse(arguments[2]);

      if (_ip_address != "" || _port != 0)
        _message_server = new MessageServer(_ip_address, _port);
      else
        _message_server = new MessageServer();
      _message_server.ListenForClientToConnect(OnConnectCallback);
    }

    public Dictionary<string, Actor> GetActors() {
      return _actors;
    }

    public void AddActor(Actor actor) {
      if (_debug) Debug.Log("Agent " + name + " has actor " + actor.name);
      _actors.Add(actor.name, actor);
    }

    public Dictionary<string, Observer> GetObservers() {
      return _observers;
    }

    public void AddObserver(Observer observer) {
      if (_debug) Debug.Log("Agent " + name + " has observer " + observer.name);
      _observers.Add(observer.name, observer);
    }

    void Update() { // Update is called once per frame, updates like actor position needs to be done on the main thread

      if (_reload_scene) {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        _reload_scene = false;
      }

      if (_lastest_reaction != null && (!_waiting_for_action || !_message_server._client_connected)) {
        ExecuteReaction(_lastest_reaction);
        if (!_continue_lastest_reaction_on_disconnect)
          _lastest_reaction = null;
      }

      if (_message_server._client_connected && !_waiting_for_action) {
        foreach (Observer obs in GetObservers().Values) {
          obs.GetComponent<Observer>().GetData();
        }
        _message_server.SendEnvironmentState(GetCurrentState());
        ResumeGame();
        _waiting_for_action = true;
      }

      if (!_message_server._client_connected) {
        ResumeGame();
      }

    }

    void FixedUpdate() {
      if (_message_server._client_connected) {
        PauseGame();
      } else {
        ResumeGame();
      }
    }

    private void OnDestroy() { //Deconstructor
      _message_server.Destroy();
    }

    EnvironmentState GetCurrentState() {
      return new EnvironmentState(5, 225, _actors, _observers, NeodroidFunctions.Objective_Function(_actors["Car"].transform.position, Vector3.zero));
    }

    void PauseGame() {
      Time.timeScale = 0;
    }

    void ResumeGame() {
      Time.timeScale = 1;
    }

    bool IsGamePaused() {
      return Time.timeScale == 0;
    }

    void ExecuteReaction(Reaction reaction) {
      var actors = GetActors();
      foreach (MotorMotion motion in reaction.GetMotions()) {
        var motion_actor_name = motion.GetActorName();
        var motion_motor_name = motion.GetMotorName();
        if (actors.ContainsKey(motion_actor_name)){
          var motors = actors[motion_actor_name].GetMotors();
          if (motors.ContainsKey(motion_motor_name)) {
            motors[motion_motor_name].ApplyMotion(motion);
          } else {
            Debug.Log("Could find not motor with the specified name: " + motion_motor_name);
          }
        } else {
          Debug.Log("Could find not actor with the specified name: " + motion_actor_name);
        }
      }
    }

    //Callbacks
    void OnReceiveCallback(Reaction reaction) {
      if (_debug) Debug.Log("Received: " + reaction.ToString());
      _lastest_reaction = reaction;
      _waiting_for_action = false;
    }

    void OnDisconnectCallback() {
      if (_debug) Debug.Log("Client disconnected.");
      _message_server.ListenForClientToConnect(OnConnectCallback);
    }

    void OnErrorCallback(string error) {
      if (_debug) Debug.Log("ErrorCallback: " + error);
    }

    void OnConnectCallback() {
      if (_debug) Debug.Log("Client connected.");
      _message_server.StartReceiving(OnReceiveCallback, OnDisconnectCallback, OnErrorCallback);
    }

    public void Register(Actor obj) {
      AddActor(obj);
    }

    public void Register(Observer obj) {
      AddObserver(obj);
    }
  }
}
