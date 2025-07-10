using UnityEngine;
using Core;

namespace Managers {
    public enum GameState {
        ClickPen,
        DrawBoard,
        ClickCup,
        ClickDispenser,
        ReturnCup,
        ClickPlant,
        ThrowTrash,   
        Completed
    }
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; }

        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        void Start() {
            SetState(GameState.ClickPen);
        }

        public void SetState(GameState s) {
            State = s;
        }

        void OnEnable()
        {
            EventBus.Subscribe<PenClickedEvent>(_        => SetState(GameState.DrawBoard));
            EventBus.Subscribe<BoardDrawnEvent>(_        => SetState(GameState.ClickCup));
            EventBus.Subscribe<CupClickedEvent>(_        => SetState(GameState.ClickDispenser));
            EventBus.Subscribe<CupFilledEvent>(_         => SetState(GameState.ClickPlant));
            EventBus.Subscribe<PlantClickedEvent>(_      => SetState(GameState.ThrowTrash));
            EventBus.Subscribe<TrashThrownEvent>(_       => SetState(GameState.Completed));
            
            EventBus.Subscribe<HoverCancelledEvent>(_    => SetState(GameState.ClickPen));
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<PenClickedEvent>(_        => SetState(GameState.DrawBoard));
            EventBus.Unsubscribe<BoardDrawnEvent>(_        => SetState(GameState.ClickCup));
            EventBus.Unsubscribe<CupClickedEvent>(_        => SetState(GameState.ClickDispenser));
            EventBus.Unsubscribe<CupFilledEvent>(_         => SetState(GameState.ClickPlant));
            EventBus.Unsubscribe<PlantClickedEvent>(_      => SetState(GameState.ThrowTrash));
            EventBus.Unsubscribe<TrashThrownEvent>(_       => SetState(GameState.Completed));
    
            EventBus.Unsubscribe<HoverCancelledEvent>(_    => SetState(GameState.ClickPen));
        }

      
    }
}
