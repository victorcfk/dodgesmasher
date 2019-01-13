using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon2DSetProjectileReferences : Weapon2DBasic
{
    protected override GameObject FireOffProjectile(
        GameObject inProjectileBasic,
        Transform inFiringPosition,
        float inProjectileSpeed)
    {

        GameObject projectile = base.FireOffProjectile(inProjectileBasic, inFiringPosition, inProjectileSpeed);

        Damager dmgr = projectile.GetComponent<Damager>();

        dmgr.firer = transform;
        dmgr.target = GameManager.Instance.PlayerAvatarTransform;

        return projectile;
    }
}
