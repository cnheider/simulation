
using Assets.NeodroidAgent.Scripts.Models;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Utilities;

namespace Assets.Agent {
  public class Actor : MonoBehaviour, HasRegister<Motor> {
    public float[] _position;
    public float[] _rotation;

    [MessagePackKnownCollectionItemType("SingleAxisMotor", typeof(SingleAxisMotor))]
    private Dictionary<string, Motor> _motors = new Dictionary<string, Motor>();

    [MessagePackIgnore]
    private int motor_counter=0;


    public Actor() { }

    [MessagePackIgnore]
    public NeodroidAgent _agent_game_object;
    [MessagePackIgnore]
    public bool _debug =false;

    private void Start() {
      Utilities.RegisterComponent(_agent_game_object, this);
    }

    private void Update() {
      _position = new float[]{transform.position.x, transform.position.y, transform.position.z};
      _rotation = new float[] {transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w };
    }

    public Dictionary<string, Motor> GetMotors() {
      return _motors;
    }

    public void AddMotor(Motor motor) {
      if (_debug) Debug.Log("Actor " + name + " has " + motor);
      _motors.Add(motor.name, motor);
    }

    public void Register(Motor obj) {
      AddMotor(obj);
    }
  }
}
