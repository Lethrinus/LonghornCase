// Assets/Scripts/Installers/GameSignalInstaller.cs
using Infrastructure.Signals;
using Zenject;

namespace Installers
{
    public class GameSignalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            /* ---------- audio ---------- */
            Container.DeclareSignal<PlaySfxSignal>().OptionalSubscriber();

            /* ---------- gameplay ------- */
            Container.DeclareSignal<CupClickedSignal>()         .OptionalSubscriber();
            Container.DeclareSignal<CupHoverCanceledSignal>()   .OptionalSubscriber();
            Container.DeclareSignal<CupFilledSignal>()          .OptionalSubscriber();
            Container.DeclareSignal<CupStartedFloatingSignal>() .OptionalSubscriber();
            Container.DeclareSignal<PlantClickedSignal>()       .OptionalSubscriber();
            Container.DeclareSignal<TrashThrownSignal>()        .OptionalSubscriber();

            Container.DeclareSignal<PenClickedSignal>()         .OptionalSubscriber();
            Container.DeclareSignal<PenHoverCanceledSignal>()   .OptionalSubscriber();
            Container.DeclareSignal<BoardDrawnSignal>()         .OptionalSubscriber();

            Container.DeclareSignal<DotClickedSignal>().OptionalSubscriber();
        }
    }
}