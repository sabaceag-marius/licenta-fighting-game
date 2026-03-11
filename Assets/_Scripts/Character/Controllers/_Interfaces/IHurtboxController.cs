
using System.Collections.Generic;
using UnityEngine;

public interface IHurtboxController
{
    void SetDefaultHiurtbox();

    void SetFramedataHurtboxes(List<HurtboxOverrideData> hurtboxes, int facingDirection);

    void ReceiveHit(Collider2D hitCollider, HitboxData hitbox);
}