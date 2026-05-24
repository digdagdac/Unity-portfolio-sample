using UnityEngine;

namespace Muloro.Portfolio
{
    [DisallowMultipleComponent]
    public sealed class PortfolioSinglePlayerBootstrap : MonoBehaviour
    {
        [Header("Scene Actors")]
        [SerializeField]
        private PortfolioOfflinePlayer _player;
        [SerializeField]
        private PortfolioOfflineBoss _boss;

        [Header("Camera")]
        [SerializeField]
        private bool _followPlayerWithMainCamera = true;
        [SerializeField]
        private Vector3 _cameraOffset = new(0f, 21.7f, -45f);
        [SerializeField]
        private bool _disableMainCameraCinemachine = true;

        private Camera _mainCamera;

        private void Awake()
        {
            WireSceneActors();

            _mainCamera = Camera.main;
            DisableMainCameraCinemachine();
            PositionMainCamera(snap: true);
        }

        private void LateUpdate()
        {
            PositionMainCamera(snap: false);
        }

        private void WireSceneActors()
        {
            if (_player == null)
            {
                _player = FindFirstObjectByType<PortfolioOfflinePlayer>();
            }

            if (_boss == null)
            {
                _boss = FindFirstObjectByType<PortfolioOfflineBoss>();
            }

            if (_player == null || _boss == null)
            {
                Debug.LogWarning("[Portfolio] Offline demo requires one player and one boss in the scene.");
                return;
            }

            _player.SetBoss(_boss);
            _boss.SetTarget(_player.transform);
        }

        private void DisableMainCameraCinemachine()
        {
            if (!_disableMainCameraCinemachine || _mainCamera == null)
            {
                return;
            }

            foreach (MonoBehaviour component in _mainCamera.GetComponents<MonoBehaviour>())
            {
                if (component == null)
                {
                    continue;
                }

                string typeName = component.GetType().FullName;
                if (typeName != null && typeName.Contains("Cinemachine"))
                {
                    component.enabled = false;
                }
            }
        }

        private void PositionMainCamera(bool snap)
        {
            if (!_followPlayerWithMainCamera || _mainCamera == null || _player == null)
            {
                return;
            }

            Vector3 targetPosition = _player.transform.position + _cameraOffset;
            _mainCamera.transform.position = targetPosition;
        }
    }
}
