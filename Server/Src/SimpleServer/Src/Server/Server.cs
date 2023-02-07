using System;
using System.Collections.Generic;
using System.Net;
using Lockstep.Logging;
using Lockstep.Network;
using Lockstep.Util;

namespace Lockstep.FakeServer{
    public class Server : IMessageDispatcher {
        //network
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 10083);
        //网络外发代理人
        private NetOuterProxy _netProxy = new NetOuterProxy(); 
        
        //update
        private const double UpdateInterval = 0.015; //frame rate = 30    
        private DateTime _lastUpdateTimeStamp;
        private DateTime _startUpTimeStamp;
        private double _deltaTime;
        private double _timeSinceStartUp;

        //user mgr 
        private Room _room;
        private Dictionary<int, PlayerServerInfo> _id2Player = new Dictionary<int, PlayerServerInfo>();
        private Dictionary<int, Session> _id2Session = new Dictionary<int, Session>();
        private Dictionary<string, PlayerServerInfo> _name2Player = new Dictionary<string, PlayerServerInfo>();

        //id
        private static int _idCounter = 0;
        private int _curCount = 0;
        
        

        public void Start(){
            
            _netProxy.MessageDispatcher = this;//消息派发员
            _netProxy.MessagePacker = MessagePacker.Instance; //消息包装者
            _netProxy.Awake(NetworkProtocol.TCP, serverIpPoint); // IP 端口号
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now; // 时间戳
        }

        //继承的接口 。消息的派发
        public void Dispatch(Session session, Packet packet){
            ushort opcode = packet.Opcode();
            var message = session.Network.MessagePacker.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index) as IMessage;
            //var msg = JsonUtil.ToJson(message);
            //Log.sLog("Server " + msg);
            var type = (EMsgType) opcode;
            switch (type) {
                case EMsgType.JoinRoom://加入房间（主游戏）
                    OnPlayerConnect(session, message);
                    break;
                case EMsgType.QuitRoom://退出房间 （主游戏）
                    OnPlayerQuit(session, message);
                    break;
                case EMsgType.PlayerInput:
                    OnPlayerInput(session, message);
                    break;
                case EMsgType.HashCode:
                    //客户端  每一帧的 帧号 计算一个hash ，把相应的hash发送给服务器，服务器收到相应的hash 就会有个匹配 判定， 如果有客户端的hash 与服务器不同 那么 就会log出来。
                    OnPlayerHashCode(session, message);
                    break;
            }
        }

        public void Update(){
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            //   这边是当前帧 大于 上一帧 所间隔 15毫秒  跑一帧   1000/15 约等于66.66循环 但是 总 update是 间隔3毫秒运行一次，每次 都是 大于 15 ，3的6被 18毫秒 1000/18 约等于55.55循环 。 整体 大约就是 1帧跑60次 update， 
            if (_deltaTime > UpdateInterval) { 
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;//服务器运行 总时间
                DoUpdate();
            }
        }

        public void DoUpdate(){
            //check frame inputs
            //当前只有 一个房间 。如果多个房间就创建 个数组 for循环
            var fDeltaTime = (float) _deltaTime;
            var fTimeSinceStartUp = (float) _timeSinceStartUp;
            _room?.DoUpdate(fTimeSinceStartUp, fDeltaTime);
        }


        void OnPlayerConnect(Session session, IMessage message){
            //TODO load from db
            
            var msg = message as Msg_JoinRoom;
            msg.name = msg.name + _idCounter;
            var name = msg.name;
            if (_name2Player.TryGetValue(name, out var val)) {
                return;
            }

            var info = new PlayerServerInfo();
            info.Id = _idCounter++;
            info.name = name;
            _name2Player[name] = info;
            _id2Player[info.Id] = info;
            _id2Session[info.Id] = session;
            session.BindInfo = info;
            _curCount++;
            if (_curCount >= Room.MaxPlayerCount) {
                _room = new Room();
                _room.Init(0);
                foreach (var player in _id2Player.Values) {
                    _room.OnPlayerJoin(_id2Session[player.Id], player);
                }

                OnGameStart(_room);
            }
            Debug.Log("OnPlayerConnect count:" + _curCount + " " + JsonUtil.ToJson(msg));
        }

        void OnPlayerQuit(Session session, IMessage message){
            Debug.Log("OnPlayerQuit count:" + _curCount);
            var player = session.GetBindInfo<PlayerServerInfo>();
            if (player == null)
                return;
            _id2Player.Remove(player.Id);
            _name2Player.Remove(player.name);
            _id2Session.Remove(player.Id);
            _curCount--;
            if (_curCount == 0) { 
                _room = null;
            }
        }

        void OnPlayerInput(Session session, IMessage message){
            var msg = message as Msg_PlayerInput;
            var player = session.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerInput(player.Id, msg);
        }
        void OnPlayerHashCode(Session session, IMessage message){
            var msg = message as Msg_HashCode;
            var player = session.GetBindInfo<PlayerServerInfo>();
            _room?.OnPlayerHashCode(player.Id, msg);
        }

        void OnGameStart(Room room){
            if (room.IsRunning) {
                return;
            }

            room.OnGameStart();
        }
    }
}