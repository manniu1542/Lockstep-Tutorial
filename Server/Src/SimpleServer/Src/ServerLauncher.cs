using System;
using System.Threading;
using Lockstep.Logging;
using Lockstep.Network;

namespace Lockstep.FakeServer{
    public class ServerLauncher {
        private static Server server;

        public static void Main(){
            //let async functions call in this thread      把异步的消息 放进 当前执行的 主线程当中执行。
            //作用：UI 更新的线程安全性，简化异步编程模型，并提高代码可读性
            //弊端：有时候消息 体过多，且很短， 那么会频繁调用 异步 传输，且要 记录上下文。这样反而 不能提升cpu的利用率。降低了 效率
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