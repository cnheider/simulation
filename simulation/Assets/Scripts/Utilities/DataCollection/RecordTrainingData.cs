using UnityEngine;
using System.IO;
using System.Text;
using Assets.Scripts.Grasping;

public class RecordTrainingData : MonoBehaviour {

  public Material _material;
  public bool _deleteFileContent = false;
  public Gripper _gripper;
  public Grasp _target;
  public Camera _camera;
  string _file_path = @"training_data/";
  string _file_path_gripper = @"gripper_position_rotation.csv";
  string _file_path_target = @"target_position_rotation.csv";
  string _file_path_images = @"depth/";

  int _i = 0;

  void Start () {
    if (!_gripper) {
      _gripper = FindObjectOfType<Gripper> ();
    }
    if (!_target) {
      _target = FindObjectOfType<Grasp> ();
    }
    if (!_camera) {
      _camera = GetComponent<Camera> ();
    }
    if (!_camera) {
      _camera = FindObjectOfType<Camera> ();
    }

    _camera.depthTextureMode = DepthTextureMode.Depth;

    File.WriteAllText (_file_path + _file_path_gripper, "");
    File.WriteAllText (_file_path + _file_path_target, "");

    /*if (!File.Exists(_file_path + _file_path_pos_rot)) {
      print("Created file/path: " + _file_path + _file_path_pos_rot);
      //File.Create(_file_path + _file_path_pos_rot);
    }
    if (_deleteFileContent) {
      File.WriteAllText(_file_path + _file_path_pos_rot, "");
      _deleteFileContent = false;
    }*/
  }

  void Update () {
    Vector3 gripper_position_relative_to_camera = this.transform.InverseTransformPoint (_gripper.transform.position);
    Vector3 gripper_direction_relative_to_camera = this.transform.InverseTransformDirection (_gripper.transform.eulerAngles);
    var gripper_transform_output = GetTransformOutput (gripper_position_relative_to_camera, gripper_direction_relative_to_camera);
    SaveToCSV (_file_path + _file_path_gripper, gripper_transform_output);

    Vector3 target_position_relative_to_camera = this.transform.InverseTransformPoint (_target.transform.position);
    Vector3 target_direction_relative_to_camera = this.transform.InverseTransformDirection (_target.transform.eulerAngles);
    var target_transform_output = GetTransformOutput (target_position_relative_to_camera, target_direction_relative_to_camera);
    SaveToCSV (_file_path + _file_path_target, target_transform_output);


    SaveDepthImage ();
  }

  string[] GetTransformOutput(Vector3 pos, Vector3 dir){
    return new string[] {
        pos.x.ToString ("0.000000"),
        pos.y.ToString ("0.000000"),
        pos.z.ToString ("0.000000"),
        dir.x.ToString ("0.000000"),
        dir.y.ToString ("0.000000"),
        dir.z.ToString ("0.000000")
    };
  }

  void SaveBytesToFile (byte[] bytes, string filename) {
    File.WriteAllBytes (filename, bytes);
  }

  //Writes to file in the format: pos x, pos y, pos z, rot x, rot y, rot z
  void SaveToCSV (string filePath, string[] output) {
    string delimiter = ", ";

    StringBuilder sb = new StringBuilder ();

    sb.AppendLine (string.Join(delimiter, output));

    File.AppendAllText (filePath, sb.ToString ());

    //using (StreamWriter sw = File.AppendText(filePath)) {
    //  sw.WriteLine(sb.ToString());
    //}

  }


  public void SaveDepthImage() {
    var texture2d = RenderTextureImage (_camera);
    var data = texture2d.EncodeToPNG ();
    string file_name = _file_path + _file_path_images + _i.ToString();
    //SaveTextureAsArray (camera, texture2d, file_name+".ssv");
    SaveBytesToFile (data, file_name + ".png");
    _i++;
    //return data;
  }

  void OnRenderImage (RenderTexture source, RenderTexture destination) {
    Graphics.Blit (source, destination, _material);
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

  void SaveTextureAsArray(Camera camera, Texture2D image, string path) {
    string[] str_array = new string[image.height];
    var colors = image.GetPixels32 ();
    for (int iss = 0; iss < image.height; iss++) {
      string str = "";
      for (int jss = 0; jss < image.width; jss++) {
        str = str + (camera.nearClipPlane + colors[iss*image.width + jss].r * camera.farClipPlane) + " ";
      }
      str_array[iss] = str;
    }
    File.WriteAllLines (path, str_array);
  }
}


