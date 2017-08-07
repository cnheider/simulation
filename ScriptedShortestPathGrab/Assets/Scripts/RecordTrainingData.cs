using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class RecordTrainingData : MonoBehaviour {

  public Material _material;
  public bool _deleteFileContent = false;
  Gripper _gripper;
  string _file_path_pos_rot = @"C:\TrainingData\posRot.csv";
  string[][] _transform_output;

  void Start() {
    _gripper = GameObject.FindObjectOfType<Gripper>();
    GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

    if (!File.Exists(_file_path_pos_rot)) {
      print("Created file/path: " + _file_path_pos_rot);
      File.Create(_file_path_pos_rot);
    }
    if (_deleteFileContent) {
      File.WriteAllText(_file_path_pos_rot, "");
      _deleteFileContent = false;
    }
  }


  void LateUpdate() {
    RecordTransform(_gripper.transform);
    SaveToCSV(_file_path_pos_rot);
    GetData();
  }

  private void RecordTransform(Transform current_transform) {
    _transform_output = new string[][] {

      new string[]{
        current_transform.position.x.ToString("0.000000"),
        current_transform.position.y.ToString("0.000000"),
        current_transform.position.z.ToString("0.000000"),
        current_transform.eulerAngles.x.ToString("0.000000"),
        current_transform.eulerAngles.y.ToString("0.000000"),
        current_transform.eulerAngles.z.ToString("0.000000")
      }};
  }

  //Writes to file in the format: pos x, pos y, pos z, rot x, rot y, rot z
  void SaveToCSV(string filePath) {
    string delimiter = ", ";

    StringBuilder sb = new StringBuilder();

    for (int index = 0; index < _transform_output.GetLength(0); index++)
      sb.AppendLine(string.Join(delimiter, _transform_output[index]));

    File.AppendAllText(filePath, sb.ToString());
  }


  public byte[] GetData() {
    var _data = RenderTextureImage(this.GetComponent<Camera>()).EncodeToPNG();
    return _data;
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    Graphics.Blit(source, destination, _material);
  }

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
}


