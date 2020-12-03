using UnityEngine;

public interface IEnemyBehavior
{
    void SetCameraReference(Transform _camera);
    void SetFaceRight(bool _facingRight);
    void DealDamageToEnemy(int _damage, Vector3 _damageSourceLocation, Vector2 _force);
    bool CanJumpOn(Vector3 _playerPosition);
    bool IsVulnerable();
    void Clash(Vector3 _clashPosition);
    void Kill();
    int RemainingHealth();
}