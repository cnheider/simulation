using UnityEngine;

namespace Neodroid.Utilities {
  public static class NeodroidFunctions {
    public static Texture2D RenderTextureImage(Camera camera) { // From unity documentation, https://docs.unity3d.com/ScriptReference/Camera.Render.html
      RenderTexture current_render_texture = RenderTexture.active;
      RenderTexture.active = camera.targetTexture;
      camera.Render();
      Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
      image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
      image.Apply();
      RenderTexture.active = current_render_texture;
      return image;
    }

    public static float Objective_Function(Vector3 pos1, Vector3 pos2) {
      return Vector3.Distance(pos1, pos2);
    }

    public static void MaybeRegisterComponent<Recipient, Caller>(Recipient r, Caller c) where Recipient : Object, HasRegister<Caller> where Caller : Component {
      Recipient component;
      if (r != null) {
        component = r;  //.GetComponent<Recipient>();
      } else if (c.GetComponentInParent<Recipient>() != null) {
        component = c.GetComponentInParent<Recipient>();
      } else {
        component = Object.FindObjectOfType<Recipient>();
      }
      if (component != null)
        component.Register(c);
    }
  }
}
