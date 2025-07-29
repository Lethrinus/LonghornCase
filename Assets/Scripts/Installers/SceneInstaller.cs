using Audio;
using Managers;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField] AudioPool audioPoolPrefab;   

    public override void InstallBindings()
    {
        /* Signals */
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<SfxSignal>();
        
        /* GameManager */
        Container.Bind<GameManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        /* AudioPool  */
        Container.Bind<AudioPool>()
            .FromComponentInNewPrefab(audioPoolPrefab)
            .AsSingle()
            .NonLazy();

        /* Route SfxSignal -> AudioPool */
        Container.BindSignal<SfxSignal>()
            .ToMethod<AudioPool>(x => x.HandleSignal)
            .FromResolve();
    }
}