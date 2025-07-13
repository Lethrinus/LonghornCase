using System;
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

    
        static readonly Action<PenClickedEvent>        _onPenClicked        = _ => Instance.SetState(GameState.DrawBoard);
        static readonly Action<BoardDrawnEvent>        _onBoardDrawn       = _ => Instance.SetState(GameState.ClickCup);
        static readonly Action<CupClickedEvent>        _onCupClicked       = _ => Instance.SetState(GameState.ClickDispenser);
        static readonly Action<CupFilledEvent>         _onCupFilled        = _ => Instance.SetState(GameState.ClickPlant);
        static readonly Action<PlantClickedEvent>      _onPlantClicked     = _ => Instance.SetState(GameState.ThrowTrash);
        static readonly Action<TrashThrownEvent>       _onTrashThrown      = _ => Instance.SetState(GameState.Completed);
        static readonly Action<CupHoverCancelledEvent> _onCupHoverCancel   = _ => Instance.SetState(GameState.ClickCup);
        static readonly Action<HoverCancelledEvent>    _onHoverCancelled   = _ => Instance.SetState(GameState.ClickPen);

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

        void OnEnable() {
            EventBus.Subscribe(_onPenClicked);
            EventBus.Subscribe(_onBoardDrawn);
            EventBus.Subscribe(_onCupClicked);
            EventBus.Subscribe(_onCupFilled);
            EventBus.Subscribe(_onPlantClicked);
            EventBus.Subscribe(_onTrashThrown);
            EventBus.Subscribe(_onCupHoverCancel);
            EventBus.Subscribe(_onHoverCancelled);
        }

        void OnDisable() {
            EventBus.Unsubscribe(_onPenClicked);
            EventBus.Unsubscribe(_onBoardDrawn);
            EventBus.Unsubscribe(_onCupClicked);
            EventBus.Unsubscribe(_onCupFilled);
            EventBus.Unsubscribe(_onPlantClicked);
            EventBus.Unsubscribe(_onTrashThrown);
            EventBus.Unsubscribe(_onCupHoverCancel);
            EventBus.Unsubscribe(_onHoverCancelled);
        }
    }
}