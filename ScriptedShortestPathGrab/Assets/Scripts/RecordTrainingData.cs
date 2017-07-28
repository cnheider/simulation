using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class RecordTrainingData : MonoBehaviour {

  //public Material _material;
  public bool deleteFileContent = false;
  Transform gripper;
  string filePath_posRot = @"C:\TrainingData\posRot.csv";
  string[][] transform_output;

  void Start () {
    gripper = GameObject.Find("Gripper").transform;
    GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

    if (!File.Exists(filePath_posRot)) {
      print("Created file/path: " + filePath_posRot);
      File.Create(filePath_posRot);
    }
    if (deleteFileContent) {
      File.WriteAllText(filePath_posRot, "");
      deleteFileContent = false;
    }
  }


  void LateUpdate () {
    

    RecordTransform(transform);
    SaveCSV(filePath_posRot);
    //GetData();
  }

  private void RecordTransform(Transform current_transform) {
    transform_output = new string[][] {
      
      new string[]{
        gripper.position.x.ToString("0.000000"),
        gripper.position.y.ToString("0.000000"),
        gripper.position.z.ToString("0.000000"),
        gripper.eulerAngles.x.ToString("0.000000"),
        gripper.eulerAngles.y.ToString("0.000000"),
        gripper.eulerAngles.z.ToString("0.000000")
      }};
  }

  //Writes to file in the format: pos x, pos y, pos z, rot x, rot y, rot z
  void SaveCSV(string filePath) {
    string delimiter = ", ";

    StringBuilder sb = new StringBuilder();
    
    for (int index = 0; index < transform_output.GetLength(0); index++)
      sb.AppendLine(string.Join(delimiter, transform_output[index]));

    File.AppendAllText(filePath, sb.ToString());

  }


  public byte[] GetData() {
    var _data = RenderTextureImage(this.GetComponent<Camera>()).EncodeToPNG();
    return _data;
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    //Graphics.Blit(source, destination, _material);
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


