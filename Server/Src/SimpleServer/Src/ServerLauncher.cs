using System;
using System.Threading;
using Lockstep.Logging;
using Lockstep.Network;

namespace Lockstep.FakeServer{
    public class ServerLauncher {
        private static Server server;

        public static void Main(){
            //let async functions call in this thread      把异步的消息 放进异步线程当中
            OneThreadSynchronizationContext contex = new OneThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(contex);
            Debug.Log("Main start");
            try {
                DoAwake();
                while (true) {
                    try {
                        Thread.Sleep(3); //睡3毫秒   
                        contex.Update(); //网络消息的派发
                        server.Update(); //服务器逻辑更新（帧同步的 逻辑帧）
                    }
                    catch (ThreadAbortException e) {
                        return;
                    }
                    catch (Exception e) {
                        Log.Error(e.ToString());
                    }
                }
            }
            catch (ThreadAbortException e) {
                return;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
            }
        }
        static void DoAwake(){
            server = new Server();
            server.Start(); //服务器的初始设置
        }
    }
}