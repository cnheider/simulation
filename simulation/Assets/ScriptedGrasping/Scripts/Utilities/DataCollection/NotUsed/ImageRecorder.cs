using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ImageRecorder : MonoBehaviour {

  public Camera _camera;
  string _file_path = @"training_data/shadow/";

  int _i = 0;

  void Start(){
    if(!_camera)
      _camera = GetComponent<Camera> ();
  }

  void Update () {
    SaveRenderTextureToImage (_i, _camera, _file_path);

    _i++;
    }

  public void SaveRenderTextureToImage(int id, Camera camera, string file_name_dd) {
    var texture2d = RenderTextureImage (camera);
    var data = texture2d.EncodeToPNG ();
    string file_name = file_name_dd + id.ToString() + ".png";
    File.WriteAllBytes (file_name, data);
  }

  public static Texture2D RenderTextureImage (Camera camera) { // From unity documentation, https://docs.unity3d.com/ScriptReference/Camera.Render.html
    RenderTexture current_render_texture = RenderTexture.active;
    RenderTexture.active = camera.targetTexture;
    camera.Render ();
    Texture2D image = new Texture2D (camera.targetTexture.width, camera.targetTexture.height);
    image.ReadPixels (new Rect (0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
    image.Apply ();
    RenderTexture.active = current_render_texture;
    return image;
  }
}
