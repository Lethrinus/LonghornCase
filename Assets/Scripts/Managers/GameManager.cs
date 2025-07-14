using UnityEngine;

namespace Managers
{
    public enum GameState
    {
        ClickPen,
        DrawBoard,
        ClickCup,
        ClickDispenser,
        ReturnCup,
        ClickPlant,
        ThrowTrash,
        Completed
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        GameState _state;
        public GameState State => _state;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetState(GameState.ClickPen);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        
         public void SetState(GameState newState)
        {
            if (_state == newState) return;
            _state = newState;
          
        }
        
        public void GotoClickPen()        => SetState(GameState.ClickPen);
        public void GotoDrawBoard()       => SetState(GameState.DrawBoard);
        public void GotoClickCup()        => SetState(GameState.ClickCup);
        public void GotoClickDispenser()  => SetState(GameState.ClickDispenser);
        public void GotoReturnCup()       => SetState(GameState.ReturnCup);
        public void GotoClickPlant()      => SetState(GameState.ClickPlant);
        public void GotoThrowTrash()      => SetState(GameState.ThrowTrash);
        public void GotoCompleted()       => SetState(GameState.Completed);
    }
}