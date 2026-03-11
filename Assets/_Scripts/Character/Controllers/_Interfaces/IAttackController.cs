
using System.Collections.Generic;

public interface IAttackController
{
    void GenerateHitboxes(List<HitboxData> hitboxes);

    void ClearHitTargets();
}