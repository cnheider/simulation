using Neodroid.Messaging.Messages;
using UnityEngine;

namespace Neodroid.Models.Motors {
  public enum MotorAxis { X, Y, Z, rot_X, rot_Y, rot_Z }

  public class SingleAxisMotor : Motor {
    public MotorAxis _axis_of_motion;



    public override void ApplyMotion(MotorMotion motion) {
      if (_debug) Debug.Log("Applying " + motion._strength.ToString() + " To " + name);
      if (!_bidirectional && motion._strength < 0) {
        Debug.Log("Motor is not bi-directional. It does not accept negative input.");
        return; // Do nothing
      }
      switch (_axis_of_motion) {
        case MotorAxis.X:
          transform.Translate(Vector3.left * motion._strength, Space.Self);
          break;
        case MotorAxis.Y:
          transform.Translate(Vector3.up * motion._strength, Space.Self);
          break;
        case MotorAxis.Z:
          transform.Translate(Vector3.forward * motion._strength, Space.Self);
          break;
        case MotorAxis.rot_X:
          transform.Rotate(Vector3.left, motion._strength, Space.Self);
          //GetComponent<Rigidbody>().AddForceAtPosition(Vector3.forward * motion._strength, transform.position);
          //GetComponent<Rigidbody>().AddRelativeTorque(Vector3.up * motion._strength);
          break;
        case MotorAxis.rot_Y:
          transform.Rotate(Vector3.up, motion._strength, Space.Self);
          break;
        case MotorAxis.rot_Z:
          transform.Rotate(Vector3.forward, motion._strength, Space.Self);
          break;
        default:
          break;
      }
      _energy_spend_since_reset += _energy_cost * motion._strength;
    }

    public override string GetMotorIdentifier() {
      return _axis_of_motion.ToString();
    }
  }
}
