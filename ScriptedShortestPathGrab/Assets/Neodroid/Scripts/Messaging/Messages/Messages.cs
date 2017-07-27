/*using MsgPack;
using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Neodroid.Messaging.Messages {
  public class EnvironmentState {
    public float[] _actor_position { get; set; }
    public float[] _actor_rotation { get; set; }

    public byte[] _depth_image { get; set; }
    public byte[] _light_mask_image { get; set; }

    public float _reward{ get; set; }

    public EnvironmentState(Vector3 actor_position, Quaternion actor_rotation, Texture2D depth_image, Texture2D light_mask_image, float reward) {
      _actor_position = new float[] { actor_position.x, actor_position.y, actor_position.z };
      _actor_rotation = new float[] { actor_rotation.x, actor_rotation.y, actor_rotation.z, actor_rotation.w };

      _depth_image = depth_image.EncodeToPNG();
      _light_mask_image = light_mask_image.EncodeToPNG();
      _reward = reward;
    }
  }

  public enum Command { Action, Reset, Unknown }

  public class AgentAction {
    public string _command {  get; set; }
    public float[] _new_actor_position {  get; set; }
    public float[] _new_actor_rotation {  get; set; }

    public Command GetCommand() {
      switch (_command) {
        case "action":
          return Command.Action;
        case "reset":
          return Command.Reset;
        default:
          return Command.Unknown;
      }
    }

    public Vector3 GetNewPosition() {
      return new Vector3(_new_actor_position[0], _new_actor_position[1], _new_actor_position[2]);
    }

    public Quaternion GetNewRotation() {
      return new Quaternion(_new_actor_rotation[0], _new_actor_rotation[1], _new_actor_rotation[2], _new_actor_rotation[3]);
    }

    public override string ToString() {
      return "AgentAction( " + _command + ", " + _new_actor_position + ", " + _new_actor_rotation + ")";
    }

    public AgentAction(string command, float[] new_actor_position, float[] new_actor_rotation) {
      switch (command) {
        case "action":
          _command = Command.Action;
            break;
        case "reset":
          _command = Command.Reset;
          break;
        default:
          _command = Command.Unknown;
           break;
      }
      //_new_actor_position = new Vector3(new_actor_position[0], new_actor_position[1], new_actor_position[2]);
      //_new_actor_rotation = new Quaternion(new_actor_rotation[0], new_actor_rotation[1], new_actor_rotation[2], new_actor_rotation[3]);
    }

    public AgentAction() {
      _command = Command.Action;
      //_new_actor_position = new Vector3(0, 0, 0);
      //_new_actor_rotation = new Quaternion(0, 0, 0, 0);
    }


    public static AgentAction MsgObj2Action(MsgPack.MessagePackObject obj) {
      AgentAction output;
      using (MemoryStream stream = new MemoryStream()) {
        using (Packer packer = Packer.Create(stream)) {
          obj.PackToMessage(packer, new PackingOptions());
          stream.Position = 0;
          output = SerializationContext.CreateClassicContext().GetSerializer<AgentAction>().Unpack(stream);
        }
      }
      return output;
    }
  }
}
*/
