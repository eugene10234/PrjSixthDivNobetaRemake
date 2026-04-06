using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Nobeta.Sam.StoryUI
{
    public class StoryUIManager : MonoBehaviour
    {
        private static class SceneNames
        {
            public const string GameScene1 = "GameScene1";
            public const string GameScene2 = "GameScene2";
            public const string BossDungeonUITest = "BossDungeon_1UITEST";
        }



        [SerializeField] private Typewriter CenterMain;
        [SerializeField] private Typewriter CenterSub;
        [SerializeField] private Typewriter BossNameText;
        [SerializeField] private Typewriter DialogText;
        [SerializeField] private GameObject dialogBox;
        [SerializeField] private GameObject ReturnButton;
        [SerializeField] private GameObject DialogNextButton;
        private int currentDialogID;
        private int currentDialogLineID;

        private Dictionary<int, string> DialogLines = new()
        {
            {1,"好......好冷" },
            {2,"驅散生魂，找回記憶的碎片" },
            {3,"我將把妳內心的冰冷化為力量。" },
            {4,"賜予妳，寒冰魔法。" },
            {5,"妳已經準備好了"},
            {6,"前往夢境深淵......"},
            {7,"想起來了．．．．．．我的名字" },
            {8,"我是貝諾塔，千年魔法使" },
        };
        private CancellationTokenSource _cts;
        private IEventBus _eventBus;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            _eventBus = EventBusProvider.GetEventBus();
            _eventBus.Subscribe<EnemyDeathEvent>(OnVictoryTextRequested);
            _eventBus.Subscribe<BossInfoEnableEvent>(OnShowBossNameRequested);
            _eventBus.Subscribe<BossInfoDisableEvent>(OnHideBossNameRequested);
            _eventBus.Subscribe<DialogEvent>(OnDialogEventRequested);
            _eventBus.Subscribe<DialogDisableEvent>(OnDialogDisableEventRequested);
            _eventBus.Subscribe<PlayerLiveStateEvent>(OnShowPlayerDeath);
            _eventBus.Subscribe<GameFinishEvent>(OnShowGameFinishText);
            _eventBus.Subscribe<DialogNextEvent>(OnDialogNextRequested);
            dialogBox.SetActive(false);
            ReturnButton.SetActive(false);
            DialogNextButton.SetActive(false);
            CenterMain.HideText();
            CenterSub.HideText();
            DialogText.HideText();
            BossNameText.HideText();
        }

        public void OnDialogNextRequested(DialogNextEvent e)
        {
            int MaxID;
            switch (currentDialogID)
            {
                case 1:
                    DialogText.HideText();
                    DialogText.FastShowText(DialogLines[currentDialogLineID]);
                    break;
                case 2:
                    currentDialogLineID++;
                    MaxID = Math.Min(currentDialogLineID, 4);
                    DialogText.HideText();
                    DialogText.FastShowText(DialogLines[MaxID]);
                    break;
                case 3:
                    currentDialogLineID++;
                    MaxID = Math.Min(currentDialogLineID, 6);
                    DialogText.HideText();
                    DialogText.FastShowText(DialogLines[MaxID]);
                    break;
                case 4:
                    currentDialogLineID++;
                    DialogText.HideText();
                    DialogText.FastShowText(DialogLines[currentDialogID]);
                    _eventBus.Publish(new DialogDisableEvent());
                    _eventBus.Publish(new GameFinishEvent());
                    break;
            }
        }
        public void OnShowGameFinishText(GameFinishEvent e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            StartGameFinishView(_cts.Token).Forget();
        }


        private async UniTask StartGameFinishView (CancellationToken token)
        {
            await UniTask.Delay(3000, cancellationToken: token);
            CenterMain.HideText();
            CenterMain.FastShowText("遊戲結束");
            ReturnButton.SetActive(true);
        }
        private void OnShowPlayerDeath(PlayerLiveStateEvent e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            StartGameOverSequence(_cts.Token).Forget();
        }

        private async UniTaskVoid StartGameOverSequence(CancellationToken token)
        {
            await UniTask.Delay(1000, cancellationToken: token);
            CenterMain.HideText();
            CenterMain.ShowText("失 " + " 敗");
            await UniTask.Delay(2000, cancellationToken: token);
            ReturnButton.SetActive(true);
        }

        public void OnClickedReturnButton()
        {
            ReturnButton.SetActive(false);
            _eventBus.Publish(new ReturnStatueEvent());
        }

        public void OnClickedDialogNextButton()
        {
            _eventBus.Publish(new DialogNextEvent());
        }
        private void OnHideBossNameRequested(BossInfoDisableEvent e)
        {
            BossNameText.HideText();
        }

        private void OnVictoryTextRequested(EnemyDeathEvent e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            ShowVictoryText(e, _cts.Token).Forget();
        }

        private void OnDialogEventRequested(DialogEvent e)
        {
            CenterMain.HideText();
            CenterSub.HideText();
            switch (e.ID)
            {
                case 1:
                    currentDialogID = 1;
                    dialogBox.SetActive(true);
                    currentDialogLineID = 1;
                    DialogText.FastShowText(DialogLines[currentDialogLineID]);
                    DialogNextButton.SetActive(true);
                    break;
                case 2:
                    currentDialogID = 2;
                    dialogBox.SetActive(true);
                    currentDialogLineID = 2;
                    DialogText.FastShowText(DialogLines[currentDialogLineID]);
                    DialogNextButton.SetActive(true);
                    break;
                case 3:
                    currentDialogID = 3;
                    dialogBox.SetActive(true);
                    currentDialogLineID = 5;
                    DialogText.FastShowText(DialogLines[currentDialogLineID]);
                    DialogNextButton.SetActive(true);
                    break;
                case 4:
                    currentDialogID = 4;
                    dialogBox.SetActive(true);
                    currentDialogLineID = 7;
                    DialogText.FastShowText(DialogLines[currentDialogLineID]);
                    DialogNextButton.SetActive(true);
                    break;
            }
        }

        private void OnDialogDisableEventRequested(DialogDisableEvent e)
        {
            dialogBox.SetActive(false);
            DialogNextButton.SetActive(false);
            DialogText.HideText();
        }

        private async UniTask ShowVictoryText(EnemyDeathEvent e, CancellationToken token)
        {
            try
            {
                CenterMain.ShowText("生 " + "魂 " + "驅 " + "散");
                await UniTask.Delay(2500, cancellationToken: token);
                CenterSub.DirectShowText($"獲得：{e.Reward}");
                await UniTask.Delay(10000, cancellationToken: token);
                CenterMain.HideText();
                await UniTask.Delay(500, cancellationToken: token); // was missing token
                CenterSub.HideText();
            }
            catch (OperationCanceledException)
            {
                // Expected when a new enemy dies before the sequence finishes
                CenterMain.HideText();
                CenterSub.HideText();
            }
        }
        private void OnShowBossNameRequested(BossInfoEnableEvent e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            BossNameText.gameObject.SetActive(true);
            ShowBossName(e, _cts.Token).Forget();
        }

        private async UniTask ShowBossName(BossInfoEnableEvent e, CancellationToken token)
        {
            try
            {
                //CenterSub.DirectShowText(e.BossName);
                await UniTask.Delay(500, cancellationToken: token);
                BossNameText.FastShowText(e.BossName);
            }
            catch (OperationCanceledException)
            {
                BossNameText.HideText();
            }
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe<EnemyDeathEvent>(OnVictoryTextRequested);
            _eventBus.Unsubscribe<BossInfoEnableEvent>(OnShowBossNameRequested);
            _eventBus.Unsubscribe<BossInfoDisableEvent>(OnHideBossNameRequested);
            _eventBus.Unsubscribe<DialogEvent>(OnDialogEventRequested);
            _eventBus.Unsubscribe<DialogDisableEvent>(OnDialogDisableEventRequested);
            _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnShowPlayerDeath);
            _eventBus.Unsubscribe<GameFinishEvent>(OnShowGameFinishText);
            _eventBus.Unsubscribe<DialogNextEvent>(OnDialogNextRequested);
        }


        private void OnEnable()
        {
            // Subscribe to the event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks or errors when the object is destroyed
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Loaded scene: {scene.name}");
            currentDialogLineID = 0;
        }


    }
}
