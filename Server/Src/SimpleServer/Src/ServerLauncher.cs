using System;
using System.Threading;
using Lockstep.Logging;
using Lockstep.Network;

namespace Lockstep.FakeServer{
    public class ServerLauncher {
        private static Server server;

        public static void Main(){
            //let async functions call in this thread      ���첽����Ϣ �Ž� ��ǰִ�е� ���̵߳���ִ�С�
            //���ã�UI ���µ��̰߳�ȫ�ԣ����첽���ģ�ͣ�����ߴ���ɶ���
            //�׶ˣ���ʱ����Ϣ ����࣬�Һ̣ܶ� ��ô��Ƶ������ �첽 ���䣬��Ҫ ��¼�����ġ��������� ��������cpu�������ʡ������� Ч��
            OneThreadSynchronizationContext contex = new OneThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(contex);
            Debug.Log("Main start");
            try {   
                DoAwake();
                while (true) {
                    try {
                        Thread.Sleep(3); //˯3����   
                        contex.Update(); //������Ϣ���ɷ�
                        server.Update(); //�������߼����£�֡ͬ���� �߼�֡��
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
            server.Start(); //�������ĳ�ʼ����
        }
    }
}