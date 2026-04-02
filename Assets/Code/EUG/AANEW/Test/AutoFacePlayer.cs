using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.Tests
{
    public class AutoFacePlayer : MonoBehaviour, IInteractable
    {
        [Header("偵測設定")]
        public float detectionRadius = 10f;    // 偵測半徑 R
        public LayerMask playerLayer;          // 指定 Player 的 Layer
        public float rotationSpeed = 5f;       // 轉向速度
        private bool IsPlayerInDetectionRange = false;
        private IEventBus _eventBus;
        private int _dMaxLevel = 0;
        private int _dCurrLevel = -1;
        private bool _facingFlag = false;

        public string InteractableName => "UnityChan";

        public float InteractionDuration => 0.1f;

        private void Awake()
        {
            _eventBus = EventBusProvider.GetEventBus();
            _dMaxLevel = 0;
            _dCurrLevel = -1;
            IsPlayerInDetectionRange = false;
            _facingFlag = false;
        }
        void Update()
        {
            Collider[] colliders;
            IsPlayerInDetectionRange = DetectPlayerInRadius(out colliders);
            if (IsPlayerInDetectionRange)
            {
                Transform target = colliders[0].transform;
                if (_facingFlag)
                {
                    FaceTarget(target);
                }
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < detectionRadius * 0.3f)
                {
                    if (_dCurrLevel == 3) return;
                    _eventBus.Publish(new InteractEreaEnterEvent
                    {
                        InteractObj = this,
                        Message = $"UnityChan亮出血條了!!!!!"
                    });
                    _eventBus.Publish(new BossInfoEnableEvent
                    {      
                    });
                    _facingFlag = true;
                    _dMaxLevel = Mathf.Max(_dMaxLevel, 3);
                    _dCurrLevel = 3;
                }
                else if (distance < detectionRadius * 0.5f)
                {
                    if (_dCurrLevel == 2) return;
                    if (_dMaxLevel <= 2)
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan希望你離開!!!"
                        });
                    }
                    else
                    {
                        if (_dCurrLevel >= 3)
                        {
                            _eventBus.Publish(new InteractEreaEnterEvent
                            {
                                InteractObj = this,
                                Message = $"UnityChan希望你繼續離開!!!"
                            });
                        }
                        if (_dCurrLevel <= 1)
                        {
                            _eventBus.Publish(new InteractEreaEnterEvent
                            {
                                InteractObj = this,
                                Message = $"UnityChan希望你繼續離開!!!"
                            });
                        }
                    }
                    if (_dMaxLevel == 0) return;
                    _facingFlag = true;
                    _dMaxLevel = Mathf.Max(_dMaxLevel, 2);
                    _dCurrLevel = 2;
                }
                else if (distance < detectionRadius * 0.8f)
                {
                    if (_dCurrLevel == 1) return;                    
                    if (_dMaxLevel <= 1)
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan注意到你了!"
                        });
                    }
                    else if (_dMaxLevel <= 2)
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan緊戒的盯著你!"
                        });
                    }
                    else
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan收起血條，但死死的盯著你!"
                        });
                        _eventBus.Publish(new BossInfoDisableEvent
                        {
                        });
                    }
                    _dMaxLevel = Mathf.Max(_dMaxLevel, 1);
                    _facingFlag = true;
                    _dCurrLevel = 1;
                }  
                else
                {
                    if (_dCurrLevel == 0) return;
                    if (_dMaxLevel <= 1)
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan感覺周圍好像有人"
                        });
                        _facingFlag = false;
                    }
                    else
                    {
                        _eventBus.Publish(new InteractEreaEnterEvent
                        {
                            InteractObj = this,
                            Message = $"UnityChan稍微放鬆了姿態"
                        });
                        _facingFlag = true;
                    }
                    _dCurrLevel = 0;
                }

            }
            else
            {
                //Debug.Log("No Player in Detection Range.");
                _eventBus.Publish(new InteractEreaExitEvent
                {
                    InteractObj = this,
                    Message = $"UnityChan不再注意你了!"
                });
                _dMaxLevel = 0;
                _facingFlag = false;
                _dCurrLevel =-1;
            }

        }

        private bool DetectPlayerInRadius(out Collider[] colliders)
        {
            colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
            //colliders[0].transform.position
            return colliders.Length > 0;
        }

        void FindAndFacePlayer()
        {
            // 1. 偵測半徑 R 內是否有在 playerLayer 的碰撞體
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

            if (colliders.Length > 0)
            {
                // 取得第一個偵測到的目標 (Player)
                Transform target = colliders[0].transform;

                FaceTarget(target);
            }
        }

        private void FaceTarget(Transform target)
        {
            // 2. 計算轉向向量
            Vector3 direction = (target.position - transform.position).normalized;

            // 如果你只想要水平旋轉 (y軸)，取消下面這行的註解：
            // direction.y = 0;

            if (direction != Vector3.zero)
            {
                // 3. 計算目標旋轉值
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // 4. 平滑轉動 (Slerp)
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }

        // 在 Scene 視窗畫出偵測範圍，方便調試
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.8f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.3f);

        }

        public void OnInteract()
        {
            throw new NotImplementedException();
        }
    }
}
