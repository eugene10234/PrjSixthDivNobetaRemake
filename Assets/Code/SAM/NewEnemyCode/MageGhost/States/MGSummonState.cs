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
        private MGAnimationActor _animation;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(MageGhostData mgData, SmallGhostFactory smallGhostFactory, MGAnimationActor animation)
        {
            _mgData = mgData;
            _animation = animation;
            _smallGhostFactory = smallGhostFactory;
        }

        private List<SmallGhost> _summonedGhosts = new List<SmallGhost>();

        public event Action OnSummonCompleted;
        public event Action OnAllSummonedDied;

        public void OnEnter()
        {
            _animation.HideRune();
            if (_mgData.hasSummoned)
            {
                // Already summoned once — skip straight to idle, barrier is already up
                OnSummonCompleted?.Invoke();
                return;
            }

            _cts = new CancellationTokenSource();
            _mgData.hasSummoned = true;
            _summonedGhosts.Clear();
            _animation.PlaySummon();

            // OnSummonCompleted fires AFTER all ghosts are spawned, inside the async flow
            RunSummonSequence(_cts.Token).Forget();
        }

        public void OnExit()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void OnFixedTick() { }

        private async UniTaskVoid RunSummonSequence(CancellationToken token)
        {
            try
            {
                await SpawnMinions(token);

                // Brief pause so the summon animation can finish before we transition
                await UniTask.Delay(1000, cancellationToken: token);

                OnSummonCompleted?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // State was interrupted — no transition, machine will handle it
            }
        }

        private async UniTask SpawnMinions(CancellationToken token)
        {
            var spawnLocs = _mgData.minionSpawnLoc;
            for (int i = 0; i < spawnLocs.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                SmallGhost minion = _smallGhostFactory.Create(
                    spawnLocs[i].transform.position,
                    spawnLocs[i].transform.rotation);

                _summonedGhosts.Add(minion);

                var facade = minion.GetComponent<BaseEnemyFacade>();
                if (facade != null)
                    facade.OnDeath += () => OnMinionDied(minion);

                // Small stagger between spawns so they don't all pop at once
                await UniTask.Delay(200, cancellationToken: token);
            }
        }

        private void OnMinionDied(SmallGhost sg)
        {
            _summonedGhosts.Remove(sg);
            if (_summonedGhosts.Count == 0)
            {
                _mgData.hasSummoned = false;
                OnAllSummonedDied?.Invoke();
            }
        }
    }

}
