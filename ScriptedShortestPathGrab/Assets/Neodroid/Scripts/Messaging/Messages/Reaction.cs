using System.Collections.Generic;

namespace Neodroid.Messaging.Messages {
  public class Reaction {
    public List<MotorMotion> _actor_motor_motions;
    public bool _reset;

    public Reaction() {

    }

    public List<MotorMotion> GetMotions() {
      return _actor_motor_motions;
    }

    public override string ToString() {
      return "<Reaction> " + _reset + " </Reaction>";
    }
  }
}
