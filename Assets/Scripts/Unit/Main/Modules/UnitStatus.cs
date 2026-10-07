using UnityEngine;
using System.Collections.Generic;

public class UnitStatus : UnitModule
{
    public struct RuntimeBuffProperties
    {
        public int id;
        public UnitBuff unitBuff;
        public float buffRuntime;
        public int buffStacks;

        public RuntimeBuffProperties(int id, UnitBuff unitBuff, float buffRuntime, int buffStacks)
        {
            this.id = id;
            this.unitBuff = unitBuff;
            this.buffRuntime = buffRuntime;
            this.buffStacks = buffStacks;
        }
    }
    private int GenerateNextId()
    {
        unchecked
        {
            nextBuffId++;
            // Не используем 0, так как 0 обычно обозначает "Null / Invalid ID"
            if (nextBuffId == 0)
                nextBuffId = 1;

            return nextBuffId;
        }
    }
    private int nextBuffId = 1;
    private readonly List<RuntimeBuffProperties> currentBuffs = new List<RuntimeBuffProperties>();
    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);
    }
    public override void OnStart()
    {

    }
    public override void OnTick(float deltaInterval)
    {
        OnTickBuffs(deltaInterval);
    }
    void OnTickBuffs(float deltaInterval)
    {
        if (currentBuffs == null || currentBuffs.Count == 0)
            return;


        for (int i = currentBuffs.Count - 1; i >= 0; i--)
        {
            RuntimeBuffProperties rbp = currentBuffs[i];
            rbp.unitBuff.OnTick(unit, deltaInterval);
            rbp.buffRuntime += deltaInterval;
            currentBuffs[i] = rbp;
            if (rbp.unitBuff.buffType == BuffType.Temporary && rbp.buffRuntime > rbp.unitBuff.buffLifetime)
            {
                RemoveBuff(i);
            }
        }
    }
    public int AddBuff(UnitBuff buffSO)
    {
        if (buffSO.stackingBuff)
        {
            for (int i = 0; i < currentBuffs.Count; i++)
            {
                RuntimeBuffProperties rbp = currentBuffs[i];
                if (rbp.unitBuff == buffSO)
                {
                    rbp.buffStacks = Mathf.Min(rbp.buffStacks + 1, rbp.unitBuff.maxStacks);
                    rbp.buffRuntime = 0f;
                    currentBuffs[i] = rbp;
                    return rbp.id; // Возвращаем существующий ID
                }
            }
        }

        int newId = GenerateNextId();
        RuntimeBuffProperties newBuff = new RuntimeBuffProperties(newId, buffSO, 0f, 1);

        currentBuffs.Add(newBuff);
        buffSO.OnAwake(unit);

        return newId;
    }
    public void RemoveBuff(UnitBuff buff)
    {
        for (int i = currentBuffs.Count - 1; i >= 0; i--)
        {
            if (currentBuffs[i].unitBuff == buff)
            {
                RemoveBuff(i);
                break;
            }
        }
    }
    public bool RemoveBuffById(int id)
    {
        for (int i = currentBuffs.Count - 1; i >= 0; i--)
        {
            if (currentBuffs[i].id == id)
            {
                RemoveBuff(i);
                return true;
            }
        }
        return false;
    }
    public void RemoveBuff(int index)
    {
        currentBuffs[index].unitBuff.OnRemove(unit);
        DestroyBuff(index);
    }
    public void DestroyBuff(RuntimeBuffProperties buff)
    {
        currentBuffs.Remove(buff);
    }
    public void DestroyBuff(int index)
    {
        currentBuffs.RemoveAt(index);
    }
}