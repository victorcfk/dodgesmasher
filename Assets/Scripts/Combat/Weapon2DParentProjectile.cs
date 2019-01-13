using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon2DParentProjectile : Weapon2DBasic
{
    protected override GameObject FireOffProjectile(
        GameObject inProjectileBasic,
        Transform inFiringPosition,
        float inProjectileSpeed)
    {

        GameObject projectile = base.FireOffProjectile(inProjectileBasic, inFiringPosition, inProjectileSpeed);

        projectile.transform.SetParent(transform);

        return projectile;
    }
}
