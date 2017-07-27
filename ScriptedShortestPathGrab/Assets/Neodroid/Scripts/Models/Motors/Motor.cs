using Neodroid.Messaging.Messages;
using UnityEngine;
using Neodroid.Utilities;

namespace Neodroid.Models.Motors {
  public abstract class Motor : MonoBehaviour {
    public bool _debug = false;
    public bool _bidirectional = true;
    public float _energy_cost = 1;
    protected float _energy_spend_since_reset = 0;
    public Actor _actor_game_object;
    public string _motor_identifier = "";

    private void Start() {
      _motor_identifier = GetMotorIdentifier();
      NeodroidFunctions.MaybeRegisterComponent(_actor_game_object, this);
    }

    private void Update() {
    }

    public abstract string GetMotorIdentifier();

    public virtual void ApplyMotion(MotorMotion motion) { }

    public virtual float GetEnergySpend() {
      return _energy_spend_since_reset;
    }

    public override string ToString() {
      return name + _motor_identifier;
    }
  }
}
