using UnityEngine;
using Core;

namespace Managers
{
    public enum GameState
    {
        ClickPen,
        DrawBoard,
        FillGlass,
        WaterPlant,
        ThrowTrash,
        UnlockDoor,
        Completed
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; }
        public event System.Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        void Start() => SetState(GameState.ClickPen);

        public void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(State);
        }

        void OnEnable()
        {
            EventBus.Subscribe<PenClickedEvent>(_ => SetState(GameState.DrawBoard));
            EventBus.Subscribe<BoardDrawnEvent>(_ => SetState(GameState.FillGlass));
            EventBus.Subscribe<GlassFilledEvent>(_ => SetState(GameState.WaterPlant));
            EventBus.Subscribe<PlantWateredEvent>(_ => SetState(GameState.ThrowTrash));
            EventBus.Subscribe<TrashThrownEvent>(_ => SetState(GameState.UnlockDoor));
            EventBus.Subscribe<DoorUnlockedEvent>(_ => SetState(GameState.Completed));
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<PenClickedEvent>(_ => SetState(GameState.DrawBoard));
            EventBus.Unsubscribe<BoardDrawnEvent>(_ => SetState(GameState.FillGlass));
            EventBus.Unsubscribe<GlassFilledEvent>(_ => SetState(GameState.WaterPlant));
            EventBus.Unsubscribe<PlantWateredEvent>(_ => SetState(GameState.ThrowTrash));
            EventBus.Unsubscribe<TrashThrownEvent>(_ => SetState(GameState.UnlockDoor));
            EventBus.Unsubscribe<DoorUnlockedEvent>(_ => SetState(GameState.Completed));
        }
    }
}