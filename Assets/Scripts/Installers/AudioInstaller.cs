
using Infrastructure.Audio;
using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "Installers/AudioInstaller")]
    public class AudioInstaller : ScriptableObjectInstaller<AudioInstaller>
    {
        [Tooltip("Prefab that only contains an AudioSource (Play On Awake = false)")]
        [SerializeField] private AudioSource sfxSourcePrefab;

        public override void InstallBindings()
        {
            Container.Bind<AudioSource>()
                .FromComponentInNewPrefab(sfxSourcePrefab)
                .UnderTransformGroup("[Audio]")
                .AsSingle();

            Container.BindInterfacesAndSelfTo<AudioManager>()
                .AsSingle()
                .NonLazy();
        }
    }
}