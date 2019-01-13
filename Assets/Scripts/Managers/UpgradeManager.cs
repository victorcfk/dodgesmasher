using System;
using UnityEngine;

public class UpgradeManager : MonoBehaviour {

    public enum UpgradeType
    {
        ENERGY,
        HEALTH,
        DMG,
        SEGMENT_SKIPS,
        ORBITAL
    }
    
    Action<int> _onPurchaseMade;
    Func<int> ObtainCurrentShards; //not the best implementation?
    Func<int> ObtainHighestLevel; //not the best implementation?
    Func<bool> ObtainIsPayingPlayer; //not the best implementation?

    int _playerHealthUpgrades;
    int _playerEnergyUpgrades;
    int _playerDMGUpgrades;
    int _playerOrbitalUpgrades;

    // Use this for initialization
    public void Initialise(Action<int> inOnPurchaseMade , Func<int> inObtainCurrentShards, Func<int> inObtainHighestLevel, Func<bool> inObtainIsPayingPlayer)
    {
        _onPurchaseMade += inOnPurchaseMade;

        ObtainCurrentShards += inObtainCurrentShards;
        ObtainHighestLevel += inObtainHighestLevel;
        ObtainIsPayingPlayer += inObtainIsPayingPlayer;

        _playerHealthUpgrades = 0;
        _playerEnergyUpgrades = 0;
        _playerDMGUpgrades = 0;
        _playerOrbitalUpgrades = 0;
    }

    /// <summary>
    /// We keep the cost inside to avoid hacking. Ideally we pull this from a server, but in interests of time
    /// </summary>
    /// <param name="inUpgradeType"></param>
    public int GetCostOf(UpgradeType inUpgradeType)
    {
        if (ObtainIsPayingPlayer()) return 0;

        int cost = 0;
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                cost = 100 + 100 * _playerEnergyUpgrades;   //100,200,300,400,500 => max 1500
                break;
            case UpgradeType.HEALTH:
                cost = 100 + 100 * _playerHealthUpgrades;   //100,200,300,400,500 => max 1500
                break;
            case UpgradeType.DMG:
                cost = 200 + 200 * _playerDMGUpgrades;  //200,400,600 => max 1200
                break;
            case UpgradeType.SEGMENT_SKIPS:
                //cost = 400 + 400 * _ownedUpgrades.NumberOf(inUpgradeType);
                break;
            case UpgradeType.ORBITAL:
               cost = 2000 + 2000 * _playerOrbitalUpgrades; //2000,4000,6000 shards => max 12000
                break;
            default:
                cost = 1000;
                break;
        }
        return cost;
    }

    /// <summary>
    /// We keep the cost inside to avoid hacking. Ideally we pull this from a server, but in interests of time
    /// </summary>
    /// <param name="inUpgradeType"></param>
    public int GetMaxNumberOf(UpgradeType inUpgradeType)
    {
        int maxNumber;
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                maxNumber = 5;
                break;
            case UpgradeType.HEALTH:
                maxNumber = 5;
                break;
            case UpgradeType.DMG:
                maxNumber = 3;
                break;
            case UpgradeType.SEGMENT_SKIPS:
                maxNumber = 3;
                break;
            case UpgradeType.ORBITAL:
                maxNumber = 3;
                break;
            default:
                maxNumber = 1;
                break;
        }
        return maxNumber;
    }

    public int GetOwnedNumberOf(UpgradeType inUpgradeType)
    {
        int ownedNum = 0;
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                ownedNum = _playerEnergyUpgrades;
                break;
            case UpgradeType.HEALTH:
                ownedNum = _playerHealthUpgrades;
                break;
            case UpgradeType.DMG:
                ownedNum = _playerDMGUpgrades;
                break;
            case UpgradeType.SEGMENT_SKIPS:
                break;
            case UpgradeType.ORBITAL:
                ownedNum = _playerOrbitalUpgrades;
                break;
            default:
                break;
        }
        return ownedNum;
    }

    public void SetNumberOfOwned(UpgradeType inUpgradeType, int inNumOfOwned)
    {
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                _playerEnergyUpgrades = inNumOfOwned;
                break;
            case UpgradeType.HEALTH:
                _playerHealthUpgrades = inNumOfOwned;
                break;
            case UpgradeType.DMG:
               _playerDMGUpgrades = inNumOfOwned;
                break;
            case UpgradeType.SEGMENT_SKIPS:
                break;
            case UpgradeType.ORBITAL:
                _playerOrbitalUpgrades = inNumOfOwned;
                break;
            default:
                break;
        }
    }

    public void AddOneNumberOfOwned(UpgradeType inUpgradeType)
    {
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                _playerEnergyUpgrades++;
                break;
            case UpgradeType.HEALTH:
                _playerHealthUpgrades++;
                break;
            case UpgradeType.DMG:
                _playerDMGUpgrades++;
                break;
            case UpgradeType.SEGMENT_SKIPS:
                break;
            case UpgradeType.ORBITAL:
                _playerOrbitalUpgrades++;
                break;
            default:
                break;
        }
    }

    public bool AttemptToBuy(UpgradeType inUpgradeType)
    {
        if (AllUpgradeConditionsMet(inUpgradeType))
        {
            int cost = GetCostOf(inUpgradeType);
            _onPurchaseMade(cost);
            AddOneNumberOfOwned(inUpgradeType);

            return true;
        }
        else
        {
            return false;
        }
    }
    
    public int GetUnlockLevel(UpgradeType inUpgradeType)
    {
        int numOfOwnedUpgrades = GetOwnedNumberOf(inUpgradeType);
        int unlocklevel = 0;
        switch (inUpgradeType)
        {
            case UpgradeType.ENERGY:
                switch (numOfOwnedUpgrades)
                {
                    case 0: unlocklevel = 0; break;
                    case 1: unlocklevel = 5; break;
                    case 2: unlocklevel = 12; break;
                    case 3: unlocklevel = 19; break;
                    case 4: unlocklevel = 22; break;

                    default:
                        unlocklevel = 0;
                        break;
                }

                break;
            case UpgradeType.HEALTH:
                switch (numOfOwnedUpgrades)
                {
                    case 0: unlocklevel = 0; break;
                    case 1: unlocklevel = 5; break;
                    case 2: unlocklevel = 12; break;
                    case 3: unlocklevel = 19; break;
                    case 4: unlocklevel = 22; break;

                    default:
                        unlocklevel = 0;
                        break;
                }
                break;
            case UpgradeType.DMG:
                switch (numOfOwnedUpgrades)
                {
                    case 0: unlocklevel = 12; break;
                    case 1: unlocklevel = 19; break;
                    case 2: unlocklevel = 22; break;
                    default:
                        unlocklevel = 0;
                        break;
                }
                break;
            case UpgradeType.SEGMENT_SKIPS:
                break;
            case UpgradeType.ORBITAL:
                switch (numOfOwnedUpgrades)
                {
                    case 0: unlocklevel = 12; break;
                    case 1: unlocklevel = 19; break;
                    case 2: unlocklevel = 22; break;

                    default:
                        unlocklevel = 0;
                        break;
                }
                break;
            default:
                break;
        }

        return unlocklevel;
    }

    public string GetPurchaseText(UpgradeType inUpgradeType)
    {
        string dialogText;
        int numOfOwnedUpgrades = GetOwnedNumberOf(inUpgradeType);
        if (numOfOwnedUpgrades == GetMaxNumberOf(inUpgradeType))
        {
            dialogText = "Fully upgraded";
        }
        else
        {
            switch (inUpgradeType)
            {
                case UpgradeType.ENERGY:
                    if (numOfOwnedUpgrades == 0 || numOfOwnedUpgrades == 2)
                        dialogText = "Max Energy +1";
                    else
                        if (numOfOwnedUpgrades == 4)
                        dialogText = " Energy Regen per second +0.25";
                    else
                        dialogText = "Energy on hit +0.25 ";
                    break;
                case UpgradeType.HEALTH:
                    if (numOfOwnedUpgrades == 1 || numOfOwnedUpgrades == 2 || numOfOwnedUpgrades == 4)  //upgrades 2 4
                        dialogText = "Max Health +1";
                    else
                        dialogText = "Health regain after stage +1";    //upgrades 1 and 4
                    break;
                case UpgradeType.DMG:
                    dialogText = "Damage +1";
                    break;
                case UpgradeType.SEGMENT_SKIPS:
                    dialogText = "Skip this segment?";
                    break;
                case UpgradeType.ORBITAL:
                    dialogText = "Buy a protecting orbit guardian";
                    break;
                default:
                    dialogText = "unknown purchase";
                    break;
            }
        }

        return dialogText;
    }

    public bool DoWeHaveAnyUpgradesToBuy
    {
        get
        {
            return
                AllUpgradeConditionsMet(UpgradeType.HEALTH) ||
                AllUpgradeConditionsMet(UpgradeType.ENERGY) ||
                AllUpgradeConditionsMet(UpgradeType.DMG)  ||
                AllUpgradeConditionsMet(UpgradeType.ORBITAL);
        }
    }

    bool AllUpgradeConditionsMet(UpgradeType inUpgradeType)
    {
        int cost = GetCostOf(inUpgradeType);
        int maxNumber = GetMaxNumberOf(inUpgradeType);
        int currentNumberOfUpgrade = GetOwnedNumberOf(inUpgradeType);

        bool hasLessThanMaxUpgrades = currentNumberOfUpgrade < maxNumber;
        bool hasSufficientMoney = ObtainCurrentShards() >= cost;
        bool hasReachedHighEnoughLevel = ObtainHighestLevel() >= GetUnlockLevel(inUpgradeType);

        return hasLessThanMaxUpgrades && hasSufficientMoney && hasReachedHighEnoughLevel;
    }

    //Hacky function in charge of upgrading player
    public int GetUpgradedPlayerHealth(int inBaseMaxHealth)
    {
        if (_playerHealthUpgrades >= 2)
            inBaseMaxHealth++;

        if (_playerHealthUpgrades >= 3)
            inBaseMaxHealth++;

        if (_playerHealthUpgrades >= 5)
            inBaseMaxHealth++;

        return inBaseMaxHealth;
    }
    
    //Hacky function in charge of upgrading player
    public int GetUpgradedPlayerHealthRegainAfterEachLevel(int inBaseHealthRegainAfterEachLevel)
    {
        if (_playerHealthUpgrades >= 1)
            inBaseHealthRegainAfterEachLevel++;
        if (_playerHealthUpgrades >= 4)
            inBaseHealthRegainAfterEachLevel++;

        return inBaseHealthRegainAfterEachLevel;
    }

    public float GetUpgradedPlayerMaxEnergy(float inBaseMaxEnergy)
    {
        if (_playerEnergyUpgrades >= 1)
            inBaseMaxEnergy++;

        if (_playerEnergyUpgrades >= 3)
            inBaseMaxEnergy++;

        return inBaseMaxEnergy;
    }

    public float GetUpgradedPlayerEnergyOnHit(float inBaseEnergyOnHit)
    {
        if (_playerEnergyUpgrades >= 2)
            inBaseEnergyOnHit++;
        if (_playerEnergyUpgrades >= 4)
            inBaseEnergyOnHit++;

        return inBaseEnergyOnHit;
    }

    public float GetUpgradedPlayerEnergyRegen(float inBaseMaxEnergyRegen)
    {
        if (_playerEnergyUpgrades >= 5)
            inBaseMaxEnergyRegen += 0.25f;
        
        return inBaseMaxEnergyRegen;
    }

    public int GetUpgradePlayerDMG(int inBaseDMG)
    {
        if (_playerDMGUpgrades >= 1)
            inBaseDMG++;
        if (_playerDMGUpgrades >= 2)
            inBaseDMG++;
        if (_playerDMGUpgrades >= 3)
            inBaseDMG++;

        return inBaseDMG;
    }

    public int GetUpgradedPlayerOrbitals(int inBaseOrbitalCount)
    {
        //one for one upgrade
        return inBaseOrbitalCount + _playerOrbitalUpgrades;
    }
}
