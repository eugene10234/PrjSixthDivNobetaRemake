using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King summon state — calls SmallGhostFactory to spawn minions.
    /// TODO: implement summon burst logic, animation lock, cooldown gating.
    /// </summary>
    public class GKSummonState : IState
    {
        private GhostKingData _gkData;
        private SmallGhostFactory _smallGhostFactory;
        private GKAnimationActor _animation;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(GhostKingData mgData, SmallGhostFactory smallGhostFactory, GKAnimationActor animation)
        {
            _gkData = mgData;
            _animation = animation;
            _smallGhostFactory = smallGhostFactory;
        }

        private List<SmallGhost> _summonedGhosts = new List<SmallGhost>();

        public event Action OnSummonCompleted;
        public event Action OnAllSummonedDied;

        public void OnEnter()
        {
            //_animation.HideRune();
            if (_gkData.hasSummoned)
            {
                // Already summoned once — skip straight to idle, barrier is already up
                OnSummonCompleted?.Invoke();
                return;
            }

            _cts = new CancellationTokenSource();
            _gkData.hasSummoned = true;
            _summonedGhosts.Clear();
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
            var spawnLocs = _gkData.minionSpawnLoc;
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
                _gkData.hasSummoned = false;
                OnAllSummonedDied?.Invoke();
            }
        }
    }
}
