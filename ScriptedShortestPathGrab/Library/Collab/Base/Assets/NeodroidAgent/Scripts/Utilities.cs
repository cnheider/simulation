using Assets.NeodroidAgent.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Utilities {
  public static class Utilities {

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

    public static void IfNullFindComponentAndRegister<C1, C2>(C1 c1, C2 c2) where C1 : Object, HasRegister where C2: Component {
      C1 component;
      if (c1 != null) {
        component = c2.GetComponent<C1>();
      } else if (c2.GetComponentInParent<C1>() != null) {
        component = c2.GetComponentInParent<C1>();
      } else {
        component = Object.FindObjectOfType<C1>();
      }
      if (component != null)
        component.Register(c2);
    }
  }
}
