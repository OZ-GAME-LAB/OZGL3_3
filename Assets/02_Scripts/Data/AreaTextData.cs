using System;

[Serializable]
public class AreaTextData : BaseData
{
    public string SpritePath;
    public string MapName;
    public string RespawnMap;      // 부활할 맵의 구간 ID (AreaText의 Id)
    public float x;                // 부활 좌표
    public float y;
    public float z;
}