using System;
using System.Collections.Generic;
using _ProjectEvE.Scripts.Data;
using UnityEngine;

[Serializable]
public class ConstellationInfo {
    public long constellation_id;
    public string name;
    public Position position;
    public long region_id;
    public long[] systems;
}