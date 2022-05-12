using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class GrassManager : MonoBehaviour
    {
        [SerializeField] private float lodValue = 1;

        private readonly List<ProceduralGrassRenderer> grassRenderers = new List<ProceduralGrassRenderer>();

        private Transform _playerTransform;

        private float _sqrMag;

        private void LateUpdate()
        {
            if (_playerTransform == null && GameManager.Instance.Player)
                _playerTransform = GameManager.Instance.Player.transform;

            if (grassRenderers.Count > 0)
            {
                for (int i = 0; i < grassRenderers.Count; i++)
                {
                    if (_playerTransform != null)
                        _sqrMag = (grassRenderers[i].transform.position - _playerTransform.position).sqrMagnitude;

                    if (lodValue * 100 > _sqrMag)
                        grassRenderers[i].RederingGrass();
                }
            }
        }

        public void AddGrass(ProceduralGrassRenderer grassRenderer)
            => grassRenderers.Add(grassRenderer);
    }
}