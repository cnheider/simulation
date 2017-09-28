using UnityEngine;
using System.IO;
using System.Text;
using Assets.Scripts.Grasping;

public class DataCollector : MonoBehaviour {

  public bool _deleteFileContent = false;
  public Gripper _gripper;
  public Grasp _target;
  public Camera _depth_camera;
  public Camera _infrared_shadow_camera;
  public Camera _rgb_camera;
  public Camera _segmentation_camera;
  public int _episode_length = 100; // Sampling rate
  string _file_path = @"training_data/";
  string _file_path_gripper = @"gripper_position_rotation.csv";
  string _file_path_target = @"target_position_rotation.csv";
  string _file_path_depth = @"depth/";
  string _file_path_infrared_shadow = @"infrared_shadow/";
  string _file_path_rgb = @"rgb/";
  string _file_path_segmentation = @"segmentation/";

  int _i = 0;
  int _current_episode_progress = 0;

  void Start () {
    if (!_gripper) {
      _gripper = FindObjectOfType<Gripper> ();
    }
    if (!_target) {
      _target = FindObjectOfType<Grasp> ();
    }
    if (!_depth_camera) {
      _depth_camera = FindObjectOfType<Camera> ();
    }
    if (_depth_camera) {
      //_depth_camera.depthTextureMode = DepthTextureMode.Depth;
    }

    //print ("GPU supports depth format: " + SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth));
    //print ("GPU supports shadowmap format: " + SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Shadowmap));

    File.WriteAllText (_file_path + _file_path_gripper, "frame, x, y, z, rot_x, rot_y, rot_z\n");
    File.WriteAllText (_file_path + _file_path_target, "frame, x, y, z, rot_x, rot_y, rot_z\n");

    /*if (!File.Exists(_file_path + _file_path_pos_rot)) {
      print("Created file/path: " + _file_path + _file_path_pos_rot);
      //File.Create(_file_path + _file_path_pos_rot);
    }
    if (_deleteFileContent) {
      File.WriteAllText(_file_path + _file_path_pos_rot, "");
      _deleteFileContent = false;
    }*/
  }

  void FixedUpdate () {
    if (_current_episode_progress == _episode_length - 1) {
      Vector3 screenPoint = _depth_camera.WorldToViewportPoint (_target.transform.position);
      if (screenPoint.z > 0 && screenPoint.x > 0.1 && screenPoint.x < 0.9 && screenPoint.y > 0.1 && screenPoint.y < 0.9) {
        Vector3 gripper_position_relative_to_camera = this.transform.InverseTransformPoint (_gripper.transform.position);
        Vector3 gripper_direction_relative_to_camera = this.transform.InverseTransformDirection (_gripper.transform.eulerAngles);
        var gripper_transform_output = GetTransformOutput (_i, gripper_position_relative_to_camera, gripper_direction_relative_to_camera);
        SaveToCSV (_file_path + _file_path_gripper, gripper_transform_output);

        Vector3 target_position_relative_to_camera = this.transform.InverseTransformPoint (_target.transform.position);
        Vector3 target_direction_relative_to_camera = this.transform.InverseTransformDirection (_target.transform.eulerAngles);
        var target_transform_output = GetTransformOutput (_i, target_position_relative_to_camera, target_direction_relative_to_camera);
        SaveToCSV (_file_path + _file_path_target, target_transform_output);

        SaveRenderTextureToImage (_i, _depth_camera, _file_path_depth);
        SaveRenderTextureToImage (_i, _infrared_shadow_camera, _file_path_infrared_shadow);
        SaveRenderTextureToImage (_i, _rgb_camera, _file_path_rgb);
        SaveRenderTextureToImage (_i, _segmentation_camera, _file_path_segmentation);

        _i++;
      }
      _current_episode_progress = 0;
    }
    _current_episode_progress++;
  }

  string[] GetTransformOutput(int id, Vector3 pos, Vector3 dir){
    return new string[] {
        id.ToString (),
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


  public void SaveRenderTextureToImage(int id, Camera camera, string file_name_dd) {
    var texture2d = RenderTextureImage (camera);
    var data = texture2d.EncodeToPNG ();
    string file_name = _file_path + file_name_dd + id.ToString();
    //SaveTextureAsArray (camera, texture2d, file_name+".ssv");
    SaveBytesToFile (data, file_name + ".png");
    //return data;
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


