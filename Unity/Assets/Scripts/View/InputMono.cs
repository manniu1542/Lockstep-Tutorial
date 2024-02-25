using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial {

    public class InputMono : UnityEngine.MonoBehaviour {
        private static bool IsReplay => GameManager.Instance.IsReplay;
        [HideInInspector] public int floorMask;
        public float camRayLength = 100;

        public bool hasHitFloor;
        public LVector2 mousePos;
        public LVector2 inputUV;
        public bool isInputFire;
        public int skillId;
        public bool isSpeedUp;

        void Start(){
            floorMask = LayerMask.GetMask("Floor");
        }
        //把该帧的相应的操作记录 成数据  
        public void Update(){
            if (!IsReplay) {
                //键盘 左右
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                inputUV = new LVector2(h.ToLFloat(), v.ToLFloat());
                //键盘开火
                isInputFire = Input.GetButton("Fire1");
                //点击 地板当前玩家 100以内的位置
                hasHitFloor = Input.GetMouseButtonDown(1);
                if (hasHitFloor) {
                    Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit floorHit;
                    if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)) {
                        mousePos = floorHit.point.ToLVector2XZ();
                    }
                }
                //技能按键 a b c d e f
                skillId = -1;
                for (int i = 0; i < 6; i++) {//
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                        skillId = i;
                    }
                }
                // 加速 按键
                isSpeedUp = Input.GetKeyDown(KeyCode.Space);
                //当前输入的 赋值
                GameManager.CurGameInput =  new PlayerInput() {
                    mousePos = mousePos,
                    inputUV = inputUV,
                    isInputFire = isInputFire,
                    skillId = skillId,
                    isSpeedUp = isSpeedUp,
                };
                
            }
        }
    }
}