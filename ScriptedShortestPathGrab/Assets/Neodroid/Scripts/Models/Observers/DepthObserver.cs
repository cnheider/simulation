using MsgPack.Serialization;
using UnityEngine;
using Neodroid.Utilities;

namespace Neodroid.Models.Observers {

  //[ExecuteInEditMode]
  public class DepthObserver : Observer {

    [MessagePackIgnore]
    public Material _material;

    void Start() {
      AddToAgent();
      GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    void LateUpdate() {
    }

    public override byte[] GetData() {
      _data = NeodroidFunctions.RenderTextureImage(this.GetComponent<Camera>()).EncodeToPNG();
      return _data;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
      Graphics.Blit(source, destination, _material);
    }
  }
}
