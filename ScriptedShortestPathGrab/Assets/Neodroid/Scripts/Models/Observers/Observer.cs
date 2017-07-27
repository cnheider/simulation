using MsgPack.Serialization;
using UnityEngine;
using Neodroid.Utilities;

namespace Neodroid.Models.Observers {

  public abstract class Observer : MonoBehaviour {

    [MessagePackIgnore]
    public NeodroidAgent _agent_game_object; //Is not send

    public byte[] _data;

    protected void AddToAgent() {
      NeodroidFunctions.MaybeRegisterComponent(_agent_game_object, this);
    }


    public abstract byte[] GetData();

    //public string _name;
    //public float[] _position;
    //public float[] _rotation;
  }
}
