using UnityEngine;
using VContainer.Unity;
using VContainer;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using Unity.VisualScripting.Antlr3.Runtime;
using System;

namespace Nobeta.Sam.Enemy
{
    public class MGSummonState : IState
    {
        private MageGhostData _mgData;
        private SmallGhostFactory _smallGhostFactory;
        private CancellationToken _token;
        private MGAnimationActor _animation;

        [Inject]
        public void Construct(MageGhostData mgData, SmallGhostFactory smallghostFactory, MGAnimationActor animation)
        { 
            _mgData = mgData;
            _animation = animation;
            _smallGhostFactory = smallghostFactory;
        }

        private List<SmallGhost> SummonedGhosts = new List<SmallGhost>();

        public void Activate(CancellationToken token)
        { 
            _token = token;
        }

        public event Action OnSummonCompleted;
        public void OnEnter()
        {
            if (!CheckSummon())
            {
                _mgData.hasSummoned = true;
                SummonedGhosts.Clear();
                _animation.PlaySummon();
                SummonDeferred(_token).Forget();  // ¡ö renamed, defers before spawning
                OnSummonCompleted?.Invoke();
            }
        }

        public void OnExit()
        {
            
        }

        public void OnFixedTick()
        {
            
        }

        private bool CheckSummon()
        {
            return _mgData.hasSummoned;
        }

        private async UniTaskVoid SummonDeferred(CancellationToken token)
        {
            await Summon(token);
            await UniTask.Delay(1000);  
        }
        private async UniTask Summon(CancellationToken token)
        {
            var spawnLocCount = _mgData.minionSpawnLoc.Count;
            var spawnLoc = _mgData.minionSpawnLoc;
            for (int i = 0; i < spawnLocCount; i++)
            {
                if (token.IsCancellationRequested) return;

                // VFX at this spawn point ¡X awaited so minion appears from effect
                //await _animation.PlaySummonVFX(point, _stat.SummonVFXDuration, token);
                //if (_token.IsCancellationRequested) return;

                SmallGhost minion = _smallGhostFactory.Create(spawnLoc[i].transform.position, spawnLoc[i].transform.rotation);
                SummonedGhosts.Add(minion);

                // Subscribe to death ¡X lambda captures minion.gameObject and this state
                var facade = minion.GetComponent<BaseEnemyFacade>();
                if (facade != null)
                    facade.OnDeath += () => OnMinionDied(minion);
            }
        }

        public event Action OnAllSummonedDied;
        private void OnMinionDied(SmallGhost sg)
        {
            SummonedGhosts.Remove(sg);
            if (SummonedGhosts.Count == 0)
            { 
                _mgData.hasSummoned = false;
                OnAllSummonedDied?.Invoke();
            }
        }

    }

}
