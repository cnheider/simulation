using System;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using MsgPack.Serialization;
using MsgPack;
using Neodroid.Messaging.Messages;

namespace Neodroid.Messaging {
  class MessageServer {
    Thread _polling_thread;

    TcpListener _listener;
    TcpClient _client;
    NetworkStream _stream;

    MessagePackSerializer _environment_state_serializer = MessagePackSerializer.Get<EnvironmentState>();
    MessagePackSerializer _reaction_serializer = MessagePackSerializer.Get<Reaction>();

    int _polling_timeout = 5; //Milliseconds

    public bool _client_connected = false;

    public MessageServer(string ip_address = "127.0.0.1", int port = 5555) {
      _listener = new TcpListener(IPAddress.Parse(ip_address), port);
    }

    public void ListenForClientToConnect(Action callback) {
      Thread _wait_for_client_thread = new Thread(unused_param => WaitForClientToConnect(callback));
      _wait_for_client_thread.IsBackground = true; // Is terminated with foreground threads, when they terminate
      _wait_for_client_thread.Start();
    }

    void WaitForClientToConnect(Action callback) {
      _client_connected = IsClientConnected();
      _listener.Start();
      _client = _listener.AcceptTcpClient();  // Busy wait for client, use on a dedicated thread
      _stream = _client.GetStream();
      _client_connected = IsClientConnected();

      callback();
    }

    public void StartReceiving(Action<Reaction> cmd_callback, Action disconnect_callback, Action<String> error_callback) {
      if (_client_connected) {
        _polling_thread = new Thread(unused_param => PollingThread(cmd_callback, disconnect_callback, error_callback));
        _polling_thread.IsBackground = true; // Is terminated with foreground threads, when they terminate
        _polling_thread.Start();
      }
    }

    public bool IsClientConnected() {
      if (_client != null && _client.Client != null && _client.Client.Connected) { // Detect if client disconnected
        try {
          var polling_time = 1; //Microseconds
          return !(_client.Client.Poll(polling_time, SelectMode.SelectRead) && _client.Client.Available == 0);
        } catch (SocketException) {
          return false;
        }
      } else {
        return false;
      }
    }

    void PollingThread(Action<Reaction> receive_callback, Action disconnect_callback, Action<String> error_callback) {
      while (_client_connected) {
        try {
          Reaction action = (Reaction)_reaction_serializer.Unpack(_stream);
          receive_callback(action);
        } catch (Exception err) {
          error_callback(err.ToString());
        }

        Thread.Sleep(_polling_timeout);
        _client_connected = IsClientConnected();
      }

      disconnect_callback();
    }

    public void SendEnvironmentState(EnvironmentState environment_state) {
      _environment_state_serializer.Pack(_stream, environment_state);
    }

    public void Destroy() {
      KillPollingThread();
    }

    void KillPollingThread() {
      if (_stream != null) _stream.Close();
      if (_polling_thread != null) {
        _polling_thread.Abort();
        _polling_thread.Join();
      }
    }
  }
}
