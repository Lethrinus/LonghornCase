using Infrastructure.Signals;
using Managers;
using UnityEngine;
using Zenject;


public sealed class StateTransitions : MonoBehaviour
{
    [Inject] private SignalBus _bus;

    /* ------------------------------------------------------------------ */
    void OnEnable()
    {
        _bus.Subscribe<PenClickedSignal>   (OnPenClicked);
        _bus.Subscribe<PenHoverCanceledSignal>  (OnPenHoverCanceled);  
        _bus.Subscribe<BoardDrawnSignal>   (OnBoardDrawn);
        _bus.Subscribe<CupClickedSignal>   (OnCupClicked);
        _bus.Subscribe<CupHoverCanceledSignal>(OnCupHoverCanceled);
        _bus.Subscribe<CupFilledSignal>    (OnCupFilled);
        _bus.Subscribe<PlantClickedSignal> (OnPlantClicked);
        _bus.Subscribe<TrashThrownSignal>  (OnTrashThrown);
        _bus.Subscribe<DotClickedSignal>   (OnDotClicked);   
    }

    void OnDisable()
    {
        _bus.Unsubscribe<PenClickedSignal>   (OnPenClicked);
        _bus.Unsubscribe<PenHoverCanceledSignal>   (OnPenHoverCanceled);
        _bus.Unsubscribe<BoardDrawnSignal>   (OnBoardDrawn);
        _bus.Unsubscribe<CupClickedSignal>   (OnCupClicked);
        _bus.Unsubscribe<CupHoverCanceledSignal>(OnCupHoverCanceled);
        _bus.Unsubscribe<CupFilledSignal>    (OnCupFilled);
        _bus.Unsubscribe<PlantClickedSignal> (OnPlantClicked);
        _bus.Unsubscribe<TrashThrownSignal>  (OnTrashThrown);
        
    }

    /* ------------------- callbacks ------------------------------------ */
    void OnPenClicked   (PenClickedSignal   _) => GameManager.Instance.GotoDrawBoard();
    void OnPenHoverCanceled  (PenHoverCanceledSignal  _) => GameManager.Instance.GotoClickPen();   
    void OnBoardDrawn   (BoardDrawnSignal   _) => GameManager.Instance.GotoClickCup();
    void OnCupClicked   (CupClickedSignal   _) => GameManager.Instance.GotoClickDispenser();
    void OnCupHoverCanceled(CupHoverCanceledSignal _) => GameManager.Instance.GotoClickCup();  
    void OnCupFilled    (CupFilledSignal    _) => GameManager.Instance.GotoClickPlant();
    void OnPlantClicked (PlantClickedSignal _) => GameManager.Instance.GotoThrowTrash();
    void OnTrashThrown (TrashThrownSignal _) => GameManager.Instance.GotoAwaitDot(); 
    void OnDotClicked  (DotClickedSignal  _) => GameManager.Instance.GotoCompleted();
    
}